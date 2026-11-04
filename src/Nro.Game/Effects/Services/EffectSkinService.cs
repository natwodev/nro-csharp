using System.Runtime.CompilerServices;
using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.IO;

namespace Nro.Game.Effects.Services;

// Bật, tắt hiệu ứng skin có thời hạn của người chơi; singleton gI() Java trở thành lớp thường do EffectEnvironment.Create tạo.
// Nguồn Java: nro/effect/EffectSkinService.java (các hàm set nhận mốc bắt đầu do caller truyền như Java để kiểm thử kiểm soát thời gian; KHÁC BIỆT CÓ CHỦ ĐÍCH: IOException Java gọi printStackTrace, C# ghi qua GameLogger tiêm vào).
public sealed class EffectSkinService : IEffectSkinService
{
    public const sbyte TURN_ON_EFFECT = 1;
    public const sbyte TURN_OFF_EFFECT = 0;
    public const sbyte TURN_OFF_ALL_EFFECT = 2;
    public const sbyte BLIND_EFFECT = 40;
    public const sbyte SLEEP_EFFECT = 41;
    public const sbyte STONE_EFFECT = 42;

    private readonly GameLogger logger;
    private readonly IEffectMapService mapEffects;
    private readonly IEffectSkillService skills;
    private readonly IEffectBroadcaster broadcaster;
    private readonly IEffectClientNotifier notifier;
    private readonly IEffectPlayerAccess players;

