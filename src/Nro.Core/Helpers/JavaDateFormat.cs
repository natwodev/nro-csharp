using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Nro.Core;

/// <summary>
/// Faithful subset of <c>java.text.SimpleDateFormat</c> (locale en-US, lenient, zone <c>Asia/Ho_Chi_Minh</c> = fixed +07:00, see <see cref="VietnamTime"/>),
/// used by <see cref="TimeUtil"/> (<c>formatTime</c>, <c>getTime(String, String)</c>, <c>getTimeNow</c>...). .NET custom formats cannot be used instead: they differ
/// (<c>MM</c> vs minutes, <c>/</c> and <c>:</c> are culture separators, <c>S</c> is a millisecond COUNT in Java, no leniency, no trailing-text rule).
/// <para>
/// FORMAT supports: <c>G y M L d k H m s S E D a h K Z X u</c>, quoted text and <c>''</c>. PARSE supports: <c>y M L d k H m s S a h K</c> and literals. A letter that is not a
/// valid Java pattern letter throws <see cref="ArgumentException"/> with the Java message (<c>Illegal pattern character 'b'</c>); a valid Java letter that is not
/// implemented here (<c>F w W z Y</c>, and for parsing also <c>G E D Z X u</c>) throws <see cref="NotSupportedException"/>, never a silently different result.
/// </para>
/// <para>
/// PARSE keeps the Java behaviour: lenient (month 13 rolls into next year, day 45 into next month), whitespace before a number is skipped, a number field is limited to the
/// pattern width when the next pattern element is another field (<c>yyyyMMddHHmmss</c>), text after the last pattern element is ignored, unset fields default to 1970-01-01 00:00:00.000.
/// DELIBERATE DIFFERENCES: dates use the proleptic Gregorian calendar (Java switches to Julian before 1582-10-15) and years below 1 are rejected; a two-digit year parsed by
/// <c>y</c>/<c>yy</c> throws <see cref="NotSupportedException"/> because Java resolves it against the current date (the golden file says never to use <c>yy</c>).
/// </para>
/// </summary>
public static class JavaDateFormat
{
    private const string JavaPatternLetters = "GyMdkHmsSEDFwWahKzZYuXL";
    private const string FormatLetters = "GyMLdkHmsSEDahKZXu";
    private const string ParseLetters = "yMLdkHmsSahK";
    private const long MillisecondsPerDay = 86_400_000L;

    private static readonly string[] MonthNames = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[..12];
    private static readonly string[] ShortMonthNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[..12];
    private static readonly string[] DayNames = CultureInfo.InvariantCulture.DateTimeFormat.DayNames;
    private static readonly string[] ShortDayNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames;
    private static readonly string[] AmPm = ["AM", "PM"];

    // A pattern element: Text != null means literal text, otherwise a field letter repeated Count times.
    private readonly record struct Element(char Letter, int Count, string? Text);

    /// <summary>Formats an instant (Java <c>Date.getTime()</c> milliseconds) in Vietnam time. Java: <c>new SimpleDateFormat(pattern).format(new Date(ms))</c>.</summary>
    public static string Format(long epochMilliseconds, string pattern) => Format(epochMilliseconds, pattern, VietnamTime.Offset);

    /// <summary>Formats an instant in the time zone with the given fixed UTC offset (used by tests to prove the result does not depend on the machine's zone).</summary>
    public static string Format(long epochMilliseconds, string pattern, TimeSpan utcOffset)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        List<Element> elements = Compile(pattern, FormatLetters, "format");

        long offsetMs = (long)utcOffset.TotalMilliseconds;
        long local = unchecked(epochMilliseconds + offsetMs);
        long days = FloorDiv(local, MillisecondsPerDay);
        long msOfDay = local - (days * MillisecondsPerDay);
        (long year, int month, int day) = CivilFromDays(days);
        int hour = (int)(msOfDay / 3_600_000);
        int minute = (int)(msOfDay / 60_000 % 60);
        int second = (int)(msOfDay / 1000 % 60);
        int millis = (int)(msOfDay % 1000);
        int dayOfWeek = (int)FloorMod(days + 4, 7); // 1970-01-01 was a Thursday; 0 = Sunday

