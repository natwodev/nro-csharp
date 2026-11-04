namespace Nro.Data.Models.Shop;

// Dữ liệu tab cửa hàng gồm nhãn và danh sách vật phẩm.
// Nguồn Java: jbcd/dao/ShopDAO.java (ShopDAO.loadShopTab đổi <> thành xuống dòng khi tải).
// Id tab cửa hàng.
// Tên tab, trong đó <> đã đổi thành xuống dòng khi tải.
// Danh sách vật phẩm của tab.
public sealed record ShopTabDefinition(int Id, string Name, IReadOnlyList<ShopItemDefinition> Items);
