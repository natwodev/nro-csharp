using Nro.Content.Catalogs;
using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Cards;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp thẻ radar, JSON body/options và mặc định constructor Java cho NULL. Nguồn Java: nro/server/Manager.java (dòng 961–994).
public sealed class RadarCatalogLoadStep : ICatalogLoadStep
{
    public const string StepName = "radar";
    private readonly IStaticRowSource source;

    public RadarCatalogLoadStep(IStaticRowSource source)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public string Name => StepName;
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<RadarRow> rows = await source.ReadRadarCardsAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var cards = new List<RadarCard>(rows.Count);
        foreach (RadarRow row in rows)
        {
            StaticCatalogValidation.Unique(ids, row.RadarId, StepName);
            int id = StaticCatalogValidation.Short(row.RadarId, StepName, "id", row.RadarId);
            short iconId = StaticCatalogValidation.Short(row.IconId ?? -1, StepName, "icon_id", row.RadarId);
            sbyte rank = StaticCatalogValidation.SByte(row.Rank ?? 0, StepName, "rank", row.RadarId);
            sbyte max = StaticCatalogValidation.SByte(row.Max ?? 0, StepName, "max", row.RadarId);
            sbyte type = StaticCatalogValidation.SByte(row.Type ?? 0, StepName, "type", row.RadarId);
            short template = StaticCatalogValidation.Short(row.MobId ?? 1, StepName, "mob_id", row.RadarId);
            (short head, short body, short leg, short bag) = CommerceCatalogJson.RadarBody(row.Body ?? "[]", row.RadarId);
            cards.Add(new RadarCard(id, iconId, rank, max, type, template, head, body, leg, bag, row.Name ?? String.Empty, row.Info ?? String.Empty, CommerceCatalogJson.RadarOptions(row.Options ?? "[]", row.RadarId), StaticCatalogValidation.Short(row.AuraId ?? -1, StepName, "aura_id", row.RadarId)));
        }

        builder.SetRadar(new RadarCatalog(cards));
        return new CatalogStepResult(rows.Count);
    }
}
