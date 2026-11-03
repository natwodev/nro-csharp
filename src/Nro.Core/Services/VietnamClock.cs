using System.Globalization;

namespace Nro.Core;

/// <summary>
/// The clock-dependent half of <c>Utils/TimeUtil.java</c>. Java read <c>System.currentTimeMillis()</c>, <c>new Date()</c>, <c>Calendar.getInstance()</c> and
/// <c>LocalTime.now()</c> in the JVM default zone (<c>Asia/Ho_Chi_Minh</c>); here the instant comes from an injected <see cref="TimeProvider"/> (use a fake one in
/// tests) and the calendar is always Vietnam time (fixed +07:00, see <see cref="VietnamTime"/>). The machine's time zone and culture, and
/// <see cref="TimeProvider.LocalTimeZone"/>, are deliberately ignored, so "00:00 in Vietnam" is the same moment on every server.
/// <para>
/// Method names are the Java names in PascalCase. Pure functions of <c>TimeUtil</c> are in the static <see cref="TimeUtil"/>.
/// </para>
/// </summary>
public class VietnamClock
{
    private readonly TimeProvider _time;

    /// <param name="timeProvider">Source of the current instant; <see cref="TimeProvider.System"/> when omitted.</param>
    public VietnamClock(TimeProvider? timeProvider = null)
    {
        _time = timeProvider ?? TimeProvider.System;
    }

    /// <summary>Java <c>System.currentTimeMillis()</c>: epoch milliseconds (UTC), from the injected provider.</summary>
    public long CurrentTimeMillis() => _time.GetUtcNow().ToUnixTimeMilliseconds();

    /// <summary>The current moment as Vietnam wall-clock time.</summary>
    public DateTimeOffset Now() => _time.GetUtcNow().ToOffset(VietnamTime.Offset);

    /// <summary>
    /// Java <c>currentTimeMillisVietNam()</c>. It builds a <c>ZonedDateTime</c> in "UTC+7" and takes <c>toInstant().toEpochMilli()</c>, which is the SAME epoch milliseconds as
    /// <see cref="CurrentTimeMillis"/> (the zone does not shift an instant). Kept as is, because callers may rely on the (perhaps unexpected) value.
    /// </summary>
    public long CurrentTimeMillisVietNam() => CurrentTimeMillis();

    /// <summary>Java <c>getCurrDay()</c> = <c>new Date().getDay()</c>: day of week, 0 = Sunday ... 6 = Saturday.</summary>
    public int GetCurrDay() => (int)Now().DayOfWeek;

    /// <summary>Java <c>getCurrHour()</c> = <c>new Date().getHours()</c> (0..23).</summary>
    public int GetCurrHour() => Now().Hour;

    /// <summary>Java <c>getCurrMin()</c> = <c>new Date().getMinutes()</c> (0..59).</summary>
    public int GetCurrMin() => Now().Minute;

    /// <summary>Java <c>getTimeNow(String format)</c>: the current time with a <c>SimpleDateFormat</c> pattern.</summary>
    public string GetTimeNow(string format) => JavaDateFormat.Format(CurrentTimeMillis(), format);

    /// <summary>Java <c>getTimeBeforeCurrent(int subTime, String format)</c>: now minus <paramref name="subTime"/> milliseconds, formatted.</summary>
    public string GetTimeBeforeCurrent(int subTime, string format) => JavaDateFormat.Format(CurrentTimeMillis() - subTime, format);

    /// <summary>
    /// Java <c>isTimeNowInRangex(d1, d2, format)</c>: <c>true</c> when now is strictly between the two times. All three are parsed with <paramref name="format"/>, so with a time-only
    /// pattern ("HH:mm") the comparison is by time of day. Now is first formatted and parsed again, which truncates it to what the pattern can express.
    /// </summary>
    /// <exception cref="InvalidTimeException">A text does not match the pattern (Java: <c>Exception("Thời gian không hợp lệ")</c>).</exception>
    public bool IsTimeNowInRangex(string d1, string d2, string format)
    {
        // Java: a null text raises NullPointerException inside the try block, which is turned into the same Exception.
        if (d1 is null || d2 is null
            || !JavaDateFormat.TryParse(d1, format, out long time1)
            || !JavaDateFormat.TryParse(d2, format, out long time2)
            || !JavaDateFormat.TryParse(JavaDateFormat.Format(CurrentTimeMillis(), format), format, out long now))
        {
            throw new InvalidTimeException();
        }

        return now > time1 && now < time2;
    }

    /// <summary>Java <c>getTimeLeft(long lastTime, int secondTarget)</c>: "N phút" when more than 60 seconds remain, otherwise "N giây" (never negative).</summary>
    public string GetTimeLeft(long lastTime, int secondTarget)
    {
        int secondsLeft = SecondsLeft(lastTime, secondTarget);
        return secondsLeft > 60
            ? (secondsLeft / 60).ToString(CultureInfo.InvariantCulture) + " phút"
            : secondsLeft.ToString(CultureInfo.InvariantCulture) + " giây";
    }

    /// <summary>Java <c>getTimeLeft(long lastTime)</c>: how long ago, as "Nn trước" (days), "Ng trước" (hours), "Np trước" (minutes) or "Ngi trước" (seconds).</summary>
    public string GetTimeLeft(long lastTime)
    {
        int secondPassed = SecondsPassed(lastTime);
        return secondPassed > 86400
            ? (secondPassed / 86400).ToString(CultureInfo.InvariantCulture) + "n trước"
            : secondPassed > 3600
                ? (secondPassed / 3600).ToString(CultureInfo.InvariantCulture) + "g trước"
                : secondPassed > 60
                    ? (secondPassed / 60).ToString(CultureInfo.InvariantCulture) + "p trước"
                    : secondPassed.ToString(CultureInfo.InvariantCulture) + "gi trước";
    }

