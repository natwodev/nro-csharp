using System.Globalization;
using System.Text;

using Nro.Core.Consts;

namespace Nro.Core;

/// <summary>
/// Port of the PURE part of <c>Utils/Util.java</c>: every function that depends only on its arguments (checked case by case against <c>tests/golden/util-pure.json</c>).
/// The random functions are in <see cref="RandomUtil"/> (they need an <see cref="Interfaces.IRandomProvider"/>); the functions that read the clock, create items or take a
/// <c>Player</c>/<c>Zone</c>/<c>Mob</c> are not here yet.
/// <para>
/// Everything that Java formats through the default locale or through <c>NumberFormat</c>/<c>DecimalFormat</c>/<c>String.format</c> is reproduced explicitly with the invariant
/// culture (<see cref="JavaNumberFormat"/>): the Java server formats numbers with the vi-VN symbols (<c>1.234,5</c>) whatever the machine setting is, and so does this class.
/// Dates are shown in Vietnam time (fixed +07:00, <see cref="VietnamTime"/>); a Java <c>Date</c> is an instant given as epoch milliseconds, with a <see cref="DateTimeOffset"/> overload.
/// </para>
/// <para>
/// Java quirks that clients and saved data depend on are kept on purpose (int overflow in <see cref="CreateIdBossClone"/>, <see cref="GetDistance(int, int, int, int)"/>,
/// <see cref="FormatTimeHMS"/> with negative input, <see cref="GetPercent"/> dividing before multiplying, the shared number format of <see cref="Format(double)"/> that
/// <see cref="FormatToSecondMinute"/> changes...). Deliberate differences are listed on the individual members.
/// </para>
/// </summary>
public static class Util
{
    private const string DateTimePattern = "yyyy-MM-dd HH:mm:ss";
    private const string FallbackDateString = "2025-01-01 01:01:00";
    private const int DefaultMaxFractionDigits = 3;
    private const long MillisecondsPerDay = 86_400_000L;

    // Java: the 'char[] SOURCE_CHARACTERS' / 'DESTINATION_CHARACTERS' tables, sorted by code point so that Arrays.binarySearch works. Copied from the Java source by a script.
    private static readonly char[] SourceCharacters = "ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝàáâãèéêìíòóôõùúýĂăĐđĨĩŨũƠơƯưẠạẢảẤấẦầẨẩẪẫẬậẮắẰằẲẳẴẵẶặẸẹẺẻẼẽẾếỀềỂểỄễỆệỈỉỊịỌọỎỏỐốỒồỔổỖỗỘộỚớỜờỞởỠỡỢợỤụỦủỨứỪừỬửỮữỰự".ToCharArray();

    private static readonly char[] DestinationCharacters = "AAAAEEEIIOOOOUUYaaaaeeeiioooouuyAaDdIiUuOoUuAaAaAaAaAaAaAaAaAaAaAaAaEeEeEeEeEeEeEeEeIiIiOoOoOoOoOoOoOoOoOoOoOoOoUuUuUuUuUuUuUu".ToCharArray();

    // Java: the static 'NumberFormat num = NumberFormat.getInstance(vi-VN)'. Its maximum fraction digits start at 3 and FormatToSecondMinute sets them to 1 for good.
    private static int numberFormatMaxFractionDigits = DefaultMaxFractionDigits;

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Identity, text
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>Java <c>createIdBossClone(idPlayer)</c>: <c>-idPlayer - 1_000_000_000</c> in <c>int</c> arithmetic (wraps at the ends: <c>int.MaxValue</c> gives 1147483649).</summary>
    public static int CreateIdBossClone(int idPlayer) => unchecked(-idPlayer - 1_000_000_000);

    /// <summary>
    /// Java <c>contains(String[] arr, String key)</c>: <c>Arrays.toString(arr).contains(key)</c>, i.e. a SUBSTRING test on the text <c>[a, b]</c> (so <c>bc</c>, <c>c, d</c> and <c>[</c> match
    /// <c>["abc","def"]</c>), a null array is the text <c>null</c> and a null element the text <c>null</c>. A null <paramref name="key"/> throws (Java <c>NullPointerException</c>).
    /// </summary>
    public static bool Contains(string?[]? arr, string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        string text = arr is null ? "null" : "[" + string.Join(", ", arr.Select(item => item ?? "null")) + "]";
        return text.Contains(key, StringComparison.Ordinal);
    }

