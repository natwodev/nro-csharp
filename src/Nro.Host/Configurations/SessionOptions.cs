// Nguồn Java: nro/server/ServerManager.java (dòng 520–582).
namespace Nro.Host.Configurations;

// Chứa giới hạn phiên được bind từ cấu hình Host. (Java: MySession)
public sealed class SessionOptions
{
    // Số gói nhận được phép chờ xử lý. (Java: QueueHandler.addMessage)
    public int InboundQueueLimit { get; set; } = 500;

    // Số gói được phép chờ ghi trước khi đóng phiên. (Java: QueueHandler)
    public int OutboundQueueLimit { get; set; } = 10_000;

    // Thời hạn bắt tay hello của kết nối mới. (Java: MySession)
    public TimeSpan HandshakeTimeout { get; set; } = TimeSpan.FromSeconds(30);

    // Thời hạn im lặng sau bắt tay; null giữ hành vi Java không giới hạn. (Java: MySession)
    public TimeSpan? IdleTimeout { get; set; }
}
