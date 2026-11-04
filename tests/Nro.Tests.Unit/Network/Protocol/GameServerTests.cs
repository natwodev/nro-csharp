using System.Net;
using System.Net.Sockets;
using Nro.Core.IO;
using Nro.Network.Configurations;
using Nro.Network.Framing;
using Nro.Network.Protocol;
using Nro.Network.Sessions;

namespace Nro.Tests.Unit.Network.Protocol;

/// <summary>The TCP listener end to end (accept, per-IP limit, registry, stop), plus the small services it is built from.</summary>
public class GameServerTests
{
    private static GameServer NewServer(RecordingHandler handler, RecordingAcceptHandler? accept = null, int maxPerIp = 3, SessionOptions? session = null)
    {
        var server = new GameServer(
            new GameServerOptions { Port = 0, BindAddress = IPAddress.Loopback, MaxConnectionsPerIp = maxPerIp, Session = session ?? new SessionOptions() },
            handler,
            accept);
        server.Start();
        return server;
    }

    private static async Task<TestClient> ConnectAsync(GameServer server)
    {
        var tcp = new TcpClient();
        await tcp.ConnectAsync(IPAddress.Loopback, server.Port);
        return new TestClient(tcp);
    }

    private static async Task WaitUntilAsync(Func<bool> condition, string what)
    {
        DateTime end = DateTime.UtcNow + TestTimeouts.Default;
        while (!condition())
        {
            Assert.True(DateTime.UtcNow < end, $"Timed out waiting for: {what}");
            await Task.Delay(5);
        }
    }

    [Fact]
    public async Task An_accepted_connection_becomes_a_registered_session_that_talks_the_protocol()
    {
        var handler = new RecordingHandler();
        var accept = new RecordingAcceptHandler();
        await using GameServer server = NewServer(handler, accept);

        using TestClient client = await ConnectAsync(server);
        await client.HandshakeAsync();
        await client.SendAsync(7, [1, 2, 3]);
        await handler.WaitForCountAsync(1);

        Assert.Equal(1, server.Sessions.Count);
        ISession session = server.Sessions.Snapshot().Single();
        Assert.Equal("127.0.0.1", session.IpAddress);
        Assert.Equal(1, accept.InitCount);

        Assert.True(session.Send(new Message(9)));
        Frame reply = await client.ReadAsync();
        Assert.Equal(9, reply.Command);
        Assert.True(server.Sessions.TryGet(session.Id, out ISession? found));
        Assert.Same(session, found);
    }

    [Fact]
    public async Task When_a_client_leaves_the_session_is_unregistered_and_its_ip_slot_is_given_back()
    {
        var accept = new RecordingAcceptHandler();
        await using GameServer server = NewServer(new RecordingHandler(), accept);

        TestClient client = await ConnectAsync(server);
        await client.HandshakeAsync();
        await WaitUntilAsync(() => server.Sessions.Count == 1, "session registered");
        Assert.Equal(1, server.IpLimiter.CountFor("127.0.0.1"));

        client.Dispose();
        await WaitUntilAsync(() => server.Sessions.Count == 0 && server.IpLimiter.CountFor("127.0.0.1") == 0, "session removed and slot released");
        Assert.Equal(1, accept.DisconnectCount);
    }

    [Fact]
    public async Task A_connection_over_the_per_ip_limit_is_closed_at_once_and_never_reaches_the_handlers()
    {
        var accept = new RecordingAcceptHandler();
        await using GameServer server = NewServer(new RecordingHandler(), accept, maxPerIp: 2);

        using TestClient first = await ConnectAsync(server);
        using TestClient second = await ConnectAsync(server);
        await first.HandshakeAsync();
        await second.HandshakeAsync();

        using TestClient third = await ConnectAsync(server);
        Assert.True(await third.WaitForEndOfStreamAsync(), "the third connection from the same IP must be closed");

        Assert.Equal(2, server.Sessions.Count);
        Assert.Equal(2, accept.InitCount);
        Assert.Equal(2, server.IpLimiter.CountFor("127.0.0.1"));
    }

