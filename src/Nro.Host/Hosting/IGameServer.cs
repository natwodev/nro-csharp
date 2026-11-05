// Nguồn Java: nro/server/ServerManager.java (dòng 520–582, 623–671).
namespace Nro.Host.Hosting;

// Cổng vòng đời listener để HostedService kiểm thử mà không mở socket thật. (Java: ServerManager)
public interface IGameServer
{
    // Mở listener sau khi tất cả catalog bắt buộc đã nạp xong. (Java: ServerManager.run)
    void Start();

    // Dừng nhận kết nối mới nhưng chưa ngắt các phiên cần được lưu. (Java: ServerManager.close)
    void StopAccepting();

    // Đóng các phiên còn lại và giải phóng listener. (Java: ServerManager.close)
    Task StopAsync(CancellationToken cancellationToken = default);
}
