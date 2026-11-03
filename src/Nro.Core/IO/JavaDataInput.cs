using System.Buffers.Binary;

namespace Nro.Core.IO;

/// <summary>
/// Replacement for JDK 17's <c>java.io.DataInputStream</c> wrapping <c>java.io.ByteArrayInputStream</c>: reads from a <c>byte[]</c>,
/// multi-byte numbers are BIG-ENDIAN, strings are modified UTF-8 (<see cref="ReadUTF"/>). Does not use <c>BinaryReader</c>/<c>Encoding.UTF8</c>.
/// <para>
/// Preserves the JDK details that are easy to get silently wrong: when a full read cannot be satisfied it throws <see cref="EndOfStreamException"/>
/// (= <c>EOFException</c>) AFTER consuming whatever remains; <see cref="Read(byte[], int, int)"/> returns -1 at the end of the data
/// even when <c>len == 0</c>; <see cref="ReadByte"/> returns <c>sbyte</c> (Java's <c>byte</c> is signed).
/// </para>
/// <para>The constructor does NOT copy the source array (like <c>ByteArrayInputStream</c>): later changes to the source array show up when reading.</para>
/// </summary>
public sealed class JavaDataInput
{
    private readonly byte[] _buf;
    private int _pos;

    public JavaDataInput(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _buf = data;
    }

    /// <summary>Number of bytes still readable (<c>available()</c>).</summary>
    public int Available => _buf.Length - _pos;

    /// <summary>The next byte (0..255), or -1 at the end of the data (<c>read()</c>).</summary>
    public int Read() => _pos < _buf.Length ? _buf[_pos++] : -1;

    /// <summary>Reads up to <c>b.Length</c> bytes; returns the number of bytes read, or -1 at the end of the data (<c>read(byte[])</c>).</summary>
    public int Read(byte[] b)
    {
        ArgumentNullException.ThrowIfNull(b);
        return Read(b, 0, b.Length);
    }

    /// <summary>
    /// <c>read(byte[], int, int)</c>: checks the range first; at the end of the data returns -1 (even when <paramref name="len"/> = 0);
    /// otherwise returns the number of bytes read (possibly fewer than <paramref name="len"/>).
    /// </summary>
    public int Read(byte[] b, int off, int len)
    {
        ArgumentNullException.ThrowIfNull(b);
        JavaIoChecks.CheckFromIndexSize(off, len, b.Length);
        if (_pos >= _buf.Length)
        {
            return -1;
        }

        int avail = _buf.Length - _pos;
        if (len > avail)
        {
            len = avail;
        }

        if (len <= 0)
        {
            return 0;
        }

        Buffer.BlockCopy(_buf, _pos, b, off, len);
        _pos += len;
        return len;
    }

    /// <summary>Fills <paramref name="b"/> completely; if there is not enough data, throws <see cref="EndOfStreamException"/>.</summary>
    public void ReadFully(byte[] b)
    {
        ArgumentNullException.ThrowIfNull(b);
        ReadFully(b, 0, b.Length);
    }

    /// <summary>
    /// <c>readFully(byte[], int, int)</c>. If there is not enough data, the part that could be read is ALREADY in <paramref name="b"/>, the position is already at the end
    /// of the data, and only then is <see cref="EndOfStreamException"/> thrown. <c>len = 0</c> never throws (even when the data is already exhausted).
    /// </summary>
    public void ReadFully(byte[] b, int off, int len)
    {
        ArgumentNullException.ThrowIfNull(b);
        JavaIoChecks.CheckFromIndexSize(off, len, b.Length);
        int n = 0;
        while (n < len)
        {
            int count = Read(b, off + n, len - n);
            if (count < 0)
            {
                throw new EndOfStreamException();
            }

            n += count;
        }
    }

    /// <summary>
    /// <c>skipBytes(int)</c>: skips at most <paramref name="n"/> bytes and returns the number of bytes actually skipped. Returns 0 when <paramref name="n"/> &lt;= 0;
    /// stops at the end of the data (does not throw).
    /// </summary>
    public int SkipBytes(int n)
    {
        int total = 0;
        while (total < n)
        {
            int cur = Skip(n - total);
            if (cur <= 0)
            {
                break;
            }

            total += cur;
        }

        return total;
    }

    /// <summary>True when the byte read is non-zero (bytes 02, 80 and FF are all true).</summary>
    public bool ReadBoolean()
    {
        int ch = Read();
        if (ch < 0)
        {
            throw new EndOfStreamException();
        }

        return ch != 0;
    }

    /// <summary>SIGNED byte (-128..127), corresponding to Java's <c>byte</c>.</summary>
    public sbyte ReadByte()
    {
        int ch = Read();
        if (ch < 0)
        {
            throw new EndOfStreamException();
        }

        return unchecked((sbyte)ch);
    }

    /// <summary>UNSIGNED byte (0..255).</summary>
    public int ReadUnsignedByte()
    {
        int ch = Read();
        if (ch < 0)
        {
            throw new EndOfStreamException();
        }

        return ch;
    }

