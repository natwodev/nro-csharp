using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;

namespace Nro.Game.Progress.Tasks;

// Trạng thái nhiệm vụ sự kiện hằng ngày của riêng người chơi; Renew dùng VietnamClock để xét sang ngày theo giờ Việt Nam. Phần trăm dùng long rồi ép int và vẫn có thể chia cho 0 trong trường hợp Java tương ứng.
// Nguồn Java: nro/task/EventTask.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).








public class EventTask
{
    // Template đang áp dụng; null khi chưa có.
    public EventTaskTemplate? Template { get; set; }

    // Tiến độ hoặc số lượng hiện có.
    public int Count { get; set; }

    // Số lượng cần đạt.
    public int MaxCount { get; set; }

    // Mức độ khó đã chọn.
    public int Level { get; set; }

    // Số lượt nhiệm vụ còn lại trong ngày.
    public int LeftTask { get; set; }

    // Thời điểm nhận hoặc gia hạn gần nhất, tính bằng mili giây.
    public long ReceivedTime { get; set; }

    // Đánh dấu đã báo ngay khi bắt đầu tiến độ.
    public bool Notify0 { get; set; }

    // Đánh dấu đã báo mốc 10 phần trăm.
    public bool Notify10 { get; set; }

    // Đánh dấu đã báo mốc 20 phần trăm.
    public bool Notify20 { get; set; }

    // Đánh dấu đã báo mốc 30 phần trăm.
    public bool Notify30 { get; set; }

    // Đánh dấu đã báo mốc 40 phần trăm.
    public bool Notify40 { get; set; }

    // Đánh dấu đã báo mốc 50 phần trăm.
    public bool Notify50 { get; set; }

    // Đánh dấu đã báo mốc 60 phần trăm.
    public bool Notify60 { get; set; }

    // Đánh dấu đã báo mốc 70 phần trăm.
    public bool Notify70 { get; set; }

    // Đánh dấu đã báo mốc 80 phần trăm.
    public bool Notify80 { get; set; }

    // Đánh dấu đã báo mốc 90 phần trăm.
    public bool Notify90 { get; set; }


    // Khởi tạo với đủ số lượt nhiệm vụ sự kiện tối đa trong ngày.
    public EventTask()
    {
        LeftTask = ConstTask.MAX_EVENT_TASK;
    }


    // Xóa template, tiến độ, cấp và cờ mốc; giữ MaxCount, LeftTask, ReceivedTime. (Java: reset)
    public void Reset()
    {
        Template = null;
        Count = 0;
        Level = 0;
        Notify0 = false;
        Notify10 = false;
        Notify20 = false;
        Notify30 = false;
        Notify40 = false;
        Notify50 = false;
        Notify60 = false;
        Notify70 = false;
        Notify80 = false;
        Notify90 = false;
    }




    // Khôi phục hạn mức ngày khi ngày Việt Nam của mốc nhận đã qua và ghi thời điểm mới. (Java: renew)
    public void Renew(VietnamClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        long now = clock.CurrentTimeMillis();
        if (Util.IsAfterDay(now, ReceivedTime))
        {
            LeftTask = ConstTask.MAX_EVENT_TASK;
            ReceivedTime = now;
        }
    }

    // Kiểm tra tiến độ đã đạt ngưỡng của template hiện tại. (Java: isDone)
    public bool IsDone() => Count >= MaxCount;


    // Trả tên template có thay %1, hoặc thông báo chưa có nhiệm vụ. (Java: getName)
    public string GetName()
    {
        if (Template is not null)
        {
            return Util.Replace(Template.Name, "%1", MaxCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        return "Hiện tại không có nhiệm vụ nào";
    }


    // Đổi cấp khó sang nhãn tiếng Việt; giá trị ngoài 0..4 trả Anwin như Java. (Java: getLevel)
    public string GetLevel()
    {
        switch (Level)
        {
            case ConstTask.EASY:
                return "dễ";
            case ConstTask.NORMAL:
                return "bình thường";
            case ConstTask.HARD:
                return "khó";
            case ConstTask.VERY_HARD:
                return "rất khó";
            case ConstTask.HELL:
                return "địa ngục";
            default:
                return "Anwin";
        }
    }



    // Tính phần trăm; giữ phép nhân int 32-bit có thể tràn như Java. (Java: getPercentProcess)
    public int GetPercentProcess()
    {
        if (Count >= MaxCount)
        {
            return 100;
        }

        return unchecked((int)((long)Count * 100 / MaxCount));
    }
}
