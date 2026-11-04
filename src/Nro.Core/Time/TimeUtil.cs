using System.Globalization;
using System.Text;
using Nro.Core.Exceptions;
namespace Nro.Core.Time;

// Phần THUẦN của TimeUtil: các hàm chỉ phụ thuộc tham số (đã đối chiếu với tests/golden/util-pure.json)
// Nguồn Java: Utils/TimeUtil.java; các hàm đọc đồng hồ hệ thống (getCurrDay, getTimeLeft, getTimeNow, isMabuOpen, isFlashSale, giờ BlackBallWar...)
// nằm ở VietnamClock, lớp đó nhận TimeProvider nên test được.
// Date của Java là một thời điểm tính bằng mili giây epoch: mỗi hàm có bản nhận long (Java date.getTime()) và bản nhận DateTimeOffset.
// Mọi phép tính lịch làm theo giờ Việt Nam (cố định +07:00, xem VietnamTime), bất kể múi giờ hay văn hóa của máy.
// Lỗi của Java được GIỮ CÓ CHỦ ĐÍCH vì client và dữ liệu đã lưu dựa vào đúng kết quả đó: DiffDate tràn int với MONTH (kết quả âm)
// và YEAR (thực chất chia cho khoảng 17 ngày); GetTimeAgo dính các phần không có dấu cách; GetTime(long) hiển thị số âm.
public static class TimeUtil
{
    // Loại SECOND của DiffDate: tổng số giây (Java: SECOND)
    public const sbyte SECOND = 1;
    // Loại MINUTE: phần phút trong giờ, 0-59 (Java: MINUTE)
    public const sbyte MINUTE = 2;
    // Loại HOUR: phần giờ trong ngày, 0-23 (Java: HOUR)
    public const sbyte HOUR = 3;
    // Loại DAY: tổng số ngày (Java: DAY)
    public const sbyte DAY = 4;
    // Loại WEEK: tổng số tuần (Java: WEEK)
    public const sbyte WEEK = 5;
    // Loại MONTH: số chia bị tràn int nên kết quả âm (Java: MONTH)
    public const sbyte MONTH = 6;
    // Loại YEAR: số chia bị tràn int, thực chất khoảng 17 ngày (Java: YEAR)
    public const sbyte YEAR = 7;

    // Số mili giây trong một phút
    private const long MillisecondsPerMinute = 60_000L;
    // Số mili giây trong một giờ
    private const long MillisecondsPerHour = 3_600_000L;
    // Số mili giây trong một ngày
    private const long MillisecondsPerDay = 86_400_000L;

    // |d1 - d2| tính theo đơn vị type (0 nếu type không biết); hằng MONTH/YEAR bị tràn int như ghi chú ở đầu lớp (Java: diffDate(Date, Date, byte))
    public static long DiffDate(long d1Milliseconds, long d2Milliseconds, sbyte type)
    {
        long diff = unchecked(d1Milliseconds - d2Milliseconds);
        long timeDiff = diff < 0 ? unchecked(-diff) : diff; // Math.abs(Long.MIN_VALUE) trong Java vẫn là số âm
        return type switch
        {
            SECOND => timeDiff / 1000,
            MINUTE => timeDiff / (60 * 1000) % 60,
            HOUR => timeDiff / (60 * 60 * 1000) % 24,
            DAY => timeDiff / (24 * 60 * 60 * 1000),
            WEEK => timeDiff / (7 * 24 * 60 * 60 * 1000),
            MONTH => timeDiff / unchecked(30 * 24 * 60 * 60 * 1000), // tràn int: -1_702_967_296
            YEAR => timeDiff / unchecked(365 * 24 * 60 * 60 * 1000), // tràn int: 1_471_228_928
            _ => 0,
        };
    }

    // Như DiffDate(long, long, sbyte) nhưng nhận DateTimeOffset (Java: diffDate(Date, Date, byte))
    public static long DiffDate(DateTimeOffset d1, DateTimeOffset d2, sbyte type) => DiffDate(d1.ToUnixTimeMilliseconds(), d2.ToUnixTimeMilliseconds(), type);

