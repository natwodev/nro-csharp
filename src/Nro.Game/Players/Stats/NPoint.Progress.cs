using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Players.Stats;

// Nhóm 4 của NPoint: sức mạnh/tiềm năng, ngưỡng sức mạnh, tiêu tiềm năng tăng chỉ số cơ sở và cập nhật mỗi tick.
// Nguồn Java: nro/player/NPoint.java (giữ công thức, chia nguyên, tràn số và lịch timer của Java).
public partial class NPoint
{
    // Tính tiềm năng/sức mạnh mỗi lần hạ quái: trả 0 khi không có zone hoặc map loại 3, 10 khi chạm giới hạn, còn lại cộng thưởng, chia ở map đặc biệt và theo hệ số đệ tử, nhân hệ số server, trừ CalculatePowerPotentialReduction rồi tối thiểu 1. Giữ đặc tính Java: bùa TNSM đệ tử được kiểm hai lần (+2x và +10x lượng gốc), hệ số đệ tử là phép chia double rồi cắt long.
    public long CalculatePowerPotential(long potential)
    {
        NPointFacts facts = Host.Capture(this);
        return CalculatePowerPotential(potential, facts);
    }

    private long CalculatePowerPotential(long potential, NPointFacts facts)
    {
        unchecked
        {
            NPointZoneFacts? zone = facts.Zone;
            if (zone is null)
            {
                return 0;
            }

            if (zone.MapType == 3)
            {
                return 0;
            }

            if (Power >= GetPowerLimit())
            {
                return 10;
            }

            NPointPlayerFacts p = facts.Player;
            NPointItemTimeFacts it = facts.ItemTime;
            NPointServerFacts server = facts.Server;
            long now = Now();
            // Tiềm năng ban đầu (Java: tiemNang).
            long originalPotential = potential;

            // Phần trăm cộng từ tùy chọn vật phẩm.
            foreach (int tl in PowerPotentialPercents)
            {
                potential += CalculatePercent(potential, tl);
            }

            // flag
            if (p.CFlag != 0)
            {
                int percent = p.CFlag == 8 ? 10 : 5;
                potential += CalculatePercent(potential, percent);
            }

            // server attribute
            if (p.IsPl)
            {
                if (server.AttributeTnsmActive)
                {
                    potential += CalculatePercent(potential, server.AttributeTnsmValue);
                }
            }

            // week / month / year / supreme cards
            if (p.WeeklyCard != 0 && p.LastWeeklyCardTime > now)
            {
                potential += CalculatePercent(potential, p.WeeklyCard == 1 ? 20 : 50);
            }

            if (p.MonthlyCard != 0 && p.LastMonthlyCardTime > now)
            {
                potential += CalculatePercent(potential, p.MonthlyCard == 1 ? 100 : 150);
            }

            if (p.YearlyCard != 0 && p.LastYearlyCardTime > now)
            {
                potential += CalculatePercent(potential, p.YearlyCard == 1 ? 200 : 300);
            }

            if (p.SupremeCard != 0 && p.LastSupremeCardTime > now)
            {
                potential += CalculatePercent(potential, 500);
            }

            // mask
            if (it.IsFaceMask)
            {
                potential += CalculatePercent(potential, 20);
            }

            // wisdom charm
            long charmBonusMultiplier = 0;
            NPointCharmsFacts charms = facts.Charms;
            if (charms.TdTriTue > now)
            {
                charmBonusMultiplier += 1;
            }

            if (charms.TdTriTue3 > now)
            {
                charmBonusMultiplier += 2;
            }

            if (charms.TdTriTue4 > now)
            {
                charmBonusMultiplier += 3;
            }

            if (charms.TdTriTue5 > now)
            {
                charmBonusMultiplier += 4;
            }

            if (charms.TdTriTue7 > now)
            {
                charmBonusMultiplier += 6;
            }

            if (charms.TdTriTue10 > now)
            {
                charmBonusMultiplier += 9;
            }

            if (charms.TdTriTue20 > now)
            {
                charmBonusMultiplier += 19;
            }

            potential += originalPotential * charmBonusMultiplier;

            // Nội tại của lần tấn công trước, xem SetOptions.
            if (intrinsic is not null && intrinsic.Id == 24)
            {
                potential += CalculatePercent(potential, intrinsic.Param1);
            }

            // chibi
            if (facts.EffectSkill.IsChibi && p.TypeChibi == 2)
            {
                potential += originalPotential * 2;
            }

            // clan buff
            NPointClanFacts? clan = facts.Clan;
            if (clan is not null)
            {
                if (now <= clan.LasttimeBuffExp + clan.TimeStarBuffExp)
                {
                    potential += originalPotential * clan.ClanPowerPotentialBonus;
                }

                if (now <= clan.WisdomCharmExpiry)
                {
                    int bonusPercent = Math.Min(clan.Level * 20, 200);
                    potential += CalculatePercent(originalPotential, bonusPercent);
                }
            }

            // satellite
            if (p.SatelliteIsIntelligent)
            {
                potential += CalculatePercent(originalPotential, 20);
            }

            // VIP
            if (p.HasVipPoint)
            {
                potential += originalPotential * 2;
            }

            // item time
            if (it.IsBoneDragon3)
            {
                potential += originalPotential * 3;
            }

            if (it.IsUseMonkeyTailPowerPotential)
            {
                potential += originalPotential * 10;
            }

            // Cộng 20% kinh nghiệm khi đánh quái; Java đọc player.nPoint, chính là đối tượng này.
            if (Grants20ExperienceOnMobAttack)
            {
                potential += CalculatePercent(potential, 20);
            }

            // goten set
            if (facts.SetClothes.Goten == 5)
            {
                potential += originalPotential * 5;
            }

            // Đệ tử nhận các thưởng của chủ.
            if (p.IsPet)
            {
                NPointMasterFacts? master = facts.Master;
                if (master is not null)
                {
                    if (master.ItemTimeIsUsePetPowerPotentialCharm)
                    {
                        potential += originalPotential * 2;
                    }

                    if (master.ItemTimeIsUsePetPowerPotentialCharm)
                    {
                        potential += originalPotential * 10;
                    }

                    long masterNow = now;
                    NPointCharmsFacts mc = master.Charms;
                    if (mc.PetTraining > masterNow)
                    {
                        potential += originalPotential * 2;
                    }

                    if (mc.PetTraining2 > masterNow)
                    {
                        potential += originalPotential * 3;
                    }

                    if (mc.PetTraining3 > masterNow)
                    {
                        potential += originalPotential * 4;
                    }

                    if (mc.PetTraining4 > masterNow)
                    {
                        potential += originalPotential * 5;
                    }

                    if (mc.PetTraining5 > masterNow)
                    {
                        potential += originalPotential * 6;
                    }

                    if (mc.PetTraining7 > masterNow)
                    {
                        potential += originalPotential * 8;
                    }

                    if (mc.PetTraining10 > masterNow)
                    {
                        potential += originalPotential * 10;
                    }

                    if (mc.PetTraining20 > masterNow)
                    {
                        potential += originalPotential * 20;
                    }

                    if (master.ItemTimeIsBoneDragon)
                    {
                        potential += originalPotential * 3;
                    }

                    if (master.WeeklyCard != 0 && master.LastWeeklyCardTime > masterNow)
                    {
                        potential += CalculatePercent(potential, master.WeeklyCard == 1 ? 20 : 50);
                    }

                    if (master.MonthlyCard != 0 && master.LastMonthlyCardTime > masterNow)
                    {
                        potential += CalculatePercent(potential, master.MonthlyCard == 1 ? 100 : 150);
                    }

                    if (master.YearlyCard != 0 && master.LastYearlyCardTime > masterNow)
                    {
                        potential += CalculatePercent(potential, master.YearlyCard == 1 ? 200 : 300);
                    }

                    if (master.SupremeCard != 0 && master.LastSupremeCardTime > masterNow)
                    {
                        potential += CalculatePercent(potential, 500);
                    }

                    // originalPotential / 100 first (cut), then times (tlTNSMPet + 100)
                    potential += originalPotential / 100 * (master.PetPowerPotentialPercent + 100);

                    if (zone.IsMapKVTH)
                    {
                        potential *= 2;
                    }
                }
            }

            // father / mother
            if (p.IsFather)
            {
                potential += originalPotential * 2;
            }

            if (p.IsMother)
            {
                potential += originalPotential * 2;
            }

            // special maps
            if (zone.IsTreasureUnderSeaMap)
            {
                potential *= 6;
            }

            if (zone.IsBarracksMap)
            {
                potential *= 3;
            }

            if (zone.IsPowerPotentialDividedMap)
            {
                potential = potential / 10;
            }

            if (p.IsPet)
            {
                int type = p.PetType;
                double factor = 1.0; // default: normal pet (type 0) = 1x

                switch (type)
                {
                    case 0:
                        factor = 1.0;
                        break;
                    case 1: // Ma Bu
                        factor = 1.0;
                        break;
                    case 2:
                    case 3:
                    case 4: // Son Tinh, Thuy Tinh
                        factor = 2.0;
                        break;
                    case 5: // Bu Nhi, Xen Nhi, Fide Dai Ka
                        factor = 4.0;
                        break;
                }

                potential = JavaNumbers.ToLong(potential / factor);
            }

            // server rate
            potential *= server.RateExpServer;

            // Giảm theo mốc sức mạnh và giới hạn trên.
            potential = CalculatePowerPotentialReduction(potential);

            if (potential <= 0)
            {
                potential = 1;
            }

            return potential;
        }
    }

