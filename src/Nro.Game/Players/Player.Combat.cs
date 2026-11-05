using Nro.Core.Util;
using Nro.Game.Players.Ports;
using Nro.Game.Skills;

namespace Nro.Game.Players;

// Nhận sát thương, tính sát thương cuối và ghi đối thủ tạm thời.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 2144–2478).
// Zone actor là chủ sở hữu luồng: không tái tạo synchronized của Java và không dùng khóa ở aggregate này.
public partial class Player
{
    // Nhận sát thương và trả đúng lượng sát thương sau mọi nhánh giảm/chặn của Java (Java: injured).
    public double Injured(Player? attacker, double damage, bool piercing, bool isMobAttack, IPlayerCombatGateway combat, IPlayerRandom random, IPlayerClock clock)
    {
        ArgumentNullException.ThrowIfNull(combat);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(clock);

        if (IsDead())
        {
            return 0;
        }

        if (attacker is not null && !ReferenceEquals(attacker, this))
        {
            SetTemporaryEnemies(attacker);
        }

        if (combat.IsHalloweenEvent && attacker is not null && IsPet && Owner is not null && Owner.Id == attacker.Id
            && !EffectSkill.IsHalloween && !NPoint.IsBoneTransformationImmune)
        {
            combat.SetHalloween(this, random.NextInt(0, 2), 1_800_000);
        }

        if (attacker is not null && IsMabuDamageCapSkill(attacker) && !attacker.IsBoss && combat.IsMabuTwelveHourMap(this))
        {
            damage = CrisGH(damage > NPoint.HpMax / 20 ? NPoint.HpMax / 20 : damage, combat);
        }

        if (combat.GetBrolyLevel(this) == 5)
        {
            damage -= CrisGH(damage / 100 * 80, combat);
        }

        if (attacker is not null && attacker.EffectSkill.IsBottled && !CanDoWithTime(clock.NowMilliseconds, attacker.EffectSkill.LastTimeBottled, 3_000))
        {
            return 0;
        }

        if (attacker is not null && attacker.IsBoss)
        {
            EffectSkin.IsInvisible = false;
            EffectSkin.LastTimeInvisible = clock.NowMilliseconds;
        }

        if (attacker is not null)
        {
            if (NPoint.EarthlingAttackPercent > 0 && Gender == 0)
            {
                damage += CrisGH(damage / 100 * NPoint.EarthlingAttackPercent, combat);
            }

            if (NPoint.NamekianAttackPercent > 0 && Gender == 1)
            {
                damage += CrisGH(damage / 100 * NPoint.NamekianAttackPercent, combat);
            }

            if (NPoint.SaiyanAttackPercent > 0 && Gender == 2)
            {
                damage += CrisGH(damage / 100 * NPoint.SaiyanAttackPercent, combat);
            }
        }

        if (attacker is not null && IsBeamSkill(attacker) && NPoint.BlastNullification > 0)
        {
            combat.RestoreHp(this, CrisGH(damage * NPoint.BlastNullification / 100, combat));
            return 0;
        }

        int armorPercent = NPoint.ArmorPercent;
        int dodgePercent = NPoint.DodgePercent;
        long now = clock.NowMilliseconds;
        if (attacker is not null && IsStackingDamageSkill(attacker))
        {
            if (attacker.DamageBonusPercent < attacker.NPoint.StackingDamagePercent)
            {
                attacker.DamageBonusPercent++;
            }

            if (attacker.NPoint.StackingDamagePercent > 0)
            {
                attacker.LastTimeAttack = now;
            }

            damage += CrisGH(damage / 100 * attacker.DamageBonusPercent, combat);
        }

        if (attacker is not null && !isMobAttack && HasSelectedSkill(attacker))
        {
            dodgePercent = IsAccurateSkill(attacker) ? dodgePercent - attacker.NPoint.AccuracyPercent : 0;
        }

        if (piercing)
        {
            armorPercent = 0;
        }

        if (dodgePercent > 90)
        {
            dodgePercent = 90;
        }

        if (armorPercent > 86)
        {
            armorPercent = 86;
        }

        if (IsTrue(random, dodgePercent, 100))
        {
            return 0;
        }

        int armorPenetrationPercent = 0;
        int trueDamagePercent = 0;
        if (attacker is not null)
        {
            trueDamagePercent = attacker.NPoint.TrueDamagePercent;
            armorPenetrationPercent = combat.IsBlastSkill(attacker)
                ? attacker.NPoint.RangedArmorPenetrationPercent
                : attacker.NPoint.MeleeArmorPenetrationPercent;
        }

        damage = CalculateFinalDamage(CrisGH(damage, combat), armorPercent, armorPenetrationPercent, trueDamagePercent, combat);
        if (NPoint.LowMpDamageReductionPercent > 0 && NPoint.Mp < NPoint.MpMax * 0.2)
        {
            damage -= CrisGH(damage / 100 * NPoint.LowMpDamageReductionPercent, combat);
        }

        damage = ReduceDamageByAttackerGender(damage, attacker, combat);

        if (attacker is not null && attacker.IsPl() && MaBuHold is not null && combat.IsMabuHoldMap(this))
        {
            PrecentMabuHold++;
            damage = 1;
        }

        if (attacker is not null && attacker.IdNRNM != -1 && (IsBoss || IsPetFollow || IsTangMonk))
        {
            return 1;
        }

        if (attacker is not null && (attacker.IdNRNM != -1 || IdNRNM != -1) && combat.AreSameClan(attacker, this))
        {
            combat.SendPrivateChat(attacker, this, "Ê cùng bang mà");
            return 0;
        }

        if (attacker is not null && attacker.EffectSkill.IsXinbato && IsTrue(random, 20, 100))
        {
            return 0;
        }

        if (!piercing && EffectSkill.IsShielding)
        {
            if (damage > NPoint.HpMax)
            {
                combat.BreakShield(this);
            }

            damage = 1;
        }

        if (isMobAttack && combat.IsImmortalCharmActive(this, clock.NowMilliseconds) && damage >= NPoint.Hp)
        {
            damage = NPoint.Hp - 1;
        }

        if (HasReviveGracePeriod(clock.NowMilliseconds))
        {
            return 0;
        }

        if (!piercing && attacker is null && isMobAttack && (combat.IsImmortalCharmActive(this, clock.NowMilliseconds) || EffectSkill.IsHalloween)
            && damage >= NPoint.Hp)
        {
            damage = NPoint.Hp - 1;
        }

        bool usesArmorItem = IsAccurateSkill(attacker) || isMobAttack;
        if (usesArmorItem)
        {
            if (ItemTime.IsUseXenArmor && !ItemTime.IsUseXenArmor2)
            {
                damage /= 2;
            }

            if (ItemTime.IsUseXenArmor2)
            {
                damage = damage / 100 * 40;
            }
        }

        if (!piercing && EffectSkill.IsShielding && !isMobAttack)
        {
            combat.SetShieldReflectDamage(this, CrisGH(damage, combat));
            if (damage > NPoint.HpMax)
            {
                combat.BreakShield(this);
            }

            damage = combat.IsDungeonMap(this) ? 10 : 1;
        }

        if (attacker is not null && NPoint.IsFounder)
        {
            combat.SendNotification(attacker, "Không thể tấn công! Vì người này là nhà sáng lập game!");
            return 0;
        }

        combat.RecordDamageTakenOnMap51(this, damage);
        if (damage >= NPoint.Hp && combat.HandleMartialArtCongressDeath(this))
        {
            LostByDeath = true;
            return 0;
        }

        if (combat.HasFriezaLeaderCostume(this) && attacker is not null && combat.HasFriezaMinionCostume(attacker) && attacker.EffectSkill.IsFideLeader)
        {
            damage = 1;
        }

        NPoint.SubHP(CrisGH(damage, combat));
        if ((attacker is not null || isMobAttack) && IsDead() && !IsBoss && !IsPetFollow && !IsTangMonk)
        {
            HandleDeathAfterDamage(attacker, combat, random, clock);
        }

        return damage;
    }

