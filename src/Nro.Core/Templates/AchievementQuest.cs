namespace Nro.Core.Templates;

// Tiến độ hoàn thành một thành tựu của người chơi.
// Nguồn Java: nro/template/AchievementQuest.java (giữ constructor hai tham số duy nhất của Java, không có constructor mặc định).
public class AchievementQuest
{
    // Số lần hoặc mức độ đã hoàn thành thành tựu (Java: completed)
    public long Completed { get; set; }

    // Cho biết đã nhận thưởng; giữ nguyên lỗi chính tả của tên Java để dễ tìm kiếm (Java: isRecieve)
    public bool IsRecieve { get; set; }

    public AchievementQuest(long completed, bool isRecieve)
    {
        Completed = completed;
        IsRecieve = isRecieve;
    }
}
