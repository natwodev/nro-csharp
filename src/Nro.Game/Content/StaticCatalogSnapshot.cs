using Nro.Core.Content;

namespace Nro.Game.Content;

// Snapshot danh mục bất biến theo tham chiếu sau khi được công bố. Nguồn Java: nro/server/Manager.java (dòng 192–218).
public sealed class StaticCatalogSnapshot : IStaticCatalogs
{
    // Khởi tạo snapshot với toàn bộ danh mục con đã hoàn tất. (Java: loadDatabase)
    public StaticCatalogSnapshot(IItemCatalog items, IWorldCatalog world, IAppearanceCatalog appearance, IImageCatalog images, ISkillCatalog skills, ITaskCatalog tasks, IShopCatalog shops, IProgressionCatalog progression, INotificationCatalog notifications, IRadarCatalog radar)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
        World = world ?? throw new ArgumentNullException(nameof(world));
        Appearance = appearance ?? throw new ArgumentNullException(nameof(appearance));
        Images = images ?? throw new ArgumentNullException(nameof(images));
        Skills = skills ?? throw new ArgumentNullException(nameof(skills));
        Tasks = tasks ?? throw new ArgumentNullException(nameof(tasks));
        Shops = shops ?? throw new ArgumentNullException(nameof(shops));
        Progression = progression ?? throw new ArgumentNullException(nameof(progression));
        Notifications = notifications ?? throw new ArgumentNullException(nameof(notifications));
        Radar = radar ?? throw new ArgumentNullException(nameof(radar));
    }

    // Danh mục vật phẩm của snapshot. (Java: ITEM_TEMPLATES)
    public IItemCatalog Items { get; }

    // Danh mục thế giới của snapshot. (Java: MAP_TEMPLATES)
    public IWorldCatalog World { get; }

    // Danh mục ngoại hình của snapshot. (Java: parts)
    public IAppearanceCatalog Appearance { get; }

    // Danh mục ảnh theo tên của snapshot. (Java: IMAGES_BY_NAME)
    public IImageCatalog Images { get; }

    // Danh mục kỹ năng của snapshot. (Java: NCLASS)
    public ISkillCatalog Skills { get; }

    // Danh mục nhiệm vụ của snapshot. (Java: TASKS)
    public ITaskCatalog Tasks { get; }

    // Danh mục cửa hàng của snapshot. (Java: SHOPS)
    public IShopCatalog Shops { get; }

    // Danh mục tiến trình của snapshot. (Java: INTRINSICS)
    public IProgressionCatalog Progression { get; }

    // Danh mục thông báo của snapshot. (Java: NOTIFY)
    public INotificationCatalog Notifications { get; }

    // Danh mục radar của snapshot. (Java: RADAR_TEMPLATE)
    public IRadarCatalog Radar { get; }
}
