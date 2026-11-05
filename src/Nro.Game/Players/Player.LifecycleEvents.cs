using Nro.Game.Players.Ports;

namespace Nro.Game.Players;

// Các helper sự kiện của vòng đời Player, tách khỏi orchestration để cổng runtime/clock/random dễ kiểm thử.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 1152–1458).
public partial class Player
{
    // Cộng hoặc gỡ ba hệ số bang khi có ít nhất hai thành viên cùng bang trong bán kính 300 (Java: UpdateOptionClan).
    private void UpdateOptionClan(IPlayerRuntimeServices services)
    {
        if (Zone is null)
        {
            return;
        }

        int nearby = 0;
        foreach (Player candidate in Zone.Players)
        {
            if (!ReferenceEquals(candidate, this) && services.AreSameClan(candidate, this) && services.GetDistance(this, candidate) <= 300)
            {
                nearby++;
            }
        }

        if (nearby >= 2)
        {
            services.GetClanBonuses(this, out int hp, out int damage, out int mp);
            ClanHpBonusPercent = hp;
            ClanDamageBonusPercent = damage;
            ClanMpBonusPercent = mp;
        }
        else
        {
            ClanHpBonusPercent = 0;
            ClanDamageBonusPercent = 0;
            ClanMpBonusPercent = 0;
        }

        services.SendPoint(this);
    }

    // Hướng dẫn giao tuần lộc cho Ông già Noel theo điều kiện và khoảng cách Java (Java: updateBossEvent).
    private void UpdateBossEvent(PlayerRuntimeContext context)
    {
        if (Zone is null || IsDead())
        {
            return;
        }

        // Java Player.java 1183–1186: zone không có boss thì thoát im lặng, kể cả player đủ điều kiện tuần lộc.
        if (Zone.Bosses is not { Count: > 0 })
        {
            return;
        }

        Player? santa = null;
        foreach (Player boss in Zone.Bosses)
        {
            if (context.Services.IsSantaBoss(boss))
            {
                santa = boss;
                break;
            }
        }

        if (!CanReward_TuanLoc || HasReward_TuanLoc)
        {
            return;
        }

        if (santa is null)
        {
            context.Services.SendBossEventNotification(this, "Hãy mang Tuần lộc trả cho Ông già Noel nào!");
            return;
        }

        if (context.Services.GetDistance(this, santa) > 200)
        {
            context.Services.SendBossEventNotification(this, "Hãy đến gần Ông già Noel hơn!");
            return;
        }

        if (Roll(context.Random, 50, 100))
        {
            context.Services.OpenSantaReindeerMenu(this);
        }
    }

    // Xóa huy hiệu hết hạn, chọn huy hiệu dùng và gửi lại mỗi hơn mười giây (Java: autoSendBadges).
    public void AutoSendBadges(long now, IPlayerRuntimeServices services)
    {
        ArgumentNullException.ThrowIfNull(services);
        for (int i = 0; i < DataBadges.Count;)
        {
            var data = DataBadges[i];
            if (now >= data.TimeofUseBadges)
            {
                DataBadges.RemoveAt(i);
                continue;
            }

            if (data.IsUse)
            {
                Badges.IdBadges = data.IdBadGes;
            }

            i++;
        }

        if (Badges.IdBadges != -1 && CanDoWithTime(now, Badges.LastTimeSendBadges, 10_000))
        {
            services.SendBadge(this, Badges.IdBadges);
            Badges.LastTimeSendBadges = now;
            services.UpdateNPoint(this);
            services.SendPoint(this);
        }
    }

    // Bật Đuôi Khỉ Trung Thu khi hết cooldown, chỉ cho Xayda cờ 0 và sự kiện còn hiệu lực (Java: SendDropTail).
    private void SendDropTail(long now, IPlayerRuntimeServices services)
    {
        if (Gender != 2 || CFlag != 0 || !IsEventGasActive || !CanDoWithTime(now, LastTimeDropTail, TIME_TRUNG_THU))
        {
            return;
        }

        if (!EffectSkill.IsMonkey)
        {
            services.ActivateMoonFestivalMonkey(this);
        }

        IsEventGasActive = false;
    }

    // Trừ HP, chat ngẫu nhiên và gỡ giữ Mabư 14 giờ qua cổng runtime (Java: SendMabu14H).
    private void SendMabuFourteenHour(PlayerRuntimeContext context)
    {
        IPlayerRuntimeServices services = context.Services;
        if (IsPl() && EffectSkill.IsMabuHold)
        {
            NPoint.SubHP(NPoint.HpMax / 500);
            if (Roll(context.Random, 1, 10))
            {
                services.SendMabuHelpChat(this);
            }

            services.SendInfoHp(this);
            if (PrecentMabuHold > 15)
            {
                services.RemoveMabuHold(this);
            }

            if (NPoint.Hp <= 0)
            {
                services.RemoveMabuHold(this);
                services.SetDie(this);
            }
        }

        if (Zone is { } zone)
        {
            services.UpdateMabuFourteenHourProtection(this, services.IsMabuFourteenHourMap(zone.Map.MapId));
        }
    }

