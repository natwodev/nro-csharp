using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Templates;
using Nro.Game;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Players.Stats;

/// <summary>
/// Targeted tests for <see cref="NPoint"/> (normal, boundary and error cases, and each Java quirk by name). The wide comparison with the original Java is
/// <see cref="NPointCharacterizationTests"/>; these tests use hand-written facts so a failure points at one rule.
/// </summary>
public class NPointTests
{
    private sealed class FakeHost : PlayerNPointHost
    {
        public NPointFacts Facts { get; set; } = new();
        public NPointAttackFacts Attack { get; set; } = new();
        public NPointUpdateFacts UpdateFacts { get; set; } = new();
        public Dictionary<int, PowerLimitInfo> Limits { get; } = [];
        public List<string> Calls { get; } = [];
        public int Counter { get; set; }

        public override NPointFacts Capture(NPoint npoint) => Facts;

        public override NPointAttackFacts CaptureAttack(NPoint npoint) => Attack;

        public override NPointUpdateFacts CaptureUpdate(NPoint npoint) => UpdateFacts;

        public override PowerLimitInfo? GetPowerLimit(int limitPower) => Limits.GetValueOrDefault(limitPower);

        public override void SetDie(Player player) => Calls.Add("die");

        public override void RestoreHpMp(Player player, long hp, long mp) => Calls.Add($"hoiPhuc {hp} {mp}");

        public override void SendCurrentStamina(Player player) => Calls.Add("stamina");

        public override void Chat(Player player, string text) => Calls.Add("chat " + text);

        public override void StopCharge(Player player) => Calls.Add("stop");

        public override int IncrementCountCharging(Player player) => ++Counter;

        public override void ClearCharging(Player player) => Calls.Add("clear");

        public override void SendNotification(Player player, string text) => Calls.Add("tb " + text);

        public override void SendNotificationOk(Player player, string text) => Calls.Add("tbok " + text);

        public override void SendMoney(Player player) => Calls.Add("money");

        public override void SendPoint(Player player) => Calls.Add("point");

        public override void CheckDoneTaskPower(Player player, long power) => Calls.Add("power " + power);

        public override void CheckPotentialUseTaskCompletion(Player player) => Calls.Add("usetn");
    }

    private static (NPoint Point, FakeHost Host, ManualTimeProvider Time) Make(long seed = 1)
    {
        Player player = new() { IsPlayer = true };
        ManualTimeProvider time = new(DateTimeOffset.FromUnixTimeMilliseconds(1_700_000_000_000L));
        FakeHost host = new();
        for (int i = 0; i < 12; i++)
        {
            host.Facts.Inventory.ItemsBody.Add(new NPointItem { IsNotNullItem = false });
        }

        host.Facts.Player.IsPl = true;
        NPoint point = new(player) { Host = host, Clock = new VietnamClock(time), Rand = new RandomUtil(new SeededRandomProvider(seed)) };
        player.NPoint = point;
        return (point, host, time);
    }

    [Fact]
    public void A_new_NPoint_has_the_java_defaults()
    {
        NPoint n = new(new Player());
        Assert.Equal(5, n.Speed);
        Assert.Equal(0, n.Hp);
        Assert.Empty(n.HpPercents);
        Assert.Equal(13, NPoint.MAX_LIMIT);
        Assert.Null(n.PowerLimit);
    }

    [Fact]
    public void CalPoint_with_nothing_equipped_gives_the_base_stats()
    {
        (NPoint n, _, _) = Make();
        n.BaseHp = 2000;
        n.BaseMp = 1500;
        n.BaseDamage = 100;
        n.BaseDefense = 10;
        n.BaseCriticalChance = 3;
        n.Hp = 99999;
        n.CalPoint();
        Assert.Equal(2000, n.HpMax);
        Assert.Equal(1500, n.MpMax);
        Assert.Equal(100, n.Damage);
        Assert.Equal(40, n.Defense);
        Assert.Equal(3, n.CriticalChance);
        Assert.Equal(2000, n.Hp);
        Assert.Equal(20, n.HpRegen);
    }

