using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;
using Nro.Game.Progress.Badges;

namespace Nro.Content.Loaders;

// Nạp điều kiện nhiệm vụ huy hiệu theo id. Nguồn Java: nro/server/Manager.java (dòng 777–788).
public sealed class BadgeTaskCatalogLoadStep : ICatalogLoadStep
{
    public const string StepName = "task_badges_template";
    private readonly IStaticRowSource source;
    private readonly CommerceCatalogLoadState state;

    internal BadgeTaskCatalogLoadStep(IStaticRowSource source, CommerceCatalogLoadState state)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        this.state = state ?? throw new ArgumentNullException(nameof(state));
    }

    public string Name => StepName;
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<TaskBadgesTemplateRow> rows = await source.ReadBadgeTaskTemplatesAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var tasks = new List<BadgeTaskTemplate>(rows.Count);
        foreach (TaskBadgesTemplateRow row in rows)
        {
            StaticCatalogValidation.Unique(ids, row.TaskBadgesTemplateId, StepName);
            tasks.Add(new BadgeTaskTemplate(row.TaskBadgesTemplateId, row.Name ?? String.Empty, row.MaxCount, row.IdBadgesReward));
        }

        state.BadgeTasks = tasks.AsReadOnly();
        return new CatalogStepResult(rows.Count);
    }
}
