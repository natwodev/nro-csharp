namespace Nro.Game.Effects.Ports;

// Gửi các lệnh thông báo client hoặc làm mới người chơi mà hiệu ứng gọi qua Service, PlayerService, ItemTimeService và InventoryService.
// Nguồn Java: services/Service.java, PlayerService.java, ItemTimeService.java và InventoryService.java (cổng C# cho các lớp track I/D chưa viết; mỗi phương thức là lệnh không trả kết quả, triển khai quyết định gói tin tương ứng).
public interface IEffectClientNotifier
{
    // Gửi chỉ số đã tính lại cho client (Java: Service.point).
    void Point(Player player);

    // Gửi trang phục ra zone (Java: Service.Send_Caitrang).
    void SendCostume(Player player);

    // Gửi thông tin nhiệm vụ người chơi (Java: Service.Send_Info_NV).
    void SendInfoNv(Player player);

    // Gửi thông tin HP (Java: PlayerService.sendInfoHp).
    void SendInfoHp(Player player);

    // Gửi thông tin HP và KI (Java: PlayerService.sendInfoHpMp).
    void SendInfoHpMp(Player player);

    // Gửi thông tin HP, KI và tiền (Java: PlayerService.sendInfoHpMpMoney).
    void SendInfoHpMpMoney(Player player);

    // Hiển thị bong bóng thoại trên người chơi (Java: Service.chat).
    void Chat(Player player, string text);

    // Gửi thông báo cho người chơi (Java: Service.sendThongBao).
    void SendNotification(Player player, string text);

    // Gửi tốc độ người chơi (Java: Service.sendSpeedPlayer).
    void SendSpeedPlayer(Player player, int speed);

    // Gửi trạng thái kết thúc biến khỉ (Java: Service.setNotMonkey).
    void SetNotMonkey(Player player);

    // Gửi trạng thái kết thúc biến hình, giữ nguyên lỗi chính tả Java (Java: Service.setNotTranformation).
    void SetNotTranformation(Player player);

    // Gửi trạng thái kết thúc tiến hóa, giữ nguyên lỗi chính tả Java (Java: Service.setNotVolution).
    void SetNotVolution(Player player);

    // Gửi trạng thái ăn đậu (Java: Service.sendInfoPlayerEatPea).
    void SendInfoPlayerEatPea(Player player);

    // Đặt vị trí người chơi (Java: Service.setPos).
    void SetPos(Player player, int x, int y);

    // Đặt vị trí người chơi theo gói thứ hai (Java: Service.setPos2).
    void SetPos2(Player player, int x, int y);

    // Gửi trạng thái Chibi (Java: Service.sendChibi).
    void SendChibi(Player player);

    // Gửi trạng thái có Chibi theo sau đến toàn bản đồ (Java: Service.sendHaveChibiFollowToAllMap).
    void SendHaveChibiFollowToAllMap(Player player);

    // Gửi trạng thái Mabu giữ với hành động và tọa độ (Java: Service.sendMabuHold).
    void SendMabuHold(Player player, int action, short x, short y);

    // Cập nhật diện mạo người chơi cho người nhận (Java: Service.playerInfoUpdate).
    void PlayerInfoUpdate(Player player, Player receiver, string name, int head, int body, int leg);

    // Đổi và gửi kiểu PK (Java: PlayerService.changeAndSendTypePK).
    void ChangeAndSendTypePk(Player player, int type);

    // Giải hồi chiêu kỹ năng; skill null khi người chơi không có kỹ năng đó như Java đã truyền null (Java: Service.releaseCooldownSkill).
    void ReleaseCooldownSkill(Player player, EffectSkillRef? skill);

    // Hồi HP và KI cho người chơi (Java: PlayerService.hoiPhuc) (Java: hoiPhuc).
    void RecoverHpMp(Player player, long hp, long mp);

    // Gửi thời gian vật phẩm; 0 giây xóa biểu tượng (Java: ItemTimeService.sendItemTime).
    void SendItemTime(Player player, int itemId, int seconds);

    // Xóa thời gian vật phẩm (Java: ItemTimeService.removeItemTime).
    void RemoveItemTime(Player player, int itemId);

    // Gửi toàn bộ thời gian vật phẩm (Java: ItemTimeService.sendAllItemTime).
    void SendAllItemTime(Player player);

    // Gửi vật phẩm đang trang bị (Java: InventoryService.sendItemBody).
    void SendItemBody(Player player);

    // Gửi vật phẩm trong túi (Java: InventoryService.sendItemBag).
    void SendItemBag(Player player);
}