    /// <summary>Java <c>getMinLeft(long lastTime, int secondTarget)</c>: minutes left, rounded down, but 1 while 1..60 seconds remain and 0 when nothing remains.</summary>
    public int GetMinLeft(long lastTime, int secondTarget)
    {
        int secondsLeft = SecondsLeft(lastTime, secondTarget);
        int minLeft = 0;
        if (secondsLeft > 0 && secondsLeft <= 60)
        {
            minLeft = 1;
        }
        else if (secondsLeft > 60)
        {
            minLeft = secondsLeft / 60;
        }

        return minLeft;
    }

    /// <summary>Java <c>getSecondLeft(long lastTime, int secondTarget)</c>: seconds left until <paramref name="secondTarget"/> seconds have passed since <paramref name="lastTime"/> (never negative).</summary>
    public int GetSecondLeft(long lastTime, int secondTarget) => SecondsLeft(lastTime, secondTarget);

    /// <summary>Java <c>getDateLeft(long lastTime, int secondTarget)</c>: the time left as text, see <see cref="TimeUtil.ConvertTime"/>.</summary>
    public string GetDateLeft(long lastTime, int secondTarget) => TimeUtil.ConvertTime(SecondsLeft(lastTime, secondTarget));

    /// <summary>Java <c>convertTimeNow(long lastTime)</c>: the time elapsed since <paramref name="lastTime"/> as text (empty when it is in the future).</summary>
    public string ConvertTimeNow(long lastTime)
    {
        int secondsLeft = SecondsPassed(lastTime);
        if (secondsLeft < 0)
        {
            secondsLeft = 0;
        }

        return TimeUtil.ConvertTime(secondsLeft);
    }

    /// <summary>
    /// Java <c>isMabuOpen()</c>: open from 12:00 to 13:00 every day. DELIBERATE DIFFERENCE: the Java method also assigned <c>MajinBuu12H.HOUR_OPEN_MAP_MABU = 12</c> on every call;
    /// that field is already initialised to 12 and nothing else writes it (the other assignments are commented out), so the port of <c>MajinBuu12H</c> must declare it as 12.
    /// </summary>
    public bool IsMabuOpen()
    {
        int hour = Now().Hour;
        return hour >= 12 && hour < 13;
    }

    /// <summary>Java <c>isOpenWeekend()</c>: Saturday or Sunday.</summary>
    public bool IsOpenWeekend()
    {
        DayOfWeek day = Now().DayOfWeek;
        return day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
    }

    /// <summary>Java <c>isFlashSale()</c>: 09:00-09:29 and 21:00-21:29.</summary>
    public bool IsFlashSale()
    {
        DateTimeOffset now = Now();
        bool morning = now.Hour == 9 && now.Minute < 30;
        bool evening = now.Hour == 21 && now.Minute < 30;
        return morning || evening;
    }

    /// <summary>Java <c>isMabu14HOpen()</c>: 14:00 to 15:59.</summary>
    public bool IsMabu14HOpen()
    {
        int hour = Now().Hour;
        return hour >= 14 && hour < 16;
    }

    /// <summary>Java <c>is21H()</c>: 21:00 to 21:59.</summary>
    public bool Is21H()
    {
        int hour = Now().Hour;
        return hour >= 21 && hour < 22;
    }

    /// <summary>
    /// Java <c>getStartTimeBlackBallWar()</c>: today's (Vietnam date) open time, converted to epoch milliseconds AS IF IT WERE UTC
    /// (<c>startDateTime.toInstant(ZoneOffset.UTC)</c>), so the result is 7 hours later than the real Vietnam instant. The shift is kept; callers compare it with their own values.
    /// </summary>
    public long GetStartTimeBlackBallWar(BlackBallWarSchedule schedule)
    {
        DateTime start = Now().Date.Add(schedule.Open.ToTimeSpan());
        return new DateTimeOffset(start, TimeSpan.Zero).ToUnixTimeMilliseconds();
    }

    /// <summary>Java <c>isBlackBallWarOpen()</c>: the current time of day is strictly between open and close.</summary>
    public bool IsBlackBallWarOpen(BlackBallWarSchedule schedule)
    {
        TimeOnly current = CurrentTimeOfDay();
        return current > schedule.Open && current < schedule.Close;
    }

    /// <summary>Java <c>isBlackBallWarCanPick()</c>: the time of day is after the pick time and the war is open.</summary>
    public bool IsBlackBallWarCanPick(BlackBallWarSchedule schedule) => CurrentTimeOfDay() > schedule.CanPick && IsBlackBallWarOpen(schedule);

    /// <summary>Java <c>getSecondsUntilCanPick()</c>: whole seconds from now until the pick time today, or 0 once it has passed.</summary>
    public long GetSecondsUntilCanPick(BlackBallWarSchedule schedule)
    {
        TimeOnly current = CurrentTimeOfDay();
        if (current < schedule.CanPick)
        {
            return (schedule.CanPick - current).Ticks / TimeSpan.TicksPerSecond;
        }

        return 0;
    }

    private TimeOnly CurrentTimeOfDay() => TimeOnly.FromTimeSpan(Now().TimeOfDay);

    // Java: (int) ((System.currentTimeMillis() - lastTime) / 1000), the cast truncating to 32 bits.
    private int SecondsPassed(long lastTime) => unchecked((int)((CurrentTimeMillis() - lastTime) / 1000));

    private int SecondsLeft(long lastTime, int secondTarget)
    {
        int secondsLeft = unchecked(secondTarget - SecondsPassed(lastTime));
        return secondsLeft < 0 ? 0 : secondsLeft;
    }
}