    // Giảm tiềm năng theo sức mạnh: 0 ở giới hạn; từ 100/50/40 tỷ còn 1%/20%/50%, tối đa 20 triệu.
    public long CalculatePowerPotentialReduction(long potential)
    {
        if (Power >= GetPowerLimit())
        {
            return 0;
        }

        if (Power >= 120_000_000_000L)
        {
            potential = CalculatePercent(potential, 1);
        }
        else if (Power >= 100_000_000_000L)
        {
            potential = CalculatePercent(potential, 1);
        }
        else if (Power >= 50_000_000_000L)
        {
            potential = CalculatePercent(potential, 20);
        }
        else if (Power >= 40_000_000_000L)
        {
            potential = CalculatePercent(potential, 50);
        }

        // cap at 20 million
        if (potential > 20_000_000L)
        {
            potential = 20_000_000L;
        }

        return potential;
    }

    // Cho biết sức mạnh đã đạt giới hạn của cấp hiện tại.
    public bool CanOpenPower() => Power >= GetPowerLimit();

    // Lấy trần sức mạnh cấp hiện tại; trả 0 nếu ngưỡng chưa được nạp.
    public long GetPowerLimit()
    {
        if (PowerLimit is not null)
        {
            return PowerLimit.Power;
        }

        return 0;
    }

