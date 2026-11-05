using Nro.Game.Players;
using Nro.Game.Players.Gifts;
using Nro.Game.Players.Relations;

namespace Nro.Tests.Unit.Game.Players;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/{Friend,Enemy,Gift,Location,PointFusion}.java (các dòng 4–47).
public sealed class PlayerSupportDataTests
{
    // Kiểm tra tám trường Friend mặc định và Enemy chỉ kế thừa nguyên payload đó (Java: Friend, Enemy).
    [Fact]
    public void Friend_and_enemy_defaults_match_java()
    {
        Friend friend = new();
        Enemy enemy = new();

        Assert.Equal(0, friend.Id);
        Assert.Null(friend.Name);
        Assert.Equal((short)0, friend.Head);
        Assert.Equal((short)0, friend.Body);
        Assert.Equal((short)0, friend.Leg);
        Assert.Equal((sbyte)0, friend.Bag);
        Assert.Equal(0L, friend.Power);
        Assert.False(friend.Online);
        Assert.IsAssignableFrom<Friend>(enemy);
        Assert.Equal(0, enemy.Id);
        Assert.Null(enemy.Name);
        Assert.Equal((short)0, enemy.Head);
        Assert.Equal((short)0, enemy.Body);
        Assert.Equal((short)0, enemy.Leg);
        Assert.Equal((sbyte)0, enemy.Bag);
        Assert.Equal(0L, enemy.Power);
        Assert.False(enemy.Online);
    }

    // Kiểm tra Gift mặc định và dispose bỏ liên kết Player (Java: Gift, dispose).
    [Fact]
    public void Gift_defaults_and_dispose_match_java()
    {
        Player player = new();
        Gift gift = new(player);

        Assert.False(gift.IsNewbieGoldClaimed);
        Assert.False(gift.IsNewbieGemClaimed);
        Assert.Same(player, gift.Player);
        gift.Dispose();
        Assert.Null(gift.Player);
    }

    // Kiểm tra Location có vị trí và thời điểm di chuyển mặc định bằng không (Java: Location).
    [Fact]
    public void Location_defaults_match_java()
    {
        Location location = new();

        Assert.Equal((0, 0, 0L), (location.X, location.Y, location.LastPlayerMoveTime));
    }

    // Kiểm tra PointFusion khởi tạo ba điểm bằng không và dispose bỏ liên kết Player (Java: PointFusion).
    [Fact]
    public void Point_fusion_defaults_and_dispose_match_java()
    {
        Player player = new();
        PointFusion points = new(player);

        Assert.Equal((0, 0, 0), (points.FusionHp, points.FusionMp, points.FusionDamage));
        Assert.Same(player, points.Player);
        points.Dispose();
        Assert.Null(points.Player);
    }
}
