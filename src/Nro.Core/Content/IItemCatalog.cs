using Nro.Core.Templates;

namespace Nro.Core.Content;

// Hợp đồng đọc các mẫu vật phẩm tĩnh, không phụ thuộc tầng dữ liệu. Nguồn Java: nro/server/Manager.java (dòng 192–218).
public interface IItemCatalog
{
    // Danh sách mẫu vật phẩm theo thứ tự id. (Java: ITEM_TEMPLATES)
    IReadOnlyList<ItemTemplate> ItemTemplates { get; }

    // Danh sách mẫu tùy chọn vật phẩm theo thứ tự id. (Java: ITEM_OPTION_TEMPLATES)
    IReadOnlyList<ItemOptionTemplate> ItemOptionTemplates { get; }

    // Các vật trang trí nền theo id. (Java: BG_ITEMS)
    IReadOnlyList<BgItem> BgItems { get; }

    // Ánh xạ vật phẩm thú cưỡi sang mã part giao thức. (Java: DataGame.MAP_MOUNT_NUM)
    IReadOnlyDictionary<short, int> MountItemParts { get; }

    // Tra mẫu vật phẩm theo id đã được kiểm liên tục khi nạp. (Java: ITEM_TEMPLATES.get)
    ItemTemplate GetItemTemplate(int id);

    // Tra mẫu tùy chọn theo id đã được kiểm liên tục khi nạp. (Java: ITEM_OPTION_TEMPLATES.get)
    ItemOptionTemplate GetItemOptionTemplate(int id);

    // Tra vật trang trí nền theo id. (Java: BG_ITEMS)
    BgItem GetBgItem(int id);

    // Thử lấy part giao thức của thú cưỡi. (Java: MAP_MOUNT_NUM.get)
    bool TryGetMountPart(short itemId, out int part);
}
