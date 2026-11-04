using System.Runtime.CompilerServices;
using Nro.Core.Consts;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Util;

namespace Nro.Game.Effects.Services;

// Quản lý trạng thái hiệu ứng kỹ năng có thời hạn và gửi gói cho client, thay singleton gI() Java bằng lớp do EffectEnvironment.Create tạo; phụ thuộc chưa viết được truy cập qua các cổng Effects/Ports.
// Nguồn Java: nro/effect/EffectSkillService.java (KHÁC BIỆT CÓ CHỦ ĐÍCH: không dùng Thread.sleep theo D1/L04; sáu biến hình có SetX áp dụng ngay và SetXAsync chờ qua scheduler rồi gọi SetX, lấy mốc sau khi chờ như Java; hủy chờ ném lỗi và không đổi trạng thái, continuation Async phải được host đưa về tick zone để giữ D1; lỗi gói và kỹ năng thiếu được log như Java, các lỗi Java để thoát vẫn để thoát; bỏ kiểm tra null các thành phần Player mà C# không bao giờ gán null).
public sealed class EffectSkillService : IEffectSkillService
{
    public const sbyte TURN_ON_EFFECT = 1;
    public const sbyte TURN_OFF_EFFECT = 0;
    public const sbyte TURN_OFF_ALL_EFFECT = 2;

    public const sbyte HOLD_EFFECT = 32;
    public const int SHIELD_EFFECT = 33;
    public const int DANH_HIEU = 215;
    public const sbyte HUYT_SAO_EFFECT = 39;
    public const sbyte BLIND_EFFECT = 40;
    public const sbyte SLEEP_EFFECT = 41;
    public const sbyte STONE_EFFECT = 42;
    public const sbyte ICE_EFFECT = 42;

    public const sbyte DOKINH_EFFECT = 4;
    public const sbyte DOKINHCAOCAP_EFFECT = 15;

    public const sbyte SUC_MANH_BOC_PHA_EFFECT = 58;

    // Id template kỹ năng từ nro/skill/Skill.java; lớp Skill do track khác sở hữu.
    private const int SkillTaiTaoNangLuong = 8;
    private const int SkillBienKhi = 13;
    private const int SkillMaPhongBa = 26;

    // Id template vật phẩm biểu tượng thời gian, giữ nguyên literal Java.
    private const int ItemTimeBinhNormal = 11175;
    private const int ItemTimeBinhItem = 11166;

    private readonly VietnamClock clock;
    private readonly RandomUtil random;
    private readonly GameLogger logger;
    private readonly EffectSettings settings;
    private readonly IEffectScheduler scheduler;
    private readonly IEffectBroadcaster broadcaster;
    private readonly IEffectClientNotifier notifier;
    private readonly IEffectPlayerAccess players;
    private readonly IEffectRules rules;

