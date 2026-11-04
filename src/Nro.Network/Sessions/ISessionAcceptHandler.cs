namespace Nro.Network.Sessions;

// Hợp đồng thông báo vòng đời chấp nhận và ngắt của phiên.
// Nguồn Java: network/interfaces/ISessionAcceptHandler.java (khởi tạo xảy ra trước gói đầu tiên; ngắt được gọi đúng một lần cho mỗi phiên).

public interface ISessionAcceptHandler
{

    // Thông báo kết nối mới trước khi đọc gói đầu tiên (Java: sessionInit).
    void SessionInit(ISession session);

    // Thông báo phiên kết thúc đúng một lần (Java: sessionDisconnect).
    void SessionDisconnect(ISession session);
}
