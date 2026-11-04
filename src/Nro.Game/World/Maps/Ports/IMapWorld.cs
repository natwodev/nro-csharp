using Nro.Core.IO;
using Nro.Core.Templates;

namespace Nro.Game.World.Maps.Ports;

// Mô tả một NPC trong gói bản đồ.
// Nguồn Java: nro/map/Npc.java (các trường status, cx, cy, tempId, avartar).
public sealed record MapNpcInfo(int Status, int Cx, int Cy, int TempId, int Avartar);

// Khai báo dịch vụ mà mô hình map, zone và quái gọi ra ngoài.
// Nguồn Java: nro/map/Map.java và nro/map/Zone.java (lớp phụ thay Service.gI(), ItemMapService.gI(), TaskService.gI(), NpcManager, BossManager và TranhNgocService; không dùng singleton gI(), host Waves 3-4 triển khai, kiểm thử dùng fake ghi nhận).
// Mọi lời gọi xuất phát từ vòng lặp tick của zone sở hữu lời gọi.
public interface IMapWorld
{
    // Gửi thông điệp cho toàn bộ người chơi trong zone (Java: Service.sendMessAllPlayerInMap).
    void SendMessAllPlayerInMap(Zone zone, Message message);

    // Gửi thông điệp cho zone hiện tại của người chơi (Java: Service.sendMessAllPlayerInMap).
    void SendMessAllPlayerInMap(Player player, Message message);

    // Gửi thông điệp cho người khác trong map, không gồm người chơi nguồn (Java: Service.sendMessAnotherNotMeInMap).
    void SendMessAnotherNotMeInMap(Player player, Message message);

    // Gửi thông báo văn bản cho người chơi (Java: Service.sendThongBao).
    void SendNotification(Player player, string text);

    // Công bố vật phẩm rơi trong zone (Java: Service.dropItemMap).
    void DropItemMap(Zone zone, ItemMap itemMap);

    // Gửi cập nhật đại boss cho zone (Java: Service.sendBigBoss).
    void SendBigBoss(Zone zone, int action, int type, int playerId, long damage);

    // Gửi cập nhật đại boss theo quái cho zone (Java: Service.sendBigBoss2).
    void SendBigBoss2(Zone zone, int action, Mob mob);

    // Gửi cập nhật đại boss theo quái cho một người chơi (Java: Service.sendBigBoss2).
    void SendBigBoss2(Player player, int action, Mob mob);

    // Gửi thay đổi thân quái (Java: Service.Send_Body_Mob).
    void SendBodyMob(Mob mob, int type, int id);

    // Tra mẫu vật phẩm; trả null khi không biết id (Java: ItemService.getTemplate).
    ItemTemplate? GetItemTemplate(short id);

    // Kiểm tra vật phẩm là ngọc rồng đen (Java: ItemMapService.isBlackBall).
    bool IsBlackBall(int itemTemplateId);

    // Kiểm tra vật phẩm là ngọc Namek (Java: ItemMapService.isNamecBall).
    bool IsNamecBall(int itemTemplateId);

    // Kiểm tra vật phẩm là đá ngọc Namek (Java: ItemMapService.isNamecBallStone).
    bool IsNamecBallStone(int itemTemplateId);

    // Xóa vật phẩm khỏi zone của nó và báo client (Java: ItemMapService.removeItemMapAndSendClient).
    void RemoveItemMapAndSendClient(ItemMap item);

    // Gửi sự kiện vật phẩm biến mất (Java: ItemMapService.sendItemMapDisappear).
    void SendItemMapDisappear(ItemMap item);

    // Lấy NPC hiển thị cho người chơi (Java: NpcManager.getNpcsByMapPlayer).
    IReadOnlyList<MapNpcInfo> GetNpcsForPlayer(Player player);

    // Lấy id nhiệm vụ hiện tại (Java: TaskService.getIdTask).
    int GetTaskId(Player player);

    // Chạy hook nhiệm vụ/thành tựu sau khi giết quái theo thứ tự Java: TaskService (thường/phụ/bang), AchievementService, kolTask khi isPl, rồi checkDoneEventTaskKillMob.
    void OnMobKilled(Player killer, Mob mob);

    // Cộng sức mạnh/tiềm năng (Java: Service.addSMTN).
    void AddSMTN(Player player, sbyte type, long amount, bool isOri);

    // Cộng tiềm năng luyện tập (Java: TrainningService.tangTnsmLuyenTap).
    void AddTrainingPowerPotential(Player player, long potential);

    // Lấy cấp hiện tại của người chơi (Java: Service.getCurrLevel).
    int GetCurrLevel(Player player);

    // Tạo boss, gắn zoneFinal rồi cho vào zone (Java: BossManager.createBoss, zoneFinal, joinMapByZone).
    void CreateBossInZone(short bossId, Zone zone);

    // Lấy phần trăm sự kiện thuộc tính vàng còn hiệu lực; null khi không có hoặc đã hết hạn (Java: ServerManager.getAttributeManager().find(VANG)).
    double? GetActiveGoldAttributeValue();

    // Kiểm tra đang trong thời gian chiến tranh ngọc (Java: TranhNgoc.gI().isTimeStartWar).
    bool IsNamekWarTime();

    // Gửi kết thúc phó bản Tranh Ngọc (Java: TranhNgocService.sendEndPhoBan).
    void SendNamekWarEnd(Zone zone, int result, bool flag);

    // Trao thưởng Tranh Ngọc cho danh sách người chơi (Java: TranhNgocService.givePrice).
    void GiveNamekWarPrize(IReadOnlyList<Player> players, int result, int point);

    // Xử lý nhặt ngọc Tranh Ngọc (Java: TranhNgocService.pickBall).
    void PickNamekWarBall(Player player, ItemMap itemMap);

    // Đặt tọa độ người chơi (Java: Service.setPos).
    void SetPos(Player player, int x, int y);

    // Đăng ký từng zone Map tạo theo thứ tự chỉ số vào dịch vụ phó bản theo loại map (Java: BlackBallWarService.addMapBlackBallWar, RedRibbonHQService.addMapDoanhTrai, SuperDivineWater.addZone...).
    void RegisterZone(Map map, int zoneIndex, Zone zone);

    // Cập nhật số lượng nhiệm vụ huy hiệu (Java: BadgesTaskService.updateCountBagesTask).
    void UpdateBadgesTaskCount(Player player, int taskType, int count);

    // Hút vật phẩm rơi thẳng vào túi bằng bùa nam châm (Java: ItemMapService.pickItem với true).
    void PullItemToPlayer(Player player, int itemMapId);
}
