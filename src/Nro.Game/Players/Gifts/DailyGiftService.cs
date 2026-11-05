namespace Nro.Game.Players.Gifts;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/DailyGift/DailyGiftService.java (dòng 5–35).
public static class DailyGiftService
{
    // Kiểm tra phần quà có mã cho trước còn chưa nhận trong ngày (Java: checkDailyGift).
    public static bool CheckDailyGift(Player player, sbyte id)
    {
        ArgumentNullException.ThrowIfNull(player);
        return player.DailyGiftData.Any(data => data.Id == id && !data.Received);
    }

    // Đánh dấu lần xuất hiện đầu tiên chưa nhận của phần quà là đã nhận (Java: updateDailyGift).
    public static void UpdateDailyGift(Player player, sbyte id)
    {
        ArgumentNullException.ThrowIfNull(player);
        DailyGiftData? data = player.DailyGiftData.FirstOrDefault(data => data.Id == id && !data.Received);
        if (data is not null)
        {
            data.Received = true;
        }
    }

    // Xóa và tạo lại ba phần quà ngày mới theo mã 0, 1, 2 (Java: addAndReset).
    public static void AddAndReset(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        player.DailyGiftData.Clear();

        for (sbyte id = 0; id < 3; id++)
        {
            player.DailyGiftData.Add(new DailyGiftData { Id = id, Received = false });
        }
    }
}
