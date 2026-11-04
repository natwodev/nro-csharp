using Nro.Core.IO;
using Nro.Network.Framing;

namespace Nro.Tests.Unit.Network.Framing;

/// <summary>Behaviour the golden vectors cannot show: arbitrary chunking of the stream, truncation, validation, and that nothing is modified behind the caller's back.</summary>
public class FrameCodecBehaviorTests
{
    /// <summary>A stream that hands out at most <c>chunk</c> bytes per read, to prove the reader does not depend on how TCP splits the bytes.</summary>
    private sealed class ChunkedStream(byte[] data, int chunk) : Stream
    {
        private int _position;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => data.Length;

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        public override int Read(Span<byte> buffer)
        {
            int n = Math.Min(Math.Min(buffer.Length, chunk), data.Length - _position);
            data.AsSpan(_position, n).CopyTo(buffer);
            _position += n;
            return n;
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => new(Read(buffer.Span));

        public override void Flush() => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private static byte[] Payload(int length, int seed) => NetworkGolden.Payload(length, seed);

    [Theory]
    [InlineData(false, 1)]
    [InlineData(false, 7)]
    [InlineData(true, 1)]
    [InlineData(true, 5)]
    [InlineData(true, 4096)]
    public async Task Frames_survive_any_chunking_of_the_stream(bool keySent, int chunk)
    {
        var writer = WireCipher.CreateDefault();
        writer.KeySent = keySent;
        var frames = new List<(sbyte Cmd, byte[] Payload)>();
        var all = new List<byte>();
        foreach (sbyte cmd in new sbyte[] { 1, -34, 12, 127, -128, 0 })
        {
            byte[] payload = Payload(cmd == 12 ? 0 : 300 + cmd, cmd & 0xFF);
            frames.Add((cmd, payload));
            all.AddRange(FrameWriter.Encode(writer, cmd, payload));
        }

        // The "server" reader reads what a "client" writer produced (ordinary commands only, 2-byte sizes).
        var readerCipher = WireCipher.CreateDefault();
        readerCipher.KeySent = keySent;
        FrameReader reader = FrameReader.ForServer(readerCipher);
        using var stream = new ChunkedStream(all.ToArray(), chunk);
        foreach ((sbyte cmd, byte[] payload) in frames)
        {
            Frame? frame = await reader.ReadAsync(stream);
            Assert.NotNull(frame);
            Assert.Equal(cmd, frame.Value.Command);
            Assert.True(payload.AsSpan().SequenceEqual(frame.Value.Payload));
        }

        Assert.Null(await reader.ReadAsync(stream));
    }

    [Fact]
    public void Large_commands_round_trip_between_the_server_writer_and_the_client_reader()
    {
        var serverCipher = WireCipher.CreateDefault();
        serverCipher.KeySent = true;
        var clientCipher = WireCipher.CreateDefault();
        clientCipher.KeySent = true;

        byte[] big = Payload(70_000, 3);
        byte[] wire = FrameWriter.Encode(serverCipher, -28, big);
        Assert.Equal(1 + 3 + big.Length, wire.Length);

        (Frame frame, int consumed) = NetworkGolden.ReadOne(FrameReader.ForClient(clientCipher), wire);
        Assert.Equal(-28, frame.Command);
        Assert.True(big.AsSpan().SequenceEqual(frame.Payload));
        Assert.Equal(wire.Length, consumed);
        Assert.Equal(serverCipher.WritePosition, clientCipher.ReadPosition);
    }

    [Fact]
    public async Task A_stream_that_ends_inside_a_frame_throws_and_one_that_ends_between_frames_returns_null()
    {
        var writer = WireCipher.CreateDefault();
        byte[] wire = FrameWriter.Encode(writer, 5, Payload(20, 1));

        for (int cut = 1; cut < wire.Length; cut++)
        {
            FrameReader reader = FrameReader.ForServer(WireCipher.CreateDefault());
            byte[] part = wire.AsSpan(0, cut).ToArray();
            await Assert.ThrowsAsync<EndOfStreamException>(async () => await reader.ReadAsync(new MemoryStream(part)));
        }

        FrameReader empty = FrameReader.ForServer(WireCipher.CreateDefault());
        Assert.Null(await empty.ReadAsync(new MemoryStream([])));
    }

    [Fact]
    public void The_writer_never_modifies_the_callers_payload()
    {
        var cipher = WireCipher.CreateDefault();
        cipher.KeySent = true;
        byte[] payload = Payload(64, 9);
        byte[] copy = (byte[])payload.Clone();

        FrameWriter.Encode(cipher, 3, payload);
        FrameWriter.Encode(cipher, -32, payload);

        Assert.True(copy.AsSpan().SequenceEqual(payload));
    }

    [Fact]
    public void An_oversize_payload_is_refused_for_both_size_widths_and_leaves_the_pointer_alone()
    {
        var cipher = WireCipher.CreateDefault();
        cipher.KeySent = true;

        var ordinary = Assert.Throws<FrameTooLargeException>(() => FrameWriter.Encode(cipher, 3, new byte[65_536]));
        Assert.Equal(65_535, ordinary.Maximum);
        var large = Assert.Throws<FrameTooLargeException>(() => FrameWriter.Encode(cipher, 66, new byte[(1 << 24)]));
        Assert.Equal((1 << 24) - 1, large.Maximum);
        Assert.Equal(0, cipher.WritePosition);

        Assert.Equal(65_535 + 3, FrameWriter.Encode(cipher, 3, new byte[65_535]).Length);
    }

    [Fact]
    public void Read_and_write_pointers_are_independent_and_wrap_around_the_key()
    {
        var cipher = new WireCipher([1, 2, 3]);
        Assert.Equal((byte)(1 ^ 0x10), cipher.WriteKey(0x10));
        Assert.Equal((byte)(1 ^ 0x20), cipher.ReadKey(0x20));
        cipher.WriteKey(0);
        cipher.WriteKey(0);
        Assert.Equal(0, cipher.WritePosition);
        Assert.Equal(1, cipher.ReadPosition);
    }

    [Fact]
    public void The_cipher_rejects_an_empty_key_and_copies_the_key_it_is_given()
    {
        Assert.Throws<ArgumentException>(() => new WireCipher(ReadOnlySpan<byte>.Empty));

        byte[] key = [9, 8, 7];
        var cipher = new WireCipher(key);
        key[0] = 0;
        Assert.Equal(9, cipher.Key[0]);
    }

    [Fact]
    public void The_default_key_is_the_documented_24_byte_text()
    {
        Assert.Equal("NguyenDucVuEntertainment", System.Text.Encoding.ASCII.GetString(WireCipher.CreateDefault().Key));
        Assert.Equal(24, WireCipher.CreateDefault().Key.Length);
    }

    [Fact]
    public void Key_packets_reject_lengths_the_client_cannot_read_and_malformed_payloads()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => KeyPacket.BuildPayload(ReadOnlySpan<byte>.Empty));
        Assert.Throws<ArgumentOutOfRangeException>(() => KeyPacket.BuildPayload(new byte[128]));
        Assert.Equal(128, KeyPacket.BuildPayload(new byte[127]).Length);

