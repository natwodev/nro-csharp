using Nro.Core.Consts;

namespace Nro.Game.Players.Stats;

// Nhóm 2 của NPoint: HP/KI tối đa và hồi phục.
// Nguồn Java: nro/player/NPoint.java (mỗi mức tối đa là chuỗi x+=x*percent/100; thứ tự từng bước quyết định kết quả vì áp dụng phần trăm trước và sau nhân đôi khác nhau, nên giữ thứ tự Java từng dòng).
public partial class NPoint
{
    // Tính hồi HP: 1% HP tối đa cộng thưởng phẳng rồi thưởng phần trăm; hai tỷ lệ bị chặn 0..100 và ghi trả vào trường như Java.
    private void SetHpRegen(NPointFacts facts)
    {
        unchecked
        {
            NPointItemTimeFacts it = facts.ItemTime;
            HpRegen = HpMax / 100;
            HpRegen += HpRegenBonus;

            if (HpRegenPercent > 100)
            {
                HpRegenPercent = 100;
            }
            else if (HpRegenPercent < 0)
            {
                HpRegenPercent = 0;
            }

            HpRegen += HpMax * HpRegenPercent / 100;

            if (SelfAndTeamHpRegenPercent > 100)
            {
                SelfAndTeamHpRegenPercent = 100;
            }
            else if (SelfAndTeamHpRegenPercent < 0)
            {
                SelfAndTeamHpRegenPercent = 0;
            }

            HpRegen += HpMax * SelfAndTeamHpRegenPercent / 100;

            if (it.IsUseStandardIpanaOintment)
            {
                HpRegen += CalculatePercent(HpMax, 10);
            }

            if (it.IsUseSpecialIpanaOintment)
            {
                HpRegen += CalculatePercent(HpMax, 10);
            }

            if (facts.SetClothes.CostumeNezuko != -1)
            {
                HpRegen += CalculatePercent(HpMax, 3);
            }

            if (it.IsUseSuperRegen)
            {
                HpRegen += CalculatePercent(HpMax, 100);
            }

            if (it.IsUseRabbitMooncake3)
            {
                HpRegen += CalculatePercent(HpMax, 10);
            }
        }
    }

    // Tính hồi KI tương tự SetHpRegen.
    private void SetMpRegen(NPointFacts facts)
    {
        unchecked
        {
            NPointItemTimeFacts it = facts.ItemTime;
            MpRegen = MpMax / 100;
            MpRegen += MpRegenBonus;

            if (MpRegenPercent > 100)
            {
                MpRegenPercent = 100;
            }
            else if (MpRegenPercent < 0)
            {
                MpRegenPercent = 0;
            }

            MpRegen += MpMax * MpRegenPercent / 100;

            if (SelfAndTeamMpRegenPercent > 100)
            {
                SelfAndTeamMpRegenPercent = 100;
            }
            else if (SelfAndTeamMpRegenPercent < 0)
            {
                SelfAndTeamMpRegenPercent = 0;
            }

            MpRegen += MpMax * SelfAndTeamMpRegenPercent / 100;

            if (it.IsUseStandardIpanaOintment)
            {
                MpRegen += CalculatePercent(MpMax, 10);
            }

            if (it.IsUseSpecialIpanaOintment)
            {
                MpRegen += CalculatePercent(MpMax, 10);
            }

            if (facts.SetClothes.CostumeNezuko != -1)
            {
                MpRegen += CalculatePercent(MpMax, 3);
            }

            if (it.IsUseSuperRegen)
            {
                MpRegen += CalculatePercent(MpMax, 100);
            }

            if (it.IsUseRabbitMooncake3)
            {
                MpRegen += CalculatePercent(MpMax, 10);
            }
        }
    }

    // Tính HP tối đa từ BaseHp+HpBonus theo đúng thứ tự Java: tùy chọn vật phẩm, bộ ấn, inventory, clan, phân thân, thẻ tuần/tháng/năm, danh hiệu, sự kiện, bộ đồ, debuff, khỉ, thuộc tính server, cuộn chiến Ngọc Rồng, đệ tử hợp thể, bông tai, huýt sáo, thuốc, chibi, buff clan, Whis, map Mabu, giảm sát thương, nước đào, sao 2, map giảm và thưởng nhỏ.
    private void SetHpMax(NPointFacts facts)
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

            long hpMax = BaseHp + HpBonus;
            foreach (int tl in HpPercents)
            {
                hpMax += hpMax * tl / 100L;
            }

