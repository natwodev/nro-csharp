namespace Nro.Data.Models.Top;

// Bản ghi xếp hạng nhiệm vụ chính với Task, SubTask và Progress từ data_task; JSON NULL hoặc không đọc được là 0.
// Nguồn Java: jbcd/dao/TopDAO.java (giữ ba giá trị data_task và thời điểm đăng nhập để phá hòa).
public sealed record TopTaskEntry(int PlayerId, string Name, int Gender, string ItemsBody, DateTime? LastTimeLoginGame, long Task, long SubTask, long Progress)
{
    // Mili giây epoch từ thời điểm đăng nhập, hoặc 0 khi cột NULL (Java: Timestamp.getTime).
    public long LastTimeLoginMillis => TopTimestamps.ToMillis(LastTimeLoginGame);
}
