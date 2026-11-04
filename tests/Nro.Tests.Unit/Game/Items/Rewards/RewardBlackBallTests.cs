using System.Reflection;
using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Game;
using Nro.Tests.Unit.Helpers;
using Nro.Tests.Unit.Game.Items.Core;

namespace Nro.Tests.Unit.Game.Items.Rewards;

public class RewardBlackBallTests
{
    private static readonly long TwentyTwoHours = 79_200_000L;

    private sealed class Rig
    {
        public Rig(DateTimeOffset? start = null)
        {
            Clock = new ManualTimeProvider(start ?? ManualTimeProvider.Vn(2025, 1, 1, 22, 0, 0));
            Host = new RecordingHost();
            Player = new Player();
            Ball = new RewardBlackBall(Player, ItemTestKit.Clock(Clock), Host);
        }

        public ManualTimeProvider Clock { get; }

        public RecordingHost Host { get; }

        public Player Player { get; }

        public RewardBlackBall Ball { get; }

        public long Now => Clock.GetUtcNow().ToUnixTimeMilliseconds();
    }

    [Theory]
    [InlineData("R1S_1", 20)]
    [InlineData("R1S_2", 21)]
    [InlineData("R2S_1", 35)]
    [InlineData("R2S_2", 20)]
    [InlineData("R3S_1", 35)]
    [InlineData("R3S_2", 10)]
    [InlineData("R4S_1", 10)]
    [InlineData("R4S_2", 35)]
    [InlineData("R5S_1", 35)]
    [InlineData("R5S_2", 20)]
    [InlineData("R5S_3", 20)]
    [InlineData("R6S_1", 40)]
    [InlineData("R6S_2", 20)]
    [InlineData("R7S_1", 14)]
    [InlineData("R7S_2", 15)]
    [InlineData("TIME_WAIT", 3_600_000)]
    public void Stat_constants_match_the_java_values(string name, int value)
    {
        FieldInfo field = typeof(RewardBlackBall).GetField(name)!;

        Assert.Equal(value, (int)field.GetRawConstantValue()!);
    }

    [Fact]
    public void A_new_reward_has_seven_empty_slots_per_array()
    {
        var rig = new Rig();

        Assert.Equal(new long[7], rig.Ball.TimeOutOfDateReward);
        Assert.Equal(new int[7], rig.Ball.QuantilyBlackBall);
        Assert.Equal(new long[7], rig.Ball.LastTimeGetReward);
        Assert.Same(rig.Player, rig.Ball.Player);
    }

    [Fact]
    public void Two_players_do_not_share_their_arrays_or_their_war_time()
    {
        var a = new Rig();
        var b = new Rig(ManualTimeProvider.Vn(2025, 2, 2, 22, 0, 0));

        a.Ball.TimeOutOfDateReward[0] = 5;

        Assert.Equal(0, b.Ball.TimeOutOfDateReward[0]);
        Assert.NotEqual(a.Ball.Time8h, b.Ball.Time8h);
    }

