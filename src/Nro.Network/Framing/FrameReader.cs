namespace Nro.Network.Framing;

/// <summary>
/// Decodes incoming packets from a byte stream. Port of <c>MessageSendCollect.readMessage</c> (client to server direction), plus the
/// mirror image for the server to client direction so that a simulated client, a proxy or a test can read what <see cref="FrameWriter"/> wrote.
/// <para>
/// Use <see cref="ForServer"/> on the server: client packets always carry a 2-byte size. Use <see cref="ForClient"/> for the opposite direction:
/// the eight <see cref="WireCommands.LargeSizeCommands"/> carry a 3-byte size. <see cref="Create"/> accepts any set of 3-byte commands; it exists
/// to test the open hypothesis that real clients also send some commands (<c>66</c>, <c>-28</c>) with a 3-byte size.
/// </para>
/// <para>
/// Key handling is the same as in Java: when <see cref="WireCipher.KeySent"/> is true every byte read advances the read pointer. A 3-byte size
/// always goes through the key, even before <see cref="WireCipher.KeySent"/> is set, mirroring <see cref="FrameWriter"/>.
/// </para>
/// Differences from Java: a stream that ends in the middle of a frame throws <see cref="EndOfStreamException"/> (Java silently returns a
/// message whose missing payload bytes are zero), and a stream that ends exactly between frames returns <c>null</c>.
/// </summary>
public sealed class FrameReader
{
    private static readonly IReadOnlySet<sbyte> None = new HashSet<sbyte>();

    private readonly WireCipher _cipher;
    private readonly IReadOnlySet<sbyte> _largeCommands;

    private FrameReader(WireCipher cipher, IReadOnlySet<sbyte> largeCommands)
    {
        ArgumentNullException.ThrowIfNull(cipher);
        ArgumentNullException.ThrowIfNull(largeCommands);
        _cipher = cipher;
        _largeCommands = largeCommands;
    }

    /// <summary>A reader for what the client sends to the server: every size field is 2 bytes.</summary>
    public static FrameReader ForServer(WireCipher cipher) => new(cipher, None);

    /// <summary>A reader for what the server sends to the client: the eight large commands use a 3-byte size.</summary>
    public static FrameReader ForClient(WireCipher cipher) => new(cipher, WireCommands.LargeSizeCommands);

    /// <summary>A reader that treats exactly <paramref name="threeByteSizeCommands"/> as 3-byte-size commands.</summary>
    public static FrameReader Create(WireCipher cipher, IReadOnlySet<sbyte> threeByteSizeCommands) => new(cipher, threeByteSizeCommands);

    /// <summary>
    /// Reads the next packet. Returns <c>null</c> when the stream ends cleanly before the first byte of a packet.
    /// Not safe to call concurrently on the same reader.
    /// </summary>
    public async ValueTask<Frame?> ReadAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        byte[] one = new byte[1];
        if (await stream.ReadAtLeastAsync(one, 1, throwOnEndOfStream: false, cancellationToken).ConfigureAwait(false) == 0)
        {
            return null;
        }

        bool encrypted = _cipher.KeySent;
        sbyte command = (sbyte)(encrypted ? _cipher.ReadKey(one[0]) : one[0]);

        int size;
        if (_largeCommands.Contains(command))
        {
            byte[] raw = new byte[3];
            await stream.ReadExactlyAsync(raw, cancellationToken).ConfigureAwait(false);

            // Client decode: size = ((v2 * 256) + v1) * 256 + v0 with v_i = readKey(raw_i) + 128 (that is, XOR 0x80 on a byte).
            int v0 = _cipher.ReadKey(raw[0]) ^ 0x80;
            int v1 = _cipher.ReadKey(raw[1]) ^ 0x80;
            int v2 = _cipher.ReadKey(raw[2]) ^ 0x80;
            size = (v2 << 16) | (v1 << 8) | v0;
        }
        else
        {
            byte[] raw = new byte[2];
            await stream.ReadExactlyAsync(raw, cancellationToken).ConfigureAwait(false);
            size = encrypted
                ? (_cipher.ReadKey(raw[0]) << 8) | _cipher.ReadKey(raw[1])
                : (raw[0] << 8) | raw[1];
        }

        byte[] payload = size == 0 ? [] : new byte[size];
        if (size > 0)
        {
            await stream.ReadExactlyAsync(payload, cancellationToken).ConfigureAwait(false);
            if (encrypted)
            {
                for (int i = 0; i < payload.Length; i++)
                {
                    payload[i] = _cipher.ReadKey(payload[i]);
                }
            }
        }

        return new Frame(command, payload);
    }
}
