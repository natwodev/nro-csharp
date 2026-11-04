using System.Globalization;
using System.Text;

using Nro.Core.Consts;
using Nro.Core.Time;

namespace Nro.Core.Util;

// Các hàm tiện ích THUẦN: mọi hàm chỉ phụ thuộc vào tham số (đối chiếu từng trường hợp với tests/golden/util-pure.json)
// Nguồn Java: phần thuần của Utils/Util.java (hàm ngẫu nhiên nằm ở RandomUtil vì cần IRandomProvider; các hàm đọc đồng hồ, tạo item hoặc nhận Player/Zone/Mob chưa có ở đây)
// Mọi chỗ Java định dạng qua locale mặc định hoặc NumberFormat/DecimalFormat/String.format đều làm lại tường minh với invariant culture (JavaNumberFormat):
// server Java định dạng số bằng ký hiệu vi-VN (1.234,5) bất kể cài đặt của máy, lớp này cũng vậy.
// Ngày giờ hiển thị theo giờ Việt Nam (cố định +07:00, VietnamTime); Date của Java là một thời điểm tính bằng mili giây epoch, có thêm bản nhận DateTimeOffset.
// Giữ có chủ đích các điểm lạ của Java mà client và dữ liệu đã lưu phụ thuộc vào: tràn số int ở CreateIdBossClone, GetDistance(int, int, int, int),
// FormatTimeHMS với số âm, GetPercent chia trước rồi mới nhân, định dạng số dùng chung của Format(double) bị FormatToSecondMinute thay đổi...
// Các điểm khác Java có chủ đích ghi ở từng hàm.
public static class Util
{
    // Mẫu ngày giờ của ToDateString và GetDateMilliseconds
    private const string DateTimePattern = "yyyy-MM-dd HH:mm:ss";
    // Chuỗi cố định ToDateString trả khi ngày là null (như Java)
    private const string FallbackDateString = "2025-01-01 01:01:00";
    // Số chữ số thập phân tối đa lúc khởi động của định dạng số dùng chung
    private const int DefaultMaxFractionDigits = 3;
    // Số mili giây trong một ngày
    private const long MillisecondsPerDay = 86_400_000L;

    // Bảng chữ có dấu (Java: bảng 'char[] SOURCE_CHARACTERS' / 'DESTINATION_CHARACTERS', sắp theo mã ký tự để Arrays.binarySearch chạy được). Chép từ mã nguồn Java bằng script.
    private static readonly char[] SourceCharacters = "ÀÁÂÃÈÉÊÌÍÒÓÔÕÙÚÝàáâãèéêìíòóôõùúýĂăĐđĨĩŨũƠơƯưẠạẢảẤấẦầẨẩẪẫẬậẮắẰằẲẳẴẵẶặẸẹẺẻẼẽẾếỀềỂểỄễỆệỈỉỊịỌọỎỏỐốỒồỔổỖỗỘộỚớỜờỞởỠỡỢợỤụỦủỨứỪừỬửỮữỰự".ToCharArray();

    // Chữ không dấu ứng với từng chữ của SourceCharacters ở cùng vị trí (Java: DESTINATION_CHARACTERS)
    private static readonly char[] DestinationCharacters = "AAAAEEEIIOOOOUUYaaaaeeeiioooouuyAaDdIiUuOoUuAaAaAaAaAaAaAaAaAaAaAaAaEeEeEeEeEeEeEeEeIiIiOoOoOoOoOoOoOoOoOoOoOoOoUuUuUuUuUuUuUu".ToCharArray();

    // Số chữ số thập phân tối đa của định dạng số dùng chung: ban đầu là 3, FormatToSecondMinute đặt thành 1 luôn từ đó (Java: biến tĩnh 'NumberFormat num = NumberFormat.getInstance(vi-VN)')
    private static int numberFormatMaxFractionDigits = DefaultMaxFractionDigits;

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Định danh, chuỗi
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Id boss nhân bản: -idPlayer - 1_000_000_000 tính bằng int (tràn vòng ở hai đầu: int.MaxValue cho 1147483649) (Java: createIdBossClone)
    public static int CreateIdBossClone(int idPlayer) => unchecked(-idPlayer - 1_000_000_000);

