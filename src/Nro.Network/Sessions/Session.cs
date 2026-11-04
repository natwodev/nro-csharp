using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nro.Core.IO;
using Nro.Network.Framing;
using Nro.Network.Protocol;

namespace Nro.Network.Sessions;

// Đại diện một kết nối client phía server với ba vòng đọc, phân phối và ghi bất đồng bộ qua Channel.
// Nguồn Java: network/session/Session.java, network/Sender.java, network/Collector.java, network/QueueHandler.java (thay ba thread polling 33 ms bằng ba vòng async).
// Vòng đọc trả lời hello -27 bằng gói khóa XOR ngay tại chỗ rồi bật mã hóa trước khung kế tiếp; vòng phân phối giữ thứ tự đến; vòng ghi chờ gói khóa và giữ thứ tự gói đã xếp hàng.
// Giữ Java: bỏ gói nhận khi quá 500, giữ thứ tự hai chiều và chỉ báo kết thúc phiên một lần. Khác Java: lỗi handler không dừng phân phối, hàng gửi có giới hạn và chậm đọc bị ngắt,
// gói quá kích thước bị bỏ kèm cảnh báo, có timeout bắt tay/nhàn rỗi; hello lần hai ngắt phiên như Java vì startSend lần hai làm Collector kết thúc.
public sealed class Session : ISession
{
    private readonly record struct Outbound(sbyte Command, byte[] Payload);

    private readonly Stream _stream;
    private readonly IMessageHandler _handler;
    private readonly ISessionAcceptHandler? _acceptHandler;
    private readonly ILogger _logger;
    private readonly WireCipher _cipher = WireCipher.CreateDefault();
    private readonly FrameReader _reader;
    private readonly Channel<Outbound> _outbound;
    private readonly Channel<Frame> _inbound;
    private readonly CancellationTokenSource _cancel = new();
    private readonly SessionOptions _options;
    private readonly TaskCompletionSource _keySent = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Lazy<Task> _run;

    private int _closed;
    private int _helloSeen;
    private long _droppedInbound;

    // Tạo phiên với mã, IP, stream, handler và các xử lý vòng đời/cấu hình/logger tùy chọn.
    public Session(
        long id,
        string ipAddress,
        Stream stream,
        IMessageHandler handler,
        ISessionAcceptHandler? acceptHandler = null,
        SessionOptions? options = null,
        ILogger? logger = null)
        : this(id, ipAddress, ipAddress, stream, handler, acceptHandler, options, logger)
    {
    }

    // Tạo phiên với mã, IP, địa chỉ IP:cổng xa, stream, handler và các xử lý vòng đời/cấu hình/logger tùy chọn.
    public Session(
        long id,
        string ipAddress,
        string remoteEndPoint,
        Stream stream,
        IMessageHandler handler,
        ISessionAcceptHandler? acceptHandler = null,
        SessionOptions? options = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);
        ArgumentNullException.ThrowIfNull(remoteEndPoint);
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(handler);
        options ??= new SessionOptions();
        if (options.InboundQueueLimit < 1 || options.OutboundQueueLimit < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Queue limits must be at least 1.");
        }

        if (options.HandshakeTimeout <= TimeSpan.Zero || options.IdleTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Timeouts must be positive (use null for no idle timeout).");
        }

