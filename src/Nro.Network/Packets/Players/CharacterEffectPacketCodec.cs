using Nro.Core.IO;

namespace Nro.Network.Packets.Players;

// Mã hóa/giải mã CHAR_EFFECT, chỉ chịu trách nhiệm thứ tự trường wire.
// Nguồn Java: nro/services/Service.java (dòng 119–145); docs/PROTOCOL.md §3.2, cmd -128 CHAR_EFFECT.
public static class CharacterEffectPacketCodec
{
    // Lệnh CHAR_EFFECT có dấu trên wire (Java: new Message(-128)).
    public const sbyte Command = -128;

    // Sub-command thêm hiệu ứng (Java: writer().writeByte(0)).
    public const sbyte AddSubCommand = 0;

    // Tạo Message S→C: b sub, i charId, s effectId, b layer, b loop, s loopCount, b stand (Java: addEffectChar).
    public static Message Encode(CharacterEffectPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Message message = new(Command);
        message.Writer().WriteByte(AddSubCommand);
        message.Writer().WriteInt(packet.CharacterId);
        message.Writer().WriteShort(packet.EffectId);
        message.Writer().WriteByte(packet.Layer);
        message.Writer().WriteByte(packet.Loop);
        message.Writer().WriteShort(packet.LoopCount);
        message.Writer().WriteByte(packet.Stand);
        return message;
    }

    // Đọc payload sub 0 theo đúng thứ tự Java; sub khác ném để caller không nhầm kiểu gói.
    public static CharacterEffectPacket Decode(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Command != Command || message.ReadByte() != AddSubCommand)
        {
            throw new InvalidDataException("CHAR_EFFECT không phải sub-command thêm hiệu ứng.");
        }

        return new CharacterEffectPacket(message.ReadInt(), message.ReadShort(), message.ReadByte(), message.ReadByte(), message.ReadShort(), message.ReadByte());
    }
}