    // Arrays.toString(arr).contains(key), tức là kiểm tra CHUỖI CON trên chữ [a, b] (nên bc, c, d và [ đều khớp ["abc","def"]); mảng null là chữ null,
    // phần tử null cũng là chữ null; key null thì ném lỗi như NullPointerException của Java (Java: contains(String[] arr, String key))
    public static bool Contains(string?[]? arr, string key)
    {
        ArgumentNullException.ThrowIfNull(key);
        string text = arr is null ? "null" : "[" + string.Join(", ", arr.Select(item => item ?? "null")) + "]";
        return text.Contains(key, StringComparison.Ordinal);
    }

    // Thay thế theo CHỮ bằng String.replace(CharSequence, CharSequence) (tham số tên regex nhưng không phải biểu thức chính quy).
    // regex rỗng khớp giữa mọi ký tự ("abc" với - cho -a-b-c-), điều mà string.Replace của .NET từ chối (Java: replace(text, regex, replacement))
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

    // true nếu text có ký tự nào khác chữ cái và chữ số ASCII. Java thử regex [^a-z0-9 ] không phân biệt hoa thường rồi thử thêm text có dấu cách không,
    // nên dấu cách cũng tính là đặc biệt; chữ có dấu, ký hiệu Kelvin U+212A và mọi ký tự ngoài ASCII khác cũng vậy (Java: haveSpecialCharacter)
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

    // Đổi 126 chữ tiếng Việt dựng sẵn trong bảng Java về chữ gốc (tìm nhị phân trong bảng đã sắp). Ký tự khác giữ nguyên, kể cả chữ y có dấu
    // U+1EF2..U+1EF9 (bảng Java thiếu) và các dấu tổ hợp rời của văn bản đã tách dấu (Java: removeAccent(char))
    public static char RemoveAccent(char ch)
    {
        int index = Array.BinarySearch(SourceCharacters, ch);
        return index >= 0 ? DestinationCharacters[index] : ch;
    }

    // Gọi RemoveAccent(char) trên từng đơn vị UTF-16; chuỗi null thì ném lỗi như NullPointerException của Java (Java: removeAccent(String))
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

    // Nối mảng: null hoặc không có mảng nào thì trả null; một mảng thì trả CHÍNH mảng đó (không sao chép, có thể null); nhiều mảng thì nối theo thứ tự
    // bằng ArrayUtils.addAll của commons-lang, coi mảng null là rỗng và chỉ trả null khi cả hai bên đều null (Java: addArray(Object[]... arrays))
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

    // Thêm gạch chéo ngược trước gạch chéo ngược, nháy đơn và nháy kép; đổi backspace, xuống dòng, về đầu dòng và tab thành \b \n \r \t
    // (thay đúng theo thứ tự đó, gạch chéo ngược trước tiên); chuỗi null thì ném lỗi như NullPointerException của Java (Java: addSlashes)
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

    // Thêm gạch chéo ngược trước mỗi ký tự ' " \ % (replaceAll("['\"\\\\%]", "\\\\$0")). Đây KHÔNG phải cách an toàn để dựng SQL,
    // tầng dữ liệu C# dùng tham số; chuỗi null thì ném lỗi như NullPointerException của Java (Java: strSQL)
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

    // true nếu Long.valueOf(str) thành công. Null và rỗng cho false; cho phép một dấu + hoặc - ở đầu; chữ số có thể là bất kỳ chữ số thập phân Unicode nào
    // (Ả Rập-Ấn, toàn độ rộng...); dấu cách, dấu chấm thập phân và giá trị ngoài phạm vi long cho false (Java: isNumeric)
    public static bool IsNumeric(string? str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return false;
        }

