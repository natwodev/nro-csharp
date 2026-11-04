
namespace Nro.Game.Progress.Badges;

// Dịch vụ cập nhật tiến độ các điều kiện huy hiệu.
// Nguồn Java: nro/badges/BadgesTaskService.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).






public static class BadgesTaskService
{
    private const long MillisecondsPerDay = 24 * 60 * 60 * 1000L;
    private const int RewardBadgeDays = 30;


    // Tạo hoặc đặt lại các điều kiện nhiệm vụ huy hiệu.
    public static void CreateAndResetTask(Player player, IBadgesCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(catalog);
        player.DataTaskBadges.Clear();
        foreach (BadgesTaskTemplate btt in catalog.TaskTemplates)
        {
            var data = new BadgesTask
            {
                Id = btt.Id,
                Count = 0,
                CountMax = btt.Count,
                IdBadgesReward = btt.IdbadgesReward,
            };
            player.DataTaskBadges.Add(data);
        }
    }





    // Cập nhật nhiệm vụ huy hiệu theo thời gian hiện tại.
    public static void UpdateDoneTask(Player player, long nowMs)
    {
        ArgumentNullException.ThrowIfNull(player);
        foreach (BadgesTask data in player.DataTaskBadges)
        {
            if (data.IsDone())
            {
                foreach (BadgesData bg in player.DataBadges)
                {
                    if (bg.IdBadGes == data.IdBadgesReward)
                    {
                        return;
                    }
                }

                var danhHieu = new BadgesData(player, data.IdBadgesReward, RewardBadgeDays, nowMs);
                player.DataBadges.Add(danhHieu);
                data.Count = 0;
            }
        }
    }


    // Cộng tiến độ nhiệm vụ huy hiệu theo mã điều kiện.
    public static void UpdateCountBagesTask(Player player, int id, int amount)
    {
        ArgumentNullException.ThrowIfNull(player);
        foreach (BadgesTask data in player.DataTaskBadges)
        {
            if (data.Id == id)
            {
                data.Count = unchecked(data.Count + amount);
                if (data.Count > data.CountMax)
                {
                    data.Count = data.CountMax;
                }

                break;
            }
        }
    }


    // Tính phần trăm nhiệm vụ cho phần thưởng huy hiệu.
    public static int SendPercenBadgesTask(Player player, int idBadgesReward)
    {
        ArgumentNullException.ThrowIfNull(player);
        foreach (BadgesTask data in player.DataTaskBadges)
        {
            if (data.IdBadgesReward == idBadgesReward)
            {
                if (data.GetPercentProcess() > 0)
                {
                    return data.GetPercentProcess();
                }

                return 0;
            }
        }

        return 0;
    }


    // Tính số ngày còn lại của huy hiệu theo mốc hiện tại.
    public static int SendDay(Player player, int id, long nowMs)
    {
        ArgumentNullException.ThrowIfNull(player);
        foreach (BadgesData data in player.DataBadges)
        {
            if (data.IdBadGes == id)
            {
                long timeDifference = unchecked(data.TimeofUseBadges - nowMs);
                return unchecked((int)(timeDifference / MillisecondsPerDay));
            }
        }

        return 0;
    }
}
