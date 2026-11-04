using Nro.Core.IO;

namespace Nro.Tests.Unit.Core.IO;

/// <summary>
/// A REFERENCE encoder written independently inside the test (structurally different from <c>JavaDataOutput</c>: it uses value-range tables and hand-written bit shifts,
/// no <c>BinaryPrimitives</c>) to cross-check what gets written on random data. The source of truth remains the JDK vectors and the SHA-256 constants taken from the JDK.
/// </summary>
internal static class ReferenceCodec
{
    /// <summary>Number of modified UTF-8 bytes of the string (not counting the 2 length bytes).</summary>
    public static long Utf8Length(string s)
    {
        long n = 0;
        foreach (char ch in s)
        {
            n += ch switch
            {
                >= '\u0001' and <= '\u007F' => 1,
                '\u0000' => 2,
                >= '\u0080' and <= '߿' => 2,
                _ => 3,
            };
        }

        return n;
    }

    /// <summary>2 length bytes + modified UTF-8; returns null if it exceeds 65535 bytes (the JDK throws UTFDataFormatException).</summary>
    public static byte[]? EncodeUtf(string s)
    {
        long len = Utf8Length(s);
        if (len > 65535)
        {
            return null;
        }

        var result = new List<byte>((int)len + 2)
        {
            (byte)(len / 256),
            (byte)(len % 256),
        };
        foreach (char ch in s)
        {
            int u = ch;
            if (u >= 1 && u <= 0x7F)
            {
                result.Add((byte)u);
            }
            else if (u == 0 || u <= 0x7FF)
            {
                result.Add((byte)(0xC0 + (u / 64)));
                result.Add((byte)(0x80 + (u % 64)));
            }
            else
            {
                result.Add((byte)(0xE0 + (u / 4096)));
                result.Add((byte)(0x80 + ((u / 64) % 64)));
                result.Add((byte)(0x80 + (u % 64)));
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// The last <paramref name="byteCount"/> bytes of <paramref name="value"/>, high byte first (big-endian), using hand-written shifts.
    /// Negative numbers use two's complement (arithmetic shift), so this also works for truncated values (e.g. <c>writeShort(70000)</c> = the 2 low bytes).
    /// </summary>
    public static byte[] BigEndian(long value, int byteCount)
    {
        var bytes = new byte[byteCount];
        for (int i = 0; i < byteCount; i++)
        {
            int shift = 8 * (byteCount - 1 - i);
            bytes[i] = (byte)((value >> shift) & 0xFF);
        }

        return bytes;
    }

    /// <summary>Bits of a float per <c>Float.floatToIntBits</c>: every NaN becomes 0x7FC00000.</summary>
    public static int FloatBits(float f) => float.IsNaN(f) ? 0x7FC00000 : BitConverter.SingleToInt32Bits(f);

    /// <summary>Bits of a double per <c>Double.doubleToLongBits</c>: every NaN becomes 0x7FF8000000000000.</summary>
    public static long DoubleBits(double d) => double.IsNaN(d) ? 0x7FF8000000000000L : BitConverter.DoubleToInt64Bits(d);
}
