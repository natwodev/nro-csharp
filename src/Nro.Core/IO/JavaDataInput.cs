using System.Buffers.Binary;

namespace Nro.Core.IO;

// Bộ đọc byte[] tương thích DataInputStream bọc ByteArrayInputStream của JDK 17: số nhiều byte big-endian, chuỗi modified UTF-8, không dùng BinaryReader hoặc Encoding.UTF8.
// Nguồn Java: JDK 17 src/java.base/share/classes/java/io/DataInputStream.java và src/java.base/share/classes/java/io/ByteArrayInputStream.java (big-endian; modified UTF-8; EOFException chỉ sau khi đã tiêu hết dữ liệu còn lại; Read trả -1 ở cuối kể cả len = 0; ReadByte trả sbyte; không sao chép mảng nguồn).
public sealed class JavaDataInput
{
    private readonly byte[] _buf;
    private int _pos;

    // Tạo bộ đọc trên mảng gốc, không sao chép nên thay đổi mảng sau đó sẽ được thấy khi đọc (Java: ByteArrayInputStream(byte[]))
    public JavaDataInput(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _buf = data;
    }

    // Số byte vẫn còn đọc được (Java: available)
    public int Available => _buf.Length - _pos;

    // Byte kế tiếp từ 0 đến 255, hoặc -1 ở cuối dữ liệu (Java: read)
    public int Read() => _pos < _buf.Length ? _buf[_pos++] : -1;

    // Đọc tối đa b.Length byte; trả số byte đã đọc hoặc -1 ở cuối dữ liệu (Java: read(byte[]))
    public int Read(byte[] b)
    {
        ArgumentNullException.ThrowIfNull(b);
        return Read(b, 0, b.Length);
    }

    // Kiểm tra phạm vi trước, trả -1 ở cuối kể cả len = 0; nếu không trả số byte đã đọc, có thể ít hơn len (Java: read(byte[], int, int))
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

    // Lấp đầy b; thiếu dữ liệu thì ném EndOfStreamException (Java: readFully(byte[]))
    public void ReadFully(byte[] b)
    {
        ArgumentNullException.ThrowIfNull(b);
        ReadFully(b, 0, b.Length);
    }

    // Lấp đầy đoạn b; khi thiếu, phần đã đọc vẫn nằm trong b và vị trí đã ở cuối rồi mới ném EndOfStreamException; len = 0 không ném kể cả khi đã hết dữ liệu (Java: readFully(byte[], int, int))
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

    // Bỏ qua tối đa n byte và trả số thực tế; n <= 0 trả 0, dừng ở cuối dữ liệu không ném lỗi (Java: skipBytes)
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

    // Đúng khi byte đọc được khác 0, gồm cả 02, 80 và FF (Java: readBoolean)
    public bool ReadBoolean()
    {
        int ch = Read();
        if (ch < 0)
        {
            throw new EndOfStreamException();
        }

        return ch != 0;
    }

    // Byte có dấu từ -128 đến 127, tương ứng byte của Java (Java: readByte)
    public sbyte ReadByte()
    {
        int ch = Read();
        if (ch < 0)
        {
            throw new EndOfStreamException();
        }

        return unchecked((sbyte)ch);
    }

    // Byte không dấu từ 0 đến 255 (Java: readUnsignedByte)
    public int ReadUnsignedByte()
    {
        int ch = Read();
        if (ch < 0)
        {
            throw new EndOfStreamException();
        }

        return ch;
    }

    // Số nguyên 16 bit có dấu theo big-endian (Java: readShort)
    public short ReadShort() => BinaryPrimitives.ReadInt16BigEndian(Take(2));

    // Số nguyên 16 bit không dấu theo big-endian (Java: readUnsignedShort)
    public int ReadUnsignedShort() => BinaryPrimitives.ReadUInt16BigEndian(Take(2));

    // Ký tự UTF-16 theo big-endian (Java: readChar)
    public char ReadChar() => (char)BinaryPrimitives.ReadUInt16BigEndian(Take(2));

    // Số nguyên 32 bit có dấu theo big-endian (Java: readInt)
    public int ReadInt() => BinaryPrimitives.ReadInt32BigEndian(Take(4));

    // Số nguyên 64 bit có dấu theo big-endian (Java: readLong)
    public long ReadLong() => BinaryPrimitives.ReadInt64BigEndian(Take(8));

    // Giữ nguyên mọi bit, kể cả phần tải NaN (Java: readFloat / Float.intBitsToFloat)
    public float ReadFloat() => BitConverter.Int32BitsToSingle(ReadInt());

    // Giữ nguyên mọi bit, kể cả phần tải NaN (Java: readDouble / Double.longBitsToDouble)
    public double ReadDouble() => BitConverter.Int64BitsToDouble(ReadLong());

    // Đọc modified UTF-8 sau độ dài uint16; thiếu byte ném EndOfStreamException, nhận 00/C0 80 là U+0000, mã hoá dài và surrogate đơn lẻ/cặp, từ chối F0..FF bằng UtfDataFormatException JDK và sau lỗi định dạng vị trí ở cuối đoạn đã khai báo (Java: readUTF)
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
                    // Dạng một byte 0xxxxxxx.
                    count++;
                    chars[charCount++] = (char)c;
                    break;
                case 12:
                case 13:
                {
                    // Dạng hai byte 110xxxxx 10xxxxxx.
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
                    // Dạng ba byte 1110xxxx 10xxxxxx 10xxxxxx.
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
                    // Dạng byte tiếp tục hoặc bốn byte không hợp lệ.
                    throw new UtfDataFormatException("malformed input around byte " + count);
            }
        }

        return new string(chars, 0, charCount);
    }

    // Không làm gì; vẫn đọc được sau khi đóng như DataInputStream và ByteArrayInputStream (Java: close)
    public void Close()
    {
    }

    // Lấy đúng n byte; thiếu thì tiêu thụ phần còn lại rồi ném EndOfStreamException như EOFException của JDK.
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

    // Lõi ByteArrayInputStream.skip(long) khi n > 0: bỏ qua tối đa n byte còn lại và trả số đã bỏ qua.
    private int Skip(int n)
    {
        int k = Math.Min(n, _buf.Length - _pos);
        _pos += k;
        return k;
    }
}
