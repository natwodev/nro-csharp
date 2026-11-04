using Nro.Game;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Items.Core;

public class ItemEventTests
{
    private static (ItemEvent Event, ManualTimeProvider Clock) Make(DateTimeOffset? start = null)
    {
        var clock = new ManualTimeProvider(start ?? ManualTimeProvider.Vn(2025, 3, 10, 10, 0, 0));
        var itemEvent = new ItemEvent(new Player(), ItemTestKit.Clock(clock));
        return (itemEvent, clock);
    }

    private static readonly (string Name, Func<ItemEvent, int, bool> Drop, Func<ItemEvent, int> Remaining, Func<ItemEvent, long> Last)[] Kinds =
    [
        ("tree decoration", (e, n) => e.CanDropTatVoGiangSinh(n), e => e.RemainingTVGSCount, e => e.LastTVGSTime),
        ("candy", (e, n) => e.CanDropKeoGiangSinh(n), e => e.RemainingKeoGiangSinhCount, e => e.LastKeoGiangSinhTime),
        ("rose", (e, n) => e.CanDropHoaHong(n), e => e.RemainingHHCount, e => e.LastHHTime),
        ("water bottle", (e, n) => e.CanDropBinhNuoc(n), e => e.RemainingBNCount, e => e.LastBNTime),
    ];

    public static IEnumerable<object[]> KindIndexes() => Enumerable.Range(0, Kinds.Length).Select(i => new object[] { i });

    [Theory]
    [MemberData(nameof(KindIndexes))]
    public void The_first_call_ever_resets_the_budget_and_does_not_use_one_up(int kind)
    {
        (ItemEvent e, ManualTimeProvider clock) = Make();
        (_, Func<ItemEvent, int, bool> drop, Func<ItemEvent, int> remaining, Func<ItemEvent, long> last) = Kinds[kind];

        Assert.True(drop(e, 3));

        Assert.Equal(3, remaining(e));
        Assert.Equal(clock.GetUtcNow().ToUnixTimeMilliseconds(), last(e));
    }

    [Theory]
    [MemberData(nameof(KindIndexes))]
    public void A_day_allows_max_count_plus_one_drops(int kind)
    {
        (ItemEvent e, _) = Make();
        Func<ItemEvent, int, bool> drop = Kinds[kind].Drop;

        bool[] results = Enumerable.Range(0, 6).Select(_ => drop(e, 3)).ToArray();

        Assert.Equal(new[] { true, true, true, true, false, false }, results);
        Assert.Equal(0, Kinds[kind].Remaining(e));
    }

    [Theory]
    [MemberData(nameof(KindIndexes))]
    public void The_budget_comes_back_after_midnight_in_vietnam(int kind)
    {
        (ItemEvent e, ManualTimeProvider clock) = Make(ManualTimeProvider.Vn(2025, 3, 10, 23, 58, 0));
        Func<ItemEvent, int, bool> drop = Kinds[kind].Drop;
        drop(e, 1);
        drop(e, 1);
        Assert.False(drop(e, 1));

        clock.Advance(TimeSpan.FromMinutes(3));

        Assert.True(drop(e, 1));
        Assert.Equal(1, Kinds[kind].Remaining(e));
    }

    [Theory]
    [MemberData(nameof(KindIndexes))]
    public void The_budget_does_not_come_back_inside_the_same_vietnam_day(int kind)
    {
        (ItemEvent e, ManualTimeProvider clock) = Make(ManualTimeProvider.Vn(2025, 3, 10, 0, 0, 1));
        Func<ItemEvent, int, bool> drop = Kinds[kind].Drop;
        drop(e, 0);
        Assert.False(drop(e, 0));

        clock.Advance(TimeSpan.FromHours(23) + TimeSpan.FromMinutes(58));

        Assert.False(drop(e, 0));
    }

    [Theory]
    [MemberData(nameof(KindIndexes))]
    public void A_zero_or_negative_budget_gives_only_the_first_free_drop(int kind)
    {
        (ItemEvent e, _) = Make();
        Func<ItemEvent, int, bool> drop = Kinds[kind].Drop;

        Assert.True(drop(e, -5));
        Assert.False(drop(e, -5));
        Assert.Equal(-5, Kinds[kind].Remaining(e));
    }

    [Fact]
    public void The_four_budgets_are_independent()
    {
        (ItemEvent e, _) = Make();

        e.CanDropTatVoGiangSinh(0);
        Assert.False(e.CanDropTatVoGiangSinh(0));
        Assert.True(e.CanDropKeoGiangSinh(0));
        Assert.True(e.CanDropHoaHong(0));
        Assert.True(e.CanDropBinhNuoc(0));
    }

    [Fact]
    public void The_budget_follows_a_clock_moved_back_to_an_earlier_day_by_staying_spent()
    {
        (ItemEvent e, ManualTimeProvider clock) = Make();
        e.CanDropHoaHong(0);
        Assert.False(e.CanDropHoaHong(0));

        clock.Advance(TimeSpan.FromDays(-1));

        Assert.False(e.CanDropHoaHong(0));
    }

    [Fact]
    public void Counters_can_be_restored_from_saved_values()
    {
        (ItemEvent e, ManualTimeProvider clock) = Make();
        e.LastHHTime = clock.GetUtcNow().ToUnixTimeMilliseconds() - 1000;
        e.RemainingHHCount = 2;

        Assert.True(e.CanDropHoaHong(9));
        Assert.True(e.CanDropHoaHong(9));
        Assert.False(e.CanDropHoaHong(9));
    }

    [Fact]
    public void The_default_clock_is_the_system_clock()
    {
        var e = new ItemEvent(new Player());

        Assert.True(e.CanDropBinhNuoc(1));
        Assert.True(e.LastBNTime > 0);
    }
}
