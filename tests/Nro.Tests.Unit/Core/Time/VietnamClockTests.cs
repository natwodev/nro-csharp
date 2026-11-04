using System.Globalization;
using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Exceptions;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Tests.Unit.Helpers;
using static Nro.Tests.Unit.Helpers.ManualTimeProvider;

namespace Nro.Tests.Unit.Core.Time;

/// <summary>
/// <see cref="VietnamClock"/> (the clock-dependent functions of <c>TimeUtil</c>) with a fake clock. Expected values come from hand-written Vietnam wall-clock
/// moments (UTC+7), converted by the plain .NET API, never by the code under test. Every check runs with providers whose "local" zone is NOT Vietnam
/// (UTC-8, UTC+14, UTC, and Vietnam itself), proving that neither the provider's nor the machine's zone is used.
/// </summary>
public class VietnamClockTests
{
    private static readonly TimeZoneInfo[] Zones =
    [
        TimeZoneInfo.CreateCustomTimeZone("t/-8", TimeSpan.FromHours(-8), "t", "t"),
        TimeZoneInfo.CreateCustomTimeZone("t/+14", TimeSpan.FromHours(14), "t", "t"),
        TimeZoneInfo.Utc,
        TimeZoneInfo.CreateCustomTimeZone("t/+7", TimeSpan.FromHours(7), "t", "t"),
    ];

    private static void ForEachZone(DateTimeOffset instant, Action<VietnamClock> check)
    {
        foreach (TimeZoneInfo zone in Zones)
        {
            check(new VietnamClock(new ManualTimeProvider(instant, zone)));
        }
    }

    [Fact]
    public void Default_clock_uses_the_system_time()
    {
        long before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long now = new VietnamClock().CurrentTimeMillis();
        long after = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Assert.InRange(now, before, after);
    }

    [Fact]
    public void CurrentTimeMillis_and_CurrentTimeMillisVietNam_are_the_epoch_milliseconds_of_the_provider()
    {
        DateTimeOffset instant = Vn(2024, 3, 1, 12, 0, 0, 123);
        ForEachZone(instant, clock =>
        {
            Assert.Equal(instant.ToUnixTimeMilliseconds(), clock.CurrentTimeMillis());
            Assert.Equal(instant.ToUnixTimeMilliseconds(), clock.CurrentTimeMillisVietNam()); // the Java "UTC+7" value is the same epoch value
        });
    }

    public static IEnumerable<object[]> DayBoundaries() =>
    [
        // vn instant (before midnight), expected (day of week, "dd/MM/yyyy HH:mm:ss") just before and just after Vietnam midnight
        [Vn(2024, 2, 29, 23, 59, 59, 999), 4, "29/02/2024 23:59:59", 23, 59],                // Thursday, leap day
        [Vn(2024, 2, 29, 23, 59, 59, 999).AddMilliseconds(1), 5, "01/03/2024 00:00:00", 0, 0], // month boundary -> Friday
        [Vn(2024, 12, 31, 23, 59, 59, 999), 2, "31/12/2024 23:59:59", 23, 59],               // Tuesday
        [Vn(2024, 12, 31, 23, 59, 59, 999).AddMilliseconds(1), 3, "01/01/2025 00:00:00", 0, 0], // year boundary -> Wednesday
        [Vn(2024, 3, 3, 23, 59, 59, 999), 0, "03/03/2024 23:59:59", 23, 59],                 // Sunday
        [Vn(2024, 3, 3, 23, 59, 59, 999).AddMilliseconds(1), 1, "04/03/2024 00:00:00", 0, 0],   // week boundary -> Monday
        // The same moments expressed in UTC: 16:59:59.999 UTC is still the 1st in Vietnam, 17:00:00 UTC is already the 2nd.
        [new DateTimeOffset(2024, 3, 1, 16, 59, 59, 999, TimeSpan.Zero), 5, "01/03/2024 23:59:59", 23, 59],
        [new DateTimeOffset(2024, 3, 1, 17, 0, 0, 0, TimeSpan.Zero), 6, "02/03/2024 00:00:00", 0, 0],
    ];

    [Theory]
    [MemberData(nameof(DayBoundaries))]
    public void Day_hour_minute_and_formatted_time_follow_Vietnam_midnight(DateTimeOffset instant, int dayOfWeek, string formatted, int hour, int minute)
    {
        ForEachZone(instant, clock =>
        {
            Assert.Equal(dayOfWeek, clock.GetCurrDay());
            Assert.Equal(hour, clock.GetCurrHour());
            Assert.Equal(minute, clock.GetCurrMin());
            Assert.Equal(formatted, clock.GetTimeNow("dd/MM/yyyy HH:mm:ss"));
        });
    }

