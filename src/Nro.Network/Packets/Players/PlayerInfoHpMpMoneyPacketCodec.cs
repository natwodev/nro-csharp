using Nro.Core.IO;
using Nro.Core.Util;

namespace Nro.Network.Packets.Players;

// Mã hóa/giải mã ME_LOAD_INFO qua sub-command -30/4.
// Nguồn Java: nro/services/PlayerService.java (dòng 154–177); docs/PROTOCOL.md §3.2, -30/4 ME_LOAD_INFO.
public static class PlayerInfoHpMpMoneyPacketCodec
{
    // Lệnh envelope có sub-command (Java: messageSubCommand).
    public const sbyte Command = -30;

    // Sub-command thông tin tiền, HP và MP (Java: messageSubCommand((byte) 4)).
    public const sbyte SubCommand = 4;

    // Mốc phiên bản chọn gold long thay cho int (Java: session.version >= 214).
    public const int LongGoldVersion = 214;

    // Tạo payload b 4, gold (L/i), i gem, C hp, C mp, i ruby theo đúng Java.
    public static Message Encode(PlayerInfoHpMpMoneyPacket packet, int clientVersion, bool readInt)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Message message = new(Command);
        message.Writer().WriteByte(SubCommand);
        if (clientVersion >= LongGoldVersion)
        {
            message.Writer().WriteLong(packet.Gold);
        }
        else
        {
            message.Writer().WriteInt(unchecked((int)packet.Gold));
        }

        message.Writer().WriteInt(packet.Gem);
        message.WriteCris(Util.CrisGH(packet.Hp, readInt), readInt);
        message.WriteCris(Util.CrisGH(packet.Mp, readInt), readInt);
        message.Writer().WriteInt(packet.Ruby);
        return message;
    }

    // Đọc payload theo ngưỡng phiên bản và cờ Manager.readInt của server.
    public static PlayerInfoHpMpMoneyPacket Decode(Message message, int clientVersion, bool readInt)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Command != Command || message.ReadByte() != SubCommand)
        {
            throw new InvalidDataException("ME_LOAD_INFO không đúng sub-command.");
        }

        long gold = clientVersion >= LongGoldVersion ? message.ReadLong() : message.ReadInt();
        int gem = message.ReadInt();
        long hp = readInt ? message.ReadInt() : message.ReadLong();
        long mp = readInt ? message.ReadInt() : message.ReadLong();
        return new PlayerInfoHpMpMoneyPacket(gold, gem, hp, mp, message.ReadInt());
    }
}
