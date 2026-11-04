namespace Nro.Network.Sessions;

// Đặt các giới hạn và thời hạn cho một phiên kết nối.
// Nguồn Java: network/session/Session.java (các giới hạn hàng đợi và thời hạn là cấu hình C# bổ sung để bảo vệ tài nguyên; mặc định 500 giữ nguyên Java).
public sealed class SessionOptions
{
    // Giới hạn gói nhận chờ xử lý; vượt giới hạn bị bỏ im lặng và được đếm, mặc định 500 như Java (Java: QueueHandler.addMessage).
    public int InboundQueueLimit { get; init; } = 500;

    // Giới hạn gói chờ ghi; khác Java dùng hàng đợi vô hạn, phiên bị ngắt khi đầy để tránh tăng bộ nhớ vô hạn; mặc định 10000.
    public int OutboundQueueLimit { get; init; } = 10_000;

    // Thời gian chờ gói hello -27 của kết nối mới; khác Java không giới hạn để socket rỗi không giữ mãi các slot IP; mặc định 30 giây.
    public TimeSpan HandshakeTimeout { get; init; } = TimeSpan.FromSeconds(30);

    // Thời gian phiên đã bắt tay được im lặng trước khi đóng; null mặc định là không giới hạn như Java, chỉ bật khi client luôn gửi gói định kỳ.
    public TimeSpan? IdleTimeout { get; init; }

    // Các lệnh client dùng kích thước 3 byte; mặc định rỗng vì đặc tả và Java đọc 2 byte, vẫn cấu hình được cho câu hỏi 66 và -28.
    public IReadOnlySet<sbyte> ClientThreeByteCommands { get; init; } = new HashSet<sbyte>();
}
