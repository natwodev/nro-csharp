namespace Nro.Game.Skills.Ports;

// Các dịch vụ NewSkill cần từ phần còn lại của trò chơi.
// Nguồn Java: nro/skill/NewSkill.java (SkillService singleton và dữ liệu Player được thay bằng interface nhỏ để tách module; kiểm thử dùng fake).
public interface ISkillSpecialHost
{
    // Kiểm tra hiệu ứng vật phẩm chiêu đặc biệt đang hoạt động (Java: itemTime.isUseNCD).
    bool IsUsingNcd(Player player);

    // Kiểm tra đây là người chơi thật, không phải đệ tử, boss hay bot (Java: isPl).
    bool IsPl(Player player);

    // Gửi cấp mới của kỹ năng đặc biệt cho client (Java: sendCurrLevelSpecial).
    void SendCurrLevelSpecial(Player player, Skill skill);

    // Chạy một bước của kỹ năng đặc biệt (Java: updateSkillSpecial).
    void UpdateSkillSpecial(Player player);
}
