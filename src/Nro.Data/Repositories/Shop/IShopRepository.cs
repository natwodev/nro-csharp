using Nro.Data.Db;
using Nro.Data.Models.Shop;
namespace Nro.Data.Repositories.Shop;

// Cung cấp hợp đồng tải cấu trúc cửa hàng bất biến cho tầng Game.
// Nguồn Java: jbcd/dao/ShopDAO.java (chỉ tải dữ liệu phân cấp; không mang trạng thái DAO Java).
public interface IShopRepository
{
    // Tải toàn bộ cấu trúc cửa hàng (Java: getShops).
    Task<IReadOnlyList<ShopDefinition>> LoadAsync(CancellationToken cancellationToken = default);
}