    public EffectSkinService(
        GameLogger logger,
        IEffectMapService mapEffects,
        IEffectSkillService skills,
        IEffectBroadcaster broadcaster,
        IEffectClientNotifier notifier,
        IEffectPlayerAccess players)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mapEffects);
        ArgumentNullException.ThrowIfNull(skills);
        ArgumentNullException.ThrowIfNull(broadcaster);
        ArgumentNullException.ThrowIfNull(notifier);
        ArgumentNullException.ThrowIfNull(players);
        this.logger = logger;
        this.mapEffects = mapEffects;
        this.skills = skills;
        this.broadcaster = broadcaster;
        this.notifier = notifier;
        this.players = players;
    }

    // Cài đặt SetDraburaFrost theo hợp đồng dịch vụ hiệu ứng.
    public void SetDraburaFrost(Player player, long lastTime, int time)
    {
        player.EffectSkin.LastTimeDraburaFrost = lastTime;
        player.EffectSkin.TimeDraburaFrost = time;
        player.EffectSkin.IsDraburaFrost = true;
        mapEffects.SendFrozenCharacterEffect(player);
        skills.SendEffectPlayer(player, player, EffectSkillService.TURN_ON_EFFECT, EffectSkillService.ICE_EFFECT);
        notifier.Point(player);
    }

    // Cài đặt RemoveDraburaFrost theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveDraburaFrost(Player player)
    {
        player.EffectSkin.IsDraburaFrost = false;
        mapEffects.RemoveFrozenCharacterEffect(player);
        SendEffectPlayer(player, player, EffectSkillService.TURN_OFF_EFFECT, EffectSkillService.ICE_EFFECT);
        notifier.Point(player);
        notifier.SendCostume(player);
    }

    // Cài đặt SetSlow theo hợp đồng dịch vụ hiệu ứng.
    public void SetSlow(Player player, long lastTimeSlow, int timeSlow)
    {
        player.EffectSkin.LastTimeSlow = lastTimeSlow;
        player.EffectSkin.TimeSlow = timeSlow;
        player.EffectSkin.IsSlow = true;
        notifier.Point(player);
    }

    // Cài đặt RemoveSlow theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveSlow(Player player)
    {
        player.EffectSkin.IsSlow = false;
        notifier.Point(player);
    }

    // Cài đặt SetChocolate theo hợp đồng dịch vụ hiệu ứng (Java: setSocola).
    public void SetChocolate(Player player, long lastTimeSocola, int timeSocola)
    {
        player.EffectSkin.LastTimeChocolate = lastTimeSocola;
        player.EffectSkin.TimeChocolate = timeSocola;
        player.EffectSkin.IsChocolate = true;
        notifier.Point(player);
    }

    // Cài đặt RemoveChocolate theo hợp đồng dịch vụ hiệu ứng (Java: removeSocola).
    public void RemoveChocolate(Player player)
    {
        player.EffectSkin.IsChocolate = false;
        notifier.Point(player);
        notifier.SendCostume(player);
    }

    // Cài đặt SetRabbitLeader theo hợp đồng dịch vụ hiệu ứng (Java: setThoDaiKa).
    public void SetRabbitLeader(Player player, long lastTime, int time)
    {
        if (players.GetNPoint(player).IsRabbitLeader)
        {
            return;
        }

        player.EffectSkin.LastTimeRabbitLeader = lastTime;
        player.EffectSkin.TimeRabbitLeader = time;
        player.EffectSkin.IsRabbitLeader = true;
        notifier.Point(player);
    }

    // Cài đặt RemoveRabbitLeader theo hợp đồng dịch vụ hiệu ứng (Java: removeThoDaiKa).
    public void RemoveRabbitLeader(Player player)
    {
        player.EffectSkin.IsRabbitLeader = false;
        notifier.Point(player);
        notifier.SendCostume(player);
    }

    // Cài đặt SetRage theo hợp đồng dịch vụ hiệu ứng (Java: setCuongNo).
    public void SetRage(Player player, long lastTimeRage, int timeRage)
    {
        player.EffectSkin.IsNezuko = true;
        player.EffectSkin.LastTimeNezuko = lastTimeRage;
        player.EffectSkin.TimeNezuko = timeRage;
        notifier.Point(player);
    }

    // Cài đặt RemoveRage theo hợp đồng dịch vụ hiệu ứng (Java: removeCuongNo).
    public void RemoveRage(Player player)
    {
        player.EffectSkin.IsNezuko = false;
        notifier.Point(player);
    }

    // Cài đặt SetInosuke theo hợp đồng dịch vụ hiệu ứng.
    public void SetInosuke(Player player, long lastTimeTanjiro, int timeTanjiro)
    {
        player.EffectSkin.IsInosuke = true;
        player.EffectSkin.LastTimeInosuke = lastTimeTanjiro;
        player.EffectSkin.TimeInosuke = timeTanjiro;
        notifier.Point(player);
    }

    // Cài đặt RemoveInosuke theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveInosuke(Player player)
    {
        player.EffectSkin.IsInosuke = false;
        notifier.Point(player);
    }

    // Cài đặt SetTanjiro theo hợp đồng dịch vụ hiệu ứng.
    public void SetTanjiro(Player player, long lastTimeTanjiro, int timeTanjiro)
    {
        player.EffectSkin.IsTanjiro = true;
        player.EffectSkin.LastTimeTanjiro = lastTimeTanjiro;
        player.EffectSkin.TimeTanjiro = timeTanjiro;
        notifier.Point(player);
    }

    // Cài đặt RemoveTanjiro theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveTanjiro(Player player)
    {
        player.EffectSkin.IsTanjiro = false;
        notifier.Point(player);
    }

    // Cài đặt SetZenitsu theo hợp đồng dịch vụ hiệu ứng.
    public void SetZenitsu(Player player, long lastTimeZenitsu, int timeZenitsu)
    {
        player.EffectSkin.IsZenitsu = true;
        player.EffectSkin.LastTimeZenitsu = lastTimeZenitsu;
        player.EffectSkin.TimeZenitsu = timeZenitsu;
        notifier.Point(player);
    }

    // Cài đặt RemoveZenitsu theo hợp đồng dịch vụ hiệu ứng.
    public void RemoveZenitsu(Player player)
    {
        player.EffectSkin.IsZenitsu = false;
        notifier.Point(player);
    }

    // Cài đặt SetInosukeHashibira theo hợp đồng dịch vụ hiệu ứng (Java: setInoHashi).
    public void SetInosukeHashibira(Player player, long lastTimeInoHashi, int timeInoHashi)
    {
        player.EffectSkin.IsInoHashi = true;
        player.EffectSkin.LastTimeInoHashi = lastTimeInoHashi;
        player.EffectSkin.TimeInoHashi = timeInoHashi;
        notifier.Point(player);
    }

    // Cài đặt RemoveInosukeHashibira theo hợp đồng dịch vụ hiệu ứng (Java: removeInoHashi).
    public void RemoveInosukeHashibira(Player player)
    {
        player.EffectSkin.IsInoHashi = false;
        notifier.Point(player);
    }

    // Cài đặt SendEffectPlayer theo hợp đồng dịch vụ hiệu ứng.
    public void SendEffectPlayer(Player plUseSkill, Player plTarget, sbyte toggle, sbyte effect)
    {
        try
        {
            var msg = new Message(-124);
            msg.Writer().WriteByte(toggle);
            msg.Writer().WriteByte(0);
            if (toggle == TURN_OFF_ALL_EFFECT)
            {
                msg.Writer().WriteInt(unchecked((int)plTarget.Id));
            }
            else
            {
                msg.Writer().WriteByte(effect);
                msg.Writer().WriteInt(unchecked((int)plTarget.Id));
                msg.Writer().WriteInt(unchecked((int)plUseSkill.Id));
            }

            broadcaster.SendToZoneOf(plUseSkill, msg);
            msg.Cleanup();
        }
        catch (Exception e)
        {
            LogError(e);
        }
    }

    private void LogError(Exception e, [CallerMemberName] string method = "") => logger.LogException(typeof(EffectSkinService), e, null, method);
}
