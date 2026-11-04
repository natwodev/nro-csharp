using System.Globalization;
namespace Nro.Core.Time;

// Đồng hồ cho phần phụ thuộc thời gian của TimeUtil, lấy thời điểm từ TimeProvider tiêm được để kiểm thử và trả lịch theo giờ Việt Nam.
// Nguồn Java: Utils/TimeUtil.java (giữ múi giờ +07:00 cố định; lấy thời điểm qua TimeProvider, không đọc trực tiếp giờ/múi giờ hay CultureInfo máy hoặc TimeProvider.LocalTimeZone nên 00:00 Việt Nam là cùng thời điểm trên mọi máy chủ; hàm thuần nằm ở TimeUtil).
public class VietnamClock
{
    private readonly TimeProvider _time;

    // Tạo đồng hồ từ nguồn thời điểm được tiêm, mặc định TimeProvider.System.
    public VietnamClock(TimeProvider? timeProvider = null)
    {
        _time = timeProvider ?? TimeProvider.System;
    }

    // Trả mili giây epoch UTC từ TimeProvider đã tiêm (Java: System.currentTimeMillis).
    public long CurrentTimeMillis() => _time.GetUtcNow().ToUnixTimeMilliseconds();

    // Trả thời điểm hiện tại theo giờ Việt Nam +07:00 cố định.
    public DateTimeOffset Now() => _time.GetUtcNow().ToOffset(VietnamTime.Offset);

    // Trả cùng mili giây epoch với CurrentTimeMillis vì đổi múi giờ không dịch chuyển thời điểm; giữ giá trị Java dù có thể gây bất ngờ cho nơi gọi (Java: currentTimeMillisVietNam).
    public long CurrentTimeMillisVietNam() => CurrentTimeMillis();

    // Trả thứ trong tuần, 0 là Chủ nhật đến 6 là Thứ bảy (Java: getCurrDay).
    public int GetCurrDay() => (int)Now().DayOfWeek;

    // Trả giờ theo giờ Việt Nam, từ 0 đến 23 (Java: getCurrHour).
    public int GetCurrHour() => Now().Hour;

    // Trả phút theo giờ Việt Nam, từ 0 đến 59 (Java: getCurrMin).
    public int GetCurrMin() => Now().Minute;

    // Định dạng thời điểm hiện tại theo mẫu SimpleDateFormat tương thích Java (Java: getTimeNow).
    public string GetTimeNow(string format) => JavaDateFormat.Format(CurrentTimeMillis(), format);

    // Định dạng thời điểm hiện tại trừ subTime mili giây theo mẫu đã cho (Java: getTimeBeforeCurrent).
    public string GetTimeBeforeCurrent(int subTime, string format) => JavaDateFormat.Format(CurrentTimeMillis() - subTime, format);

    // Đúng khi hiện tại nằm chặt giữa d1 và d2 sau khi cùng phân tích theo format; mẫu chỉ giờ so theo giờ trong ngày và now được định dạng rồi phân tích lại để cắt độ chính xác; văn bản không hợp lệ ném InvalidTimeException (Java: isTimeNowInRangex).
    public bool IsTimeNowInRangex(string d1, string d2, string format)
    {
        // Java: văn bản null gây NullPointerException trong khối try, rồi được đổi thành cùng Exception.
        if (d1 is null || d2 is null
            || !JavaDateFormat.TryParse(d1, format, out long time1)
            || !JavaDateFormat.TryParse(d2, format, out long time2)
            || !JavaDateFormat.TryParse(JavaDateFormat.Format(CurrentTimeMillis(), format), format, out long now))
        {
            throw new InvalidTimeException();
        }

        return now > time1 && now < time2;
    }

    // Trả "N phút" khi còn hơn 60 giây, ngược lại "N giây"; không bao giờ âm (Java: getTimeLeft).
    public string GetTimeLeft(long lastTime, int secondTarget)
    {
        int secondsLeft = SecondsLeft(lastTime, secondTarget);
        return secondsLeft > 60
            ? (secondsLeft / 60).ToString(CultureInfo.InvariantCulture) + " phút"
            : secondsLeft.ToString(CultureInfo.InvariantCulture) + " giây";
    }

    // Trả thời gian đã qua dạng Nn, Ng, Np hoặc Ngi trước cho ngày, giờ, phút hoặc giây (Java: getTimeLeft).
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

