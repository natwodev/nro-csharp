namespace Nro.Network.Sessions;

// Hợp đồng tra cứu các phiên đang kết nối, an toàn luồng.
// Nguồn Java: network/session/SessionManager.java (TryGet trả kết quả tồn tại thay vì Java ném khi id lạ và giả định id có thứ tự; Snapshot trả bản sao để duyệt an toàn).

public interface ISessionManager
{

    // Số phiên đang kết nối (Java: getNumSession).
    int Count { get; }

    // Tra phiên theo id; trả false thay vì ném khi id không tồn tại (Java: findByID).
    bool TryGet(long id, out ISession? session);

    // Trả bản sao phiên để duyệt an toàn khi danh sách thay đổi (Java: getSessions).
    IReadOnlyList<ISession> Snapshot();
}
