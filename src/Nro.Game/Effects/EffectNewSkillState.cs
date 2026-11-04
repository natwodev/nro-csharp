using Nro.Game.Effects.Ports;

namespace Nro.Game.Effects;

// Chứa bản sao chỉ đọc dữ liệu NewSkill mà Ma Phong Ba dùng: biến thể kỹ năng, hướng nhìn và các mục tiêu bị bắt; các danh sách là snapshot nên mã hiệu ứng có thể sửa danh sách gốc.
// Nguồn Java: nro/skill/NewSkill.java (DTO phụ trợ; lớp chính do track khác sở hữu).
public sealed record EffectNewSkillState
{
    // Kiểu Ma Phong Ba: 0 là thường, giá trị khác là phiên bản vật phẩm (Java: typeItem).
    public int TypeItem { get; init; }

    // Hướng nhìn: -1 là trái, giá trị khác là phải; Java dùng byte (Java: dir).
    public int Dir { get; init; }

    // Danh sách snapshot người chơi bị bắt (Java: playersTaget).
    public IReadOnlyList<Player> PlayersTarget { get; init; } = [];

    // Danh sách snapshot quái bị bắt (Java: mobsTaget).
    public IReadOnlyList<IEffectMobTarget> MobsTarget { get; init; } = [];
}
