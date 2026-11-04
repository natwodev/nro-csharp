using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.Core;

public class ItemMapServiceTests
{
    [Theory]
    [InlineData(371, false)]
    [InlineData(372, true)]
    [InlineData(375, true)]
    [InlineData(378, true)]
    [InlineData(379, false)]
    [InlineData(-372, false)]
    public void Black_balls_are_372_to_378(int id, bool expected) => Assert.Equal(expected, ItemMapService.IsBlackBall(id));

    [Theory]
    [InlineData(352, false)]
    [InlineData(353, true)]
    [InlineData(356, true)]
    [InlineData(360, true)]
    [InlineData(361, false)]
    [InlineData(362, false)]
    public void Namek_balls_are_353_to_360(int id, bool expected) => Assert.Equal(expected, ItemMapService.IsNamecBall(id));

    [Theory]
    [InlineData(361, false)]
    [InlineData(362, true)]
    [InlineData(363, false)]
    public void The_namek_ball_stone_is_362(int id, bool expected) => Assert.Equal(expected, ItemMapService.IsNamecBallStone(id));
}
