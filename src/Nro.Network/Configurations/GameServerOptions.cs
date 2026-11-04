using System.Net;
using Nro.Network.Sessions;

namespace Nro.Network.Configurations;

// Tập cấu hình bộ lắng nghe TCP của máy chủ game.
// Nguồn Java: network/ServerManager.java (giữ cổng, giới hạn IP và buffer; C# tắt Nagle có chủ đích để tránh độ trễ delayed-ACK cho gói nhỏ).

public sealed class GameServerOptions
{

    // Cổng TCP lắng nghe; 0 để hệ điều hành chọn cổng trống (Java: ServerManager.PORT).
    public int Port { get; init; } = 14445;

    // Địa chỉ cục bộ lắng nghe; null là mọi giao diện IPv4/IPv6 như wildcard Java.
    public IPAddress? BindAddress { get; init; }

    // Số kết nối đồng thời tối đa từ một IP (Java: Manager.MAX_PER_IP).
    public int MaxConnectionsPerIp { get; init; } = 3;

    // Kích thước buffer gửi và nhận của socket theo giá trị Java 0x100000.
    public int SocketBufferBytes { get; init; } = 0x100000;

    // Bật TCP NoDelay để gói nhỏ không chờ Nagle/delayed-ACK; khác Java có chủ đích.
    public bool NoDelay { get; init; } = true;

    // Giới hạn áp dụng cho từng phiên.
    public SessionOptions Session { get; init; } = new();
}
