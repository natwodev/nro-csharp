using Nro.Game.Players.Ports;

namespace Nro.Game.Players.Sessions;

// Phiên đăng nhập gắn với người chơi; transport ở sau cổng Game để không tạo phụ thuộc Nro.Game -> Nro.Network.
// Nguồn Java: network/session/MySession.java (dòng 1–263).
public class MySession
{
    // Transport phiên thật; null khi người chơi chưa đăng nhập hoặc đã bị tháo phiên (Java: Session).
    public IPlayerSessionTransport? Transport { get; set; }

    // Phiên bản client để chọn các nhánh định dạng gói (Java: version).
    public int Version { get; set; }

    // Cờ đã kích hoạt phiên (Java: actived).
    public bool IsActive { get; set; }

    // Cờ tài khoản nhà sáng lập (Java: isFounder).
    public bool IsFounder { get; set; }

    // Cờ tài khoản trong nhà giam (Java: isJail).
    public bool IsJail { get; set; }

    // Cờ tài khoản điều hành viên (Java: isQuanTriVien).
    public bool IsModerator { get; set; }
}