    [Fact]
    public void Percent_options_are_applied_one_after_the_other_with_integer_division()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.BaseHp = 999;
        h.Facts.Inventory.ItemsBody[0] = new NPointItem { TemplateId = 1, ItemOptions = [new(77, 10), new(77, 15)] };
        n.CalPoint();
        // 999 + 999*10/100 = 1098 (99 cut), then 1098 + 1098*15/100 = 1262 (164 cut)
        Assert.Equal(1262, n.HpMax);
    }

    [Fact]
    public void A_doubling_and_a_percent_do_not_commute()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.BaseHp = 1001;
        h.Facts.ItemTime.IsUseBloodTonic = true;
        h.Facts.ItemTime.IsUseRedPeach999 = true;
        n.CalPoint();
        // bo huyet (x2) comes before hong dao 999 (+100%): ((1001 * 2) + 100%) = 4004
        Assert.Equal(4004, n.HpMax);
    }

    [Fact]
    public void The_times_2_point_2_boost_is_a_double_multiplication_cut_to_long()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.BaseHp = 9_007_199_254_740_993L; // 2^53 + 1 is not a double
        h.Facts.ItemTime.IsUseBloodTonic2 = true;
        n.CalPoint();
        Assert.Equal(JavaNumbers.ToLong(9_007_199_254_740_993L * 2.2), n.HpMax);
    }

    [Fact]
    public void Option_2_overflows_the_int_total_like_java()
    {
        (NPoint n, FakeHost h, _) = Make();
        h.Facts.Inventory.ItemsBody[0] = new NPointItem { TemplateId = 1, ItemOptions = [new(2, 3_000_000)] };
        n.CalPoint();
        Assert.Equal(unchecked(3_000_000 * 1000), n.HpBonus);
        Assert.True(n.HpBonus < 0);
    }

    [Fact]
    public void Speed_bonuses_compound_on_the_current_speed_and_wrap_as_a_byte()
    {
        (NPoint n, FakeHost h, _) = Make();
        h.Facts.Inventory.ItemsBody[0] = new NPointItem { TemplateId = 1, ItemOptions = [new(16, 100), new(16, 100), new(16, 100), new(16, 100), new(16, 100)] };
        n.CalPoint();
        // 5 -> 10 -> 20 -> 40 -> 80 -> 160 (as a signed byte: -96)
        Assert.Equal(unchecked((sbyte)160), n.Speed);
    }

    [Fact]
    public void A_slowed_player_ends_with_speed_1()
    {
        (NPoint n, FakeHost h, _) = Make();
        h.Facts.EffectSkin.IsSlow = true;
        n.CalPoint();
        Assert.Equal(1, n.Speed);
    }

    [Fact]
    public void Defence_wraps_as_an_int()
    {
        (NPoint n, _, _) = Make();
        n.BaseDefense = 1_000_000_000;
        n.CalPoint();
        Assert.Equal(unchecked(1_000_000_000 * 4), n.Defense);
    }

    [Fact]
    public void The_ki_maximum_applies_black_ball_star_six_twice()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.BaseMp = 1000;
        h.Facts.BlackBallTimeOut[5] = long.MaxValue;
        n.CalPoint();
        // 1000 + 1000*40/100 = 1400, then 1400 + 1400*40/100 = 1960
        Assert.Equal(1960, n.MpMax);
    }

    [Fact]
    public void The_Whis_rank_three_ki_bonus_lands_on_the_hp_maximum()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.BaseHp = 1000;
        n.BaseMp = 500;
        h.Facts.Player.Id = 3;
        h.Facts.Server.WhisPlayerIds = [1, 2, 3, 4, 5];
        n.CalPoint();
        // rank 3: hp gets +10% in setHpMax, and then setMpMax adds 10% of the ki (50) to the hp maximum again
        Assert.Equal(1100 + 50, n.HpMax);
        Assert.Equal(500, n.MpMax);
    }

    [Fact]
    public void Option_176_sets_the_speed_to_5_and_adds_ten_percent_attack_for_a_player_only()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.BaseDamage = 1000;
        h.Facts.Inventory.ItemsBody[0] = new NPointItem { TemplateId = 1, ItemOptions = [new(176, 1)] };
        n.CalPoint();
        Assert.Equal(1100, n.Damage);
        Assert.Equal(5, n.Speed);

        h.Facts.Player.IsPl = false;
        n.CalPoint();
        Assert.Equal(1000, n.Damage);
    }

    [Fact]
    public void The_world_cup_set_is_counted_from_the_worn_items_and_written_back()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.BaseHp = 1000;
        h.Facts.Inventory.ItemsBody[0] = new NPointItem { TemplateId = 966 };
        h.Facts.Inventory.ItemsBody[1] = new NPointItem { TemplateId = 904 };
        n.CalPoint();
        Assert.Equal(2, h.Facts.SetClothes.Worldcup);
        Assert.Equal(1100, n.HpMax);
    }

    [Fact]
    public void A_body_list_of_exactly_five_throws_like_the_java_IndexOutOfBounds()
    {
        (NPoint n, FakeHost h, _) = Make();
        h.Facts.Inventory.ItemsBody.RemoveRange(5, 7);
        Assert.Throws<ArgumentOutOfRangeException>(n.CalPoint);
    }

    [Fact]
    public void The_regeneration_ratios_are_clamped_and_stored_back()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.BaseHp = 1000;
        h.Facts.Inventory.ItemsBody[0] = new NPointItem { TemplateId = 1, ItemOptions = [new(80, 130), new(81, -5)] };
        n.CalPoint();
        Assert.Equal(100, n.HpRegenPercent);
        Assert.Equal(0, n.MpRegenPercent);
        Assert.Equal((1000 / 100) + 1000, n.HpRegen);
    }

    [Fact]
    public void The_trbne_dodge_boost_doubles_the_dodge_plus_k()
    {
        (NPoint n, FakeHost h, _) = Make();
        h.Facts.Player.Trbne = 3;
        h.Facts.Inventory.ItemsBody[0] = new NPointItem { TemplateId = 1, ItemOptions = [new(108, 10)] };
        n.CalPoint();
        Assert.Equal(10 + (10 + 3), n.DodgePercent);
    }

    [Fact]
    public void The_intrinsic_gold_bonus_needs_a_previous_attack()
    {
        (NPoint n, FakeHost h, _) = Make();
        h.Facts.Intrinsic = new NPointIntrinsic { Id = 23, Param1 = 17 };
        h.Attack.Intrinsic = h.Facts.Intrinsic;
        h.Attack.SkillSelect = new NPointSkillFacts { TemplateId = Skill.DRAGON, Damage = 100, Point = 1 };
        n.CalPoint();
        Assert.Equal(0, n.GoldPercent);
        n.GetDamageAttack(false);
        n.CalPoint();
        Assert.Equal(17, n.GoldPercent);
    }

    [Fact]
    public void The_teleport_skill_forces_a_critical_hit_and_draws_its_damage_within_five_percent()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.Damage = 10_000;
        n.MpMax = 20_000;
        h.Attack.SkillSelect = new NPointSkillFacts { TemplateId = Skill.DICH_CHUYEN_TUC_THOI, Damage = 50, Point = 1 };
        // draws: NextLong(1001) for the range [9500, 10500] -> 500 gives 10000; getOne (even -> -1); Crisnext(1000) -> 10
        n.Rand = new RandomUtil(new ScriptedRandomProvider().Enqueue(500L, 0, 10));
        double result = n.GetDamageAttack(false);
        // critical: 10000 * 2 = 20000 (no critical-damage bonus); jitter -10 + 1
        Assert.Equal(19_991.0, result);
        Assert.True(n.IsCritical);
        Assert.True(n.IsTeleportCritical);
    }

    [Fact]
    public void Makankosappo_is_the_ki_maximum_times_the_skill_percent_without_critical_or_jitter()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.Damage = 10_000;
        n.MpMax = 20_000;
        n.CriticalChance = 100;
        h.Attack.SkillSelect = new NPointSkillFacts { TemplateId = Skill.MAKANKOSAPPO, Damage = 50, Point = 1 };
        n.Rand = new RandomUtil(new ScriptedRandomProvider()); // any draw would throw
        Assert.Equal(10_000.0, n.GetDamageAttack(false));
    }

    [Fact]
    public void An_unknown_skill_uses_the_plain_attack_with_the_jitter()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.Damage = 100_000;
        n.CriticalChance = 0;
        h.Attack.SkillSelect = new NPointSkillFacts { TemplateId = 99, Damage = 50, Point = 1 };
        double result = n.GetDamageAttack(false);
        // jitter is up to 5% of the damage (5000) in either direction plus one
        Assert.InRange(result, 100_000 - 5000, 100_000 + 5001);
    }

    [Fact]
    public void The_energy_ball_adds_ten_percent_of_the_hp_around()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.Damage = 1000;
        h.Attack.SkillSelect = new NPointSkillFacts { TemplateId = Skill.QUA_CAU_KENH_KHI, Point = 1 };
        h.Attack.AroundMobHp = 5000;
        h.Attack.AroundPlayerHp = 2000;
        n.Rand = new RandomUtil(new ScriptedRandomProvider().Enqueue(5)); // NextInt(-5, 5): draw 5 of 11 gives 0
        double result = n.GetDamageAttack(false);
        // 5000*10/100 + 2000*10/100 + 1000*10 = 10700, jitter 0
        Assert.Equal(10_700, result);
    }

    [Fact]
    public void A_critical_hit_doubles_the_damage_then_adds_the_critical_damage_percent()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.Damage = 1000;
        n.IsGuaranteedCritical = true;
        n.CriticalDamagePercent = 50;
        h.Attack.SkillSelect = new NPointSkillFacts { TemplateId = Skill.DRAGON, Damage = 100, Point = 1 };
        // critical: no draw. 1000 -> 2000 -> 3000. jitter: getOne draw then Crisnext(150)
        n.Rand = new RandomUtil(new ScriptedRandomProvider().Enqueue(0, 10)); // getOne(-1,1): nextInt()=0 -> -1; Crisnext: nextInt(150)=10
        double result = n.GetDamageAttack(false);
        Assert.Equal(3000 + (-1 * 10) + 1, result);
        Assert.False(n.IsGuaranteedCritical);
    }

    // ---- hp / ki accessors ----

    [Fact]
    public void SetHP_clamps_to_the_maximum_and_kills_at_zero()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.HpMax = 1000;
        n.SetHP(5000);
        Assert.Equal(1000, n.Hp);
        n.SetHP(0);
        n.SetHP(-5);
        Assert.Equal(new[] { "die", "die" }, h.Calls);
        Assert.Equal(1000, n.Hp);
    }

    [Fact]
    public void SetHp_with_a_lower_case_p_does_not_clamp_or_kill()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.HpMax = 1000;
        n.SetHp(5000);
        Assert.Equal(5000, n.Hp);
        Assert.Equal(1000, n.GetHP());
        n.SetHp(-3);
        Assert.Equal(0, n.Hp);
        Assert.Empty(h.Calls);
    }

    [Fact]
    public void AddHp_ignores_non_positive_amounts_and_AddMp_clamps_both_ways()
    {
        (NPoint n, _, _) = Make();
        n.HpMax = 100;
        n.MpMax = 100;
        n.Hp = 50;
        n.Mp = 50;
        n.AddHp(-30);
        n.AddHp(0);
        Assert.Equal(50, n.Hp);
        n.AddHp(500);
        Assert.Equal(100, n.Hp);
        n.AddMp(-500);
        Assert.Equal(0, n.Mp);
        n.AddMp(70);
        n.AddMp(70);
        Assert.Equal(100, n.Mp);
    }

    [Fact]
    public void SubHP_cuts_a_double_and_floors_at_zero_without_killing()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.Hp = 100;
        n.SubHP(10.9);
        Assert.Equal(89, n.Hp); // 100 - 10.9 = 89.1, cut
        n.SubHP(1e30);
        Assert.Equal(0, n.Hp);
        Assert.Empty(h.Calls);
    }

    [Fact]
    public void Percent_of_the_current_hp_has_a_zero_guard_for_hp_only()
    {
        (NPoint n, _, _) = Make();
        Assert.Equal(100, n.GetCurrentHpPercent());
        Assert.Throws<DivideByZeroException>(() => n.GetCurrentMpPercent());
        n.HpMax = 300;
        n.Hp = 100;
        Assert.Equal(33, n.GetCurrentHpPercent());
    }

    [Fact]
    public void CalPercent_multiplies_first_and_wraps()
    {
        (NPoint n, _, _) = Make();
        Assert.Equal(99, n.CalculatePercent(199, 50));
        Assert.Equal(0, n.CalculatePercent(1, 99));
        Assert.Equal(unchecked(long.MaxValue * 3) / 100, n.CalculatePercent(long.MaxValue, 3));
    }

    [Fact]
    public void SubSucManh_and_friends_floor_at_zero()
    {
        (NPoint n, _, _) = Make();
        n.Power = 40;
        n.Potential = 60;
        n.SubtractPowerPotential(50);
        Assert.Equal(0, n.Power);
        Assert.Equal(10, n.Potential);
        n.SubtractPotential(99);
        Assert.Equal(0, n.Potential);
        n.SubtractPower(5);
        Assert.Equal(0, n.Power);
    }

    [Fact]
    public void ReduceDamageByDefense_never_goes_below_one()
    {
        (NPoint n, _, _) = Make();
        n.Defense = 100;
        Assert.Equal(900.5, n.ReduceDamageByDefense(1000.5));
        Assert.Equal(1, n.ReduceDamageByDefense(50));
        Assert.Equal(0, n.ReduceDamageByDefense(100));
    }

    [Fact]
    public void GetPlayerRank_is_one_based_and_minus_one_when_absent()
    {
        (NPoint n, _, _) = Make();
        List<Player> list = [new() { Id = 7 }, new() { Id = 9 }];
        Assert.Equal(2, n.GetPlayerRank(list, new Player { Id = 9 }));
        Assert.Equal(-1, n.GetPlayerRank(list, new Player { Id = 1 }));
        Assert.Equal(-1, n.GetPlayerRank(new List<Player>(), new Player { Id = 1 }));
    }

    // ---- power ----

    [Fact]
    public void Power_limit_is_zero_until_loaded_then_comes_from_the_host()
    {
        (NPoint n, FakeHost h, _) = Make();
        Assert.Equal(0, n.GetPowerLimit());
        Assert.True(n.CanOpenPower()); // power 0 >= limit 0
        h.Limits[0] = new PowerLimitInfo(5000, 1, 1, 1, 1, 1);
        h.Limits[1] = new PowerLimitInfo(9000, 1, 1, 1, 1, 1);
        n.InitPowerLimit();
        Assert.Equal(5000, n.GetPowerLimit());
        Assert.Equal(9000, n.GetPowerNextLimit());
        Assert.False(n.CanOpenPower());
        n.Power = 5000;
        Assert.True(n.CanOpenPower());
        n.LimitPower = 5;
        Assert.Equal(0, n.GetPowerNextLimit());
    }

    [Fact]
    public void PowerUp_adds_and_reports_to_the_task_service()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.PowerUp(100);
        n.PowerUp(50);
        Assert.Equal(150, n.Power);
        Assert.Equal(new[] { "power 100", "power 150" }, h.Calls);
    }

    [Theory]
    [InlineData(0L, 1000L, 1000L)]
    [InlineData(39_999_999_999L, 1000L, 1000L)]
    [InlineData(40_000_000_000L, 1000L, 500L)]
    [InlineData(50_000_000_000L, 1000L, 200L)]
    [InlineData(100_000_000_000L, 1000L, 10L)]
    [InlineData(120_000_000_000L, 1000L, 10L)]
    [InlineData(0L, 50_000_000L, 20_000_000L)]
    public void CalSubTNSM_cuts_by_milestone_and_caps_at_20_million(long power, long amount, long expected)
    {
        (NPoint n, FakeHost h, _) = Make();
        h.Limits[0] = new PowerLimitInfo(long.MaxValue, 1, 1, 1, 1, 1);
        n.InitPowerLimit();
        n.Power = power;
        Assert.Equal(expected, n.CalculatePowerPotentialReduction(amount));
    }

    [Fact]
    public void CalSubTNSM_is_zero_at_the_power_limit()
    {
        (NPoint n, FakeHost h, _) = Make();
        h.Limits[0] = new PowerLimitInfo(1000, 1, 1, 1, 1, 1);
        n.InitPowerLimit();
        n.Power = 1000;
        Assert.Equal(0, n.CalculatePowerPotentialReduction(5000));
    }

    [Fact]
    public void CalculatePowerPotential_returns_zero_without_a_zone_and_ten_at_the_limit()
    {
        (NPoint n, FakeHost h, _) = Make();
        Assert.Equal(0, n.CalculatePowerPotential(1000));
        h.Facts.Zone = new NPointZoneFacts();
        Assert.Equal(10, n.CalculatePowerPotential(1000)); // no limit loaded: power 0 >= limit 0
        h.Facts.Zone.MapType = 3;
        Assert.Equal(0, n.CalculatePowerPotential(1000));
    }

    [Fact]
    public void CalculatePowerPotential_applies_the_server_rate_and_never_returns_less_than_one()
    {
        (NPoint n, FakeHost h, _) = Make();
        h.Limits[0] = new PowerLimitInfo(1_000_000_000, 1, 1, 1, 1, 1);
        n.InitPowerLimit();
        h.Facts.Zone = new NPointZoneFacts();
        Assert.Equal(10_000, n.CalculatePowerPotential(1000)); // x10 default rate
        h.Facts.Server.RateExpServer = 0;
        Assert.Equal(1, n.CalculatePowerPotential(1000));
        Assert.Equal(1, n.CalculatePowerPotential(-5));
    }

    [Fact]
    public void A_pet_adds_the_master_potential_scroll_twice()
    {
        (NPoint n, FakeHost h, _) = Make();
        h.Limits[0] = new PowerLimitInfo(1_000_000_000, 1, 1, 1, 1, 1);
        n.InitPowerLimit();
        h.Facts.Zone = new NPointZoneFacts();
        h.Facts.Server.RateExpServer = 1;
        h.Facts.Player.IsDeTu = true;
        h.Facts.Player.DetuTypeDeTu = 0;
        h.Facts.Master = new NPointMasterFacts { ItemTimeIsUsePetPowerPotentialCharm = true, PetPowerPotentialPercent = -100 };
        // 1000 + 1000*2 + 1000*10 = 13000; the pet-potential term is 1000/100*(−100+100) = 0
        Assert.Equal(13_000, n.CalculatePowerPotential(1000));
    }

    // ---- spending potential ----

    private static (NPoint, FakeHost) Spender(long tiemNang)
    {
        (NPoint n, FakeHost h, _) = Make();
        h.Limits[0] = new PowerLimitInfo(1_000_000, 20_000, 20_000, 1000, 100, 5);
        n.InitPowerLimit();
        n.Potential = tiemNang;
        n.BaseHp = 2000;
        n.BaseMp = 2000;
        n.BaseDamage = 100;
        n.BaseDefense = 10;
        n.BaseCriticalChance = 2;
        return (n, h);
    }

    [Fact]
    public void IncreasePoint_hp_costs_the_arithmetic_series_and_adds_20_per_point()
    {
        (NPoint n, FakeHost h) = Spender(10_000_000);
        n.IncreasePoint(0, 5, false);
        // pointHp = 100; cost = 5 * (2*(2000+1000) + 100 - 20) / 2 = 5 * 6080 / 2 = 15200
        Assert.Equal(2100, n.BaseHp);
        Assert.Equal(10_000_000 - 15_200, n.Potential);
        Assert.Equal(new[] { "usetn", "point" }, h.Calls);
    }

    [Fact]
    public void IncreasePoint_refuses_when_the_cap_would_be_exceeded()
    {
        (NPoint n, FakeHost h) = Spender(10_000_000);
        n.BaseHp = 19_990;
        n.IncreasePoint(0, 1, false);
        Assert.Equal(19_990, n.BaseHp);
        Assert.Equal(new[] { "tb HP của bạn đã đạt mức tối đa", "money" }, h.Calls);
    }

    [Fact]
    public void IncreasePoint_tells_the_player_when_potential_is_short()
    {
        (NPoint n, FakeHost h) = Spender(10);
        n.IncreasePoint(2, 1, false);
        Assert.Equal(100, n.BaseDamage);
        Assert.Equal(new[] { "tbok Bạn không đủ tiềm năng" }, h.Calls);
    }

    [Fact]
    public void IncreasePoint_does_nothing_for_protected_accounts_zero_points_or_no_limit()
    {
        (NPoint n, FakeHost h) = Spender(1_000_000_000);
        n.Player.Baovetaikhoan = true;
        n.IncreasePoint(0, 1, false);
        Assert.Equal(new[] { "tb Chức năng bảo vệ đã được bật. Bạn vui lòng kiểm tra lại" }, h.Calls);
        n.Player.Baovetaikhoan = false;
        h.Calls.Clear();
        n.IncreasePoint(0, 0, false);
        n.IncreasePoint(0, -4, false);
        n.PowerLimit = null;
        n.IncreasePoint(0, 1, false);
        Assert.Empty(h.Calls);
        Assert.Equal(2000, n.BaseHp);
    }

    [Fact]
    public void IncreasePoint_defence_cost_is_computed_in_int_and_can_wrap()
    {
        (NPoint n, FakeHost h) = Spender(1000);
        h.Limits[0] = new PowerLimitInfo(1_000_000, 20_000, 20_000, 1000, 2_000_000_000, 5);
        n.InitPowerLimit();
        n.BaseDefense = 200_000_000;
        // the cost is an int expression that has wrapped before it is widened
        int cost = unchecked(2 * (200_000_000 + 5) / 2 * 100000);
        Assert.NotEqual(2L * (200_000_000 + 5) / 2 * 100000, cost);
        n.IncreasePoint(3, 1, false);
        if (cost > 1000)
        {
            Assert.Equal(1000, n.Potential);
            Assert.Equal(200_000_000, n.BaseDefense);
        }
        else
        {
            Assert.Equal(1000 - cost, n.Potential); // a negative wrapped cost even ADDS potential, as in Java
            Assert.Equal(200_000_001, n.BaseDefense);
        }
    }

    [Fact]
    public void IncreasePoint_for_the_pet_refreshes_its_panels()
    {
        (NPoint n, FakeHost h) = Spender(1_000_000_000);
        n.Player.Detu = new Detu();
        n.IncreasePoint(2, 1, true);
        Assert.Equal(new[] { "usetn", "point", "point" }, h.Calls);
    }

    [Theory]
    [InlineData(0L, 0L)]
    [InlineData(20L, 1000L)]
    [InlineData(39L, 1000L)] // 39 / 20 is cut to 1 first
    [InlineData(2000L, 199_000L)]
    public void GetFullTN_of_hp_is_the_sum_of_the_cost_series(long hpg, long expected)
    {
        (NPoint n, _, _) = Make();
        n.BaseHp = hpg;
        Assert.Equal(expected, n.GetFullTN());
    }

    [Fact]
    public void GetFullTN_critical_term_uses_five_to_the_power()
    {
        (NPoint n, _, _) = Make();
        n.BaseCriticalChance = 3;
        Assert.Equal(50L * (125 - 1) / 4 * 1_000_000L, n.GetFullTN());
    }

    // ---- update ----

    [Fact]
    public void Update_regenerates_every_30_seconds_and_gives_stamina_every_minute()
    {
        (NPoint n, FakeHost h, ManualTimeProvider time) = Make();
        n.HpRegen = 12;
        n.MpRegen = 34;
        n.Stamina = 1;
        n.MaxStamina = 5;
        n.Update();
        Assert.Equal(new[] { "hoiPhuc 12 34", "hoiPhuc 0 0", "stamina" }, h.Calls);
        Assert.Equal(2, n.Stamina);

        h.Calls.Clear();
        time.Advance(TimeSpan.FromSeconds(10));
        n.Update(); // exactly 10 s later: "now - last > 10000" is false
        Assert.Empty(h.Calls);

        time.Advance(TimeSpan.FromMilliseconds(1));
        n.Update();
        Assert.Equal(new[] { "hoiPhuc 0 0" }, h.Calls);

        h.Calls.Clear();
        time.Advance(TimeSpan.FromSeconds(30));
        n.Update();
        Assert.Equal(new[] { "hoiPhuc 12 34", "hoiPhuc 0 0" }, h.Calls);
    }

    [Fact]
    public void Update_does_nothing_without_an_effect_skill_object()
    {
        (NPoint n, FakeHost h, _) = Make();
        h.UpdateFacts.HasEffectSkill = false;
        n.Update();
        Assert.Empty(h.Calls);
    }

    [Fact]
    public void Update_charging_heals_a_percent_of_the_maximum_and_chats_every_third_tick()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.HpMax = 10_000;
        n.MpMax = 8_000;
        n.Hp = 100;
        n.Mp = 100;
        h.UpdateFacts = new NPointUpdateFacts { IsCharging = true, CountCharging = 3, SkillSelectPoint = 4 };
        n.Update();
        // level 4 -> 7%: 10000/100*7 = 700, 8000/100*7 = 560
        Assert.Contains("hoiPhuc 700 560", h.Calls);
        Assert.Contains("chat Phục hồi năng lượng 1%", h.Calls);
    }

    [Fact]
    public void Update_charging_stops_when_full_dead_or_under_another_effect()
    {
        foreach (Action<NPoint, FakeHost> arrange in new Action<NPoint, FakeHost>[]
        {
            (n, _) => { n.Hp = n.HpMax; n.Mp = n.MpMax; },
            (n, _) => n.Hp = 0,
            (_, h) => h.UpdateFacts.IsHaveEffectSkill = true,
        })
        {
            (NPoint n, FakeHost h, _) = Make();
            n.HpMax = 1000;
            n.MpMax = 1000;
            n.Hp = 10;
            n.Mp = 10;
            h.UpdateFacts = new NPointUpdateFacts { IsCharging = true, CountCharging = 1, SkillSelectPoint = 1 };
            arrange(n, h);
            n.Update();
            Assert.Contains("stop", h.Calls);
            Assert.DoesNotContain(h.Calls, c => c.StartsWith("chat", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Update_stops_the_charge_after_the_tenth_tick()
    {
        (NPoint n, FakeHost h, _) = Make();
        n.HpMax = 1000;
        n.MpMax = 1000;
        n.Hp = 10;
        h.Counter = 9;
        h.UpdateFacts = new NPointUpdateFacts { IsCharging = true, CountCharging = 9, SkillSelectPoint = 1 };
        n.Update();
        Assert.Contains("stop", h.Calls);
    }

    [Fact]
    public void Update_clears_a_charge_with_no_selected_skill_instead_of_throwing()
    {
        (NPoint n, FakeHost h, _) = Make();
        h.UpdateFacts = new NPointUpdateFacts { IsCharging = true, CountCharging = 4, HasSkillSelect = false };
        n.Update();
        Assert.Contains("clear", h.Calls);
        Assert.DoesNotContain("stop", h.Calls);
    }

    [Fact]
    public void Update_sends_no_stamina_packet_for_a_boss_or_a_pet()
    {
        foreach (Action<Player> mark in new Action<Player>[] { p => p.IsBoss = true, p => p.IsDeTu = true, p => p.IsBo = true, p => p.IsMe = true, p => p.IsNguoiYeu = true, p => p.IsConOne = true })
        {
            (NPoint n, FakeHost h, _) = Make();
            mark(n.Player);
            n.Stamina = 0;
            n.MaxStamina = 3;
            n.Update();
            Assert.Equal(1, n.Stamina);
            Assert.DoesNotContain("stamina", h.Calls);
        }
    }

    // ---- misc ----

    [Fact]
    public void Dispose_clears_the_lists_and_keeps_the_player()
    {
        (NPoint n, _, _) = Make();
        Player player = n.Player;
        n.HpPercents.Add(5);
        n.SpeedPercent.Add(5);
        n.Dispose();
        Assert.Empty(n.HpPercents);
        Assert.Empty(n.SpeedPercent);
        Assert.Same(player, n.Player);
    }

    [Fact]
    public void The_default_host_reads_the_player_shell_flags()
    {
        Player p = new() { IsPlayer = true, Id = 42, THE_TUAN = 2, LASTTIME_THE_TUAN = 99, Isbienhinh = 3, Buff = Buff.BUFF_HP };
        NPointFacts facts = PlayerNPointHost.Shared.Capture(new NPoint(p));
        Assert.True(facts.Player.IsPl);
        Assert.True(facts.Player.IsPlMan);
        Assert.Equal(42, facts.Player.Id);
        Assert.Equal(2, facts.Player.THE_TUAN);
        Assert.Equal(3, facts.Player.Isbienhinh);
        Assert.Equal(Buff.BUFF_HP, facts.Player.Buff);
        Assert.Null(facts.Detu);
    }

    [Fact]
    public void The_default_host_tells_a_boss_from_a_player()
    {
        Assert.False(PlayerNPointHost.IsPl(new Player { IsPlayer = true, IsBoss = true }));
        Assert.False(PlayerNPointHost.IsPl(new Player { IsPlayer = true, IsDeTu = true }));
        Assert.True(PlayerNPointHost.IsPlMan(new Player { IsDeTu = true }));
        Assert.False(PlayerNPointHost.IsPlMan(new Player { IsPlayer = true, IsBot = true }));
    }

    [Fact]
    public void The_default_host_exposes_the_pet_numbers_after_the_pet_recalculated()
    {
        (NPoint master, FakeHost host, _) = Make();
        Player pet = new Detu { IsDeTu = true };
        pet.NPoint = new NPoint(pet) { Host = host };
        pet.NPoint.HpMax = 777;
        master.Player.Detu = (Detu)pet;
        NPointFacts facts = PlayerNPointHost.Shared.Capture(master);
        Assert.Equal(777, facts.Detu!.HpMax);
    }
    // ---- the default host reads the real components of the player ----

    [Fact]
    public void Every_facts_flag_has_a_counterpart_on_the_real_component()
    {
        Assert.Empty(PlayerNPointHost.UnmatchedProperties(typeof(ItemTime), typeof(NPointItemTimeFacts)));
        Assert.Empty(PlayerNPointHost.UnmatchedProperties(typeof(EffectSkill), typeof(NPointEffectSkillFacts)));
        Assert.Empty(PlayerNPointHost.UnmatchedProperties(typeof(EffectSkin), typeof(NPointEffectSkinFacts)));
    }

    private static Player RealPlayer()
    {
        Player p = new() { IsPlayer = true, Id = 5 };
        p.Inventory = new Inventory();
        for (int i = 0; i < 12; i++)
        {
            p.Inventory.ItemsBody.Add(new Item());
        }

        p.ItemTime = new ItemTime(p);
        p.EffectSkill = new EffectSkill(p);
        p.EffectSkin = new EffectSkin(p);
        p.EffectFlagBag = new EffectFlagBag();
        p.RewardBlackBall = new RewardBlackBall(p);
        p.PlayerIntrinsic = new IntrinsicPlayer();
        p.PlayerSkill = new PlayerSkill(p);
        p.NPoint = new NPoint(p);
        return p;
    }

    private static Item Worn(int templateId, int type = 0, params (int Id, int Param)[] options)
    {
        Item item = new() { Template = new ItemTemplate { Id = (short)templateId, Type = (sbyte)type } };
        foreach ((int id, int param) in options)
        {
            item.ItemOptions.Add(new ItemOption { OptionTemplate = new ItemOptionTemplate { Id = id }, Param = param });
        }

        return item;
    }

    [Fact]
    public void The_default_host_makes_CalPoint_use_the_flags_and_items_of_a_real_player()
    {
        Player p = RealPlayer();
        p.NPoint.BaseHp = 1000;
        p.NPoint.BaseDamage = 100;
        p.Inventory.ItemsBody[0] = Worn(7, 0, (77, 10), (0, 50));
        p.ItemTime.IsUseRedPeach999 = true;
        p.NPoint.CalPoint();
        Assert.Equal(2200, p.NPoint.HpMax); // 1000 + 10% = 1100, hong dao 999 doubles it
        Assert.Equal(300, p.NPoint.Damage); // (100 + 50), doubled
        Assert.Equal(50, p.NPoint.DamageBonus);
    }

    [Fact]
    public void The_default_host_reads_the_effect_skill_black_ball_and_flag_bag_of_a_real_player()
    {
        Player p = RealPlayer();
        p.NPoint.BaseHp = 1000;
        p.NPoint.BaseMp = 1000;
        p.EffectSkill.IsVirus = true;
        p.RewardBlackBall.TimeOutOfDateReward[1] = long.MaxValue; // two stars: +35% hp
        p.EffectFlagBag.UseFishSkewer = true;
        p.NPoint.Clock = new VietnamClock(new ManualTimeProvider(DateTimeOffset.FromUnixTimeMilliseconds(1_700_000_000_000L)));
        p.NPoint.CalPoint();
        // hp: virus -10% = 900, then +35% of 900 (315) = 1215
        Assert.Equal(1215, p.NPoint.HpMax);
        // ki: virus -10% = 900, grilled fish +15% of 900 (135) = 1035
        Assert.Equal(1035, p.NPoint.MpMax);
    }

    [Fact]
    public void The_default_host_writes_the_chosen_training_armour_back_to_the_inventory()
    {
        Player worn = RealPlayer();
        worn.Inventory.ItemsBody[6] = Worn(529);
        worn.NPoint.CalPoint();
        Assert.Same(worn.Inventory.ItemsBody[6], worn.Inventory.TrainingArmor);
        Assert.Equal(10, worn.NPoint.DamageReductionPercent);
        Assert.True(worn.NPoint.WearingTrainArmor);

        Player bag = RealPlayer();
        Item carried = Worn(531, 32, (9, 5));
        bag.Inventory.ItemsBag.Add(Worn(7));
        bag.Inventory.ItemsBag.Add(carried);
        bag.NPoint.BaseDamage = 1000;
        bag.NPoint.CalPoint();
        Assert.Same(carried, bag.Inventory.TrainingArmor);
        Assert.Equal([30], bag.NPoint.DamagePercents);
        Assert.Equal(1300, bag.NPoint.Damage);
    }

    [Fact]
    public void The_default_host_reports_the_charging_state_of_the_effect_skill()
    {
        Player p = RealPlayer();
        p.EffectSkill.IsCharging = true;
        p.EffectSkill.CountCharging = 4;
        NPointUpdateFacts uf = PlayerNPointHost.Shared.CaptureUpdate(p.NPoint);
        Assert.True(uf.IsCharging);
        Assert.Equal(4, uf.CountCharging);
        Assert.False(uf.HasSkillSelect);
        Assert.Equal(5, PlayerNPointHost.Shared.IncrementCountCharging(p));
        PlayerNPointHost.Shared.ClearCharging(p);
        Assert.False(p.EffectSkill.IsCharging);
        Assert.Equal(0, p.EffectSkill.CountCharging);
        p.EffectSkill = null!;
        Assert.False(PlayerNPointHost.Shared.CaptureUpdate(p.NPoint).HasEffectSkill);
    }

    [Fact]
    public void The_default_host_refuses_an_attack_without_a_selected_skill_and_writes_the_palm_flags_back()
    {
        Player p = RealPlayer();
        Assert.Throws<NullReferenceException>(() => PlayerNPointHost.Shared.CaptureAttack(p.NPoint));
        NPointAttackFacts af = new();
        af.EffectSkin.IsChargedBlast = false;
        af.EffectSkin.IsDamageBoosted = true;
        af.EffectSkin.LastTimeChargedBlast = 77;
        PlayerNPointHost.Shared.CommitAttack(p.NPoint, af);
        Assert.False(p.EffectSkin.IsChargedBlast);
        Assert.True(p.EffectSkin.IsDamageBoosted);
        Assert.Equal(77, p.EffectSkin.LastTimeChargedBlast);
    }
}
