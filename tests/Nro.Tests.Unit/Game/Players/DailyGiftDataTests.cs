using Nro.Game.Players.Gifts;

namespace Nro.Tests.Unit.Game.Players;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/DailyGift/DailyGiftData.java (dòng 3–17).
public sealed class DailyGiftDataTests
{
    // Kiểm tra giá trị mặc định và chuỗi JSON Java tính tay (Java: DailyGiftData, toString).
    [Fact]
    public void Defaults_and_json_string_match_java()
    {
        DailyGiftData data = new();

        Assert.Equal(-1, data.Id);
        Assert.False(data.Received);
        Assert.Equal("{\"id\":\"-1\",\"daNhan\":\"false\"}", data.ToString());

        data.Id = 2;
        data.Received = true;
        Assert.Equal("{\"id\":\"2\",\"daNhan\":\"true\"}", data.ToString());
    }
}
