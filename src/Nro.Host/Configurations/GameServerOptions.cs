// Nguồn Java: nro/server/ServerManager.java (dòng 132–218, 520–582).
namespace Nro.Host.Configurations;

// Chứa cấu hình không bí mật cho bộ lắng nghe TCP. (Java: ServerManager.PORT)
public sealed class GameServerOptions
{
    // Cổng TCP lắng nghe; 0 dành riêng cho kiểm thử cổng tạm. (Java: ServerManager.PORT)
    public int Port { get; set; } = 14445;

    // Địa chỉ IP cục bộ; để trống sẽ lắng nghe mọi giao diện. (Java: ServerSocket)
    public string? BindAddress { get; set; }

    // Số kết nối đồng thời tối đa theo một IP. (Java: Manager.MAX_PER_IP)
    public int MaxConnectionsPerIp { get; set; } = 3;

    // Kích thước buffer gửi và nhận của socket. (Java: Socket.setSendBufferSize)
    public int SocketBufferBytes { get; set; } = 0x100000;

    // Bật TCP NoDelay cho gói game nhỏ. (Java: Socket)
    public bool NoDelay { get; set; } = true;

    // Các giới hạn và thời hạn của một phiên. (Java: MySession)
    public SessionOptions Session { get; set; } = new();
}
