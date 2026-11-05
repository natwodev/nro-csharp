using Nro.Core.IO;
using Nro.Core.Templates;
using Nro.Game.Items.Core;
using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Network.Packets.Players;

namespace Nro.Host.Adapters.Packets;

// Cài đặt cổng gói Player, chỉ lo tạo gói và gửi qua Player session.
// Nguồn Java: nro/services/PlayerService.java (dòng 114–177); nro/services/Service.java (dòng 798–987, 1181–1290 trong bảng I-02).
public sealed class PlayerPacketAdapter : IPlayerVitalsNotifier, IPlayerCharacterPacketNotifier
{
    // Cờ Manager.readInt quyết định độ rộng C trong gói HP/MP Java.
    private readonly bool _readInt;

    // Broadcaster I-01 tái dùng cho nhánh -8; null chỉ khi adapter vitals cũ được dựng độc lập.
    private readonly BroadcastPacketAdapter? _broadcast;

    // Nhận đúng cờ Manager.readInt của server để giữ kích thước C HP/MP Java.
    public PlayerPacketAdapter(bool readInt)
    {
        _readInt = readInt;
    }

    // Ghép broadcaster đã có, không nhân bản vòng lặp zone của Service.sendMessAnotherNotMeInMap.
    public PlayerPacketAdapter(bool readInt, BroadcastPacketAdapter broadcast)
    {
        _readInt = readInt;
        _broadcast = broadcast ?? throw new ArgumentNullException(nameof(broadcast));
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
        message.Cleanup();
    }

    // Gửi -42 sau khi service nghiệp vụ đã calPoint/Send_Info_NV; D17 giữ BaseDefense là short trên wire Java.
    public void SendPlayerPoint(Player player)
    {
        if (player is null || player.IsPet || player.IsBoss)
        {
            return;
        }

        var point = player.NPoint;
        Message message = PlayerPointPacketCodec.Encode(new PlayerPointPacket(
            point.BaseHp, point.BaseMp, point.BaseDamage, point.HpMax, point.MpMax, point.Hp, point.Mp,
            point.Speed, point.Damage, point.Defense, point.CriticalChance, point.Potential,
            point.BaseDefense, point.BaseCriticalChance));
        player.SendMessage(message);
        message.Cleanup();
    }

    // Gửi -30/0 từ snapshot Player và catalog đầu/avatar được composition root nạp; không giữ catalog/singleton trong codec.
    public void SendCharacterLoad(Player player, IReadOnlyList<HeadAvatar> headAvatars)
    {
        if (player is null || headAvatars is null)
        {
            return;
        }

        int clientVersion = player.Session?.Version ?? 0;
        Message message = CharacterLoadPacketCodec.Encode(CreateLoadPacket(player, headAvatars), clientVersion);
        player.SendMessage(message);
        message.Cleanup();
    }

    // Gửi -17 cho chính chủ rồi -8 tới người khác; không ném khi session null/đóng vì Player.SendMessage/Broadcast I-01 đã bỏ qua.
    public void SendCharacterDeath(Player player)
    {
        if (player is null)
        {
            return;
        }

        var packet = new CharacterDeathPacket(player.Id, player.Location.X, player.Location.Y);
        if (!player.IsPet && player.IsPl())
        {
            Message self = CharacterDeathPacketCodec.EncodeSelf(packet);
            player.SendMessage(self);
            self.Cleanup();
        }

        if (_broadcast is not null)
        {
            _broadcast.SendToZoneExcept(player, CharacterDeathPacketCodec.EncodeOther(packet));
        }
    }

    private static CharacterLoadPacket CreateLoadPacket(Player player, IReadOnlyList<HeadAvatar> headAvatars)
    {
        IReadOnlyList<short> skills = (player.PlayerSkill.Skills ?? []).Where(skill => skill.SkillId != -1).Select(skill => skill.SkillId).ToArray();
        return new CharacterLoadPacket(
            player.Id, player.PlayerTask.TaskMain?.Id ?? 0, player.Gender, player.Head, player.Name, player.TypePk, player.NPoint.Power,
            skills, player.Inventory.Gold, player.Inventory.Ruby, player.Inventory.Gem,
            ToLoadItems(player.Inventory.ItemsBody), ToLoadItems(player.Inventory.ItemsBag), ToLoadItems(player.Inventory.ItemsBox),
            headAvatars.Select(value => new CharacterLoadHeadAvatar(value.HeadId, value.AvatarId)).ToArray(),
            player.Fusion.TypeFusion != 0, player.DeltaTime, player.IsNewMember,
            player.GetAura(), player.GetFrontEffect(), player.GetHat());
    }

    private static IReadOnlyList<CharacterLoadItem> ToLoadItems(IEnumerable<Item> items) => items.Select(item =>
    {
        if (item is null || !item.IsNotNullItem())
        {
            return new CharacterLoadItem(-1, 0, string.Empty, string.Empty, []);
        }

        return new CharacterLoadItem(item.Template!.Id, item.Quantity, item.GetInfo(), item.GetContent(),
            item.ItemOptions.Select(option => new CharacterLoadItemOption(option.OptionTemplate.Id, option.Param)).ToArray());
    }).ToArray();
}
