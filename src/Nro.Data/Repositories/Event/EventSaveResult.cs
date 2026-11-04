using Nro.Data.Db;
namespace Nro.Data.Repositories.Event;

// Số dòng cập nhật bởi bốn lệnh lưu sự kiện Ngày Phụ nữ, Tết âm lịch, Giáng sinh và Hùng Vương.
// Nguồn Java: jbcd/dao/EventDAO.java (0 nghĩa là dòng sự kiện không tồn tại; Java chỉ ghi cảnh báo).
public sealed record EventSaveResult(int WomensDayRows, int LunaNewYearRows, int ChristmasRows, int HungVuongRows);
