using System.Net.Sockets;
using Nro.Core.IO;
using Nro.Network.Framing;
using Nro.Network.Protocol;
using Nro.Network.Sessions;

namespace Nro.Tests.Unit.Network;

internal static class TestTimeouts
{
    public static readonly TimeSpan Default = TimeSpan.FromSeconds(10);
}

internal sealed class RecordingHandler : IMessageHandler
{
    private readonly object _lock = new();
    private readonly List<(sbyte Command, byte[] Payload)> _received = [];
    private TaskCompletionSource? _gate;

    public Func<sbyte, bool> ThrowFor { get; set; } = _ => false;

    public TaskCompletionSource? Started { get; set; }

    public void BlockUntilReleased() => _gate = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public void Release() => _gate?.TrySetResult();

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _received.Count;
            }
        }
    }

    public (sbyte Command, byte[] Payload)[] Snapshot()
    {
        lock (_lock)
        {
            return [.. _received];
        }
    }

    public async ValueTask OnMessageAsync(ISession session, Message message, CancellationToken cancellationToken)
    {
        Started?.TrySetResult();
        if (_gate is not null)
        {
            await _gate.Task.WaitAsync(cancellationToken);
        }

        if (ThrowFor(message.Command))
        {
            throw new InvalidOperationException("handler failure on purpose");
        }

        lock (_lock)
        {
            _received.Add((message.Command, message.GetDataForTest()));
        }
    }

    public async Task WaitForCountAsync(int count)
    {
        DateTime end = DateTime.UtcNow + TestTimeouts.Default;
        while (Count < count)
        {
            Assert.True(DateTime.UtcNow < end, $"Timed out waiting for {count} messages (got {Count})");
            await Task.Delay(5);
        }
    }
}

internal sealed class RecordingAcceptHandler : ISessionAcceptHandler
{
    private int _init;
    private int _disconnect;
    private readonly TaskCompletionSource _disconnected = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public int InitCount => Volatile.Read(ref _init);

    public int DisconnectCount => Volatile.Read(ref _disconnect);

    public Task Disconnected => _disconnected.Task;

    public void SessionInit(ISession session) => Interlocked.Increment(ref _init);

    public void SessionDisconnect(ISession session)
    {
        Interlocked.Increment(ref _disconnect);
        _disconnected.TrySetResult();
    }
}

/// <summary>The client's side of the protocol, written with the same codec the server uses.</summary>
internal sealed class TestClient : IDisposable
{
    private readonly TcpClient _tcp;
    private readonly NetworkStream _stream;
    private readonly WireCipher _cipher = WireCipher.CreateDefault();
    private readonly FrameReader _reader;

    public TestClient(TcpClient tcp)
    {
        _tcp = tcp;
        _stream = tcp.GetStream();
        _reader = FrameReader.ForClient(_cipher);
    }

    /// <summary>Sends the hello (<c>-27</c>, clear text), reads the key packet and switches the client's cipher on.</summary>
    public async Task HandshakeAsync()
    {
        await _stream.WriteAsync(new byte[] { 0xE5, 0x00, 0x00 });
        Frame keyFrame = await ReadAsync();
        Assert.Equal(WireCommands.GetSessionId, keyFrame.Command);
        _cipher.SetKey(KeyPacket.ParsePayload(keyFrame.Payload));
        _cipher.KeySent = true;
    }

    /// <summary>
    /// Sends the hello and an already encrypted packet in ONE write (a client that does not wait for the key), then reads the key packet.
    /// The server must read that second packet as encrypted every time.
    /// </summary>
    public async Task HandshakePipelinedAsync(sbyte command, byte[] payload)
    {
        var writeCipher = WireCipher.CreateDefault();
        writeCipher.KeySent = true;
        byte[] packet = FrameWriter.Encode(writeCipher, command, payload, new HashSet<sbyte>());
        byte[] wire = new byte[3 + packet.Length];
        new byte[] { 0xE5, 0x00, 0x00 }.CopyTo(wire, 0);
        packet.CopyTo(wire, 3);
        await _stream.WriteAsync(wire);

        Frame keyFrame = await ReadAsync();
        Assert.Equal(WireCommands.GetSessionId, keyFrame.Command);
        _cipher.SetKey(KeyPacket.ParsePayload(keyFrame.Payload));
        _cipher.KeySent = true;
    }

    /// <summary>Sends only the hello (clear text) and does not read the answer.</summary>
    public Task SendHelloOnlyAsync() => _stream.WriteAsync(new byte[] { 0xE5, 0x00, 0x00 }).AsTask();

    /// <summary>True when no byte arrives within <paramref name="wait"/>.</summary>
    public async Task<bool> ReceivesNothingAsync(TimeSpan wait)
    {
        // Polls instead of reading: an abandoned read would swallow the first byte of the next real read.
        DateTime end = DateTime.UtcNow + wait;
        while (DateTime.UtcNow < end)
        {
            if (_tcp.Available > 0)
            {
                return false;
            }

            await Task.Delay(10);
        }

        return _tcp.Available == 0;
    }

    public async Task SendAsync(sbyte command, byte[] payload)
    {
        byte[] wire = FrameWriter.Encode(_cipher, command, payload, new HashSet<sbyte>());   // a client always sends a 2-byte size
        await _stream.WriteAsync(wire);
    }

    public async Task<Frame> ReadAsync()
    {
        Frame? frame = await _reader.ReadAsync(_stream).AsTask().WaitAsync(TestTimeouts.Default);
        Assert.NotNull(frame);
        return frame.Value;
    }

    public async Task<bool> WaitForEndOfStreamAsync()
    {
        byte[] one = new byte[1];
        int n = await _stream.ReadAsync(one).AsTask().WaitAsync(TestTimeouts.Default);
        return n == 0;
    }

    public void Dispose() => _tcp.Dispose();
}

internal static class MessageTestExtensions
{
    /// <summary>The payload of a read-mode message (what the handler received).</summary>
    public static byte[] GetDataForTest(this Message message)
    {
        int length = message.Reader().Available;
        var data = new byte[length];
        message.Reader().ReadFully(data);
        return data;
    }
}
