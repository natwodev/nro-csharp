namespace Nro.Core.IO;

// Gói tin gồm lệnh Command (sbyte) và một luồng ghi hoặc một luồng đọc; các ReadX/WriteX chỉ ủy quyền cho Reader/Writer.
// Nguồn Java: network/io/Message.java (đo trên JDK 17: bảo toàn ngữ nghĩa ByteArrayOutputStream/ByteArrayInputStream, Close không làm mất dữ liệu).
// Vì vậy Cleanup vẫn cho đọc/ghi và GetData sau session.sendMessage(msg); msg.cleanup() (587 vị trí gọi trong game); Dispose mới dọn tham chiếu luồng,
// còn Command vẫn đọc được; sai chế độ thay NPE Java bằng InvalidOperationException không kế thừa IOException.
// Lớp cố ý không triển khai IDisposable theo IMessage; ReadImage/WriteImage của Java không được port vì không có nơi gọi.
public class Message : IMessage
{
    private JavaDataOutput? _dos;
    private JavaDataInput? _dis;
    private bool _disposed;

    // Lệnh gói tin có dấu, tương ứng trường byte command công khai, ghi được của Java (ví dụ -27, -28) (Java: command)
    public sbyte Command { get; set; }

    // Tạo gói ghi, cắt command như ép byte Java: lấy 8 bit thấp có dấu (-218 thành 38, 200 thành -56, 256 thành 0)
    public Message(int command)
        : this(unchecked((sbyte)command))
    {
    }

    // Tạo gói ở chế độ ghi; GetData trả mảng rỗng thay vì null khi chưa ghi dữ liệu
    public Message(sbyte command)
    {
        Command = command;
        _dos = new JavaDataOutput();
    }

    // Tạo gói ở chế độ đọc trực tiếp trên data, không sao chép; data null ném ArgumentNullException như NPE khi dựng Java
    public Message(sbyte command, byte[] data)
    {
        Command = command;
        _dis = new JavaDataInput(data);
    }

    // Trả luồng ghi; gói đọc hoặc đã Dispose ném lỗi tương ứng (Java: writer)
    public JavaDataOutput Writer() => _dos ?? throw Unavailable("writer", isWriter: true);

    // Trả luồng đọc; gói ghi hoặc đã Dispose ném lỗi tương ứng (Java: reader)
    public JavaDataInput Reader() => _dis ?? throw Unavailable("reader", isWriter: false);

    // Trả toàn bộ byte đã ghi; gói đọc hoặc đã Dispose ném lỗi tương ứng (Java: getData)
    public byte[] GetData() => (_dos ?? throw Unavailable("writer (GetData)", isWriter: true)).ToArray();

    // Ghi int 4 byte từ 32 bit thấp khi is đúng, ngược lại ghi long 8 byte (Java: writeCris)
    public void WriteCris(long v, bool @is)
    {
        if (@is)
        {
            WriteInt(unchecked((int)v));
            return;
        }

        WriteLong(v);
    }

    // Ghi short 2 byte từ 16 bit thấp khi is đúng, ngược lại ghi long 8 byte (Java: writeShortToLong)
    public void WriteShortToLong(long v, bool @is)
    {
        if (@is)
        {
            WriteShort(unchecked((short)v));
            return;
        }

        WriteLong(v);
    }

    // Ghi byte 1 byte từ 8 bit thấp khi is đúng, ngược lại ghi long 8 byte (Java: writeByteToLong)
    public void WriteByteToLong(long v, bool @is)
    {
        if (@is)
        {
            WriteByte(unchecked((sbyte)v));
            return;
        }

        WriteLong(v);
    }

    // Đóng các luồng còn tham chiếu; đóng luồng bộ nhớ không ảnh hưởng dữ liệu hay việc đọc/ghi tiếp (Java: cleanup)
    public void Cleanup()
    {
        _dis?.Close();
        _dos?.Close();
    }

    // Dọn luồng rồi bỏ tham chiếu chúng; Command không đổi, các lần gọi Cleanup/Dispose sau vẫn không lỗi (Java: dispose)
    public void Dispose()
    {
        Cleanup();
        _disposed = true;
        _dis = null;
        _dos = null;
    }

    // Đọc byte kế tiếp hoặc -1 ở cuối luồng (Java: read)
    public int Read() => Reader().Read();

    // Đọc byte vào toàn bộ mảng đích (Java: read)
    public int Read(byte[] b) => Reader().Read(b);

    // Đọc tối đa len byte vào mảng đích từ vị trí off (Java: read)
    public int Read(byte[] b, int off, int len) => Reader().Read(b, off, len);

