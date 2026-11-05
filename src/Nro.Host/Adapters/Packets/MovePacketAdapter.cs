using Nro.Game.Effects.Ports;
using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Core.IO;
using Nro.Network.Packets.Maps;

namespace Nro.Host.Adapters.Packets;

// Adapter phát wire cho vị trí -7; tách riêng để không đụng WorldPacketAdapter của I-03.
// Nguồn Java: nro/services/MapService.java (dòng 424–436).
public sealed class MovePacketAdapter : IPlayerMoveGateway
{
    // Broadcaster phạm vi zone do host cài đặt. (Java: Service.sendMessAllPlayerInMap)
    private readonly IEffectBroadcaster _broadcaster;

    // Ghép broadcaster zone do Host cung cấp.
    public MovePacketAdapter(IEffectBroadcaster broadcaster)
    {
        _broadcaster = broadcaster ?? throw new ArgumentNullException(nameof(broadcaster));
    }

    // Narrow long id như Java `(int) player.id`, rồi broadcast -7 cho cả zone.
    public void SendPlayerMove(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        Message message = PlayerMovePacketCodec.Encode(new PlayerMovePacket(unchecked((int)player.Id), unchecked((short)player.Location.X), unchecked((short)player.Location.Y)));
        _broadcaster.SendToZoneOf(player, message);
    }
}
