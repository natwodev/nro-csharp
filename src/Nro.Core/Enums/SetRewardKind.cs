namespace Nro.Core.Enums;

// Phân loại phần thưởng bộ để chọn tạo trang bị Thần Linh hoặc Hủy Diệt.
// Nguồn Java: ItemService.settl* và sethd* (kiểu dữ liệu phụ trợ C# của lớp ItemService chính).
public enum SetRewardKind
{
    // Trang bị Thần Linh từ các phương thức Java settl* (Java: settl*).
    ThanLinh,

    // Trang bị Hủy Diệt từ các phương thức Java sethd* (Java: sethd*).
    HuyDiet,
}
