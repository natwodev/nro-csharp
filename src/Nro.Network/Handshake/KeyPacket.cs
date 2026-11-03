using Nro.Network.Framing;

namespace Nro.Network.Handshake;

/// <summary>
/// The key packet (<c>-27</c>) the server sends right after the client's hello. Port of <c>KeyHandler.sendKey</c> / <c>KeyHandler.setKey</c>.
/// <para>
/// Payload: <c>[length][K0][K1^K0][K2^K1]...[K(n-1)^K(n-2)]</c>, where <c>length</c> is ONE byte, read as a signed byte by the client,
/// so a key longer than 127 bytes cannot be used. The packet is sent in clear text (the cipher is not active yet); the cipher starts
/// with the next packet, with the write pointer at 0.
/// </para>
/// </summary>
public static class KeyPacket
{
    /// <summary>The longest key the client can read (its length byte is signed).</summary>
    public const int MaxKeyLength = 127;

    /// <summary>Builds the payload of the key packet for <paramref name="key"/>.</summary>
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

    /// <summary>
    /// Recovers the key from a key-packet payload (the client side). Bytes after the key (the Unity client reads extra fields there) are ignored.
    /// </summary>
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

    /// <summary>
    /// Produces the wire bytes of the server's key packet for <paramref name="cipher"/> and then switches the cipher on
    /// (<see cref="WireCipher.KeySent"/> = true), so every later packet is encrypted. The key packet itself is not encrypted.
    /// The cipher must not have been switched on already.
    /// </summary>
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
