namespace Nro.Data.Models.Shop;

// Dữ liệu một tùy chọn của vật phẩm cửa hàng.
// Nguồn Java: jbcd/dao/ShopDAO.java (ShopDAO.loadItemShopOption).
// Id tùy chọn vật phẩm (Java: option_id).
// Tham số của tùy chọn.
public sealed record ShopItemOptionDefinition(int OptionId, int Param);