    /// <summary>
    /// Java <c>replace(text, regex, replacement)</c>: <c>String.replace(CharSequence, CharSequence)</c>, a LITERAL replacement (the parameter called <c>regex</c> is no regular expression).
    /// An empty <paramref name="regex"/> matches between every character (<c>"abc"</c> with <c>-</c> gives <c>-a-b-c-</c>), which <c>string.Replace</c> in .NET refuses.
    /// </summary>
    public static string Replace(string text, string regex, string replacement)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(regex);
        ArgumentNullException.ThrowIfNull(replacement);
        if (regex.Length > 0)
        {
            return text.Replace(regex, replacement, StringComparison.Ordinal);
        }

        var sb = new StringBuilder(text.Length + ((text.Length + 1) * replacement.Length));
        sb.Append(replacement);
        foreach (char c in text)
        {
            sb.Append(c).Append(replacement);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Java <c>haveSpecialCharacter(text)</c>: true when <paramref name="text"/> has any character other than the ASCII letters and digits. (Java tests the regex <c>[^a-z0-9 ]</c>,
    /// case insensitive, and then also whether the text contains a space, so a space counts as special; accented letters, the Kelvin sign U+212A and every other non-ASCII character too.)
    /// </summary>
    public static bool HaveSpecialCharacter(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        foreach (char c in text)
        {
            if (!char.IsAsciiLetterOrDigit(c))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Java <c>removeAccent(char)</c>: maps the 126 precomposed Vietnamese letters of the Java table to their base letter (binary search in the sorted table). Other characters are returned
    /// unchanged, including the accented y of U+1EF2..U+1EF9 (missing from the Java table) and the separate combining marks of decomposed text.
    /// </summary>
    public static char RemoveAccent(char ch)
    {
        int index = Array.BinarySearch(SourceCharacters, ch);
        return index >= 0 ? DestinationCharacters[index] : ch;
    }

    /// <summary>Java <c>removeAccent(String)</c>: <see cref="RemoveAccent(char)"/> on every UTF-16 unit. A null string throws (Java <c>NullPointerException</c>).</summary>
    public static string RemoveAccent(string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        return string.Create(str.Length, str, static (span, source) =>
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = RemoveAccent(source[i]);
            }
        });
    }

    /// <summary>
    /// Java <c>addArray(Object[]... arrays)</c>: null or no array gives null; one array gives THAT array (not a copy, it may be null); several are joined in order with
    /// commons-lang <c>ArrayUtils.addAll</c>, which treats a null array as empty and gives null only when both sides are null.
    /// </summary>
    public static object?[]? AddArray(params object?[]?[]? arrays)
    {
        if (arrays is null || arrays.Length == 0)
        {
            return null;
        }

        if (arrays.Length == 1)
        {
            return arrays[0];
        }

        object?[]? joined = arrays[0];
        for (int i = 1; i < arrays.Length; i++)
        {
            joined = AddAll(joined, arrays[i]);
        }

        return joined;
    }

    /// <summary>
    /// Java <c>addSlashes(input)</c>: puts a backslash before backslash, apostrophe and double quote and turns backspace, line feed, carriage return and tab into <c>\b \n \r \t</c>
    /// (replaced in that order, backslash first). A null string throws (Java <c>NullPointerException</c>).
    /// </summary>
    public static string AddSlashes(string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        input = input.Replace("\\", "\\\\", StringComparison.Ordinal);
        input = input.Replace("'", "\\'", StringComparison.Ordinal);
        input = input.Replace("\"", "\\\"", StringComparison.Ordinal);
        input = input.Replace("\b", "\\b", StringComparison.Ordinal);
        input = input.Replace("\n", "\\n", StringComparison.Ordinal);
        input = input.Replace("\r", "\\r", StringComparison.Ordinal);
        input = input.Replace("\t", "\\t", StringComparison.Ordinal);
        return input;
    }

    /// <summary>
    /// Java <c>strSQL(str)</c>: puts a backslash before each of <c>'</c> <c>"</c> <c>\</c> <c>%</c> (<c>replaceAll("['\"\\\\%]", "\\\\$0")</c>). This is NOT a safe way to build SQL;
    /// the C# data layer uses parameters. A null string throws (Java <c>NullPointerException</c>).
    /// </summary>
    public static string StrSql(string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        var sb = new StringBuilder(str.Length + 8);
        foreach (char c in str)
        {
            if (c is '\'' or '"' or '\\' or '%')
            {
                sb.Append('\\');
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Java <c>isNumeric(str)</c>: <c>Long.valueOf(str)</c> succeeds. Null and empty give false; an optional leading <c>+</c> or <c>-</c> is allowed; the digits may be any Unicode decimal digit
    /// (Arabic-Indic, full-width...); spaces, a decimal point and values outside the <c>long</c> range give false.
    /// </summary>
    public static bool IsNumeric(string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return false;
        }

        // Port of Long.parseLong(s, 10): the value is accumulated as a negative number so that Long.MIN_VALUE is accepted.
        long result = 0;
        long limit = -long.MaxValue;
        int i = 0;
        char first = str[0];
        if (first < '0')
        {
            if (first == '-')
            {
                limit = long.MinValue;
            }
            else if (first != '+')
            {
                return false;
            }

            if (str.Length == 1)
            {
                return false;
            }

            i++;
        }

        long multiplicationMinimum = limit / 10;
        while (i < str.Length)
        {
            int digit = JavaDigit(str[i++]);
            if (digit < 0 || result < multiplicationMinimum)
            {
                return false;
            }

            result *= 10;
            if (result < limit + digit)
            {
                return false;
            }

            result -= digit;
        }

        return true;
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Number formatting (Java NumberFormat / DecimalFormat / String.format with explicit vi-VN symbols)
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Java <c>formatTimeHMS(totalSeconds)</c>: <c>hh:mm:ss</c> by <c>String.format("%02d:%02d:%02d")</c>; hours are not limited to 24 (<c>100:00:00</c>) and negative input gives negative parts
    /// (<c>-3661</c> is <c>-1:-1:-1</c>).
    /// </summary>
    public static string FormatTimeHMS(int totalSeconds)
    {
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        return JavaNumbers.FormatZeroPadded(hours, 2) + ":" + JavaNumbers.FormatZeroPadded(minutes, 2) + ":" + JavaNumbers.FormatZeroPadded(seconds, 2);
    }

    /// <summary>
    /// Java <c>formatNumber(power, style)</c>: scales by 1e9, 1e6 and 1e3 in turn, printing at most one fraction digit CUT (not rounded) with the vi-VN symbols, and appends the
    /// suffixes glued together without a space (<c>1e12</c> is <c>1 NghìnTỷ</c> or <c>1 KB</c>); numbers below 1000 and all negative numbers get no suffix (<c>-1500</c> is <c>-1.500</c>).
    /// DELIBERATE DIFFERENCE: <c>+Infinity</c> makes the Java loop run forever; here it returns <c>∞</c>.
    /// </summary>
    public static string FormatNumber(double power, FormatStyle style)
    {
        if (double.IsPositiveInfinity(power))
        {
            return "∞";
        }

        string suffix = string.Empty;
        while (power >= 1_000_000_000)
        {
            power /= 1_000_000_000;
            suffix = (style == FormatStyle.KMB ? "B" : "Tỷ") + suffix;
        }

        while (power >= 1_000_000)
        {
            power /= 1_000_000;
            suffix = (style == FormatStyle.KMB ? "M" : "Triệu") + suffix;
        }

        while (power >= 1_000)
        {
            power /= 1_000;
            suffix = (style == FormatStyle.KMB ? "K" : "Nghìn") + suffix;
        }

        string number = JavaNumberFormat.FormatDouble(power, 1, 0, JavaRoundingMode.Down, JavaNumberSymbols.ViVn);
        return suffix.Length == 0 ? number : number + " " + suffix;
    }

    /// <summary>
    /// Java <c>numberToMoney(power)</c>: <c>Tỷ</c> from 1e9, <c>Tr</c> from 1e6, <c>k</c> from 1000 with at most one fraction digit rounded HALF_EVEN on the exact value
    /// (<c>1250</c> is <c>1,2 k</c>, <c>1350</c> is <c>1,4 k</c>, <c>999999</c> is <c>1.000 k</c>); below 1000 and for every negative number the plain grouped integer.
    /// </summary>
    public static string NumberToMoney(long power)
    {
        if (power >= 1000000000)
        {
            return JavaNumberFormat.FormatDouble((double)power / 1000000000, 1, 0, JavaRoundingMode.HalfEven, JavaNumberSymbols.ViVn) + " Tỷ";
        }

        if (power >= 1000000)
        {
            return JavaNumberFormat.FormatDouble((double)power / 1000000, 1, 0, JavaRoundingMode.HalfEven, JavaNumberSymbols.ViVn) + " Tr";
        }

        if (power >= 1000)
        {
            return JavaNumberFormat.FormatDouble((double)power / 1000, 1, 0, JavaRoundingMode.HalfEven, JavaNumberSymbols.ViVn) + " k";
        }

        return JavaNumberFormat.FormatLong(power, JavaNumberSymbols.ViVn);
    }

    /// <summary>
    /// Java <c>roundToTwoDecimals(num)</c> with the DEFAULT locale of the reference JVM (en-US): <c>Math.round(num * 100) / 100</c> (half up, saturating at the <c>long</c> range) printed
    /// by <c>DecimalFormat("#,##0.00")</c>, e.g. <c>1,234.57</c>. The Java result depends on the JVM's default locale; use <see cref="RoundToTwoDecimalsViVn"/> for <c>1.234,57</c>.
    /// </summary>
    public static string RoundToTwoDecimals(double num) => RoundToTwoDecimals(num, JavaNumberSymbols.EnUs);

    /// <summary>The same as <see cref="RoundToTwoDecimals(double)"/> when the JVM's default locale is vi-VN (golden field <c>resultWhenDefaultLocaleViVN</c>): <c>1.234,57</c>.</summary>
    public static string RoundToTwoDecimalsViVn(double num) => RoundToTwoDecimals(num, JavaNumberSymbols.ViVn);

    /// <summary>
    /// Java <c>format(double)</c>: <c>NumberFormat.getInstance(vi-VN).format(power)</c> on the SHARED static format, HALF_EVEN, at most
    /// <see cref="NumberFormatMaxFractionDigits"/> fraction digits (3 until <see cref="FormatToSecondMinute"/> is called for the first time, 1 afterwards).
    /// </summary>
    public static string Format(double power) => JavaNumberFormat.FormatDouble(power, NumberFormatMaxFractionDigits, 0, JavaRoundingMode.HalfEven, JavaNumberSymbols.ViVn);

    /// <summary>Java <c>format(int)</c>: the integer grouped with dots (<c>1.234.567</c>); not affected by <see cref="NumberFormatMaxFractionDigits"/>.</summary>
    public static string Format(int power) => JavaNumberFormat.FormatLong(power, JavaNumberSymbols.ViVn);

    /// <summary>Java <c>format(long)</c>: the integer grouped with dots; not affected by <see cref="NumberFormatMaxFractionDigits"/>.</summary>
    public static string Format(long power) => JavaNumberFormat.FormatLong(power, JavaNumberSymbols.ViVn);

    /// <summary>
    /// Java <c>formatToSecond_Minute(power)</c>: from 60 on, <c>power / 60</c> with one fraction digit and <c>phút</c> (<c>75</c> is <c>1,2 phút</c>); below 60 (and negative) the plain number.
    /// KEPT JAVA BUG: it calls <c>num.setMaximumFractionDigits(1)</c> on the format SHARED with <see cref="Format(double)"/>, so after the first call every <c>Format(double)</c>
    /// shows one fraction digit instead of three, for the rest of the process (the golden file tests both phases).
    /// </summary>
    public static string FormatToSecondMinute(long power)
    {
        Volatile.Write(ref numberFormatMaxFractionDigits, 1);
        if (power >= 60)
        {
            return JavaNumberFormat.FormatDouble((double)power / 60, 1, 0, JavaRoundingMode.HalfEven, JavaNumberSymbols.ViVn) + " phút";
        }

        return JavaNumberFormat.FormatLong(power, JavaNumberSymbols.ViVn);
    }

    /// <summary>The maximum fraction digits of the shared number format behind <see cref="Format(double)"/> (see <see cref="FormatToSecondMinute"/>).</summary>
    public static int NumberFormatMaxFractionDigits => Volatile.Read(ref numberFormatMaxFractionDigits);

    /// <summary>Puts the shared number format back to its start-up state (3 fraction digits). Java has no such method; it exists so that tests do not depend on each other.</summary>
    public static void ResetSharedNumberFormat() => Volatile.Write(ref numberFormatMaxFractionDigits, DefaultMaxFractionDigits);

    /// <summary>
    /// Java <c>formatBytes(bytes)</c>: <c>N bytes</c>, <c>N KB</c>, <c>N MB</c> or <c>N GB</c> with the integer quotient (truncated, never rounded); negative numbers are always <c>bytes</c>.
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        string text;
        if (bytes < 1024)
        {
            text = bytes.ToString(CultureInfo.InvariantCulture) + " bytes";
        }
        else if (bytes < 1024 * 1024)
        {
            text = (bytes / 1024).ToString(CultureInfo.InvariantCulture) + " KB";
        }
        else if (bytes < 1024 * 1024 * 1024)
        {
            text = (bytes / (1024 * 1024)).ToString(CultureInfo.InvariantCulture) + " MB";
        }
        else
        {
            text = (bytes / (1024 * 1024 * 1024)).ToString(CultureInfo.InvariantCulture) + " GB";
        }

        return text;
    }

    /// <summary>
    /// Java <c>convertMillisecondsDouble(ms)</c>: seconds with two decimals and a comma, <c>String.format("%.2f", ms / 1000.0)</c> with <c>.</c> replaced by <c>,</c>. Java rounds HALF_UP on the
    /// shortest decimal text of the double (<c>1005</c> ms is <c>1,01</c>); .NET <c>ToString("F2")</c> would give <c>1,00</c>.
    /// </summary>
    public static string ConvertMillisecondsDouble(long milliseconds)
    {
        double seconds = (double)milliseconds / 1000;
        return JavaNumberFormat.FormatFixed(seconds, 2).Replace('.', ',');
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Arithmetic
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Java <c>getDistance(x1, y1, x2, y2)</c>: <c>(int)Math.sqrt((x1-x2)^2 + (y1-y2)^2)</c>. The differences are computed in <c>int</c> and wrap on overflow
    /// (<c>2000000000</c> versus <c>-2000000000</c> gives 294967296); the result is cut, not rounded, and saturates at <c>int.MaxValue</c>.
    /// </summary>
    public static int GetDistance(int x1, int y1, int x2, int y2)
    {
        double dx = unchecked(x1 - x2);
        double dy = unchecked(y1 - y2);
        return JavaNumbers.ToInt(Math.Sqrt((dx * dx) + (dy * dy)));
    }

    /// <summary>Java <c>getPercent(value, percent)</c>: <c>value / 100 * percent</c> in <c>long</c>; the division comes FIRST and is cut, so <c>getPercent(199, 50)</c> is 50, not 99.</summary>
    public static long GetPercent(long value, int percent) => unchecked(value / 100 * percent);

    /// <summary>
    /// Java <c>CrisGH(a)</c> with <c>Manager.readInt</c> passed as <paramref name="readInt"/>: when set, <paramref name="a"/> is limited to <c>int.MaxValue</c> and cut to an <c>int</c>
    /// (saturating at <c>int.MinValue</c> below, NaN gives 0); otherwise it is cut to a <c>long</c> (saturating, NaN gives 0).
    /// </summary>
    public static long CrisGH(double a, bool readInt)
    {
        if (readInt)
        {
            if (a > int.MaxValue)
            {
                a = int.MaxValue;
            }

            return JavaNumbers.ToInt(a);
        }

        return JavaNumbers.ToLong(a);
    }

    /// <summary>Java <c>highlightsItem(highlights, value)</c>: <c>(int)(value * 1.1)</c> when <paramref name="highlights"/>, else <paramref name="value"/>. Cut toward zero, saturating at the <c>int</c> range.</summary>
    public static int HighlightsItem(bool highlights, int value)
    {
        const double highlightsNumber = 1.1;
        return highlights ? JavaNumbers.ToInt(value * highlightsNumber) : value;
    }

    /// <summary>
    /// Java <c>tinhLuyThua(coSo, soMu)</c>: <c>coSo</c> to the power <c>soMu</c> by repeated multiplication in <c>long</c>, WITHOUT overflow check (wraps at 64 bits); an exponent of 0 or less gives 1.
    /// </summary>
    public static long TinhLuyThua(int coSo, int soMu)
    {
        long ketQua = 1;
        for (int i = 0; i < soMu; i++)
        {
            ketQua = unchecked(ketQua * coSo);
        }

        return ketQua;
    }

    /// <summary>
    /// Java <c>checkDo(itemOption)</c> for an option with id <paramref name="optionId"/> and value <paramref name="param"/> (the Java method reads <c>itemOption.optionTemplate.id</c> and
    /// <c>itemOption.param</c>): false when the value is above the cheat limit of that option, true otherwise (an unknown option id is always true; negative values pass).
    /// </summary>
    public static bool CheckDo(int optionId, int param)
    {
        switch (optionId)
        {
            case 0: // attack
                return param <= 12000;
            case 14: // critical
                return param <= 30;
            case 107: // star
            case 102:
                return param <= 8;
            case 77:
            case 103:
            case 95:
            case 96:
                return param <= 41;
            case 50:
                return param <= 24;
            case 6: // hp
            case 7: // ki
                return param <= 120000;
            case 47: // armor
                return param <= 3500;
            default:
                return true;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Time text
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>Java <c>convertSecondsToTime(totalSeconds)</c>: <c>Ns trước</c> (under a minute), <c>Np trước</c>, <c>Ng trước</c> (hours), <c>Nn trước</c> (days); the unit is cut, never rounded.</summary>
    public static string ConvertSecondsToTime(long totalSeconds)
    {
        if (totalSeconds < 60)
        {
            return totalSeconds.ToString(CultureInfo.InvariantCulture) + "s trước";
        }

        if (totalSeconds < 3600)
        {
            return (totalSeconds / 60).ToString(CultureInfo.InvariantCulture) + "p" + " trước";
        }

        if (totalSeconds < 86400)
        {
            return (totalSeconds / 3600).ToString(CultureInfo.InvariantCulture) + "g" + " trước";
        }

        return (totalSeconds / 86400).ToString(CultureInfo.InvariantCulture) + "n" + " trước";
    }

    /// <summary>Java <c>convertMilliseconds(ms)</c>: <c>hh:mm:ss</c> with unlimited hours (<c>String.format("%02d:%02d:%02d")</c>); negative input gives negative parts.</summary>
    public static string ConvertMilliseconds(long ms)
    {
        long seconds = ms / 1000 % 60;
        long minutes = ms / (1000 * 60) % 60;
        long hours = ms / (1000 * 60 * 60);
        return JavaNumbers.FormatZeroPadded(hours, 2) + ":" + JavaNumbers.FormatZeroPadded(minutes, 2) + ":" + JavaNumbers.FormatZeroPadded(seconds, 2);
    }

    /// <summary>Java <c>convertMilliseconds_ms(milliseconds)</c>: <c>M m S s</c> (whole minutes, then the seconds left, e.g. <c>1 m 1 s</c>).</summary>
    public static string ConvertMillisecondsMs(long milliseconds)
    {
        long seconds = milliseconds / 1000;
        long minutes = seconds / 60;
        seconds %= 60;
        return minutes.ToString(CultureInfo.InvariantCulture) + " m " + seconds.ToString(CultureInfo.InvariantCulture) + " s";
    }

    /// <summary>Java <c>convertMilliseconds_Giay(milliseconds)</c>: whole seconds and <c>giây</c>; negative input is treated as 0.</summary>
    public static string ConvertMillisecondsGiay(long milliseconds)
    {
        if (milliseconds < 0)
        {
            milliseconds = 0;
        }

        return (milliseconds / 1000).ToString(CultureInfo.InvariantCulture) + " giây";
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Dates (Java Date = epoch milliseconds, shown in Vietnam time)
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>Java <c>toDateString(date)</c> for the instant <paramref name="epochMilliseconds"/>: <c>yyyy-MM-dd HH:mm:ss</c> in Vietnam time (<c>SimpleDateFormat</c>).</summary>
    public static string ToDateString(long epochMilliseconds) => JavaDateFormat.Format(epochMilliseconds, DateTimePattern);

    /// <summary>Java <c>toDateString(date)</c>: a null date gives the fixed text <c>2025-01-01 01:01:00</c> (Java catches the <c>NullPointerException</c>).</summary>
    public static string ToDateString(DateTimeOffset? date) => date is null ? FallbackDateString : ToDateString(date.Value.ToUnixTimeMilliseconds());

    /// <summary>
    /// Java <c>getDate(dateString)</c> as epoch milliseconds: parses <c>yyyy-MM-dd HH:mm:ss</c> in Vietnam time with the LENIENT rules of <c>SimpleDateFormat</c> (month 13 rolls into the next year,
    /// hour 24 into the next day, <c>2024-1-5 1:2:3</c> is fine, text after the seconds is ignored). Where Java returns <c>new Date()</c> (null, empty or unparsable text) this returns
    /// <paramref name="nowMilliseconds"/>, so the function stays testable; see <see cref="GetDate(string, TimeProvider)"/>.
    /// </summary>
    public static long GetDateMilliseconds(string? dateString, long nowMilliseconds)
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return nowMilliseconds;
        }

        return JavaDateFormat.TryParse(dateString, DateTimePattern, out long parsed) ? parsed : nowMilliseconds;
    }

    /// <summary>Java <c>getDate(dateString)</c>: <see cref="GetDateMilliseconds"/> with the clock <paramref name="clock"/> for the failure case, as a <see cref="DateTimeOffset"/> at +07:00.</summary>
    public static DateTimeOffset GetDate(string? dateString, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        long ms = GetDateMilliseconds(dateString, clock.GetUtcNow().ToUnixTimeMilliseconds());
        return DateTimeOffset.FromUnixTimeMilliseconds(ms).ToOffset(VietnamTime.Offset);
    }

    /// <summary>
    /// Java <c>isAfterDay(now, when)</c>: true when the CALENDAR DAY (in Vietnam time) of <paramref name="nowMilliseconds"/> is later than that of <paramref name="whenMilliseconds"/>;
    /// the time of day is ignored (00:30 on the 16th is after 23:00 on the 15th).
    /// </summary>
    public static bool IsAfterDay(long nowMilliseconds, long whenMilliseconds) => LocalDay(nowMilliseconds) > LocalDay(whenMilliseconds);

    /// <inheritdoc cref="IsAfterDay(long, long)"/>
    public static bool IsAfterDay(DateTimeOffset now, DateTimeOffset when) => IsAfterDay(now.ToUnixTimeMilliseconds(), when.ToUnixTimeMilliseconds());

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Race appearance
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    /// <summary>Java <c>getHead(gender)</c>: the head part id: 28 for Xayda (2), 32 for Namek (1), 64 for everything else (Earthling 0 and unknown values).</summary>
    public static sbyte GetHead(sbyte gender) => gender switch
    {
        2 => (sbyte)28,
        1 => (sbyte)32,
        _ => (sbyte)64,
    };

    /// <summary>Java <c>getLeg(gender)</c>: the leg part id: 17 for Xayda (2), 11 for Namek (1), 15 for everything else.</summary>
    public static sbyte GetLeg(sbyte gender) => gender switch
    {
        2 => (sbyte)17,
        1 => (sbyte)11,
        _ => (sbyte)15,
    };

    /// <summary>Java <c>getBody(gender)</c>: the body part id: 16 for Xayda (2), 10 for Namek (1), 14 for everything else.</summary>
    public static sbyte GetBody(sbyte gender) => gender switch
    {
        2 => (sbyte)16,
        1 => (sbyte)10,
        _ => (sbyte)14,
    };

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static string RoundToTwoDecimals(double num, JavaNumberSymbols symbols)
    {
        double roundedNumber = JavaNumbers.Round(num * 100.0) / 100.0;
        return JavaNumberFormat.FormatDouble(roundedNumber, 2, 2, JavaRoundingMode.HalfEven, symbols);
    }

    // commons-lang 2.6 ArrayUtils.addAll(Object[], Object[]): null is empty; null only when both are null; otherwise a new array.
    private static object?[]? AddAll(object?[]? array1, object?[]? array2)
    {
        if (array1 is null)
        {
            return (object?[]?)array2?.Clone();
        }

        if (array2 is null)
        {
            return (object?[])array1.Clone();
        }

        var joined = new object?[array1.Length + array2.Length];
        array1.CopyTo(joined, 0);
        array2.CopyTo(joined, array1.Length);
        return joined;
    }

    // Java Character.digit(ch, 10): the value of a Unicode decimal digit (category Nd), otherwise -1.
    private static int JavaDigit(char c)
    {
        if (c >= '0' && c <= '9')
        {
            return c - '0';
        }

        return char.IsDigit(c) ? (int)char.GetNumericValue(c) : -1;
    }

    private static long LocalDay(long epochMilliseconds)
    {
        long local = unchecked(epochMilliseconds + VietnamTime.OffsetMilliseconds);
        long day = local / MillisecondsPerDay;
        return local % MillisecondsPerDay < 0 ? day - 1 : day;
    }
}
