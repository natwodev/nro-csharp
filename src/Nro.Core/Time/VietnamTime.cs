namespace Nro.Core.Time;

// Múi giờ server Java chạy: Asia/Ho_Chi_Minh, mô hình hóa thành độ lệch CỐ ĐỊNH +07:00 thay vì tra theo tên
// Nguồn Java: múi giờ mặc định của JVM (Asia/Ho_Chi_Minh), không có lớp Java riêng
// Việt Nam giữ cố định UTC+7 (không có giờ mùa hè) từ 1975-06-13, nên mọi phép tính ngày không phụ thuộc múi giờ của máy,
// cơ sở dữ liệu tz đã cài (image Docker gọn có thể không có) hay InvariantGlobalization.
// KHÁC JAVA CÓ CHỦ ĐÍCH: các thời điểm trước 1975-06-13 (lúc Java sẽ dùng +08:00 hoặc +07:00 theo luật lịch sử) cũng hiển thị ở +07:00.
public static class VietnamTime
{
    // Độ lệch UTC cố định: +07:00
    public static readonly TimeSpan Offset = TimeSpan.FromHours(7);

    // Độ lệch tính bằng mili giây
    public const long OffsetMilliseconds = 7L * 60 * 60 * 1000;

    // TimeZoneInfo có độ lệch cố định +07:00 (id và tên hiển thị là Asia/Ho_Chi_Minh) (Java: TimeZone.getTimeZone("Asia/Ho_Chi_Minh"))
    public static readonly TimeZoneInfo Zone = TimeZoneInfo.CreateCustomTimeZone("Asia/Ho_Chi_Minh", Offset, "Asia/Ho_Chi_Minh", "Indochina Time");
}
