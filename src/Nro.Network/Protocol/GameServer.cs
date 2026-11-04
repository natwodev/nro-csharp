using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nro.Network.Configurations;
using Nro.Network.Ports;
using Nro.Network.Sessions;

namespace Nro.Network.Protocol;

// Lắng nghe TCP của game server, nhận kết nối, cấp slot IP và vòng đời phiên.
// Nguồn Java: network/Network.java, nro/server/ServerManager.java (khác Java: từ chối IP quá hạn trước khi tạo phiên; các phiên chạy vòng lặp bất đồng bộ riêng).
public sealed class GameServer : IAsyncDisposable
{
    private sealed class ForwardingAcceptHandler(ISessionAcceptHandler? inner) : ISessionAcceptHandler
    {
        // Chuyển tiếp thông báo khởi tạo phiên đến handler gốc.
        public void SessionInit(ISession session) => inner?.SessionInit(session);

        // Chuyển tiếp thông báo ngắt phiên đến handler gốc.
        public void SessionDisconnect(ISession session) => inner?.SessionDisconnect(session);
    }

    private readonly GameServerOptions _options;
    private readonly IMessageHandler _handler;
    private readonly ISessionAcceptHandler _acceptHandler;
    private readonly ILogger _logger;
    private readonly SessionManager _sessions = new();
    private readonly IpConnectionLimiter _limiter;
    private readonly ConcurrentDictionary<long, Task> _running = new();
    private readonly CancellationTokenSource _cancel = new();

    private TcpListener? _listener;
    private Task? _acceptLoop;
    private int _started;
    private int _stopped;

    // Tạo TCP server với cấu hình, handler gói, handler vòng đời và logger tùy chọn.
    public GameServer(GameServerOptions options, IMessageHandler handler, ISessionAcceptHandler? acceptHandler = null, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(handler);
        _options = options;
        _handler = handler;
        _acceptHandler = new ForwardingAcceptHandler(acceptHandler);
        _logger = logger ?? NullLogger.Instance;
        _limiter = new IpConnectionLimiter(options.MaxConnectionsPerIp);
    }

    // Các phiên hiện đang kết nối.
    public ISessionManager Sessions => _sessions;

    // Bộ đếm kết nối theo IP.
    public IIpConnectionLimiter IpLimiter => _limiter;

    // Cổng listener đang gắn, hữu ích khi cấu hình cổng 0; chỉ dùng sau Start.
    public int Port => _listener is null
        ? throw new InvalidOperationException("The server has not been started.")
        : ((IPEndPoint)_listener.LocalEndpoint).Port;

    // Gắn cổng và bắt đầu nhận kết nối; ném lỗi khi cổng đã dùng thay vì Java thoát tiến trình.
    public void Start()
    {
        if (Interlocked.Exchange(ref _started, 1) != 0)
        {
            throw new InvalidOperationException("The server was already started.");
        }

        TcpListener listener;
        if (_options.BindAddress is not null)
        {
            listener = new TcpListener(_options.BindAddress, _options.Port);
        }
        else if (Socket.OSSupportsIPv6)
        {
            listener = new TcpListener(IPAddress.IPv6Any, _options.Port);
            listener.Server.DualMode = true;
        }
        else
        {
            listener = new TcpListener(IPAddress.Any, _options.Port);
        }

        listener.Start();
        _listener = listener;
        _logger.LogInformation("Game server listening on port {Port}", Port);
        _acceptLoop = Task.Run(AcceptLoopAsync);
    }

    // Ngừng lắng nghe, ngắt mọi phiên và chờ kết thúc; gọi nhiều lần vẫn an toàn.
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _stopped, 1) == 0)
        {
            _cancel.Cancel();
            _listener?.Stop();
            foreach (ISession session in _sessions.Snapshot())
            {
                session.Disconnect();
            }
        }

        if (_acceptLoop is not null)
        {
            await _acceptLoop.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        // Kết nối được nhận khi ảnh chụp đầu tiên đang tạo sẽ tự từ chối trong Admit; lượt này là lưới an toàn.
        foreach (ISession session in _sessions.Snapshot())
        {
            session.Disconnect();
        }

        await Task.WhenAll(_running.Values).WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    // Dừng server và giải phóng token hủy.
    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
        _cancel.Dispose();
    }

    private async Task AcceptLoopAsync()
    {
        TcpListener listener = _listener!;
        CancellationToken ct = _cancel.Token;
        int failures = 0;
        while (!ct.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await listener.AcceptTcpClientAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Accept loop stopped");
                break;
            }
            catch (ObjectDisposedException)
            {
                _logger.LogDebug("Accept loop stopped: the listener was closed");
                break;
            }
            catch (Exception ex)
            {
                // Hết handle hoặc buffer khi bị dồn kết nối sẽ lỗi lại ngay; chờ tăng dần thay vì quay vòng nóng.
                failures++;
                _logger.LogWarning(ex, "Accepting a connection failed ({Count} in a row); still listening", failures);
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(Math.Min(1000, 50 * failures)), ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                continue;
            }

            failures = 0;

            try
            {
                Admit(client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not set up an accepted connection; it was closed");
                client.Dispose();
            }
        }
    }

    private void Admit(TcpClient client)
    {
        IPAddress address = ((IPEndPoint)client.Client.RemoteEndPoint!).Address;
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        string ip = address.ToString();
        if (!_limiter.TryAcquire(ip))
        {
            _logger.LogWarning("Connection refused: IP {Ip} already holds {Max} connections", ip, _options.MaxConnectionsPerIp);
            client.Dispose();
            return;
        }

        try
        {
            client.SendBufferSize = _options.SocketBufferBytes;
            client.ReceiveBufferSize = _options.SocketBufferBytes;
            client.NoDelay = _options.NoDelay;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            var session = new Session(_sessions.NextId(), ip, client.GetStream(), _handler, _acceptHandler, _options.Session, _logger);
            var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _running[session.Id] = done.Task;
            _sessions.Add(session);
            _ = RunSessionAsync(session, client, ip, done);
            if (Volatile.Read(ref _stopped) != 0)
            {
                // StopAsync có thể đã chụp trước khi thêm phiên này; phiên tự kết thúc để không còn kết nối.
                session.Disconnect();
            }
        }
        catch
        {
            _limiter.Release(ip);
            throw;
        }
    }

    private async Task RunSessionAsync(Session session, TcpClient client, string ip, TaskCompletionSource done)
    {
        try
        {
            await session.RunAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Session {Id} ended with an unexpected error", session.Id);
        }
        finally
        {
            _sessions.Remove(session);
            _limiter.Release(ip);
            client.Dispose();
            _running.TryRemove(session.Id, out _);
            done.TrySetResult();
        }
    }
}
