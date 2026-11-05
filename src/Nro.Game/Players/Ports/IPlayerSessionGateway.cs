using Nro.Core.IO;
using Nro.Game.Players.Sessions;

namespace Nro.Game.Players.Ports;

// Cổng phiên đăng nhập cho Player, tách gói tin và cờ phiên khỏi shell người chơi.
// Nguồn Java: network/session/MySession.java và nro/player/Player.java (dòng 768–845).
public interface IPlayerSessionGateway
{
    // Gửi gói tin qua phiên đang gắn với người chơi (Java: session.sendMessage).
    void Send(MySession session, Message message);

    // Kiểm tra phiên đã kích hoạt (Java: session.actived).
    bool IsActive(MySession session);

    // Kiểm tra phiên của người sáng lập (Java: session.isFounder).
    bool IsFounder(MySession session);

    // Kiểm tra phiên đang ở nhà giam (Java: session.isJail).
    bool IsJail(MySession session);

    // Kiểm tra phiên của điều hành viên (Java: session.isQuanTriVien).
    bool IsModerator(MySession session);
}
