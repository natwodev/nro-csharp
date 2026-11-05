using Nro.Core.Content;

namespace Nro.Game.Content;

// Bộ dựng cục bộ chỉ dùng trong một lần nạp trước khi công bố snapshot. Nguồn Java: nro/server/Manager.java (dòng 192–218).
public sealed class StaticCatalogBuilder
{
    private IItemCatalog items = EmptyStaticCatalogs.Instance;
    private IWorldCatalog world = EmptyStaticCatalogs.Instance;
    private IAppearanceCatalog appearance = EmptyStaticCatalogs.Instance;
    private IImageCatalog images = EmptyStaticCatalogs.Instance;
    private ISkillCatalog skills = EmptyStaticCatalogs.Instance;
    private ITaskCatalog tasks = EmptyStaticCatalogs.Instance;
    private IShopCatalog shops = EmptyStaticCatalogs.Instance;
    private IProgressionCatalog progression = EmptyStaticCatalogs.Instance;
    private IBadgesCatalog badges = EmptyStaticCatalogs.Instance;
    private INotificationCatalog notifications = EmptyStaticCatalogs.Instance;
    private IRadarCatalog radar = EmptyStaticCatalogs.Instance;

    // Gán danh mục vật phẩm cho snapshot đang dựng. (Java: ITEM_TEMPLATES)
    public void SetItems(IItemCatalog value) => items = value ?? throw new ArgumentNullException(nameof(value));

    // Gán danh mục thế giới cho snapshot đang dựng. (Java: MAP_TEMPLATES)
    public void SetWorld(IWorldCatalog value) => world = value ?? throw new ArgumentNullException(nameof(value));

    // Gán danh mục ngoại hình cho snapshot đang dựng. (Java: parts)
    public void SetAppearance(IAppearanceCatalog value) => appearance = value ?? throw new ArgumentNullException(nameof(value));

    // Gán danh mục ảnh cho snapshot đang dựng. (Java: IMAGES_BY_NAME)
    public void SetImages(IImageCatalog value) => images = value ?? throw new ArgumentNullException(nameof(value));

    // Gán danh mục kỹ năng cho snapshot đang dựng. (Java: NCLASS)
    public void SetSkills(ISkillCatalog value) => skills = value ?? throw new ArgumentNullException(nameof(value));

    // Gán danh mục nhiệm vụ cho snapshot đang dựng. (Java: TASKS)
    public void SetTasks(ITaskCatalog value) => tasks = value ?? throw new ArgumentNullException(nameof(value));

    // Gán danh mục cửa hàng cho snapshot đang dựng. (Java: SHOPS)
    public void SetShops(IShopCatalog value) => shops = value ?? throw new ArgumentNullException(nameof(value));

    // Gán danh mục tiến trình cho snapshot đang dựng. (Java: INTRINSICS)
    public void SetProgression(IProgressionCatalog value) => progression = value ?? throw new ArgumentNullException(nameof(value));

    // Gán danh mục huy hiệu cho snapshot đang dựng. (Java: BAGES_TEMPLATES)
    public void SetBadges(IBadgesCatalog value) => badges = value ?? throw new ArgumentNullException(nameof(value));

    // Gán danh mục thông báo cho snapshot đang dựng. (Java: NOTIFY)
    public void SetNotifications(INotificationCatalog value) => notifications = value ?? throw new ArgumentNullException(nameof(value));

    // Gán danh mục radar cho snapshot đang dựng. (Java: RADAR_TEMPLATE)
    public void SetRadar(IRadarCatalog value) => radar = value ?? throw new ArgumentNullException(nameof(value));

    // Đóng bộ dựng thành snapshot chỉ đọc mới. (Java: loadDatabase)
    public IStaticCatalogs Build() => new StaticCatalogSnapshot(items, world, appearance, images, skills, tasks, shops, progression, badges, notifications, radar);
}