            // Tinh An (full seal set)
            if (HasFullFiveStarSeal())
            {
                hpMax += CalculatePercent(hpMax, 15);
            }

            if (p.IsPl)
            {
                if (facts.Inventory.HasRongNhi)
                {
                    hpMax += CalculatePercent(hpMax, 1);
                }

                hpMax += CalculatePercent(hpMax, facts.Inventory.HpItemsInBox);
            }

            if (p.ClanHpBonusPercent > 0)
            {
                hpMax += CalculatePercent(hpMax, p.ClanHpBonusPercent);
            }

            if (p.IsBodyClone)
            {
                hpMax = CalculatePercent(RequireMaster(facts).HpMax, NPointSkillMath.GetClonePercent(facts.SkillSelect));
            }

            if (p.WeeklyCard == 1 && p.LastWeeklyCardTime > now)
            {
                hpMax += CalculatePercent(hpMax, 3);
            }

            if (p.WeeklyCard == 2 && p.LastWeeklyCardTime > now)
            {
                hpMax += CalculatePercent(hpMax, 5);
            }

            if (p.MonthlyCard == 1 && p.LastMonthlyCardTime > now)
            {
                hpMax += CalculatePercent(hpMax, 7);
            }

            if (p.MonthlyCard == 2 && p.LastMonthlyCardTime > now)
            {
                hpMax += CalculatePercent(hpMax, 10);
            }

            if (p.YearlyCard == 1 && p.LastYearlyCardTime > now)
            {
                hpMax += CalculatePercent(hpMax, 15);
            }

            if (p.YearlyCard == 2 && p.LastYearlyCardTime > now)
            {
                hpMax += CalculatePercent(hpMax, 18);
            }

            if (p.SupremeCard == 1 && p.LastSupremeCardTime > now)
            {
                hpMax += CalculatePercent(hpMax, 20);
            }

            if (p.IsPl && p.UsesEmperorTitle && p.LastEmperorTitleTime > 0)
            {
                hpMax += CalculatePercent(hpMax, 5);
            }

            if (server.InternationalWomansDay)
            {
                hpMax += CalculatePercent(hpMax, server.WomansDayHp);
            }

            // Nappa set
            if (sc.Nappa == 5)
            {
                hpMax += CalculatePercent(hpMax, 100);
            }

            if (sc.CadicM >= 2)
            {
                hpMax += CalculatePercent(hpMax, 20);
            }

            if (es.IsVirus)
            {
                hpMax -= CalculatePercent(hpMax, 10);
            }

            if (es.IsSnowflake)
            {
                hpMax -= CalculatePercent(hpMax, 20);
            }

            // world cup set
            if (sc.Worldcup == 2)
            {
                hpMax += CalculatePercent(hpMax, 10);
            }

            if (it.IsUseRocket1h)
            {
                hpMax += CalculatePercent(hpMax, 20);
            }

            if (it.IsBoneDragon2)
            {
                hpMax += CalculatePercent(hpMax, 15);
            }

            if (es.IsMonkey)
            {
                if (!p.IsPet || (p.IsPet && p.PetStatus != NPointFacts.PetFusionStatus))
                {
                    int percent = NPointSkillMath.GetPercentHpMonkey(es.LevelMonkey);
                    hpMax += hpMax * percent / 100;
                }
            }

            if (p.IsPlMan)
            {
                if (server.AttributeHpActive)
                {
                    hpMax += CalculatePercent(hpMax, server.AttributeHpValue);
                }
            }

            // scroll (black ball war)
            if (zone is not null && zone.IsMapBlackBallWar)
            {
                hpMax *= facts.EffectSkin.HpKiMultiplier;
            }

            if (pet is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                if (pet.PetType == 1)
                {
                    hpMax += pet.HpMax * 20 / 100L;
                }
            }

            if (pet is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                if (pet.PetType == 5)
                {
                    hpMax += pet.HpMax * 40 / 100L;
                }
            }

            // + pet hp
            if (pet is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                hpMax += pet.HpMax;
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_2)
            {
                hpMax += CalculatePercent(hpMax, 5);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_3)
            {
                hpMax += CalculatePercent(hpMax, 10);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_4)
            {
                hpMax += CalculatePercent(hpMax, 15);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_5)
            {
                hpMax += CalculatePercent(hpMax, 20);
            }

