using Nro.Core.IO;
using Nro.Game.Players.Ports;
using Nro.Network.Sessions;

namespace Nro.Host.Adapters.Packets;

// Bọc ISession Network thành transport mà Game được phép nhìn thấy.
// Nguồn Java: network/session/MySession.java (dòng 1–263).
public sealed class NetworkPlayerSessionTransport : IPlayerSessionTransport
{
    // Session Network thật được bọc ở Host (Java: Session).
    private readonly ISession _session;

    // Gắn một session network đã được tạo vào player session.
    public NetworkPlayerSessionTransport(ISession session)
    {
        _session = session ?? throw new ArgumentNullException(nameof(session));
    }

    // True khi session network chưa đóng (Java: isConnected).
    public bool IsConnected => _session.IsConnected;

    // Không ném khi session đã đóng; ISession trả false trong trường hợp đó (Java: sendMessage).
    public bool Send(Message message) => IsConnected && _session.Send(message);
}
