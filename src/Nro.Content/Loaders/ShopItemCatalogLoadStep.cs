using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp item đạt được từ tab shop và resolve item template. Nguồn Java: jbcd/dao/ShopDAO.java (dòng 76–110).
public sealed class ShopItemCatalogLoadStep : ICatalogLoadStep
{
    public const string StepName = "item_shop";
    private readonly IStaticRowSource source;
    private readonly CommerceCatalogLoadState state;

    internal ShopItemCatalogLoadStep(IStaticRowSource source, CommerceCatalogLoadState state)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        this.state = state ?? throw new ArgumentNullException(nameof(state));
    }

    public string Name => StepName;
    public IReadOnlyCollection<string> Dependencies { get; } = [ShopTabCatalogLoadStep.StepName];

    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<TabShopRow> tabs = state.Tabs ?? throw Missing(ShopTabCatalogLoadStep.StepName);
        IReadOnlySet<int> allTabIds = state.AllShopTabIds ?? throw Missing(ShopTabCatalogLoadStep.StepName);
        IReadOnlyList<ItemShopRow> rows = await source.ReadShopItemsAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var sourceTabIds = tabs.Select(row => SourceTabId(row.TabShopId)).ToHashSet();
        var reachable = new List<ItemShopRow>(rows.Count);
        int orphanCount = 0;
        int notForSaleCount = 0;
        int unreachableCount = 0;
        int quirkUnreadCount = 0;
        foreach (ItemShopRow row in rows)
        {
            StaticCatalogValidation.Unique(ids, row.ItemShopId, StepName);
            if (!row.IsSell)
            {
                notForSaleCount++;
                continue;
            }

            if (!allTabIds.Contains(row.TabId))
            {
                orphanCount++;
                continue;
            }

            if (!sourceTabIds.Contains(row.TabId))
            {
                unreachableCount++;
                if (row.TabId is >= 41 and <= 43)
                {
                    quirkUnreadCount++;
                }

                continue;
            }

            int templateId = StaticCatalogValidation.Short(row.TempId, StepName, "temp_id", row.ItemShopId);
            try
            {
                _ = state.Items.GetItemTemplate(templateId);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new CatalogLoadException(StepName, $"Item shop id {row.ItemShopId} tham chiếu temp_id {templateId} không tồn tại trong ItemCatalog.", exception);
            }

            _ = StaticCatalogValidation.SByte(row.TypeSell ?? 0, StepName, "type_sell", row.ItemShopId);
            reachable.Add(row);
        }

        state.AllShopItemIds = ids;
        state.ShopItems = reachable
            .OrderBy(row => row.TabId)
            .ThenByDescending(row => row.CreateTime)
            .ThenBy(row => row.ItemShopId)
            .ToArray();
        return new CatalogStepResult(state.ShopItems.Count, Warnings(orphanCount, notForSaleCount, unreachableCount, quirkUnreadCount));
    }

    // Java đổi 41–43 thành 10–12 trước khi truy vấn item_shop.
    private static int SourceTabId(int tabId) => tabId is >= 41 and <= 43 ? tabId - 31 : tabId;

    private static IEnumerable<string> Warnings(int orphanCount, int notForSaleCount, int unreachableCount, int quirkUnreadCount)
    {
        if (orphanCount != 0)
        {
            yield return $"{StepName}: bỏ qua {orphanCount} hàng mồ côi tab_id không tồn tại (Java đọc theo cây, không đọc các hàng này)";
        }

        if (notForSaleCount != 0)
        {
            yield return $"{StepName}: bỏ qua {notForSaleCount} hàng is_sell=false (Java đọc theo cây, không đọc các hàng này)";
        }

        if (unreachableCount != 0)
        {
            yield return $"{StepName}: bỏ qua {unreachableCount} hàng không đạt được từ shop->tab (Java đọc theo cây, không đọc các hàng này)";
        }

        if (quirkUnreadCount != 0)
        {
            yield return $"{StepName}: {quirkUnreadCount} hàng của tab 41–43 không được Java đọc (quirk)";
        }
    }

    private static CatalogLoadException Missing(string step) => new(StepName, $"Thiếu dữ liệu từ bước '{step}'.");
}
