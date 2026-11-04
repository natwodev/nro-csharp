namespace Nro.Core.IO;

// Lỗi dữ liệu UTF-8 kiểu Java: chuỗi mã hoá vượt quá 65.535 byte khi ghi, hoặc dữ liệu modified UTF-8 bị hỏng khi đọc
// Nguồn Java: java.io.UTFDataFormatException (kế thừa IOException như bản Java); câu thông báo giữ y như JDK 17, ví dụ "malformed input around byte 2"
public sealed class UtfDataFormatException : IOException
{
    // Tạo lỗi không kèm thông báo
    public UtfDataFormatException()
    {
    }

    // Tạo lỗi với câu thông báo
    public UtfDataFormatException(string message)
        : base(message)
    {
    }
}