    // Lấy trần sức mạnh cấp kế; trả 0 nếu không có.
    public long GetPowerNextLimit()
    {
        PowerLimitInfo? next = Host.GetPowerLimit(LimitPower + 1);
        if (next is not null)
        {
            return next.Power;
        }

        return 0;
    }

    // Cộng sức mạnh.
    public void PowerUp(long power)
    {
        Power = unchecked(Power + power);
        Host.CheckDoneTaskPower(Player, Power);
    }

    // Cộng tiềm năng.
    public void IncreasePotential(long potential)
    {
        Potential = unchecked(Potential + potential);
    }

    // Dùng tiềm năng tăng chỉ số gốc: 0 HP, 1 KI, 2 sát thương, 3 phòng thủ, 4 chí mạng. Giữ kiểu số Java: giá phòng thủ int có thể tràn, giá chí mạng nhân 5 theo từng điểm và có thể tràn long. Chạm trần chỉ gửi thông báo/gói tiền, không trừ; không làm gì khi bảo vệ tài khoản bật, chưa nạp ngưỡng hoặc point không dương.
    public void IncreasePoint(sbyte type, short point, bool manualForPet)
    {
        unchecked
        {
            if (Player.IsAccountProtected)
            {
                Host.SendNotification(Player, "Chức năng bảo vệ đã được bật. Bạn vui lòng kiểm tra lại");
                return;
            }

            if (PowerLimit is null)
            {
                return;
            }

            if (point <= 0)
            {
                return;
            }

            bool updatePoint = false;
            // Chi phí tiềm năng (Java: tiemNangUse).
            long potentialCost;
            if (type == 0)
            {
                int pointHp = point * 20;
                potentialCost = point * ((2 * (BaseHp + 1000)) + pointHp - 20) / 2;
                if ((BaseHp + pointHp) <= PowerLimit.Hp)
                {
                    if (TrySpendPotential(potentialCost))
                    {
                        BaseHp += pointHp;
                        updatePoint = true;
                    }
                }
                else
                {
                    Host.SendNotification(Player, "HP của bạn đã đạt mức tối đa");
                    Host.SendMoney(Player);
                    return;
                }
            }

            if (type == 1)
            {
                int pointMp = point * 20;
                potentialCost = point * ((2 * (BaseMp + 1000)) + pointMp - 20) / 2;
                if ((BaseMp + pointMp) <= PowerLimit.Mp)
                {
                    if (TrySpendPotential(potentialCost))
                    {
                        BaseMp += pointMp;
                        updatePoint = true;
                    }
                }
                else
                {
                    Host.SendNotification(Player, "KI của bạn đã đạt mức tối đa");
                    Host.SendMoney(Player);
                    return;
                }
            }

            if (type == 2)
            {
                potentialCost = point * ((2 * BaseDamage) + point - 1) / 2 * 100;
                if ((BaseDamage + point) <= PowerLimit.Damage)
                {
                    if (TrySpendPotential(potentialCost))
                    {
                        BaseDamage += point;
                        updatePoint = true;
                    }
                }
                else
                {
                    Host.SendNotification(Player, "Sức đánh của bạn đã đạt mức tối đa");
                    Host.SendMoney(Player);
                    return;
                }
            }

            if (type == 3)
            {
                // Số học int Java: 2*(BaseDefense+5)/2*100000 có thể tràn trước khi mở rộng sang long.
                potentialCost = 2 * (BaseDefense + 5) / 2 * 100000;
                if ((BaseDefense + point) <= PowerLimit.Defense)
                {
                    if (TrySpendPotential(potentialCost))
                    {
                        BaseDefense += point;
                        updatePoint = true;
                    }
                }
                else
                {
                    Host.SendNotification(Player, "Giáp của bạn đã đạt mức tối đa");
                    Host.SendMoney(Player);
                    return;
                }
            }

            if (type == 4)
            {
                potentialCost = 50000000L;
                for (int i = 0; i < BaseCriticalChance; i++)
                {
                    potentialCost *= 5L;
                }

                if ((BaseCriticalChance + point) <= PowerLimit.Critical)
                {
                    if (TrySpendPotential(potentialCost))
                    {
                        BaseCriticalChance += point;
                        updatePoint = true;
                    }
                }
                else
                {
                    Host.SendNotification(Player, "Chí mạng của bạn đã đạt mức tối đa");
                    Host.SendMoney(Player);
                    return;
                }
            }

            if (updatePoint)
            {
                Host.SendPoint(Player);
            }

            if (manualForPet)
            {
                if (Player.Pet is not null)
                {
                    Host.SendBasePetInfo(Player);
                    Host.ShowInfoPet(Player);
                    Host.SendPoint(Player);
                }
            }
        }
    }

