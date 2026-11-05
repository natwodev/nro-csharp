using Nro.Core.IO;

namespace Nro.Network.Packets.Players;

// Mã hóa ME_LOAD_POINT, không sửa các độ rộng không khớp client nguồn.
// Nguồn Java: nro/services/Service.java (dòng 798–831 trong bảng I-02; bản SRC hiện có 425–459).
public static class PlayerPointPacketCodec
{
    // Lệnh tải chỉ số nhân vật. (Java: new Message(-42))
    public const sbyte Command = -42;

    // Ghi đúng thứ tự/ép kiểu DataOutput Java; D17: defg là s (2 byte), không phải i của client nguồn.
    public static Message Encode(PlayerPointPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Message message = new(Command);
        message.Writer().WriteInt(unchecked((int)packet.BaseHp));
        message.Writer().WriteInt(unchecked((int)packet.BaseMp));
        message.Writer().WriteInt(unchecked((int)packet.BaseDamage));
        message.Writer().WriteInt(unchecked((int)packet.HpMax));
        message.Writer().WriteInt(unchecked((int)packet.MpMax));
        message.Writer().WriteInt(unchecked((int)packet.Hp));
        message.Writer().WriteInt(unchecked((int)packet.Mp));
        message.Writer().WriteByte(packet.Speed);
        message.Writer().WriteByte(20);
        message.Writer().WriteByte(20);
        message.Writer().WriteByte(1);
        message.Writer().WriteInt(unchecked((int)packet.Damage));
        message.Writer().WriteInt(packet.Defense);
        message.Writer().WriteByte(unchecked((sbyte)packet.CriticalChance));
        message.Writer().WriteLong(packet.Potential);
        message.Writer().WriteShort(100);
        message.Writer().WriteShort(unchecked((short)packet.BaseDefense));
        message.Writer().WriteByte(unchecked((sbyte)packet.BaseCriticalChance));
        return message;
    }
}
