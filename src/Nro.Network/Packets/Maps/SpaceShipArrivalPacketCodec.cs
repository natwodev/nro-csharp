using Nro.Core.IO;

namespace Nro.Network.Packets.Maps;

// Mã hóa TELEPORT, chỉ chịu trách nhiệm thứ tự trường wire Java.
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 741–763); docs/PROTOCOL.md, dòng -65.
public static class SpaceShipArrivalPacketCodec
{
    // Lệnh TELEPORT có dấu trên wire (Java: new Message(-65)).
    public const sbyte Command = -65;

    // Tạo payload i characterId, b typeSpace đúng thứ tự Java.
    public static Message Encode(SpaceShipArrivalPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Message message = new(Command);
        message.Writer().WriteInt(packet.CharacterId);
        message.Writer().WriteByte(packet.SpaceShipType);
        return message;
    }

    // Đọc payload -65 theo thứ tự Java để golden test xác nhận byte-exact.
    public static SpaceShipArrivalPacket Decode(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Command != Command)
        {
            throw new InvalidDataException("TELEPORT không đúng command.");
        }

        return new SpaceShipArrivalPacket(message.ReadInt(), message.ReadByte());
    }
}
