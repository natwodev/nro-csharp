using Nro.Data.Db;
namespace Nro.Data.Repositories.Top;

// Bản ghi xếp hạng người sở hữu nhiều thỏi vàng vật phẩm 457 nhất trong túi và rương.
// (Java: THOI_VANG trong jbcd/dao/TopDAO.java; tổng vượt 2^64-1 được bão hòa tại ulong.MaxValue thay vì làm lỗi toàn bộ truy vấn MariaDB).
public sealed record TopGoldBarEntry(int PlayerId, int AccountId, string Name, int Gender, string ItemsBody, ulong BagQuantity, ulong BoxQuantity, ulong Total);
