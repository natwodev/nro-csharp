using Nro.Content.Catalogs;
using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp thông báo mới nhất trước và ghép name<>text ở kiểu miền. Nguồn Java: nro/server/Manager.java (dòng 953–959).
public sealed class NotificationCatalogLoadStep : ICatalogLoadStep
{
    public const string StepName = "notify";
    private readonly IStaticRowSource source;

    public NotificationCatalogLoadStep(IStaticRowSource source)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public string Name => StepName;
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<NotifyRow> rows = await source.ReadNotificationsAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var notifications = new List<ServerNotification>(rows.Count);
        foreach (NotifyRow row in rows)
        {
            StaticCatalogValidation.Unique(ids, row.NotifyId, StepName);
            notifications.Add(new ServerNotification(row.NotifyId, row.Name ?? String.Empty, row.Text ?? String.Empty));
        }

        builder.SetNotifications(new NotificationCatalog(notifications));
        return new CatalogStepResult(rows.Count);
    }
}
