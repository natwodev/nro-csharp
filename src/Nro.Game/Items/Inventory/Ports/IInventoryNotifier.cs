using Nro.Core.IO;

namespace Nro.Game.Items.Inventory.Ports;

// Gom các thông báo và làm mới phía khách mà InventoryService gọi qua Service, PlayerService hoặc player.sendMessage.
// Nguồn Java: nro/inventory/InventoryService.java (interface phụ của InventoryService; gói kho -35/-36/-37 do InventoryService tự dựng rồi chuyển qua SendMessage; chạy trong tick zone, không được chặn).
public interface IInventoryNotifier
{
    // Xếp gói cho session người chơi; không làm gì nếu người chơi chưa có session. (Java: sendMessage)
    void SendMessage(Player player, Message message);

    // Gửi thông báo một dòng lệnh -25. (Java: sendThongBao)
    void SendNotification(Player player, string text);

    // Gửi thông báo có nút OK. (Java: sendThongBaoOK)
    void SendNotificationOk(Player player, string text);

    // Gửi cửa sổ thông điệp lớn của quản trị. (Java: sendThongBaoFromAdmin)
    void SendAdminNotification(Player player, string text);

    // Đồng bộ vàng, ngọc và ruby cho khách. (Java: sendMoney)
    void SendMoney(Player player);

    // Tính lại và gửi chỉ số nhân vật. (Java: point)
    void Point(Player player);

    // Phát ngoại hình cải trang của người chơi. (Java: sendCaitrang)
    void SendCostume(Player player);

    // Đồng bộ túi cờ. (Java: sendFlagBag)
    void SendFlagBag(Player player);

    // Gửi hiệu ứng trang bị Chân thiên tử, loại vật phẩm 39. (Java: sendChanMenh)
    void SendTrueSonOfHeaven(Player player, int itemTemplateId);

    // Gỡ danh hiệu hiện có. (Java: removeDanhHieu)
    void RemoveTitle(Player player);

    // Gỡ hiệu ứng theo mã. (Java: removeEffPlayer)
    void RemovePlayerEffect(Player player, int idEff);

    // Hiển thị lại thông tin đệ tử. (Java: showInfoPet)
    void ShowInfoPet(Player player);

    // Cộng tiềm năng theo loại và lượng. (Java: congTiemNang)
    void AddPotential(Player player, sbyte type, long amount);

    // Đồng bộ HP và MP. (Java: sendInfoHpMp)
    void SendInfoHpMp(Player player);
}
