namespace Nro.Core.IO;

// Gói tin mạng giữa server và client: đọc/ghi các kiểu dữ liệu theo định dạng DataInputStream/DataOutputStream của Java
// Nguồn Java: network/interfaces/IMessage.java (35 hàm, cùng thứ tự khai báo với bản Java)
// Đã bỏ readImage() và writeImage(BufferedImage, String): không chỗ nào trong mã nguồn gọi tới và .NET không có BufferedImage;
// nếu cần gửi ảnh thì ghi byte[] (WriteInt(độ dài) rồi Write(bytes)), khớp định dạng gửi đi của Java.
// Đổi kiểu (01-ANALYSIS-MAPPING mục 3.3): byte có dấu -> sbyte, boolean -> bool, String -> string, DataOutputStream/DataInputStream -> JavaDataOutput/JavaDataInput.
// Cố ý KHÔNG kế thừa IDisposable: Dispose() ở đây là dispose() của Java (bỏ tham chiếu tới stream, sau đó Writer()/GetData() ném lỗi),
// nên dùng using sẽ vô tình làm hỏng gói tin vẫn còn nằm trong hàng đợi gửi.
public interface IMessage
{
    // Đọc một byte không dấu (0-255), trả -1 khi hết dữ liệu
    int Read();

    // Đọc tối đa b.Length byte vào mảng, trả số byte đã đọc hoặc -1 khi hết dữ liệu
    int Read(byte[] b);

    // Đọc tối đa len byte vào mảng từ vị trí off, trả số byte đã đọc hoặc -1 khi hết dữ liệu
    int Read(byte[] b, int off, int len);

    // Đọc một giá trị đúng/sai (1 byte)
    bool ReadBoolean();

    // Đọc một byte có dấu
    sbyte ReadByte();

    // Đọc số nguyên 2 byte có dấu
    short ReadShort();

    // Đọc số nguyên 4 byte
    int ReadInt();

    // Đọc số nguyên 8 byte
    long ReadLong();

    // Đọc số thực 4 byte
    float ReadFloat();

    // Đọc số thực 8 byte
    double ReadDouble();

    // Đọc một ký tự 2 byte
    char ReadChar();

    // Đọc chuỗi modified UTF-8 kèm độ dài 2 byte phía trước
    string ReadUTF();

    // Đọc đủ b.Length byte, ném lỗi nếu hết dữ liệu giữa chừng
    void ReadFully(byte[] b);

    // Đọc đủ len byte vào mảng từ vị trí off, ném lỗi nếu hết dữ liệu giữa chừng
    void ReadFully(byte[] b, int off, int len);

    // Đọc một byte không dấu (0-255)
    int ReadUnsignedByte();

    // Đọc số nguyên 2 byte không dấu (0-65535)
    int ReadUnsignedShort();

    // Ghi toàn bộ mảng byte
    void Write(byte[] b);

    // Ghi byte thấp nhất của b
    void Write(int b);

    // Ghi len byte của mảng từ vị trí off
    void Write(byte[] b, int off, int len);

    // Ghi một giá trị đúng/sai (1 byte)
    void WriteBoolean(bool v);

    // Ghi byte thấp nhất của v
    void WriteByte(int v);

    // Ghi chuỗi, mỗi ký tự chỉ giữ byte thấp
    void WriteBytes(string s);

    // Ghi một ký tự 2 byte
    void WriteChar(int v);

    // Ghi chuỗi, mỗi ký tự 2 byte
    void WriteChars(string s);

    // Ghi số thực 8 byte
    void WriteDouble(double v);

    // Ghi số thực 4 byte
    void WriteFloat(float v);

    // Ghi số nguyên 4 byte
    void WriteInt(int v);

    // Ghi số nguyên 8 byte
    void WriteLong(long v);

    // Ghi 2 byte thấp của v
    void WriteShort(int v);

    // Ghi chuỗi modified UTF-8 kèm độ dài 2 byte phía trước
    void WriteUTF(string str);

    // Stream ghi (Java: DataOutputStream); ném lỗi nếu gói tin đang ở chế độ đọc hoặc đã Dispose
    JavaDataOutput Writer();

    // Stream đọc (Java: DataInputStream); ném lỗi nếu gói tin đang ở chế độ ghi hoặc đã Dispose
    JavaDataInput Reader();

    // BẢN SAO các byte đã ghi (mỗi lần gọi là một mảng mới); ném lỗi nếu đang ở chế độ đọc hoặc đã Dispose
    byte[] GetData();

    // Đóng các stream; với stream trong bộ nhớ thì việc đóng KHÔNG làm gì, dữ liệu và các lần đọc/ghi sau vẫn còn nguyên
    void Cleanup();

    // Cleanup rồi bỏ tham chiếu tới stream: sau đó mọi thao tác đọc/ghi, Writer(), Reader() và GetData() đều ném lỗi
    void Dispose();
}
