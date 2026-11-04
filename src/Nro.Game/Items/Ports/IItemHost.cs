using Nro.Core.IO;

namespace Nro.Game.Items.Ports;

// Định nghĩa các dịch vụ ngoài mà mã vật phẩm cần để gửi gói tin, túi đồ, hiệu ứng và trạng thái người chơi.
// Nguồn Java: Service, InventoryService, EffectSkillService và Player (lớp chính Java được thay bằng cổng phụ thuộc để mã vật phẩm kiểm thử bằng fake; mọi lệnh chạy trong tick của zone, không đồng thời trên một người chơi).
public interface IItemHost
{
    // Gửi gói tin trực tiếp cho người chơi (Java: player.sendMessage).
    void SendMessage(Player player, Message message);

    // Gửi lại chỉ số của người chơi (Java: Service.gI).point).
    void SendPoint(Player player);

    // Gửi một dòng thông báo thường (Java: Service.gI).sendThongBao).
    void SendNotification(Player player, string text);

    // Gửi thông báo mang danh quản trị viên (Java: Service.gI).sendThongBaoFromAdmin).
    void SendNotificationFromAdmin(Player player, string text);

    // Đồng bộ túi đồ xuống máy khách (Java: InventoryService.gI).sendItemBag).
    void SendItemBag(Player player);

    // Đếm số ô trống trong túi (Java: InventoryService.gI).getCountEmptyBag).
    int GetCountEmptyBag(Player player);

    // Thêm vật phẩm vào túi (Java: InventoryService.gI).addItemBag).
    void AddItemBag(Player player, Item item);

    // Trừ số lượng vật phẩm trong túi; item có thể null khi tìm không thấy (Java: InventoryService.gI).subQuantityItemsBag).
    void SubQuantityItemsBag(Player player, Item? item, int quantity);

    // Tìm vật phẩm túi theo mẫu, trả null nếu không có (Java: InventoryService.gI).findItem).
    Item? FindBagItem(Player player, int templateId);

    // Lấy các vật phẩm đang đặt trong cửa sổ ép sao (Java: player.combine.itemsCombine).
    IReadOnlyList<Item> GetCombineItems(Player player);

    // Kết thúc hiệu ứng Đuôi Khỉ (Java: EffectSkillService.gI).RemoveMonkeyTail).
    void RemoveMonkeyTail(Player player);

    // Tăng một bậc giới hạn sức mạnh, chặn tại NPoint.MAX_LIMIT rồi khởi tạo lại giới hạn (Java: player.nPoint.limitPower).
    void IncreasePowerLimit(Player player);

    // Lấy bộ đếm bang, hợp thể và kỹ năng hiệu ứng cho ItemTimeService gửi thời gian còn lại.
    ExternalItemTimes GetExternalItemTimes(Player player);
}
