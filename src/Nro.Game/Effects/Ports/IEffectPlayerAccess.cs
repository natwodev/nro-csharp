namespace Nro.Game.Effects.Ports;

// Đọc và ghi phần mô hình người chơi mà hiệu ứng cần, qua adapter H-01 cho các lớp do track khác sở hữu hoặc chưa viết.
// Nguồn Java: nro/player/Player.java, NPoint.java, Location.java, SetClothes.java, NewSkill.java, PlayerSkill.java, ItemTime.java, Zone.java và MaBuHold.java (cổng C#; danh sách phải là bản sao để lặp khi game sửa danh sách, bốn truy vấn zone ném InvalidOperationException khi không có zone như NullPointerException Java được caller log, mọi gọi chạy trong tick zone theo D1 nên không khóa).
public interface IEffectPlayerAccess
{
    // Kiểm tra người chơi đã chết, HP không quá 0 (Java: player.isDie).
    bool IsDie(Player player);

    // Kiểm tra đây là người chơi thật do người điều khiển (Java: player.isPl).
    bool IsPl(Player player);

    // Kiểm tra người chơi đang có zone (Java: player.zone != null).
    bool HasZone(Player player);

    // Lấy bản chụp tọa độ X, Y của người chơi (Java: player.location.x/y).
    EffectLocation GetLocation(Player player);

    // Lấy id bản đồ của zone người chơi (Java: player.zone.map.mapId).
    int GetMapId(Player player);

    // Lấy bản sao người chơi không phải boss trong zone (Java: player.zone.getNotBosses).
    IReadOnlyList<Player> GetNotBosses(Player player);

    // Lấy bản sao người chơi trong zone (Java: player.zone.getPlayers).
    IReadOnlyList<Player> GetPlayersInZone(Player player);

    // Lấy bản sao quái trong zone (Java: player.zone.mobs).
    IReadOnlyList<IEffectMobTarget> GetMobs(Player player);

    // Lấy bản chụp các trường NPoint hiệu ứng đọc tại thời điểm này.
    EffectNPointState GetNPoint(Player player);

    // Lấy bản chụp các trường SetClothes hiệu ứng đọc tại thời điểm này.
    EffectSetClothesState GetSetClothes(Player player);

    // Lấy bản chụp trường NewSkill mà hiệu ứng Ma Phong Ba đọc tại thời điểm này.
    EffectNewSkillState GetNewSkill(Player player);

    // Tìm kỹ năng theo id template, null nếu người chơi không có (Java: SkillUtil.getSkillbyId).
    EffectSkillRef? GetSkillById(Player player, int skillTemplateId);

    // Lấy kỹ năng được chọn, null nếu chưa chọn (Java: player.playerSkill.skillSelect).
    EffectSkillRef? GetSelectedSkill(Player player);

    // Kiểm tra thời gian vật phẩm khẩu trang (Java: player.itemTime.IsKhauTrang).
    bool HasFaceMaskItemTime(Player player);

    // Tính lại chỉ số người chơi (Java: player.nPoint.calPoint).
    void CalPoint(Player player);

    // Đặt HP, giá trị âm trở thành 0 (Java: player.nPoint.setHp).
    void SetHp(Player player, long hp);

    // Chia nguyên HP cho 2, không giới hạn kết quả (Java: player.nPoint.hp /= 2).
    void HalveHp(Player player);

    // Cộng HP cho người chơi (Java: player.nPoint.addHp).
    void AddHp(Player player, long hp);

    // Cộng KI cho người chơi (Java: player.nPoint.addMp).
    void AddMp(Player player, long mp);

    // Bắt đầu thời gian Đuôi Khỉ và bật cờ tương ứng (Java: player.itemTime.MonkeyTailStartTime/HasMonkeyTail).
    void StartDuoiKhi(Player player, long timeDuoiKhi);

    // Tắt cờ Đuôi Khỉ (Java: player.itemTime.HasMonkeyTail).
    void StopDuoiKhi(Player player);

    // Lưu cờ biến hình; không được ném lỗi vì Java chỉ log rồi tiếp tục (Java: PlayerDAO.saveisBienHinh).
    void SaveIsBienHinh(Player player);

    // Làm nạn nhân bị thương; hiệu ứng không dùng kết quả trả về của Java (Java: victim.injured).
    void Injure(Player victim, Player? attacker, double damage, bool piercing, bool isMobAttack);

    // Lấy tọa độ MaBuHold đã ép về short như Java (Java: maBuHold.x/y).
    (short X, short Y) GetMabuHoldPosition(MaBuHold hold);
}
