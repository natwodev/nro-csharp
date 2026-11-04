namespace Nro.Game.Progress.Tasks.Ports;

// Hợp đồng dịch vụ túi đồ và thông báo để nộp KOL; được gọi trong tick của zone người chơi.
// Nguồn Java: nro/task/KolTask.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).





public interface IKolTaskHost : IKolTaskItemFactory
{

    void SendNotification(Player player, string text);


    int GetCountEmptyBag(Player player);


    void AddItemBag(Player player, Item item);


    void SendItemBag(Player player);
}