        _options = options;
        _run = new Lazy<Task>(RunCoreAsync, LazyThreadSafetyMode.ExecutionAndPublication);
        Id = id;
        IpAddress = ipAddress;
        RemoteEndPoint = remoteEndPoint;
        _stream = stream;
        _handler = handler;
        _acceptHandler = acceptHandler;
        _logger = logger ?? NullLogger.Instance;
        _reader = FrameReader.Create(_cipher, options.ClientThreeByteCommands);
        _outbound = Channel.CreateBounded<Outbound>(new BoundedChannelOptions(options.OutboundQueueLimit)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });
        _inbound = Channel.CreateBounded<Frame>(new BoundedChannelOptions(options.InboundQueueLimit)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = true,
        });
    }

    // Mã định danh phiên.
    public long Id { get; }

    // Địa chỉ IP của client.
    public string IpAddress { get; }

    // Địa chỉ IP:cổng xa của client.
    public string RemoteEndPoint { get; }

    // Cho biết phiên chưa bị đóng.
    public bool IsConnected => Volatile.Read(ref _closed) == 0;

    // Số gói nhận bị bỏ vì handler chậm quá InboundQueueLimit gói.
    public long DroppedInboundCount => Interlocked.Read(ref _droppedInbound);

    // Khởi chạy phiên và trả task hoàn tất khi mọi vòng dừng, stream đóng và đã gọi SessionDisconnect; gọi lại trả cùng task.
    public Task RunAsync() => _run.Value;

    // Xếp Message để gửi; trả false khi đã đóng hoặc gói không hợp lệ/không còn ở chế độ ghi.
    public bool Send(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (!IsConnected)
        {
            return false;
        }

        byte[] data;
        try
        {
            data = message.GetData();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            // Java lỗi trong Sender rồi nuốt lỗi; một gói lỗi không được làm dừng vòng broadcast của bên gọi.
            _logger.LogWarning(ex, "Session {Id}: command {Command} cannot be sent (the message is not in write mode or was disposed)", Id, message.Command);
            return false;
        }

        return Enqueue(new Outbound(message.Command, data));
    }

    // Đóng phiên một lần, hủy các vòng, hoàn tất hàng đợi và giải phóng stream.
    public void Disconnect()
    {
        if (Interlocked.Exchange(ref _closed, 1) != 0)
        {
            return;
        }

        _cancel.Cancel();
        _outbound.Writer.TryComplete();
        _inbound.Writer.TryComplete();
        try
        {
            _stream.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Session {Id}: error while closing the stream", Id);
        }
    }

    private bool Enqueue(Outbound item)
    {
        if (_outbound.Writer.TryWrite(item))
        {
            return true;
        }

        if (IsConnected)
        {
            _logger.LogWarning("Session {Id} ({Ip}): outbound queue is full, disconnecting a client that is not reading", Id, IpAddress);
            Disconnect();
        }

        return false;
    }

    private async Task RunCoreAsync()
    {
        // Nếu không SessionInit chạy trên thread bên gọi (vòng accept); handler chậm không được chặn việc nhận kết nối.
        await Task.Yield();
        try
        {
            _acceptHandler?.SessionInit(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session {Id}: SessionInit threw", Id);
        }

        Task read = Task.Run(ReadLoopAsync);
        Task write = Task.Run(WriteLoopAsync);
        Task dispatch = Task.Run(DispatchLoopAsync);

        await Task.WhenAny(read, write, dispatch).ConfigureAwait(false);
        Disconnect();
        await Task.WhenAll(read, write, dispatch).ConfigureAwait(false);

        try
        {
            _acceptHandler?.SessionDisconnect(this);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session {Id}: SessionDisconnect threw", Id);
        }
    }

    private async Task ReadLoopAsync()
    {
        CancellationToken ct = _cancel.Token;
        long startedAt = Stopwatch.GetTimestamp();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                // Trước hello dùng hạn bắt tay, sau đó dùng hạn nhàn rỗi mặc định không giới hạn cho từng gói.
                // Hạn bắt tay là một mốc tuyệt đối từ khi bắt đầu phiên: khung rác trước hello không gia hạn.
                TimeSpan? deadline = Volatile.Read(ref _helloSeen) == 0
                    ? _options.HandshakeTimeout - Stopwatch.GetElapsedTime(startedAt)
                    : _options.IdleTimeout;
                if (deadline is { } left && left <= TimeSpan.Zero)
                {
                    left = TimeSpan.Zero;
                    deadline = left;
                }

                Frame? frame;
                using (CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct))
                {
                    if (deadline is not null)
                    {
                        timeout.CancelAfter(deadline.Value);
                    }

                    try
                    {
                        frame = await _reader.ReadAsync(_stream, timeout.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                    {
                        _logger.LogInformation("Session {Id} ({Ip}): nothing received within {Seconds} s; disconnecting", Id, IpAddress, (int)deadline!.Value.TotalSeconds);
                        break;
                    }
                }

                if (frame is null)
                {
                    break;
                }

                if (frame.Value.Command == WireCommands.GetSessionId)
                {
                    if (Interlocked.Exchange(ref _helloSeen, 1) != 0)
                    {
                        _logger.LogInformation("Session {Id} ({Ip}): second hello; disconnecting like the Java server", Id, IpAddress);
                        break;
                    }

                    // Ghi ngay tại đây thay vì vòng ghi để mã XOR hoạt động trước khi đọc khung kế tiếp.
                    byte[] wire = KeyPacket.CreateServerFrame(_cipher);
                    await _stream.WriteAsync(wire, ct).ConfigureAwait(false);
                    await _stream.FlushAsync(ct).ConfigureAwait(false);
                    _logger.LogInformation("Phiên {Id} ({Ip}): nhận cmd -27 (GET_SESSION_ID) bắt tay, {Length} byte; đã gửi khóa", Id, IpAddress, frame.Value.Payload.Length);
                    _keySent.TrySetResult();
                    continue;
                }

                if (!_inbound.Writer.TryWrite(frame.Value))
                {
                    Interlocked.Increment(ref _droppedInbound);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Session {Id}: loop stopped because the session was closed", Id);
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or EndOfStreamException)
        {
            _logger.LogDebug(ex, "Session {Id}: connection ended while reading", Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session {Id}: unexpected error while reading", Id);
        }
        finally
        {
            Disconnect();
        }
    }

    private async Task WriteLoopAsync()
    {
        CancellationToken ct = _cancel.Token;
        try
        {
            await _keySent.Task.WaitAsync(ct).ConfigureAwait(false);
            await foreach (Outbound item in _outbound.Reader.ReadAllAsync(ct).ConfigureAwait(false))
            {
                byte[] wire;
                try
                {
                    wire = FrameWriter.Encode(_cipher, item.Command, item.Payload);
                }
                catch (FrameTooLargeException ex)
                {
                    _logger.LogWarning(ex, "Session {Id}: packet skipped", Id);
                    continue;
                }

                await _stream.WriteAsync(wire, ct).ConfigureAwait(false);
                await _stream.FlushAsync(ct).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Session {Id}: loop stopped because the session was closed", Id);
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException)
        {
            _logger.LogDebug(ex, "Session {Id}: connection ended while writing", Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session {Id}: unexpected error while writing", Id);
        }
        finally
        {
            Disconnect();
        }
    }

    private async Task DispatchLoopAsync()
    {
        CancellationToken ct = _cancel.Token;
        try
        {
            await foreach (Frame frame in _inbound.Reader.ReadAllAsync(ct).ConfigureAwait(false))
            {
                Message message = frame.ToMessage();
                try
                {
                    await _handler.OnMessageAsync(this, message, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Session {Id}: the handler threw for command {Command}", Id, message.Command);
                }
                finally
                {
                    message.Cleanup();
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Session {Id}: loop stopped because the session was closed", Id);
        }
        finally
        {
            Disconnect();
        }
    }
}
