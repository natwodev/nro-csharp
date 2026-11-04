using Nro.Core.IO;

namespace Nro.Game.World.Maps.Ports;

// Cổng duy nhất để mô hình map, zone và quái đọc hoặc đổi trạng thái Player mà Player shell Wave 2 chưa phơi bày, hay nằm trong dịch vụ.
// Nguồn Java: nro/map/Zone.java (lớp phụ thay Player.location, isDie, injured, sendMessage và các lời gọi liên quan; host H-01/J triển khai trên Player thật, kiểm thử dùng fake, mọi lời gọi thuộc tick zone theo D1).
public interface IMapPlayerGateway
{
    // Lấy tọa độ X người chơi (Java: player.location.x).
    int GetX(Player player);

    // Lấy tọa độ Y người chơi (Java: player.location.y).
    int GetY(Player player);

    // Kiểm tra người chơi đã chết (Java: player.isDie).
    bool IsDie(Player player);

    // Kiểm tra đối tượng là người chơi chính (Java: player.isPl).
    bool IsPl(Player player);

    // Cập nhật người chơi, NPC không tương tác hoặc hình nhân sát thương (Java: player.update).
    void UpdatePlayer(Player player);

    // Gây thương tích và trả sát thương thực nhận (Java: victim.injured).
    double Injured(Player victim, Player? attacker, double damage, bool piercing, bool isMobAttack);

    // Gửi gói tin cho người chơi (Java: player.sendMessage).
    void SendMessage(Player player, Message message);

    // Gửi gói -5 thông tin người chơi khác cùng gói cờ/danh hiệu/chân mệnh; thuộc tầng packet I-01 (Java: Zone.infoPlayer).
    void SendPlayerInfo(Player receiver, Player info);

    // Lấy id tàu vũ trụ đang đánh dấu (Java: player.iDMark.getIdSpaceShip).
    int GetIdSpaceShip(Player player);

    // Kiểm tra hai người cùng bang, với cả hai bang không null (Java: so sánh clan).
    bool SameClan(Player a, Player b);

    // Lấy id bang; trả -1 khi người chơi chưa có bang (Java: player.clan.id).
    int GetClanId(Player player);

    // Kiểm tra Player là NPC không tương tác (Java: instanceof NonInteractiveNPC).
    bool IsNonInteractiveNpc(Player player);

    // Kiểm tra Player là hình nhân thử sát thương (Java: instanceof TestDame).
    bool IsDamageTestPlayer(Player player);

    // Kiểm tra candidate là TrainningBoss đang tấn công owner (Java: instanceof TrainningBoss và playerAtt.equals).
    bool IsTrainingBossOf(Player candidate, Player owner);

    // Lấy snapshot trạng thái người chơi mà quái cần khi tấn công hoặc bị tấn công: bùa, vệ tinh, bang, cộng tấn công... (MobPlayerView).
    MobPlayerView GetMobPlayerView(Player player);

    // Tắt phòng thủ vệ tinh, chỉ gọi khi isPl và cờ đang bật (Java: pl.satellite.isDefend = false).
    void ClearSatelliteDefend(Player player);

    // Áp dụng một lần hiệu ứng vệ tinh 342 MP, 343 thông minh, 344 phòng thủ hoặc 345 HP cho người chơi trong tầm; chỉ gọi khi có vệ tinh và sở hữu vật phẩm hoặc cùng bang (Java: ItemMap.satelliteUpdate).
    void ApplySatelliteEffect(Player player, int itemTemplateId, long nowMs);

    // Kiểm tra người chơi có vệ tinh (Java: pl.satellite != null).
    bool HasSatellite(Player player);

    // Tính sức mạnh/tiềm năng nhận được (Java: pl.nPoint.calSucManhTiemNang).
    double CalculatePowerPotential(Player player, long potential);

    // Lấy dữ kiện người chơi phong ấn quái bằng Ma Phong Ba: điểm kỹ năng, HP tối đa, loại bình và vật phẩm mới; null khi playerSkill null trong Java.
    MafubaInfo? GetMafubaInfo(Player player);

    // Lấy các số mà new EggMob(player) đọc từ chủ sở hữu (EggMobSource).
    EggMobSource GetEggMobSource(Player player);
}

// Gom dữ kiện Ma Phong Ba: điểm kỹ năng, HP tối đa, loại bình và loại vật phẩm mới; dùng bởi IMapPlayerGateway.GetMafubaInfo.
// Nguồn Java: Player.playerSkill, nPoint.hpMax, effectSkill.typeBinh và newSkill.typeItem.
public sealed record MafubaInfo(int SkillPoint, long HpMax, int EffectTypeBinh, int NewSkillTypeItem);

// Gom dữ kiện EggMob: cấp kỹ năng 12 từ 1..7, HP tối đa, sát thương tấn công và trạng thái đủ bộ PikkoroDaimao bằng 5.
// Nguồn Java: Player.playerSkill, nPoint và setClothes; dùng bởi IMapPlayerGateway.GetEggMobSource.
public sealed record EggMobSource(int SkillLevel, long HpMax, double DamageAttack, int PikkoroDaimao);