        var sb = new StringBuilder();
        foreach (Element e in CollectionsMarshal.AsSpan(elements))
        {
            if (e.Text is not null)
            {
                sb.Append(e.Text);
                continue;
            }

            switch (e.Letter)
            {
                case 'G':
                    sb.Append("AD");
                    break;
                case 'y':
                    sb.Append(e.Count == 2 ? ZeroPad(FloorMod(year, 100), 2) : ZeroPad(year, e.Count));
                    break;
                case 'M':
                case 'L':
                    sb.Append(e.Count >= 4 ? MonthNames[month - 1] : e.Count == 3 ? ShortMonthNames[month - 1] : ZeroPad(month, e.Count));
                    break;
                case 'd':
                    sb.Append(ZeroPad(day, e.Count));
                    break;
                case 'k':
                    sb.Append(ZeroPad(hour == 0 ? 24 : hour, e.Count));
                    break;
                case 'H':
                    sb.Append(ZeroPad(hour, e.Count));
                    break;
                case 'm':
                    sb.Append(ZeroPad(minute, e.Count));
                    break;
                case 's':
                    sb.Append(ZeroPad(second, e.Count));
                    break;
                case 'S':
                    sb.Append(ZeroPad(millis, e.Count));
                    break;
                case 'E':
                    sb.Append(e.Count >= 4 ? DayNames[dayOfWeek] : ShortDayNames[dayOfWeek]);
                    break;
                case 'D':
                    sb.Append(ZeroPad(days - DaysFromCivil(year, 1, 1) + 1, e.Count));
                    break;
                case 'a':
                    sb.Append(AmPm[hour < 12 ? 0 : 1]);
                    break;
                case 'h':
                    sb.Append(ZeroPad(hour % 12 == 0 ? 12 : hour % 12, e.Count));
                    break;
                case 'K':
                    sb.Append(ZeroPad(hour % 12, e.Count));
                    break;
                case 'u':
                    sb.Append(ZeroPad(dayOfWeek == 0 ? 7 : dayOfWeek, e.Count));
                    break;
                case 'Z':
                    AppendOffset(sb, offsetMs, 2, colon: false, zForZero: false);
                    break;
                case 'X':
                    AppendOffset(sb, offsetMs, e.Count, colon: e.Count == 3, zForZero: true);
                    break;
                default:
                    throw new NotSupportedException($"Pattern letter '{e.Letter}' is not supported");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Java: <c>new SimpleDateFormat(pattern).parse(text).getTime()</c> in Vietnam time. Returns <c>false</c> where Java throws <c>ParseException</c>;
    /// an invalid pattern still throws (<see cref="ArgumentException"/>) like Java does when the formatter is constructed.
    /// </summary>
    public static bool TryParse(string text, string pattern, out long epochMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(pattern);
        List<Element> elements = Compile(pattern, ParseLetters, "parse");
        epochMilliseconds = 0;

        long? year = null;
        int month = 0;
        int day = 1;
        int? hourOfDay = null;
        int? hour12 = null;
        int amPm = 0;
        int minute = 0;
        int second = 0;
        int millis = 0;

        int start = 0;
        for (int i = 0; i < elements.Count; i++)
        {
            Element e = elements[i];
            if (e.Text is not null)
            {
                if (start + e.Text.Length > text.Length || string.CompareOrdinal(text, start, e.Text, 0, e.Text.Length) != 0)
                {
                    return false;
                }

                start += e.Text.Length;
                continue;
            }

            // Java: the width of a number is limited to the pattern width when the next pattern element is a field (contiguous digits like yyyyMMdd).
            bool obeyCount = i + 1 < elements.Count && elements[i + 1].Text is null;

            int pos = start;
            while (true)
            {
                if (pos >= text.Length)
                {
                    return false;
                }

                if (text[pos] != ' ' && text[pos] != '\t')
                {
                    break;
                }

                pos++;
            }

            bool textMonth = (e.Letter is 'M' or 'L') && e.Count >= 3;
            if (e.Letter == 'a' || textMonth)
            {
                int end;
                int index;
                if (e.Letter == 'a')
                {
                    end = MatchString(text, pos, AmPm, out index);
                    if (end < 0)
                    {
                        return false;
                    }

                    amPm = index;
                }
                else
                {
                    end = MatchString(text, pos, MonthNames, out index);
                    if (end < 0)
                    {
                        end = MatchString(text, pos, ShortMonthNames, out index);
                    }

                    if (end < 0)
                    {
                        return false;
                    }

                    month = index;
                }

                start = end;
                continue;
            }

            int limit = text.Length;
            if (obeyCount)
            {
                if (start + e.Count > text.Length)
                {
                    return false;
                }

                limit = start + e.Count;
            }

            if (!TryParseInt(text, pos, limit, out int value, out int numberEnd))
            {
                return false;
            }

            switch (e.Letter)
            {
                case 'y':
                    if (e.Count <= 2 && numberEnd - pos == 2 && char.IsAsciiDigit(text[pos]) && char.IsAsciiDigit(text[pos + 1]))
                    {
                        throw new NotSupportedException("Parsing a two-digit year depends on the current date in Java (SimpleDateFormat century window); use 'yyyy'");
                    }

                    year = value;
                    break;
                case 'M':
                case 'L':
                    month = value - 1;
                    break;
                case 'd':
                    day = value;
                    break;
                case 'H':
                    hourOfDay = value;
                    break;
                case 'k':
                    hourOfDay = value == 24 ? 0 : value;
                    break;
                case 'h':
                    hour12 = value == 12 ? 0 : value;
                    break;
                case 'K':
                    hour12 = value;
                    break;
                case 'm':
                    minute = value;
                    break;
                case 's':
                    second = value;
                    break;
                case 'S':
                    millis = value;
                    break;
                default:
                    throw new NotSupportedException($"Pattern letter '{e.Letter}' is not supported for parsing");
            }

            start = numberEnd;
        }

        if (start == 0)
        {
            return false; // java.text.DateFormat.parse(String): position 0 means failure
        }

        long yearValue = year ?? 1970;
        long totalMonths = (yearValue * 12) + month;
        long normalizedYear = FloorDiv(totalMonths, 12);
        if (normalizedYear < 1)
        {
            return false;
        }

        int monthIndex = (int)(totalMonths - (normalizedYear * 12));
        long hours = hourOfDay ?? (hour12.GetValueOrDefault() + (12L * amPm));
        long days = DaysFromCivil(normalizedYear, monthIndex + 1, 1) + (day - 1L);
        long local = (days * MillisecondsPerDay) + (hours * 3_600_000L) + (minute * 60_000L) + (second * 1000L) + millis;
        epochMilliseconds = local - VietnamTime.OffsetMilliseconds;
        return true;
    }

    // ---- pattern compiler (java.text.SimpleDateFormat.compile) ----

    private static List<Element> Compile(string pattern, string supportedLetters, string operation)
    {
        var elements = new List<Element>();
        var literal = new StringBuilder();

        void FlushLiteral()
        {
            if (literal.Length > 0)
            {
                elements.Add(new Element(' ', 0, literal.ToString()));
                literal.Clear();
            }
        }

        int i = 0;
        while (i < pattern.Length)
        {
            char c = pattern[i];
            if (char.IsAsciiLetter(c))
            {
                int count = 1;
                while (i + count < pattern.Length && pattern[i + count] == c)
                {
                    count++;
                }

                if (!JavaPatternLetters.Contains(c))
                {
                    throw new ArgumentException($"Illegal pattern character '{c}'");
                }

                if (c == 'X' && count > 3)
                {
                    throw new ArgumentException($"invalid ISO 8601 format: length={count}");
                }

                if (!supportedLetters.Contains(c))
                {
                    throw new NotSupportedException($"Pattern letter '{c}' is not supported for {operation}");
                }

                FlushLiteral();
                elements.Add(new Element(c, count, null));
                i += count;
            }
            else if (c == '\'')
            {
                if (i + 1 < pattern.Length && pattern[i + 1] == '\'')
                {
                    literal.Append('\'');
                    i += 2;
                    continue;
                }

                i++;
                bool closed = false;
                while (i < pattern.Length)
                {
                    char q = pattern[i];
                    if (q == '\'')
                    {
                        if (i + 1 < pattern.Length && pattern[i + 1] == '\'')
                        {
                            literal.Append('\'');
                            i += 2;
                            continue;
                        }

                        closed = true;
                        i++;
                        break;
                    }

                    literal.Append(q);
                    i++;
                }

                if (!closed)
                {
                    throw new ArgumentException("Unterminated quote");
                }
            }
            else
            {
                literal.Append(c);
                i++;
            }
        }

        FlushLiteral();
        return elements;
    }

    // ---- helpers ----

    private static string ZeroPad(long value, int minDigits)
    {
        string digits = value.ToString(CultureInfo.InvariantCulture);
        if (value >= 0)
        {
            return digits.PadLeft(minDigits, '0');
        }

        return "-" + digits[1..].PadLeft(minDigits, '0');
    }

    private static void AppendOffset(StringBuilder sb, long offsetMs, int count, bool colon, bool zForZero)
    {
        if (offsetMs == 0 && zForZero)
        {
            sb.Append('Z');
            return;
        }

        long minutes = offsetMs / 60_000;
        sb.Append(minutes < 0 ? '-' : '+');
        minutes = Math.Abs(minutes);
        sb.Append(ZeroPad(minutes / 60, 2));
        if (count == 1)
        {
            return; // ISO 8601 hour only
        }

        if (colon)
        {
            sb.Append(':');
        }

        sb.Append(ZeroPad(minutes % 60, 2));
    }

    // java.text.DecimalFormat with parseIntegerOnly: optional '-', then digits; stops at the first other char or at the limit.
    private static bool TryParseInt(string text, int from, int limit, out int value, out int end)
    {
        value = 0;
        end = from;
        int i = from;
        bool negative = false;
        if (i < limit && text[i] == '-')
        {
            negative = true;
            i++;
        }

        int digitsStart = i;
        long acc = 0;
        while (i < limit && char.IsAsciiDigit(text[i]))
        {
            if (i - digitsStart >= 18)
            {
                return false;
            }

            acc = (acc * 10) + (text[i] - '0');
            i++;
        }

        if (i == digitsStart)
        {
            return false;
        }

        value = unchecked((int)(negative ? -acc : acc));
        end = i;
        return true;
    }

    // SimpleDateFormat.matchString: the longest name that matches at 'start' ignoring case; returns the end position or -1.
    private static int MatchString(string text, int start, string[] names, out int index)
    {
        index = -1;
        int bestLength = 0;
        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];
            if (name.Length > bestLength
                && start + name.Length <= text.Length
                && string.Compare(text, start, name, 0, name.Length, StringComparison.OrdinalIgnoreCase) == 0)
            {
                bestLength = name.Length;
                index = i;
            }
        }

        return index < 0 ? -1 : start + bestLength;
    }

