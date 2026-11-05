using Nro.Core.IO;
using Nro.Core.Util;

namespace Nro.Network.Packets.Players;

// Mã hóa/giải mã ME_LOAD_MP qua sub-command -30/6.
// Nguồn Java: nro/services/PlayerService.java (dòng 126–136); docs/PROTOCOL.md §3.2, -30/6 ME_LOAD_MP.
public static class CurrentMpPacketCodec
{
    // Lệnh envelope có sub-command (Java: messageSubCommand).
    public const sbyte Command = -30;

    // Sub-command MP hiện tại (Java: messageSubCommand((byte) 6)).
    public const sbyte SubCommand = 6;

    // Tạo payload b 6 rồi C mp; C là i khi Manager.readInt, ngược lại là L (Java: writeCris(Util.CrisGH(...), Manager.readInt)).
    public static Message Encode(CurrentMpPacket packet, bool readInt)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Message message = new(Command);
        message.Writer().WriteByte(SubCommand);
        message.WriteCris(Util.CrisGH(packet.Mp, readInt), readInt);
        return message;
    }

    // Đọc payload theo độ rộng Manager.readInt đã được cấu hình cho server.
    public static CurrentMpPacket Decode(Message message, bool readInt)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Command != Command || message.ReadByte() != SubCommand)
        {
            throw new InvalidDataException("ME_LOAD_MP không đúng sub-command.");
        }

        return new CurrentMpPacket(readInt ? message.ReadInt() : message.ReadLong());
    }
}
