using System.Net;
using System.Net.Sockets;
using Nro.Core.IO;
using Nro.Core.Util;
using Nro.Network.Framing;
using Nro.Network.Protocol;
using Nro.Network.Sessions;

namespace Nro.Tests.Unit.Network.Sessions;

/// <summary>
/// The server-side <see cref="Session"/> over a real TCP connection on the loopback interface, with a small simulated client:
/// handshake, ordering, size limits, queue limits, error handling and disconnect.
/// </summary>
public class SessionTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private sealed class Pair : IAsyncDisposable
    {
        public required Session Session { get; init; }

        public required TestClient Client { get; init; }

        public required Task Run { get; init; }

        public required TcpListener Listener { get; init; }

        public async ValueTask DisposeAsync()
        {
            Session.Disconnect();
            Client.Dispose();
            Listener.Stop();
            await Run.WaitAsync(Timeout);
        }
    }

    private static async Task<Pair> StartAsync(IMessageHandler handler, ISessionAcceptHandler? accept = null, SessionOptions? options = null)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var tcpClient = new TcpClient();
        Task connect = tcpClient.ConnectAsync(IPAddress.Loopback, ((IPEndPoint)listener.LocalEndpoint).Port);
        TcpClient serverSide = await listener.AcceptTcpClientAsync();
        await connect;

        var session = new Session(1, "127.0.0.1", serverSide.GetStream(), handler, accept, options);
        return new Pair { Session = session, Client = new TestClient(tcpClient), Run = session.RunAsync(), Listener = listener };
    }

    private static Message Packet(sbyte command, byte[] payload)
    {
        var message = new Message(command);
        message.Writer().Write(payload);
        return message;
    }

    [Fact]
    public async Task Handshake_then_packets_flow_both_ways_encrypted()
    {
        var handler = new RecordingHandler();
        var accept = new RecordingAcceptHandler();
        await using Pair p = await StartAsync(handler, accept);
        Assert.Equal(0, accept.DisconnectCount);

        await p.Client.HandshakeAsync();

        byte[] up = NetworkGolden.Payload(40, 3);
        await p.Client.SendAsync(7, up);
        await handler.WaitForCountAsync(1);
        (sbyte Command, byte[] Payload) got = handler.Snapshot()[0];
        Assert.Equal(7, got.Command);
        Assert.True(up.AsSpan().SequenceEqual(got.Payload));

        byte[] down = NetworkGolden.Payload(90, 9);
        Assert.True(p.Session.Send(Packet(-30, down)));
        Frame reply = await p.Client.ReadAsync();
        Assert.Equal(-30, reply.Command);
        Assert.True(down.AsSpan().SequenceEqual(reply.Payload));

        Assert.Equal(1, accept.InitCount);
        Assert.True(p.Session.IsConnected);
    }

    [Fact]
    public async Task Packets_keep_their_order_in_both_directions()
    {
        var handler = new RecordingHandler();
        await using Pair p = await StartAsync(handler);
        await p.Client.HandshakeAsync();

        const int count = 400;
        for (int i = 0; i < count; i++)
        {
            await p.Client.SendAsync(5, BitConverter.GetBytes(i));
            Assert.True(p.Session.Send(Packet(6, BitConverter.GetBytes(i))));
        }

        await handler.WaitForCountAsync(count);
        (sbyte Command, byte[] Payload)[] received = handler.Snapshot();
        for (int i = 0; i < count; i++)
        {
            Assert.Equal(i, BitConverter.ToInt32(received[i].Payload));
            Frame reply = await p.Client.ReadAsync();
            Assert.Equal(i, BitConverter.ToInt32(reply.Payload));
        }
    }

    [Fact]
    public async Task A_large_command_packet_bigger_than_64KiB_arrives_intact()
    {
        var handler = new RecordingHandler();
        await using Pair p = await StartAsync(handler);
        await p.Client.HandshakeAsync();

        byte[] big = NetworkGolden.Payload(200_000, 5);
        Assert.True(p.Session.Send(Packet(-28, big)));
        Frame frame = await p.Client.ReadAsync();
        Assert.Equal(-28, frame.Command);
        Assert.True(big.AsSpan().SequenceEqual(frame.Payload));
    }

    [Fact]
    public async Task A_packet_too_large_for_its_size_field_is_skipped_and_the_session_carries_on()
    {
        await using Pair p = await StartAsync(new RecordingHandler());
        await p.Client.HandshakeAsync();

        Assert.True(p.Session.Send(Packet(3, new byte[70_000])));
        Assert.True(p.Session.Send(Packet(4, [1, 2, 3])));

        Frame frame = await p.Client.ReadAsync();
        Assert.Equal(4, frame.Command);
        Assert.Equal(new byte[] { 1, 2, 3 }, frame.Payload);
        Assert.True(p.Session.IsConnected);
    }

    [Fact]
    public async Task The_payload_is_captured_when_Send_is_called_so_the_caller_may_dispose_the_message()
    {
        await using Pair p = await StartAsync(new RecordingHandler());
        await p.Client.HandshakeAsync();

        Message message = Packet(8, [9, 8, 7]);
        Assert.True(p.Session.Send(message));
        message.Dispose();

        Frame frame = await p.Client.ReadAsync();
        Assert.Equal(new byte[] { 9, 8, 7 }, frame.Payload);
    }

    [Fact]
    public async Task Inbound_packets_beyond_the_queue_limit_are_dropped_like_the_original_500()
    {
        var handler = new RecordingHandler { Started = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        handler.BlockUntilReleased();
        await using Pair p = await StartAsync(handler, options: new SessionOptions { InboundQueueLimit = 500 });
        await p.Client.HandshakeAsync();

        await p.Client.SendAsync(5, BitConverter.GetBytes(0));
        await handler.Started.Task.WaitAsync(Timeout);

        for (int i = 1; i <= 700; i++)
        {
            await p.Client.SendAsync(5, BitConverter.GetBytes(i));
        }

        DateTime end = DateTime.UtcNow + Timeout;
        while (p.Session.DroppedInboundCount < 200)
        {
            Assert.True(DateTime.UtcNow < end, $"Only {p.Session.DroppedInboundCount} packets dropped");
            await Task.Delay(5);
        }

        Assert.Equal(200, p.Session.DroppedInboundCount);
        handler.Release();
        await handler.WaitForCountAsync(501);

        (sbyte Command, byte[] Payload)[] received = handler.Snapshot();
        Assert.Equal(501, received.Length);
        for (int i = 0; i <= 500; i++)
        {
            Assert.Equal(i, BitConverter.ToInt32(received[i].Payload));
        }
    }

    [Fact]
    public async Task A_second_hello_disconnects_the_session_like_the_java_server()
    {
        var handler = new RecordingHandler();
        await using Pair p = await StartAsync(handler);
        await p.Client.HandshakeAsync();

        await p.Client.SendAsync(WireCommands.GetSessionId, []);

        Assert.True(await p.Client.WaitForEndOfStreamAsync());
        await p.Run.WaitAsync(Timeout);
        Assert.False(p.Session.IsConnected);
        Assert.Equal(0, handler.Count);
    }

    private sealed class GreetingAcceptHandler : ISessionAcceptHandler
    {
        public void SessionInit(ISession session) => session.Send(Packet(77, [1, 2, 3]));

        public void SessionDisconnect(ISession session)
        {
        }
    }

    [Fact]
    public async Task Nothing_is_written_before_the_key_packet_and_early_packets_then_leave_encrypted()
    {
        await using Pair p = await StartAsync(new RecordingHandler(), new GreetingAcceptHandler());

        Assert.True(await p.Client.ReceivesNothingAsync(TimeSpan.FromMilliseconds(400)), "the server wrote before the hello");

        await p.Client.HandshakeAsync();
        Frame greeting = await p.Client.ReadAsync();
        Assert.Equal(77, greeting.Command);
        Assert.Equal(new byte[] { 1, 2, 3 }, greeting.Payload);
    }

    [Fact]
    public async Task A_packet_pipelined_right_behind_the_hello_is_always_read_as_encrypted()
    {
        for (int round = 0; round < 40; round++)
        {
            var handler = new RecordingHandler();
            await using Pair p = await StartAsync(handler);
            await p.Client.HandshakePipelinedAsync(7, [9, 8, 7]);
            await handler.WaitForCountAsync(1);
            Assert.Equal(7, handler.Snapshot()[0].Command);
            Assert.Equal(new byte[] { 9, 8, 7 }, handler.Snapshot()[0].Payload);
        }
    }

    [Fact]
    public async Task A_connection_that_never_says_hello_is_closed_after_the_handshake_timeout()
    {
        var options = new SessionOptions { HandshakeTimeout = TimeSpan.FromMilliseconds(300) };
        await using Pair p = await StartAsync(new RecordingHandler(), options: options);

        Assert.True(await p.Client.WaitForEndOfStreamAsync());
        await p.Run.WaitAsync(Timeout);
    }

    [Fact]
    public async Task Junk_packets_before_the_hello_do_not_extend_the_handshake_deadline()
    {
        var options = new SessionOptions { HandshakeTimeout = TimeSpan.FromMilliseconds(700) };
        await using Pair p = await StartAsync(new RecordingHandler(), options: options);

        DateTime start = DateTime.UtcNow;
        while (p.Session.IsConnected && DateTime.UtcNow - start < TimeSpan.FromSeconds(4))
        {
            try
            {
                await p.Client.SendAsync(7, [1]);
            }
            catch (IOException)
            {
                break;
            }

            await Task.Delay(200);
        }

        Assert.False(p.Session.IsConnected, "the junk packets kept the unauthenticated session alive");
        Assert.True(DateTime.UtcNow - start < TimeSpan.FromSeconds(3));
    }

    [Fact]
    public async Task An_idle_timeout_closes_a_silent_established_connection_but_not_an_active_one()
    {
        var handler = new RecordingHandler();
        var options = new SessionOptions { IdleTimeout = TimeSpan.FromMilliseconds(600) };
        await using Pair p = await StartAsync(handler, options: options);
        await p.Client.HandshakeAsync();

        for (int i = 0; i < 4; i++)
        {
            await Task.Delay(300);
            await p.Client.SendAsync(5, [1]);
        }

        Assert.True(p.Session.IsConnected);
        Assert.True(await p.Client.WaitForEndOfStreamAsync());
        await p.Run.WaitAsync(Timeout);
    }

    [Fact]
    public void Timeouts_must_be_positive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Session(1, "127.0.0.1", new MemoryStream(), new RecordingHandler(), null, new SessionOptions { HandshakeTimeout = TimeSpan.Zero }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Session(1, "127.0.0.1", new MemoryStream(), new RecordingHandler(), null, new SessionOptions { IdleTimeout = TimeSpan.Zero }));
    }

    [Fact]
    public async Task A_handler_exception_does_not_stop_delivery_of_later_packets()
    {
        var handler = new RecordingHandler { ThrowFor = cmd => cmd == 9 };
        await using Pair p = await StartAsync(handler);
        await p.Client.HandshakeAsync();

        await p.Client.SendAsync(9, [1]);
        await p.Client.SendAsync(10, [2]);
        await handler.WaitForCountAsync(1);

        Assert.Equal(10, handler.Snapshot()[0].Command);
        Assert.True(p.Session.IsConnected);
    }

    [Fact]
    public async Task Unknown_commands_are_delivered_like_any_other()
    {
        var handler = new RecordingHandler();
        await using Pair p = await StartAsync(handler);
        await p.Client.HandshakeAsync();

        await p.Client.SendAsync(-100, [5, 5]);
        await handler.WaitForCountAsync(1);
        Assert.Equal(-100, handler.Snapshot()[0].Command);
    }

    [Fact]
    public async Task When_the_client_disconnects_the_session_ends_once_and_stops_accepting_packets()
    {
        var accept = new RecordingAcceptHandler();
        await using Pair p = await StartAsync(new RecordingHandler(), accept);
        await p.Client.HandshakeAsync();

        p.Client.Dispose();
        await accept.Disconnected.WaitAsync(Timeout);
        await p.Run.WaitAsync(Timeout);

        Assert.False(p.Session.IsConnected);
        Assert.False(p.Session.Send(Packet(1, [])));
        Assert.Equal(1, accept.InitCount);
        Assert.Equal(1, accept.DisconnectCount);
    }

    [Fact]
    public async Task Disconnect_from_the_server_closes_the_connection_and_notifies_once_even_when_called_twice()
    {
        var accept = new RecordingAcceptHandler();
        await using Pair p = await StartAsync(new RecordingHandler(), accept);
        await p.Client.HandshakeAsync();

        p.Session.Disconnect();
        p.Session.Disconnect();

        Assert.True(await p.Client.WaitForEndOfStreamAsync());
        await accept.Disconnected.WaitAsync(Timeout);
        await p.Run.WaitAsync(Timeout);
        Assert.Equal(1, accept.DisconnectCount);
        Assert.False(p.Session.IsConnected);
    }

    /// <summary>A stream whose reads and writes never complete until cancelled or disposed (a client that stopped reading).</summary>
    private sealed class StalledStream : Stream
    {
        private readonly TaskCompletionSource _closed = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await _closed.Task.WaitAsync(cancellationToken);
            return 0;
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
            await _closed.Task.WaitAsync(cancellationToken);

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            _closed.TrySetResult();
            base.Dispose(disposing);
        }
    }

    [Fact]
    public async Task A_client_that_stops_reading_is_disconnected_when_the_outbound_queue_is_full()
    {
        var accept = new RecordingAcceptHandler();
        var session = new Session(1, "10.0.0.1", new StalledStream(), new RecordingHandler(), accept, new SessionOptions { OutboundQueueLimit = 3 });
        Task run = session.RunAsync();

        bool refused = false;
        for (int i = 0; i < 20 && !refused; i++)
        {
            refused = !session.Send(Packet(2, [(byte)i]));
        }

        Assert.True(refused, "A full outbound queue must refuse the packet");
        Assert.False(session.IsConnected);
        await run.WaitAsync(Timeout);
        Assert.Equal(1, accept.DisconnectCount);
    }

    [Fact]
    public void The_queue_limits_must_be_positive()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Session(1, "x", new MemoryStream(), new RecordingHandler(), null, new SessionOptions { InboundQueueLimit = 0 }));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new Session(1, "x", new MemoryStream(), new RecordingHandler(), null, new SessionOptions { OutboundQueueLimit = 0 }));
    }
}
