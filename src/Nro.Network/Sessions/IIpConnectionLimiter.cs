namespace Nro.Network.Sessions;

// Hợp đồng giới hạn số kết nối đồng thời theo địa chỉ IP.
// Nguồn Java: network/ServerManager.java và network/session/Session.java (bộ đếm không âm khi trả chỗ trống).

public interface IIpConnectionLimiter
{

    // Giữ một chỗ kết nối cho IP; trả false nếu đã đạt giới hạn.
    bool TryAcquire(string ip);

    // Trả chỗ kết nối cho IP; không làm bộ đếm âm.
    void Release(string ip);

    // Trả số kết nối hiện tại của IP.
    int CountFor(string ip);
}
