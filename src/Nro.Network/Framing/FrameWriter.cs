namespace Nro.Network.Framing;

/// <summary>
/// Encodes one outgoing packet. Port of <c>MessageSendCollect.doSendMessage</c> (server to client direction).
/// <list type="bullet">
/// <item><description><b>Clear text</b> (<see cref="WireCipher.KeySent"/> false): <c>[cmd][size high][size low][payload]</c>, size a big-endian <c>uint16</c>.</description></item>
/// <item><description><b>Encrypted</b> (<see cref="WireCipher.KeySent"/> true): every byte of that same layout is XORed with the next write-side key byte, in order.</description></item>
/// <item><description><b>Large commands</b> (<see cref="WireCommands.LargeSizeCommands"/>): the size field is 3 bytes, little-endian, each byte
/// XORed with the key and then XORed with <c>0x80</c> (Java subtracts 128). This path ALWAYS uses and advances the key, even while
/// <see cref="WireCipher.KeySent"/> is false (golden quirk <c>extended-cmd-before-sentKey</c>). It never happens in a normal session because the
/// first packet is the key packet.</description></item>
/// </list>
/// Differences from the Java code, both deliberate: a payload larger than the size field can describe throws <see cref="FrameTooLargeException"/>
/// (Java truncates the size and emits a corrupt frame), and the caller's payload is never modified (Java encrypts the message's data array in place).
/// The cipher pointers are advanced only after the size check, so a refused payload leaves the cipher untouched.
/// </summary>
public static class FrameWriter
{
    /// <summary>Returns the exact bytes to put on the wire for (<paramref name="command"/>, <paramref name="payload"/>) and advances the write pointer.</summary>
    public static byte[] Encode(WireCipher cipher, sbyte command, ReadOnlySpan<byte> payload) =>
        Encode(cipher, command, payload, WireCommands.LargeSizeCommands);

    /// <summary>
    /// Like <see cref="Encode(WireCipher, sbyte, ReadOnlySpan{byte})"/> for a chosen set of commands that use the 3-byte size. The server uses
    /// <see cref="WireCommands.LargeSizeCommands"/>; a simulated CLIENT passes an empty set, because client packets carry a 2-byte size
    /// (the mirror image of <see cref="FrameReader.ForServer"/>).
    /// </summary>
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
            // Java: dos.writeByte(writeKey(size) - 128), which is the same as XOR 0x80 on a byte.
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
