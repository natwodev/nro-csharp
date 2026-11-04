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
    public long CalculatePowerPotential(long tiemNang)
    {
        NPointFacts facts = Host.Capture(this);
        return CalculatePowerPotential(tiemNang, facts);
    }

    private long CalculatePowerPotential(long tiemNang, NPointFacts facts)
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
            long originalTiemNang = tiemNang;

            // Phần trăm cộng từ tùy chọn vật phẩm.
            foreach (int tl in PowerPotentialPercents)
            {
                tiemNang += CalculatePercent(tiemNang, tl);
            }

            // flag
            if (p.CFlag != 0)
            {
                int percent = p.CFlag == 8 ? 10 : 5;
                tiemNang += CalculatePercent(tiemNang, percent);
            }

            // server attribute
            if (p.IsPl)
            {
                if (server.AttributeTnsmActive)
                {
                    tiemNang += CalculatePercent(tiemNang, server.AttributeTnsmValue);
                }
            }

            // week / month / year / supreme cards
            if (p.THE_TUAN != 0 && p.LASTTIME_THE_TUAN > now)
            {
                tiemNang += CalculatePercent(tiemNang, p.THE_TUAN == 1 ? 20 : 50);
            }

            if (p.THE_THANG != 0 && p.LASTTIME_THE_THANG > now)
            {
                tiemNang += CalculatePercent(tiemNang, p.THE_THANG == 1 ? 100 : 150);
            }

            if (p.THE_NAM != 0 && p.LASTTIME_THE_NAM > now)
            {
                tiemNang += CalculatePercent(tiemNang, p.THE_NAM == 1 ? 200 : 300);
            }

            if (p.THE_CHI_TON != 0 && p.LASTTIME_THE_CHI_TON > now)
            {
                tiemNang += CalculatePercent(tiemNang, 500);
            }

            // mask
            if (it.IsFaceMask)
            {
                tiemNang += CalculatePercent(tiemNang, 20);
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

            tiemNang += originalTiemNang * charmBonusMultiplier;

            // Nội tại của lần tấn công trước, xem SetOptions.
            if (intrinsic is not null && intrinsic.Id == 24)
            {
                tiemNang += CalculatePercent(tiemNang, intrinsic.Param1);
            }

            // chibi
            if (facts.EffectSkill.IsChibi && p.TypeChibi == 2)
            {
                tiemNang += originalTiemNang * 2;
            }

            // clan buff
            NPointClanFacts? clan = facts.Clan;
            if (clan is not null)
            {
                if (now <= clan.LasttimeBuffExp + clan.TimeStarBuffExp)
                {
                    tiemNang += originalTiemNang * clan.AddPotentialSucManhToanBangHoi;
                }

                if (now <= clan.BuaTriTue)
                {
                    int bonusPercent = Math.Min(clan.Level * 20, 200);
                    tiemNang += CalculatePercent(originalTiemNang, bonusPercent);
                }
            }

            // satellite
            if (p.SatelliteIsIntelligent)
            {
                tiemNang += CalculatePercent(originalTiemNang, 20);
            }

            // VIP
            if (p.HasVipPoint)
            {
                tiemNang += originalTiemNang * 2;
            }

            // item time
            if (it.IsBoneDragon3)
            {
                tiemNang += originalTiemNang * 3;
            }

            if (it.IsUseMonkeyTailPowerPotential)
            {
                tiemNang += originalTiemNang * 10;
            }

            // Cộng 20% kinh nghiệm khi đánh quái; Java đọc player.nPoint, chính là đối tượng này.
            if (Grants20ExperienceOnMobAttack)
            {
                tiemNang += CalculatePercent(tiemNang, 20);
            }

            // goten set
            if (facts.SetClothes.Goten == 5)
            {
                tiemNang += originalTiemNang * 5;
            }

            // Đệ tử nhận các thưởng của chủ.
            if (p.IsDeTu)
            {
                NPointMasterFacts? master = facts.Master;
                if (master is not null)
                {
                    if (master.ItemTimeIsUsePetPowerPotentialCharm)
                    {
                        tiemNang += originalTiemNang * 2;
                    }

                    if (master.ItemTimeIsUsePetPowerPotentialCharm)
                    {
                        tiemNang += originalTiemNang * 10;
                    }

                    long masterNow = now;
                    NPointCharmsFacts mc = master.Charms;
                    if (mc.TdDeTu > masterNow)
                    {
                        tiemNang += originalTiemNang * 2;
                    }

                    if (mc.TdDeTu2 > masterNow)
                    {
                        tiemNang += originalTiemNang * 3;
                    }

                    if (mc.TdDeTu3 > masterNow)
                    {
                        tiemNang += originalTiemNang * 4;
                    }

                    if (mc.TdDeTu4 > masterNow)
                    {
                        tiemNang += originalTiemNang * 5;
                    }

                    if (mc.TdDeTu5 > masterNow)
                    {
                        tiemNang += originalTiemNang * 6;
                    }

                    if (mc.TdDeTu7 > masterNow)
                    {
                        tiemNang += originalTiemNang * 8;
                    }

                    if (mc.TdDeTu10 > masterNow)
                    {
                        tiemNang += originalTiemNang * 10;
                    }

                    if (mc.TdDeTu20 > masterNow)
                    {
                        tiemNang += originalTiemNang * 20;
                    }

                    if (master.ItemTimeIsBoneDragon)
                    {
                        tiemNang += originalTiemNang * 3;
                    }

                    if (master.THE_TUAN != 0 && master.LASTTIME_THE_TUAN > masterNow)
                    {
                        tiemNang += CalculatePercent(tiemNang, master.THE_TUAN == 1 ? 20 : 50);
                    }

                    if (master.THE_THANG != 0 && master.LASTTIME_THE_THANG > masterNow)
                    {
                        tiemNang += CalculatePercent(tiemNang, master.THE_THANG == 1 ? 100 : 150);
                    }

                    if (master.THE_NAM != 0 && master.LASTTIME_THE_NAM > masterNow)
                    {
                        tiemNang += CalculatePercent(tiemNang, master.THE_NAM == 1 ? 200 : 300);
                    }

                    if (master.THE_CHI_TON != 0 && master.LASTTIME_THE_CHI_TON > masterNow)
                    {
                        tiemNang += CalculatePercent(tiemNang, 500);
                    }

                    // originalTiemNang / 100 first (cut), then times (tlTNSMPet + 100)
                    tiemNang += originalTiemNang / 100 * (master.PetPowerPotentialPercent + 100);

                    if (zone.IsMapKVTH)
                    {
                        tiemNang *= 2;
                    }
                }
            }

            // father / mother
            if (p.IsBo)
            {
                tiemNang += originalTiemNang * 2;
            }

            if (p.IsMe)
            {
                tiemNang += originalTiemNang * 2;
            }

            // special maps
            if (zone.IsMapBanDoKhoBau)
            {
                tiemNang *= 6;
            }

            if (zone.IsMapDoanhTrai)
            {
                tiemNang *= 3;
            }

            if (zone.IsTnsmDividedMap)
            {
                tiemNang = tiemNang / 10;
            }

            if (p.IsDeTu)
            {
                int type = p.DetuTypeDeTu;
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

                tiemNang = JavaNumbers.ToLong(tiemNang / factor);
            }

            // server rate
            tiemNang *= server.RateExpServer;

            // Giảm theo mốc sức mạnh và giới hạn trên.
            tiemNang = CalculatePowerPotentialReduction(tiemNang);

            if (tiemNang <= 0)
            {
                tiemNang = 1;
            }

            return tiemNang;
        }
    }

    // Giảm tiềm năng theo sức mạnh: 0 ở giới hạn; từ 100/50/40 tỷ còn 1%/20%/50%, tối đa 20 triệu.
    public long CalculatePowerPotentialReduction(long tiemNang)
    {
        if (Power >= GetPowerLimit())
        {
            return 0;
        }

        if (Power >= 120_000_000_000L)
        {
            tiemNang = CalculatePercent(tiemNang, 1);
        }
        else if (Power >= 100_000_000_000L)
        {
            tiemNang = CalculatePercent(tiemNang, 1);
        }
        else if (Power >= 50_000_000_000L)
        {
            tiemNang = CalculatePercent(tiemNang, 20);
        }
        else if (Power >= 40_000_000_000L)
        {
            tiemNang = CalculatePercent(tiemNang, 50);
        }

        // cap at 20 million
        if (tiemNang > 20_000_000L)
        {
            tiemNang = 20_000_000L;
        }

        return tiemNang;
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
    public void IncreasePotential(long tiemNang)
    {
        Potential = unchecked(Potential + tiemNang);
    }

    // Dùng tiềm năng tăng chỉ số gốc: 0 HP, 1 KI, 2 sát thương, 3 phòng thủ, 4 chí mạng. Giữ kiểu số Java: giá phòng thủ int có thể tràn, giá chí mạng nhân 5 theo từng điểm và có thể tràn long. Chạm trần chỉ gửi thông báo/gói tiền, không trừ; không làm gì khi bảo vệ tài khoản bật, chưa nạp ngưỡng hoặc point không dương.
    public void IncreasePoint(sbyte type, short point, bool manualForPet)
    {
        unchecked
        {
            if (Player.Baovetaikhoan)
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
            long tiemNangUse;
            if (type == 0)
            {
                int pointHp = point * 20;
                tiemNangUse = point * ((2 * (BaseHp + 1000)) + pointHp - 20) / 2;
                if ((BaseHp + pointHp) <= PowerLimit.Hp)
                {
                    if (DoUseTiemNang(tiemNangUse))
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
                tiemNangUse = point * ((2 * (BaseMp + 1000)) + pointMp - 20) / 2;
                if ((BaseMp + pointMp) <= PowerLimit.Mp)
                {
                    if (DoUseTiemNang(tiemNangUse))
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
                tiemNangUse = point * ((2 * BaseDamage) + point - 1) / 2 * 100;
                if ((BaseDamage + point) <= PowerLimit.Damage)
                {
                    if (DoUseTiemNang(tiemNangUse))
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
                tiemNangUse = 2 * (BaseDefense + 5) / 2 * 100000;
                if ((BaseDefense + point) <= PowerLimit.Defense)
                {
                    if (DoUseTiemNang(tiemNangUse))
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
                tiemNangUse = 50000000L;
                for (int i = 0; i < BaseCriticalChance; i++)
                {
                    tiemNangUse *= 5L;
                }

                if ((BaseCriticalChance + point) <= PowerLimit.Critical)
                {
                    if (DoUseTiemNang(tiemNangUse))
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
                if (Player.Detu is not null)
                {
                    Host.SendBasePetInfo(Player);
                    Host.ShowInfoPet(Player);
                    Host.SendPoint(Player);
                }
            }
        }
    }

    private bool DoUseTiemNang(long tiemNang)
    {
        if (Potential < tiemNang)
        {
            Host.SendNotificationOk(Player, "Bạn không đủ tiềm năng");
            return false;
        }

        if (Potential >= tiemNang && unchecked(Potential - tiemNang) >= 0)
        {
            Potential = unchecked(Potential - tiemNang);
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
            int tiLeHoiPhuc = NPointSkillMath.GetPercentCharge(uf.SkillSelectPoint);
            if (uf.IsCharging && Hp > 0 && !uf.IsHaveEffectSkill && (Hp < HpMax || Mp < MpMax))
            {
                long hpRecovered = unchecked(HpMax / 100 * tiLeHoiPhuc);
                long mpRecovered = unchecked(MpMax / 100 * tiLeHoiPhuc);

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

        if (CanDoWithTime(now, lastTimeHoiPhuc, 30000))
        {
            Host.RestoreHpMp(Player, Util.CrisGH(HpRegen, uf.ReadInt), Util.CrisGH(MpRegen, uf.ReadInt));
            lastTimeHoiPhuc = now;
        }

        if (CanDoWithTime(now, lastTimeHoiPhuc10s, 10000))
        {
            Host.RestoreHpMp(Player, SelfAndAllyHpRegenPercent, 0);
            lastTimeHoiPhuc10s = now;
        }

        if (CanDoWithTime(now, lastTimeHoiStamina, 60000) && Stamina < MaxStamina)
        {
            Stamina++;
            lastTimeHoiStamina = now;

            if (!Player.IsBoss && !Player.IsDeTu && !Player.IsBo && !Player.IsMe && !Player.IsNguoiYeu && !Player.IsConOne && !Player.IsConTwo && !Player.IsConThree)
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
