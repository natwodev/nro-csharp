// Nguồn Java: nro/server/ServerManager.java (dòng 132–218, 623–671).
namespace Nro.Host.Configurations;

// Chứa các lựa chọn vòng đời khởi động và dừng Host. (Java: ServerManager)
public sealed class StartupOptions
{
    // Bắt buộc nạp catalog tĩnh trước khi mở cổng; chỉ tắt cho thử nghiệm không DB. (Java: Manager.loadDatabase)
    public bool LoadStaticCatalogs { get; set; } = true;

    // Hạn chót lưu người chơi sau khi đã ngừng nhận kết nối mới. (Java: ServerManager.close)
    public TimeSpan ShutdownSaveTimeout { get; set; } = TimeSpan.FromSeconds(30);
}
