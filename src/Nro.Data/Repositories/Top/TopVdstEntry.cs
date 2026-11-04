using Nro.Data.Db;
namespace Nro.Data.Repositories.Top;

// Bản ghi xếp hạng Võ đài sinh tử từ player.vodaisinhtu.
// Nguồn Java: jbcd/dao/TopDAO.java (Field2 là vodaisinhtu[2], SQL Java đặt bí danh LastTimeLoginGame dù là số và TopServer không đọc nó).
public sealed record TopVdstEntry(int PlayerId, string Name, int Gender, string ItemsBody, long Field2, long Time);
