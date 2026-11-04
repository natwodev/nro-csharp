namespace Nro.Network.Framing;

// Bộ mã hóa một gói gửi ra thành byte trên dây.
// Nguồn Java: network/io/MessageSendCollect.java (khung thường là [lệnh][kích thước big-endian 2 byte][payload], XOR từng byte sau KeySent; 8 lệnh lớn dùng kích thước little-endian 3 byte, mỗi byte XOR key rồi XOR 0x80 và luôn làm tiến key kể cả trước KeySent; C# từ chối quá cỡ trước khi dịch key và không sửa mảng payload của bên gọi).

public static class FrameWriter
{

    // Mã hóa khung gửi ra theo tập tám lệnh kích thước 3 byte mặc định.
    public static byte[] Encode(WireCipher cipher, sbyte command, ReadOnlySpan<byte> payload) =>
        Encode(cipher, command, payload, WireCommands.LargeSizeCommands);

    // Mã hóa khung gửi ra và làm tiến vị trí ghi XOR theo thứ tự byte giao thức.
    public static byte[] Encode(WireCipher cipher, sbyte command, ReadOnlySpan<byte> payload, IReadOnlySet<sbyte> threeByteSizeCommands)
    {
        ArgumentNullException.ThrowIfNull(cipher);
        ArgumentNullException.ThrowIfNull(threeByteSizeCommands);

        bool large = threeByteSizeCommands.Contains(command);
        int size = payload.Length;
        int maximum = large ? WireCommands.MaxThreeByteSize : WireCommands.MaxTwoByteSize;
        if (size > maximum)
        {
            throw new FrameTooLargeException(command, size, maximum);
        }

        bool encrypt = cipher.KeySent;
        int headerLength = large ? 4 : 3;
        byte[] wire = new byte[headerLength + size];

        wire[0] = encrypt ? cipher.WriteKey((byte)command) : (byte)command;
        if (large)
        {

            wire[1] = (byte)(cipher.WriteKey((byte)size) ^ 0x80);
            wire[2] = (byte)(cipher.WriteKey((byte)(size >> 8)) ^ 0x80);
            wire[3] = (byte)(cipher.WriteKey((byte)(size >> 16)) ^ 0x80);
        }
        else if (encrypt)
        {
            wire[1] = cipher.WriteKey((byte)(size >> 8));
            wire[2] = cipher.WriteKey((byte)(size & 0xFF));
        }
        else
        {
            wire[1] = (byte)(size >> 8);
            wire[2] = (byte)size;
        }

        Span<byte> body = wire.AsSpan(headerLength);
        if (encrypt)
        {
            for (int i = 0; i < payload.Length; i++)
            {
                body[i] = cipher.WriteKey(payload[i]);
            }
        }
        else
        {
            payload.CopyTo(body);
        }

        return wire;
    }
}
