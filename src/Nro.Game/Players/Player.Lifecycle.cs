using Nro.Game.Items.Core;
using Nro.Game.Players.Ports;

namespace Nro.Game.Players;

// Vòng đời tick xác định của Player do Zone gọi; không port Runnable/run/start hoặc Thread Java.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 848–1239).
public partial class Player
{
    // Cập nhật Player một lần theo thứ tự Java; Zone actor chịu trách nhiệm nhịp tick và một luồng ghi (Java: update).
    public void Update(PlayerRuntimeContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Khác Java có chủ đích: Dispose C# là terminal và BeforeDispose cũng dừng tick (Java: beforeDispose).
        if (BeforeDispose || IsDisposed)
        {
            return;
        }

        try
        {
            long now = context.Clock.NowMilliseconds;
            IPlayerRuntimeServices services = context.Services;
            if (Zone is not null || !IsPl())
            {
                services.UpdateItemTime(this);
                if (MagicTree is not null)
                {
                    services.UpdateMagicTree(this);
                }

                if (IsPl() && Zone is { } tutorialZone && tutorialZone.Map.MapId == Gender + 21 && services.IsStarterTask(this))
                {
                    services.AdvanceStarterTask(this);
                }
            }

            // Khác Java có chủ đích: guard Zone null trước zone.map để player tách zone không ném NullReferenceException.
            if (IsPl() && Zone is { } homeZone && services.IsHomeMap(homeZone.Map.MapId) && IDMark is not null && services.ShouldDisconnectBannedPlayer(this, now))
            {
                services.Disconnect(this);
                return;
            }

            if ((Zone is { } activeZone && !services.IsHomeMap(activeZone.Map.MapId)) || (!IsPl() && Zone is null))
            {
                if (IsPl() && IDMark is not null && services.ShouldDisconnectBannedPlayer(this, now))
                {
                    services.Disconnect(this);
                    return;
                }

                UpdateActivePlayerComponents(context);
                ExpireTemporaryTitles(now);

                if (Zone is { } homeMapZone && homeMapZone.Map.MapId == 21 + Gender)
                {
                    if (MabuEgg is not null)
                    {
                        services.SendMabuEgg(this);
                    }

                    if (WatermelonPlant is not null)
                    {
                        services.SendWatermelon(this);
                    }
                }

                if (IsPl())
                {
                    services.AdvanceDedicatedAchievement(this, 1_000);
                }

                if (Zone is { } effectZone)
                {
                    services.NormalizeEffectSkinBonuses(this, services.IsBlackBallWarMap(effectZone.Map.MapId));
                }

                if (IsPl() && Zone is not null)
                {
                    services.FixBlackBallWar(this);
                }

                if (IsPl() && SuperRank is not null && PointRank == 0 && services.IsAfterMidnightForSuperRank(this, now))
                {
                    services.RewardSuperRank(this);
                    PointRank = 1;
                }

                if (IsAutoRespawn && IsDead())
                {
                    services.AutoRespawn(this);
                }

                ResetDamageCounters(now, services);
                if (IsPl())
                {
                    UpdatePlayerOnly(now, context);
                }

                UpdateRemainingEvents(now, context);
            }
        }
        catch (Exception exception)
        {
            context.Services.ReportUpdateFailure(this, exception);
        }
    }

    // Đẩy các component có Update đã port và các placeholder qua cổng theo đúng thứ tự Java (Java: PhanThan.update ... dropItem.update).
    private void UpdateActivePlayerComponents(PlayerRuntimeContext context)
    {
        IPlayerRuntimeServices services = context.Services;
        if (BodyClone is not null)
        {
            services.UpdateBodyClone(this);
        }

        if (Saibamen is not null)
        {
            if (context.CompanionGateway is not null)
            {
                Saibamen.Update(context, context.CompanionGateway);
            }
            else
            {
                services.UpdateSaiBaMen(this);
            }
        }

        services.UpdateNPoint(this);
        services.UpdateFusion(this);
        services.UpdateEffectSkill(this);
        if (EggMob is not null)
        {
            services.UpdateEggMob(this);
        }

        services.UpdateEffectSkin(this);
        if (Pet is not null)
        {
            services.UpdatePet(this);
        }

        if (PetFollow is not null)
        {
            if (context.CompanionGateway is not null)
            {
                PetFollow.Update(context, context.CompanionGateway);
            }
            else
            {
                services.UpdatePetFollow(this);
            }
        }

        if (TangMonk is not null)
        {
            services.UpdateTangMonk(this);
        }

        services.UpdateSatellite(this);
        services.UpdateDropItems(this);
    }

    // Hết hạn bốn danh hiệu tạm sau sáu giây, dùng đồng hồ injected thay System.currentTimeMillis (Java: LastTimeDanhHieu_*).
    private void ExpireTemporaryTitles(long now)
    {
        if (LastEmperorTitleTime != 0 && CanDoWithTime(now, LastEmperorTitleTime, 6_000))
        {
            LastEmperorTitleTime = 0;
            UsesEmperorTitle = false;
        }

        if (LastTitleTime_2 != 0 && CanDoWithTime(now, LastTitleTime_2, 6_000))
        {
            LastTitleTime_2 = 0;
            UsesTitle_2 = false;
        }

        if (LastTitleTime_3 != 0 && CanDoWithTime(now, LastTitleTime_3, 6_000))
        {
            LastTitleTime_3 = 0;
            UsesTitle_3 = false;
        }

        if (LastTitleTime_4 != 0 && CanDoWithTime(now, LastTitleTime_4, 6_000))
        {
            LastTitleTime_4 = 0;
            UsesTitle_4 = false;
        }
    }