    public static IEnumerable<object[]> WeekendBoundaries() =>
    [
        [Vn(2024, 3, 1, 23, 59, 59, 999), false],                         // Friday night
        [Vn(2024, 3, 1, 23, 59, 59, 999).AddMilliseconds(1), true],       // Saturday 00:00
        [Vn(2024, 3, 3, 23, 59, 59, 999), true],                          // Sunday night
        [Vn(2024, 3, 3, 23, 59, 59, 999).AddMilliseconds(1), false],      // Monday 00:00
        [new DateTimeOffset(2024, 3, 1, 16, 59, 59, 999, TimeSpan.Zero), false], // 23:59:59.999 in Vietnam (UTC date is still Friday as well)
        [new DateTimeOffset(2024, 3, 1, 17, 0, 0, 0, TimeSpan.Zero), true],      // UTC Friday 17:00 = Vietnam Saturday 00:00
        [new DateTimeOffset(2024, 3, 3, 16, 59, 59, 999, TimeSpan.Zero), true],  // UTC Sunday 16:59 = Vietnam Sunday 23:59
        [new DateTimeOffset(2024, 3, 3, 17, 0, 0, 0, TimeSpan.Zero), false],     // UTC Sunday 17:00 = Vietnam Monday 00:00
    ];

    [Theory]
    [MemberData(nameof(WeekendBoundaries))]
    public void IsOpenWeekend_is_true_from_Saturday_00_00_to_Sunday_23_59_59_999_Vietnam_time(DateTimeOffset instant, bool expected)
    {
        ForEachZone(instant, clock => Assert.Equal(expected, clock.IsOpenWeekend()));
    }

    // vn time of day, then expected: IsMabuOpen (12:00-13:00), IsFlashSale (09:00-09:30, 21:00-21:30), IsMabu14HOpen (14:00-16:00), Is21H (21:00-22:00)
    public static IEnumerable<object[]> WindowRows() =>
    [
        [8, 59, 59, 999, false, false, false, false],
        [9, 0, 0, 0, false, true, false, false],
        [9, 29, 59, 999, false, true, false, false],
        [9, 30, 0, 0, false, false, false, false],
        [11, 59, 59, 999, false, false, false, false],
        [12, 0, 0, 0, true, false, false, false],
        [12, 59, 59, 999, true, false, false, false],
        [13, 0, 0, 0, false, false, false, false],
        [13, 59, 59, 999, false, false, false, false],
        [14, 0, 0, 0, false, false, true, false],
        [15, 59, 59, 999, false, false, true, false],
        [16, 0, 0, 0, false, false, false, false],
        [20, 59, 59, 999, false, false, false, false],
        [21, 0, 0, 0, false, true, false, true],
        [21, 29, 59, 999, false, true, false, true],
        [21, 30, 0, 0, false, false, false, true],
        [21, 59, 59, 999, false, false, false, true],
        [22, 0, 0, 0, false, false, false, false],
        [0, 0, 0, 0, false, false, false, false],
        [23, 59, 59, 999, false, false, false, false],
    ];

    [Theory]
    [MemberData(nameof(WindowRows))]
    public void Daily_windows_are_decided_by_the_Vietnam_time_of_day(int hour, int minute, int second, int ms, bool mabu, bool flash, bool mabu14, bool h21)
    {
        ForEachZone(Vn(2024, 5, 8, hour, minute, second, ms), clock =>
        {
            Assert.Equal(mabu, clock.IsMabuOpen());
            Assert.Equal(flash, clock.IsFlashSale());
            Assert.Equal(mabu14, clock.IsMabu14HOpen());
            Assert.Equal(h21, clock.Is21H());
        });
    }

