using Nro.Core.Content;

namespace Nro.Game.Content;

// Snapshot gốc chỉ đọc của mọi danh mục tĩnh mà Game tiêu thụ. Nguồn Java: nro/server/Manager.java (dòng 192–218).
public interface IStaticCatalogs
{
    // Danh mục vật phẩm dùng kiểu miền Nro.Core.Templates. (Java: ITEM_TEMPLATES)
    IItemCatalog Items { get; }

    // Danh mục thế giới dùng kiểu miền Nro.Game. (Java: MAP_TEMPLATES)
    IWorldCatalog World { get; }

    // Danh mục ngoại hình dùng kiểu miền Nro.Core.Templates. (Java: parts)
    IAppearanceCatalog Appearance { get; }

    // Danh mục ảnh theo tên. (Java: IMAGES_BY_NAME)
    IImageCatalog Images { get; }

    // Danh mục kỹ năng dùng kiểu miền Nro.Game.Skills. (Java: NCLASS)
    ISkillCatalog Skills { get; }

    // Danh mục nhiệm vụ dùng kiểu miền Nro.Game.Progress.Tasks. (Java: TASKS)
    ITaskCatalog Tasks { get; }

    // Danh mục cửa hàng dùng kiểu miền Nro.Game. (Java: SHOPS)
    IShopCatalog Shops { get; }

    // Danh mục tiến trình dùng kiểu miền Nro.Game.Skills và Nro.Game.Progress. (Java: INTRINSICS)
    IProgressionCatalog Progression { get; }

    // Danh mục thông báo máy chủ. (Java: NOTIFY)
    INotificationCatalog Notifications { get; }

    // Danh mục thẻ radar. (Java: RADAR_TEMPLATE)
    IRadarCatalog Radar { get; }
}
