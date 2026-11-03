using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nro.Network.Configurations;
using Nro.Network.Interfaces;
using Nro.Network.Sessions;

namespace Nro.Network.Services;

/// <summary>
/// The TCP listener of the game server. Port of <c>network/Network.java</c> (accept loop) plus the accept-time rules of
/// <c>ServerManager.activeServerSocket</c>: for every accepted connection it takes a per-IP slot (<see cref="IpConnectionLimiter"/>), gives the connection
/// an id, registers a <see cref="Session"/> in the <see cref="SessionManager"/>, runs it, and cleans up when it ends.
/// <list type="bullet">
/// <item><description>A connection from an IP that already holds <see cref="GameServerOptions.MaxConnectionsPerIp"/> connections is closed at once; no session is
/// created and no handler hears about it (in Java the rejected session was created, registered, then disconnected).</description></item>
/// <item><description>Every session runs on its own asynchronous loops, so a slow client never delays the accept loop.</description></item>
/// <item><description><see cref="StopAsync"/> stops listening, disconnects every session and waits for them to finish.</description></item>
/// </list>
/// </summary>
public sealed class GameServer : IAsyncDisposable
{
    private sealed class ForwardingAcceptHandler(ISessionAcceptHandler? inner) : ISessionAcceptHandler
    {
        public void SessionInit(ISession session) => inner?.SessionInit(session);

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

    /// <summary>The sessions that are connected right now.</summary>
    public ISessionManager Sessions => _sessions;

    /// <summary>The per-IP connection counter.</summary>
    public IIpConnectionLimiter IpLimiter => _limiter;

    /// <summary>The port the listener is bound to (useful when <see cref="GameServerOptions.Port"/> is 0). Available after <see cref="Start"/>.</summary>
    public int Port => _listener is null
        ? throw new InvalidOperationException("The server has not been started.")
        : ((IPEndPoint)_listener.LocalEndpoint).Port;

    /// <summary>Binds the port and starts accepting connections. Throws when the port is taken (Java exited the process).</summary>
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

    /// <summary>Stops listening, disconnects every session and waits until they have all ended. Safe to call more than once.</summary>
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

        // A connection admitted while the first snapshot was taken refuses itself (see Admit), this second pass is the safety net.
        foreach (ISession session in _sessions.Snapshot())
        {
            session.Disconnect();
        }

        await Task.WhenAll(_running.Values).WaitAsync(cancellationToken).ConfigureAwait(false);
    }

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
                // Out of handles or buffers (a connection flood) fails again at once: wait a little, with a growing delay, instead of spinning.
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
                // StopAsync may have taken its snapshot before this session was added: it would stay connected, so it ends itself.
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
