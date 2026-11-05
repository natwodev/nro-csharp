using Nro.Game.Players;
using Nro.Game.Players.Gifts;

namespace Nro.Tests.Unit.Game.Players;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/DailyGift/DailyGiftService.java (dòng 5–35).
public sealed class DailyGiftServiceTests
{
    // Kiểm tra reset ngày mới, kiểm tra quà, và đánh dấu đúng phần tử đầu tiên chưa nhận (Java: checkDailyGift, updateDailyGift, addAndReset).
    [Fact]
    public void Daily_gifts_are_reset_checked_and_updated_like_java()
    {
        Player player = new();
        player.DailyGiftData.Add(new DailyGiftData { Id = 99, Received = true });

        DailyGiftService.AddAndReset(player);

        Assert.Collection(player.DailyGiftData,
            data => Assert.Equal(((sbyte)0, false), (data.Id, data.Received)),
            data => Assert.Equal(((sbyte)1, false), (data.Id, data.Received)),
            data => Assert.Equal(((sbyte)2, false), (data.Id, data.Received)));
        Assert.True(DailyGiftService.CheckDailyGift(player, 1));

        DailyGiftService.UpdateDailyGift(player, 1);

        Assert.False(DailyGiftService.CheckDailyGift(player, 1));
        Assert.True(DailyGiftService.CheckDailyGift(player, 2));
        DailyGiftService.UpdateDailyGift(player, 9);
        Assert.Equal(3, player.DailyGiftData.Count);
    }
}