    // Trả số phút còn lại làm tròn xuống, nhưng 1 khi còn 1..60 giây và 0 khi hết (Java: getMinLeft).
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

    // Trả số giây còn lại đến khi đủ secondTarget giây từ lastTime; không bao giờ âm (Java: getSecondLeft).
    public int GetSecondLeft(long lastTime, int secondTarget) => SecondsLeft(lastTime, secondTarget);

    // Trả thời gian còn lại ở dạng chữ theo TimeUtil.ConvertTime (Java: getDateLeft).
    public string GetDateLeft(long lastTime, int secondTarget) => TimeUtil.ConvertTime(SecondsLeft(lastTime, secondTarget));

    // Trả thời gian đã qua từ lastTime ở dạng chữ; thời điểm tương lai cho chuỗi rỗng (Java: convertTimeNow).
    public string ConvertTimeNow(long lastTime)
    {
        int secondsLeft = SecondsPassed(lastTime);
        if (secondsLeft < 0)
        {
            secondsLeft = 0;
        }

        return TimeUtil.ConvertTime(secondsLeft);
    }

    // Đúng từ 12:00 đến trước 13:00; khác biệt có chủ đích là không gán lại MajinBuu12H.HOUR_OPEN_MAP_MABU = 12 mỗi lần vì trường đã khởi tạo 12 và không nơi nào khác ghi nó (Java: isMabuOpen).
    public bool IsMabuOpen()
    {
        int hour = Now().Hour;
        return hour >= 12 && hour < 13;
    }

    // Đúng vào Thứ bảy hoặc Chủ nhật (Java: isOpenWeekend).
    public bool IsOpenWeekend()
    {
        DayOfWeek day = Now().DayOfWeek;
        return day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
    }

    // Đúng trong 09:00-09:29 hoặc 21:00-21:29 (Java: isFlashSale).
    public bool IsFlashSale()
    {
        DateTimeOffset now = Now();
        bool morning = now.Hour == 9 && now.Minute < 30;
        bool evening = now.Hour == 21 && now.Minute < 30;
        return morning || evening;
    }

    // Đúng từ 14:00 đến 15:59 (Java: isMabu14HOpen).
    public bool IsMabu14HOpen()
    {
        int hour = Now().Hour;
        return hour >= 14 && hour < 16;
    }

    // Đúng từ 21:00 đến 21:59 (Java: is21H).
    public bool Is21H()
    {
        int hour = Now().Hour;
        return hour >= 21 && hour < 22;
    }

    // Trả giờ mở hôm nay theo ngày Việt Nam dưới dạng mili giây epoch nhưng coi thời điểm là UTC nên muộn hơn thời điểm Việt Nam thật 7 giờ; giữ độ lệch vì nơi gọi so với giá trị riêng (Java: getStartTimeBlackBallWar).
    public long GetStartTimeBlackBallWar(BlackBallWarSchedule schedule)
    {
        DateTime start = Now().Date.Add(schedule.Open.ToTimeSpan());
        return new DateTimeOffset(start, TimeSpan.Zero).ToUnixTimeMilliseconds();
    }

    // Đúng khi giờ trong ngày hiện tại nằm chặt giữa giờ mở và đóng (Java: isBlackBallWarOpen).
    public bool IsBlackBallWarOpen(BlackBallWarSchedule schedule)
    {
        TimeOnly current = CurrentTimeOfDay();
        return current > schedule.Open && current < schedule.Close;
    }

    // Đúng khi đã qua giờ nhặt và chiến tranh đang mở (Java: isBlackBallWarCanPick).
    public bool IsBlackBallWarCanPick(BlackBallWarSchedule schedule) => CurrentTimeOfDay() > schedule.CanPick && IsBlackBallWarOpen(schedule);

    // Trả số giây nguyên từ hiện tại đến giờ nhặt hôm nay, hoặc 0 khi đã qua (Java: getSecondsUntilCanPick).
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

    // Java: (int) ((System.currentTimeMillis() - lastTime) / 1000), ép kiểu cắt về 32 bit.
    private int SecondsPassed(long lastTime) => unchecked((int)((CurrentTimeMillis() - lastTime) / 1000));

    private int SecondsLeft(long lastTime, int secondTarget)
    {
        int secondsLeft = unchecked(secondTarget - SecondsPassed(lastTime));
        return secondsLeft < 0 ? 0 : secondsLeft;
    }
}
