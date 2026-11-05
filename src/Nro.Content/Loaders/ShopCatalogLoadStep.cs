using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp hàng shop gốc theo thứ tự NPC Java. Nguồn Java: jbcd/dao/ShopDAO.java (dòng 20–47).
public sealed class ShopCatalogLoadStep : ICatalogLoadStep
{
    public const string StepName = "shop";
    private readonly IStaticRowSource source;
    private readonly CommerceCatalogLoadState state;

    internal ShopCatalogLoadStep(IStaticRowSource source, CommerceCatalogLoadState state)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        this.state = state ?? throw new ArgumentNullException(nameof(state));
    }

    public string Name => StepName;
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<ShopRow> rows = await source.ReadShopsAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        foreach (ShopRow row in rows)
        {
            StaticCatalogValidation.Unique(ids, row.ShopId, StepName);
            _ = StaticCatalogValidation.SByte(row.NpcId, StepName, "npc_id", row.ShopId);
            _ = StaticCatalogValidation.SByte(row.TypeShop ?? 0, StepName, "type_shop", row.ShopId);
        }

        state.Shops = rows;
        return new CatalogStepResult(rows.Count);
    }
}