    // Đọc Boolean theo định dạng DataInput (Java: readBoolean)
    public bool ReadBoolean() => Reader().ReadBoolean();

    // Đọc byte có dấu (Java: readByte)
    public sbyte ReadByte() => Reader().ReadByte();

    // Đọc short big-endian có dấu (Java: readShort)
    public short ReadShort() => Reader().ReadShort();

    // Đọc int big-endian có dấu (Java: readInt)
    public int ReadInt() => Reader().ReadInt();

    // Đọc long big-endian có dấu (Java: readLong)
    public long ReadLong() => Reader().ReadLong();

    // Đọc float IEEE 754 big-endian (Java: readFloat)
    public float ReadFloat() => Reader().ReadFloat();

    // Đọc double IEEE 754 big-endian (Java: readDouble)
    public double ReadDouble() => Reader().ReadDouble();

    // Đọc ký tự UTF-16 từ hai byte big-endian (Java: readChar)
    public char ReadChar() => Reader().ReadChar();

    // Đọc chuỗi modified UTF-8 với tiền tố độ dài kiểu DataInput (Java: readUTF)
    public string ReadUTF() => Reader().ReadUTF();

    // Đọc đủ toàn bộ mảng đích hoặc ném lỗi khi thiếu dữ liệu (Java: readFully)
    public void ReadFully(byte[] b) => Reader().ReadFully(b);

    // Đọc đủ len byte vào mảng đích từ vị trí off hoặc ném lỗi khi thiếu dữ liệu (Java: readFully)
    public void ReadFully(byte[] b, int off, int len) => Reader().ReadFully(b, off, len);

    // Đọc byte không dấu thành int 0..255 (Java: readUnsignedByte)
    public int ReadUnsignedByte() => Reader().ReadUnsignedByte();

    // Đọc short không dấu big-endian thành int 0..65535 (Java: readUnsignedShort)
    public int ReadUnsignedShort() => Reader().ReadUnsignedShort();

    // Ghi toàn bộ mảng byte (Java: write)
    public void Write(byte[] b) => Writer().Write(b);

    // Ghi 8 bit thấp của int thành một byte (Java: write)
    public void Write(int b) => Writer().Write(b);

    // Ghi len byte của mảng từ vị trí off (Java: write)
    public void Write(byte[] b, int off, int len) => Writer().Write(b, off, len);

    // Ghi Boolean theo định dạng DataOutput (Java: writeBoolean)
    public void WriteBoolean(bool v) => Writer().WriteBoolean(v);

    // Ghi 8 bit thấp của int thành byte (Java: writeByte)
    public void WriteByte(int v) => Writer().WriteByte(v);

    // Ghi 8 bit thấp của từng ký tự chuỗi, không có tiền tố độ dài (Java: writeBytes)
    public void WriteBytes(string s) => Writer().WriteBytes(s);

    // Ghi 16 bit thấp của int thành ký tự UTF-16 big-endian (Java: writeChar)
    public void WriteChar(int v) => Writer().WriteChar(v);

    // Ghi từng ký tự UTF-16 big-endian, không có tiền tố độ dài (Java: writeChars)
    public void WriteChars(string s) => Writer().WriteChars(s);

    // Ghi double IEEE 754 big-endian (Java: writeDouble)
    public void WriteDouble(double v) => Writer().WriteDouble(v);

    // Ghi float IEEE 754 big-endian (Java: writeFloat)
    public void WriteFloat(float v) => Writer().WriteFloat(v);

    // Ghi int big-endian (Java: writeInt)
    public void WriteInt(int v) => Writer().WriteInt(v);

    // Ghi long big-endian (Java: writeLong)
    public void WriteLong(long v) => Writer().WriteLong(v);

    // Ghi 16 bit thấp của int thành short big-endian (Java: writeShort)
    public void WriteShort(int v) => Writer().WriteShort(v);

    // Ghi chuỗi modified UTF-8 với tiền tố độ dài kiểu DataOutput (Java: writeUTF)
    public void WriteUTF(string str) => Writer().WriteUTF(str);

    // Thay null/NPE của Java bằng ObjectDisposedException khi đã Dispose, còn lại là lỗi sai chế độ luồng
    private Exception Unavailable(string what, bool isWriter)
    {
        if (_disposed)
        {
            return new ObjectDisposedException(
                nameof(Message),
                $"Message has been disposed with Dispose(): {what} is no longer available (Java: returns null, using it would throw NullPointerException)");
        }

        return new InvalidOperationException(
            isWriter
                ? $"Message is in READ mode (created with Message(command, data)): there is no {what} (Java: null/NPE)"
                : $"Message is in WRITE mode (created with Message(command)): there is no {what} (Java: null/NPE)");
    }
}