    // Tính sát thương sau giáp, xuyên giáp và sát thương chuẩn (Java: calculateFinalDamage).
    public double CalculateFinalDamage(double baseDamage, int armorPercent, int armorPenetrationPercent, int trueDamagePercent, IPlayerCombatGateway combat)
    {
        ArgumentNullException.ThrowIfNull(combat);

        int effectiveArmor = armorPercent - armorPenetrationPercent;
        if (effectiveArmor < 0)
        {
            int bonus = Math.Min((-effectiveArmor) / 2, 20);
            effectiveArmor = 0;
            baseDamage += CrisGH(baseDamage, combat) * bonus / 100;
        }

        double finalDamage = CrisGH(baseDamage, combat) * (100 - effectiveArmor) / 100;
        if (trueDamagePercent > 0)
        {
            finalDamage += CrisGH(baseDamage, combat) * trueDamagePercent / 100;
        }

        return Math.Max(1, finalDamage);
    }

    // Thêm người tấn công vào mọi danh sách đối thủ tạm thời (Java: setTemporaryEnemies).
    public void SetTemporaryEnemies(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        List<Player>[] lists = [TemporaryEnemies, FatherTemporaryEnemies, MotherTemporaryEnemies, LoverTemporaryEnemies, ChildOneTemporaryEnemies, ChildTwoTemporaryEnemies, ChildThreeTemporaryEnemies];
        foreach (List<Player> list in lists)
        {
            if (!list.Contains(player))
            {
                list.Add(player);
            }
        }
    }

