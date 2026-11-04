namespace Nro.Game.World.Mobs;

// Ảnh chụp chỉ đọc của player cho một hành động mob, thay các chuỗi Charms, Satellite, NPoint và Clan chưa hoàn chỉnh ở Wave 2; thời gian là epoch mili-giây và còn hiệu lực khi lớn hơn hiện tại.
// Nguồn Java: player.charms, player.clan, player.nPoint và các thuộc tính Player (các giá trị mặc định nghĩa là không có hiệu ứng đặc biệt).
public sealed record MobPlayerView
{
    // Cho biết player đã chết.
    public bool IsDie { get; init; }

    // Tọa độ ngang của player.
    public int X { get; init; }

    // Tọa độ dọc của player.
    public int Y { get; init; }

    // HP hiện tại của player.
    public long Hp { get; init; }

    // HP tối đa của player.
    public long HpMax { get; init; }

    // Vệ tinh đang che chắn cho player. (Java: pl.satellite != null && pl.satellite.isDefend)
    public bool SatelliteDefend { get; init; }

    // Player đang vô hình. (Java: pl.effectSkin != null && pl.effectSkin.isVoHinh)
    public bool IsInvisible { get; init; }

    // Mốc hết hiệu lực bùa Đá Trâu. (Java: charms.tdDaTrau)
    public long TdDaTrau { get; init; }

    // Mốc hết hiệu lực bùa Oai Hùng. (Java: charms.tdOaiHung)
    public long TdOaiHung { get; init; }

    // Mốc hết hiệu lực bùa Thu Hút của người nhặt; với đệ tử/thú là chủ ở ThuHutOwner. (Java: charms.tdThuHut)
    public long TdThuHut { get; init; }

    // Người quyết định hút vật phẩm: chính player hoặc chủ của đệ tử/thú; null khi không xác định được chủ.
    public Player? ThuHutOwner { get; init; }

    // Mốc hết hiệu lực bùa Đệ Tử của chủ; 0 nếu không phải đệ tử hoặc thiếu chủ. (Java: ((Detu) player).master.charms.tdDeTu)
    public long MasterPetCharmTraining { get; init; }

    // Mốc bùa Đá Trâu của bang; 0 khi không có bang. (Java: player.clan != null && clan.BuaDaTrau)
    public long ClanBuaDaTrau { get; init; }

    // Cấp bang của player. (Java: player.clan.level)
    public int ClanLevel { get; init; }

    // Cho biết đang có trạng thái Commeson. (Java: player.itemTime.iscommenson)
    public bool IsCommenson { get; init; }

    // Phần trăm phản sát thương. (Java: nPoint.tlPST)
    public int ReflectDamagePercent { get; init; }

    // Phần trăm sát thương thêm lên mob bay. (Java: nPoint.tlDameMobFly)
    public int FlyingMobDamagePercent { get; init; }

    // Phần trăm sát thương thêm lên mob khỉ. (Java: nPoint.tlDameMobMonkey)
    public int MonkeyMobDamagePercent { get; init; }

    // Phần trăm sát thương thêm lên mob chạy. (Java: nPoint.tlDameMobRun)
    public int RunningMobDamagePercent { get; init; }

    // Cho biết đòn hiện tại chí mạng. (Java: nPoint.isCrit)
    public bool IsCritical { get; init; }

    // Mã kỹ năng đang chọn; -1 khi chưa chọn kỹ năng. (Java: playerSkill.skillSelect.template.id)
    public int SelectedSkillId { get; init; } = -1;
}
