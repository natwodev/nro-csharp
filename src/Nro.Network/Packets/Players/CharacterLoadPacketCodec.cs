using Nro.Core.IO;

namespace Nro.Network.Packets.Players;

// Mã hóa riêng writer -30/0 ME_LOAD_ALL; không dùng lại writer -30/7 vì schema khác (PROTOCOL D/E2).
// Nguồn Java: nro/services/Service.java (dòng 860–958 trong bảng I-02; bản SRC hiện có 479–583).
public static class CharacterLoadPacketCodec
{
    // Envelope sub-command. (Java: messageSubCommand)
    public const sbyte Command = -30;

    // Snapshot đăng nhập. (Java: messageSubCommand((byte) 0))
    public const sbyte SubCommand = 0;

    // Mốc phiên bản đổi gold i thành L. (Java: session.version >= 214)
    public const int LongGoldVersion = 214;

    // Ghi full ME_LOAD_ALL đúng các cast writeByte/writeShort Java, gồm UTF modified UTF-8 của MessageWriter.
    public static Message Encode(CharacterLoadPacket packet, int clientVersion)
    {
        ArgumentNullException.ThrowIfNull(packet);
        Message message = new(Command);
        message.Writer().WriteByte(SubCommand);
        message.Writer().WriteInt(unchecked((int)packet.CharacterId));
        message.Writer().WriteByte(unchecked((sbyte)packet.TaskMainId));
        message.Writer().WriteByte(packet.Gender);
        message.Writer().WriteShort(packet.Head);
        message.Writer().WriteUTF(packet.Name ?? string.Empty);
        message.Writer().WriteByte(0);
        message.Writer().WriteByte(packet.TypePk);
        message.Writer().WriteLong(packet.Power);
        message.Writer().WriteShort(0);
        message.Writer().WriteShort(0);
        message.Writer().WriteByte(packet.Gender);
        message.Writer().WriteByte(unchecked((sbyte)packet.SkillIds.Count));
        foreach (short skillId in packet.SkillIds) message.Writer().WriteShort(skillId);
        if (clientVersion >= LongGoldVersion) message.Writer().WriteLong(packet.Gold);
        else message.Writer().WriteInt(unchecked((int)packet.Gold));
        message.Writer().WriteInt(packet.Ruby);
        message.Writer().WriteInt(packet.Gem);
        WriteItems(message, packet.BodyItems);
        WriteItems(message, packet.BagItems);
        WriteItems(message, packet.BoxItems);
        message.Writer().WriteShort(unchecked((short)packet.HeadAvatars.Count));
        foreach (CharacterLoadHeadAvatar avatar in packet.HeadAvatars)
        {
            message.Writer().WriteShort(unchecked((short)avatar.HeadId));
            message.Writer().WriteShort(unchecked((short)avatar.AvatarId));
        }
        message.Writer().WriteShort(514);
        message.Writer().WriteShort(515);
        message.Writer().WriteShort(537);
        message.Writer().WriteByte(packet.IsFusion ? 1 : 0);
        message.Writer().WriteInt(packet.DeltaTime);
        message.Writer().WriteByte(packet.IsNewMember ? 1 : 0);
        message.Writer().WriteShort(packet.Aura);
        message.Writer().WriteByte(packet.FrontEffect);
        message.Writer().WriteShort(unchecked((short)packet.Hat));
        return message;
    }

    private static void WriteItems(Message message, IReadOnlyList<CharacterLoadItem> items)
    {
        message.Writer().WriteByte(unchecked((sbyte)items.Count));
        foreach (CharacterLoadItem item in items)
        {
            message.Writer().WriteShort(item.TemplateId);
            if (item.TemplateId == -1) continue;
            message.Writer().WriteInt(item.Quantity);
            message.Writer().WriteUTF(item.Info ?? string.Empty);
            message.Writer().WriteUTF(item.Content ?? string.Empty);
            message.Writer().WriteByte(unchecked((sbyte)item.Options.Count));
            foreach (CharacterLoadItemOption option in item.Options)
            {
                message.Writer().WriteByte(unchecked((sbyte)option.OptionId));
                message.Writer().WriteShort(unchecked((short)option.Parameter));
            }
        }
    }
}
