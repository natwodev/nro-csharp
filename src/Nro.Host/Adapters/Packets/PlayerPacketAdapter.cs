using Nro.Core.IO;
using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Network.Packets.Players;

namespace Nro.Host.Adapters.Packets;

// Cài đặt cổng vitals PlayerService, chỉ lo tạo gói và gửi qua Player session.
// Nguồn Java: nro/services/PlayerService.java (dòng 114–177).
public sealed class PlayerPacketAdapter : IPlayerVitalsNotifier
{
    // Cờ Manager.readInt quyết định độ rộng C trong gói HP/MP Java.
    private readonly bool _readInt;

    // Nhận đúng cờ Manager.readInt của server để giữ kích thước C HP/MP Java.
    public PlayerPacketAdapter(bool readInt)
    {
        _readInt = readInt;
    }

    // Gửi -30/5, không ném khi player/session đã đóng (Java: sendInfoHp).
    public void SendCurrentHp(Player player, long hp)
    {
        if (player is null)
        {
            return;
        }

        Message message = CurrentHpPacketCodec.Encode(new CurrentHpPacket(hp), _readInt);
        player.SendMessage(message);
        message.Cleanup();
    }

    // Gửi -30/6, không ném khi player/session đã đóng (Java: sendInfoMp).
    public void SendCurrentMp(Player player, long mp)
    {
        if (player is null)
        {
            return;
        }

        Message message = CurrentMpPacketCodec.Encode(new CurrentMpPacket(mp), _readInt);
        player.SendMessage(message);
        message.Cleanup();
    }

    // Gửi -30/4; player không phải người chơi thật bị Java return trước khi tạo Message (Java: sendInfoHpMpMoney).
    public void SendInfoHpMpMoney(Player player)
    {
        if (player is null || !player.IsPl())
        {
            return;
        }

        int clientVersion = player.Session?.Version ?? 0;
        var packet = new PlayerInfoHpMpMoneyPacket(player.Inventory.Gold, player.Inventory.Gem, player.NPoint.Hp, player.NPoint.Mp, player.Inventory.Ruby);
        Message message = PlayerInfoHpMpMoneyPacketCodec.Encode(packet, clientVersion, _readInt);
        player.SendMessage(message);
    }
}
