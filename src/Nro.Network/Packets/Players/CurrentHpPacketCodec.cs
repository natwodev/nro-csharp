using Nro.Core.IO;
using Nro.Core.Util;

namespace Nro.Network.Packets.Players;

// Mã hóa/giải mã ME_LOAD_HP qua sub-command -30/5.
// Nguồn Java: nro/services/PlayerService.java (dòng 114–124); docs/PROTOCOL.md §3.2, -30/5 ME_LOAD_HP.
public static class CurrentHpPacketCodec
{
    // Lệnh envelope có sub-command (Java: messageSubCommand).
    public const sbyte Command = -30;

    // Sub-command HP hiện tại (Java: messageSubCommand((byte) 5)).
    public const sbyte SubCommand = 5;

    // Tạo payload b 5 rồi C hp; C là i khi Manager.readInt, ngược lại là L (Java: writeCris(Util.CrisGH(...), Manager.readInt)).
    public static Message Encode(CurrentHpPacket packet, bool readInt)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Message message = new(Command);
        message.Writer().WriteByte(SubCommand);
        message.WriteCris(Util.CrisGH(packet.Hp, readInt), readInt);
        return message;
    }

    // Đọc payload theo độ rộng Manager.readInt đã được cấu hình cho server.
    public static CurrentHpPacket Decode(Message message, bool readInt)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Command != Command || message.ReadByte() != SubCommand)
        {
            throw new InvalidDataException("ME_LOAD_HP không đúng sub-command.");
        }

        return new CurrentHpPacket(readInt ? message.ReadInt() : message.ReadLong());
    }
}