    // Thử tiêu tiềm năng (Java: subTiemNang).
    private bool TrySpendPotential(long potential)
    {
        if (Potential < potential)
        {
            Host.SendNotificationOk(Player, "Bạn không đủ tiềm năng");
            return false;
        }

        if (Potential >= potential && unchecked(Potential - potential) >= 0)
        {
            Potential = unchecked(Potential - potential);
            Host.CheckPotentialUseTaskCompletion(Player);
            return true;
        }

        return false;
    }

    // Cập nhật mỗi tick zone: kỹ năng nạp hồi phần trăm tối đa 10 tick, hồi HP/KI mỗi 30 giây, tự hồi mỗi 10 giây và một thể lực mỗi 60 giây. Timer dùng now-last>interval nên tick đầu luôn đến hạn vì khởi tạo 0. Khác biệt có chủ đích: đang nạp mà chưa chọn kỹ năng thì xóa nạp thay vì ném lỗi, cùng guard với java-run patch sửa boss bị kẹt.
    public void Update()
    {
        NPointUpdateFacts uf = Host.CaptureUpdate(this);
        if (!uf.HasEffectSkill)
        {
            return;
        }

        long now = Now();
        if (uf.IsCharging && !uf.HasSkillSelect)
        {
            Host.ClearCharging(Player);
            uf.IsCharging = false;
            uf.CountCharging = 0;
        }

        if (uf.IsCharging && uf.CountCharging < 10)
        {
            // Tỷ lệ hồi phục khi nạp kỹ năng (Java: tiLeHoiPhuc).
            int regenPercent = NPointSkillMath.GetPercentCharge(uf.SkillSelectPoint);
            if (uf.IsCharging && Hp > 0 && !uf.IsHaveEffectSkill && (Hp < HpMax || Mp < MpMax))
            {
                long hpRecovered = unchecked(HpMax / 100 * regenPercent);
                long mpRecovered = unchecked(MpMax / 100 * regenPercent);

                Host.RestoreHpMp(Player, Util.CrisGH(hpRecovered, uf.ReadInt), Util.CrisGH(mpRecovered, uf.ReadInt));

                if (uf.CountCharging % 3 == 0)
                {
                    Host.Chat(Player, "Phục hồi năng lượng " + GetCurrentHpPercent() + "%");
                }
            }
            else
            {
                Host.StopCharge(Player);
            }

            if (Host.IncrementCountCharging(Player) >= 10)
            {
                Host.StopCharge(Player);
            }
        }

        if (CanDoWithTime(now, lastRegenTime, 30000))
        {
            Host.RestoreHpMp(Player, Util.CrisGH(HpRegen, uf.ReadInt), Util.CrisGH(MpRegen, uf.ReadInt));
            lastRegenTime = now;
        }

        if (CanDoWithTime(now, lastRegen10sTime, 10000))
        {
            Host.RestoreHpMp(Player, SelfAndAllyHpRegenPercent, 0);
            lastRegen10sTime = now;
        }

        if (CanDoWithTime(now, lastStaminaRegenTime, 60000) && Stamina < MaxStamina)
        {
            Stamina++;
            lastStaminaRegenTime = now;

            if (!Player.IsBoss && !Player.IsPet && !Player.IsFather && !Player.IsMother && !Player.IsLover && !Player.IsChildOne && !Player.IsChildTwo && !Player.IsChildThree)
            {
                Host.SendCurrentStamina(Player);
            }
        }
    }

