using Nro.Content.StaticData;
using Nro.Core.Content;

namespace Nro.Content.Loaders;

// Tập bước J-01D, dùng ItemCatalog đã nạp ở J-01B để kiểm FK. Nguồn Java: nro/server/Manager.java (dòng 723–810, 953–994).
public static class CommerceBadgeNotificationRadarCatalogSteps
{
    // Tạo loader có ExpectedCounts J-01D. (Java: loadDatabase)
    public static StaticCatalogLoader CreateLoader(IStaticRowSource source, IItemCatalog items) => new(Create(source, items), ExpectedCounts.CommerceBadgesNotificationsRadar);

    // Tạo các bước độc lập để bootstrap tổng ghép vào loader chung. (Java: loadDatabase)
    public static IReadOnlyList<ICatalogLoadStep> Create(IStaticRowSource source, IItemCatalog items)
    {
        ArgumentNullException.ThrowIfNull(source);
        var state = new CommerceCatalogLoadState(items);
        return [
            new ShopCatalogLoadStep(source, state),
            new ShopTabCatalogLoadStep(source, state),
            new ShopItemCatalogLoadStep(source, state),
            new ShopItemOptionCatalogLoadStep(source, state),
            new BadgeTaskCatalogLoadStep(source, state),
            new BadgesCatalogLoadStep(source, state),
            new NotificationCatalogLoadStep(source),
            new RadarCatalogLoadStep(source),
        ];
    }
}
