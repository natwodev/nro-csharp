using Nro.Core.IO;
using Nro.Game.Effects.Ports;
using Nro.Game.Players;
using Nro.Game.Services.Maps.Ports;
using Nro.Network.Packets.Maps;

namespace Nro.Host.Adapters.Packets;

// Adapter wire cho gói đổi map; không giữ state map và không thay MapService/ChangeMapService.
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 64–100, 741–763).
public sealed class WorldPacketAdapter : IChangeMapPacketGateway
{
    // Cổng broadcast zone để thực hiện ba phạm vi nhận của -65 (Java: Service.sendMessAll/AnotherNotMe).
    private readonly IEffectBroadcaster _broadcaster;

    // BroadcastPacketAdapter là cổng phát đúng phạm vi zone Java; DI ghép adapter này vào đây.
    public WorldPacketAdapter(IEffectBroadcaster broadcaster)
    {
        _broadcaster = broadcaster ?? throw new ArgumentNullException(nameof(broadcaster));
    }

    // Gửi -91 cho chính người chơi; session null tự bị Player.SendMessage bỏ qua (Java: openChangeMapTab).
    public void SendMapTransport(Player player, MapTransportPacket packet)
    {
        if (player is null)
        {
            return;
        }

        Message message = MapTransportPacketCodec.Encode(packet);
        player.SendMessage(message);
        message.Cleanup();
    }

    // Phát -65 với ba phạm vi typeSendMSG Java, giữ command/payload ở codec Network.
    public void SendSpaceShipArrival(Player player, SpaceShipArrivalPacket packet, SpaceShipArrivalAudience audience)
    {
        if (player is null)
        {
            return;
        }

        Message message = SpaceShipArrivalPacketCodec.Encode(packet);
        switch (audience)
        {
            case SpaceShipArrivalAudience.AllInMap:
                _broadcaster.SendToZoneOf(player, message);
                break;
            case SpaceShipArrivalAudience.Self:
                player.SendMessage(message);
                message.Cleanup();
                break;
            case SpaceShipArrivalAudience.OthersInMap:
                _broadcaster.SendToZoneExcept(player, message);
                break;
            default:
                message.Cleanup();
                break;
        }
    }
}