    // Kiểm tra canDoWithTime bằng now-lastTime>miniTimeTarget, giữ phép trừ unchecked.
    private static bool CanDoWithTime(long now, long lastTime, long miniTimeTarget) => unchecked(now - lastTime) > miniTimeTarget;

    // Tính tổng tiềm năng đã dùng cho HP, KI, sát thương, phòng thủ và chí mạng; giữ chia nguyên BaseHp/20 và Math.Pow(5,BaseCriticalChance) kiểu double rồi cắt long.
    public long GetFullTN()
    {
        unchecked
        {
            long tnhp = 0, tnki = 0, tnsd = 0, tng = 0, tncm = 0;

            if (BaseHp > 0)
            {
                tnhp = (BaseHp / 20L) * (50L + (50L + (BaseHp / 20L) - 1L)) / 2L * 20L;
            }

            if (BaseMp > 0)
            {
                tnki = (BaseMp / 20L) * (50L + (50L + (BaseMp / 20L) - 1L)) / 2L * 20L;
            }

            if (BaseDamage > 0)
            {
                tnsd = BaseDamage * (BaseDamage - 1L) * 100L / 2L;
            }

            if (BaseDefense > 0)
            {
                tng = BaseDefense * (500000L + (500000L + ((BaseDefense - 1L) * 100000L))) / 2L;
            }

            if (BaseCriticalChance > 0)
            {
                tncm = 50L * (JavaNumbers.ToLong(Math.Pow(5L, BaseCriticalChance)) - 1L) / (5L - 1L) * 1000000L;
            }

            return tnhp + tnki + tnsd + tng + tncm;
        }
    }
}
