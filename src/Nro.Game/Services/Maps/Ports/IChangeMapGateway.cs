using Nro.Game.Players;
using Nro.Game.World.Maps;

namespace Nro.Game.Services.Maps.Ports;

// Cổng nhỏ cho các service chưa port mà ChangeMapService Java gọi; không che giấu thứ tự luật đổi zone/map.
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 66–454).
public interface IChangeMapGateway : IChangeMapPacketGateway
{
    // Lấy các đích Capsule, luôn làm mới cache Player như Java (Java: MapService.getMapCapsule).
    IReadOnlyList<Zone> GetCapsuleZones(Player player);

    // Lấy các đích chiến tranh ngọc đen (Java: MapService.getMapBlackBall).
    IReadOnlyList<Zone> GetBlackBallZones();

    // Lấy các đích Ma Bư (Java: MapService.getMapMaBu).
    IReadOnlyList<Zone> GetMajinBuuZones();

    // Kiểm tra map offline/phó bản/Ma Bư/Ma Bư 2 giờ (Java: MapService.isMap*).
    bool IsOfflineMap(int mapId);
    bool IsDungeonMap(int mapId);
    bool IsMajinBuuMap(int mapId);
    bool IsMajinBuuTwoHourMap(int mapId);

    // Kiểm tra map Cold/Tương Lai theo object map Java (Java: MapService.isMapCold/isMapTuongLai).
    bool IsColdMap(Map map);
    bool IsFutureMap(Map map);

    // Phân loại map của lõi ChangeMapService hiện hành (Java: isMapChristMasEvent/isMapCereal/isMap5000NamTruoc).
    bool IsChristmasEventMap(int mapId);
    bool IsCerealMap(Map map);
    bool Is5000YearsAgoMap(int mapId);

    // Chọn zone map có thể vào; MapService sẽ được port ở I-05 (Java: getMapCanJoin).
    Zone? GetJoinableZone(Player player, int mapId, int zoneId);

    // Áp luật map riêng/chặn/giới tính chưa thuộc I-03 (Java: checkMapCanJoin).
    Zone? CheckMapCanJoin(Player player, Zone? zone);

    // Áp luật dịch chuyển Yardrat chưa thuộc I-03 (Java: checkMapCanJoinByYardart).
    Zone? CheckYardratDestination(Player player, Zone? zone);

    // Hủy giao dịch đang dang dở (Java: TransactionService.cancelTrade).
    void CancelTrade(Player player);

    // Lấy zone Tranh Ngọc theo phe, đổi cờ và tạo phó bản (Java: getZoneTranhNgoc/changeFlag/sendCreatePhoBan).
    Zone? GetNamekWarZone(sbyte team);
    void ChangeNamekWarFlag(Player player, sbyte team);
    void SendCreateNamekWarInstance(Player player);

    // Đồng bộ vị trí ngọc Namek đang giữ (Java: NgocRongNamecService.map/name/zone/p/idpNrNamec).
    void UpdateNamekBallLocation(Player player, Zone destination);

    // Thả ngọc Namek khi rời vùng hợp lệ (Java: NgocRongNamecService.dropNamekBall).
    void DropNamekBall(Player player);

    // Kiểm tra map chứa ngọc Namek (Java: NgocRongNamecService.isMapNRNM).
    bool IsNamekBallMap(int mapId);

    // Đặt vị trí hợp lệ và phát cập nhật (Java: Service.resetPoint).
    void ResetPoint(Player player, int x, int y);

    // Dừng nạp năng lượng và gỡ trói (Java: EffectSkillService.stopCharge/removeUseTroi).
    void StopCharge(Player player);
    void RemoveBinding(Player player);

    // Hồi sinh và gửi HP/MP; giữ ngoài Game vì PlayerService/Service chưa port (Java: hsChar/sendInfoHpMp).
    void Revive(Player player, long hp, long mp);
    void SendInfoHpMp(Player player);

    // Thông báo thường/tutorial, cùng ranh giới packet/NPC service (Java: Service.sendThongBao, NpcService.createTutorial).
    void SendNotification(Player player, string text);
    void SendTutorial(Player player, string text);

    // Gửi danh sách khu cmd 29 (Java: openZoneUI); codec chi tiết thuộc packet adapter map.
    void SendZoneList(Player player, IReadOnlyList<Zone> zones);

    // Xóa dữ liệu map cũ trước map-info (Java: Service.clearMap).
    void ClearMap(Player player);

    // Đưa pet vào zone chủ sau khi chủ đã đi map (Java: pet.joinMapMaster).
    void JoinPetMap(Player player);

    // Gửi map-info -24 qua codec/adapter thế giới (Java: Zone.mapInfo; docs/PROTOCOL.md -24).
    void SendMapInfo(Player player, Zone zone);

    // Thông báo/đồng bộ đặc thù map sau khi vào map (Java: checkJoinSpecialMap/checkJoinMapMaBu).
    void CheckJoinSpecialMap(Player player);
    void CheckJoinMajinBuuMap(Player player);

    // Dọn điểm Ma Bư và trao vật phẩm vào map Ma Bư (Java: fightMabu.clear, InventoryService.addItemBag).
    void ClearMajinBuuFight(Player player);
    void GrantMajinBuuEntryReward(Player player, int minutes);

    // Đồng bộ điểm/chỉ số cơ sở sau đổi Cold/Tương Lai (Java: Service.point/Send_Info_NV).
    void SendPoint(Player player);
    void SendCharacterInfo(Player player);

    // Gọi Tàu Pảy Pảy khi vào map 47 ở nhiệm vụ tương ứng (Java: Service.CallTauPayPay).
    int GetTaskId(Player player);
    void CallTauPayPay(Player player);

    // Đồng bộ trạng thái nhấc ngọc Tranh Ngọc sau khi đổi map (Java: TranhNgocService.sendUpdateLift).
    void SendNamekWarLiftUpdate(Player player);

    // Tìm cửa đang đứng và kiểm tra hiệu ứng chuyển map khí gas (Java: MapService.getWaypointPlayerIn/shouldChangeMap).
    WayPoint? GetWaypointPlayerIn(Player player);
    bool ShouldChangeDestronGasMap(int fromMapId, int toMapId);
    void BeginDestronGasMapChange(Player player, Zone destination, int x, int y);
}
