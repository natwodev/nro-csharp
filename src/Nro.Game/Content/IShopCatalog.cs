using Nro.Data.Models.Shop;

namespace Nro.Game.Content;

// Danh mục cửa hàng chỉ đọc theo NPC và mã cửa hàng. Nguồn Java: jbcd/dao/ShopDAO.java (dòng 20–132).
public interface IShopCatalog
{
    // Toàn bộ cửa hàng theo thứ tự truy vấn. (Java: SHOPS)
    IReadOnlyList<ShopDefinition> Shops { get; }

    // Lấy cửa hàng theo NPC, trả null khi NPC không có shop. (Java: ShopService.getShop)
    ShopDefinition? GetByNpc(int npcId);

    // Lấy cửa hàng theo mã. (Java: SHOPS)
    ShopDefinition Get(int id);
}
