using Nro.Core.IO;
using Nro.Game.Players.Ports;

namespace Nro.Tests.Unit.Network.Packets.Players;

// IPlayerSessionTransport giả ghi packet và thứ tự gửi cho các adapter I-01.
// Nguồn Java: network/interfaces/ISession.java (sendMessage).
internal sealed class RecordingSession(string label, List<string> order) : IPlayerSessionTransport
{
    // Các gói được chụp trước khi Message bị adapter Dispose (Java: session.sendMessage).
    public List<(sbyte Command, byte[] Payload)> Packets { get; } = [];

    // Mã phiên test.
    public long Id => 1;

    // IP test.
    public string IpAddress => "127.0.0.1";

    // Endpoint test.
    public string RemoteEndPoint => IpAddress;

    // Cờ kết nối test.
    public bool IsConnected { get; private set; } = true;

    // Chụp payload và thứ tự gửi để đối chiếu vòng Java.
    public bool Send(Message message)
    {
        if (!IsConnected)
        {
            return false;
        }

        order.Add(label);
        Packets.Add((message.Command, message.GetData()));
        return true;
    }

    // Đóng giả lập session.
    public void Disconnect() => IsConnected = false;
}
