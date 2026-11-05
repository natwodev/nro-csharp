namespace Nro.Game.Players.Ports;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 2480–2729).
// Cổng cho dịch vụ thế giới mà Player dùng khi di chuyển, kiểm tra map và xử lý phạt; không tạo Service singleton.
public interface IPlayerWorldGateway
{
    // Lấy tọa độ hiện tại của người chơi (Java: player.location.x, player.location.y).
    void GetPosition(Player player, out int x, out int y);

    // Gán tọa độ hiện tại của người chơi (Java: player.location.x, player.location.y).
    void SetPosition(Player player, int x, int y);

    // Phát gói di chuyển của người chơi trong map (Java: MapService.sendPlayerMove).
    void SendPlayerMove(Player player);

    // Lấy dữ kiện map hiện tại; false khi zone/map không còn hợp lệ (Java: player.zone.map).
    bool TryGetMapState(Player player, out int mapId, out int mapWidth, out int mapHeight);

    // Lấy mặt đất phía trên tại tọa độ map (Java: Map.yPhysicInTop).
    int GetYPhysicInTop(Player player, int x, int y);

    // Kiểm tra map chiến tranh Ngọc Rồng sao đen (Java: switch mapId 85–91).
    bool IsBlackBallWarMap(int mapId);

    // Kiểm tra người chơi đang đứng tại cửa chuyển map (Java: MapService.getWaypointPlayerIn).
    bool HasWaypointPlayerIn(Player player);

    // Đặt lại vị trí người chơi ở chiến tranh ngọc đen (Java: Service.resetPoint).
    void ResetBlackBallPosition(Player player, int x, int y);

    // Áp dụng mất một phần HP khi vượt biên chiến tranh ngọc đen (Java: nPoint.hp -= nPoint.hpMax / 10).
    void ApplyBlackBallBoundaryPenalty(Player player);

    // Gửi HP hiện tại sau phạt chiến tranh ngọc đen (Java: PlayerService.sendInfoHp).
    void SendInfoHp(Player player);

    // Dịch chuyển người chơi theo thay đổi map thông thường (Java: ChangeMapService.changeMap).
    void ChangeMap(Player player, int mapId, int zoneId, int x, int y);

    // Đưa người chơi về nhà không qua phi thuyền (Java: ChangeMapService.changeMapNonSpaceship).
    void ReturnPlayerHome(Player player, int mapId, int x, int y);

    // Kiểm tra map Ngũ Hành Sơn (Java: MapService.isMapNguHanhSon).
    bool IsNguHanhSonMap(int mapId);

    // Kiểm tra map Cereal (Java: MapService.isMapCereal).
    bool IsCerealMap(int mapId);

    // Kiểm tra map hành tinh ngục tù (Java: MapService.isMapHanhTinhNgucTu).
    bool IsPrisonPlanetMap(int mapId);

    // Kiểm tra lãnh địa bang hội (Java: MapService.islanhdiabanghoi).
    bool IsClanTerritoryMap(int mapId);

    // Kiểm tra map tương lai (Java: MapService.isMapTuongLai).
    bool IsFutureMap(int mapId);

    // Kiểm tra map lạnh (Java: MapService.isMapCold).
    bool IsColdMap(int mapId);

    // Kiểm tra map khí gas hủy diệt (Java: MapService.isMapKhiGasHuyDiet).
    bool IsDestronGasMap(int mapId);

    // Lấy id nhiệm vụ hiện tại để kiểm tra cổng map (Java: TaskService.getIdTask).
    int GetTaskId(Player player);

    // Tịch thu vàng và ruby, đặt số dư không đủ về 0 (Java: inventory.subGold, inventory.subRuby).
    void ConfiscateCurrency(Player player, long gold, int ruby);

    // Gửi tiền hiện tại cho client sau phạt thanh toán (Java: Service.sendMoney).
    void SendMoney(Player player);

    // Gửi HP, MP và tiền hiện tại cho client sau phạt map (Java: PlayerService.sendInfoHpMpMoney).
    void SendInfoHpMpMoney(Player player);

    // Gửi thông báo thường cho người chơi (Java: Service.sendThongBao).
    void SendNotification(Player player, string text);

    // Gửi thông báo lớn cho người chơi (Java: Service.sendBigMessage).
    void SendBigMessage(Player player, int messageId, string text);

    // Phát tín hiệu khóa tài khoản tại boundary host, không truy cập DB (Java: PlayerService.KhoaTaiKhoan).
    void BanAccount(Player player);

    // Đồng bộ snapshot sức mạnh và diện mạo thành viên bang (Java: clanMember.powerPoint/head/body/leg).
    void UpdateClanMemberSnapshot(Player player, long power, short head, short body, short leg);

    // Gọi Sai Ba Men khi bom được phép kích hoạt (Java: Service.CallSaiBaMen).
    void CallSaiBaMen(Player player);
}
