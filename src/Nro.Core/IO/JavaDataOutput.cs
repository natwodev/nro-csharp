using System.Buffers.Binary;

namespace Nro.Core.IO;

/// <summary>
/// Replacement for JDK 17's <c>java.io.DataOutputStream</c> wrapping <c>java.io.ByteArrayOutputStream</c>: writes to a growable byte buffer,
/// ALL multi-byte numbers are BIG-ENDIAN, strings are modified UTF-8 (<see cref="WriteUTF"/>).
/// Does not use <c>BinaryWriter</c> (little-endian) or <c>Encoding.UTF8</c>.
/// <para>
/// Method names follow Java, capitalised. The only deliberate differences: a <c>null</c> argument throws
/// <see cref="ArgumentNullException"/> (Java: <c>NullPointerException</c>) and an invalid range throws
/// <see cref="ArgumentOutOfRangeException"/> (Java: <c>IndexOutOfBoundsException</c>); both happen BEFORE any byte is written.
/// </para>
/// <para>
/// Not safe when several threads WRITE at once (just like using <c>Message</c> in the real Java code). Several threads calling
/// <see cref="ToArray"/> when nobody is writing any more is safe.
/// </para>
/// </summary>
public sealed class JavaDataOutput
{
    /// <summary>Initial capacity of the JDK's <c>ByteArrayOutputStream()</c>.</summary>
    private const int DefaultCapacity = 32;

    /// <summary>Maximum number of encoded bytes for <c>writeUTF</c> (the length is a uint16).</summary>
    private const int MaxUtfLength = 65535;

    private byte[] _buf;
    private int _count;

    public JavaDataOutput()
        : this(DefaultCapacity)
    {
    }

    /// <param name="capacity">Initial capacity (corresponds to <c>new ByteArrayOutputStream(size)</c>); a negative value throws, as in Java.</param>
    public JavaDataOutput(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _buf = new byte[capacity];
    }

    /// <summary>Number of bytes written (<c>DataOutputStream.size()</c>).</summary>
    public int Size => _count;

    /// <summary>
    /// A COPY of the bytes written (<c>ByteArrayOutputStream.toByteArray()</c>). It must be a copy: the caller (packet writer)
    /// XOR-encrypts this array in place, while the same <c>Message</c> may be waiting to be sent to many other players.
    /// </summary>
    public byte[] ToArray() => _buf.AsSpan(0, _count).ToArray();

    /// <summary>Writes the low 8 bits of <paramref name="b"/> (<c>write(int)</c>).</summary>
    public void Write(int b)
    {
        Reserve(1);
        _buf[_count++] = unchecked((byte)b);
    }

    /// <summary>Writes the whole array (<c>write(byte[])</c>).</summary>
    public void Write(byte[] b)
    {
        ArgumentNullException.ThrowIfNull(b);
        Write(b, 0, b.Length);
    }

    /// <summary>Writes <paramref name="len"/> bytes from <paramref name="off"/> (<c>write(byte[], int, int)</c>); an invalid range writes nothing.</summary>
    public void Write(byte[] b, int off, int len)
    {
        ArgumentNullException.ThrowIfNull(b);
        JavaIoChecks.CheckFromIndexSize(off, len, b.Length);
        Reserve(len);
        Buffer.BlockCopy(b, off, _buf, _count, len);
        _count += len;
    }

    /// <summary>1 byte: <c>01</c> if true, <c>00</c> if false.</summary>
    public void WriteBoolean(bool v) => Write(v ? 1 : 0);

    /// <summary>Keeps only the low 8 bits (<c>writeByte(256)</c> writes <c>00</c>, <c>writeByte(-1)</c> writes <c>FF</c>).</summary>
    public void WriteByte(int v) => Write(v);

    /// <summary>Keeps only the low 16 bits, big-endian.</summary>
    public void WriteShort(int v)
    {
        Reserve(2);
        BinaryPrimitives.WriteInt16BigEndian(_buf.AsSpan(_count), unchecked((short)v));
        _count += 2;
    }

    /// <summary>Same as <see cref="WriteShort"/>: keeps only the low 16 bits, big-endian.</summary>
    public void WriteChar(int v) => WriteShort(v);

    public void WriteInt(int v)
    {
        Reserve(4);
        BinaryPrimitives.WriteInt32BigEndian(_buf.AsSpan(_count), v);
        _count += 4;
    }

    public void WriteLong(long v)
    {
        Reserve(8);
        BinaryPrimitives.WriteInt64BigEndian(_buf.AsSpan(_count), v);
        _count += 8;
    }

    /// <summary>
    /// Like <c>Float.floatToIntBits</c>: the bits of the float, BUT every NaN is normalised to <c>7FC00000</c>
    /// (unlike <c>BitConverter.SingleToInt32Bits</c>, which preserves the NaN payload).
    /// </summary>
    public void WriteFloat(float v) => WriteInt(float.IsNaN(v) ? 0x7fc00000 : BitConverter.SingleToInt32Bits(v));