    [Fact]
    public async Task A_slot_freed_by_a_closing_connection_lets_a_new_one_in()
    {
        await using GameServer server = NewServer(new RecordingHandler(), maxPerIp: 1);

        TestClient first = await ConnectAsync(server);
        await first.HandshakeAsync();

        using TestClient refused = await ConnectAsync(server);
        Assert.True(await refused.WaitForEndOfStreamAsync());

        first.Dispose();
        await WaitUntilAsync(() => server.IpLimiter.CountFor("127.0.0.1") == 0, "slot released");

        using TestClient again = await ConnectAsync(server);
        await again.HandshakeAsync();
        Assert.Equal(1, server.IpLimiter.CountFor("127.0.0.1"));
    }

    [Fact]
    public async Task Session_ids_are_unique_and_start_at_zero()
    {
        await using GameServer server = NewServer(new RecordingHandler());

        using TestClient a = await ConnectAsync(server);
        using TestClient b = await ConnectAsync(server);
        using TestClient c = await ConnectAsync(server);
        await a.HandshakeAsync();
        await b.HandshakeAsync();
        await c.HandshakeAsync();
        await WaitUntilAsync(() => server.Sessions.Count == 3, "three sessions");

        Assert.Equal([0L, 1L, 2L], server.Sessions.Snapshot().Select(s => s.Id));
    }

    [Fact]
    public async Task Stop_disconnects_every_client_and_stops_accepting()
    {
        var accept = new RecordingAcceptHandler();
        GameServer server = NewServer(new RecordingHandler(), accept);
        int port = server.Port;

        using TestClient a = await ConnectAsync(server);
        using TestClient b = await ConnectAsync(server);
        await a.HandshakeAsync();
        await b.HandshakeAsync();
        await WaitUntilAsync(() => server.Sessions.Count == 2, "two sessions");

        await server.StopAsync();

        Assert.True(await a.WaitForEndOfStreamAsync());
        Assert.True(await b.WaitForEndOfStreamAsync());
        Assert.Equal(0, server.Sessions.Count);
        Assert.Equal(2, accept.DisconnectCount);
        await Assert.ThrowsAnyAsync<SocketException>(async () =>
        {
            using var late = new TcpClient();
            await late.ConnectAsync(IPAddress.Loopback, port);
        });

        await server.StopAsync();
        await server.DisposeAsync();
    }

    [Fact]
    public async Task Stop_never_hangs_when_connections_arrive_at_the_same_moment()
    {
        for (int round = 0; round < 8; round++)
        {
            GameServer server = NewServer(new RecordingHandler(), maxPerIp: 1000);
            int port = server.Port;
            var clients = new List<TcpClient>();
            using var spam = new CancellationTokenSource();
            Task spammer = Task.Run(async () =>
            {
                while (!spam.IsCancellationRequested)
                {
                    var c = new TcpClient();
                    lock (clients)
                    {
                        clients.Add(c);
                    }

                    try
                    {
                        await c.ConnectAsync(IPAddress.Loopback, port);
                    }
                    catch (SocketException)
                    {
                        return;
                    }
                }
            });

            await Task.Delay(round % 15);
            await server.StopAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(0, server.Sessions.Count);

            spam.Cancel();
            await spammer.WaitAsync(TimeSpan.FromSeconds(5));
            lock (clients)
            {
                clients.ForEach(c => c.Dispose());
            }
        }
    }

    [Fact]
    public async Task Many_clients_from_different_slots_run_at_the_same_time_without_mixing_their_packets()
    {
        var handler = new RecordingHandler();
        await using GameServer server = NewServer(handler, maxPerIp: 20);

        const int clients = 12;
        const int perClient = 30;
        TestClient[] all = new TestClient[clients];
        for (int i = 0; i < clients; i++)
        {
            all[i] = await ConnectAsync(server);
            await all[i].HandshakeAsync();
        }

        await Task.WhenAll(all.Select(async (c, index) =>
        {
            for (int n = 0; n < perClient; n++)
            {
                await c.SendAsync((sbyte)(index + 1), BitConverter.GetBytes(n));
            }
        }));

        await handler.WaitForCountAsync(clients * perClient);
        foreach (IGrouping<sbyte, (sbyte Command, byte[] Payload)> group in handler.Snapshot().GroupBy(m => m.Command))
        {
            // Packets of one client (one command value here) keep their order.
            Assert.Equal(Enumerable.Range(0, perClient), group.Select(m => BitConverter.ToInt32(m.Payload)));
        }

        foreach (TestClient c in all)
        {
            c.Dispose();
        }
    }

