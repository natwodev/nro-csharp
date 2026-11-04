namespace Nro.Game.Items.Rewards;

// Phân loại phần thưởng bộ để chọn tạo trang bị Thần Linh hoặc Hủy Diệt.
// Nguồn Java: ItemService.settl* và sethd* (kiểu dữ liệu phụ trợ C# của lớp ItemService chính).
public enum SetRewardKind
{
    // Divine (Than Linh) gear, Java settl* methods. (Java: settl*).
    ThanLinh,

    // Destruction (Huy Diet) gear, Java sethd* methods. (Java: sethd*).
    HuyDiet,
}
