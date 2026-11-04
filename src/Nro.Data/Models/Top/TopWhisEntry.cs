namespace Nro.Data.Models.Top;

// Bản ghi xếp hạng thử thách luyện tập Whis từ player.training_data (Java: data_luyentap).
// Nguồn Java: jbcd/dao/TopDAO.java (Time NULL hoặc không đọc được trả 0 nhưng khi phá hòa vẫn đứng trước số theo thứ tự NULL của MariaDB).
public sealed record TopWhisEntry(int PlayerId, string Name, int Gender, string ItemsBody, DateTime? LastTimeLoginGame, long Top, long Time)
{
    // Mili giây epoch từ thời điểm đăng nhập, hoặc 0 khi cột NULL (Java: Timestamp.getTime).
    public long LastTimeLoginMillis => TopTimestamps.ToMillis(LastTimeLoginGame);
}