        // Làm lại Long.parseLong(s, 10): giá trị được cộng dồn dưới dạng số âm để chấp nhận được Long.MIN_VALUE.
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
    // Định dạng số (Java NumberFormat / DecimalFormat / String.format với ký hiệu vi-VN tường minh)
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Chuỗi hh:mm:ss bằng String.format("%02d:%02d:%02d"); giờ không giới hạn ở 24 (100:00:00), số âm cho các phần âm (-3661 là -1:-1:-1) (Java: formatTimeHMS)
    public static string FormatTimeHMS(int totalSeconds)
    {
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;
        return JavaNumbers.FormatZeroPadded(hours, 2) + ":" + JavaNumbers.FormatZeroPadded(minutes, 2) + ":" + JavaNumbers.FormatZeroPadded(seconds, 2);
    }

    // Chia lần lượt cho 1e9, 1e6 và 1e3, in tối đa một chữ số thập phân bị CẮT (không làm tròn) với ký hiệu vi-VN, nối các hậu tố dính liền nhau không dấu cách
    // (1e12 là 1 NghìnTỷ hoặc 1 KB); số dưới 1000 và mọi số âm không có hậu tố (-1500 là -1.500).
    // KHÁC JAVA CÓ CHỦ ĐÍCH: +Infinity làm vòng lặp Java chạy mãi; ở đây trả ∞ (Java: formatNumber(power, style))
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

    // Tỷ từ 1e9, Tr từ 1e6, k từ 1000, tối đa một chữ số thập phân làm tròn HALF_EVEN trên giá trị chính xác (1250 là 1,2 k, 1350 là 1,4 k, 999999 là 1.000 k);
    // dưới 1000 và mọi số âm thì in số nguyên có phân nhóm (Java: numberToMoney)
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

    // Theo locale MẶC ĐỊNH của JVM tham chiếu (en-US): Math.round(num * 100) / 100 (làm tròn half up, bão hòa ở phạm vi long) in bằng DecimalFormat("#,##0.00"),
    // vd 1,234.57. Kết quả Java phụ thuộc locale mặc định của JVM; dùng RoundToTwoDecimalsViVn để có 1.234,57 (Java: roundToTwoDecimals)
    public static string RoundToTwoDecimals(double num) => RoundToTwoDecimals(num, JavaNumberSymbols.EnUs);

    // Giống RoundToTwoDecimals(double) khi locale mặc định của JVM là vi-VN (trường golden resultWhenDefaultLocaleViVN): 1.234,57 (Java: roundToTwoDecimals)
    public static string RoundToTwoDecimalsViVn(double num) => RoundToTwoDecimals(num, JavaNumberSymbols.ViVn);

    // NumberFormat.getInstance(vi-VN).format(power) trên định dạng tĩnh DÙNG CHUNG, HALF_EVEN, tối đa NumberFormatMaxFractionDigits chữ số thập phân
    // (3 cho tới lần đầu gọi FormatToSecondMinute, sau đó là 1) (Java: format(double))
    public static string Format(double power) => JavaNumberFormat.FormatDouble(power, NumberFormatMaxFractionDigits, 0, JavaRoundingMode.HalfEven, JavaNumberSymbols.ViVn);

    // Số nguyên phân nhóm bằng dấu chấm (1.234.567); không bị NumberFormatMaxFractionDigits ảnh hưởng (Java: format(int))
    public static string Format(int power) => JavaNumberFormat.FormatLong(power, JavaNumberSymbols.ViVn);

    // Số nguyên phân nhóm bằng dấu chấm; không bị NumberFormatMaxFractionDigits ảnh hưởng (Java: format(long))
    public static string Format(long power) => JavaNumberFormat.FormatLong(power, JavaNumberSymbols.ViVn);

    // Từ 60 trở lên: power / 60 với một chữ số thập phân và chữ phút (75 là 1,2 phút); dưới 60 (và số âm) in số thường.
    // GIỮ LỖI JAVA: hàm gọi num.setMaximumFractionDigits(1) trên định dạng DÙNG CHUNG với Format(double), nên sau lần gọi đầu tiên mọi Format(double)
    // chỉ hiện một chữ số thập phân thay vì ba, cho tới hết tiến trình (file golden kiểm tra cả hai giai đoạn) (Java: formatToSecond_Minute)
    public static string FormatToSecondMinute(long power)
    {
        Volatile.Write(ref numberFormatMaxFractionDigits, 1);
        if (power >= 60)
        {
            return JavaNumberFormat.FormatDouble((double)power / 60, 1, 0, JavaRoundingMode.HalfEven, JavaNumberSymbols.ViVn) + " phút";
        }

        return JavaNumberFormat.FormatLong(power, JavaNumberSymbols.ViVn);
    }

