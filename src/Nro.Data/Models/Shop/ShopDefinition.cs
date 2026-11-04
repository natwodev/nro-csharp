namespace Nro.Data.Models.Shop;

// Biểu diễn chỉ dữ liệu cho cây cửa hàng gồm cửa hàng, tab, vật phẩm và tùy chọn.
// Nguồn Java: jbcd/dao/ShopDAO.java (cây dữ liệu tách khỏi template vật phẩm của Nro.Game; thứ tự phần tử do truy vấn trong ShopRepository bảo toàn).
// Id cửa hàng.
// Id NPC sở hữu cửa hàng.
// Nhãn phân loại cửa hàng.
// Kiểu cửa hàng.
// Danh sách tab của cửa hàng.
public sealed record ShopDefinition(int Id, sbyte NpcId, string TagName, sbyte TypeShop, IReadOnlyList<ShopTabDefinition> Tabs);