        Assert.Throws<FormatException>(() => KeyPacket.ParsePayload([]));
        Assert.Throws<FormatException>(() => KeyPacket.ParsePayload([5, 1, 2]));
        Assert.Throws<FormatException>(() => KeyPacket.ParsePayload([0x80, 1]));

        // Extra fields after the key (the Unity client reads some) are ignored.
        byte[] key = [4, 5, 6];
        byte[] payload = [.. KeyPacket.BuildPayload(key), 0xAA, 0xBB];
        Assert.True(key.AsSpan().SequenceEqual(KeyPacket.ParsePayload(payload)));
    }

    [Fact]
    public void The_server_key_packet_can_only_be_created_once_and_switches_encryption_on()
    {
        var cipher = WireCipher.CreateDefault();
        Assert.False(cipher.KeySent);
        byte[] first = KeyPacket.CreateServerFrame(cipher);
        Assert.Equal(0xE5, first[0]);
        Assert.True(cipher.KeySent);
        Assert.Throws<InvalidOperationException>(() => KeyPacket.CreateServerFrame(cipher));
    }

    [Fact]
    public void A_decoded_frame_converts_to_a_read_mode_Message()
    {
        var frame = new Frame(7, [0, 5, 0x41]);
        Message message = frame.ToMessage();
        Assert.Equal(7, message.Command);
        Assert.Equal(5, message.Reader().ReadShort());
    }
}