    [Fact]
    public void Time_left_functions_match_the_Java_formulas()
    {
        DateTimeOffset now = Vn(2024, 1, 15, 10, 30, 45);
        long nowMs = now.ToUnixTimeMilliseconds();
        var clock = new VietnamClock(new ManualTimeProvider(now));

        // passed 30 s
        long last = nowMs - 30_000;
        Assert.Equal("1 phút", clock.GetTimeLeft(last, 100));   // 70 s left -> more than 60 -> minutes (70 / 60 = 1)
        Assert.Equal("60 giây", clock.GetTimeLeft(last, 90));    // exactly 60 left is NOT more than 60
        Assert.Equal("1 giây", clock.GetTimeLeft(last, 31));
        Assert.Equal("0 giây", clock.GetTimeLeft(last, 30));
        Assert.Equal("0 giây", clock.GetTimeLeft(last, 5));      // never negative
        Assert.Equal("2 phút", clock.GetTimeLeft(last, 150));    // 120 s left

        Assert.Equal(0, clock.GetMinLeft(last, 30));
        Assert.Equal(0, clock.GetMinLeft(last, 10));
        Assert.Equal(1, clock.GetMinLeft(last, 31));             // 1 s left -> 1 minute
        Assert.Equal(1, clock.GetMinLeft(last, 90));             // 60 s left -> 1
        Assert.Equal(1, clock.GetMinLeft(last, 91));             // 61 s left -> 61 / 60 = 1
        Assert.Equal(1, clock.GetMinLeft(last, 149));            // 119 s
        Assert.Equal(2, clock.GetMinLeft(last, 150));            // 120 s

        Assert.Equal(70, clock.GetSecondLeft(last, 100));
        Assert.Equal(0, clock.GetSecondLeft(last, 10));
        Assert.Equal(3661, clock.GetSecondLeft(nowMs, 3661));
        Assert.Equal("1 giờ 1 phút 1 giây", clock.GetDateLeft(nowMs, 3661));
        Assert.Equal(string.Empty, clock.GetDateLeft(last, 10));
    }

    [Theory]
    [InlineData(0L, "0gi trước")]
    [InlineData(59_999L, "59gi trước")]
    [InlineData(60_000L, "60gi trước")]
    [InlineData(61_000L, "1p trước")]
    [InlineData(3_600_000L, "60p trước")]
    [InlineData(3_601_000L, "1g trước")]
    [InlineData(86_400_000L, "24g trước")]
    [InlineData(86_401_000L, "1n trước")]
    [InlineData(-5_000L, "-5gi trước")] // a lastTime in the future
    public void GetTimeLeft_since_shows_the_largest_unit_with_the_Java_comparisons(long elapsedMs, string expected)
    {
        DateTimeOffset now = Vn(2024, 1, 15, 10, 30, 45);
        var clock = new VietnamClock(new ManualTimeProvider(now));
        Assert.Equal(expected, clock.GetTimeLeft(now.ToUnixTimeMilliseconds() - elapsedMs));
    }

    [Fact]
    public void Seconds_passed_wraps_to_32_bits_like_the_Java_int_cast()
    {
        // now = 1705289445000 ms; lastTime = -5e12 -> 6705289445 s -> (int) = -1884645147
        var clock = new VietnamClock(new ManualTimeProvider(Vn(2024, 1, 15, 10, 30, 45)));
        Assert.Equal("-1884645147gi trước", clock.GetTimeLeft(-5_000_000_000_000L));
    }

    [Fact]
    public void ConvertTimeNow_shows_the_elapsed_time_and_is_empty_for_the_future()
    {
        DateTimeOffset now = Vn(2024, 1, 15, 10, 30, 45);
        long nowMs = now.ToUnixTimeMilliseconds();
        var clock = new VietnamClock(new ManualTimeProvider(now));
        Assert.Equal("1 ngày 1 giờ 1 phút 1 giây", clock.ConvertTimeNow(nowMs - 90_061_000L));
        Assert.Equal(string.Empty, clock.ConvertTimeNow(nowMs));
        Assert.Equal(string.Empty, clock.ConvertTimeNow(nowMs + 5000));
        Assert.Equal("5 giây", clock.ConvertTimeNow(nowMs - 5999));
    }

    [Fact]
    public void GetTimeBeforeCurrent_subtracts_milliseconds_and_formats_in_Vietnam_time()
    {
        DateTimeOffset now = Vn(2024, 3, 1, 0, 0, 30);
        ForEachZone(now, clock =>
        {
            Assert.Equal("2024-03-01", clock.GetTimeBeforeCurrent(0, "yyyy-MM-dd"));
            Assert.Equal("2024-02-29", clock.GetTimeBeforeCurrent(30_001, "yyyy-MM-dd"));
            Assert.Equal("2024-02-27", clock.GetTimeBeforeCurrent(3 * 24 * 60 * 60 * 1000, "yyyy-MM-dd")); // the call in the game (3 days) crosses the leap day
        });
    }

