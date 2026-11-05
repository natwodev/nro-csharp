using System.Collections.ObjectModel;
using Nro.Core.Content;
using Nro.Core.Templates;

namespace Nro.Content.Catalogs;

// Danh mục vật phẩm chỉ đọc, được dựng sau khi kiểm tính liên tục của id. Nguồn Java: nro/server/Manager.java (dòng 397–436, 727–741).
public sealed class ItemCatalog : IItemCatalog
{
    private readonly ReadOnlyCollection<ItemTemplate> itemTemplates;
    private readonly ReadOnlyCollection<ItemOptionTemplate> itemOptionTemplates;
    private readonly ReadOnlyCollection<BgItem> bgItems;
    private readonly IReadOnlyDictionary<int, BgItem> bgItemsById;
    private readonly IReadOnlyDictionary<short, int> mountItemParts;

    // Đóng danh mục từ dữ liệu đã xác thực bởi các bước nạp. (Java: ITEM_TEMPLATES, ITEM_OPTION_TEMPLATES, BG_ITEMS)
    public ItemCatalog(IEnumerable<ItemTemplate> itemTemplates, IEnumerable<ItemOptionTemplate> itemOptionTemplates, IEnumerable<BgItem> bgItems, IReadOnlyDictionary<short, int> mountItemParts)
    {
        ArgumentNullException.ThrowIfNull(itemTemplates);
        ArgumentNullException.ThrowIfNull(itemOptionTemplates);
        ArgumentNullException.ThrowIfNull(bgItems);
        ArgumentNullException.ThrowIfNull(mountItemParts);

        this.itemTemplates = Array.AsReadOnly(itemTemplates.ToArray());
        this.itemOptionTemplates = Array.AsReadOnly(itemOptionTemplates.ToArray());
        this.bgItems = Array.AsReadOnly(bgItems.ToArray());
        bgItemsById = new ReadOnlyDictionary<int, BgItem>(this.bgItems.ToDictionary(item => item.Id));
        this.mountItemParts = new ReadOnlyDictionary<short, int>(new Dictionary<short, int>(mountItemParts));
    }

    // Mẫu vật phẩm theo vị trí id. (Java: ITEM_TEMPLATES)
    public IReadOnlyList<ItemTemplate> ItemTemplates => itemTemplates;

    // Mẫu option theo vị trí id. (Java: ITEM_OPTION_TEMPLATES)
    public IReadOnlyList<ItemOptionTemplate> ItemOptionTemplates => itemOptionTemplates;

    // Vật trang trí nền theo thứ tự id từ truy vấn. (Java: BG_ITEMS)
    public IReadOnlyList<BgItem> BgItems => bgItems;

    // Ánh xạ thú cưỡi chỉ đọc. (Java: DataGame.MAP_MOUNT_NUM)
    public IReadOnlyDictionary<short, int> MountItemParts => mountItemParts;

    // Lấy mẫu vật phẩm bằng vị trí id. (Java: ITEM_TEMPLATES.get)
    public ItemTemplate GetItemTemplate(int id) => (uint)id < (uint)itemTemplates.Count
        ? itemTemplates[id]
        : throw new ArgumentOutOfRangeException(nameof(id), id, "Không có mẫu vật phẩm tại id đã cho.");

    // Lấy mẫu option bằng vị trí id. (Java: ITEM_OPTION_TEMPLATES.get)
    public ItemOptionTemplate GetItemOptionTemplate(int id) => (uint)id < (uint)itemOptionTemplates.Count
        ? itemOptionTemplates[id]
        : throw new ArgumentOutOfRangeException(nameof(id), id, "Không có mẫu tùy chọn vật phẩm tại id đã cho.");

    // Lấy vật trang trí nền theo id. (Java: BG_ITEMS)
    public BgItem GetBgItem(int id) => bgItemsById.TryGetValue(id, out BgItem? value)
        ? value
        : throw new KeyNotFoundException($"Không có vật trang trí nền id {id}.");

    // Thử lấy mã part giao thức của thú cưỡi. (Java: MAP_MOUNT_NUM.get)
    public bool TryGetMountPart(short itemId, out int part) => mountItemParts.TryGetValue(itemId, out part);
}
