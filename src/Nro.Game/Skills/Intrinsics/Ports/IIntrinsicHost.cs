using Nro.Core.IO;

namespace Nro.Game.Skills.Intrinsics.Ports;

// Dịch vụ trò chơi mà IntrinsicService cần để thao tác người chơi và gửi giao diện.
// Nguồn Java: IntrinsicService (thay các singleton Service/NpcService/PlayerService và dữ liệu Player bằng interface; gọi từ tick zone, test dùng fake).
public interface IIntrinsicHost
{
    // Đọc sức mạnh người chơi (Java: player.nPoint.power).
    long GetPower(Player player);

    // Đọc vàng người chơi (Java: player.inventory.gold).
    long GetGold(Player player);

    // Gán vàng người chơi (Java: player.inventory.gold).
    void SetGold(Player player, long gold);

    // Đồng bộ HP, MP và vàng lên client (Java: sendInfoHpMpMoney).
    void SendInfoHpMpMoney(Player player);

    // Gửi thông báo cho người chơi (Java: sendThongBao).
    void SendNotification(Player player, string text);

    // Gửi Message cho người chơi (Java: player.sendMessage).
    void SendMessage(Player player, Message message);

    // Tạo menu NPC Con Mèo với các lựa chọn (Java: createMenuConMeo).
    void CreateMenuConMeo(Player player, int indexMenu, int avatar, string text, params string[] select);
}