            // whistle
            if (!p.IsPet || (p.IsPet && p.PetStatus != NPointFacts.PetFusionStatus))
            {
                if (es.HpWhistlePercent != 0)
                {
                    hpMax += hpMax * es.HpWhistlePercent / 100L;
                }
            }

            // blood boost
            if (it.IsUseBloodTonic)
            {
                hpMax *= 2;
            }

            // chibi
            if (es.IsChibi && p.TypeChibi == 3)
            {
                hpMax *= 2;
            }

            if (p.Buff == Buff.BUFF_HP)
            {
                hpMax += CalculatePercent(hpMax, 20);
            }

            // Top Whis ranking
            IReadOnlyList<long> list = server.WhisPlayerIds;
            if (list.Count != 0 && list.Count > 2 && p.IsPl)
            {
                if (list.Count >= 5 && p.IsPl)
                {
                    int playerRank = GetPlayerRank(list, p.Id);
                    if (playerRank == 1)
                    {
                        hpMax += CalculatePercent(hpMax, 30);
                    }
                    else if (playerRank == 2)
                    {
                        hpMax += CalculatePercent(hpMax, 20);
                    }
                    else if (playerRank == 3)
                    {
                        hpMax += CalculatePercent(hpMax, 10);
                    }
                    else if (playerRank >= 4 && playerRank <= 5)
                    {
                        hpMax += CalculatePercent(hpMax, 5);
                    }
                    else if (playerRank >= 6 && playerRank <= 10)
                    {
                        hpMax += CalculatePercent(hpMax, 3);
                    }
                }
            }

            // mabu map scroll
            if (p.IsMabuMapProtected)
            {
                hpMax += 1_000_000;
            }

            // option "reduce hp"
            hpMax -= hpMax * HpReductionPercent / 100;

            // peach juices
            if (it.IsUseRedPeach0)
            {
                hpMax -= CalculatePercent(hpMax, 99);
            }

            if (it.IsUseRedPeach)
            {
                hpMax += CalculatePercent(hpMax, 1);
            }

            if (it.IsUseRedPeach1)
            {
                hpMax += CalculatePercent(hpMax, 2);
            }

            if (it.IsUseRedPeach3)
            {
                hpMax += CalculatePercent(hpMax, 3);
            }

            if (it.IsUseRedPeach5)
            {
                hpMax += CalculatePercent(hpMax, 5);
            }

            if (it.IsUseRedPeach10)
            {
                hpMax += CalculatePercent(hpMax, 8);
            }

            if (it.IsUseRedPeach25)
            {
                hpMax += CalculatePercent(hpMax, 12);
            }

            if (it.IsUseRedPeach50)
            {
                hpMax += CalculatePercent(hpMax, 15);
            }

            if (it.IsUseRedPeach99)
            {
                hpMax += CalculatePercent(hpMax, 20);
            }

            if (it.IsUseRedPeach999)
            {
                hpMax += CalculatePercent(hpMax, 100);
            }

            if (it.HasHpBoost30)
            {
                hpMax += CalculatePercent(hpMax, 30);
            }

            if (it.HasHpBoost15)
            {
                hpMax += CalculatePercent(hpMax, 15);
            }

            // black ball 2 stars
            if (facts.BlackBallTimeOut[1] > now)
            {
                hpMax += hpMax * RewardBlackBall.R2S_1 / 100;
            }

