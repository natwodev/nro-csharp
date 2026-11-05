using Nro.Game.Players;
using Nro.Game.Players.Ports;

namespace Nro.Host.Adapters.Packets;

// Bổ sung hai nhánh PlayerService đã port vào base runtime hiện hữu; các nhánh runtime khác không thuộc I-01.
// Nguồn Java: nro/services/PlayerService.java (dòng 114–136).
public sealed class PlayerRuntimePacketAdapter : PlayerRuntimeServices
{
    // Codec/adapter vitals dùng chung để hai callback runtime ghi cùng wire format.
    private readonly PlayerPacketAdapter _packets;

    // Dùng chung cờ Manager.readInt với codec HP/MP Java.
    public PlayerRuntimePacketAdapter(bool readInt)
    {
        _packets = new PlayerPacketAdapter(readInt);
    }

    // Gửi HP hiện tại qua codec -30/5 (Java: sendInfoHp).
    public override void SendInfoHp(Player player) => _packets.SendCurrentHp(player, player.NPoint.Hp);

    // Gửi MP hiện tại qua codec -30/6 (Java: sendInfoMp).
    public override void SendInfoMp(Player player) => _packets.SendCurrentMp(player, player.NPoint.Mp);
}
