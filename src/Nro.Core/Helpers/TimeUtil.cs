using System.Globalization;
using System.Text;

namespace Nro.Core;

/// <summary>
/// Port of the PURE part of <c>Utils/TimeUtil.java</c>: the functions that depend only on their arguments (checked against <c>tests/golden/util-pure.json</c>).
/// The functions that read the system clock (<c>getCurrDay</c>, <c>getTimeLeft</c>, <c>getTimeNow</c>, <c>isMabuOpen</c>, <c>isFlashSale</c>, BlackBallWar times...)
/// live in <see cref="VietnamClock"/>, which takes a <see cref="TimeProvider"/> so they are testable.
/// <para>
/// A Java <c>Date</c> is an instant given as epoch milliseconds: every function has a <c>long</c> overload (Java <c>date.getTime()</c>) and a <see cref="DateTimeOffset"/> overload.
/// All calendar work is done in Vietnam time (fixed +07:00, see <see cref="VietnamTime"/>), whatever the machine's time zone or culture is.
/// </para>
/// <para>
/// Java bugs are kept ON PURPOSE because the client and saved data rely on the exact output: <see cref="DiffDate(long, long, sbyte)"/> overflows <c>int</c> for MONTH
/// (negative result) and YEAR (about 17 days); <see cref="GetTimeAgo"/> glues the parts without a space; <see cref="GetTime(long)"/> shows negative numbers.
/// </para>
/// </summary>
public static class TimeUtil
{
    public const sbyte SECOND = 1;
    public const sbyte MINUTE = 2;
    public const sbyte HOUR = 3;
    public const sbyte DAY = 4;
    public const sbyte WEEK = 5;
    public const sbyte MONTH = 6;
    public const sbyte YEAR = 7;

    private const long MillisecondsPerMinute = 60_000L;
    private const long MillisecondsPerHour = 3_600_000L;
    private const long MillisecondsPerDay = 86_400_000L;

    /// <summary>Java <c>diffDate(Date, Date, byte)</c>: |d1 - d2| in the unit <paramref name="type"/> (0 for an unknown type). See the class remarks for the overflowing MONTH/YEAR constants.</summary>
    public static long DiffDate(long d1Milliseconds, long d2Milliseconds, sbyte type)
    {
        long diff = unchecked(d1Milliseconds - d2Milliseconds);
        long timeDiff = diff < 0 ? unchecked(-diff) : diff; // Math.abs(Long.MIN_VALUE) stays negative in Java
        return type switch
        {
            SECOND => timeDiff / 1000,
            MINUTE => timeDiff / (60 * 1000) % 60,
            HOUR => timeDiff / (60 * 60 * 1000) % 24,
            DAY => timeDiff / (24 * 60 * 60 * 1000),
            WEEK => timeDiff / (7 * 24 * 60 * 60 * 1000),
            MONTH => timeDiff / unchecked(30 * 24 * 60 * 60 * 1000), // int overflow: -1_702_967_296
            YEAR => timeDiff / unchecked(365 * 24 * 60 * 60 * 1000), // int overflow: 1_471_228_928
            _ => 0,
        };
    }

    /// <inheritdoc cref="DiffDate(long, long, sbyte)"/>
    public static long DiffDate(DateTimeOffset d1, DateTimeOffset d2, sbyte type) => DiffDate(d1.ToUnixTimeMilliseconds(), d2.ToUnixTimeMilliseconds(), type);

    /// <summary>
    /// Java <c>getTime(String time, String format)</c>: parses <paramref name="time"/> with the <c>SimpleDateFormat</c> <paramref name="format"/> in Vietnam time and returns the epoch milliseconds.
    /// </summary>
    /// <exception cref="InvalidTimeException">The text does not match (Java: <c>Exception("Thời gian không hợp lệ")</c>).</exception>
    /// <exception cref="ArgumentException">Illegal pattern character (Java: <c>IllegalArgumentException</c>, not caught by Java either).</exception>
    public static long GetTime(string time, string format)
    {
        if (!JavaDateFormat.TryParse(time, format, out long result))
        {
            throw new InvalidTimeException();
        }

        return result;
    }

    /// <summary>Java <c>getTime(long time)</c>: a duration in milliseconds shown as "m phút s giây", "h giờ m phút" or "d ngày h giờ".</summary>
    public static string GetTime(long time)
    {
        long seconds = time / 1000;
        long minutes = seconds / 60;
        long hours = minutes / 60;
        long days = hours / 24;

        if (seconds <= 0)
        {
            seconds = 0; // after minutes was computed: a negative time keeps its negative minutes
        }

        if (hours <= 0)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{minutes % 60} phút {seconds % 60} giây");
        }

