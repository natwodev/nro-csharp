using Nro.Core.Content;
using Nro.Data.Entities.StaticData;
using Nro.Game.Progress.Badges;

namespace Nro.Content.Loaders;

// Trạng thái cục bộ nối các bước J-01D trước khi công bố catalog. Nguồn Java: nro/server/Manager.java (dòng 723–810, 953–994).
internal sealed class CommerceCatalogLoadState
{
    public CommerceCatalogLoadState(IItemCatalog items)
    {
        Items = items ?? throw new ArgumentNullException(nameof(items));
    }

    public IItemCatalog Items { get; }
    public IReadOnlyList<ShopRow>? Shops { get; set; }
    public IReadOnlyList<TabShopRow>? Tabs { get; set; }
    public IReadOnlySet<int>? AllShopTabIds { get; set; }
    public IReadOnlyList<ItemShopRow>? ShopItems { get; set; }
    public IReadOnlySet<int>? AllShopItemIds { get; set; }
    public IReadOnlyList<ItemShopOptionRow>? ShopItemOptions { get; set; }
    public IReadOnlyList<BadgeTaskTemplate>? BadgeTasks { get; set; }
}