    // Cập nhật bộ đếm sát thương, giữ số học điểm máy đấm Java (Java: lastTimeDame, lastTimeDameMayDam).
    private void ResetDamageCounters(long now, IPlayerRuntimeServices services)
    {
        if (CanDoWithTime(now, LastDamageTime, 5_000) && TotalDamage != 0)
        {
            TotalDamage = 0;
            ResetDamage = true;
        }

        if (!CanDoWithTime(now, LastPunchingMachineDamageTime, 2_000) || PunchingMachineTotalDamage <= 0)
        {
            return;
        }

        long score = Math.Max(1L, (long)(Math.Sqrt(PunchingMachineTotalDamage / 10.0) * 2));
        services.SendPunchingMachineScore(this, score);
        switch (Gender)
        {
            case 0:
                EarthlingPunchingMachineRank += score;
                break;
            case 1:
                NamekianPunchingMachineRank += score;
                break;
            case 2:
                SaiyanPunchingMachineRank += score;
                break;
        }

        PunchingMachineTotalDamage = 0;
    }

    // Cập nhật phần chỉ có người chơi thật theo đúng thứ tự Java (Java: UpdateOptionItem ... BossNomalService).
    private void UpdatePlayerOnly(long now, PlayerRuntimeContext context)
    {
        IPlayerRuntimeServices services = context.Services;
        UpdateOptionItem(now, services);
        if ((services.CurrentHour < 15 || services.CurrentHour > 16) && (services.CurrentHour < 22 || services.CurrentHour > 23)
            && Zone is { } hirudegarnZone && hirudegarnZone.Map.MapId == 126)
        {
            services.LeaveHirudegarn(this, 1_000 + context.Random.NextInt(-100, 100), 360);
        }

        if (services.IsUsingSuperRegen(this))
        {
            services.RestoreSuperDivineWater(this);
        }

        if (CanDoWithTime(now, LastTimeAttack, 7_000))
        {
            DamageBonusPercent = 0;
        }

        services.UpdateNguHanhSon(this);
        if (Roll(context.Random, 20, 100))
        {
            UpdateBossEvent(context);
        }

        services.SendSubTaskUpdate(this);
        AutoSendBadges(now, services);
        services.UpdateBadgeTasks(this);
        services.SendDailyGiftReminders(this, now);
        services.UpdateNamekWar(this);
        UpdateOptionClan(services);
        if (Clan is not null)
        {
            services.CheckClanTask(this);
            services.ResetExpiredClanExperienceBuff(this, now);
        }
        if (CanDoWithTime(now, LastLifecycleLoginUpdate, 60_000))
        {
            services.UpdateLastLogin(this);
            LastLifecycleLoginUpdate = now;
        }

        if (!services.HasMagicBottle(this))
        {
            foreach (Item item in Inventory.ItemsBag.ToArray())
            {
                if (item.IsNotNullItem() && item.Template!.Id == 1_258 && Roll(context.Random, 20, 100))
                {
                    services.ReleaseGhostSoul(this, item);
                }
            }
        }

        services.UpdateAutomaticWaterDonation(this);
    }

    // Cập nhật hạn dùng vật phẩm mỗi giây; giữ nguyên điều kiện OR đáng ngờ của Java (Java: UpdateOptionItem).
    private void UpdateOptionItem(long now, IPlayerRuntimeServices services)
    {
        if (!CanDoWithTime(now, ResetDateOut, 1_000))
        {
            return;
        }

        foreach (Item item in Inventory.ItemsBag)
        {
            if (item.GetOptionParam(260) > 0 || item.GetOptionParam(261) > 0)
            {
                services.LoadItemTimeStatus(this, item);
            }
        }

        foreach (Item item in Inventory.ItemsBag.ToArray())
        {
            // Giữ `||` của Java dù có vẻ nên là &&; không tự thay đổi hành vi (Java: option261 < 1 || option260 < 1).
            if ((item.GetOptionParam(261) < 1 || item.GetOptionParam(260) < 1) && services.IsOutOfDateTime(item))
            {
                services.RemoveExpiredItem(this, item);
            }
        }

        ResetDateOut = now;
    }

    // Lưu mốc login chỉ cho lifecycle vì Player Java dùng static LAST_TIME_UPDATE_LOGIN (Java: LAST_TIME_UPDATE_LOGIN).
    private static long LastLifecycleLoginUpdate { get; set; }

    // Rút ngẫu nhiên phần trăm Java qua IPlayerRandom (Java: Util.isTrue).
    private static bool Roll(IPlayerRandom random, int numerator, int denominator) => random.NextInt(1, denominator) <= numerator;
}
