using Nro.Data.Db;
namespace Nro.Data.Repositories.Shop;

// Biểu diễn chỉ dữ liệu cho cây cửa hàng gồm cửa hàng, tab, vật phẩm và tùy chọn.
// Nguồn Java: jbcd/dao/ShopDAO.java (cây dữ liệu tách khỏi template vật phẩm của Nro.Game; thứ tự phần tử do truy vấn trong ShopRepository bảo toàn).
// Id cửa hàng.
// Id NPC sở hữu cửa hàng.
// Nhãn phân loại cửa hàng.
// Kiểu cửa hàng.
// Danh sách tab của cửa hàng.
public sealed record ShopDefinition(int Id, sbyte NpcId, string TagName, sbyte TypeShop, IReadOnlyList<ShopTabDefinition> Tabs);

// Id tab cửa hàng.
// Tên tab, trong đó <> đã đổi thành xuống dòng khi tải.
// Danh sách vật phẩm của tab.
public sealed record ShopTabDefinition(int Id, string Name, IReadOnlyList<ShopItemDefinition> Items);

// Id bản ghi vật phẩm cửa hàng.
// Id mẫu vật phẩm (Java: temp_id).
// Cờ vật phẩm mới (Java: is_new).
// Giá tiền của vật phẩm.
// Giá vàng của vật phẩm (Java: costgold).
// Id biểu tượng đặc biệt (Java: icon_spec).
// Kiểu bán (Java: type_sell).
// Danh sách tùy chọn vật phẩm.
public sealed record ShopItemDefinition(int Id, short TemplateId, bool IsNew, int Cost, int CostGold, int IconSpec, sbyte TypeSell, IReadOnlyList<ShopItemOptionDefinition> Options);

// Id tùy chọn vật phẩm (Java: option_id).
// Tham số của tùy chọn.
public sealed record ShopItemOptionDefinition(int OptionId, int Param);
