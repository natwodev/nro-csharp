using Nro.Game.Content;

namespace Nro.Content.Catalogs;

// Danh mục thông báo bất biến, đã xếp mới nhất trước. Nguồn Java: nro/server/Manager.java (dòng 953–959).
public sealed class NotificationCatalog : INotificationCatalog
{
    private readonly IReadOnlyList<ServerNotification> latest;

    public NotificationCatalog(IEnumerable<ServerNotification> latest)
    {
        ArgumentNullException.ThrowIfNull(latest);
        this.latest = Array.AsReadOnly(latest.ToArray());
    }

    public IReadOnlyList<ServerNotification> Latest => latest;
}
