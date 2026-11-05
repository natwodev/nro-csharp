using Nro.Content.Catalogs;
using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;
using Nro.Game.Items.Core;
using Nro.Game.Progress.Badges;

namespace Nro.Content.Loaders;

// Nạp mẫu huy hiệu, parse JSON option và công bố catalog. Nguồn Java: nro/server/Manager.java (dòng 790–810).
public sealed class BadgesCatalogLoadStep : ICatalogLoadStep
{
    public const string StepName = "data_badges";
    private readonly IStaticRowSource source;
    private readonly CommerceCatalogLoadState state;

    internal BadgesCatalogLoadStep(IStaticRowSource source, CommerceCatalogLoadState state)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        this.state = state ?? throw new ArgumentNullException(nameof(state));
    }

    public string Name => StepName;
    public IReadOnlyCollection<string> Dependencies { get; } = [BadgeTaskCatalogLoadStep.StepName];

    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<BadgeTaskTemplate> tasks = state.BadgeTasks ?? throw new CatalogLoadException(StepName, $"Thiếu dữ liệu từ bước '{BadgeTaskCatalogLoadStep.StepName}'.");
        IReadOnlyList<DataBadgesRow> rows = await source.ReadBadgeTemplatesAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var templates = new List<BadgeTemplate>(rows.Count);
        foreach (DataBadgesRow row in rows)
        {
            StaticCatalogValidation.Unique(ids, row.DataBadgesId, StepName);
            // Java chỉ giữ idItem để so khớp hiệu ứng; không tra ItemService khi nạp data_badges.
            IReadOnlyList<ItemOption> options = CommerceCatalogJson.BadgeOptions(row.Options ?? String.Empty, row.DataBadgesId, optionId =>
            {
                try
                {
                    return new ItemOption(state.Items.GetItemOptionTemplate(optionId), 0);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    throw new CatalogLoadException(StepName, $"Huy hiệu id {row.DataBadgesId} tham chiếu option id {optionId} không tồn tại trong ItemCatalog.", exception);
                }
            });
            templates.Add(new BadgeTemplate(row.DataBadgesId, row.IdEffect, row.IdItem, row.Name ?? String.Empty, options));
        }

        builder.SetBadges(new Nro.Content.Catalogs.BadgesCatalog(tasks, templates));
        return new CatalogStepResult(rows.Count);
    }
}
