using Nro.Core.IO;

namespace Nro.Game.Progress.Achievements.Ports;

// Hợp đồng các dịch vụ game mà AchievementService cần, thay các singleton và trường Player của Java; mọi lệnh chạy trong tick zone.
// Nguồn Java: nro/achievement/AchievementService.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).






public interface IAchievementHost
{

    bool IsPl(Player player);


    long GetSessionPaidAmount(Player player);


    int GetCountEmptyBag(Player player);


    Item CreateNewItem(short itemTemplateId, int quantity);


    void AddItemBag(Player player, Item item);


    void SendItemBag(Player player);


    void AddRuby(Player player, int amount);


    void SendMoney(Player player);


    void SendNotification(Player player, string text);


    void SendMessage(Player player, Message message);
}
