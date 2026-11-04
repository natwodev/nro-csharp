namespace Nro.Network.Framing;

// Tạo và phân tích gói trao đổi khóa -27 trong bắt tay.
// Nguồn Java: network/logic/KeyHandler.java (payload là [độ dài][K0][K1^K0]..., độ dài là một byte có dấu nên tối đa 127; gói khóa truyền rõ, khóa XOR chỉ bật cho gói sau với vị trí ghi bằng 0; byte thừa sau khóa khi đọc được bỏ qua như client Unity).

public static class KeyPacket
{

    // Độ dài khóa lớn nhất client đọc được vì byte độ dài là có dấu.
    public const int MaxKeyLength = 127;

    // Tạo payload gói khóa bằng chuỗi XOR chênh lệch giữa các byte khóa.
    public static byte[] BuildPayload(ReadOnlySpan<byte> key)
    {
        if (key.Length is < 1 or > MaxKeyLength)
        {
            throw new ArgumentOutOfRangeException(nameof(key), key.Length, $"A key must be 1 to {MaxKeyLength} bytes long.");
        }

        byte[] payload = new byte[1 + key.Length];
        payload[0] = (byte)key.Length;
        payload[1] = key[0];
        for (int i = 1; i < key.Length; i++)
        {
            payload[1 + i] = (byte)(key[i] ^ key[i - 1]);
        }

        return payload;
    }

    // Khôi phục khóa phía client và bỏ qua byte sau khóa.
    public static byte[] ParsePayload(ReadOnlySpan<byte> payload)
    {
        if (payload.IsEmpty)
        {
            throw new FormatException("The key packet payload is empty.");
        }

        int length = (sbyte)payload[0];
        if (length < 1 || payload.Length < 1 + length)
        {
            throw new FormatException($"The key packet declares {length} key bytes but carries {payload.Length - 1}.");
        }

        byte[] key = payload.Slice(1, length).ToArray();
        for (int j = 0; j < key.Length - 1; j++)
        {
            key[j + 1] ^= key[j];
        }

        return key;
    }

    // Tạo gói khóa không mã hóa rồi bật KeySent để mọi gói sau được XOR.
    public static byte[] CreateServerFrame(WireCipher cipher)
    {
        ArgumentNullException.ThrowIfNull(cipher);
        if (cipher.KeySent)
        {
            throw new InvalidOperationException("The key packet was already sent on this connection.");
        }

        byte[] wire = FrameWriter.Encode(cipher, WireCommands.GetSessionId, BuildPayload(cipher.Key));
        cipher.KeySent = true;
        return wire;
    }
}
