using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;

namespace Nro.Game.Players.Stats;

// Nhóm 3 của NPoint: sát thương, phòng thủ, chí mạng, hút, tùy chọn còn lại và sát thương một đòn.
// Nguồn Java: nro/player/NPoint.java (giữ thứ tự công thức, kiểu số và thứ tự rút ngẫu nhiên của Java).
public partial class NPoint
{
    // Tính sát thương theo đúng thứ tự và kiểu số Java: phần trăm vật phẩm từng bước, bộ đồ, thẻ, danh hiệu, buff, đệ tử hợp thể, bông tai, thuốc cuồng nộ (nhân 2 hoặc 2.2 rồi cắt long), map giảm, sao 1, khỉ, cuộn nhân và thưởng nhỏ.
    private void SetDamage(NPointFacts facts)
    {
        unchecked
        {
            long now = Now();
            NPointPlayerFacts p = facts.Player;
            NPointItemTimeFacts it = facts.ItemTime;
            NPointEffectSkillFacts es = facts.EffectSkill;
            NPointSetClothesFacts sc = facts.SetClothes;
            NPointZoneFacts? zone = facts.Zone;
            NPointServerFacts server = facts.Server;
            NPointDetuFacts? detu = facts.Detu;

            long dame = BaseDamage + DamageBonus;
            foreach (int tl in DamagePercents)
            {
                dame += dame * tl / 100L;
            }

            // Nguyet An: five pieces give +15% attack
            if (HasFull5NguyetAn())
            {
                dame += CalculatePercent(dame, 15);
            }

            if (p.IsPl)
            {
                if (facts.Inventory.HasRongNhi)
                {
                    dame += CalculatePercent(dame, 1);
                }

                dame += CalculatePercent(dame, facts.Inventory.DamageItemsInBox);
            }

            if (sc.Cumber == 5)
            {
                dame += CalculatePercent(dame, 20);
            }

            if (p.IsPhanThan)
            {
                dame = CalculatePercent(RequireMaster(facts).Damage, NPointSkillMath.GetClonePercent(facts.SkillSelect));
            }

            if (p.THE_TUAN == 1 && p.LASTTIME_THE_TUAN > now)
            {
                dame += CalculatePercent(dame, 3);
            }

            if (p.THE_TUAN == 2 && p.LASTTIME_THE_TUAN > now)
            {
                dame += CalculatePercent(dame, 5);
            }

            if (p.THE_THANG == 1 && p.LASTTIME_THE_THANG > now)
            {
                dame += CalculatePercent(dame, 7);
            }

            if (p.THE_THANG == 2 && p.LASTTIME_THE_THANG > now)
            {
                dame += CalculatePercent(dame, 10);
            }

            if (p.THE_NAM == 1 && p.LASTTIME_THE_NAM > now)
            {
                dame += CalculatePercent(dame, 15);
            }

            if (p.THE_NAM == 2 && p.LASTTIME_THE_NAM > now)
            {
                dame += CalculatePercent(dame, 18);
            }

            if (p.THE_CHI_TON == 1 && p.LASTTIME_THE_CHI_TON > now)
            {
                dame += CalculatePercent(dame, 20);
            }

            if (p.ClanDamageBonusPercent > 0)
            {
                dame += CalculatePercent(dame, p.ClanDamageBonusPercent);
            }

            if (server.InternationalWomansDay)
            {
                dame += CalculatePercent(dame, server.WomansDayDame);
            }

            if (es.IsVirus)
            {
                dame -= CalculatePercent(dame, 10);
            }

            dame += dame * SexyDamagePercent / 100;

            dame += dame * CoolDamagePercent / 100;

            dame += dame * CuteDamagePercent / 100;

            if (p.IsPl && p.IsUseDanhHieu_ThienTu && p.LastTimeDanhHieu_ThienTu > 0)
            {
                dame += CalculatePercent(dame, 5);
            }

            if (it.HasMonkeyTail)
            {
                dame += CalculatePercent(dame, 10);
            }

            if (p.Buff == Buff.BUFF_ATK)
            {
                dame += CalculatePercent(dame, 20);
            }

            if (it.IsUseRabbitMooncake1)
            {
                dame += CalculatePercent(dame, 5);
            }

            if (it.IsUseRabbitMooncake2)
            {
                dame += CalculatePercent(dame, 10);
            }

            if (it.IsUseRabbitMooncake3)
            {
                dame += CalculatePercent(dame, 15);
            }

            if (it.IsUseMoonFestivalOneEgg)
            {
                dame += CalculatePercent(dame, 10);
            }

            if (it.IsUseMoonFestivalTwoEgg)
            {
                dame += CalculatePercent(dame, 15);
            }

            if (it.IsUseSpecialMoonFestival)
            {
                dame += CalculatePercent(dame, 20);
            }

            if (it.IsUseMoonFestivalCakeBox)
            {
                dame += CalculatePercent(dame, 25);
            }

            if (it.IsUseRocket1h)
            {
                dame += CalculatePercent(dame, 20);
            }

            if (es.IsSnowflake)
            {
                dame -= CalculatePercent(dame, 20);
            }

            if (it.IsBoneDragon2)
            {
                dame += CalculatePercent(dame, 15);
            }

            if (facts.EffectSkin.IsRabbitLeader)
            {
                dame -= CalculatePercent(dame, 15);
            }

            // world cup set
            if (sc.Worldcup == 2)
            {
                dame += CalculatePercent(dame, 10);
            }

            // food. Java precedence: (!isDeTu && itemTime.isEatMeal) || (isDeTu && master.itemTime.isEatMeal)
            if ((!p.IsDeTu && it.IsEatMeal) || (p.IsDeTu && RequireMaster(facts).ItemTimeIsEatMeal))
            {
                dame += CalculatePercent(dame, 10);
            }

            if (detu is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                if (detu.TypeDeTu >= 5)
                {
                    dame += detu.Damage * facts.DameFusion / 100L;
                }
            }

            // fusion
            if (detu is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                if (detu.TypeDeTu == 1)
                {
                    dame += detu.Damage * 20 / 100L;
                }
            }

            if (detu is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                if (detu.TypeDeTu == 5)
                {
                    dame += detu.Damage * 40 / 100L;
                }
            }

            if (detu is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                dame += detu.Damage;
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_2)
            {
                dame += CalculatePercent(dame, 5);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_3)
            {
                dame += CalculatePercent(dame, 10);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_4)
            {
                dame += CalculatePercent(dame, 15);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_5)
            {
                dame += CalculatePercent(dame, 20);
            }

            // rage
            if (it.IsUseRage)
            {
                dame *= 2;
            }

            if (it.IsUseRedPeach)
            {
                dame += CalculatePercent(dame, 1);
            }

            // peach juices
            if (it.IsUseRedPeach0)
            {
                dame -= CalculatePercent(dame, 99);
            }

            if (it.IsUseRedPeach1)
            {
                dame += CalculatePercent(dame, 2);
            }

            if (it.IsUseRedPeach3)
            {
                dame += CalculatePercent(dame, 3);
            }

            if (it.IsUseRedPeach5)
            {
                dame += CalculatePercent(dame, 5);
            }

            if (it.IsUseRedPeach10)
            {
                dame += CalculatePercent(dame, 8);
            }

            if (it.IsUseRedPeach25)
            {
                dame += CalculatePercent(dame, 12);
            }

            if (it.IsUseRedPeach50)
            {
                dame += CalculatePercent(dame, 15);
            }

            if (it.IsUseRedPeach99)
            {
                dame += CalculatePercent(dame, 20);
            }

            if (it.IsUseRedPeach999)
            {
                dame += CalculatePercent(dame, 100);
            }

            if (it.IsUseRage2)
            {
                dame = JavaNumbers.ToLong(dame * 2.2);
            }

            if (it.HasDamageBoost30)
            {
                dame += CalculatePercent(dame, 30);
            }

            if (it.HasDamageBoost15)
            {
                dame += CalculatePercent(dame, 15);
            }

            if (it.IsUseCylindricalRiceCake)
            {
                dame += CalculatePercent(dame, 15);
            }

            if (it.IsUseSquareRiceCake)
            {
                dame += CalculatePercent(dame, 25);
            }

            // mabu map scroll
            if (p.IsPhuHoMapMabu)
            {
                dame += 10_000;
            }

            if (p.IsPlMan)
            {
                if (server.AttributeSucDanhActive)
                {
                    dame += CalculatePercent(dame, server.AttributeSucDanhValue);
                }
            }

            // Whis ranking
            IReadOnlyList<long> list = server.WhisPlayerIds;
            if (list.Count != 0 && list.Count > 2 && p.IsPl)
            {
                if (list.Count >= 5 && p.IsPl)
                {
                    int playerRank = GetPlayerRank(list, p.Id);
                    if (playerRank == 1)
                    {
                        dame += CalculatePercent(dame, 30);
                    }
                    else if (playerRank == 2)
                    {
                        dame += CalculatePercent(dame, 20);
                    }
                    else if (playerRank == 3)
                    {
                        dame += CalculatePercent(dame, 10);
                    }
                    else if (playerRank >= 4 && playerRank <= 5)
                    {
                        dame += CalculatePercent(dame, 5);
                    }
                    else if (playerRank >= 6 && playerRank <= 10)
                    {
                        dame += CalculatePercent(dame, 3);
                    }
                }
            }

            // Breakthrough power skill
            if (p.IsPl && facts.SkillSelect is not null)
            {
                int tiLeDameSucManhBocPha = NPointSkillMath.GetPowerBurstDamagePercent(facts.SkillSelect.Point);
                if (es.IsPowerBurst)
                {
                    dame += dame * tiLeDameSucManhBocPha / 100;
                }
            }

            // option "reduce attack"
            dame -= dame * DamageReductionPercent / 100;

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapCold && !IsColdImmune)
            {
                dame /= 2;
            }

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapChristMasEvent && !IsColdImmune)
            {
                dame /= 2;
            }

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMap5000NamTruoc)
            {
                dame -= CalculatePercent(dame, 90);
            }

            if (p.Gender == ConstPlayer.SAIYAN)
            {
                if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapCereal)
                {
                    dame /= 2;
                }
            }

            // black ball 1 star
            if (facts.BlackBallTimeOut[0] > now)
            {
                dame += dame * RewardBlackBall.R1S_2 / 100;
            }

            if (es.IsMonkey)
            {
                if (!p.IsDeTu || (p.IsDeTu && p.DetuStatus != NPointFacts.DetuFusionStatus))
                {
                    int percent = NPointSkillMath.GetMonkeyDamagePercent(es.LevelMonkey);
                    dame += dame * percent / 100;
                }
            }

            // scroll (black ball war)
            if (zone is not null && zone.IsMapBlackBallWar)
            {
                dame *= facts.EffectSkin.DamageMultiplier;
            }

            if (it.IsUseCaterpillarHamburger)
            {
                dame += CalculatePercent(dame, 10);
            }

            if (it.IsUseStandardIpanaOintment)
            {
                dame += CalculatePercent(dame, 10);
            }

            if (it.IsUseSpecialIpanaOintment)
            {
                dame += CalculatePercent(dame, 10);
            }

            if (it.IsUseTamarindCrab)
            {
                dame += CalculatePercent(dame, 5);
            }

            Damage = dame;
        }
    }

    // Cộng các tùy chọn còn lại sau chỉ số cơ sở. Thưởng vàng nội tại đọc intrinsic chỉ GetDamageAttack nạp nên thiếu cho tới lần đánh đầu như Java; Trbne nhân đôi né hiện tại bằng tlNeDon+=tlNeDon+k.
    private void SetOptions(NPointFacts facts)
    {
        unchecked
        {
            NPointPlayerFacts p = facts.Player;
            NPointItemTimeFacts it = facts.ItemTime;
            if (facts.SetClothes.Gohan == 5)
            {
                LuckPercent += 150;
                GoldPercent += 80;
            }

            if (p.IsPl && intrinsic is not null && intrinsic.Id == 23)
            {
                GoldPercent += intrinsic.Param1;
            }

            if (it.IsUseDodge)
            {
                DodgePercent += 10;
            }

            if (it.IsUseDodge2)
            {
                DodgePercent += 20;
            }

            if (it.IsUseReflectDamage)
            {
                ReflectDamagePercent += 3;
            }

            if (it.IsUseReflectDamage2)
            {
                ReflectDamagePercent += 6;
            }

            if (it.IsUseReflectDamage3)
            {
                ReflectDamagePercent += 12;
            }

            if (it.IsFaceMask)
            {
                MobAttackDamagePercents.Add(10);
            }

            if (it.IsUseTrueDamage)
            {
                TrueDamagePercent += 10;
            }

            if (it.IsUseTrueDamage2 || it.IsUseDurian)
            {
                TrueDamagePercent += 15;
            }

            if (it.IsUseStandardInvisibilityPotion)
            {
                WearingInvisibilityArmor = true;
            }

            if (it.IsUseEnhancedInvisibilityPotion)
            {
                WearingInvisibilityArmor = true;
            }

            if (p.Gender == ConstPlayer.SAIYAN)
            {
                if (facts.Zone is not null && facts.Zone.IsInvisibleMap)
                {
                    WearingInvisibilityArmor = true;
                }
            }

            // Java có một if trbne==k cho từng k từ 1..10; nhiều nhất một điều kiện khớp nên gộp thành một kiểm tra này.
            if (p.Trbne >= 1 && p.Trbne <= 10)
            {
                DodgePercent += DodgePercent + p.Trbne;
            }
        }
    }

    // Tính phòng thủ giữ def là int: BaseDefense*4 có thể tràn, tổng long bị cắt về int nên phòng thủ rất lớn có thể thành âm như Java.
    private void SetDef(NPointFacts facts)
    {
        unchecked
        {
            NPointItemTimeFacts it = facts.ItemTime;
            long now = Now();
            Defense = BaseDefense * 4;
            Defense += DefenseBonus;
            foreach (int tl in DefensePercents)
            {
                Defense = (int)(Defense + ((long)Defense * tl / 100));
            }

            if (it.IsUseSpiderCake)
            {
                Defense = (int)(Defense + ((long)Defense * 10 / 100));
            }

            // black ball 2 stars
            if (facts.BlackBallTimeOut[1] > now)
            {
                Defense = (int)(Defense + ((long)Defense * RewardBlackBall.R2S_2 / 100));
            }

            switch (facts.Player.Isbienhinh)
            {
                case 1:
                    Defense += 5000;
                    break;
                case 2:
                    Defense += 10000;
                    break;
                case 3:
                    Defense += 15000;
                    break;
                case 4:
                    Defense += 20000;
                    break;
                case 5:
                    Defense += 25000;
                    break;
                default:
                    break;
            }

            if (facts.SetClothes.CostumeInosuke != -1)
            {
                Defense = (int)(Defense + CalculatePercent(Defense, 50));
            }

            if (facts.SetClothes.CostumeInosukeHashibira != -1)
            {
                Defense = (int)(Defense + CalculatePercent(Defense, 60));
            }
        }
    }

    // Tính chí mạng từ cơ sở, tùy chọn và thưởng; khỉ, Kefla hoặc thuốc hút khỉ ép 100 và kết quả bị chặn ở 100.
    private void SetCrit(NPointFacts facts)
    {
        unchecked
        {
            NPointPlayerFacts p = facts.Player;
            NPointItemTimeFacts it = facts.ItemTime;
            NPointEffectSkillFacts es = facts.EffectSkill;
            NPointSetClothesFacts sc = facts.SetClothes;
            long now = Now();
            CriticalChance = BaseCriticalChance;
            CriticalChance += CriticalChanceBonus;
            if (p.IsPl)
            {
                CriticalChance += facts.Inventory.CritItemsInBox;
            }

            if (sc.Cumber == 5)
            {
                CriticalChance += 100;
            }

            if (it.IsUseRabbitMooncake2)
            {
                CriticalChance += 10;
            }

            if (it.IsUseMoonFestivalOneEgg)
            {
                CriticalChance += 10;
            }

            if (it.IsUseMoonFestivalTwoEgg)
            {
                CriticalChance += 15;
            }

            if (it.IsUseSpecialMoonFestival)
            {
                CriticalChance += 20;
            }

            if (it.IsUseMoonFestivalCakeBox)
            {
                CriticalChance += 25;
            }

            if (it.IsUseRocket1h)
            {
                CriticalChance += 20;
            }

            if (it.IsUseCriticalChance2)
            {
                CriticalChance += 10;
            }

            if (it.IsUseCriticalChance3)
            {
                CriticalChance += 15;
            }

            if (it.IsPowerBurst)
            {
                CriticalChance += 5;
            }

            if (it.IsUseOneEyedCandy)
            {
                CriticalChance += 5;
            }

            if (it.IsUseCylindricalRiceCake)
            {
                CriticalChance += 15;
            }

            if (it.IsUseSquareRiceCake)
            {
                CriticalChance += 25;
            }

            if (sc.ThanVuTruKaio >= 2)
            {
                CriticalChance += 20;
            }

            if (es.IsMonkey)
            {
                CriticalChance = 100;
            }

            if (es.IsCumber)
            {
                CriticalChance += 20;
            }

            if (es.IsCumber2)
            {
                CriticalChance += 15;
            }

            if (es.IsKefla)
            {
                CriticalChance = 100;
            }

            if (it.HasMonkeyTail)
            {
                CriticalChance = 100;
            }

            if (p.Buff == Buff.BUFF_CRIT)
            {
                CriticalChance += 10;
            }

            switch (p.Isbienhinh)
            {
                case 1:
                    CriticalChance += 10;
                    break;
                case 2:
                    CriticalChance += 20;
                    break;
                case 3:
                    CriticalChance += 30;
                    break;
                case 4:
                    CriticalChance += 40;
                    break;
                case 5:
                    CriticalChance += 50;
                    break;
            }

            if (facts.BlackBallTimeOut[2] > now)
            {
                CriticalChance += RewardBlackBall.R3S_2;
            }

            if (CriticalChance > 100)
            {
                CriticalChance = 100;
            }
        }
    }

    // Thêm thưởng bộ đồ và thuốc vào danh sách sát thương chí mạng và đồng thời vào tổng CriticalDamagePercent.
    private void SetCritDame(NPointFacts facts)
    {
        NPointItemTimeFacts it = facts.ItemTime;
        NPointSetClothesFacts sc = facts.SetClothes;
        if (sc.CostumeTanjiro != -1)
        {
            CriticalDamagePercent += 30;
            CriticalDamagePercents.Add(30);
        }

        if (it.IsUseGrilledOctopus)
        {
            CriticalDamagePercent += 5;
            CriticalDamagePercents.Add(5);
        }

        if (it.IsPowerBurst)
        {
            CriticalDamagePercent += 5;
            CriticalDamagePercents.Add(5);
        }

        // world cup set
        if (sc.Worldcup == 2)
        {
            CriticalDamagePercent += 20;
            CriticalDamagePercents.Add(20);
        }

        // nail set
        if (sc.Nail >= 2)
        {
            CriticalDamagePercent += 20;
            CriticalDamagePercents.Add(20);
        }

        if (sc.Cumber == 5)
        {
            CriticalDamagePercent += 100;
            CriticalDamagePercents.Add(100);
        }
    }

    // Tính tỷ lệ hút HP.
    private void SetHpAbsorption(NPointFacts facts)
    {
        if (facts.ItemTime.IsUseRabbitMooncake1)
        {
            HpAbsorptionPercent += 30;
        }

        if (facts.ItemTime.IsUseRabbitMooncake2)
        {
            HpAbsorptionPercent += 50;
        }
    }

    // Tính tỷ lệ hút KI.
    private void SetMpAbsorption(NPointFacts facts)
    {
        if (facts.ItemTime.IsUseRabbitMooncake1)
        {
            MpAbsorptionPercent += 30;
        }

        if (facts.ItemTime.IsUseRabbitMooncake2)
        {
            MpAbsorptionPercent += 50;
        }
    }

    // Xác định chí mạng: nội tại 25 ép chí mạng khi HP không vượt ngưỡng (đọc nội tại của đòn trước), IsGuaranteedCritical bị tiêu thụ, hoặc rút ngẫu nhiên CriticalChance/100; Util.isTrue không rút khi tỷ lệ 0 hoặc 100.
    private void SetIsCrit()
    {
        if (intrinsic is not null && intrinsic.Id == 25 && GetCurrentHpPercent() <= intrinsic.Param1)
        {
            IsCritical = true;
        }
        else if (IsGuaranteedCritical)
        {
            IsGuaranteedCritical = false;
            IsCritical = true;
        }
        else
        {
            IsCritical = Rand.IsTrueInt(CriticalChance, ConstRatio.PER100);
        }
    }

    // Tính sát thương một đòn trước phòng thủ mục tiêu. Thứ tự rút ngẫu nhiên: chí mạng, tầm dịch chuyển hoặc dao động quả cầu, rồi dao động ±5%. Giữ chi tiết Java: Dịch chuyển tức thời ép chí mạng và dao động theo cơ sở rồi xử lý tiếp; Makankosappo, Quả cầu và Đẻ trứng trả sớm; rút chí mạng dùng nội tại lần trước còn nội tại đọc là lần này; kết quả double từ long mất chính xác trên 2^53.
    public double GetDamageAttack(bool isAttackMob)
    {
        NPointAttackFacts af = Host.CaptureAttack(this);
        double result = ComputeDameAttack(isAttackMob, af);
        Host.CommitAttack(this, af);
        return result;
    }

    private double ComputeDameAttack(bool isAttackMob, NPointAttackFacts af)
    {
        unchecked
        {
            SetIsCrit();
            long dameAttack = Damage;
            intrinsic = af.Intrinsic;
            percentDameIntrinsic = 0;
            int percentDameSkill = 0;
            sbyte percentXDame = 0;
            NPointSkillFacts skillSelect = af.SkillSelect;
            NPointSetClothesFacts sc = af.SetClothes;
            if (skillSelect.TemplateId != Skill.DICH_CHUYEN_TUC_THOI && IsTeleportCritical)
            {
                IsCritical = true;
                IsTeleportCritical = false;
            }

            switch (skillSelect.TemplateId)
            {
                case Skill.DRAGON:
                    if (intrinsic.Id == 1)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    percentDameSkill = skillSelect.Damage;
                    break;
                case Skill.KAMEJOKO:
                    if (intrinsic.Id == 2)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    percentDameSkill = skillSelect.Damage;
                    if (sc.Songoku == 5)
                    {
                        percentXDame = 100;
                    }

                    break;
                case Skill.GALICK:
                    if (intrinsic.Id == 16)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    percentDameSkill = skillSelect.Damage;
                    if (sc.Kakarot == 5)
                    {
                        percentXDame = 100;
                    }

                    break;
                case Skill.ANTOMIC:
                    if (intrinsic.Id == 17)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    percentDameSkill = skillSelect.Damage;
                    break;
                case Skill.DEMON:
                    if (intrinsic.Id == 8)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    percentDameSkill = skillSelect.Damage;
                    break;
                case Skill.MASENKO:
                    if (intrinsic.Id == 9)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    if (sc.Nail == 5)
                    {
                        percentXDame = 50;
                    }

                    percentDameSkill = skillSelect.Damage;
                    break;
                case Skill.LIEN_HOAN:
                    if (intrinsic.Id == 13)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    percentDameSkill = skillSelect.Damage;
                    if (sc.OcTieu == 5)
                    {
                        percentXDame = 100;
                    }

                    break;
                case Skill.KAIOKEN:
                    if (intrinsic.Id == 26)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    percentDameSkill = skillSelect.Damage;
                    if (sc.Yamcha == 5)
                    {
                        percentXDame = 100;
                    }
                    else if (sc.ThanVuTruKaio == 5)
                    {
                        percentXDame = 30;
                    }

                    break;
                case Skill.TU_SAT:
                    percentDameSkill = skillSelect.Damage;
                    if (sc.CadicM == 4)
                    {
                        percentXDame = 20;
                    }
                    else if (sc.CadicM == 5)
                    {
                        percentXDame = 40;
                    }

                    break;
                case Skill.DICH_CHUYEN_TUC_THOI:
                    IsCritical = true;
                    IsTeleportCritical = true;
                    dameAttack = Rand.NextLong(
                        Util.CrisGH(dameAttack - (dameAttack / 100 * 5), af.ReadInt),
                        Util.CrisGH(dameAttack + (dameAttack / 100 * 5), af.ReadInt));
                    break;
                case Skill.MAKANKOSAPPO:
                    percentDameSkill = skillSelect.Damage;
                    long dameSkill = Util.CrisGH(MpMax * percentDameSkill / 100, af.ReadInt);
                    return dameSkill;
                case Skill.QUA_CAU_KENH_KHI:
                    long hpmob = af.AroundMobHp;
                    long hppl = af.AroundPlayerHp;
                    long dameqckk = (hpmob * 10 / 100) + (hppl * 10 / 100) + (Damage * 10);

                    if (sc.Kirin == 5)
                    {
                        dameqckk *= 2;
                    }

                    dameqckk = dameqckk + (Rand.NextInt(-5, 5) * dameqckk / 100);

                    return dameqckk;
                case Skill.DE_TRUNG:
                    if (sc.PikkoroDaimao == 5)
                    {
                        dameAttack *= 2;
                    }

                    return dameAttack;
            }

            if (percentDameSkill != 0)
            {
                dameAttack = dameAttack * percentDameSkill / 100;
            }

            dameAttack += dameAttack * percentDameIntrinsic / 100;
            dameAttack += dameAttack * DamageAfter / 100;

            if (af.EffectSkill.IsDamageBuff && (SexyDamagePercent == 0 || CoolDamagePercent == 0 || CuteDamagePercent == 0))
            {
                int tiLeDame = af.EffectSkill.DamageBuffPercent;
                dameAttack += dameAttack * tiLeDame / 100L;
            }

            if (isAttackMob)
            {
                foreach (int tl in MobAttackDamagePercents)
                {
                    dameAttack += dameAttack * tl / 100;
                }

                if (af.IsDeTu && af.MasterCharmsTdDeTu > Now())
                {
                    dameAttack *= 2;
                }
            }

            DamageAfter = 0;

            if (IsCritical)
            {
                dameAttack *= 2;
                dameAttack += dameAttack * CriticalDamagePercent / 100;
            }

            if (isAttackMob)
            {
                foreach (int tl in MobAttackDamagePercents)
                {
                    dameAttack += dameAttack * tl / 100;
                }
            }

            dameAttack += dameAttack * percentXDame / 100;

            long tempDameAttack = dameAttack / 100L * 5L;
            if (tempDameAttack <= 0)
            {
                tempDameAttack = 1;
            }

            dameAttack += Rand.GetOne(-1, 1) * Rand.Crisnext((double)tempDameAttack) + 1;

            if (af.EffectSkin.IsChargedBlast && (skillSelect.TemplateId == Skill.KAMEJOKO || skillSelect.TemplateId == Skill.ANTOMIC || skillSelect.TemplateId == Skill.MASENKO))
            {
                dameAttack *= ChargedBlastLevel;
                af.EffectSkin.IsDamageBoosted = true;
                af.EffectSkin.IsChargedBlast = false;
                af.EffectSkin.LastTimeChargedBlast = Now();
            }

            return dameAttack;
        }
    }
}