    [Fact]
    public void The_war_time_is_the_opening_time_of_the_vietnam_date_taken_as_if_it_were_utc()
    {
        var rig = new Rig();

        // 2025-01-01 20:00:00 read as UTC
        Assert.Equal(new DateTimeOffset(2025, 1, 1, 20, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds(), rig.Ball.Time8h);
    }

    [Fact]
    public void The_war_time_follows_the_schedule_and_the_clock()
    {
        var rig = new Rig();
        var other = new RewardBlackBall(rig.Player, ItemTestKit.Clock(rig.Clock), rig.Host, new BlackBallWarSchedule(new TimeOnly(19, 30, 0), new TimeOnly(20, 30, 0), new TimeOnly(19, 35, 0)));

        Assert.Equal(new DateTimeOffset(2025, 1, 1, 19, 30, 0, TimeSpan.Zero).ToUnixTimeMilliseconds(), other.Time8h);

        rig.Clock.Advance(TimeSpan.FromDays(1));

        Assert.Equal(new DateTimeOffset(2025, 1, 2, 19, 30, 0, TimeSpan.Zero).ToUnixTimeMilliseconds(), other.Time8h);
    }

    [Fact]
    public void Winning_a_ball_starts_a_22_hour_bonus_and_resends_the_stats()
    {
        var rig = new Rig();

        rig.Ball.Reward(3);

        Assert.Equal(rig.Now + TwentyTwoHours, rig.Ball.TimeOutOfDateReward[2]);
        Assert.Equal(0, rig.Ball.QuantilyBlackBall[2]);
        Assert.Equal(new[] { "point" }, rig.Host.Calls);
    }

    [Fact]
    public void Winning_again_while_the_bonus_runs_past_the_war_time_counts_one_more()
    {
        var rig = new Rig();
        rig.Ball.Reward(3);

        rig.Ball.Reward(3);
        rig.Ball.Reward(3);

        Assert.Equal(2, rig.Ball.QuantilyBlackBall[2]);
    }

    [Fact]
    public void A_bonus_that_ends_before_the_war_time_is_not_counted_again()
    {
        var rig = new Rig(ManualTimeProvider.Vn(2025, 1, 1, 1, 0, 0));
        rig.Ball.Reward(1);

        // the bonus ends 2025-01-01 23:00 Vietnam = 16:00 UTC, before the war time 20:00 "UTC"
        rig.Ball.Reward(1);

        Assert.Equal(0, rig.Ball.QuantilyBlackBall[0]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    public void The_first_and_last_star_are_valid(sbyte star)
    {
        var rig = new Rig();

        rig.Ball.Reward(star);

        Assert.True(rig.Ball.TimeOutOfDateReward[star - 1] > 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    [InlineData(-1)]
    public void An_invalid_star_throws(sbyte star)
    {
        var rig = new Rig();

        Assert.Throws<IndexOutOfRangeException>(() => rig.Ball.Reward(star));
        Assert.Empty(rig.Host.Calls);
    }

    [Fact]
    public void Select_picks_among_the_stars_whose_bonus_still_runs()
    {
        var rig = new Rig();
        rig.Ball.TimeOutOfDateReward[1] = rig.Now + 1000;
        rig.Ball.TimeOutOfDateReward[2] = rig.Now - 1;
        rig.Ball.TimeOutOfDateReward[4] = rig.Now + 1000;

        rig.Ball.GetRewardSelect(1);

        Assert.Equal(1, rig.Ball.QuantilyBlackBall[4]);
        Assert.Equal(0, rig.Ball.QuantilyBlackBall[1]);
        Assert.Equal("Phần thưởng chỉ số 5 sao tự động nhận", rig.Host.Notifications.Single());
        Assert.Equal(new[] { "notify", "point" }, rig.Host.Calls);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(100)]
    [InlineData(-1)]
    public void Select_outside_the_list_does_nothing(sbyte select)
    {
        var rig = new Rig();
        rig.Ball.TimeOutOfDateReward[1] = rig.Now + 1000;
        rig.Ball.TimeOutOfDateReward[4] = rig.Now + 1000;

        rig.Ball.GetRewardSelect(select);

        Assert.Empty(rig.Host.Calls);
        Assert.Equal(new int[7], rig.Ball.QuantilyBlackBall);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    public void Every_star_has_its_claim_text(int star)
    {
        var rig = new Rig();
        rig.Ball.TimeOutOfDateReward[star - 1] = rig.Now + 1000;

        rig.Ball.GetRewardSelect(0);

        Assert.Equal("Phần thưởng chỉ số " + star + " sao tự động nhận", rig.Host.Notifications.Single());
        Assert.Equal(rig.Now, rig.Ball.LastTimeGetReward[star - 1]);
    }

    [Fact]
    public void A_second_claim_inside_the_hour_is_refused_with_the_always_zero_minutes_text()
    {
        var rig = new Rig();
        rig.Ball.TimeOutOfDateReward[0] = rig.Now + TwentyTwoHours;
        rig.Ball.GetRewardSelect(0);
        rig.Host.Notifications.Clear();

        rig.Clock.Advance(TimeSpan.FromMinutes(10));
        rig.Ball.GetRewardSelect(0);

        Assert.Equal("Chưa thể nhận phần quà ngay lúc này, vui lòng đợi 0 phút nữa", rig.Host.Notifications.Single());
        Assert.Equal(1, rig.Ball.QuantilyBlackBall[0]);
    }

    [Fact]
    public void The_hour_is_strict_exactly_one_hour_is_refused_one_millisecond_more_is_allowed()
    {
        var rig = new Rig();
        rig.Ball.TimeOutOfDateReward[0] = rig.Now + TwentyTwoHours;
        rig.Ball.GetRewardSelect(0);

        rig.Clock.Advance(TimeSpan.FromMilliseconds(RewardBlackBall.TIME_WAIT));
        rig.Ball.GetRewardSelect(0);
        Assert.Equal(1, rig.Ball.QuantilyBlackBall[0]);

        rig.Clock.Advance(TimeSpan.FromMilliseconds(1));
        rig.Ball.GetRewardSelect(0);
        Assert.Equal(2, rig.Ball.QuantilyBlackBall[0]);
    }

    [Fact]
    public void Dispose_does_nothing()
    {
        var rig = new Rig();
        rig.Ball.TimeOutOfDateReward[0] = 9;

        rig.Ball.Dispose();

        Assert.Equal(9, rig.Ball.TimeOutOfDateReward[0]);
    }

    [Fact]
    public void The_shell_constructor_works_with_the_default_clock_and_host()
    {
        var player = new Player();

        player.RewardBlackBall.Reward(2);

        Assert.True(player.RewardBlackBall.TimeOutOfDateReward[1] > 0);
    }

    [Fact]
    public void The_default_schedule_is_20_to_21()
    {
        Assert.Equal(new TimeOnly(20, 0, 0), RewardBlackBall.DefaultSchedule.Open);
        Assert.Equal(new TimeOnly(21, 0, 0), RewardBlackBall.DefaultSchedule.Close);
    }
}