    [Fact]
    public void IsTimeNowInRangex_compares_strictly_with_the_current_time_cut_to_the_pattern()
    {
        DateTimeOffset now = Vn(2024, 1, 15, 10, 30, 45, 500);
        ForEachZone(now, clock =>
        {
            const string Full = "yyyy-MM-dd HH:mm:ss";
            Assert.True(clock.IsTimeNowInRangex("2024-01-15 10:00:00", "2024-01-15 11:00:00", Full));
            Assert.False(clock.IsTimeNowInRangex("2024-01-15 10:30:45", "2024-01-15 11:00:00", Full)); // equal to the start (now is cut to the second): not strictly after
            Assert.False(clock.IsTimeNowInRangex("2024-01-15 10:00:00", "2024-01-15 10:30:45", Full)); // equal to the end
            Assert.True(clock.IsTimeNowInRangex("2024-01-15 10:30:44", "2024-01-15 10:30:46", Full));
            Assert.False(clock.IsTimeNowInRangex("2024-01-16 00:00:00", "2024-01-17 00:00:00", Full));
            Assert.True(clock.IsTimeNowInRangex("10:29", "10:31", "HH:mm"));
            Assert.False(clock.IsTimeNowInRangex("10:30", "10:31", "HH:mm"));
            Assert.False(clock.IsTimeNowInRangex("23:00", "23:59", "HH:mm"));
        });
    }

    [Fact]
    public void IsTimeNowInRangex_reports_bad_text_like_Java_and_a_bad_pattern_as_ArgumentException()
    {
        var clock = new VietnamClock(new ManualTimeProvider(Vn(2024, 1, 15, 10, 30, 45)));
        InvalidTimeException e = Assert.Throws<InvalidTimeException>(() => clock.IsTimeNowInRangex("x", "2024-01-15", "yyyy-MM-dd"));
        Assert.Equal("Thời gian không hợp lệ", e.Message);
        Assert.Throws<InvalidTimeException>(() => clock.IsTimeNowInRangex("2024-01-15", "y", "yyyy-MM-dd"));
        Assert.Throws<InvalidTimeException>(() => clock.IsTimeNowInRangex(null!, "2024-01-15", "yyyy-MM-dd"));
        Assert.Throws<ArgumentException>(() => clock.IsTimeNowInRangex("2024", "2025", "bbbb"));
    }

    private static readonly BlackBallWarSchedule Schedule = new(new TimeOnly(20, 0, 0), new TimeOnly(21, 0, 0), new TimeOnly(20, 5, 30));

    [Fact]
    public void BlackBallWar_open_close_and_pick_times_are_strict_and_use_Vietnam_time_of_day()
    {
        (DateTimeOffset At, bool Open, bool CanPick)[] rows =
        [
            (Vn(2024, 1, 15, 19, 59, 59, 999), false, false),
            (Vn(2024, 1, 15, 20, 0, 0, 0), false, false),   // open is exclusive
            (Vn(2024, 1, 15, 20, 0, 0, 1), true, false),
            (Vn(2024, 1, 15, 20, 5, 30, 0), true, false),   // can-pick is exclusive
            (Vn(2024, 1, 15, 20, 5, 30, 1), true, true),
            (Vn(2024, 1, 15, 20, 59, 59, 999), true, true),
            (Vn(2024, 1, 15, 21, 0, 0, 0), false, false),   // close is exclusive
            (new DateTimeOffset(2024, 1, 15, 13, 0, 0, 1, TimeSpan.Zero), true, false), // 20:00:00.001 Vietnam
        ];
        foreach ((DateTimeOffset at, bool open, bool canPick) in rows)
        {
            ForEachZone(at, clock =>
            {
                Assert.Equal(open, clock.IsBlackBallWarOpen(Schedule));
                Assert.Equal(canPick, clock.IsBlackBallWarCanPick(Schedule));
            });
        }
    }

