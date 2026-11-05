namespace Nro.Game.Content;

// Thông báo máy chủ ghép đúng dữ liệu name<>text của Java. Nguồn Java: nro/server/Manager.java (dòng 953–959).
public sealed record ServerNotification(int Id, string Name, string Text)
{
    // Chuỗi truyền cho client theo định dạng Java. (Java: NOTIFY.add)
    public string WireText => Name + "<>" + Text;
}
