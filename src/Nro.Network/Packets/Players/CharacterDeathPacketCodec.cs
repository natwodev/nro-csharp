using Nro.Core.IO;

namespace Nro.Network.Packets.Players;

// Hai writer chết giữ schema độc lập vì id có độ rộng khác nhau.
// Nguồn Java: nro/services/Service.java (dòng 1213–1239 trong bảng I-02; bản SRC hiện có 761–789).
public static class CharacterDeathPacketCodec
{
    // Lệnh chết gửi cho chính chủ. (Java: new Message(-17))
    public const sbyte SelfCommand = -17;

    // Lệnh chết phát cho người khác. (Java: new Message(-8))
    public const sbyte OtherCommand = -8;

    // Ghi b id (low byte), s x, s y; không thêm cPk/power mà client nguồn suy đoán.
    public static Message EncodeSelf(CharacterDeathPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Message message = new(SelfCommand);
        message.Writer().WriteByte(unchecked((sbyte)packet.CharacterId));
        message.Writer().WriteShort(unchecked((short)packet.X));
        message.Writer().WriteShort(unchecked((short)packet.Y));
        return message;
    }

    // Ghi s id, b cPk=0, s x, s y; PROTOCOL D9/Zone.java:811 viết id i là writer khác, không chuẩn hóa.
    public static Message EncodeOther(CharacterDeathPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Message message = new(OtherCommand);
        message.Writer().WriteShort(unchecked((short)packet.CharacterId));
        message.Writer().WriteByte(0);
        message.Writer().WriteShort(unchecked((short)packet.X));
        message.Writer().WriteShort(unchecked((short)packet.Y));
        return message;
    }
}
