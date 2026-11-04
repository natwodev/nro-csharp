namespace Nro.Game.Progress.Badges;

// Dịch vụ xử lý điều kiện và thông báo huy hiệu, thay singleton Java bằng phụ thuộc được truyền vào.
// Nguồn Java: nro/badges/BadgesService.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).




public static class BadgesService
{

    // Bật huy hiệu được chọn cho người chơi.
    public static void TurnOnBadges(Player player, int id)
    {
        ArgumentNullException.ThrowIfNull(player);
        foreach (BadgesData data in player.DataBadges)
        {
            data.IsUse = data.IdBadGes == id;
        }
    }
}
