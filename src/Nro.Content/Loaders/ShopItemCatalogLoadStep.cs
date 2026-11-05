using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp item đang bán và resolve item template. Nguồn Java: jbcd/dao/ShopDAO.java (dòng 76–110).
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
        IReadOnlyList<ItemShopRow> rows = await source.ReadShopItemsAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var tabIds = tabs.Select(row => row.TabShopId).ToHashSet();
        foreach (ItemShopRow row in rows)
        {
            StaticCatalogValidation.Unique(ids, row.ItemShopId, StepName);
            if (!row.IsSell)
            {
                throw new CatalogLoadException(StepName, $"Item shop id {row.ItemShopId} không có is_sell=1.");
            }

            if (!tabIds.Contains(row.TabId))
            {
                throw new CatalogLoadException(StepName, $"Item shop id {row.ItemShopId} tham chiếu tab_id {row.TabId} không tồn tại.");
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
        }

        state.ShopItems = rows;
        return new CatalogStepResult(rows.Count);
    }

    private static CatalogLoadException Missing(string step) => new(StepName, $"Thiếu dữ liệu từ bước '{step}'.");
}