    [Fact]
    public async Task Starting_twice_or_reading_the_port_before_starting_is_an_error()
    {
        var server = new GameServer(new GameServerOptions { Port = 0, BindAddress = IPAddress.Loopback }, new RecordingHandler());
        Assert.Throws<InvalidOperationException>(() => server.Port);
        server.Start();
        Assert.Throws<InvalidOperationException>(server.Start);
        await server.DisposeAsync();
    }

    [Fact]
    public void An_invalid_per_ip_limit_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new GameServer(new GameServerOptions { MaxConnectionsPerIp = 0 }, new RecordingHandler()));
    }

    // ---- IpConnectionLimiter

    [Fact]
    public void The_limiter_allows_exactly_the_maximum_per_ip_and_counts_each_ip_separately()
    {
        var limiter = new IpConnectionLimiter(3);
        Assert.True(limiter.TryAcquire("1.1.1.1"));
        Assert.True(limiter.TryAcquire("1.1.1.1"));
        Assert.True(limiter.TryAcquire("1.1.1.1"));
        Assert.False(limiter.TryAcquire("1.1.1.1"));
        Assert.True(limiter.TryAcquire("2.2.2.2"));
        Assert.Equal(3, limiter.CountFor("1.1.1.1"));
        Assert.Equal(1, limiter.CountFor("2.2.2.2"));

        limiter.Release("1.1.1.1");
        Assert.True(limiter.TryAcquire("1.1.1.1"));
    }

    [Fact]
    public void The_limiter_never_goes_negative_and_forgets_an_ip_when_its_last_connection_closes()
    {
        var limiter = new IpConnectionLimiter(2);
        limiter.Release("9.9.9.9");
        Assert.Equal(0, limiter.CountFor("9.9.9.9"));

        Assert.True(limiter.TryAcquire("9.9.9.9"));
        limiter.Release("9.9.9.9");
        limiter.Release("9.9.9.9");
        Assert.Equal(0, limiter.CountFor("9.9.9.9"));
        Assert.True(limiter.TryAcquire("9.9.9.9"));
        Assert.True(limiter.TryAcquire("9.9.9.9"));
        Assert.False(limiter.TryAcquire("9.9.9.9"));
    }

    [Fact]
    public void The_limiter_is_exact_under_concurrency()
    {
        var limiter = new IpConnectionLimiter(10);
        int granted = 0;
        Parallel.For(0, 2000, _ =>
        {
            if (limiter.TryAcquire("5.5.5.5"))
            {
                Interlocked.Increment(ref granted);
            }
        });

        Assert.Equal(10, granted);
        Assert.Equal(10, limiter.CountFor("5.5.5.5"));
    }

    // ---- SessionManager

    private static Session NewSession(long id) => new(id, "127.0.0.1", new MemoryStream(), new RecordingHandler());

    [Fact]
    public void The_manager_registers_finds_lists_and_removes_sessions()
    {
        var manager = new SessionManager();
        Session a = NewSession(manager.NextId());
        Session b = NewSession(manager.NextId());
        manager.Add(b);
        manager.Add(a);

        Assert.Equal(2, manager.Count);
        Assert.Equal([a.Id, b.Id], manager.Snapshot().Select(s => s.Id));
        Assert.True(manager.TryGet(b.Id, out ISession? found));
        Assert.Same(b, found);
        Assert.False(manager.TryGet(99, out _));

        Assert.True(manager.Remove(a));
        Assert.False(manager.Remove(a));
        Assert.Equal(1, manager.Count);
        Assert.Throws<InvalidOperationException>(() => manager.Add(b));
    }

    [Fact]
    public void The_manager_hands_out_unique_ids_under_concurrency()
    {
        var manager = new SessionManager();
        long[] ids = new long[5000];
        Parallel.For(0, ids.Length, i => ids[i] = manager.NextId());

        Assert.Equal(ids.Length, ids.Distinct().Count());
        Assert.Equal(0, ids.Min());
        Assert.Equal(ids.Length - 1, ids.Max());
    }
}
