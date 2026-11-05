using Nro.Core.Templates;

namespace Nro.Core.Content;

// Hợp đồng đọc các mẫu vật phẩm tĩnh, không phụ thuộc tầng dữ liệu. Nguồn Java: nro/server/Manager.java (dòng 192–218).
public interface IItemCatalog
{
    // Danh sách mẫu vật phẩm theo thứ tự id. (Java: ITEM_TEMPLATES)
    IReadOnlyList<ItemTemplate> ItemTemplates { get; }

    // Danh sách mẫu tùy chọn vật phẩm theo thứ tự id. (Java: ITEM_OPTION_TEMPLATES)
    IReadOnlyList<ItemOptionTemplate> ItemOptionTemplates { get; }
}