    private static long FloorDiv(long a, long b)
    {
        long q = a / b;
        return (a % b != 0 && (a < 0) != (b < 0)) ? q - 1 : q;
    }

    private static long FloorMod(long a, long b) => a - (FloorDiv(a, b) * b);

    // Howard Hinnant's civil-date algorithms (proleptic Gregorian, any year).
    private static long DaysFromCivil(long year, int month, int day)
    {
        long y = month <= 2 ? year - 1 : year;
        long era = FloorDiv(y, 400);
        long yoe = y - (era * 400);
        long doy = (((153L * (month > 2 ? month - 3 : month + 9)) + 2) / 5) + day - 1;
        long doe = (yoe * 365) + (yoe / 4) - (yoe / 100) + doy;
        return (era * 146_097) + doe - 719_468;
    }

    private static (long Year, int Month, int Day) CivilFromDays(long days)
    {
        long z = days + 719_468;
        long era = FloorDiv(z, 146_097);
        long doe = z - (era * 146_097);
        long yoe = (doe - (doe / 1460) + (doe / 36_524) - (doe / 146_096)) / 365;
        long y = yoe + (era * 400);
        long doy = doe - ((365 * yoe) + (yoe / 4) - (yoe / 100));
        long mp = ((5 * doy) + 2) / 153;
        int d = (int)(doy - (((153 * mp) + 2) / 5) + 1);
        int m = (int)(mp < 10 ? mp + 3 : mp - 9);
        return (m <= 2 ? y + 1 : y, m, d);
    }
}
