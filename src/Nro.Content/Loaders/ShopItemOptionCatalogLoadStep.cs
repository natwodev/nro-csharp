using Nro.Content.Catalogs;
using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Data.Models.Shop;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp option rồi dựng cây shop bất biến. Nguồn Java: jbcd/dao/ShopDAO.java (dòng 112–132).
public sealed class ShopItemOptionCatalogLoadStep : ICatalogLoadStep
{
    public const string StepName = "item_shop_option";
    private readonly IStaticRowSource source;
    private readonly CommerceCatalogLoadState state;

    internal ShopItemOptionCatalogLoadStep(IStaticRowSource source, CommerceCatalogLoadState state)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        this.state = state ?? throw new ArgumentNullException(nameof(state));
    }

    public string Name => StepName;
    public IReadOnlyCollection<string> Dependencies { get; } = [ShopItemCatalogLoadStep.StepName];

    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<ShopRow> shops = state.Shops ?? throw Missing(ShopCatalogLoadStep.StepName);
        IReadOnlyList<TabShopRow> tabs = state.Tabs ?? throw Missing(ShopTabCatalogLoadStep.StepName);
        IReadOnlyList<ItemShopRow> items = state.ShopItems ?? throw Missing(ShopItemCatalogLoadStep.StepName);
        IReadOnlySet<int> allItemIds = state.AllShopItemIds ?? throw Missing(ShopItemCatalogLoadStep.StepName);
        IReadOnlyList<ItemShopOptionRow> rows = await source.ReadShopItemOptionsAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var itemIds = items.Select(row => row.ItemShopId).ToHashSet();
        var reachable = new List<ItemShopOptionRow>(rows.Count);
        int orphanCount = 0;
        int unreachableCount = 0;
        foreach (ItemShopOptionRow row in rows)
        {
            StaticCatalogValidation.Unique(ids, row.ItemShopOptionId, StepName);
            if (!itemIds.Contains(row.ItemShopId))
            {
                if (allItemIds.Contains(row.ItemShopId))
                {
                    unreachableCount++;
                }
                else
                {
                    orphanCount++;
                }

                continue;
            }

            try
            {
                _ = state.Items.GetItemOptionTemplate(row.OptionId);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new CatalogLoadException(StepName, $"Option shop id {row.ItemShopOptionId} tham chiếu option_id {row.OptionId} không tồn tại trong ItemCatalog.", exception);
            }

            reachable.Add(row);
        }

        var optionsByItem = reachable.GroupBy(row => row.ItemShopId).ToDictionary(group => group.Key, group =>
            (IReadOnlyList<ShopItemOptionDefinition>)Array.AsReadOnly(group.Select(row => new ShopItemOptionDefinition(row.OptionId, row.Param)).ToArray()));
        var itemsByTab = items.GroupBy(row => row.TabId).ToDictionary(group => group.Key, group =>
            (IReadOnlyList<ShopItemDefinition>)Array.AsReadOnly(group.Select(row => new ShopItemDefinition(
                row.ItemShopId,
                StaticCatalogValidation.Short(row.TempId, StepName, "temp_id", row.ItemShopId),
                row.IsNew != 0,
                row.Cost ?? 0,
                row.Costgold ?? 0,
                row.IconSpec ?? 0,
                StaticCatalogValidation.SByte(row.TypeSell ?? 0, StepName, "type_sell", row.ItemShopId),
                optionsByItem.GetValueOrDefault(row.ItemShopId, Array.Empty<ShopItemOptionDefinition>()))).ToArray()));
        var tabsByShop = tabs.GroupBy(row => row.ShopId).ToDictionary(group => group.Key, group =>
            (IReadOnlyList<ShopTabDefinition>)Array.AsReadOnly(group.Select(row => new ShopTabDefinition(
                row.TabShopId,
                row.Name.Replace("<>", "\n", StringComparison.Ordinal),
                itemsByTab.GetValueOrDefault(SourceTabId(row.TabShopId), Array.Empty<ShopItemDefinition>()))).ToArray()));
        builder.SetShops(new ShopCatalog(shops.Select(row => new ShopDefinition(
            row.ShopId,
            StaticCatalogValidation.SByte(row.NpcId, StepName, "npc_id", row.ShopId),
            row.TagName ?? String.Empty,
            StaticCatalogValidation.SByte(row.TypeShop ?? 0, StepName, "type_shop", row.ShopId),
            tabsByShop.GetValueOrDefault(row.ShopId, Array.Empty<ShopTabDefinition>())))));
        state.ShopItemOptions = reachable;
        return new CatalogStepResult(reachable.Count, Warnings(orphanCount, unreachableCount));
    }

    private static IEnumerable<string> Warnings(int orphanCount, int unreachableCount)
    {
        if (orphanCount != 0)
        {
            yield return $"{StepName}: bỏ qua {orphanCount} hàng mồ côi item_shop_id không tồn tại (Java đọc theo cây, không đọc các hàng này)";
        }

        if (unreachableCount != 0)
        {
            yield return $"{StepName}: bỏ qua {unreachableCount} hàng thuộc item_shop không đạt được (Java đọc theo cây, không đọc các hàng này)";
        }
    }

    private static int SourceTabId(int tabId) => tabId is >= 41 and <= 43 ? tabId - 31 : tabId;
    private static CatalogLoadException Missing(string step) => new(StepName, $"Thiếu dữ liệu từ bước '{step}'.");
}