    // Đọc chuỗi time theo mẫu SimpleDateFormat format, theo giờ Việt Nam, trả mili giây epoch (Java: getTime(String time, String format))
    // Ném InvalidTimeException nếu chuỗi không khớp (Java: Exception("Thời gian không hợp lệ")); ném ArgumentException nếu mẫu có chữ không hợp lệ
    // (Java: IllegalArgumentException, Java cũng không bắt lỗi này)
    public static long GetTime(string time, string format)
    {
        if (!JavaDateFormat.TryParse(time, format, out long result))
        {
            throw new InvalidTimeException();
        }

        return result;
    }

    // Hiển thị khoảng thời gian (mili giây) dạng "m phút s giây", "h giờ m phút" hoặc "d ngày h giờ" (Java: getTime(long time))
    public static string GetTime(long time)
    {
        long seconds = time / 1000;
        long minutes = seconds / 60;
        long hours = minutes / 60;
        long days = hours / 24;

        if (seconds <= 0)
        {
            seconds = 0; // đặt sau khi đã tính minutes: thời gian âm vẫn giữ số phút âm
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

    // Hiển thị số giây dạng "d ngày h giờ m phút s giây", bỏ các phần bằng 0 (chuỗi rỗng nếu 0 hoặc âm) (Java: convertTime(int))
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

        return result.ToString().Trim(); // String.trim() của Java bỏ các ký tự <= U+0020; các phần ở đây chỉ thêm dấu cách thường
    }

    // Định dạng thời điểm theo mẫu SimpleDateFormat, giờ Việt Nam (Java: formatTime(long time, String format))
    public static string FormatTime(long time, string format) => JavaDateFormat.Format(time, format);

    // Như trên nhưng nhận DateTimeOffset (Java: formatTime(Date time, String format))
    public static string FormatTime(DateTimeOffset time, string format) => JavaDateFormat.Format(time.ToUnixTimeMilliseconds(), format);

    // Hiển thị số giây dạng phút, giờ + phút hoặc ngày + giờ (Java: getTimeAgo(int)). Giữ nguyên các điểm lạ của Java: so sánh bằng > (60 giây vẫn là "1 phút",
    // 3600 giây là "60 phút"), dạng giờ/ngày KHÔNG có dấu cách trước số thứ hai ("1 giờ1 phút", "1 ngày1 giờ") và giờ chỉ dồn sang ngày khi vượt quá 24.
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

    // Mili giây đổi ra số phút dạng String.format("%02d phút", minutes); số âm không được thêm số 0 (Java: convertMillisecondToMinute(long))
    public static string ConvertMillisecondToMinute(long time) => Pad2(time / MillisecondsPerMinute) + " phút";

    // Mili giây đổi ra số giờ dạng "%02d giờ" (Java: convertMillisecondToHour(long))
    public static string ConvertMillisecondToHour(long time) => Pad2(time / MillisecondsPerHour) + " giờ";

    // Mili giây đổi ra số ngày dạng "%02d ngày" (Java: convertMillisecondToDay(long))
    public static string ConvertMillisecondToDay(long time) => Pad2(time / MillisecondsPerDay) + " ngày";

    // Thời điểm tạo tài khoản cộng thêm timeAgeDay ngày, hiển thị dạng "HH:mm:ss dd/MM/yyyy" theo Asia/Ho_Chi_Minh
    // (code Java đặt múi giờ cố định nên chưa bao giờ phụ thuộc múi giờ của máy) (Java: getDeadline(long timeCreateAccount, long timeAgeDay))
    public static string GetDeadline(long timeCreateAccount, long timeAgeDay)
    {
        long deadlineMillis = unchecked(timeCreateAccount + (timeAgeDay * 24L * 60 * 60 * 1000));
        return JavaDateFormat.Format(deadlineMillis, "HH:mm:ss dd/MM/yyyy");
    }

    // Độ rộng tối thiểu 2 TÍNH CẢ dấu trừ nên -1 vẫn là "-1" (Java: String.format("%02d", value))
    private static string Pad2(long value)
    {
        string digits = value.ToString(CultureInfo.InvariantCulture);
        return digits.Length >= 2 ? digits : "0" + digits;
    }
}
