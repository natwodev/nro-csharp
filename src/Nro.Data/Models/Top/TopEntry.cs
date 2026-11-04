namespace Nro.Data.Models.Top;

// Bản ghi một hạng một giá trị gồm id, tên, giới tính, trang bị, lần đăng nhập và giá trị xếp hạng.
// Nguồn Java: jbcd/dao/TopDAO.java, nro/server/TopServer.java (id dùng phá hòa; NULL là 0, còn cột không dấu giữ mẫu bit nên số âm vẫn xếp trên số dương như MariaDB).
public sealed record TopEntry(int PlayerId, string Name, int Gender, string ItemsBody, DateTime? LastTimeLoginGame, long Value)
{
    // Mili giây epoch từ thời điểm đăng nhập, hoặc 0 khi cột NULL (Java: Timestamp.getTime).
    public long LastTimeLoginMillis => TopTimestamps.ToMillis(LastTimeLoginGame);
}