    /// <summary>Like <c>Double.doubleToLongBits</c>: every NaN is normalised to <c>7FF8000000000000</c>.</summary>
    public void WriteDouble(double v) => WriteLong(double.IsNaN(v) ? 0x7ff8000000000000L : BitConverter.DoubleToInt64Bits(v));

    /// <summary>Each character is written as 1 byte = the low 8 bits of the UTF-16 unit (characters &gt; U+00FF are truncated, as in Java).</summary>
    public void WriteBytes(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        int len = s.Length;
        Reserve(len);
        byte[] buf = _buf;
        int count = _count;
        for (int i = 0; i < len; i++)
        {
            buf[count++] = unchecked((byte)s[i]);
        }

        _count = count;
    }

    /// <summary>Each UTF-16 unit is written as 2 big-endian bytes.</summary>
    public void WriteChars(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        int len = s.Length;
        Reserve(2L * len);
        byte[] buf = _buf;
        int count = _count;
        for (int i = 0; i < len; i++)
        {
            int c = s[i];
            buf[count++] = unchecked((byte)(c >> 8));
            buf[count++] = unchecked((byte)c);
        }

        _count = count;
    }

    /// <summary>
    /// Modified UTF-8 exactly like JDK 17's <c>DataOutputStream.writeUTF</c>: a 2-byte length (uint16, the number of encoded BYTES) followed by
    /// each UTF-16 unit encoded separately: U+0001..U+007F = 1 byte; U+0000 and U+0080..U+07FF = 2 bytes (U+0000 is <c>C0 80</c>);
    /// U+0800..U+FFFF = 3 bytes. A character outside the BMP is TWO 3-byte sequences (a surrogate pair), and a lone surrogate is still encoded as 3 bytes.
    /// If it exceeds 65535 bytes, throws <see cref="UtfDataFormatException"/> BEFORE any byte is written (the buffer is left unchanged).
    /// </summary>
    public void WriteUTF(string s)
    {
        ArgumentNullException.ThrowIfNull(s);
        int strlen = s.Length;
        long utflen = strlen;
        for (int i = 0; i < strlen; i++)
        {
            int c = s[i];
            if (c >= 0x80 || c == 0)
            {
                utflen += (c >= 0x800) ? 2 : 1;
            }
        }

        if (utflen > MaxUtfLength)
        {
            throw new UtfDataFormatException(TooLongMessage(s, utflen));
        }

        Reserve(utflen + 2);
        byte[] buf = _buf;
        int count = _count;
        buf[count++] = unchecked((byte)(utflen >> 8));
        buf[count++] = unchecked((byte)utflen);
        for (int i = 0; i < strlen; i++)
        {
            int c = s[i];
            if (c < 0x80 && c != 0)
            {
                buf[count++] = (byte)c;
            }
            else if (c >= 0x800)
            {
                buf[count++] = (byte)(0xE0 | ((c >> 12) & 0x0F));
                buf[count++] = (byte)(0x80 | ((c >> 6) & 0x3F));
                buf[count++] = (byte)(0x80 | (c & 0x3F));
            }
            else
            {
                buf[count++] = (byte)(0xC0 | ((c >> 6) & 0x1F));
                buf[count++] = (byte)(0x80 | (c & 0x3F));
            }
        }

        _count = count;
    }

    /// <summary>Does nothing (in-memory buffer; Java's <c>flush()</c> does the same). There are 5 call sites of <c>writer().flush()</c> in the game code.</summary>
    public void Flush()
    {
    }

    /// <summary>
    /// Does nothing, like <c>close()</c> of <c>DataOutputStream</c>/<c>ByteArrayOutputStream</c>: AFTER closing, writing can continue
    /// and <see cref="ToArray"/> still returns all the data. Keep this behaviour (see <c>Message.Cleanup</c>).
    /// </summary>
    public void Close()
    {
    }

    /// <summary>The message is identical to the JDK 17 <c>DataOutputStream.tooLongMsg</c> (the real length, not overflowed as an int).</summary>
    private static string TooLongMessage(string s, long utflen)
    {
        string head = s.Substring(0, 8);
        string tail = s.Substring(s.Length - 8, 8);
        return $"encoded string ({head}...{tail}) too long: {utflen} bytes";
    }

    /// <summary>Ensures room for <paramref name="extra"/> more bytes (doubles the capacity, like <c>ByteArrayOutputStream</c>).</summary>
    private void Reserve(long extra)
    {
        long required = _count + extra;
        if (required <= _buf.Length)
        {
            return;
        }

        if (required > Array.MaxLength)
        {
            throw new OutOfMemoryException("The write buffer exceeds the maximum array size of .NET");
        }

        long newCapacity = Math.Max((long)_buf.Length * 2, required);
        if (newCapacity > Array.MaxLength)
        {
            newCapacity = Array.MaxLength;
        }

        Array.Resize(ref _buf, (int)newCapacity);
    }
}