    // Xử lý rơi vật phẩm và handoff chết sau khi HP đã về 0 (Java: injured).
    private void HandleDeathAfterDamage(Player? attacker, IPlayerCombatGateway combat, IPlayerRandom random, IPlayerClock clock)
    {
        if (attacker is not null && CanDropMonkeyTail(attacker, combat, clock))
        {
            combat.DropCombatItem(this, 579, 1, attacker);
            LastTimeDropTail = clock.NowMilliseconds;
            combat.MarkTailDropTimeDirty(this);
            combat.SendNotification(attacker, "Rớt đuôi khỉ kìa");
            combat.SendNotification(this, "Bạn đã bị hạ và rớt đuôi khỉ, trong vòng 1 giờ sẽ không rớt đuôi khỉ nữa");
        }

        if (attacker is not null && combat.CanDropDeathItems(this))
        {
            if (EffectSkill.IsChocolate)
            {
                combat.DropCombatItem(this, 516, 1, attacker);
            }

            if (EffectSkin.IsRabbitLeader)
            {
                combat.DropCombatItem(this, 670, 1, attacker);
            }
        }

        if (IsTrue(random, NPoint.BombPercent, 100))
        {
            combat.TriggerBombDeath(this, attacker);
        }
        else
        {
            combat.TriggerPlayerDeath(this, attacker);
        }
    }

    // Kiểm tra điều kiện Java rơi đuôi khỉ, ngoại trừ task do combat gateway chụp (Java: injured).
    private bool CanDropMonkeyTail(Player attacker, IPlayerCombatGateway combat, IPlayerClock clock) => IsPl() && attacker.Gender == 0 && IsMonkeyTailDropSkill(attacker)
        && EffectSkill.IsMonkey && !IsEventGasActive && CanDoWithTime(clock.NowMilliseconds, LastTimeDropTail, TIME_TRUNG_THU) && combat.CanDropMonkeyTail(this, attacker);

    // Kiểm tra toàn bộ grace period hồi sinh của gia đình (Java: injured).
    private bool HasReviveGracePeriod(long now) => !CanDoWithTime(now, LastTimeRevived, 1_500)
        || !CanDoWithTime(now, LastFatherRevivedTime, 1_500)
        || !CanDoWithTime(now, LastMotherRevivedTime, 1_500)
        || !CanDoWithTime(now, LastLoverRevivedTime, 1_500)
        || !CanDoWithTime(now, LastChildOneRevivedTime, 1_500)
        || !CanDoWithTime(now, LastChildTwoRevivedTime, 1_500)
        || !CanDoWithTime(now, LastChildThreeRevivedTime, 1_500);