        if (days <= 0)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{hours % 24} giờ {minutes % 60} phút");
        }

        return string.Create(CultureInfo.InvariantCulture, $"{days} ngày {hours % 24} giờ");
    }

    /// <summary>Java <c>convertTime(int)</c>: seconds shown as "d ngày h giờ m phút s giây", omitting zero parts (empty string for 0 or negative).</summary>
    public static string ConvertTime(int totalSeconds)
    {
        long days = totalSeconds / 86_400L;
        long hours = totalSeconds / 3600L % 24;
        long minutes = totalSeconds / 60L % 60;
        long seconds = totalSeconds % 60L;

        var result = new StringBuilder();
        if (days > 0)
        {
            result.Append(days.ToString(CultureInfo.InvariantCulture)).Append(" ngày ");
        }

        if (hours > 0)
        {
            result.Append(hours.ToString(CultureInfo.InvariantCulture)).Append(" giờ ");
        }

        if (minutes > 0)
        {
            result.Append(minutes.ToString(CultureInfo.InvariantCulture)).Append(" phút ");
        }

        if (seconds > 0)
        {
            result.Append(seconds.ToString(CultureInfo.InvariantCulture)).Append(" giây");
        }

        return result.ToString().Trim(); // Java String.trim(): removes chars <= U+0020; the pieces only add plain spaces
    }

    /// <summary>Java <c>formatTime(long time, String format)</c>: the instant formatted with a <c>SimpleDateFormat</c> pattern in Vietnam time.</summary>
    public static string FormatTime(long time, string format) => JavaDateFormat.Format(time, format);

    /// <summary>Java <c>formatTime(Date time, String format)</c>.</summary>
    public static string FormatTime(DateTimeOffset time, string format) => JavaDateFormat.Format(time.ToUnixTimeMilliseconds(), format);

    /// <summary>
    /// Java <c>getTimeAgo(int)</c>. Quirks kept: it compares with <c>&gt;</c> (60 seconds is still "1 phút", 3600 seconds is "60 phút"),
    /// the hours/days forms have NO space before the second number ("1 giờ1 phút", "1 ngày1 giờ") and hours only roll into days above 24.
    /// </summary>
    public static string GetTimeAgo(int timeRemainS)
    {
        int num = 0;
        if (timeRemainS > 60)
        {
            num = timeRemainS / 60;
        }

        int num2 = 0;
        if (num > 60)
        {
            num2 = num / 60;
            num %= 60;
        }

        int num3 = 0;
        if (num2 > 24)
        {
            num3 = num2 / 24;
            num2 %= 24;
        }

        string text = string.Empty;
        if (num3 > 0)
        {
            text += num3.ToString(CultureInfo.InvariantCulture);
            text += " ngày";
            text += num2.ToString(CultureInfo.InvariantCulture) + " giờ";
        }
        else if (num2 > 0)
        {
            text += num2.ToString(CultureInfo.InvariantCulture);
            text += " giờ";
            text += num.ToString(CultureInfo.InvariantCulture) + " phút";
        }
        else
        {
            if (num == 0)
            {
                num = 1;
            }

            text += num.ToString(CultureInfo.InvariantCulture);
            text += " phút";
        }

        return text;
    }

    /// <summary>Java <c>convertMillisecondToMinute(long)</c>: <c>String.format("%02d phút", minutes)</c> (a negative number gets no extra zero).</summary>
    public static string ConvertMillisecondToMinute(long time) => Pad2(time / MillisecondsPerMinute) + " phút";

    /// <summary>Java <c>convertMillisecondToHour(long)</c>: <c>"%02d giờ"</c>.</summary>
    public static string ConvertMillisecondToHour(long time) => Pad2(time / MillisecondsPerHour) + " giờ";

    /// <summary>Java <c>convertMillisecondToDay(long)</c>: <c>"%02d ngày"</c>.</summary>
    public static string ConvertMillisecondToDay(long time) => Pad2(time / MillisecondsPerDay) + " ngày";

    /// <summary>
    /// Java <c>getDeadline(long timeCreateAccount, long timeAgeDay)</c>: creation time plus days, shown as "HH:mm:ss dd/MM/yyyy" in <c>Asia/Ho_Chi_Minh</c>
    /// (a fixed zone in the Java code, so it never depended on the machine's zone).
    /// </summary>
    public static string GetDeadline(long timeCreateAccount, long timeAgeDay)
    {
        long deadlineMillis = unchecked(timeCreateAccount + (timeAgeDay * 24L * 60 * 60 * 1000));
        return JavaDateFormat.Format(deadlineMillis, "HH:mm:ss dd/MM/yyyy");
    }

    // String.format("%02d", value): minimum width 2 INCLUDING the minus sign, so -1 stays "-1".
    private static string Pad2(long value)
    {
        string digits = value.ToString(CultureInfo.InvariantCulture);
        return digits.Length >= 2 ? digits : "0" + digits;
    }
}
