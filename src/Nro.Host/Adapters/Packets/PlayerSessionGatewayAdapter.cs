using Nro.Core.IO;
using Nro.Game.Players.Ports;
using Nro.Game.Players.Sessions;

namespace Nro.Host.Adapters.Packets;

// Cài đặt cổng session Player qua IPlayerSessionTransport, không để null/đóng làm đứt broadcast.
// Nguồn Java: network/session/MySession.java và nro/player/Player.java (dòng 768–845).
public sealed class PlayerSessionGatewayAdapter : IPlayerSessionGateway
{
    // Gửi nếu transport hiện hữu và còn kết nối; Java bỏ qua lỗi send tại các vòng broadcast.
    public void Send(MySession session, Message message)
    {
        if (session?.Transport?.IsConnected == true)
        {
            _ = session.Transport.Send(message);
        }
    }

    // Đọc cờ active của session (Java: actived).
    public bool IsActive(MySession session) => session?.IsActive == true;

    // Đọc cờ founder của session (Java: isFounder).
    public bool IsFounder(MySession session) => session?.IsFounder == true;

    // Đọc cờ jail của session (Java: isJail).
    public bool IsJail(MySession session) => session?.IsJail == true;

    // Đọc cờ moderator của session (Java: isQuanTriVien).
    public bool IsModerator(MySession session) => session?.IsModerator == true;
}