    // Áp dụng kháng sát thương theo giới tính người tấn công, cắt phần trăm ở 99 như Java (Java: injured).
    private double ReduceDamageByAttackerGender(double damage, Player? attacker, IPlayerCombatGateway combat)
    {
        if (attacker is null)
        {
            return damage;
        }

        int reduction = attacker.Gender switch
        {
            0 => Math.Min(NPoint.EarthlingDamageReductionPercent, 99),
            1 => Math.Min(NPoint.NamekianDamageReductionPercent, 99),
            2 => Math.Min(NPoint.SaiyanDamageReductionPercent, 99),
            _ => 0,
        };
        return reduction > 0 ? damage - CrisGH(damage / 100 * reduction, combat) : damage;
    }

    // Giữ quy tắc thời gian strict lớn hơn Java Util.canDoWithTime.
    private static bool CanDoWithTime(long now, long lastTime, long duration) => unchecked(now - lastTime) > duration;

    // Cắt double theo cờ Manager.readInt của Java (Java: Util.CrisGH).
    private static long CrisGH(double value, IPlayerCombatGateway combat) => Util.CrisGH(value, combat.ReadInt);

    // Giữ đủ guard và một lần nextInt(typeRatio) của Java Util.isTrue (Java: Util.isTrue).
    private static bool IsTrue(IPlayerRandom random, int ratio, int typeRatio)
    {
        if (typeRatio <= 0 || ratio <= 0)
        {
            return false;
        }

        return ratio >= typeRatio || random.NextInt(0, typeRatio - 1) < ratio;
    }

    // Kiểm tra ba chiêu hoàn HP khi vô hiệu chưởng (Java: KAMEJOKO/MASENKO/ANTOMIC).
    private static bool IsBeamSkill(Player player) => SkillId(player) is Skill.KAMEJOKO or Skill.MASENKO or Skill.ANTOMIC;

    // Kiểm tra nhóm chiêu bị cap sát thương Mabư (Java: switch skillSelect.template.id).
    private static bool IsMabuDamageCapSkill(Player player) => SkillId(player) is Skill.KAMEJOKO or Skill.MASENKO or Skill.ANTOMIC or Skill.DRAGON or Skill.DEMON or Skill.GALICK or Skill.LIEN_HOAN or Skill.KAIOKEN;

    // Kiểm tra nhóm chiêu cộng dồn sát thương (Java: switch skillSelect.template.id).
    private static bool IsStackingDamageSkill(Player player) => SkillId(player) is Skill.DRAGON or Skill.DEMON or Skill.GALICK or Skill.LIEN_HOAN or Skill.KAIOKEN;

    // Kiểm tra nhóm chiêu trừ né và kích giáp Xên (Java: switch skillSelect.template.id).
    private static bool IsAccurateSkill(Player? player) => player is not null && SkillId(player) is Skill.KAMEJOKO or Skill.MASENKO or Skill.ANTOMIC or Skill.DRAGON or Skill.DEMON or Skill.GALICK or Skill.LIEN_HOAN or Skill.KAIOKEN or Skill.QUA_CAU_KENH_KHI or Skill.MAKANKOSAPPO or Skill.DICH_CHUYEN_TUC_THOI;

    // Kiểm tra Quả cầu kênh khí dùng trong nhánh rơi đuôi khỉ (Java: Skill.QUA_CAU_KENH_KHI).
    private static bool IsMonkeyTailDropSkill(Player player) => SkillId(player) == Skill.QUA_CAU_KENH_KHI;

    // Kiểm tra playerSkill.skillSelect và template có đủ trước switch Java (Java: injured).
    private static bool HasSelectedSkill(Player player) => player.PlayerSkill?.SkillSelect?.Template is not null;

    // Lấy id template kỹ năng đã chọn; thiếu skill/template thì không khớp switch Java.
    private static sbyte? SkillId(Player player) => player.PlayerSkill?.SkillSelect?.Template?.Id;
}
