using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Nro.Core.Time;

// Định dạng và đọc ngày giờ theo đúng cách của Java, giờ Việt Nam cố định +07:00 (xem VietnamTime); TimeUtil dùng cho formatTime, getTime(String, String), getTimeNow...
// Nguồn Java: một phần của java.text.SimpleDateFormat (locale en-US, lenient, múi giờ Asia/Ho_Chi_Minh). Không dùng định dạng của .NET được vì khác nghĩa:
// MM với phút, "/" và ":" là ký tự ngăn cách theo văn hóa, S trong Java là SỐ mili giây, .NET không lenient và không bỏ qua chữ thừa ở cuối.
// Chữ được hỗ trợ: định dạng G y M L d k H m s S E D a h K Z X u, chữ trong nháy đơn và ''; đọc y M L d k H m s S a h K và chữ cố định.
// Chữ không phải chữ mẫu của Java thì ném ArgumentException với đúng câu của Java ("Illegal pattern character 'b'"); chữ hợp lệ trong Java nhưng chưa làm
// (F w W z Y, riêng khi đọc thêm G E D Z X u) thì ném NotSupportedException, không bao giờ âm thầm trả kết quả khác.
// Khi đọc giữ hành vi Java: lenient (tháng 13 sang năm sau, ngày 45 sang tháng sau), bỏ khoảng trắng trước số, số bị giới hạn theo độ dài mẫu khi phần tử kế tiếp
// cũng là trường (yyyyMMddHHmmss), bỏ qua chữ thừa sau phần tử cuối, trường không có mặc định là 1970-01-01 00:00:00.000.
// Khác Java có chủ đích: dùng lịch Gregory cho mọi năm (Java đổi sang lịch Julius trước 1582-10-15), từ chối năm nhỏ hơn 1; đọc năm 2 chữ số bằng y/yy
// thì ném NotSupportedException vì Java tính theo ngày hiện tại (file golden ghi rõ không bao giờ dùng yy).
public static class JavaDateFormat
{
    // Mọi chữ mẫu hợp lệ của Java SimpleDateFormat
    private const string JavaPatternLetters = "GyMdkHmsSEDFwWahKzZYuXL";

    // Chữ mẫu được hỗ trợ khi định dạng
    private const string FormatLetters = "GyMLdkHmsSEDahKZXu";

    // Chữ mẫu được hỗ trợ khi đọc
    private const string ParseLetters = "yMLdkHmsSahK";

    // Số mili giây trong một ngày
    private const long MillisecondsPerDay = 86_400_000L;

    // Tên tháng đầy đủ tiếng Anh (January...)
    private static readonly string[] MonthNames = CultureInfo.InvariantCulture.DateTimeFormat.MonthNames[..12];

    // Tên tháng viết tắt tiếng Anh (Jan...)
    private static readonly string[] ShortMonthNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedMonthNames[..12];

    // Tên thứ đầy đủ tiếng Anh, bắt đầu từ Chủ nhật (Sunday...)
    private static readonly string[] DayNames = CultureInfo.InvariantCulture.DateTimeFormat.DayNames;

    // Tên thứ viết tắt tiếng Anh (Sun...)
    private static readonly string[] ShortDayNames = CultureInfo.InvariantCulture.DateTimeFormat.AbbreviatedDayNames;

    // Ký hiệu sáng/chiều
    private static readonly string[] AmPm = ["AM", "PM"];

    // Một phần tử của mẫu: Text khác null là chữ cố định, còn lại là chữ mẫu Letter lặp Count lần
    private readonly record struct Element(char Letter, int Count, string? Text);

    // Định dạng một thời điểm (mili giây như Date.getTime() của Java) theo giờ Việt Nam (Java: new SimpleDateFormat(pattern).format(new Date(ms)))
    public static string Format(long epochMilliseconds, string pattern) => Format(epochMilliseconds, pattern, VietnamTime.Offset);

    // Định dạng một thời điểm theo độ lệch UTC cố định cho trước (test dùng để chứng minh kết quả không phụ thuộc múi giờ của máy)
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
        int dayOfWeek = (int)FloorMod(days + 4, 7); // 1970-01-01 là thứ Năm; 0 = Chủ nhật

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

    // Đọc chuỗi ngày giờ theo mẫu, theo giờ Việt Nam; trả false ở chỗ Java ném ParseException, mẫu sai vẫn ném ArgumentException như lúc Java tạo formatter
    // (Java: new SimpleDateFormat(pattern).parse(text).getTime())
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

            // Như Java: số bị giới hạn theo độ dài mẫu khi phần tử kế tiếp cũng là trường (các chữ số liền nhau như yyyyMMdd)
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
            return false; // java.text.DateFormat.parse(String): dừng ở vị trí 0 nghĩa là thất bại
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

    // ---- dịch mẫu (java.text.SimpleDateFormat.compile) ----

    // Tách mẫu thành các phần tử; kiểm tra chữ mẫu như Java và chỉ nhận các chữ được hỗ trợ cho thao tác này
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

    // ---- hàm phụ ----

    // Thêm số 0 bên trái cho đủ số chữ số tối thiểu, giữ dấu trừ ở đầu với số âm
    private static string ZeroPad(long value, int minDigits)
    {
        string digits = value.ToString(CultureInfo.InvariantCulture);
        if (value >= 0)
        {
            return digits.PadLeft(minDigits, '0');
        }

        return "-" + digits[1..].PadLeft(minDigits, '0');
    }

    // Ghi độ lệch múi giờ dạng +0700 (Z) hoặc +07, +0700, +07:00 (X); X ghi "Z" khi độ lệch bằng 0
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
            return; // ISO 8601 chỉ ghi giờ
        }

        if (colon)
        {
            sb.Append(':');
        }

        sb.Append(ZeroPad(minutes % 60, 2));
    }

    // Đọc số nguyên như java.text.DecimalFormat với parseIntegerOnly: dấu '-' tùy chọn rồi các chữ số; dừng ở ký tự khác đầu tiên hoặc ở giới hạn
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

    // Tìm tên dài nhất khớp tại vị trí start, không phân biệt hoa thường; trả vị trí kết thúc hoặc -1 (Java: SimpleDateFormat.matchString)
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

    // Chia làm tròn xuống (như Math.floorDiv của Java), khác phép chia của C# với số âm
    private static long FloorDiv(long a, long b)
    {
        long q = a / b;
        return (a % b != 0 && (a < 0) != (b < 0)) ? q - 1 : q;
    }

    // Phần dư luôn cùng dấu với số chia (như Math.floorMod của Java)
    private static long FloorMod(long a, long b) => a - (FloorDiv(a, b) * b);

    // Đổi ngày/tháng/năm sang số ngày kể từ 1970-01-01 (thuật toán của Howard Hinnant, lịch Gregory cho mọi năm)
    private static long DaysFromCivil(long year, int month, int day)
    {
        long y = month <= 2 ? year - 1 : year;
        long era = FloorDiv(y, 400);
        long yoe = y - (era * 400);
        long doy = (((153L * (month > 2 ? month - 3 : month + 9)) + 2) / 5) + day - 1;
        long doe = (yoe * 365) + (yoe / 4) - (yoe / 100) + doy;
        return (era * 146_097) + doe - 719_468;
    }

    // Đổi số ngày kể từ 1970-01-01 sang năm/tháng/ngày (chiều ngược của DaysFromCivil)
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
