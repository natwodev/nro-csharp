namespace Nro.Data.Models.Shop;

// Dữ liệu vật phẩm của một tab cửa hàng và các tùy chọn kèm theo.
// Nguồn Java: jbcd/dao/ShopDAO.java (ShopDAO.loadItemShop giữ thứ tự dữ liệu do truy vấn trả về).
// Id bản ghi vật phẩm cửa hàng.
// Id mẫu vật phẩm (Java: temp_id).
// Cờ vật phẩm mới (Java: is_new).
// Giá tiền của vật phẩm.
// Giá vàng của vật phẩm (Java: costgold).
// Id biểu tượng đặc biệt (Java: icon_spec).
// Kiểu bán (Java: type_sell).
// Danh sách tùy chọn vật phẩm.
public sealed record ShopItemDefinition(int Id, short TemplateId, bool IsNew, int Cost, int CostGold, int IconSpec, sbyte TypeSell, IReadOnlyList<ShopItemOptionDefinition> Options);
