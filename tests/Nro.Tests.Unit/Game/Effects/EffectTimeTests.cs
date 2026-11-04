using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Game;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Effects;

/// <summary><see cref="EffectTime.CanDoWithTime"/> (Java <c>Util.canDoWithTime</c>) on a fake clock.</summary>
public class EffectTimeTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 3, 10, 0, 0, TimeSpan.FromHours(7));

    private static (ManualTimeProvider Time, VietnamClock Clock) Make()
    {
        var time = new ManualTimeProvider(Start);
        return (time, new VietnamClock(time));
    }

    [Fact]
    public void Is_strictly_greater_than_the_target()
    {
        (ManualTimeProvider time, VietnamClock clock) = Make();
        long last = clock.CurrentTimeMillis();
        Assert.False(clock.CanDoWithTime(last, 5000));
        time.Advance(TimeSpan.FromMilliseconds(4999));
        Assert.False(clock.CanDoWithTime(last, 5000));
        time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.False(clock.CanDoWithTime(last, 5000)); // exactly 5000 ms: still false
        time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.True(clock.CanDoWithTime(last, 5000)); // 5001 ms: true
    }

    [Fact]
    public void A_target_of_zero_is_true_one_millisecond_later_and_false_at_the_same_instant()
    {
        (ManualTimeProvider time, VietnamClock clock) = Make();
        long last = clock.CurrentTimeMillis();
        Assert.False(clock.CanDoWithTime(last, 0));
        time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.True(clock.CanDoWithTime(last, 0));
    }

    [Fact]
    public void A_never_set_time_of_zero_is_always_due()
    {
        (_, VietnamClock clock) = Make();
        Assert.True(clock.CanDoWithTime(0, 1_800_000));
    }

    [Fact]
    public void A_time_in_the_future_is_not_due_until_the_clock_catches_up()
    {
        (ManualTimeProvider time, VietnamClock clock) = Make();
        long future = clock.CurrentTimeMillis() + 60_000;
        Assert.False(clock.CanDoWithTime(future, 0));
        time.Advance(TimeSpan.FromMilliseconds(60_000));
        Assert.False(clock.CanDoWithTime(future, 0));
        time.Advance(TimeSpan.FromMilliseconds(1));
        Assert.True(clock.CanDoWithTime(future, 0));
    }

    [Fact]
    public void A_negative_target_is_due_even_in_the_same_instant()
    {
        (_, VietnamClock clock) = Make();
        Assert.True(clock.CanDoWithTime(clock.CurrentTimeMillis(), -1));
    }

    [Fact]
    public void The_subtraction_wraps_like_Java_long_arithmetic()
    {
        (_, VietnamClock clock) = Make();
        // now - long.MinValue overflows to a negative number in Java, so the answer is false.
        Assert.False(clock.CanDoWithTime(long.MinValue, 0));
    }

    [Fact]
    public void Null_clock_is_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => EffectTime.CanDoWithTime(null!, 0, 0));
    }
}