            // Vật phẩm siêu cấp: HpMax*=2.2 là phép nhân double rồi cắt về long.
            if (it.IsUseBloodTonic2)
            {
                hpMax = Nro.Core.Util.JavaNumbers.ToLong(hpMax * 2.2);
            }

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapCold && !IsColdImmune)
            {
                hpMax /= 2;
            }

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapChristMasEvent && !IsColdImmune)
            {
                hpMax /= 2;
            }

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMap5000NamTruoc)
            {
                hpMax -= CalculatePercent(hpMax, 90);
            }

            if (p.Gender == ConstPlayer.SAIYAN)
            {
                if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapCereal)
                {
                    hpMax /= 2;
                }
            }

            if (it.IsUseDarkPumpkinSoup)
            {
                hpMax += CalculatePercent(hpMax, 10);
            }

            if (it.IsUseBreadedShrimp)
            {
                hpMax += CalculatePercent(hpMax, 5);
            }

            HpMax = hpMax;
        }
    }

    // Hạ HP hiện tại về HP tối đa nếu đang vượt quá.
    private void SetHp()
    {
        if (Hp > HpMax)
        {
            Hp = HpMax;
        }
    }

    // Tính KI tối đa theo SetHpMax, nhưng giữ các đặc tính Java: sao 6 áp dụng hai lần, thưởng Whis hạng 3 cộng nhầm vào HpMax đã tính trước thay vì KI, và bộ Picolo nhân đôi KI trước mọi thứ.
    private void SetMpMax(NPointFacts facts)
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

            long mpMax = BaseMp + MpBonus;
            foreach (int tl in MpPercents)
            {
                mpMax += mpMax * tl / 100L;
            }

            // Nhat An (full seal set)
            if (HasFull5NhatAn())
            {
                mpMax += CalculatePercent(mpMax, 15);
            }

            // Picolo set
            if (sc.Picolo == 5)
            {
                mpMax *= 2;
            }

            if (p.IsPl)
            {
                if (facts.Inventory.HasRongNhi)
                {
                    mpMax += CalculatePercent(mpMax, 1);
                }

                mpMax += CalculatePercent(mpMax, facts.Inventory.MpItemsInBox);
            }

            if (p.ClanMpBonusPercent > 0)
            {
                mpMax += CalculatePercent(mpMax, p.ClanMpBonusPercent);
            }

            if (p.IsBodyClone)
            {
                mpMax = CalculatePercent(RequireMaster(facts).MpMax, NPointSkillMath.GetClonePercent(facts.SkillSelect));
            }

            if (p.WeeklyCard == 1 && p.LastWeeklyCardTime > now)
            {
                mpMax += CalculatePercent(mpMax, 3);
            }

            if (p.WeeklyCard == 2 && p.LastWeeklyCardTime > now)
            {
                mpMax += CalculatePercent(mpMax, 5);
            }

            if (p.MonthlyCard == 1 && p.LastMonthlyCardTime > now)
            {
                mpMax += CalculatePercent(mpMax, 7);
            }

            if (p.MonthlyCard == 2 && p.LastMonthlyCardTime > now)
            {
                mpMax += CalculatePercent(mpMax, 10);
            }

            if (p.YearlyCard == 1 && p.LastYearlyCardTime > now)
            {
                mpMax += CalculatePercent(mpMax, 15);
            }

            if (p.YearlyCard == 2 && p.LastYearlyCardTime > now)
            {
                mpMax += CalculatePercent(mpMax, 18);
            }

            if (p.SupremeCard == 1 && p.LastSupremeCardTime > now)
            {
                mpMax += CalculatePercent(mpMax, 20);
            }

            if (p.IsPl && p.UsesEmperorTitle && p.LastEmperorTitleTime > 0)
            {
                mpMax += CalculatePercent(mpMax, 5);
            }

            if (p.Buff == Buff.BUFF_KI)
            {
                mpMax += CalculatePercent(mpMax, 20);
            }

            if (server.InternationalWomansDay)
            {
                mpMax += CalculatePercent(mpMax, server.WomansDayMp);
            }

            if (it.IsBoneDragon2)
            {
                mpMax += CalculatePercent(mpMax, 15);
            }

            if (es.IsSnowflake)
            {
                mpMax -= CalculatePercent(mpMax, 20);
            }

            // mabu map scroll
            if (p.IsMabuMapProtected)
            {
                mpMax += 1_000_000;
            }

            if (it.IsUseRocket1h)
            {
                mpMax += CalculatePercent(mpMax, 20);
            }

            // Ngọc Rồng sao 6, áp dụng hai lần như Java.
            if (facts.BlackBallTimeOut[5] > now)
            {
                mpMax += mpMax * RewardBlackBall.R6S_1 / 100;
                mpMax += mpMax * RewardBlackBall.R6S_1 / 100L;
            }

            if (es.IsVirus)
            {
                mpMax -= CalculatePercent(mpMax, 10);
            }

            // world cup set
            if (sc.Worldcup == 2)
            {
                mpMax += CalculatePercent(mpMax, 10);
            }

            if (pet is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                if (pet.PetType == 1)
                {
                    mpMax += pet.MpMax * 20 / 100L;
                }
            }

            if (pet is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                if (pet.PetType == 5)
                {
                    mpMax += pet.MpMax * 40 / 100L;
                }
            }

            // fusion
            if (pet is not null && facts.TypeFusion != ConstPlayer.NON_FUSION)
            {
                mpMax += pet.MpMax;
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_2)
            {
                mpMax += CalculatePercent(mpMax, 5);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_3)
            {
                mpMax += CalculatePercent(mpMax, 10);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_4)
            {
                mpMax += CalculatePercent(mpMax, 15);
            }

            if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_5)
            {
                mpMax += CalculatePercent(mpMax, 20);
            }

            // ki boost
            if (it.IsUseEnergyTonic)
            {
                mpMax *= 2;
            }

            if (p.IsPlMan)
            {
                if (server.AttributeKiActive)
                {
                    mpMax += CalculatePercent(mpMax, server.AttributeKiValue);
                }
            }

            // Hạng Whis: hạng 3 cộng vào HP tối đa, lỗi Java được giữ nguyên.
            IReadOnlyList<long> list = server.WhisPlayerIds;
            if (list.Count != 0 && list.Count > 2 && p.IsPl)
            {
                if (list.Count >= 5 && p.IsPl)
                {
                    int playerRank = GetPlayerRank(list, p.Id);
                    if (playerRank == 1)
                    {
                        mpMax += CalculatePercent(mpMax, 30);
                    }
                    else if (playerRank == 2)
                    {
                        mpMax += CalculatePercent(mpMax, 20);
                    }
                    else if (playerRank == 3)
                    {
                        HpMax += CalculatePercent(mpMax, 10);
                    }
                    else if (playerRank >= 4 && playerRank <= 5)
                    {
                        mpMax += CalculatePercent(mpMax, 5);
                    }
                    else if (playerRank >= 6 && playerRank <= 10)
                    {
                        mpMax += CalculatePercent(mpMax, 3);
                    }
                }
            }

            // peach juices
            if (it.IsUseRedPeach0)
            {
                mpMax -= CalculatePercent(mpMax, 99);
            }

            if (it.IsUseRedPeach)
            {
                mpMax += CalculatePercent(mpMax, 1);
            }

            if (it.IsUseRedPeach1)
            {
                mpMax += CalculatePercent(mpMax, 2);
            }

            if (it.IsUseRedPeach3)
            {
                mpMax += CalculatePercent(mpMax, 3);
            }

            if (it.IsUseRedPeach5)
            {
                mpMax += CalculatePercent(mpMax, 5);
            }

            if (it.IsUseRedPeach10)
            {
                mpMax += CalculatePercent(mpMax, 8);
            }

            if (it.IsUseRedPeach25)
            {
                mpMax += CalculatePercent(mpMax, 12);
            }

            if (it.IsUseRedPeach50)
            {
                mpMax += CalculatePercent(mpMax, 15);
            }

            if (it.IsUseRedPeach99)
            {
                mpMax += CalculatePercent(mpMax, 20);
            }

            if (it.IsUseRedPeach999)
            {
                mpMax += CalculatePercent(mpMax, 100);
            }

            if (it.IsUseEnergyTonic2)
            {
                mpMax = Nro.Core.Util.JavaNumbers.ToLong(mpMax * 2.2);
            }

            // option "reduce ki"
            mpMax -= mpMax * MpReductionPercent / 100;

            if (it.HasEnergyBoost30)
            {
                mpMax += CalculatePercent(mpMax, 30);
            }

            if (it.HasEnergyBoost15)
            {
                mpMax += CalculatePercent(mpMax, 15);
            }

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapCold && !IsColdImmune)
            {
                mpMax /= 2;
            }

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapChristMasEvent && !IsColdImmune)
            {
                mpMax /= 2;
            }

            if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMap5000NamTruoc)
            {
                mpMax -= CalculatePercent(mpMax, 90);
            }

            if (p.Gender == ConstPlayer.SAIYAN)
            {
                if (!p.IsBoss && !p.IsBotAny && zone is not null && zone.IsMapCereal)
                {
                    mpMax /= 2;
                }
            }

            // scroll (black ball war)
            if (zone is not null && zone.IsMapBlackBallWar)
            {
                mpMax *= facts.EffectSkin.HpKiMultiplier;
            }

            // grilled fish skewer
            if (facts.Player.UseFishSkewer)
            {
                mpMax += CalculatePercent(mpMax, 15);
            }

            MpMax = mpMax;
        }
    }

    // Hạ KI hiện tại về KI tối đa nếu đang vượt quá.
    private void SetMp()
    {
        if (Mp > MpMax)
        {
            Mp = MpMax;
        }
    }

    private static NPointMasterFacts RequireMaster(NPointFacts facts)
    {
        return facts.Master ?? throw new InvalidOperationException("The player is flagged as a body clone (Java: isPhanThan) but the host supplied no master; the Java cast ((PhanThan) player).master would have thrown.");
    }
}