    public short ReadShort() => BinaryPrimitives.ReadInt16BigEndian(Take(2));

    public int ReadUnsignedShort() => BinaryPrimitives.ReadUInt16BigEndian(Take(2));

    public char ReadChar() => (char)BinaryPrimitives.ReadUInt16BigEndian(Take(2));

    public int ReadInt() => BinaryPrimitives.ReadInt32BigEndian(Take(4));

    public long ReadLong() => BinaryPrimitives.ReadInt64BigEndian(Take(8));

    /// <summary>Preserves every bit (including the NaN payload), like <c>Float.intBitsToFloat</c>.</summary>
    public float ReadFloat() => BitConverter.Int32BitsToSingle(ReadInt());

    /// <summary>Preserves every bit (including the NaN payload), like <c>Double.longBitsToDouble</c>.</summary>
    public double ReadDouble() => BitConverter.Int64BitsToDouble(ReadLong());

    /// <summary>
    /// Modified UTF-8 exactly like JDK 17's <c>DataInputStream.readUTF</c>: reads a uint16 length and then exactly that many bytes
    /// (<see cref="EndOfStreamException"/> if short), then decodes. Accepts a bare <c>00</c> and <c>C0 80</c> as U+0000, accepts overlong forms
    /// (e.g. <c>C1 81</c> = 'A'), accepts 2x3-byte surrogate pairs and lone surrogates; REJECTS standard 4-byte UTF-8 (lead F0..FF) with
    /// <see cref="UtfDataFormatException"/> carrying the same message as the JDK. After a format error, the position is at the end of the declared length.
    /// </summary>
    public string ReadUTF()
    {
        int utflen = ReadUnsignedShort();
        ReadOnlySpan<byte> bytes = Take(utflen);
        char[] chars = new char[utflen];
        int count = 0;
        int charCount = 0;

        while (count < utflen)
        {
            int c = bytes[count];
            if (c > 127)
            {
                break;
            }

            count++;
            chars[charCount++] = (char)c;
        }

        while (count < utflen)
        {
            int c = bytes[count];
            switch (c >> 4)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    // 0xxxxxxx
                    count++;
                    chars[charCount++] = (char)c;
                    break;
                case 12:
                case 13:
                {
                    // 110xxxxx 10xxxxxx
                    count += 2;
                    if (count > utflen)
                    {
                        throw new UtfDataFormatException("malformed input: partial character at end");
                    }

                    int char2 = bytes[count - 1];
                    if ((char2 & 0xC0) != 0x80)
                    {
                        throw new UtfDataFormatException("malformed input around byte " + count);
                    }

                    chars[charCount++] = (char)(((c & 0x1F) << 6) | (char2 & 0x3F));
                    break;
                }

                case 14:
                {
                    // 1110xxxx 10xxxxxx 10xxxxxx
                    count += 3;
                    if (count > utflen)
                    {
                        throw new UtfDataFormatException("malformed input: partial character at end");
                    }

                    int char2 = bytes[count - 2];
                    int char3 = bytes[count - 1];
                    if (((char2 & 0xC0) != 0x80) || ((char3 & 0xC0) != 0x80))
                    {
                        throw new UtfDataFormatException("malformed input around byte " + (count - 1));
                    }

                    chars[charCount++] = (char)(((c & 0x0F) << 12) | ((char2 & 0x3F) << 6) | (char3 & 0x3F));
                    break;
                }

                default:
                    // 10xxxxxx, 1111xxxx
                    throw new UtfDataFormatException("malformed input around byte " + count);
            }
        }

        return new string(chars, 0, charCount);
    }

    /// <summary>Does nothing, like <c>close()</c> of <c>DataInputStream</c>/<c>ByteArrayInputStream</c>: reading after closing still works.</summary>
    public void Close()
    {
    }

    /// <summary>
    /// Takes exactly the next <paramref name="n"/> bytes. If there are too few, CONSUMES whatever remains and then throws <see cref="EndOfStreamException"/>
    /// (the JDK reads byte by byte / via <c>readFully</c>, so the position is also at the end when the <c>EOFException</c> is thrown).
    /// </summary>
    private ReadOnlySpan<byte> Take(int n)
    {
        if (_buf.Length - _pos < n)
        {
            _pos = _buf.Length;
            throw new EndOfStreamException();
        }

        var span = new ReadOnlySpan<byte>(_buf, _pos, n);
        _pos += n;
        return span;
    }

    /// <summary>
    /// Core of <c>ByteArrayInputStream.skip(long)</c> for n &gt; 0 (only <see cref="SkipBytes"/> calls it, and always with n &gt; 0,
    /// so the n &lt; 0 branch is unnecessary): skips at most <paramref name="n"/> of the remaining bytes and returns the number of bytes skipped.
    /// </summary>
    private int Skip(int n)
    {
        int k = Math.Min(n, _buf.Length - _pos);
        _pos += k;
        return k;
    }
}