    // Số chữ số thập phân tối đa hiện tại của định dạng số dùng chung mà Format(double) dùng (xem FormatToSecondMinute)
    public static int NumberFormatMaxFractionDigits => Volatile.Read(ref numberFormatMaxFractionDigits);

    // Đưa định dạng số dùng chung về trạng thái lúc khởi động (3 chữ số thập phân). Java không có hàm này; chỉ để các test không phụ thuộc lẫn nhau
    public static void ResetSharedNumberFormat() => Volatile.Write(ref numberFormatMaxFractionDigits, DefaultMaxFractionDigits);

    // N bytes, N KB, N MB hoặc N GB với thương nguyên (cắt bớt, không bao giờ làm tròn); số âm luôn là bytes (Java: formatBytes)
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

    // Số giây với hai chữ số thập phân và dấu phẩy: String.format("%.2f", ms / 1000.0) rồi thay . bằng , . Java làm tròn HALF_UP trên chuỗi thập phân
    // ngắn nhất của số double (1005 ms là 1,01); ToString("F2") của .NET sẽ cho 1,00 (Java: convertMillisecondsDouble)
    public static string ConvertMillisecondsDouble(long milliseconds)
    {
        double seconds = (double)milliseconds / 1000;
        return JavaNumberFormat.FormatFixed(seconds, 2).Replace('.', ',');
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Tính toán
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Khoảng cách (int)Math.sqrt((x1-x2)^2 + (y1-y2)^2). Các hiệu tính bằng int và tràn vòng khi tràn số (2000000000 với -2000000000 cho 294967296);
    // kết quả bị cắt, không làm tròn, và bão hòa ở int.MaxValue (Java: getDistance(x1, y1, x2, y2))
    public static int GetDistance(int x1, int y1, int x2, int y2)
    {
        double dx = unchecked(x1 - x2);
        double dy = unchecked(y1 - y2);
        return JavaNumbers.ToInt(Math.Sqrt((dx * dx) + (dy * dy)));
    }

    // value / 100 * percent tính bằng long; phép chia làm TRƯỚC và bị cắt, nên getPercent(199, 50) là 50 chứ không phải 99 (Java: getPercent)
    public static long GetPercent(long value, int percent) => unchecked(value / 100 * percent);

    // readInt là Manager.readInt của Java truyền vào: khi bật thì a bị giới hạn ở int.MaxValue rồi cắt về int (bão hòa ở int.MinValue phía dưới, NaN cho 0);
    // ngược lại cắt về long (bão hòa, NaN cho 0) (Java: CrisGH(a))
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

    // (int)(value * 1.1) khi highlights bật, ngược lại trả value; cắt về phía 0, bão hòa ở phạm vi int (Java: highlightsItem)
    public static int HighlightsItem(bool highlights, int value)
    {
        const double highlightsNumber = 1.1;
        return highlights ? JavaNumbers.ToInt(value * highlightsNumber) : value;
    }

    // coSo mũ soMu bằng cách nhân lặp trong long, KHÔNG kiểm tra tràn (tràn vòng ở 64 bit); số mũ 0 hoặc nhỏ hơn cho 1 (Java: tinhLuyThua)
    public static long TinhLuyThua(int coSo, int soMu)
    {
        long ketQua = 1;
        for (int i = 0; i < soMu; i++)
        {
            ketQua = unchecked(ketQua * coSo);
        }

        return ketQua;
    }

    // Kiểm tra chỉ số gian lận của option có id optionId và giá trị param (hàm Java đọc itemOption.optionTemplate.id và itemOption.param):
    // false khi giá trị vượt giới hạn chống gian lận của option đó, ngược lại true (id option lạ luôn true; giá trị âm luôn qua) (Java: checkDo(itemOption))
    public static bool CheckDo(int optionId, int param)
    {
        switch (optionId)
        {
            case 0: // sức đánh
                return param <= 12000;
            case 14: // chí mạng
                return param <= 30;
            case 107: // sao
            case 102:
                return param <= 8;
            case 77:
            case 103:
            case 95:
            case 96:
                return param <= 41;
            case 50:
                return param <= 24;
            case 6: // HP
            case 7: // KI
                return param <= 120000;
            case 47: // giáp
                return param <= 3500;
            default:
                return true;
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Chuỗi thời gian
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Ns trước (dưới một phút), Np trước, Ng trước (giờ), Nn trước (ngày); đơn vị bị cắt, không bao giờ làm tròn (Java: convertSecondsToTime)
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

    // hh:mm:ss với số giờ không giới hạn (String.format("%02d:%02d:%02d")); số âm cho các phần âm (Java: convertMilliseconds)
    public static string ConvertMilliseconds(long ms)
    {
        long seconds = ms / 1000 % 60;
        long minutes = ms / (1000 * 60) % 60;
        long hours = ms / (1000 * 60 * 60);
        return JavaNumbers.FormatZeroPadded(hours, 2) + ":" + JavaNumbers.FormatZeroPadded(minutes, 2) + ":" + JavaNumbers.FormatZeroPadded(seconds, 2);
    }

    // M m S s (số phút nguyên, rồi số giây còn lại, vd 1 m 1 s) (Java: convertMilliseconds_ms)
    public static string ConvertMillisecondsMs(long milliseconds)
    {
        long seconds = milliseconds / 1000;
        long minutes = seconds / 60;
        seconds %= 60;
        return minutes.ToString(CultureInfo.InvariantCulture) + " m " + seconds.ToString(CultureInfo.InvariantCulture) + " s";
    }

    // Số giây nguyên kèm chữ giây; số âm coi như 0 (Java: convertMilliseconds_Giay)
    public static string ConvertMillisecondsGiay(long milliseconds)
    {
        if (milliseconds < 0)
        {
            milliseconds = 0;
        }

        return (milliseconds / 1000).ToString(CultureInfo.InvariantCulture) + " giây";
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Ngày giờ (Date của Java = mili giây epoch, hiển thị theo giờ Việt Nam)
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Thời điểm epochMilliseconds dạng yyyy-MM-dd HH:mm:ss theo giờ Việt Nam (SimpleDateFormat) (Java: toDateString(date))
    public static string ToDateString(long epochMilliseconds) => JavaDateFormat.Format(epochMilliseconds, DateTimePattern);

    // Bản nhận DateTimeOffset; ngày null cho chuỗi cố định 2025-01-01 01:01:00 vì Java bắt NullPointerException (Java: toDateString(date))
    public static string ToDateString(DateTimeOffset? date) => date is null ? FallbackDateString : ToDateString(date.Value.ToUnixTimeMilliseconds());

    // Đọc yyyy-MM-dd HH:mm:ss theo giờ Việt Nam ra mili giây epoch, theo luật LENIENT của SimpleDateFormat (tháng 13 sang năm sau, giờ 24 sang ngày sau,
    // 2024-1-5 1:2:3 vẫn được, chữ sau phần giây bị bỏ qua). Chỗ Java trả new Date() (chuỗi null, rỗng hoặc không đọc được) thì ở đây trả nowMilliseconds
    // để hàm vẫn test được; xem GetDate(string, TimeProvider) (Java: getDate(dateString))
    public static long GetDateMilliseconds(string? dateString, long nowMilliseconds)
    {
        if (string.IsNullOrEmpty(dateString))
        {
            return nowMilliseconds;
        }

        return JavaDateFormat.TryParse(dateString, DateTimePattern, out long parsed) ? parsed : nowMilliseconds;
    }

    // GetDateMilliseconds lấy giờ hiện tại từ đồng hồ clock cho trường hợp lỗi, trả DateTimeOffset ở +07:00 (Java: getDate(dateString))
    public static DateTimeOffset GetDate(string? dateString, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        long ms = GetDateMilliseconds(dateString, clock.GetUtcNow().ToUnixTimeMilliseconds());
        return DateTimeOffset.FromUnixTimeMilliseconds(ms).ToOffset(VietnamTime.Offset);
    }

    // true khi NGÀY LỊCH (theo giờ Việt Nam) của nowMilliseconds sau ngày của whenMilliseconds; bỏ qua giờ trong ngày
    // (00:30 ngày 16 là sau 23:00 ngày 15) (Java: isAfterDay(now, when))
    public static bool IsAfterDay(long nowMilliseconds, long whenMilliseconds) => LocalDay(nowMilliseconds) > LocalDay(whenMilliseconds);

    // Như IsAfterDay(long, long) nhưng nhận DateTimeOffset (Java: isAfterDay(now, when))
    public static bool IsAfterDay(DateTimeOffset now, DateTimeOffset when) => IsAfterDay(now.ToUnixTimeMilliseconds(), when.ToUnixTimeMilliseconds());

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Ngoại hình theo hành tinh
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Id phần đầu: 28 cho Xayda (2), 32 cho Namek (1), 64 cho còn lại (Trái Đất 0 và giá trị lạ) (Java: getHead)
    public static sbyte GetHead(sbyte gender) => gender switch
    {
        2 => (sbyte)28,
        1 => (sbyte)32,
        _ => (sbyte)64,
    };

    // Id phần chân: 17 cho Xayda (2), 11 cho Namek (1), 15 cho còn lại (Java: getLeg)
    public static sbyte GetLeg(sbyte gender) => gender switch
    {
        2 => (sbyte)17,
        1 => (sbyte)11,
        _ => (sbyte)15,
    };

    // Id phần thân: 16 cho Xayda (2), 10 cho Namek (1), 14 cho còn lại (Java: getBody)
    public static sbyte GetBody(sbyte gender) => gender switch
    {
        2 => (sbyte)16,
        1 => (sbyte)10,
        _ => (sbyte)14,
    };

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Hàm phụ riêng
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Phần chung của hai bản RoundToTwoDecimals: Math.round(num * 100) / 100 rồi in đúng hai chữ số thập phân với bộ ký hiệu symbols (Java: roundToTwoDecimals)
    private static string RoundToTwoDecimals(double num, JavaNumberSymbols symbols)
    {
        double roundedNumber = JavaNumbers.Round(num * 100.0) / 100.0;
        return JavaNumberFormat.FormatDouble(roundedNumber, 2, 2, JavaRoundingMode.HalfEven, symbols);
    }

    // Nối hai mảng như ArrayUtils.addAll(Object[], Object[]) của commons-lang 2.6: null coi là rỗng; chỉ trả null khi cả hai đều null; còn lại luôn là mảng mới
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

        // Kết quả có kiểu phần tử lúc chạy của mảng ĐẦU TIÊN, như commons-lang (nơi gọi ép kiểu nó, vd (int[][]) Util.addArray(...)).
        // Phần tử của mảng thứ hai không hợp kiểu đó sẽ lỗi như ArrayStoreException của Java (ở đây là InvalidCastException).
        var joined = (object?[])Array.CreateInstance(array1.GetType().GetElementType()!, array1.Length + array2.Length);
        array1.CopyTo(joined, 0);
        array2.CopyTo(joined, array1.Length);
        return joined;
    }

    // Giá trị của một chữ số thập phân Unicode (nhóm Nd), ngược lại -1 (Java: Character.digit(ch, 10))
    private static int JavaDigit(char c)
    {
        if (c >= '0' && c <= '9')
        {
            return c - '0';
        }

        return char.IsDigit(c) ? (int)char.GetNumericValue(c) : -1;
    }

    // Số thứ tự ngày lịch theo giờ Việt Nam của một thời điểm (chia làm tròn xuống, đúng cả trước 1970), dùng cho IsAfterDay
    private static long LocalDay(long epochMilliseconds)
    {
        long local = unchecked(epochMilliseconds + VietnamTime.OffsetMilliseconds);
        long day = local / MillisecondsPerDay;
        return local % MillisecondsPerDay < 0 ? day - 1 : day;
    }
}
