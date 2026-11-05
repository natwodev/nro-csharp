using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp tab shop và kiểm khóa ngoại shop. Nguồn Java: jbcd/dao/ShopDAO.java (dòng 49–74).
public sealed class ShopTabCatalogLoadStep : ICatalogLoadStep
{
    public const string StepName = "tab_shop";
    private readonly IStaticRowSource source;
    private readonly CommerceCatalogLoadState state;

    internal ShopTabCatalogLoadStep(IStaticRowSource source, CommerceCatalogLoadState state)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        this.state = state ?? throw new ArgumentNullException(nameof(state));
    }

    public string Name => StepName;
    public IReadOnlyCollection<string> Dependencies { get; } = [ShopCatalogLoadStep.StepName];

    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<ShopRow> shops = state.Shops ?? throw Missing(ShopCatalogLoadStep.StepName);
        IReadOnlyList<TabShopRow> rows = await source.ReadShopTabsAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var shopIds = shops.Select(row => row.ShopId).ToHashSet();
        foreach (TabShopRow row in rows)
        {
            StaticCatalogValidation.Unique(ids, row.TabShopId, StepName);
            if (!shopIds.Contains(row.ShopId))
            {
                throw new CatalogLoadException(StepName, $"Tab id {row.TabShopId} tham chiếu shop_id {row.ShopId} không tồn tại.");
            }
        }

        state.Tabs = rows;
        return new CatalogStepResult(rows.Count);
    }

    private static CatalogLoadException Missing(string step) => new(StepName, $"Thiếu dữ liệu từ bước '{step}'.");
}