    // Áp dụng/gỡ Chibi và hồi HP/KI một phần mười mỗi hơn một giây theo dải sức mạnh Java (Java: SendPetChiBi).
    private void SendPetChibi(PlayerRuntimeContext context)
    {
        if (Zone is not { } zone)
        {
            return;
        }

        IPlayerRuntimeServices services = context.Services;
        int mapId = zone.Map.MapId;
        bool forbiddenMap = services.IsWarMap(mapId) || services.IsBlackBallWarMap(mapId) || services.IsOfflineMap(mapId);
        if (IsPlAndBot() && EffectSkill.IsChibi && forbiddenMap)
        {
            services.RemoveChibi(this);
        }

        if (IsPlAndBot() && !IsDead() && !EffectSkill.IsChibi && CanDoWithTime(context.Clock.NowMilliseconds, LastTimeChibi, 1_500_000))
        {
            int denominator = NPoint.Power switch
            {
                <= 1_500_000 => IsPlayer ? 100 : 2_000,
                <= 150_000_000 => IsPlayer ? 140 : 4_000,
                <= 10_000_000_000L => IsPlayer ? 160 : 5_000,
                <= 40_000_000_000L => IsPlayer ? 180 : 8_000,
                _ => IsPlayer ? 200 : 10_000,
            };

            if (Roll(context.Random, 1, denominator) && !forbiddenMap)
            {
                services.SetChibi(this, 600_000);
            }
        }

        long now = context.Clock.NowMilliseconds;
        if (!IsPlAndBot() || IsDead() || !EffectSkill.IsChibi || !CanDoWithTime(now, LastTimeUpdateChibi, 1_000))
        {
            return;
        }

        if (TypeChibi == 1)
        {
            if (NPoint.Mp < NPoint.MpMax)
            {
                NPoint.Mp = NPoint.MpMax - NPoint.Mp < NPoint.MpMax / 10 ? NPoint.MpMax : NPoint.Mp + NPoint.MpMax / 10;
            }

            // Java Player.java 1349–1357 gửi gói KI cả khi KI đã đầy.
            services.SendInfoMp(this);
        }
        else if (TypeChibi == 3)
        {
            if (NPoint.Hp < NPoint.HpMax)
            {
                NPoint.Hp = NPoint.HpMax - NPoint.Hp < NPoint.HpMax / 10 ? NPoint.HpMax : NPoint.Hp + NPoint.HpMax / 10;
            }

            // Java Player.java 1358–1366 gửi gói HP cả khi HP đã đầy.
            services.SendInfoHp(this);
        }

        LastTimeUpdateChibi = now;
    }

    // Gỡ hiệu ứng cải trang khi người chơi không còn mặc costume tương ứng (Java: UpdateCaiTrangBienHinh).
    private void UpdateTransformationCostume(IPlayerRuntimeServices services) => services.UpdateTransformationCostume(this);

    // Chuyển map 144 của Con Đường Rắn Độc sau hơn năm giây khi dungeon hoàn tất (Java: UpdatePhoBan).
    private void UpdateDungeon(long now, IPlayerRandom random, IPlayerRuntimeServices services)
    {
        if (IsPl() && JoinCDRD && HasTalkedToKarin && Zone is { } zone && zone.Map.MapId == 47 && services.IsSnakeWayCompleted(this)
            && CanDoWithTime(now, TimeChangeMap144, 5_000))
        {
            services.MoveToSnakeWayExit(this, 300 + random.NextInt(-100, 100), 312);
            TimeChangeMap144 = now;
        }
    }

    // Gửi chat Khụ khụ tại khoảng ngẫu nhiên Java (Java: UpdateChat).
    private void UpdateChat(long now, IPlayerRandom random, IPlayerRuntimeServices services)
    {
        if (IsPl() && CanDoWithTime(now, TimeChatKhukhu, random.NextInt(15_000, 25_000)))
        {
            services.SendCoughChat(this);
            TimeChatKhukhu = now;
        }
    }

    // Hiệu ứng Whis cho ba hạng đầu, nhưng Java còn đòi list > 2 rồi list >= 5 (Java: UpdateEffectKillWhis 1399–1418).
    private void UpdateWhisKillEffect(IPlayerRuntimeServices services)
    {
        int whisParticipantCount = services.GetWhisParticipantCount();
        if (!IsPl() || whisParticipantCount <= 2)
        {
            return;
        }

        if (whisParticipantCount < 5)
        {
            return;
        }

        int effectId = services.GetWhisRank(this) switch
        {
            1 => 58,
            2 => 57,
            3 => 56,
            _ => -1,
        };
        if (effectId != -1)
        {
            services.AddWhisEffect(this, effectId);
        }
    }

    // Không port body: tất cả lời gọi RadarService trong Java đều bị comment (Java: UpdateSkillBienHinhAndAura).
    private static void UpdateSkillTransformationAndAura()
    {
    }

    // Chạy phần cuối update đúng thứ tự Java sau nhánh player-only (Java: SendPetChiBi ... PetFollow).
    private void UpdateRemainingEvents(long now, PlayerRuntimeContext context)
    {
        IPlayerRuntimeServices services = context.Services;
        SendPetChibi(context);
        services.UpdateSuperDivineWater(this);
        services.UpdateMabuTwelveHour(this);
        services.UpdatePastEvent(this);
        UpdateChat(now, context.Random, services);
        UpdateDungeon(now, context.Random, services);
        SendMabuFourteenHour(context);
        // Java gọi hai lần (1073, 1078); giữ nguyên, chờ chủ dự án quyết định có bỏ trùng không.
        services.UpdateMabuTwelveHour(this);
        UpdateWhisKillEffect(services);
        SendDropTail(now, services);
        services.UpdateTravelMarkers(this, now, context.Random);
        if (Zone is not null)
        {
            services.ApplyTrap(this);
        }

        services.SynchronizePetFollow(this);
    }
}
