namespace Nro.Game.Content;

// Danh mục thông báo máy chủ chỉ đọc. Nguồn Java: nro/server/Manager.java (dòng 953–959).
public interface INotificationCatalog
{
    // Thông báo mới nhất đứng trước theo id giảm dần. (Java: NOTIFY)
    IReadOnlyList<ServerNotification> Latest { get; }
}
