using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;

namespace Nro.Game.Players.Stats;

// Nhóm 1 của NPoint: dựng lại chỉ số từ trang bị.
// Nguồn Java: nro/player/NPoint.java (thứ tự là một phần kết quả: reset, quà Ngọc Rồng, thẻ ra-đa dùng, bông tai hợp thể trong túi, huy hiệu, vật phẩm mặc và đếm World Cup, giáp tập luyện rồi chỉ số cơ sở).
public partial class NPoint
{
    // Tính lại đệ tử trước để chỉ số của nó cấp cho HP/KI/sát thương hợp thể của chủ, rồi mới tính người chơi này.
    public void CalPoint()
    {
        Player? pet = Player.Pet;
        pet?.NPoint.SetPointWhenWearClothes();
        SetPointWhenWearClothes();
    }

    private void SetPointWhenWearClothes()
    {
        NPointFacts facts = Host.Capture(this);
        SetPointWhenWearClothes(facts);
        Host.Commit(this, facts);
    }

    private void SetPointWhenWearClothes(NPointFacts facts)
    {
        ResetPoint();
        long now = Now();
        long[] blackBall = facts.BlackBallTimeOut;
        if (blackBall[2] > now)
        {
            HpAbsorptionPercent += RewardBlackBall.R3S_1;
        }

        if (blackBall[3] > now)
        {
            ReflectDamagePercent += RewardBlackBall.R4S_2;
        }

        if (blackBall[4] > now)
        {
            CriticalDamagePercents.Add(RewardBlackBall.R5S_1);
            CriticalDamagePercent += RewardBlackBall.R5S_1;
        }

        if (blackBall[6] > now)
        {
            DodgePercent += RewardBlackBall.R7S_1;
        }

        NPointCard? card = facts.Cards.FirstOrDefault(r => r.Used == 1);
        if (card is not null)
        {
            foreach (NPointCardOption io in card.Options)
            {
                if (io.Active == card.Level || (card.Level == -1 && io.Active == 0))
                {
                    ApplyCardOption(io);
                }
            }
        }

        // Earring level 2
        if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_2)
        {
            ApplyEarring(facts.Inventory.ItemsBag, 921, facts);
        }

        // Earring level 3
        if (facts.TypeFusion == ConstPlayer.PORATA_FUSION_3)
        {
            ApplyEarring(facts.Inventory.ItemsBag, 1943, facts);
        }

        foreach (NPointOption io in facts.BadgeOptions)
        {
            AddOption(io, facts);
        }

        facts.SetClothes.Worldcup = 0;
        foreach (NPointItem item in facts.Inventory.ItemsBody)
        {
            if (item.IsNotNullItem)
            {
                switch (item.TemplateId)
                {
                    case 966:
                    case 982:
                    case 983:
                    case 883:
                    case 904:
                        facts.SetClothes.Worldcup++;
                        break;
                }

                if (item.TemplateId >= 592 && item.TemplateId <= 594)
                {
                    Teleport = true;
                }

                foreach (NPointOption io in item.ItemOptions!)
                {
                    AddOption(io, facts);
                }
            }
        }

