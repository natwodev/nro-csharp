using Nro.Core.Templates;

namespace Nro.Game.Items.Core;

// Biểu diễn phần dữ liệu mục cửa hàng mà mã vật phẩm cần: mẫu và danh sách tùy chọn.
// Nguồn Java: nro/shop/ItemShop (lớp phụ trợ C#; ItemCatalog giữ các mục theo thứ tự shop, tab rồi vật phẩm để bảo toàn thứ tự duyệt Manager.SHOPS).
public sealed record ShopItemEntry(ItemTemplate Template, IReadOnlyList<ItemOption> Options);
