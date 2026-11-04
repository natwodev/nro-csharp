using System.Buffers.Binary;

namespace Nro.Core.IO;

// Bộ ghi vào bộ đệm byte tăng dần tương thích DataOutputStream bọc ByteArrayOutputStream: số nhiều byte theo big-endian, chuỗi theo modified UTF-8; không dùng BinaryWriter hoặc Encoding.UTF8.
// Nguồn Java: java.io.DataOutputStream bọc java.io.ByteArrayOutputStream của JDK 17 (bảo toàn big-endian, modified UTF-8, chuẩn hoá NaN khi ghi float/double, close/flush không làm ngừng ghi và cách tăng sức chứa của ByteArrayOutputStream).
// Khác biệt C# có chủ đích: null ném ArgumentNullException thay vì NullPointerException, phạm vi sai ném ArgumentOutOfRangeException thay vì IndexOutOfBoundsException; cả hai xảy ra trước khi ghi byte nào.
// Không an toàn khi nhiều luồng cùng ghi như Message Java; nhiều luồng chỉ gọi ToArray sau khi đã ngừng ghi thì an toàn.
public sealed class JavaDataOutput
{
    // Sức chứa ban đầu của ByteArrayOutputStream() trong JDK
    private const int DefaultCapacity = 32;

    // Số byte mã hoá tối đa của writeUTF vì độ dài là uint16
    private const int MaxUtfLength = 65535;

    private byte[] _buf;
    private int _count;

    // Tạo bộ ghi với sức chứa mặc định của ByteArrayOutputStream (Java: ByteArrayOutputStream())
    public JavaDataOutput()
        : this(DefaultCapacity)
    {
    }

    // Tạo bộ ghi với sức chứa ban đầu; giá trị âm ném lỗi như Java (Java: ByteArrayOutputStream(int))
    public JavaDataOutput(int capacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(capacity);
        _buf = new byte[capacity];
    }

    // Số byte đã ghi (Java: size)
    public int Size => _count;

    // Trả bản sao byte đã ghi để nơi ghi gói có thể mã hoá XOR tại chỗ dù cùng Message còn chờ gửi cho nhiều người chơi (Java: toByteArray)
    public byte[] ToArray() => _buf.AsSpan(0, _count).ToArray();

    // Ghi 8 bit thấp của b (Java: write)
    public void Write(int b)
    {
        Reserve(1);
        _buf[_count++] = unchecked((byte)b);
    }

    // Ghi toàn bộ mảng (Java: write(byte[]))
    public void Write(byte[] b)
    {
        ArgumentNullException.ThrowIfNull(b);
        Write(b, 0, b.Length);
    }

    // Ghi len byte từ off; phạm vi sai không ghi byte nào (Java: write(byte[], int, int))
    public void Write(byte[] b, int off, int len)
    {
        ArgumentNullException.ThrowIfNull(b);
        JavaIoChecks.CheckFromIndexSize(off, len, b.Length);
        Reserve(len);
        Buffer.BlockCopy(b, off, _buf, _count, len);
        _count += len;
    }

    // Ghi 1 byte: 01 khi true, 00 khi false (Java: writeBoolean)
    public void WriteBoolean(bool v) => Write(v ? 1 : 0);

    // Chỉ giữ 8 bit thấp; writeByte(256) ghi 00 và writeByte(-1) ghi FF (Java: writeByte)
    public void WriteByte(int v) => Write(v);

    // Chỉ giữ 16 bit thấp theo big-endian (Java: writeShort)
    public void WriteShort(int v)
    {
        Reserve(2);
        BinaryPrimitives.WriteInt16BigEndian(_buf.AsSpan(_count), unchecked((short)v));
        _count += 2;
    }

    // Như WriteShort: chỉ giữ 16 bit thấp theo big-endian (Java: writeChar)
    public void WriteChar(int v) => WriteShort(v);

    // Ghi số nguyên 32 bit có dấu theo big-endian (Java: writeInt)
    public void WriteInt(int v)
    {
        Reserve(4);
        BinaryPrimitives.WriteInt32BigEndian(_buf.AsSpan(_count), v);
        _count += 4;
    }

    // Ghi số nguyên 64 bit có dấu theo big-endian (Java: writeLong)
    public void WriteLong(long v)
    {
        Reserve(8);
        BinaryPrimitives.WriteInt64BigEndian(_buf.AsSpan(_count), v);
        _count += 8;
    }

    // Ghi bit float như Float.floatToIntBits: mọi NaN chuẩn hoá thành 7FC00000, không giữ phần tải NaN như BitConverter.SingleToInt32Bits (Java: writeFloat)
    public void WriteFloat(float v) => WriteInt(float.IsNaN(v) ? 0x7fc00000 : BitConverter.SingleToInt32Bits(v));

    // Ghi bit double như Double.doubleToLongBits: mọi NaN chuẩn hoá thành 7FF8000000000000 (Java: writeDouble)
    public void WriteDouble(double v) => WriteLong(double.IsNaN(v) ? 0x7ff8000000000000L : BitConverter.DoubleToInt64Bits(v));

    // Ghi mỗi đơn vị UTF-16 thành 1 byte thấp; ký tự lớn hơn U+00FF bị cắt như Java (Java: writeBytes)
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

    // Ghi mỗi đơn vị UTF-16 thành 2 byte big-endian (Java: writeChars)
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

    // Ghi modified UTF-8 đúng DataOutputStream.writeUTF của JDK 17: độ dài uint16 là số BYTE, U+0001..U+007F = 1 byte, U+0000/U+0080..U+07FF = 2 byte (U+0000 là C0 80), U+0800..U+FFFF = 3 byte; ngoài BMP là hai chuỗi 3 byte, surrogate đơn lẻ vẫn 3 byte; quá 65535 byte ném UtfDataFormatException trước khi đổi bộ đệm (Java: writeUTF)
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

    // Không làm gì vì bộ đệm ở bộ nhớ như flush() Java; mã game gọi writer().flush() tại 5 chỗ (Java: flush)
    public void Flush()
    {
    }

    // Không làm gì như close() của DataOutputStream/ByteArrayOutputStream: đóng rồi vẫn ghi được và ToArray vẫn trả toàn bộ dữ liệu; Message.Cleanup phụ thuộc điều này (Java: close)
    public void Close()
    {
    }

    // Tạo thông báo giống DataOutputStream.tooLongMsg của JDK 17, dùng độ dài thực không tràn int
    private static string TooLongMessage(string s, long utflen)
    {
        string head = s.Substring(0, 8);
        string tail = s.Substring(s.Length - 8, 8);
        return $"encoded string ({head}...{tail}) too long: {utflen} bytes";
    }

    // Bảo đảm chỗ cho extra byte nữa bằng cách gấp đôi sức chứa như ByteArrayOutputStream
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