    [Fact]
    public void GetSecondsUntilCanPick_counts_whole_seconds_down_to_zero()
    {
        ForEachZone(Vn(2024, 1, 15, 20, 0, 0), clock => Assert.Equal(330, clock.GetSecondsUntilCanPick(Schedule)));
        ForEachZone(Vn(2024, 1, 15, 20, 0, 0, 500), clock => Assert.Equal(329, clock.GetSecondsUntilCanPick(Schedule))); // 329.5 s -> Duration.getSeconds() = 329
        ForEachZone(Vn(2024, 1, 15, 19, 0, 0), clock => Assert.Equal(3930, clock.GetSecondsUntilCanPick(Schedule)));
        ForEachZone(Vn(2024, 1, 15, 20, 5, 29, 999), clock => Assert.Equal(0, clock.GetSecondsUntilCanPick(Schedule))); // 1 ms left -> 0 whole seconds
        ForEachZone(Vn(2024, 1, 15, 20, 5, 30), clock => Assert.Equal(0, clock.GetSecondsUntilCanPick(Schedule)));
        ForEachZone(Vn(2024, 1, 15, 23, 0, 0), clock => Assert.Equal(0, clock.GetSecondsUntilCanPick(Schedule)));
    }

    [Fact]
    public void GetStartTimeBlackBallWar_uses_the_Vietnam_date_and_labels_the_open_time_as_UTC()
    {
        long expected = new DateTimeOffset(2024, 1, 15, 20, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        ForEachZone(Vn(2024, 1, 15, 8, 0, 0), clock => Assert.Equal(expected, clock.GetStartTimeBlackBallWar(Schedule)));
        ForEachZone(Vn(2024, 1, 15, 23, 59, 59, 999), clock => Assert.Equal(expected, clock.GetStartTimeBlackBallWar(Schedule)));

        // 00:30 on the 16th in Vietnam is still the 15th in UTC: the Vietnam date (16th) must be used
        long expectedNextDay = new DateTimeOffset(2024, 1, 16, 20, 0, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();
        ForEachZone(Vn(2024, 1, 16, 0, 30, 0), clock => Assert.Equal(expectedNextDay, clock.GetStartTimeBlackBallWar(Schedule)));
    }

    [Fact]
    public void Now_is_Vietnam_wall_clock_with_a_plus_seven_offset()
    {
        DateTimeOffset instant = new(2024, 3, 1, 17, 0, 0, TimeSpan.Zero);
        ForEachZone(instant, clock =>
        {
            DateTimeOffset now = clock.Now();
            Assert.Equal(TimeSpan.FromHours(7), now.Offset);
            Assert.Equal(new DateTime(2024, 3, 2, 0, 0, 0), now.DateTime);
            Assert.Equal(instant, now);
        });
    }

    [Fact]
    public void The_date_code_never_reads_the_machine_time_zone()
    {
        // TimeZoneInfo.Local / ToLocalTime / DateTime.Now would make results depend on the server's zone (lint L12 only covers DateTime.Now).
        string root = Path.Combine(TestPaths.RepoRoot(), "src", "Nro.Core");
        foreach (string file in new[] { "Time/TimeUtil.cs", "Time/JavaDateFormat.cs", "Time/VietnamTime.cs", "Time/VietnamClock.cs" })
        {
            string code = File.ReadAllText(Path.Combine(root, file));
            foreach (string forbidden in new[] { "TimeZoneInfo.Local", "ToLocalTime", "DateTime.Now", "DateTime.Today", "DateTimeOffset.Now", "LocalTimeZone", "CultureInfo.CurrentCulture", "DateTime.UtcNow" })
            {
                // the doc comments may MENTION LocalTimeZone / the like; only code lines count
                bool inCode = code.Split('\n').Any(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal) && !line.TrimStart().StartsWith('*') && line.Contains(forbidden, StringComparison.Ordinal));
                Assert.False(inCode, $"{file} uses {forbidden}");
            }
        }
    }

    [Fact]
    public void Output_keeps_ASCII_digits_and_the_minus_sign_under_a_hostile_culture()
    {
        HostileCulture.Run(() =>
        {
            var clock = new VietnamClock(new ManualTimeProvider(Vn(2024, 1, 15, 10, 30, 45)));
            Assert.Equal("15/01/2024 10:30:45", clock.GetTimeNow("dd/MM/yyyy HH:mm:ss"));
            Assert.Equal("5p trước", clock.GetTimeLeft(clock.CurrentTimeMillis() - 300_000 - 500));
            Assert.Equal("-5gi trước", clock.GetTimeLeft(clock.CurrentTimeMillis() + 5000));
            Assert.Equal("2 phút", clock.GetTimeLeft(clock.CurrentTimeMillis(), 150));
            Assert.Equal("-1 phút 0 giây", TimeUtil.GetTime(-90000));
            Assert.Equal("-1 phút", TimeUtil.ConvertMillisecondToMinute(-60000));
        });
    }
}
