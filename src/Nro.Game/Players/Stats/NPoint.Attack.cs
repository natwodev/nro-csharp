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
            NPointPetFacts? pet = facts.Pet;

            // Sát thương đang tính (Java: dame).
            long damage = BaseDamage + DamageBonus;
            foreach (int tl in DamagePercents)
            {
                damage += damage * tl / 100L;
            }

            // Nguyet An: five pieces give +15% attack
            if (HasFull5NguyetAn())
            {
                damage += CalculatePercent(damage, 15);
            }

            if (p.IsPl)
            {
                if (facts.Inventory.HasRongNhi)
                {
                    damage += CalculatePercent(damage, 1);
                }

                damage += CalculatePercent(damage, facts.Inventory.DamageItemsInBox);
            }

            if (sc.Cumber == 5)
            {
                damage += CalculatePercent(damage, 20);
            }

            if (p.IsBodyClone)
            {
                damage = CalculatePercent(RequireMaster(facts).Damage, NPointSkillMath.GetClonePercent(facts.SkillSelect));
            }

            if (p.WeeklyCard == 1 && p.LastWeeklyCardTime > now)
            {
                damage += CalculatePercent(damage, 3);
            }

            if (p.WeeklyCard == 2 && p.LastWeeklyCardTime > now)
            {
                damage += CalculatePercent(damage, 5);
            }

            if (p.MonthlyCard == 1 && p.LastMonthlyCardTime > now)
            {
                damage += CalculatePercent(damage, 7);
            }

            if (p.MonthlyCard == 2 && p.LastMonthlyCardTime > now)
            {
                damage += CalculatePercent(damage, 10);
            }

            if (p.YearlyCard == 1 && p.LastYearlyCardTime > now)
            {
                damage += CalculatePercent(damage, 15);
            }

            if (p.YearlyCard == 2 && p.LastYearlyCardTime > now)
            {
                damage += CalculatePercent(damage, 18);
            }

            if (p.SupremeCard == 1 && p.LastSupremeCardTime > now)
            {
                damage += CalculatePercent(damage, 20);
            }

            if (p.ClanDamageBonusPercent > 0)
            {
                damage += CalculatePercent(damage, p.ClanDamageBonusPercent);
            }

            if (server.InternationalWomansDay)
            {
                damage += CalculatePercent(damage, server.WomansDayDame);
            }

            if (es.IsVirus)
            {
                damage -= CalculatePercent(damage, 10);
            }

            damage += damage * SexyDamagePercent / 100;

            damage += damage * CoolDamagePercent / 100;

            damage += damage * CuteDamagePercent / 100;

            if (p.IsPl && p.UsesEmperorTitle && p.LastEmperorTitleTime > 0)
            {
                damage += CalculatePercent(damage, 5);
            }

            if (it.HasMonkeyTail)
            {
                damage += CalculatePercent(damage, 10);
            }

            if (p.Buff == Buff.BUFF_ATK)
            {
                damage += CalculatePercent(damage, 20);
            }

            if (it.IsUseRabbitMooncake1)
            {
                damage += CalculatePercent(damage, 5);
            }

            if (it.IsUseRabbitMooncake2)
            {
                damage += CalculatePercent(damage, 10);
            }

            if (it.IsUseRabbitMooncake3)
            {
                damage += CalculatePercent(damage, 15);
            }

            if (it.IsUseMoonFestivalOneEgg)
            {
                damage += CalculatePercent(damage, 10);
            }

            if (it.IsUseMoonFestivalTwoEgg)
            {
                damage += CalculatePercent(damage, 15);
            }

            if (it.IsUseSpecialMoonFestival)
            {
                damage += CalculatePercent(damage, 20);
            }

            if (it.IsUseMoonFestivalCakeBox)
            {
                damage += CalculatePercent(damage, 25);
            }

            if (it.IsUseRocket1h)
            {
                damage += CalculatePercent(damage, 20);
            }

            if (es.IsSnowflake)
            {
                damage -= CalculatePercent(damage, 20);
            }

            if (it.IsBoneDragon2)
            {
                damage += CalculatePercent(damage, 15);
            }

            if (facts.EffectSkin.IsRabbitLeader)
            {
                damage -= CalculatePercent(damage, 15);
            }

            // world cup set
            if (sc.Worldcup == 2)
            {
                damage += CalculatePercent(damage, 10);
            }

            // food. Java precedence: (!isDeTu && itemTime.isEatMeal) || (isDeTu && master.itemTime.isEatMeal)
            if ((!p.IsPet && it.IsEatMeal) || (p.IsPet && RequireMaster(facts).ItemTimeIsEatMeal))
            {
                damage += CalculatePercent(damage, 10);
            }

            if (pet is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                if (pet.PetType >= 5)
                {
                    damage += pet.Damage * facts.FusionDamage / 100L;
                }
            }

            // fusion
            if (pet is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                if (pet.PetType == 1)
                {
                    damage += pet.Damage * 20 / 100L;
                }
            }

            if (pet is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                if (pet.PetType == 5)
                {
                    damage += pet.Damage * 40 / 100L;
                }
            }

            if (pet is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                damage += pet.Damage;
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_2)
            {
                damage += CalculatePercent(damage, 5);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_3)
            {
                damage += CalculatePercent(damage, 10);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_4)
            {
                damage += CalculatePercent(damage, 15);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_5)
            {
                damage += CalculatePercent(damage, 20);
            }

            // rage
            if (it.IsUseRage)
            {
                damage *= 2;
            }

            if (it.IsUseRedPeach)
            {
                damage += CalculatePercent(damage, 1);
            }

            // peach juices
            if (it.IsUseRedPeach0)
            {
                damage -= CalculatePercent(damage, 99);
            }

            if (it.IsUseRedPeach1)
            {
                damage += CalculatePercent(damage, 2);
            }

            if (it.IsUseRedPeach3)
            {
                damage += CalculatePercent(damage, 3);
            }

            if (it.IsUseRedPeach5)
            {
                damage += CalculatePercent(damage, 5);
            }

            if (it.IsUseRedPeach10)
            {
                damage += CalculatePercent(damage, 8);
            }

            if (it.IsUseRedPeach25)
            {
                damage += CalculatePercent(damage, 12);
            }

            if (it.IsUseRedPeach50)
            {
                damage += CalculatePercent(damage, 15);
            }

            if (it.IsUseRedPeach99)
            {
                damage += CalculatePercent(damage, 20);
            }

            if (it.IsUseRedPeach999)
            {
                damage += CalculatePercent(damage, 100);
            }

            if (it.IsUseRage2)
            {
                damage = JavaNumbers.ToLong(damage * 2.2);
            }

            if (it.HasDamageBoost30)
            {
                damage += CalculatePercent(damage, 30);
            }

            if (it.HasDamageBoost15)
            {
                damage += CalculatePercent(damage, 15);
            }

            if (it.IsUseCylindricalRiceCake)
            {
                damage += CalculatePercent(damage, 15);
            }

            if (it.IsUseSquareRiceCake)
            {
                damage += CalculatePercent(damage, 25);
            }

            // mabu map scroll
            if (p.IsMabuMapProtected)
            {
                damage += 10_000;
            }

            if (p.IsPlMan)
            {
                if (server.AttributeSucDanhActive)
                {
                    damage += CalculatePercent(damage, server.AttributeSucDanhValue);
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
                        damage += CalculatePercent(damage, 30);
                    }
                    else if (playerRank == 2)
                    {
                        damage += CalculatePercent(damage, 20);
                    }
                    else if (playerRank == 3)
                    {
                        damage += CalculatePercent(damage, 10);
                    }
                    else if (playerRank >= 4 && playerRank <= 5)
                    {
                        damage += CalculatePercent(damage, 5);
                    }
                    else if (playerRank >= 6 && playerRank <= 10)
                    {
                        damage += CalculatePercent(damage, 3);
                    }
                }
            }

            // Breakthrough power skill
            if (p.IsPl && facts.SkillSelect is not null)
            {
                // Tỷ lệ sát thương sức mạnh bộc phá (Java: tiLeDameSucManhBocPha).
                int powerBurstDamagePercent = NPointSkillMath.GetPowerBurstDamagePercent(facts.SkillSelect.Point);
                if (es.IsPowerBurst)
                {
                    damage += damage * powerBurstDamagePercent / 100;
                }
            }

            // option "reduce attack"
            damage -= damage * DamageReductionPercent / 100;

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapCold && !IsColdImmune)
            {
                damage /= 2;
            }

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapChristMasEvent && !IsColdImmune)
            {
                damage /= 2;
            }

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMap5000NamTruoc)
            {
                damage -= CalculatePercent(damage, 90);
            }

            if (p.Gender == ConstPlayer.SAIYAN)
            {
                if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapCereal)
                {
                    damage /= 2;
                }
            }

            // black ball 1 star
            if (facts.BlackBallTimeOut[0] > now)
            {
                damage += damage * RewardBlackBall.R1S_2 / 100;
            }

            if (es.IsMonkey)
            {
                if (!p.IsPet || (p.IsPet && p.PetStatus != NPointFacts.PetFusionStatus))
                {
                    int percent = NPointSkillMath.GetMonkeyDamagePercent(es.LevelMonkey);
                    damage += damage * percent / 100;
                }
            }

            // scroll (black ball war)
            if (zone is not null && zone.IsMapBlackBallWar)
            {
                damage *= facts.EffectSkin.DamageMultiplier;
            }

            if (it.IsUseCaterpillarHamburger)
            {
                damage += CalculatePercent(damage, 10);
            }

            if (it.IsUseStandardIpanaOintment)
            {
                damage += CalculatePercent(damage, 10);
            }

            if (it.IsUseSpecialIpanaOintment)
            {
                damage += CalculatePercent(damage, 10);
            }

            if (it.IsUseTamarindCrab)
            {
                damage += CalculatePercent(damage, 5);
            }

            Damage = damage;
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

            switch (facts.Player.TransformationState)
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

            switch (p.TransformationState)
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

    // Thêm thưởng bộ đồ và thuốc vào danh sách sát thương chí mạng và đồng thời vào tổng CriticalDamagePercent (Java: setCritDame).
    private void SetCriticalDamage(NPointFacts facts)
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
        double result = ComputeAttackDamage(isAttackMob, af);
        Host.CommitAttack(this, af);
        return result;
    }

    // Tính sát thương tấn công (Java: computeDameAttack).
    private double ComputeAttackDamage(bool isAttackMob, NPointAttackFacts af)
    {
        unchecked
        {
            SetIsCrit();
            // Sát thương tấn công đang tính (Java: dameAttack).
            long attackDamage = Damage;
            intrinsic = af.Intrinsic;
            percentDameIntrinsic = 0;
            // Phần trăm sát thương kỹ năng và sát thương bổ sung (Java: percentDameSkill, percentXDame).
            int skillDamagePercent = 0;
            sbyte extraDamagePercent = 0;
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

                    skillDamagePercent = skillSelect.Damage;
                    break;
                case Skill.KAMEJOKO:
                    if (intrinsic.Id == 2)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    skillDamagePercent = skillSelect.Damage;
                    if (sc.Songoku == 5)
                    {
                        extraDamagePercent = 100;
                    }

                    break;
                case Skill.GALICK:
                    if (intrinsic.Id == 16)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    skillDamagePercent = skillSelect.Damage;
                    if (sc.Kakarot == 5)
                    {
                        extraDamagePercent = 100;
                    }

                    break;
                case Skill.ANTOMIC:
                    if (intrinsic.Id == 17)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    skillDamagePercent = skillSelect.Damage;
                    break;
                case Skill.DEMON:
                    if (intrinsic.Id == 8)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    skillDamagePercent = skillSelect.Damage;
                    break;
                case Skill.MASENKO:
                    if (intrinsic.Id == 9)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    if (sc.Nail == 5)
                    {
                        extraDamagePercent = 50;
                    }

                    skillDamagePercent = skillSelect.Damage;
                    break;
                case Skill.LIEN_HOAN:
                    if (intrinsic.Id == 13)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    skillDamagePercent = skillSelect.Damage;
                    if (sc.OcTieu == 5)
                    {
                        extraDamagePercent = 100;
                    }

                    break;
                case Skill.KAIOKEN:
                    if (intrinsic.Id == 26)
                    {
                        percentDameIntrinsic = intrinsic.Param1;
                    }

                    skillDamagePercent = skillSelect.Damage;
                    if (sc.Yamcha == 5)
                    {
                        extraDamagePercent = 100;
                    }
                    else if (sc.ThanVuTruKaio == 5)
                    {
                        extraDamagePercent = 30;
                    }

                    break;
                case Skill.TU_SAT:
                    skillDamagePercent = skillSelect.Damage;
                    if (sc.CadicM == 4)
                    {
                        extraDamagePercent = 20;
                    }
                    else if (sc.CadicM == 5)
                    {
                        extraDamagePercent = 40;
                    }

                    break;
                case Skill.DICH_CHUYEN_TUC_THOI:
                    IsCritical = true;
                    IsTeleportCritical = true;
                    attackDamage = Rand.NextLong(
                        Util.CrisGH(attackDamage - (attackDamage / 100 * 5), af.ReadInt),
                        Util.CrisGH(attackDamage + (attackDamage / 100 * 5), af.ReadInt));
                    break;
                case Skill.MAKANKOSAPPO:
                    skillDamagePercent = skillSelect.Damage;
                    // Sát thương kỹ năng (Java: dameSkill).
                    long skillDamage = Util.CrisGH(MpMax * skillDamagePercent / 100, af.ReadInt);
                    return skillDamage;
                case Skill.QUA_CAU_KENH_KHI:
                    long hpmob = af.AroundMobHp;
                    long hppl = af.AroundPlayerHp;
                    // Sát thương kỹ năng đặc biệt (Java: dameqckk).
                    long specialSkillDamage = (hpmob * 10 / 100) + (hppl * 10 / 100) + (Damage * 10);

                    if (sc.Kirin == 5)
                    {
                        specialSkillDamage *= 2;
                    }

                    specialSkillDamage = specialSkillDamage + (Rand.NextInt(-5, 5) * specialSkillDamage / 100);

                    return specialSkillDamage;
                case Skill.DE_TRUNG:
                    if (sc.PikkoroDaimao == 5)
                    {
                        attackDamage *= 2;
                    }

                    return attackDamage;
            }

            if (skillDamagePercent != 0)
            {
                attackDamage = attackDamage * skillDamagePercent / 100;
            }

            attackDamage += attackDamage * percentDameIntrinsic / 100;
            attackDamage += attackDamage * DamageAfter / 100;

            if (af.EffectSkill.IsDamageBuff && (SexyDamagePercent == 0 || CoolDamagePercent == 0 || CuteDamagePercent == 0))
            {
                // Tỷ lệ sát thương tăng cường (Java: tiLeDame).
                int damagePercent = af.EffectSkill.DamageBuffPercent;
                attackDamage += attackDamage * damagePercent / 100L;
            }

            if (isAttackMob)
            {
                foreach (int tl in MobAttackDamagePercents)
                {
                    attackDamage += attackDamage * tl / 100;
                }

                if (af.IsPet && af.MasterPetCharmTraining > Now())
                {
                    attackDamage *= 2;
                }
            }

            DamageAfter = 0;

            if (IsCritical)
            {
                attackDamage *= 2;
                attackDamage += attackDamage * CriticalDamagePercent / 100;
            }

            if (isAttackMob)
            {
                foreach (int tl in MobAttackDamagePercents)
                {
                    attackDamage += attackDamage * tl / 100;
                }
            }

            attackDamage += attackDamage * extraDamagePercent / 100;

            // Biên độ sát thương tấn công tạm thời (Java: tempDameAttack).
            long temporaryAttackDamage = attackDamage / 100L * 5L;
            if (temporaryAttackDamage <= 0)
            {
                temporaryAttackDamage = 1;
            }

            attackDamage += Rand.GetOne(-1, 1) * Rand.Crisnext((double)temporaryAttackDamage) + 1;

            if (af.EffectSkin.IsBlastDamageMultiplierReady && (skillSelect.TemplateId == Skill.KAMEJOKO || skillSelect.TemplateId == Skill.ANTOMIC || skillSelect.TemplateId == Skill.MASENKO))
            {
                attackDamage *= BlastDamageMultiplier;
                af.EffectSkin.IsBlastDamageMultiplierApplied = true;
                af.EffectSkin.IsBlastDamageMultiplierReady = false;
                af.EffectSkin.LastTimeBlastDamageMultiplier = Now();
            }

            return attackDamage;
        }
    }
}
