using Nro.Core.IO;

namespace Nro.Network.Packets.Maps;

// Codec vị trí -7 theo DataOutputStream Java; mọi số là big-endian.
// Nguồn Java: nro/services/MapService.java (dòng 424–436); docs/PROTOCOL.md, mục -7.
public static class PlayerMovePacketCodec
{
    // Lệnh di chuyển người chơi có dấu trên wire. (Java: new Message(-7))
    public const sbyte Command = -7;

    // Ghi i playerId, s x, s y; đúng thứ tự Java. (Java: writeInt/writeShort/writeShort)
    public static Message Encode(PlayerMovePacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Message message = new(Command);
        JavaDataOutput writer = message.Writer();
        writer.WriteInt(packet.PlayerId);
        writer.WriteShort(packet.X);
        writer.WriteShort(packet.Y);
        return message;
    }

    // Đọc payload -7 để golden test xác nhận byte-exact.
    public static PlayerMovePacket Decode(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Command != Command)
        {
            throw new InvalidDataException("Gói di chuyển người chơi không đúng command -7.");
        }

        return new PlayerMovePacket(message.ReadInt(), message.ReadShort(), message.ReadShort());
    }
}
