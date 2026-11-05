using Nro.Core.IO;

namespace Nro.Game.Players.Ports;

// Transport tối thiểu của phiên Player; tách ISession Network khỏi lớp MySession của Game.
// Nguồn Java: network/interfaces/ISession.java và network/session/MySession.java (dòng 1–263).
public interface IPlayerSessionTransport
{
    // Cho biết transport còn nhận được gói gửi (Java: isConnected).
    bool IsConnected { get; }

    // Chụp và xếp gói gửi; false khi transport đã đóng (Java: sendMessage).
    bool Send(Message message);
}
