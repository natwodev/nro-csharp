using Nro.Core.IO;

namespace Nro.Network.Sessions;

// Giao diện một kết nối client theo góc nhìn của tầng game.
// Nguồn Java: network/interfaces/ISession.java (C# đưa sender, collector và hàng đợi vào Sessions.Session thay vì các thread riêng; gửi chụp payload tại lúc gọi để message có thể dọn ngay).

public interface ISession
{

    // Mã phiên do bộ quản lý gán (Java: getID).
    long Id { get; }

    // Địa chỉ client không gồm cổng (Java: getIP).
    string IpAddress { get; }

    // Địa chỉ IP:cổng xa của client; phiên cũ hoặc mock chỉ có IP sẽ trả IpAddress.
    string RemoteEndPoint => IpAddress;

    // Cho biết phiên còn kết nối (Java: isConnected).
    bool IsConnected { get; }

    // Đưa message vào hàng gửi; payload được chụp ngay và trả false khi phiên đã đóng (Java: sendMessage).
    bool Send(Message message);

    // Đóng phiên; gọi nhiều lần hoặc từ bất kỳ luồng nào đều an toàn (Java: disconnect).
    void Disconnect();
}
