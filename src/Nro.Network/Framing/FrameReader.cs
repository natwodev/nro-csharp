namespace Nro.Network.Framing;

// Bộ giải mã khung gói từ luồng byte theo hướng client-server hoặc server-client.
// Nguồn Java: network/io/MessageSendCollect.java (server đọc kích thước 2 byte; client mô phỏng đọc 8 lệnh kích thước 3 byte little-endian; kích thước 3 byte luôn qua XOR rồi XOR 0x80 kể cả trước KeySent; EOF giữa khung ném EndOfStreamException thay vì trả payload thiếu toàn số 0, EOF giữa hai khung trả null; không an toàn khi đọc đồng thời cùng reader).

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

    // Tạo reader cho chiều client gửi server; mọi trường kích thước đều 2 byte.
    public static FrameReader ForServer(WireCipher cipher) => new(cipher, None);

    // Tạo reader cho chiều server gửi client; tám lệnh lớn dùng kích thước 3 byte.
    public static FrameReader ForClient(WireCipher cipher) => new(cipher, WireCommands.LargeSizeCommands);

    // Tạo reader với tập lệnh kích thước 3 byte chỉ định để kiểm thử giả thuyết client.
    public static FrameReader Create(WireCipher cipher, IReadOnlySet<sbyte> threeByteSizeCommands) => new(cipher, threeByteSizeCommands);

    // Đọc khung kế tiếp; trả null chỉ khi EOF sạch trước byte đầu và không gọi đồng thời cùng reader.
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
