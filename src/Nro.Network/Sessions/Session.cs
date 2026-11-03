using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nro.Core.IO;
using Nro.Network.Framing;
using Nro.Network.Handshake;
using Nro.Network.Interfaces;

namespace Nro.Network.Sessions;

/// <summary>
/// One client connection on the server side. Port of <c>network/session/Session.java</c> together with <c>Sender</c>, <c>Collector</c> and
/// <c>QueueHandler</c>: where Java used three threads per connection polling every 33 ms, this class runs three asynchronous loops over
/// <see cref="Channel{T}"/> queues.
/// <list type="bullet">
/// <item><description><b>Read loop</b> (Collector): reads packets with <see cref="FrameReader"/>. The hello (<c>-27</c>) is answered right here with the key
/// packet, and the cipher is switched on before the next packet is read (deterministic, like Java); every other packet goes to the inbound queue.</description></item>
/// <item><description><b>Dispatch loop</b> (QueueHandler): hands inbound packets to the <see cref="IMessageHandler"/> one at a time, in arrival order.</description></item>
/// <item><description><b>Write loop</b> (Sender): writes the queued packets. Like the Java Sender, which <c>sendKey</c> starts, it writes nothing before the key
/// packet has gone out; packets queued earlier then leave encrypted, in order.</description></item>
/// </list>
/// Rules kept from Java: inbound packets beyond <see cref="SessionOptions.InboundQueueLimit"/> (500) are dropped; order is preserved in both directions; the
/// accept handler hears about the end of every session exactly once. Deliberate differences: a handler exception no longer kills delivery for the rest
/// of the session; the outbound queue is bounded and a session that falls that far behind is disconnected; a packet too large for its size field is skipped with
/// a warning instead of corrupting the stream; a connection that never sends the hello is closed after <see cref="SessionOptions.HandshakeTimeout"/> and, when
/// configured, one that sends nothing for <see cref="SessionOptions.IdleTimeout"/> (Java has neither). A second hello disconnects the session, as in Java
/// (its second <c>startSend()</c> threw and ended the Collector thread).
/// </summary>
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

    public Session(
        long id,
        string ipAddress,
        Stream stream,
        IMessageHandler handler,
        ISessionAcceptHandler? acceptHandler = null,
        SessionOptions? options = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);
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

    public long Id { get; }

    public string IpAddress { get; }

    public bool IsConnected => Volatile.Read(ref _closed) == 0;

    /// <summary>How many inbound packets were dropped because the handler was more than <see cref="SessionOptions.InboundQueueLimit"/> packets behind.</summary>
    public long DroppedInboundCount => Interlocked.Read(ref _droppedInbound);

    /// <summary>
    /// Starts the session and returns a task that completes when it has fully ended (all loops stopped, the stream closed and
    /// <see cref="ISessionAcceptHandler.SessionDisconnect"/> called). Calling it again returns the same task.
    /// </summary>
    public Task RunAsync() => _run.Value;

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
            // Java failed inside the Sender and swallowed it; one bad packet must not abort a broadcast loop in the caller.
            _logger.LogWarning(ex, "Session {Id}: command {Command} cannot be sent (the message is not in write mode or was disposed)", Id, message.Command);
            return false;
        }

        return Enqueue(new Outbound(message.Command, data));
    }

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
        // SessionInit would otherwise run on the caller's thread (the accept loop): a slow handler must not hold up accepting.
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
                // Deadline for the hello first, then the idle deadline (none by default) for every packet.
                // The handshake deadline is ONE absolute time from the start of the session: junk frames before the hello do not extend it.
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

                    // Written here, not by the write loop, so the cipher is on before the next frame is read.
                    byte[] wire = KeyPacket.CreateServerFrame(_cipher);
                    await _stream.WriteAsync(wire, ct).ConfigureAwait(false);
                    await _stream.FlushAsync(ct).ConfigureAwait(false);
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