        SetDameTrainArmor(facts);
        SetBasePoint(facts);
        SetOutfitFusion(facts);
    }

    // Dùng tùy chọn của vật phẩm túi đầu tiên có mã template; tùy chọn 72 là cấp bông tai.
    private void ApplyEarring(List<NPointItem> bag, int templateId, NPointFacts facts)
    {
        NPointItem? earring = bag.FirstOrDefault(it => it.IsNotNullItem && it.TemplateId == templateId);
        if (earring is not null)
        {
            foreach (NPointOption io in earring.ItemOptions!)
            {
                AddOption(io, facts);
                if (io.Id == 72)
                {
                    EarringLevel = io.Param;
                }
            }
        }
    }

    // Áp dụng bảng tùy chọn thẻ ra-đa; không gộp với AddOption vì Java có hai bảng khác nhau, bảng thẻ không có các mã 25,26,32,38,42-46,110,111,115,159,160,176,197-204,256.
    private void ApplyCardOption(NPointCardOption io)
    {
        unchecked
        {
            switch (io.Id)
            {
                case 0: //Attack +#
                    DamageBonus += io.Param;
                    break;
                case 2: //HP, KI +#000
                    HpBonus += io.Param * 1000;
                    MpBonus += io.Param * 1000;
                    break;
                case 3: // palm immunity
                    BlastNullification += io.Param;
                    break;
                case 5: //+#% critical damage
                    CriticalDamagePercents.Add(io.Param);
                    CriticalDamagePercent += io.Param;
                    break;
                case 6: //HP+#
                    HpBonus += io.Param;
                    break;
                case 7: //KI+#
                    MpBonus += io.Param;
                    break;
                case 8: //drain #% HP, KI around every 5 seconds
                    AroundHpMpAbsorptionPercent += io.Param;
                    break;
                case 10:
                    TrueDamagePercent += io.Param;
                    break;
                case 14: //Critical +#%
                case 192:
                    CriticalChanceBonus += io.Param;
                    break;
                case 16: // Speed
                case 114:
                case 148:
                    SpeedPercent.Add(io.Param);
                    break;
                case 18: //Accuracy
                    AccuracyPercent += io.Param;
                    break;
                case 19: //Attack +#% against monsters
                    MobAttackDamagePercents.Add(io.Param);
                    break;
                case 22: //HP+#K
                    HpBonus += io.Param * 1000;
                    break;
                case 23: //MP+#K
                    MpBonus += io.Param * 1000;
                    break;
                case 24:
                    WearingBuiBui = true;
                    break;
                case 27: //+# HP/30s
                    HpRegenBonus += io.Param;
                    break;
                case 28: //+# KI/30s
                    MpRegenBonus += io.Param;
                    break;
                case 29:
                    WearingMabu = true;
                    break;
                case 33: //teleport
                    Teleport = true;
                    break;
                case 34:
                    StarSealSetCount += 1;
                    break;
                case 35:
                    NguyetSealSetCount += 1;
                    break;
                case 36:
                    NhatSealSetCount += 1;
                    break;
                case 47: //Defence +#
                    DefenseBonus += io.Param;
                    break;
                case 48: //HP/KI +#
                    HpBonus += io.Param;
                    MpBonus += io.Param;
                    break;
                case 49: //Attack +#%
                case 50: //Attack power +#%
                    DamagePercents.Add(io.Param);
                    break;
                case 77: //HP+#%
                    HpPercents.Add(io.Param);
                    break;
                case 80: //HP+#%/30s
                    HpRegenPercent += io.Param;
                    break;
                case 81: //MP+#%/30s
                    MpRegenPercent += io.Param;
                    break;
                case 83:
                    Grants20ExperienceOnMobAttack = true;
                    break;
                case 88: //+#% exp when hitting monsters
                    PowerPotentialPercents.Add(io.Param);
                    break;
                case 94: //Defence #%
                    ArmorPercent += io.Param;
                    break;
                case 95: //turn #% attack into HP
                    HpAbsorptionPercent += io.Param;
                    break;
                case 96: //turn #% attack into MP
                    MpAbsorptionPercent += io.Param;
                    break;
                case 97: //reflect #% damage
                    ReflectDamagePercent += io.Param;
                    break;
                case 98: //armour piercing (palm)
                    RangedArmorPenetrationPercent += io.Param;
                    break;
                case 99: //armour piercing (melee)
                    MeleeArmorPenetrationPercent += io.Param;
                    break;
                case 100: //+#% gold from monsters
                    GoldPercent += io.Param;
                    break;
                case 101: //+#% potential and power
                    PowerPotentialPercents.Add(io.Param);
                    break;
                case 103: //KI +#%
                    MpPercents.Add(io.Param);
                    break;
                case 104: //turn #% attack on monsters into HP
                    MobHpAbsorptionPercent += io.Param;
                    break;
                case 105: //invisible when not attacking monsters and bosses
                    WearingInvisibilityArmor = true;
                    break;
                case 106: //not affected by the cold
                    IsColdImmune = true;
                    break;
                case 108: //#% dodge
                    DodgePercent += io.Param;
                    break;
                case 109: //smelly, lose #% HP
                    PercentDirtyHpLoss += io.Param;
                    break;
                case 116: //resists the stun skill
                    IsSolarFlareResistant = true;
                    break;
                case 153:
                    BombPercent += io.Param;
                    break;
                case 117: //Handsome +#% attack for self and nearby players
                    if (io.Param > SexyDamagePercent)
                    {
                        SexyDamagePercent = io.Param;
                    }

                    break;
                case 147: //+#% attack
                    DamagePercents.Add(io.Param);
                    break;
                case 155: //-50% attack, HP, KI and +#% potential, gold from monsters
                    DamageReductionPercent += 50;
                    HpReductionPercent += 50;
                    MpReductionPercent += 50;
                    PowerPotentialPercents.Add(io.Param);
                    GoldPercent += io.Param;
                    break;
                case 156:
                    StackingDamagePercent += io.Param;
                    break;
                case 157:
                    LowMpDamageReductionPercent += io.Param;
                    break;
                case 162: //Cute restores #% KI/s for self and around
                    CuteMpRegen += io.Param;
                    break;
                case 163:
                    IsPumpkinAura = true;
                    break;
                case 173: //restores #% HP and KI of allies
                    SelfAndTeamHpRegenPercent += io.Param;
                    SelfAndTeamMpRegenPercent += io.Param;
                    break;
                case 236: //luck
                    LuckPercent += io.Param;
                    break;
                case 258:
                    if (io.Param > CoolDamagePercent)
                    {
                        CoolDamagePercent = io.Param;
                    }

                    break;
                case 259: //HP+#%/10s
                    SelfAndAllyHpRegenPercent += io.Param;
                    break;
                case 226:
                    if (io.Param > CuteDamagePercent)
                    {
                        CuteDamagePercent = io.Param;
                    }

                    break;
                case 227:
                    ControlResistancePercent += io.Param;
                    break;
            }
        }
    }

    // Áp dụng bảng tùy chọn chính cho mọi tùy chọn của vật phẩm mặc, huy hiệu và bông tai.
    private void AddOption(NPointOption io, NPointFacts facts)
    {
        unchecked
        {
            switch (io.Id)
            {
                case 0: //Attack +#
                    DamageBonus += io.Param;
                    break;
                case 2: //HP, KI +#000
                    HpBonus += io.Param * 1000;
                    MpBonus += io.Param * 1000;
                    break;
                case 3: // palm immunity
                    BlastNullification += io.Param;
                    break;
                case 5: //+#% critical damage
                    CriticalDamagePercents.Add(io.Param);
                    CriticalDamagePercent += io.Param;
                    break;
                case 6: //HP+#
                    HpBonus += io.Param;
                    break;
                case 7: //KI+#
                    MpBonus += io.Param;
                    break;
                case 8: //drain #% HP, KI around every 5 seconds
                    AroundHpMpAbsorptionPercent += io.Param;
                    break;
                case 10:
                    TrueDamagePercent += io.Param;
                    break;
                case 14: //Critical +#%
                case 192:
                    CriticalChanceBonus += io.Param;
                    break;
                case 16: // Speed
                case 114:
                case 148:
                    SpeedPercent.Add(io.Param);
                    break;
                case 18: //Accuracy
                    AccuracyPercent += io.Param;
                    break;
                case 19: //Attack +#% against monsters
                    MobAttackDamagePercents.Add(io.Param);
                    break;
                case 22: //HP+#K
                    HpBonus += io.Param * 1000;
                    break;
                case 23: //MP+#K
                    MpBonus += io.Param * 1000;
                    break;
                case 24: //slow
                    WearingBuiBui = true;
                    break;
                case 25: //invisible
                    IsInvisible = true;
                    break;
                case 26: //petrify
                    IsPetrified = true;
                    break;
                case 27: //+# HP/30s
                    HpRegenBonus += io.Param;
                    break;
                case 28: //+# KI/30s
                    MpRegenBonus += io.Param;
                    break;
                case 29:
                    WearingMabu = true;
                    break;
                case 32:
                    IsBoneTransformationImmune = true;
                    break;
                case 33: //teleport
                    Teleport = true;
                    break;
                case 34:
                    StarSealSetCount += 1;
                    break;
                case 35:
                    NguyetSealSetCount += 1;
                    break;
                case 36:
                    NhatSealSetCount += 1;
                    break;
                case 38:
                    IsFusionActive = true;
                    break;
                case 42:
                    FlyingMobDamagePercent += io.Param;
                    break;
                case 43:
                    MonkeyMobDamagePercent += io.Param;
                    break;
                case 44:
                    RunningMobDamagePercent += io.Param;
                    break;
                case 45:
                    NamekianAttackPercent += io.Param;
                    break;
                case 46:
                    EarthlingAttackPercent += io.Param;
                    break;
                case 47: //Defence +#
                    DefenseBonus += io.Param;
                    break;
                case 48: //HP/KI +#
                    HpBonus += io.Param;
                    MpBonus += io.Param;
                    break;
                case 49: //Attack +#%
                case 50: //Attack power +#%
                    DamagePercents.Add(io.Param);
                    break;
                case 77: //HP+#%
                    HpPercents.Add(io.Param);
                    break;
                case 80: //HP+#%/30s
                    HpRegenPercent += io.Param;
                    break;
                case 81: //MP+#%/30s
                    MpRegenPercent += io.Param;
                    break;
                case 83:
                    Grants20ExperienceOnMobAttack = true;
                    break;
                case 88: //+#% exp when hitting monsters
                    PowerPotentialPercents.Add(io.Param);
                    break;
                case 94: //Defence #%
                    ArmorPercent += io.Param;
                    break;
                case 95: //turn #% attack into HP
                    HpAbsorptionPercent += io.Param;
                    break;
                case 96: //turn #% attack into MP
                    MpAbsorptionPercent += io.Param;
                    break;
                case 97: //reflect #% damage
                    ReflectDamagePercent += io.Param;
                    break;
                case 98: //armour piercing (palm)
                    RangedArmorPenetrationPercent += io.Param;
                    break;
                case 99: //armour piercing (melee)
                    MeleeArmorPenetrationPercent += io.Param;
                    break;
                case 100: //+#% gold from monsters
                    GoldPercent += io.Param;
                    break;
                case 101: //+#% potential and power
                    PowerPotentialPercents.Add(io.Param);
                    break;
                case 103: //KI +#%
                    MpPercents.Add(io.Param);
                    break;
                case 104: //turn #% attack on monsters into HP
                    MobHpAbsorptionPercent += io.Param;
                    break;
                case 105: //invisible when not attacking monsters and bosses
                    WearingInvisibilityArmor = true;
                    break;
                case 106: //not affected by the cold
                    IsColdImmune = true;
                    break;
                case 108: //#% dodge
                    DodgePercent += io.Param;
                    break;
                case 109: //smelly, lose #% HP
                    PercentDirtyHpLoss += io.Param;
                    break;
                case 110: //spl outfit
                    HasCrystalStarEquipment = true;
                    break;
                case 111: //distraction
                    IsXinbato = true;
                    break;
                case 115: //carrot transformation
                    IsRabbitLeader = true;
                    break;
                case 116: //resists the stun skill
                    IsSolarFlareResistant = true;
                    break;
                case 117: //Handsome +#% attack for self and nearby players
                    if (io.Param > SexyDamagePercent)
                    {
                        SexyDamagePercent = io.Param;
                    }

                    break;
                case 147: //+#% attack
                    DamagePercents.Add(io.Param);
                    break;
                case 155: //-50% attack, HP, KI and +#% potential, gold from monsters
                    DamageReductionPercent += 50;
                    HpReductionPercent += 50;
                    MpReductionPercent += 50;
                    PowerPotentialPercents.Add(io.Param);
                    GoldPercent += io.Param;
                    break;
                case 156:
                    StackingDamagePercent += io.Param;
                    break;
                case 157:
                    LowMpDamageReductionPercent += io.Param;
                    break;
                case 162: //Cute restores #% KI/s for self and around
                    CuteMpRegen += io.Param;
                    break;
                case 159: // palm multiplier
                    BlastDamageMultiplier = io.Param;
                    break;
                case 160: // pet potential
                    PetPowerPotentialPercent += io.Param;
                    break;
                case 173: //restores #% HP and KI of allies
                    SelfAndTeamHpRegenPercent += io.Param;
                    SelfAndTeamMpRegenPercent += io.Param;
                    break;
                case 176:
                    SetInfoOption176(facts);
                    break;
                case 197:
                    SaiyanAttackPercent += io.Param;
                    break;
                case 198:
                    EarthlingDamageReductionPercent += io.Param;
                    break;
                case 199:
                    NamekianDamageReductionPercent += io.Param;
                    break;
                case 200:
                    SaiyanDamageReductionPercent += io.Param;
                    break;
                case 201:
                    ClanDamagePercent += io.Param;
                    break;
                case 202:
                    ClanHpPercent += io.Param;
                    break;
                case 203:
                    ClanMpPercent += io.Param;
                    break;
                case 204:
                    BossDamagePercent += io.Param;
                    break;
                case 153: //% explode after death
                    BombPercent += io.Param;
                    break;
                case 163:
                    IsPumpkinAura = true;
                    break;
                case 256: //founder
                    IsFounder = true;
                    break;
                case 236: //luck
                    LuckPercent += io.Param;
                    break;
                case 258:
                    if (io.Param > CoolDamagePercent)
                    {
                        CoolDamagePercent = io.Param;
                    }

                    break;
                case 259: //HP+#%/10s
                    SelfAndAllyHpRegenPercent += io.Param;
                    break;
                case 226:
                    if (io.Param > CuteDamagePercent)
                    {
                        CuteDamagePercent = io.Param;
                    }

                    break;
                case 227:
                    ControlResistancePercent += io.Param;
                    break;
            }
        }
    }

    // Cộng từng thưởng tốc độ theo phần trăm của tốc độ hiện thời; mỗi bước ép về byte nên thưởng cộng dồn và có thể tràn, người bị chậm kết thúc ở 1.
    private void SetSpeed(NPointFacts facts)
    {
        foreach (int tl in SpeedPercent)
        {
            Speed = unchecked((sbyte)(Speed + CalculatePercent(Speed, tl)));
        }

        if (facts.EffectSkin.IsSlow)
        {
            Speed = 1;
        }
    }

    // Cộng 10% sát thương cho người chơi thực; phép chia nguyên 50/100 bằng 0 nên Java luôn ghi tốc độ 5, đúng giá trị ResetPoint để lại và thực tế không có tác dụng trước SetSpeed.
    private void SetInfoOption176(NPointFacts facts)
    {
        if (facts.Player.IsPl)
        {
            DamagePercents.Add(10);
            Speed = unchecked((sbyte)(5 + (3 * (50 / 100))));
        }
    }

    // Nhận diện trang phục hợp thể Gogeta khi người chơi và đệ tử mặc cặp Super Saiyan Blue 1693 và 1553, không phụ thuộc thứ tự.
    private void SetOutfitFusion(NPointFacts facts)
    {
        List<NPointItem> body = facts.Inventory.ItemsBody;
        NPointPetFacts? pet = facts.Pet;
        if (body.Count < 6 || pet is null || pet.BodyItemCount < 6)
        {
            return;
        }

        NPointItem skin = body[5];
        NPointItem? pskin = pet.BodySkin;
        if (skin.IsNotNullItem && pskin is not null && pskin.IsNotNullItem)
        {
            IsGogeta = (skin.TemplateId == 1693 && pskin.TemplateId == 1553) || (skin.TemplateId == 1553 && pskin.TemplateId == 1693);
        }
        else
        {
            IsGogeta = false;
        }
    }

    // Với người chơi thực, giáp tập luyện ô 6 giảm sát thương theo phần trăm, còn giáp loại 32 trong túi có tùy chọn 9 dương tăng sát thương; đặt Inventory.TrainingArmor để host ghi trả. Lỗi được ghi log và nuốt như catch Java.
    private void SetDameTrainArmor(NPointFacts facts)
    {
        if (!facts.Player.IsPl)
        {
            return;
        }

        NPointInventoryFacts inventory = facts.Inventory;
        if (inventory.ItemsBody.Count < 7)
        {
            return;
        }

        try
        {
            NPointItem gtl = inventory.ItemsBody[6];
            if (gtl.IsNotNullItem)
            {
                WearingTrainArmor = true;
                inventory.TrainingArmor = gtl;
                DamageReductionPercent = unchecked(DamageReductionPercent + gtl.TrainArmorPercent);
            }
            else
            {
                if (inventory.TrainingArmor is null)
                {
                    NPointItem? found = inventory.ItemsBag.FirstOrDefault(item => item.IsNotNullItem
                        && item.TemplateType == 32
                        && item.ItemOptions is not null
                        && item.ItemOptions.Any(io => io.Id == 9 && io.Param > 0));
                    if (found is null)
                    {
                        return;
                    }

                    inventory.TrainingArmor = found;
                }

                WearingTrainArmor = false;
                NPointItem? armor = inventory.TrainingArmor;
                if (armor.ItemOptions is not null)
                {
                    foreach (NPointOption io in armor.ItemOptions)
                    {
                        if (io.Id == 9 && io.Param > 0)
                        {
                            DamagePercents.Add(armor.TrainArmorPercent);
                            break;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Logger.Error("Lỗi get giáp tập luyện " + facts.Player.Name + "\n" + e + "\n");
        }
    }

    // Tính lại mọi chỉ số dẫn xuất từ chỉ số cơ sở và tổng tùy chọn hiện có, không đặt lại tổng tùy chọn.
    public void SetBasePoint()
    {
        NPointFacts facts = Host.Capture(this);
        SetBasePoint(facts);
        Host.Commit(this, facts);
    }

    private void SetBasePoint(NPointFacts facts)
    {
        SetHpMax(facts);
        SetHp();
        SetMpMax(facts);
        SetMp();
        SetDamage(facts);
        SetDef(facts);
        SetCrit(facts);
        SetCriticalDamage(facts);
        SetHpRegen(facts);
        SetMpRegen(facts);
        SetHpAbsorption(facts);
        SetMpAbsorption(facts);
        SetRabbitBulma(facts);
        SetBeachParty(facts);
        SetBulmaColoredHair(facts);
        SetSealSets();
        SetSpeed(facts);
        SetOptions(facts);
    }

    // Giữ lỗi biên Java: kiểm size>=5 nhưng đọc chỉ số 5 nên đúng 5 phần tử sẽ ném lỗi; người chơi luôn có 12 ô mặc nên không xảy ra khi chơi.
    private void SetRabbitBulma(NPointFacts facts)
    {
        List<NPointItem> body = facts.Inventory.ItemsBody;
        IsRabbitBulma = body.Count >= 5 && body[5].IsNotNullItem && body[5].TemplateId == 584;
    }

    // Áp dụng Bunma tóc màu với cùng lỗi chỉ số biên như SetRabbitBulma.
    private void SetBulmaColoredHair(NPointFacts facts)
    {
        List<NPointItem> body = facts.Inventory.ItemsBody;
        IsBulmaColoredHair = body.Count >= 5 && body[5].IsNotNullItem && body[5].TemplateId >= 1208 && body[5].TemplateId <= 1210;
    }

    // Áp dụng Tiệc bãi biển với cùng lỗi chỉ số biên như SetRabbitBulma.
    private void SetBeachParty(NPointFacts facts)
    {
        List<NPointItem> body = facts.Inventory.ItemsBody;
        IsBeachParty = body.Count >= 5 && body[5].IsNotNullItem && body[5].TemplateId >= 1234 && body[5].TemplateId <= 1236;
    }

    // Hoàn tất bộ ấn Tinh Nhật Nguyệt khi mặc đủ năm món.
    private void SetSealSets()
    {
        HasStarSeal = StarSealSetCount >= 5;
        HasNhatSeal = NhatSealSetCount >= 5;
        HasNguyetSeal = NguyetSealSetCount >= 5;
    }

    // Kiểm tra đủ năm ấn sao (Java: hasFull5TinhAn; nghĩa chưa chắc, cần chủ dự án duyệt).
    private bool HasFullFiveStarSeal() => StarSealSetCount >= 5;

    private bool HasFull5NguyetAn() => NguyetSealSetCount >= 5;

    private bool HasFull5NhatAn() => NhatSealSetCount >= 5;

    // Đếm vật phẩm đang mặc có tùy chọn; phương thức private này không được Java gọi.
    internal static int CountItemsHaveAn(int optionId, NPointFacts facts)
    {
        int count = 0;
        foreach (NPointItem item in facts.Inventory.ItemsBody)
        {
            if (!item.IsNotNullItem || item.ItemOptions is null)
            {
                continue;
            }

            if (item.ItemOptions.Any(io => io.Id == optionId))
            {
                count++;
            }
        }

        return count;
    }

    // Đặt lại chỉ số tạm; cố ý không đặt lại WearingTrainArmor, EarringLevel, HP, MP và chỉ số cơ sở như Java.
    private void ResetPoint()
    {
        BlastNullification = 0;
        HpBonus = 0;
        MpBonus = 0;
        DamageBonus = 0;
        DefenseBonus = 0;
        CriticalChanceBonus = 0;
        HpPercents.Clear();
        MpPercents.Clear();
        DefensePercents.Clear();
        DamagePercents.Clear();
        CriticalDamagePercents.Clear();
        MobAttackDamagePercents.Clear();
        CriticalDamagePercent = 0;
        SelfAndTeamHpRegenPercent = 0;
        SelfAndTeamMpRegenPercent = 0;
        HpRegen = 0;
        MpRegen = 0;
        CuteMpRegen = 0;
        HpRegenPercent = 0;
        SelfAndAllyHpRegenPercent = 0;
        MpRegenPercent = 0;
        HpAbsorptionPercent = 0;
        MpAbsorptionPercent = 0;
        MobHpAbsorptionPercent = 0;
        AroundHpMpAbsorptionPercent = 0;
        ReflectDamagePercent = 0;
        FlyingMobDamagePercent = 0;
        MonkeyMobDamagePercent = 0;
        RunningMobDamagePercent = 0;
        PowerPotentialPercents.Clear();
        MobAttackDamagePercents.Clear();
        GoldPercent = 0;
        DodgePercent = 0;
        LuckPercent = 0;
        NamekianDamageReductionPercent = 0;
        EarthlingDamageReductionPercent = 0;
        SaiyanDamageReductionPercent = 0;
        NamekianAttackPercent = 0;
        EarthlingAttackPercent = 0;
        SaiyanAttackPercent = 0;
        ClanDamagePercent = 0;
        ClanHpPercent = 0;
        ClanMpPercent = 0;
        BossDamagePercent = 0;
        BombPercent = 0;
        ArmorPercent = 0;
        MeleeArmorPenetrationPercent = 0;
        RangedArmorPenetrationPercent = 0;
        TrueDamagePercent = 0;
        AccuracyPercent = 0;
        LowMpDamageReductionPercent = 0;
        StackingDamagePercent = 0;
        PetPowerPotentialPercent = 0;
        BlastDamageMultiplier = 0;
        IsFounder = false;
        StarSealSetCount = 0;
        NhatSealSetCount = 0;
        NguyetSealSetCount = 0;
        SexyDamagePercent = 0;
        CuteDamagePercent = 0;
        CoolDamagePercent = 0;
        DamageReductionPercent = 0;
        HpReductionPercent = 0;
        MpReductionPercent = 0;
        PercentDirtyHpLoss = 0;
        ControlResistancePercent = 0;
        SpeedPercent.Clear();
        Speed = 5;
        IsBoneTransformationImmune = false;
        Teleport = false;
        WearingInvisibilityArmor = false;
        IsColdImmune = false;
        IsPumpkinAura = false;
        IsSolarFlareResistant = false;
        IsInvisible = false;
        IsPetrified = false;
        WearingBuiBui = false;
        WearingMabu = false;
        HasCrystalStarEquipment = false;
        IsXinbato = false;
        IsRabbitLeader = false;
        IsRabbitBulma = false;
        IsBulmaColoredHair = false;
        IsBeachParty = false;
        Grants20ExperienceOnMobAttack = false;
    }
}
