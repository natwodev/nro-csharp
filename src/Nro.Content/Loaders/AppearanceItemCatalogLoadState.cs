using Nro.Core.Templates;
using Nro.Content.Catalogs;

namespace Nro.Content.Loaders;

// Trạng thái riêng của một graph bước nạp, không được công bố cùng snapshot. Nguồn Java: nro/server/Manager.java (dòng 351–436, 560–600, 727–741).
internal sealed class AppearanceItemCatalogLoadState
{
    // Part đã parse. (Java: parts)
    public IReadOnlyList<Part>? Parts { get; set; }

    // Nền đã chuyển kiểu miền. (Java: BG_ITEMS)
    public IReadOnlyList<BgItem>? BgItems { get; set; }

    // Mẫu vật phẩm đã chuyển kiểu miền. (Java: ITEM_TEMPLATES)
    public IReadOnlyList<ItemTemplate>? ItemTemplates { get; set; }

    // Mẫu option đã chuyển kiểu miền. (Java: ITEM_OPTION_TEMPLATES)
    public IReadOnlyList<ItemOptionTemplate>? ItemOptionTemplates { get; set; }

    // Avatar đầu đã chuyển kiểu miền. (Java: HEAD_AVATARS)
    public IReadOnlyList<HeadAvatar>? HeadAvatars { get; set; }

    // Túi cờ đã parse CSV. (Java: FLAGS_BAGS)
    public IReadOnlyList<FlagBag>? FlagBags { get; set; }

    // Frame đầu đã parse JSON. (Java: ARR_HEAD_2_FRAMES)
    public IReadOnlyList<ArrHead2Frames>? HeadFrames { get; set; }

    // Catalog ảnh đã đóng. (Java: IMAGES_BY_NAME)
    public ImageCatalog? Images { get; set; }
}
