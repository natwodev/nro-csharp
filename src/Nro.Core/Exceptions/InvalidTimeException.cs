namespace Nro.Core.Exceptions;

// Lỗi thời gian không hợp lệ: chuỗi ngày giờ không khớp mẫu định dạng (TimeUtil.getTime, TimeUtil.isTimeNowInRangex)
// Nguồn Java: java.lang.Exception("Thời gian không hợp lệ"); tách thành lớp riêng để bắt đúng loại lỗi, giữ nguyên câu thông báo vì game có thể hiện cho người chơi
public sealed class InvalidTimeException : Exception
{
    // Câu thông báo gốc của Java
    public const string JavaMessage = "Thời gian không hợp lệ";

    // Tạo lỗi với câu thông báo gốc của Java
    public InvalidTimeException()
        : base(JavaMessage)
    {
    }

    // Tạo lỗi với câu thông báo tùy chọn
    public InvalidTimeException(string message)
        : base(message)
    {
    }

    // Tạo lỗi với câu thông báo tùy chọn và lỗi gốc bên trong
    public InvalidTimeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