    public EffectSkillService(
        VietnamClock clock,
        RandomUtil random,
        GameLogger logger,
        EffectSettings settings,
        IEffectScheduler scheduler,
        IEffectBroadcaster broadcaster,
        IEffectClientNotifier notifier,
        IEffectPlayerAccess players,
        IEffectRules rules)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(broadcaster);
        ArgumentNullException.ThrowIfNull(notifier);
        ArgumentNullException.ThrowIfNull(players);
        ArgumentNullException.ThrowIfNull(rules);
        this.clock = clock;
        this.random = random;
        this.logger = logger;
        this.settings = settings;
        this.scheduler = scheduler;
        this.broadcaster = broadcaster;
        this.notifier = notifier;
        this.players = players;
        this.rules = rules;
    }

    // ---- helpers ---------------------------------------------------------------------------------------------------------------------------------------------------

    // Theo Java: System.currentTimeMillis().
    private long Now() => clock.CurrentTimeMillis();

    // Theo Java: (int) player.id.
    private static int IdOf(Player player) => unchecked((int)player.Id);

    // Tương ứng Util.CrisGH(double), lấy Manager.readInt từ cấu hình (Java: Util.CrisGH).
    private long CrisGH(double a) => Util.CrisGH(a, settings.ReadInt);

    private void LogError(Exception e, [CallerMemberName] string method = "") => logger.LogException(typeof(EffectSkillService), e, null, method);

    private static EffectSkillRef Require(EffectSkillRef? skill, string what)
        => skill ?? throw new InvalidOperationException("The player has no " + what + " (Java: NullPointerException).");

    // Gói hiệu ứng kỹ năng -45: byte hành động, id người chơi, rồi short của kỹ năng; đọc trong try như Java.
    private void SendPacket45(Player player, sbyte action, Func<int> shortValue, bool exceptSelf = false)
    {
        try
        {
            var msg = new Message(-45);
            msg.Writer().WriteByte(action);
            msg.Writer().WriteInt(IdOf(player));
            msg.Writer().WriteShort(shortValue());
            if (exceptSelf)
            {
                broadcaster.SendToZoneExcept(player, msg);
            }
            else
            {
                broadcaster.SendToZoneOf(player, msg);
            }

            msg.Cleanup();
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    // Phần kết thúc chung khi Cumber, Cumber2, Pain, Kefla và Đuôi Khỉ hạ xuống, giống hệt Java.
    private void EndTransformationCommon(Player player)
    {
        EffectNPointState state = players.GetNPoint(player);
        if (state.Hp > state.HpMax)
        {
            players.SetHp(player, CrisGH(state.HpMax));
        }

        SendEffectEndCharge(player);
        SendTransformationOrItemEffect(player);
        notifier.SetNotMonkey(player);
        notifier.SendCostume(player);
        notifier.Point(player);
        notifier.SendInfoHpMp(player);
        notifier.SendInfoNv(player);
        notifier.SendInfoPlayerEatPea(player);
    }

    // ---- packets ---------------------------------------------------------------------------------------------------------------------------------------------------

    // Cài đặt SendEffectUseSkill theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectUseSkill(Player player, sbyte skillId)
    {
        EffectSkillRef? skill = players.GetSkillById(player, skillId);
        SendPacket45(player, 8, () => Require(skill, "skill " + skillId).SkillId, exceptSelf: true);
    }

    // Cài đặt SendSelfEffect35 theo hợp đồng dịch vụ hiệu ứng (Java: effcha).
    public void SendSelfEffect35(Player player)
    {
        try
        {
            var msg = new Message(-124);
            msg.Writer().WriteByte(1);
            msg.Writer().WriteByte(0);
            msg.Writer().WriteByte(35);
            msg.Writer().WriteInt(IdOf(player));
            msg.Writer().WriteInt(IdOf(player));
            broadcaster.SendToZoneOf(player, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    // Cài đặt SendEffectPlayer theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectPlayer(Player plUseSkill, Player plTarget, sbyte toggle, int effect)
    {
        try
        {
            var msg = new Message(-124);
            msg.Writer().WriteByte(toggle);
            msg.Writer().WriteByte(0);
            if (toggle == TURN_OFF_ALL_EFFECT)
            {
                msg.Writer().WriteInt(IdOf(plTarget));
            }
            else
            {
                msg.Writer().WriteByte(effect);
                msg.Writer().WriteInt(IdOf(plTarget));
                msg.Writer().WriteInt(IdOf(plUseSkill));
            }

            broadcaster.SendToZoneOf(plUseSkill, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    // Cài đặt SendSelfEffect theo hợp đồng dịch vụ hiệu ứng (Java: sendEffPlayer).
    public void SendSelfEffect(Player player, int effect)
    {
        try
        {
            var msg = new Message(-124);
            msg.Writer().WriteByte(1);
            msg.Writer().WriteByte(0);
            msg.Writer().WriteByte(effect);
            msg.Writer().WriteInt(IdOf(player));
            msg.Writer().WriteInt(IdOf(player));
            broadcaster.SendToZoneOf(player, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    // Cài đặt SendEffectMob theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectMob(Player plUseSkill, IEffectMobTarget mobTarget, sbyte toggle, sbyte effect)
    {
        try
        {
            var msg = new Message(-124);
            msg.Writer().WriteByte(toggle);
            msg.Writer().WriteByte(1);
            msg.Writer().WriteByte(effect);
            msg.Writer().WriteByte(mobTarget.Id);
            msg.Writer().WriteInt(IdOf(plUseSkill));
            broadcaster.SendToZoneOfMob(mobTarget, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    // Cài đặt SendSolarFlareBlindEffect theo hợp đồng dịch vụ hiệu ứng (Java: sendEffectBlindThaiDuongHaSan).
    public void SendSolarFlareBlindEffect(Player plUseSkill, IReadOnlyList<Player> targetPlayers, IReadOnlyList<IEffectMobTarget> mobs, int timeStun)
    {
        try
        {
            var msg = new Message(-45);
            msg.Writer().WriteByte(0);
            msg.Writer().WriteInt(IdOf(plUseSkill));
            msg.Writer().WriteShort(Require(players.GetSelectedSkill(plUseSkill), "selected skill").SkillId);
            msg.Writer().WriteByte(mobs.Count);
            foreach (IEffectMobTarget mob in mobs)
            {
                msg.Writer().WriteByte(mob.Id);
                msg.Writer().WriteByte(timeStun / 1000);
            }

            msg.Writer().WriteByte(targetPlayers.Count);
            foreach (Player pl in targetPlayers)
            {
                msg.Writer().WriteInt(IdOf(pl));
                msg.Writer().WriteByte(timeStun / 1000);
            }

            broadcaster.SendToZoneOf(plUseSkill, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    // Cài đặt SendEffectStartCharge theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectStartCharge(Player player)
    {
        EffectSkillRef? skill = players.GetSkillById(player, SkillTaiTaoNangLuong);
        SendPacket45(player, 6, () => Require(skill, "Tai Tao Nang Luong skill").SkillId);
    }

    // Cài đặt SendEffectCharge theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectCharge(Player player)
    {
        EffectSkillRef? skill = players.GetSkillById(player, SkillTaiTaoNangLuong);
        SendPacket45(player, 1, () => Require(skill, "Tai Tao Nang Luong skill").SkillId);
    }

    // Cài đặt SendEffectStopCharge theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectStopCharge(Player player) => SendPacket45(player, 3, () => -1);

    // Cài đặt SendEffectEndCharge theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectEndCharge(Player player)
        => SendPacket45(player, 5, () => Require(players.GetSelectedSkill(player), "selected skill").SkillId);

    // Cài đặt SendEffectMonkey theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectMonkey(Player player)
    {
        EffectSkillRef? skill = players.GetSkillById(player, SkillBienKhi);
        SendPacket45(player, 6, () => Require(skill, "Bien Khi skill").SkillId);
    }

    // Cài đặt SendTransformationOrItemEffect theo hợp đồng dịch vụ hiệu ứng (Java: SendEffectBienHinhorUseItem).
    public void SendTransformationOrItemEffect(Player player) => SendPacket45(player, 6, () => 95);

    // Cài đặt SendTransformationEffect theo hợp đồng dịch vụ hiệu ứng (Java: sendEffectTranformation).
    public void SendTransformationEffect(Player player) => SendPacket45(player, 6, () => 95);

    // Cài đặt SendEvolutionEffect theo hợp đồng dịch vụ hiệu ứng (Java: sendEffectVolution).
    public void SendEvolutionEffect(Player player) => SendPacket45(player, 6, () => 95);

    // Cài đặt TransformMobToChocolate theo hợp đồng dịch vụ hiệu ứng (Java: sendMobToSocola).
    public void TransformMobToChocolate(Player player, IEffectMobTarget mob, int timeSocola)
    {
        try
        {
            var msg = new Message(-112);
            msg.Writer().WriteByte(1);
            msg.Writer().WriteByte(mob.Id);
            msg.Writer().WriteShort(4133);
            broadcaster.SendToZoneOf(player, msg);
            msg.Cleanup();
            mob.SetChocolate(Now(), timeSocola);
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    // ---- simple timed flags ------------------------------------------------------------------------------------------------------------------------------------------

    // Cài đặt SetPumpkin theo hợp đồng dịch vụ hiệu ứng (Java: setBiNgo).
    public void SetPumpkin(Player player, int time)
    {
        player.EffectSkill.IsPumpkin = true;
        player.EffectSkill.TimePumpkin = time;
        player.EffectSkill.LastTimePumpkin = Now();
        notifier.SendCostume(player);
    }

    // Cài đặt RemovePumpkin theo hợp đồng dịch vụ hiệu ứng (Java: removeBiNgo).
    public void RemovePumpkin(Player player)
    {
        player.EffectSkill.IsPumpkin = false;
        notifier.SendCostume(player);
    }

    // Cài đặt SetSnowflake theo hợp đồng dịch vụ hiệu ứng (Java: setBongTuyet).
    public void SetSnowflake(Player player, int time)
    {
        player.EffectSkill.IsSnowflake = true;
        player.EffectSkill.TimeSnowflake = time;
        player.EffectSkill.LastTimeSnowflake = Now();
    }

    // Cài đặt RemoveSnowflake theo hợp đồng dịch vụ hiệu ứng (Java: removeBongTuyet).
    public void RemoveSnowflake(Player player) => player.EffectSkill.IsSnowflake = false;

    // Cài đặt SetVirus theo hợp đồng dịch vụ hiệu ứng.
    public void SetVirus(Player player, int time)
    {
        player.EffectSkill.IsVirus = true;
        player.EffectSkill.TimeVirus = time;
        player.EffectSkill.LastTimeVirus = Now();
    }

    // Cài đặt RemoveVirus theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveVirus(Player player) => player.EffectSkill.IsVirus = false;

    // Cài đặt SetMabuHold theo hợp đồng dịch vụ hiệu ứng.
    public void SetMabuHold(Player player, MaBuHold mabuHold)
    {
        (short x, short y) = players.GetMabuHoldPosition(mabuHold);
        player.MaBuHold = mabuHold;
        player.PrecentMabuHold = 0;
        notifier.ChangeAndSendTypePk(player, ConstPlayer.PK_ALL);
        notifier.SendMabuHold(player, 1, x, y);
        player.EffectSkill.IsMabuHold = true;
    }

    // Cài đặt RemoveMabuHold theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveMabuHold(Player player)
    {
        player.MaBuHold = null;
        player.PrecentMabuHold = 0;
        notifier.ChangeAndSendTypePk(player, ConstPlayer.NON_PK);
        notifier.SendMabuHold(player, 0, unchecked((short)players.GetLocation(player).X), 312);
        player.EffectSkill.IsMabuHold = false;
    }

    // Cài đặt SetPvpSuperDivineWater theo hợp đồng dịch vụ hiệu ứng (Java: setPKSTT).
    public void SetPvpSuperDivineWater(Player player, int time)
    {
        player.EffectSkill.IsPvpSuperDivineWater = true;
        player.EffectSkill.TimePvpSuperDivineWater = time;
        player.EffectSkill.LastTimePvpSuperDivineWater = Now();
        notifier.SendItemTime(player, 2295, time / 1000);
    }

    // Cài đặt RemovePvpSuperDivineWater theo hợp đồng dịch vụ hiệu ứng (Java: removePKSTT).
    public void RemovePvpSuperDivineWater(Player player)
    {
        player.EffectSkill.IsPvpSuperDivineWater = false;
        notifier.SendItemTime(player, 2295, 0);
    }

    // Cài đặt SetPvpCommeson theo hợp đồng dịch vụ hiệu ứng (Java: setPKCommeson).
    public void SetPvpCommeson(Player player, int time)
    {
        player.EffectSkill.IsPvpCommeson = true;
        player.EffectSkill.TimePvpCommeson = time;
        player.EffectSkill.LastTimePvpCommeson = Now();
        notifier.SendItemTime(player, 2295, time / 1000);
    }

    // Cài đặt RemovePvpCommeson theo hợp đồng dịch vụ hiệu ứng (Java: removePKCommeson).
    public void RemovePvpCommeson(Player player)
    {
        player.EffectSkill.IsPvpCommeson = false;
        notifier.SendItemTime(player, 2295, 0);
    }

    // Cài đặt SetDamageBuff theo hợp đồng dịch vụ hiệu ứng (Java: setDameBuff).
    public void SetDamageBuff(Player player, int time, int percent)
    {
        player.EffectSkill.IsDamageBuff = true;
        player.EffectSkill.TimeDamageBuff = time;
        player.EffectSkill.DamageBuffPercent = percent;
        player.EffectSkill.LastTimeDamageBuff = Now();
    }

    // Cài đặt RemoveDamageBuff theo hợp đồng dịch vụ hiệu ứng (Java: removeDameBuff).
    public void RemoveDamageBuff(Player player) => player.EffectSkill.IsDamageBuff = false;

    // Cài đặt SetXinbato theo hợp đồng dịch vụ hiệu ứng.
    public void SetXinbato(Player player, int time)
    {
        player.EffectSkill.IsXinbato = true;
        player.EffectSkill.TimeXinbato = time;
        player.EffectSkill.LastTimeXinbato = Now();
    }

    // Cài đặt RemoveXinbato theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveXinbato(Player player) => player.EffectSkill.IsXinbato = false;

    // Cài đặt SetFideLeader theo hợp đồng dịch vụ hiệu ứng (Java: setFideDaiCa).
    public void SetFideLeader(Player player, int time)
    {
        player.EffectSkill.IsFideLeader = true;
        player.EffectSkill.TimeFideLeader = time;
        player.EffectSkill.LastTimeFideLeader = Now();
    }

    // Cài đặt RemoveFideLeader theo hợp đồng dịch vụ hiệu ứng (Java: removeFideDaiCa).
    public void RemoveFideLeader(Player player) => player.EffectSkill.IsFideLeader = false;

    // Cài đặt SetIsHalloween theo hợp đồng dịch vụ hiệu ứng.
    public void SetIsHalloween(Player player, int outFit, int time)
    {
        if (!players.GetNPoint(player).IsBoneTransformationImmune)
        {
            player.EffectSkill.IsHalloween = true;
            player.EffectSkill.HalloweenOutfitId = outFit != -1 ? outFit : random.NextInt(3);
            player.EffectSkill.TimeHalloween = time;
            player.EffectSkill.LastTimeHalloween = Now();
            notifier.SendItemTime(player, 5101, time / 1000);
            notifier.SendCostume(player);
        }
    }

    // Cài đặt RemoveHalloween theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveHalloween(Player player)
    {
        if (!players.GetNPoint(player).IsBoneTransformationImmune)
        {
            player.EffectSkill.IsHalloween = false;
            notifier.SendCostume(player);
        }
    }

    // Cài đặt SetChibiBird theo hợp đồng dịch vụ hiệu ứng (Java: setChibi).
    public void SetChibiBird(Player player, int time)
    {
        player.EffectSkill.IsChibi = true;
        player.EffectSkill.TimeChibi = time;
        player.TypeChibi = random.NextInt(0, 3);
        if (player.TypeChibi == 3)
        {
            players.CalPoint(player);
            players.SetHp(player, CrisGH(players.GetNPoint(player).HpMax));
            notifier.Point(player);
            notifier.SendInfoNv(player);
        }

        player.EffectSkill.LastTimeChibi = Now();
        notifier.SendItemTime(player, 433, time / 1000);
        notifier.SendChibi(player);
        notifier.SendHaveChibiFollowToAllMap(player);
    }

    // Cài đặt RemoveChibiBird theo hợp đồng dịch vụ hiệu ứng (Java: removeChibi).
    public void RemoveChibiBird(Player player)
    {
        player.EffectSkill.IsChibi = false;
        player.TypeChibi = -1;
        notifier.SendItemTime(player, 433, 0);
        notifier.SendChibi(player);
        notifier.SendHaveChibiFollowToAllMap(player);
    }

    // ---- Mafuba, intrinsic, stone, body change, invisibility -------------------------------------------------------------------------------------------------------

    // Cài đặt SetBottled theo hợp đồng dịch vụ hiệu ứng (Java: setIsBinh).
    public void SetBottled(Player plAtt, Player player, int time)
    {
        int typeBinh = players.GetNewSkill(plAtt).TypeItem;
        player.EffectSkill.IsBottled = true;
        player.EffectSkill.BottledType = typeBinh;
        player.EffectSkill.TimeBottled = time;
        player.EffectSkill.MafubaUser = plAtt;
        player.EffectSkill.LastTimeBottled = Now();
        notifier.SendItemTime(player, typeBinh == 0 ? ItemTimeBinhNormal : ItemTimeBinhItem, time / 1000);
        notifier.SendCostume(player);
    }

    // Cài đặt RemoveBottled theo hợp đồng dịch vụ hiệu ứng (Java: BinhDown).
    public void RemoveBottled(Player player)
    {
        player.EffectSkill.IsBottled = false;
        notifier.SendCostume(player);
    }

    // Cài đặt StartUseMafuba theo hợp đồng dịch vụ hiệu ứng.
    public void StartUseMafuba(Player player, int time)
    {
        player.EffectSkill.IsUsingMafuba = true;
        player.EffectSkill.TimeUsingMafuba = time;
        player.EffectSkill.LastTimeUsingMafuba = Now();
    }

    // Cài đặt FinishUseMafuba theo hợp đồng dịch vụ hiệu ứng.
    public void FinishUseMafuba(Player player)
    {
        player.EffectSkill.IsUsingMafuba = false;
        EffectNewSkillState newSkill = players.GetNewSkill(player);
        foreach (Player playerMap in newSkill.PlayersTarget)
        {
            try
            {
                SetBottled(player, playerMap, 11000 * (newSkill.TypeItem == 0 ? 1 : 2));
                int x = players.GetLocation(player).X + (newSkill.Dir == -1 ? -75 : 75);
                int y = players.GetLocation(player).Y;
                if (players.HasZone(player) && !rules.IsMapBlackBallWar(players.GetMapId(player)))
                {
                    notifier.SetPos(playerMap, x, y);
                }
            }
            catch (Exception e)
            {
                LogError(e);
            }
        }

        // Giữ đặc điểm Java: thời gian quái phụ thuộc BottledType của chủ Ma Phong Ba, không phải newSkill.typeItem như người chơi ở trên.
        foreach (IEffectMobTarget mobMap in newSkill.MobsTarget)
        {
            mobMap.SetBottle(player, Now(), 11000 * (player.EffectSkill.BottledType == 0 ? 1 : 2));
        }
    }

    // Cài đặt SetIntrinsic theo hợp đồng dịch vụ hiệu ứng.
    public void SetIntrinsic(Player player, int skillId, int cooldown, long lastTimeUseSkill)
    {
        player.EffectSkill.IsIntrinsic = true;
        player.EffectSkill.SkillId = skillId;
        player.EffectSkill.Cooldown = cooldown;
        player.EffectSkill.LastTimeUseSkill = lastTimeUseSkill;
    }

    // Cài đặt ReleaseCooldownSkillByIntrinsic theo hợp đồng dịch vụ hiệu ứng.
    public void ReleaseCooldownSkillByIntrinsic(Player player)
    {
        player.EffectSkill.IsIntrinsic = false;
        EffectSkillRef? skill = players.GetSkillById(player, player.EffectSkill.SkillId);
        notifier.ReleaseCooldownSkill(player, skill);
    }

    // Cài đặt SetIntrinsicVip theo hợp đồng dịch vụ hiệu ứng.
    public void SetIntrinsicVip(Player player, int skillId, int cooldown, long lastTimeUseSkill)
    {
        player.EffectSkill.IsIntrinsicVip = true;
        player.EffectSkill.SkillIdVip = skillId;
        player.EffectSkill.CooldownVip = cooldown;
        player.EffectSkill.LastTimeUseSkillVip = lastTimeUseSkill;
    }

    // Cài đặt ReleaseCooldownSkillByIntrinsicVip theo hợp đồng dịch vụ hiệu ứng.
    public void ReleaseCooldownSkillByIntrinsicVip(Player player)
    {
        player.EffectSkill.IsIntrinsicVip = false;
        EffectSkillRef? skill = players.GetSkillById(player, player.EffectSkill.SkillIdVip);
        notifier.ReleaseCooldownSkill(player, skill);
    }

    // Cài đặt SetIsStone theo hợp đồng dịch vụ hiệu ứng.
    public void SetIsStone(Player player, int time)
    {
        if (rules.IsMapMaBu12H(players.GetMapId(player)))
        {
            players.HalveHp(player);
            notifier.SendInfoHp(player);
            notifier.SendInfoNv(player);
        }

        player.EffectSkill.IsStone = true;
        player.EffectSkill.TimeStone = time;
        player.EffectSkill.LastTimeStone = Now();
        notifier.SendItemTime(player, 4392, time / 1000);
        notifier.SendCostume(player);
        SendEffectPlayer(player, player, TURN_ON_EFFECT, STONE_EFFECT);
    }

    // Cài đặt RemoveStone theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveStone(Player player)
    {
        player.EffectSkill.IsStone = false;
        notifier.SendCostume(player);
        SendEffectPlayer(player, player, TURN_OFF_EFFECT, STONE_EFFECT);
    }

    // Cài đặt SetIsBodyChangeTechnique theo hợp đồng dịch vụ hiệu ứng.
    public void SetIsBodyChangeTechnique(Player player)
    {
        player.EffectSkill.IsBodyChangeTechnique = true;
        notifier.SendNotification(player, "Bạn đã bị biến thành Tiểu Đội Trưởng");
        notifier.ChangeAndSendTypePk(player, 5);
        IReadOnlyList<Player> inZone = players.GetPlayersInZone(player);
        for (int j = inZone.Count - 1; j >= 0; j--)
        {
            Player pl = inZone[j];
            notifier.PlayerInfoUpdate(player, pl, "!Tiểu đội trưởng", 180, 181, 182);
        }
    }

    // Cài đặt RemoveBodyChangeTechnique theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveBodyChangeTechnique(Player player)
    {
        notifier.ChangeAndSendTypePk(player, 0);

        // Giữ đặc thù Java: tại đây xóa isTanHinh, không phải isBodyChangeTechnique (Java: isTanHinh).
        player.EffectSkill.IsInvisible = false;
    }

    // Cài đặt SetInvisible theo hợp đồng dịch vụ hiệu ứng (Java: setIsTanHinh).
    public void SetInvisible(Player player, int time)
    {
        notifier.SetPos2(player, players.GetLocation(player).X, 10000);
        player.EffectSkill.IsInvisible = true;
        player.EffectSkill.TimeInvisible = time;
        player.EffectSkill.LastTimeInvisible = Now();
    }

    // Cài đặt RemoveInvisibility theo hợp đồng dịch vụ hiệu ứng (Java: removeTanHinh).
    public void RemoveInvisibility(Player player)
    {
        EffectLocation location = players.GetLocation(player);
        notifier.SetPos2(player, location.X, location.Y);
        player.EffectSkill.IsInvisible = false;
    }

    // ---- monkey ----------------------------------------------------------------------------------------------------------------------------------------------------

    // Cài đặt StartUseSkillMonkey theo hợp đồng dịch vụ hiệu ứng.
    public void StartUseSkillMonkey(Player player)
    {
        SendEffectMonkey(player);
        if (player.IsBoss)
        {
            SetIsMonkey(player);
            return;
        }

        notifier.SendSpeedPlayer(player, 0);
        player.EffectSkill.IsUseSkillMonkey = true;
        player.EffectSkill.TimeUseSkillMonkey = 1500;
        player.EffectSkill.LastTimeUseSkillMonkey = Now();
    }

    // Cài đặt FinishUseMonkey theo hợp đồng dịch vụ hiệu ứng.
    public void FinishUseMonkey(Player player)
    {
        player.EffectSkill.IsUseSkillMonkey = false;
        notifier.SendSpeedPlayer(player, -1);
        if (!players.IsDie(player))
        {
            SetIsMonkey(player);
        }
    }

    // Cài đặt SetIsMonkey theo hợp đồng dịch vụ hiệu ứng.
    public void SetIsMonkey(Player player)
    {
        SendEffectMonkey(player);
        EffectSkillRef selected = Require(players.GetSelectedSkill(player), "selected skill");
        int timeMonkey = rules.GetTimeMonkey(selected.Point);
        if (players.GetSetClothes(player).Cadic == 5)
        {
            timeMonkey = unchecked(timeMonkey * 5);
        }

        player.EffectSkill.IsMonkey = true;
        player.EffectSkill.TimeMonkey = timeMonkey;
        player.EffectSkill.LastTimeUpMonkey = Now();
        player.EffectSkill.LevelMonkey = unchecked((sbyte)selected.Point);
        players.SetHp(player, CrisGH(unchecked(players.GetNPoint(player).Hp * 2)));
        notifier.SendCostume(player);
        if (!player.IsPet && !player.IsFather && !player.IsMother && !player.IsLover && !player.IsChildOne && !player.IsChildTwo && !player.IsChildThree)
        {
            notifier.SendInfoHpMp(player);
        }

        notifier.Point(player);
        notifier.SendInfoNv(player);
        notifier.SendInfoPlayerEatPea(player);
    }

    // Cài đặt MonkeyDown theo hợp đồng dịch vụ hiệu ứng.
    public void MonkeyDown(Player player)
    {
        player.EffectSkill.IsMonkey = false;
        player.EffectSkill.LevelMonkey = 0;
        EffectNPointState state = players.GetNPoint(player);
        if (state.Hp > state.HpMax)
        {
            players.SetHp(player, CrisGH(state.HpMax));
        }

        SendEffectEndCharge(player);
        SendEffectMonkey(player);
        notifier.SetNotMonkey(player);
        notifier.SendCostume(player);
        notifier.Point(player);
        notifier.SendInfoHpMp(player);
        notifier.SendInfoNv(player);
        notifier.SendInfoPlayerEatPea(player);
    }

    // Cài đặt SetMoonFestivalMonkey theo hợp đồng dịch vụ hiệu ứng (Java: setIsMonkeyTrungThu).
    public void SetMoonFestivalMonkey(Player player)
    {
        EffectSkillRef khi = Require(players.GetSkillById(player, SkillBienKhi), "Bien Khi skill");
        int timeMonkey;
        if (khi.Point == 0)
        {
            timeMonkey = (1 + 5) * 10000;
        }
        else
        {
            timeMonkey = rules.GetTimeMonkey(khi.Point);
        }

        if (players.GetSetClothes(player).Cadic == 5)
        {
            timeMonkey = unchecked(timeMonkey * 5);
        }

        player.EffectSkill.IsMonkey = true;
        player.EffectSkill.TimeMonkey = timeMonkey;
        player.EffectSkill.LastTimeUpMonkey = Now();
        player.EffectSkill.LevelMonkey = khi.Point == 0 ? (sbyte)1 : unchecked((sbyte)khi.Point);
        players.SetHp(player, CrisGH(unchecked(players.GetNPoint(player).Hp * 2)));
    }

    // Cài đặt SetMoonFestivalMonkeyAsync theo hợp đồng dịch vụ hiệu ứng (Java: setIsMonkeyTrungThu).
    public async Task SetMoonFestivalMonkeyAsync(Player player, CancellationToken cancellationToken = default)
    {
        await scheduler.DelayAsync(TimeSpan.FromMilliseconds(200), cancellationToken).ConfigureAwait(false);
        SetMoonFestivalMonkey(player);
    }

    // ---- bind, hypnosis, stun, chocolate, teleport blind, whistle, charge, shield --------------------------------------------------------------------------------

    // Cài đặt RemoveBinding theo hợp đồng dịch vụ hiệu ứng (Java: removeUseTroi).
    public void RemoveBinding(Player player)
    {
        EffectSkill effectSkill = player.EffectSkill;
        effectSkill.BoundMob?.RemoveBind();
        if (effectSkill.BoundPlayer is not null)
        {
            RemoveBound(effectSkill.BoundPlayer);
        }

        effectSkill.IsBinding = false;
        effectSkill.BoundMob = null;
        effectSkill.BoundPlayer = null;
        SendEffectPlayer(player, player, TURN_OFF_EFFECT, HOLD_EFFECT);
        if (player.BodyClone is { } bodyClone && bodyClone.EffectSkill.IsBinding)
        {
            RemoveBinding(bodyClone);
        }
    }

    // Cài đặt RemoveBound theo hợp đồng dịch vụ hiệu ứng (Java: removeAnTroi).
    public void RemoveBound(Player player)
    {
        if (player is not null)
        {
            player.EffectSkill.IsBound = false;
            player.EffectSkill.BindingPlayer = null;
            SendEffectPlayer(player, player, TURN_OFF_EFFECT, HOLD_EFFECT);
        }
    }

    // Cài đặt SetBound theo hợp đồng dịch vụ hiệu ứng (Java: setAnTroi).
    public void SetBound(Player player, Player plTroi, long lastTimeAnTroi, int timeAnTroi)
    {
        player.EffectSkill.IsBound = true;
        player.EffectSkill.BindingPlayer = plTroi;
    }

    // Cài đặt SetBinding theo hợp đồng dịch vụ hiệu ứng (Java: setUseTroi).
    public void SetBinding(Player player, long lastTimeTroi, int timeTroi)
    {
        player.EffectSkill.IsBinding = true;
        player.EffectSkill.LastTimeBinding = lastTimeTroi;
        player.EffectSkill.TimeBinding = timeTroi;
    }

    // Cài đặt SetHypnosis theo hợp đồng dịch vụ hiệu ứng (Java: setThoiMien).
    public void SetHypnosis(Player player, long lastTimeThoiMien, int timeThoiMien)
    {
        player.EffectSkill.IsHypnosis = true;
        player.EffectSkill.LastTimeHypnosis = lastTimeThoiMien;
        player.EffectSkill.TimeHypnosis = timeThoiMien;
    }

    // Cài đặt RemoveHypnosis theo hợp đồng dịch vụ hiệu ứng (Java: removeThoiMien).
    public void RemoveHypnosis(Player player)
    {
        player.EffectSkill.IsHypnosis = false;
        SendEffectPlayer(player, player, TURN_OFF_EFFECT, SLEEP_EFFECT);
    }

    // Cài đặt StartStun theo hợp đồng dịch vụ hiệu ứng.
    public void StartStun(Player player, long lastTimeStartBlind, int timeBlind)
    {
        player.EffectSkill.LastTimeStartStun = lastTimeStartBlind;
        player.EffectSkill.TimeStun = timeBlind;
        player.EffectSkill.IsStun = true;
        SendEffectPlayer(player, player, TURN_ON_EFFECT, BLIND_EFFECT);
    }

    // Cài đặt RemoveStun theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveStun(Player player)
    {
        player.EffectSkill.IsStun = false;
        SendEffectPlayer(player, player, TURN_OFF_EFFECT, BLIND_EFFECT);
    }

    // Cài đặt SetChocolate theo hợp đồng dịch vụ hiệu ứng (Java: setSocola).
    public void SetChocolate(Player player, long lastTimeSocola, int timeSocola)
    {
        player.EffectSkill.LastTimeChocolate = lastTimeSocola;
        player.EffectSkill.TimeChocolate = timeSocola;
        player.EffectSkill.IsChocolate = true;
        player.EffectSkill.OneHpHitCount = 0;
    }

    // Cài đặt RemoveChocolate theo hợp đồng dịch vụ hiệu ứng (Java: removeSocola).
    public void RemoveChocolate(Player player)
    {
        player.EffectSkill.IsChocolate = false;
        notifier.SendCostume(player);
    }

    // Cài đặt SetTeleportBlind theo hợp đồng dịch vụ hiệu ứng (Java: setBlindDCTT).
    public void SetTeleportBlind(Player player, long lastTimeDCTT, int timeBlindDCTT)
    {
        player.EffectSkill.IsTeleportBlind = true;
        player.EffectSkill.LastTimeTeleportBlind = lastTimeDCTT;
        player.EffectSkill.TimeTeleportBlind = timeBlindDCTT;
    }

    // Cài đặt RemoveTeleportBlind theo hợp đồng dịch vụ hiệu ứng (Java: removeBlindDCTT).
    public void RemoveTeleportBlind(Player player)
    {
        player.EffectSkill.IsTeleportBlind = false;
        SendEffectPlayer(player, player, TURN_OFF_EFFECT, BLIND_EFFECT);
    }

    // Cài đặt StartWhistle theo hợp đồng dịch vụ hiệu ứng (Java: setStartHuytSao).
    public void StartWhistle(Player player, int hpPercent)
    {
        player.EffectSkill.HpWhistlePercent = hpPercent;
        player.EffectSkill.LastTimeWhistle = Now();
    }

    // Cài đặt RemoveWhistle theo hợp đồng dịch vụ hiệu ứng (Java: removeHuytSao).
    public void RemoveWhistle(Player player)
    {
        player.EffectSkill.HpWhistlePercent = 0;
        SendEffectPlayer(player, player, TURN_OFF_EFFECT, HUYT_SAO_EFFECT);
        notifier.Point(player);
        notifier.SendInfoNv(player);
    }

    // Cài đặt StartCharge theo hợp đồng dịch vụ hiệu ứng.
    public void StartCharge(Player player)
    {
        if (!player.EffectSkill.IsCharging)
        {
            player.EffectSkill.IsCharging = true;
            SendEffectCharge(player);
        }
    }

    // Cài đặt StopCharge theo hợp đồng dịch vụ hiệu ứng.
    public void StopCharge(Player player)
    {
        player.EffectSkill.CountCharging = 0;
        player.EffectSkill.IsCharging = false;
        SendEffectStopCharge(player);
    }

    // Cài đặt SetStartShield theo hợp đồng dịch vụ hiệu ứng.
    public void SetStartShield(Player player)
    {
        player.EffectSkill.IsShielding = true;
        player.EffectSkill.LastTimeShieldUp = Now();
        player.EffectSkill.TimeShield = rules.GetTimeShield(Require(players.GetSelectedSkill(player), "selected skill").Point);
    }

    // Cài đặt RemoveShield theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveShield(Player player)
    {
        player.EffectSkill.IsShielding = false;
        SendEffectPlayer(player, player, TURN_OFF_EFFECT, SHIELD_EFFECT);
    }

    // Cài đặt BreakShield theo hợp đồng dịch vụ hiệu ứng.
    public void BreakShield(Player player)
    {
        RemoveShield(player);
        notifier.SendNotification(player, "Khiên năng lượng đã bị vỡ!");
        notifier.RemoveItemTime(player, 3784);
    }

    // ---- transformations -----------------------------------------------------------------------------------------------------------------------------------------

    // Cài đặt SetCumber theo hợp đồng dịch vụ hiệu ứng (Java: setiscumber).
    public void SetCumber(Player player)
    {
        player.EffectSkill.IsCumber = true;
        player.EffectSkill.TimeCumber = 180000;
        player.EffectSkill.LastTimeCumber = Now();
        notifier.SendAllItemTime(player);
    }

    // Cài đặt SetCumberAsync theo hợp đồng dịch vụ hiệu ứng (Java: setiscumber).
    public async Task SetCumberAsync(Player player, CancellationToken cancellationToken = default)
    {
        await scheduler.DelayAsync(TimeSpan.FromMilliseconds(1000), cancellationToken).ConfigureAwait(false);
        SetCumber(player);
    }

    // Cài đặt RemoveCumber theo hợp đồng dịch vụ hiệu ứng (Java: cumberdown).
    public void RemoveCumber(Player player)
    {
        player.EffectSkill.IsCumber = false;
        EndTransformationCommon(player);
    }

    // Cài đặt SetCumber2 theo hợp đồng dịch vụ hiệu ứng (Java: setiscumber2).
    public void SetCumber2(Player player)
    {
        player.EffectSkill.IsCumber2 = true;
        player.EffectSkill.TimeCumber2 = 180000;
        player.EffectSkill.LastTimeCumber2 = Now();
        notifier.SendAllItemTime(player);
    }

    // Cài đặt SetCumber2Async theo hợp đồng dịch vụ hiệu ứng (Java: setiscumber2).
    public async Task SetCumber2Async(Player player, CancellationToken cancellationToken = default)
    {
        await scheduler.DelayAsync(TimeSpan.FromMilliseconds(1000), cancellationToken).ConfigureAwait(false);
        SetCumber2(player);
    }

    // Cài đặt RemoveCumber2 theo hợp đồng dịch vụ hiệu ứng (Java: cumberdown2).
    public void RemoveCumber2(Player player)
    {
        player.EffectSkill.IsCumber2 = false;
        EndTransformationCommon(player);
    }

    // Cài đặt SetPain theo hợp đồng dịch vụ hiệu ứng (Java: setispain).
    public void SetPain(Player player)
    {
        player.EffectSkill.IsPain = true;
        player.EffectSkill.TimePain = 180000;
        player.EffectSkill.LastTimeUpPain = Now();
        notifier.SendAllItemTime(player);
    }

    // Cài đặt SetPainAsync theo hợp đồng dịch vụ hiệu ứng (Java: setispain).
    public async Task SetPainAsync(Player player, CancellationToken cancellationToken = default)
    {
        await scheduler.DelayAsync(TimeSpan.FromMilliseconds(1000), cancellationToken).ConfigureAwait(false);
        SetPain(player);
    }

    // Cài đặt RemovePain theo hợp đồng dịch vụ hiệu ứng (Java: paindown).
    public void RemovePain(Player player)
    {
        player.EffectSkill.IsPain = false;
        EndTransformationCommon(player);
    }

    // Cài đặt SetKefla theo hợp đồng dịch vụ hiệu ứng (Java: setiskefla).
    public void SetKefla(Player player)
    {
        player.EffectSkill.IsKefla = true;
        player.EffectSkill.TimeKefla = 300000;
        player.EffectSkill.LastTimeKefla = Now();
        notifier.SendAllItemTime(player);
    }

    // Cài đặt SetKeflaAsync theo hợp đồng dịch vụ hiệu ứng (Java: setiskefla).
    public async Task SetKeflaAsync(Player player, CancellationToken cancellationToken = default)
    {
        await scheduler.DelayAsync(TimeSpan.FromMilliseconds(1000), cancellationToken).ConfigureAwait(false);
        SetKefla(player);
    }

    // Cài đặt RemoveKefla theo hợp đồng dịch vụ hiệu ứng (Java: kefladown).
    public void RemoveKefla(Player player)
    {
        player.EffectSkill.IsKefla = false;
        EndTransformationCommon(player);
    }

    // Cài đặt SetMonkeyTail theo hợp đồng dịch vụ hiệu ứng (Java: setHasMonkeyTail).
    public void SetMonkeyTail(Player player) => players.StartDuoiKhi(player, Now());

    // Cài đặt SetMonkeyTailAsync theo hợp đồng dịch vụ hiệu ứng (Java: setHasMonkeyTail).
    public async Task SetMonkeyTailAsync(Player player, CancellationToken cancellationToken = default)
    {
        await scheduler.DelayAsync(TimeSpan.FromMilliseconds(2000), cancellationToken).ConfigureAwait(false);
        SetMonkeyTail(player);
    }

    // Cài đặt RemoveMonkeyTail theo hợp đồng dịch vụ hiệu ứng (Java: DuoiKhiDown).
    public void RemoveMonkeyTail(Player player)
    {
        players.StopDuoiKhi(player);
        EndTransformationCommon(player);
    }

    // Cài đặt SetTransformation theo hợp đồng dịch vụ hiệu ứng (Java: setIsTranformation).
    public void SetTransformation(Player player)
    {
        int timeTranformation = rules.GetTimeTranformation();
        player.TransformationState = 0;
        players.SaveTransformationState(player);
        player.EffectSkill.IsTransformation = true;
        player.EffectSkill.TimeTransformation = timeTranformation;
        player.EffectSkill.TransformationLevel = unchecked((sbyte)Require(players.GetSelectedSkill(player), "selected skill").Point);
        notifier.SendSpeedPlayer(player, 7);
        SendTransformationEffect(player);
        notifier.SendCostume(player);
        notifier.Point(player);
    }

    // Cài đặt SetEvolution theo hợp đồng dịch vụ hiệu ứng (Java: setIsEvolution).
    public void SetEvolution(Player player)
    {
        int timeTranformation = rules.GetTimett();
        player.EffectSkill.IsEvolution = true;
        player.EffectSkill.TimeEvolution = timeTranformation;
        player.EffectSkill.LastTimeEvolution = Now();
        SendEvolutionEffect(player);
        notifier.SendCostume(player);
        notifier.Point(player);
    }

    // Cài đặt RemoveTransformation theo hợp đồng dịch vụ hiệu ứng (Java: TranformationDown).
    public void RemoveTransformation(Player player)
    {
        player.TransformationState = 0;
        players.SaveTransformationState(player);
        player.EffectSkill.IsTransformation = false;
        player.EffectSkill.TransformationLevel = 0;
        EffectNPointState state = players.GetNPoint(player);
        if (state.Hp > state.HpMax)
        {
            players.SetHp(player, CrisGH(state.HpMax));
        }

        SendEffectEndCharge(player);
        SendTransformationEffect(player);
        notifier.SetNotTranformation(player);
        notifier.SendCostume(player);
        notifier.Point(player);
        notifier.SendInfoHpMp(player);
        notifier.SendInfoNv(player);
    }

    // Cài đặt RemoveEvolution theo hợp đồng dịch vụ hiệu ứng (Java: EvolutionDown).
    public void RemoveEvolution(Player player)
    {
        player.TransformationState = 0;
        players.SaveTransformationState(player);
        player.EffectSkill.IsEvolution = false;
        EffectNPointState state = players.GetNPoint(player);
        if (state.Hp > state.HpMax)
        {
            players.SetHp(player, CrisGH(state.HpMax));
        }

        SendEffectEndCharge(player);
        SendEvolutionEffect(player);
        notifier.SetNotVolution(player);
        notifier.SendCostume(player);
        notifier.Point(player);
        notifier.SendInfoHpMp(player);
        notifier.SendInfoNv(player);
    }

    // Cài đặt StartPowerBurst theo hợp đồng dịch vụ hiệu ứng (Java: setStartSucManhBocPha).
    public void StartPowerBurst(Player player)
    {
        player.EffectSkill.IsPowerBurst = true;
        player.EffectSkill.LastTimePowerBurst = Now();
        player.EffectSkill.TimePowerBurst = rules.GetPowerBurstDuration(Require(players.GetSelectedSkill(player), "selected skill").Point);
    }

    // Cài đặt RemovePowerBurst theo hợp đồng dịch vụ hiệu ứng (Java: removeSucManhBocPha).
    public void RemovePowerBurst(Player player)
    {
        player.EffectSkill.IsPowerBurst = false;
        SendEffectPlayer(player, player, TURN_OFF_EFFECT, SUC_MANH_BOC_PHA_EFFECT);
        notifier.Point(player);
        notifier.SendInfoNv(player);
        notifier.SendInfoHpMp(player);
    }
}
