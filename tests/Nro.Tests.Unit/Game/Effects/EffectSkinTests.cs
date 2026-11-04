using Nro.Game;

namespace Nro.Tests.Unit.Game.Effects;

/// <summary>
/// <see cref="EffectSkin"/> (Java <c>EffectSkin.java</c>): every periodic aura with its period, range, filter, scripted random draws and exact client commands, the expiry of the
/// skin effects, the Java quirks that were kept, and the safety properties (a failing effect does not stop the next one, a player leaving the zone during the update cannot break
/// the loop). Time is a fake clock, random numbers are scripted.
/// </summary>
public class EffectSkinTests
{
    /// <summary>An owner (id 1, at 0,0) and one neighbour (id 2, <paramref name="dx"/> to the right).</summary>
    private static (EffectRig Rig, Player Owner, Player Neighbour) Pair(
        Action<FakeWorld.Sim>? owner = null,
        Action<FakeWorld.Sim>? neighbour = null,
        int dx = 100,
        Action<Player>? ownerPlayer = null,
        Action<Player>? neighbourPlayer = null)
    {
        var rig = new EffectRig();
        Player o = rig.Add(1, 0, 0, owner, ownerPlayer);
        Player n = rig.Add(2, dx, 0, neighbour, neighbourPlayer);
        Quiet(rig, o);
        return (rig, o, n);
    }

    /// <summary>Stamps the training-armor timers with "now" so the 60 s housekeeping of the training armor stays out of the call list.</summary>
    private static void Quiet(EffectRig rig, Player player)
    {
        player.EffectSkin.LastTimeAddTimeTrainArmor = rig.Now;
        player.EffectSkin.LastTimeSubTimeTrainArmor = rig.Now;
    }

    private static Action<FakeWorld.Sim> Stat(Func<EffectNPointState, EffectNPointState> change) => s => s.NPoint = change(s.NPoint);

    private static Action<FakeWorld.Sim> Clothes(EffectSetClothesState state) => s => s.Clothes = state;

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Construction, environment, dispose
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void A_new_skin_state_has_no_multiplier_and_nothing_on()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        EffectSkin s = p.EffectSkin;
        Assert.Same(p, s.Player);
        Assert.Equal(1, s.HpKiMultiplier);
        Assert.Equal(1, s.DamageMultiplier);
        Assert.False(s.IsNezuko || s.IsTanjiro || s.IsZenitsu || s.IsInosuke || s.IsInoHashi || s.IsChocolate || s.IsRabbitLeader || s.IsDraburaFrost || s.IsSlow);
        Assert.False(s.IsInvisible || s.IsBlastDamageMultiplierReady || s.IsBlastDamageMultiplierApplied);
    }

    [Fact]
    public void The_constructor_rejects_a_null_player()
    {
        Assert.Throws<ArgumentNullException>(() => new EffectSkin(null!));
    }

    [Fact]
    public void Update_before_the_environment_is_set_fails_clearly()
    {
        var player = new Player { Id = 1 };
        Assert.Throws<InvalidOperationException>(() => player.EffectSkin.Update());
    }

    [Fact]
    public void Dispose_keeps_the_player_and_stops_the_updates()
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => n with { IsFounder = true }));
        owner.EffectSkin.Dispose();
        owner.EffectSkin.Dispose();
        owner.EffectSkin.Update();
        Assert.Same(owner, owner.EffectSkin.Player);
        Assert.Empty(rig.World.Calls);
    }

    [Fact]
    public void The_text_tables_have_the_Java_contents()
    {
        Assert.Equal(["Hôi quá, tránh xa ta ra", "Biến đi", "Trời ơi đồ ở dơ", "Thúi quá", "Mùi gì hôi quá"], EffectSkin.DirtyTalk);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Gate of the auras: dead, no zone, offline map
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void A_dead_owner_runs_no_aura()
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => n with { IsFounder = true, Hp = 0 }));
        owner.EffectSkin.Update();
        Assert.Empty(rig.World.Calls);
    }

    [Fact]
    public void An_owner_without_a_zone_runs_no_aura_and_needs_no_map_lookup()
    {
        (EffectRig rig, Player owner, _) = Pair(s =>
        {
            s.NPoint = s.NPoint with { IsFounder = true };
            s.HasZone = false;
        });
        owner.EffectSkin.Update();
        Assert.Empty(rig.World.Calls);
        Assert.Empty(rig.Errors);
    }

    [Fact]
    public void An_owner_in_an_offline_map_runs_no_aura_but_the_multiplier_reset_and_training_armor_still_run()
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => n with { IsFounder = true }));
        rig.World.OfflineMaps.Add(5);
        owner.EffectSkin.HpKiMultiplier = 3;
        owner.EffectSkin.LastTimeHpKiMultiplier = rig.Now - 1_800_001;
        owner.EffectSkin.Update();
        Assert.Equal(1, owner.EffectSkin.HpKiMultiplier);
        Assert.Equal(["Point:1"], rig.World.Calls);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Multipliers and expiry
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void The_hp_ki_and_damage_multipliers_reset_after_thirty_minutes_strictly(bool hpKi)
    {
        (EffectRig rig, Player owner, _) = Pair();
        EffectSkin s = owner.EffectSkin;
        if (hpKi)
        {
            s.HpKiMultiplier = 3;
            s.LastTimeHpKiMultiplier = rig.Now;
        }
        else
        {
            s.DamageMultiplier = 4;
            s.LastTimeDamageMultiplier = rig.Now;
        }

        rig.Advance(1_800_000);
        Quiet(rig, owner);
        s.Update();
        Assert.Equal(hpKi ? 3 : 1, s.HpKiMultiplier);
        Assert.Equal(hpKi ? 1 : 4, s.DamageMultiplier);
        Assert.Empty(rig.World.Calls);

        rig.Advance(1);
        s.Update();
        Assert.Equal(1, s.HpKiMultiplier);
        Assert.Equal(1, s.DamageMultiplier);
        Assert.Equal(["Point:1"], rig.World.Calls);
    }

    [Fact]
    public void A_multiplier_of_one_is_never_reset()
    {
        (EffectRig rig, Player owner, _) = Pair();
        owner.EffectSkin.Update();
        Assert.Empty(rig.World.Calls);
    }

    private sealed record ExpiryCase(string Name, Action<EffectSkin, long> Arm, Func<EffectSkin, bool> IsOn, string[] Calls);

    private static readonly ExpiryCase[] Expiry =
    [
        new("Chocolate", (s, now) => { s.IsChocolate = true; s.LastTimeChocolate = now; s.TimeChocolate = 5000; }, s => s.IsChocolate, ["Point:1", "Caitrang:1"]),
        new("RabbitLeader", (s, now) => { s.IsRabbitLeader = true; s.LastTimeRabbitLeader = now; s.TimeRabbitLeader = 5000; }, s => s.IsRabbitLeader, ["Point:1", "Caitrang:1"]),
        new("DraburaFrost", (s, now) => { s.IsDraburaFrost = true; s.LastTimeDraburaFrost = now; s.TimeDraburaFrost = 5000; }, s => s.IsDraburaFrost, ["Point:1", "Caitrang:1"]),
        new("Slow", (s, now) => { s.IsSlow = true; s.LastTimeSlow = now; s.TimeSlow = 5000; }, s => s.IsSlow, ["Point:1"]),
    ];

    public static TheoryData<string> ExpiryNames => [.. Expiry.Select(c => c.Name)];

    [Theory]
    [MemberData(nameof(ExpiryNames))]
    public void Skin_effects_end_one_millisecond_after_their_duration(string name)
    {
        ExpiryCase c = Expiry.Single(x => x.Name == name);
        (EffectRig rig, Player owner, _) = Pair();
        c.Arm(owner.EffectSkin, rig.Now);

        rig.Advance(5000);
        owner.EffectSkin.Update();
        Assert.True(c.IsOn(owner.EffectSkin), name + " at the end");
        Assert.Empty(rig.World.Calls);

        rig.Advance(1);
        owner.EffectSkin.Update();
        Assert.False(c.IsOn(owner.EffectSkin), name + " after the end");
        Assert.Equal(c.Calls, rig.World.Calls.Where(x => !x.StartsWith("ItemBag", StringComparison.Ordinal)).ToArray());
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Auras that talk to or hurt the players around
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Dirty_hurts_each_neighbour_by_a_percentage_of_his_maximum_hp_with_integer_division()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair(
            Stat(n => n with { PercentDirtyHpLoss = 10 }),
            Stat(n => n with { Hp = 1999, HpMax = 1999 }));
        rig.Random.Enqueue(2);

        owner.EffectSkin.Update();

        Assert.Equal(
            ["Chat:2:Trời ơi đồ ở dơ", "InfoHpMpMoney:2", "InfoNv:2", "Injure:2:-:199:True:False"],
            rig.World.Calls);
        Assert.Equal(1800, rig.World.SimOf(neighbour).NPoint.Hp);
        Assert.Equal([("NextInt", (object?)5)], rig.Random.Calls);
    }

    [Fact]
    public void Dirty_never_kills_the_neighbour()
    {
        (EffectRig rig, Player owner, _) = Pair(
            Stat(n => n with { PercentDirtyHpLoss = 90 }),
            Stat(n => n with { Hp = 50, HpMax = 1000 }));
        rig.Random.Enqueue(0);
        owner.EffectSkin.Update();
        Assert.Contains("Injure:2:-:49:True:False", rig.World.Calls);
    }

    [Fact]
    public void Dirty_waits_ten_seconds_between_rounds_strictly()
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => n with { PercentDirtyHpLoss = 1 }));
        rig.Random.Enqueue(0, 0, 0);

        owner.EffectSkin.Update();
        Assert.Equal(1, rig.World.Count("Injure"));

        rig.Advance(10_000);
        owner.EffectSkin.Update();
        Assert.Equal(1, rig.World.Count("Injure"));

        rig.Advance(1);
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Equal(2, rig.World.Count("Injure"));
    }

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, false)]
    public void Dirty_reaches_exactly_200_units(int distance, bool hit)
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => n with { PercentDirtyHpLoss = 10 }), dx: distance);
        rig.Random.Enqueue(0);
        owner.EffectSkin.Update();
        Assert.Equal(hit ? 1 : 0, rig.World.Count("Injure"));
    }

    [Fact]
    public void Dirty_spares_bosses_dead_players_and_the_owner_himself()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { PercentDirtyHpLoss = 10 }));
        rig.Add(2, 10, 0, null, p => p.IsBoss = true);
        rig.Add(3, 10, 0, Stat(n => n with { Hp = 0 }));
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Empty(rig.World.Calls);
    }

    [Fact]
    public void Dirty_with_a_zero_percentage_does_nothing_and_does_not_stamp_the_time()
    {
        (EffectRig rig, Player owner, _) = Pair();
        owner.EffectSkin.Update();
        Assert.Empty(rig.World.Calls);
        Assert.Empty(rig.Random.Calls);
    }

    [Fact]
    public void Dirty_uses_the_read_int_switch_for_big_numbers()
    {
        var rig = new EffectRig(readInt: true);
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { PercentDirtyHpLoss = 100 }));
        Player victim = rig.Add(2, 10, 0, Stat(n => n with { Hp = 9_000_000_000L, HpMax = 9_000_000_000L }));
        Quiet(rig, owner);
        rig.Random.Enqueue(0);
        owner.EffectSkin.Update();
        Assert.Equal(9_000_000_000L - 2_147_483_647L, rig.World.SimOf(victim).NPoint.Hp);
    }

    [Fact]
    public void Founder_greets_players_up_to_500_units_away_every_five_seconds()
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => n with { IsFounder = true }), dx: 500);
        rig.Random.Enqueue(3, 0, 1);

        owner.EffectSkin.Update();
        Assert.Equal(["Chat:2:Idol"], rig.World.Calls);
        Assert.Equal([("NextInt", (object?)6)], rig.Random.Calls);

        rig.Advance(5000);
        owner.EffectSkin.Update();
        Assert.Single(rig.World.Calls);

        rig.Advance(1);
        owner.EffectSkin.Update();
        Assert.Equal(["Chat:2:Idol", "Chat:2:MaiTienDung"], rig.World.Calls);
    }

    [Fact]
    public void Founder_does_not_reach_501_units()
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => n with { IsFounder = true }), dx: 501);
        owner.EffectSkin.Update();
        Assert.Empty(rig.World.Calls);
        Assert.Empty(rig.Random.Calls);
    }

    [Fact]
    public void Xinbato_makes_neighbours_chat_and_grants_them_the_xinbato_effect()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair(Stat(n => n with { IsXinbato = true }), dx: 300);
        rig.Random.Enqueue(4);
        owner.EffectSkin.Update();
        Assert.Equal(["Chat:2:Tránh ra đi Xinbatô ơi"], rig.World.Calls);
        Assert.True(neighbour.EffectSkill.IsXinbato);
        Assert.Equal(5000, neighbour.EffectSkill.TimeXinbato);
        Assert.False(owner.EffectSkill.IsXinbato);
    }

    [Fact]
    public void Rabbit_bulma_buffs_the_owner_once_per_neighbour_not_the_neighbour()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { SexyDamagePercent = 7, IsRabbitBulma = true }));
        Player a = rig.Add(2, 120, 0);
        Player b = rig.Add(3, 50, 0);
        rig.Add(4, 121, 0);
        Quiet(rig, owner);
        rig.Random.Enqueue(0, 0);

        owner.EffectSkin.Update();

        // the list is walked from the end: 4 is out of range (121 > 120), then 3, then 2
        Assert.Equal(["Chat:3:Wow, sexy quá", "Chat:2:Wow, sexy quá"], rig.World.Calls);
        Assert.True(owner.EffectSkill.IsDamageBuff);
        Assert.Equal((11_000, 7), (owner.EffectSkill.TimeDamageBuff, owner.EffectSkill.DamageBuffPercent));
        Assert.False(a.EffectSkill.IsDamageBuff);
        Assert.False(b.EffectSkill.IsDamageBuff);
    }

    [Fact]
    public void Sexy_damage_without_the_bulma_rabbit_uses_the_compliments()
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => n with { SexyDamagePercent = 3 }), dx: 10);
        rig.Random.Enqueue(2);
        owner.EffectSkin.Update();
        Assert.Equal(["Chat:2:Xinh quá!"], rig.World.Calls);
        Assert.Equal([("NextInt", (object?)3)], rig.Random.Calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Cool_dame_and_cute_dame_buff_the_owner_when_somebody_is_near(bool cool)
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => cool ? n with { CoolDamagePercent = 12 } : n with { CuteDamagePercent = 12 }));
        owner.EffectSkin.Update();
        Assert.True(owner.EffectSkill.IsDamageBuff);
        Assert.Equal((11_000, 12), (owner.EffectSkill.TimeDamageBuff, owner.EffectSkill.DamageBuffPercent));
        Assert.Empty(rig.World.Calls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Cool_dame_and_cute_dame_do_nothing_when_alone(bool cool)
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => cool ? n with { CoolDamagePercent = 12 } : n with { CuteDamagePercent = 12 }));
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.False(owner.EffectSkill.IsDamageBuff);
    }

    [Fact]
    public void Fide_dai_ca_boosts_the_followers_within_500_units()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Clothes(new EffectSetClothesState { CostumeFriezaBoss = 5 }));
        Player follower = rig.Add(2, 500, 0, Clothes(new EffectSetClothesState { CostumeFriezaMinion = 6 }));
        Player stranger = rig.Add(3, 10, 0);
        Player far = rig.Add(4, 501, 0, Clothes(new EffectSetClothesState { CostumeFriezaMinion = 6 }));
        Quiet(rig, owner);

        owner.EffectSkin.Update();

        Assert.Equal(["Chat:2:Fide đại ca"], rig.World.Calls);
        Assert.True(follower.EffectSkill.IsFideLeader);
        Assert.Equal(10_000, follower.EffectSkill.TimeFideLeader);
        Assert.False(stranger.EffectSkill.IsFideLeader);
        Assert.False(far.EffectSkill.IsFideLeader);
    }

    [Fact]
    public void The_four_demon_slayer_set_effects_fire_every_two_minutes_and_end_by_their_own_aura_stamp()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Clothes(new EffectSetClothesState { CostumeNezuko = 1, CostumeTanjiro = 1, CostumeInosukeHashibira = 1, CostumeInosuke = 1 }));
        Quiet(rig, owner);
        EffectSkin s = owner.EffectSkin;

        s.Update();
        Assert.Equal(
            [
                "Chat:1:Hơi thở của dã thú....", "Point:1", // InoHashi
                "Chat:1:Hơi thở của dã thú....", "Point:1", // Inosuke
                "Chat:1:Grừuuuuuu............", "Point:1", // Nezuko
                "Chat:1:Hơi Thở của Nước......", "Point:1", // Tanjiro
            ],
            rig.World.Calls);
        Assert.True(s.IsNezuko && s.IsTanjiro && s.IsInoHashi && s.IsInosuke);
        Assert.Equal((10_000, 15_000, 20_000, 20_000), (s.TimeNezuko, s.TimeTanjiro, s.TimeInoHashi, s.TimeInosuke));

        rig.World.Calls.Clear();
        rig.Advance(10_000);
        s.Update();
        Assert.True(s.IsNezuko);
        rig.Advance(1);
        s.Update();
        Assert.False(s.IsNezuko);
        Assert.True(s.IsTanjiro && s.IsInoHashi && s.IsInosuke);
        Assert.Equal(["Point:1"], rig.World.Calls);

        rig.Advance(5000); // 15001 since the aura
        s.Update();
        Assert.False(s.IsTanjiro);
        rig.Advance(5000); // 20001
        s.Update();
        Assert.False(s.IsInoHashi || s.IsInosuke);
    }

    [Fact]
    public void Demon_slayer_aura_fires_again_after_120_seconds_strictly()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Clothes(new EffectSetClothesState { CostumeNezuko = 1 }));
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Equal(1, rig.World.Count("Chat"));

        rig.Advance(120_000);
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Equal(1, rig.World.Count("Chat"));

        rig.Advance(1);
        owner.EffectSkin.Update();
        Assert.Equal(2, rig.World.Count("Chat"));
    }

    [Fact]
    public void Zenitsu_snores_and_confuses_neighbours_every_two_seconds()
    {
        (EffectRig rig, Player owner, _) = Pair(Clothes(new EffectSetClothesState { CostumeZenitsu = 1 }), dx: 200);
        owner.EffectSkin.Update();
        Assert.Equal(["Chat:1:Khò khò.......", "Chat:2:Hoang mang quá!"], rig.World.Calls);

        rig.Advance(2000);
        owner.EffectSkin.Update();
        Assert.Equal(2, rig.World.Calls.Count);

        rig.Advance(1);
        owner.EffectSkin.Update();
        Assert.Equal(4, rig.World.Calls.Count);
    }

    [Fact]
    public void Dong_minh_heals_neighbours_within_200_every_ten_seconds()
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => n with { SelfAndAllyHpRegenPercent = 5 }));
        owner.EffectSkin.Update();
        Assert.Equal(["Recover:2:5:0"], rig.World.Calls);
        owner.EffectSkin.Update();
        Assert.Single(rig.World.Calls);
    }

    [Fact]
    public void Bi_ngo_aura_speaks_first_even_with_nobody_around_then_pumpkins_the_neighbours()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { IsPumpkinAura = true }));
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Equal(["Chat:1:Hóa bí đi!"], rig.World.Calls);

        Player neighbour = rig.Add(2, 100, 0);
        rig.World.Calls.Clear();
        rig.Advance(10_001);
        Quiet(rig, owner);
        rig.Random.Enqueue(4);
        owner.EffectSkin.Update();

        Assert.Equal(
            ["Chat:1:Hóa bí đi!", "ItemTime:2:7057:5", "Chat:2:Đầu tôi đâu?!", "Caitrang:2", "InfoHpMpMoney:2", "InfoNv:2", "Point:2"],
            rig.World.Calls);
        Assert.True(neighbour.EffectSkill.IsPumpkin);
        Assert.Equal(5000, neighbour.EffectSkill.TimePumpkin);
    }

    [Fact]
    public void Bong_tuyet_item_freezes_neighbours_every_seven_seconds()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair(s => s.HasBongTuyet = true);
        rig.Random.Enqueue(1);
        owner.EffectSkin.Update();
        Assert.Equal(
            ["ItemTime:2:7066:5", "Chat:2:Ư... lạnh ghê...", "InfoHpMpMoney:2", "InfoNv:2", "Point:2"],
            rig.World.Calls);
        Assert.True(neighbour.EffectSkill.IsSnowflake);
        Assert.Equal(5000, neighbour.EffectSkill.TimeSnowflake);

        rig.World.Calls.Clear();
        rig.Advance(7000);
        owner.EffectSkin.Update();
        Assert.Empty(rig.World.Calls);
    }

    [Theory]
    [InlineData(69, true)]
    [InlineData(70, false)]
    public void Dabura_ice_hits_with_a_seventy_percent_roll(int roll, bool hit)
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair(s => s.HasFrostSkin = true, dx: 300);
        rig.Random.Enqueue(roll);
        owner.EffectSkin.Update();

        Assert.Equal(hit, neighbour.EffectSkin.IsDraburaFrost);
        if (hit)
        {
            Assert.Equal(5000, neighbour.EffectSkin.TimeDraburaFrost);
            Assert.Equal(rig.Now, neighbour.EffectSkin.LastTimeDraburaFrost);
            Assert.Equal(["Point:2", "Caitrang:2", "Chat:1:Phụt!", "Chat:2:Hự...", "ItemTime:2:11085:5"], rig.World.Calls);
        }
        else
        {
            Assert.Empty(rig.World.Calls);
        }

        // the 30 s timer is stored whether or not the roll hit
        rig.World.Calls.Clear();
        rig.Advance(30_000);
        rig.Random.Enqueue(0);
        owner.EffectSkin.Update();
        Assert.Empty(rig.Random.Calls.Skip(1));
    }

    [Theory]
    [InlineData(79, true)]
    [InlineData(80, false)]
    public void Stone_aura_petrifies_with_an_eighty_percent_roll(int roll, bool hit)
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair(Stat(n => n with { IsPetrified = true }));
        rig.Random.Enqueue(roll);
        owner.EffectSkin.Update();
        Assert.Equal(hit, neighbour.EffectSkill.IsStone);
        if (hit)
        {
            Assert.Equal(["ItemTime:2:4392:6", "Caitrang:2", "Chat:1:Phẹt.....", "Chat:2:hự....."], rig.World.Calls);
            Assert.Equal(6000, neighbour.EffectSkill.TimeStone);
        }
        else
        {
            Assert.Empty(rig.World.Calls);
        }
    }

    [Fact]
    public void Bui_bui_slows_neighbours_for_three_seconds_every_three_seconds()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair(Stat(n => n with { WearingBuiBui = true }));
        owner.EffectSkin.Update();
        Assert.Equal(["Chat:2:Nặng quá!", "Point:2"], rig.World.Calls);
        Assert.True(neighbour.EffectSkin.IsSlow);
        Assert.Equal((3000, rig.Now), (neighbour.EffectSkin.TimeSlow, neighbour.EffectSkin.LastTimeSlow));

        rig.Advance(3000);
        owner.EffectSkin.Update();
        Assert.Equal(2, rig.World.Calls.Count);
        rig.Advance(1);
        owner.EffectSkin.Update();
        Assert.Equal(4, rig.World.Calls.Count);
    }

    [Fact]
    public void Mabu_turns_neighbours_into_chocolate_every_thirty_seconds()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair(Stat(n => n with { WearingMabu = true }));
        owner.EffectSkin.Update();
        Assert.Equal(["Chat:1:Phẹt.....", "Chat:2:hự.....", "Point:2", "Caitrang:2", "ItemTime:2:3780:5"], rig.World.Calls);
        Assert.True(neighbour.EffectSkin.IsChocolate);
        Assert.Equal(5000, neighbour.EffectSkin.TimeChocolate);
    }

    [Fact]
    public void Carrot_aura_turns_neighbours_into_carrots_every_330_seconds()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair(Stat(n => n with { IsRabbitLeader = true }));
        rig.Random.Enqueue(0);
        owner.EffectSkin.Update();
        Assert.Equal(["Point:2", "Caitrang:2", "Chat:2:Biến thành cà rốt cho ta!", "ItemTime:2:4082:300"], rig.World.Calls);
        Assert.True(neighbour.EffectSkin.IsRabbitLeader);
        Assert.Equal(300_000, neighbour.EffectSkin.TimeRabbitLeader);
    }

    [Fact]
    public void Carrot_aura_does_not_carrot_a_neighbour_who_is_a_carrot_himself()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair(Stat(n => n with { IsRabbitLeader = true }), Stat(n => n with { IsRabbitLeader = true }));
        rig.Random.Enqueue(0);
        owner.EffectSkin.Update();
        Assert.False(neighbour.EffectSkin.IsRabbitLeader);
    }

    [Fact]
    public void Palm_flag_is_raised_once_a_minute()
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => n with { BlastDamageMultiplier = 3 }));
        owner.EffectSkin.Update();
        Assert.True(owner.EffectSkin.IsBlastDamageMultiplierReady);
        Assert.Equal(rig.Now, owner.EffectSkin.LastTimeBlastDamageMultiplier);

        owner.EffectSkin.IsBlastDamageMultiplierReady = false;
        rig.Advance(60_000);
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.False(owner.EffectSkin.IsBlastDamageMultiplierReady);
        rig.Advance(1);
        owner.EffectSkin.Update();
        Assert.True(owner.EffectSkin.IsBlastDamageMultiplierReady);
    }

    [Fact]
    public void Invisibility_aura_hides_the_owner_below_the_map_every_five_seconds()
    {
        (EffectRig rig, Player owner, _) = Pair(Stat(n => n with { IsInvisible = true }));
        owner.EffectSkin.Update();
        Assert.True(owner.EffectSkill.IsInvisible);
        Assert.Equal(1500, owner.EffectSkill.TimeInvisible);
        Assert.Equal(["SetPos2:1:0:10000"], rig.World.Calls);
    }

    [Fact]
    public void Halloween_aura_infects_eligible_neighbours_only()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0);
        owner.EffectSkill.IsHalloween = true;
        Player ok = rig.Add(2, 10, 0);
        Player already = rig.Add(3, 10, 0);
        already.EffectSkill.IsHalloween = true;
        Player pet = rig.Add(4, 10, 0, null, p => p.IsPet = true);
        Player far = rig.Add(5, 201, 0);
        Player dead = rig.Add(6, 10, 0, Stat(n => n with { Hp = 0 }));
        Player bot = rig.Add(7, 10, 0, null, p => p.IsFather = true);
        Player me = rig.Add(8, 10, 0, null, p => p.IsMother = true);
        Quiet(rig, owner);
        rig.Random.Enqueue(2);

        owner.EffectSkin.Update();

        Assert.True(ok.EffectSkill.IsHalloween);
        Assert.Equal(2, ok.EffectSkill.HalloweenOutfitId);
        Assert.Equal(1_800_000, ok.EffectSkill.TimeHalloween);
        Assert.False(pet.EffectSkill.IsHalloween || far.EffectSkill.IsHalloween || dead.EffectSkill.IsHalloween || bot.EffectSkill.IsHalloween || me.EffectSkill.IsHalloween);
        Assert.Equal(["ItemTime:2:5101:1800", "Caitrang:2"], rig.World.Calls);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Virus
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Virus_coughs_and_infects_a_neighbour_on_two_successful_rolls()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair();
        owner.EffectSkill.IsVirus = true;
        rig.Random.Enqueue(10, 2, 5); // 10 < 50: cough; 2: "Choáng váng..."; 5 < 10: spread

        owner.EffectSkin.Update();

        Assert.Equal(
            ["Chat:1:Choáng váng...", "ItemTime:2:7143:300", "Point:2", "Notice:2:Bạn bị lây virus từ P1"],
            rig.World.Calls);
        Assert.True(neighbour.EffectSkill.IsVirus);
        Assert.Equal(300_000, neighbour.EffectSkill.TimeVirus);
        Assert.Equal([("NextInt", (object?)100), ("NextInt", 7), ("NextInt", 100)], rig.Random.Calls);
    }

    [Fact]
    public void Virus_failed_spread_roll_does_not_start_the_five_second_wait()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair();
        owner.EffectSkill.IsVirus = true;
        rig.Random.Enqueue(60, 99, 60, 99);

        owner.EffectSkin.Update();
        owner.EffectSkin.Update(); // no time passed, but the time was not stored, so it rolls again

        Assert.Equal(4, rig.Random.Calls.Count);
        Assert.False(neighbour.EffectSkill.IsVirus);
        Assert.Empty(rig.World.Calls);
    }

    [Fact]
    public void Virus_successful_spread_waits_five_seconds_strictly()
    {
        (EffectRig rig, Player owner, _) = Pair();
        owner.EffectSkill.IsVirus = true;
        rig.Random.Enqueue(60, 5, 60, 5);

        owner.EffectSkin.Update();
        Assert.Equal(2, rig.Random.Calls.Count);

        rig.Advance(5000);
        owner.EffectSkin.Update();
        Assert.Equal(2, rig.Random.Calls.Count);

        rig.Advance(1);
        owner.EffectSkin.Update();
        Assert.Equal(4, rig.Random.Calls.Count);
    }

    [Fact]
    public void Virus_stops_at_the_first_neighbour_wearing_a_mask_without_storing_the_time()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1);
        owner.EffectSkill.IsVirus = true;
        Player masked = rig.Add(2, 10, 0, s => s.HasFaceMaskAvatar = true);
        Player other = rig.Add(3, 10, 0);
        Quiet(rig, owner);
        rig.Random.Enqueue(60, 5, 60, 5);

        owner.EffectSkin.Update();
        owner.EffectSkin.Update();

        Assert.False(masked.EffectSkill.IsVirus);
        Assert.False(other.EffectSkill.IsVirus);
        Assert.Equal(4, rig.Random.Calls.Count);
        Assert.Empty(rig.World.Calls);
    }

    [Fact]
    public void Virus_mask_item_time_also_protects()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair(null, s => s.FaceMaskItemTime = true);
        owner.EffectSkill.IsVirus = true;
        rig.Random.Enqueue(60, 5);
        owner.EffectSkin.Update();
        Assert.False(neighbour.EffectSkill.IsVirus);
    }

    [Fact]
    public void Virus_skips_neighbours_who_are_already_infected_but_stores_the_time()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair();
        owner.EffectSkill.IsVirus = true;
        neighbour.EffectSkill.IsVirus = true;
        neighbour.EffectSkill.TimeVirus = 7;
        rig.Random.Enqueue(60, 5);
        owner.EffectSkin.Update();
        Assert.Equal(7, neighbour.EffectSkill.TimeVirus);
        Assert.Empty(rig.World.Calls);
        owner.EffectSkin.Update();
        Assert.Equal(2, rig.Random.Calls.Count); // timer stored: no new draws
    }

    [Fact]
    public void A_dead_virus_carrier_does_nothing()
    {
        (EffectRig rig, Player owner, Player neighbour) = Pair(Stat(n => n with { Hp = 0 }));
        owner.EffectSkill.IsVirus = true;
        rig.Random.Enqueue(10, 5);
        owner.EffectSkin.Update();
        Assert.False(neighbour.EffectSkill.IsVirus);
        Assert.Empty(rig.Random.Calls);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Drain around the player and the Mafuba damage
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Xen_hut_drains_mobs_and_players_and_heals_the_owner_with_the_sum()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { Hp = 500, HpMax = 1000, Mp = 100, MpMax = 1000, AroundHpMpAbsorptionPercent = 10 }));
        Player victim = rig.Add(2, 100, 0, Stat(n => n with { Hp = 2000, HpMax = 2000, Mp = 1000, MpMax = 1000 }));
        Quiet(rig, owner);
        var near = new FakeMob { Id = 1, Hp = 3000, HpFull = 3000, X = 150, Y = 0 };
        var far = new FakeMob { Id = 2, Hp = 3000, HpFull = 3000, X = 201, Y = 0 };
        var almostDead = new FakeMob { Id = 3, Hp = 1, HpFull = 3000, X = 10, Y = 0 };
        rig.World.Mobs.AddRange([near, far, almostDead]);

        owner.EffectSkin.Update();

        Assert.Equal([(-1L, 300.0, false)], near.InjuredCalls);
        Assert.Empty(far.InjuredCalls);
        Assert.Empty(almostDead.InjuredCalls);
        Assert.Equal(
            ["InfoHpMpMoney:2", "InfoNv:2", "Injure:2:-:200:True:False", "AddHp:1:500", "AddMp:1:100", "InfoHpMpMoney:1", "InfoNv:1"],
            rig.World.Calls);
        Assert.Equal(1000, rig.World.SimOf(owner).NPoint.Hp);
        Assert.Equal(200, rig.World.SimOf(owner).NPoint.Mp);
        Assert.Equal(1800, rig.World.SimOf(victim).NPoint.Hp);
    }

    [Fact]
    public void Xen_hut_never_takes_a_victims_last_hp_or_mp()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { Hp = 1, HpMax = 1000, Mp = 1, MpMax = 1000, AroundHpMpAbsorptionPercent = 50 }));
        Player victim = rig.Add(2, 10, 0, Stat(n => n with { Hp = 30, HpMax = 1000, Mp = 20, MpMax = 1000 }));
        Quiet(rig, owner);
        var mob = new FakeMob { Hp = 100, HpFull = 1000, X = 10, Y = 10 };
        rig.World.Mobs.Add(mob);

        owner.EffectSkin.Update();

        Assert.Equal([(-1L, 99.0, false)], mob.InjuredCalls);
        Assert.Contains("Injure:2:-:29:True:False", rig.World.Calls);
        Assert.Contains("AddHp:1:128", rig.World.Calls);
        Assert.Contains("AddMp:1:19", rig.World.Calls);
        Assert.Equal(1, rig.World.SimOf(victim).NPoint.Hp);
    }

    [Fact]
    public void Xen_hut_does_nothing_for_an_owner_at_full_hp_and_mp_or_with_a_zero_stat()
    {
        var rig = new EffectRig();
        Player full = rig.Add(1, 0, 0, Stat(n => n with { AroundHpMpAbsorptionPercent = 10 }));
        Player zero = rig.Add(2, 0, 0, Stat(n => n with { Hp = 1, AroundHpMpAbsorptionPercent = 0 }));
        Quiet(rig, full);
        Quiet(rig, zero);
        full.EffectSkin.Update();
        zero.EffectSkin.Update();
        Assert.Empty(rig.World.Calls);
    }

    [Fact]
    public void Xen_hut_waits_five_seconds_strictly()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { Hp = 1, HpMax = 1000, AroundHpMpAbsorptionPercent = 10 }));
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Equal(1, rig.World.Count("AddHp"));
        rig.Advance(5000);
        owner.EffectSkin.Update();
        Assert.Equal(1, rig.World.Count("AddHp"));
        rig.Advance(1);
        owner.EffectSkin.Update();
        Assert.Equal(2, rig.World.Count("AddHp"));
    }

    [Theory]
    [InlineData(0, 4, 40)]
    [InlineData(1, 4, 80)]
    public void Mafuba_bottle_damage_is_a_percentage_of_the_owners_max_hp_doubled_for_the_item_version(int typeBinh, int skillPoint, int expected)
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s =>
        {
            s.Skills[26] = new EffectSkillRef(2600, skillPoint);
            s.NPoint = s.NPoint with { HpMax = 1000 };
        });
        Player victim = rig.Add(2, 5, 5);
        Quiet(rig, victim);
        victim.EffectSkill.IsBottled = true;
        victim.EffectSkill.BottledType = typeBinh;
        victim.EffectSkill.MafubaUser = owner;

        victim.EffectSkin.Update();

        Assert.Equal(["InfoHpMpMoney:2", "InfoNv:2", $"Injure:2:1:{expected}:True:False"], rig.World.Calls);
    }

    [Fact]
    public void Mafuba_bottle_damage_is_cut_not_rounded()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s =>
        {
            s.Skills[26] = new EffectSkillRef(2600, 4);
            s.NPoint = s.NPoint with { HpMax = 1999 };
        });
        Player victim = rig.Add(2);
        Quiet(rig, victim);
        victim.EffectSkill.IsBottled = true;
        victim.EffectSkill.MafubaUser = owner;
        victim.EffectSkin.Update();
        Assert.Contains("Injure:2:1:79:True:False", rig.World.Calls); // 1999 * 4 / 100 = 79.96
    }

    [Theory]
    [InlineData(30, 70)] // would kill: abs(hp - 100)
    [InlineData(20, 80)]
    [InlineData(100, 60)] // 60 < 100: the normal damage
    public void Mafuba_bottle_never_kills_with_the_abs_rule(int hp, int expectedDamage)
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s =>
        {
            s.Skills[26] = new EffectSkillRef(2600, hp == 100 ? 6 : 4);
            s.NPoint = s.NPoint with { HpMax = 1000 };
        });
        Player victim = rig.Add(2, 0, 0, Stat(n => n with { Hp = hp }));
        Quiet(rig, victim);
        victim.EffectSkill.IsBottled = true;
        victim.EffectSkill.MafubaUser = owner;
        victim.EffectSkin.Update();
        Assert.Contains($"Injure:2:1:{expectedDamage}:True:False", rig.World.Calls);
    }

    [Fact]
    public void Mafuba_bottle_waits_half_a_second_strictly()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s => s.Skills[26] = new EffectSkillRef(2600, 4));
        Player victim = rig.Add(2);
        Quiet(rig, victim);
        victim.EffectSkill.IsBottled = true;
        victim.EffectSkill.MafubaUser = owner;

        victim.EffectSkin.Update();
        Assert.Equal(1, rig.World.Count("Injure"));
        rig.Advance(500);
        victim.EffectSkin.Update();
        Assert.Equal(1, rig.World.Count("Injure"));
        rig.Advance(1);
        victim.EffectSkin.Update();
        Assert.Equal(2, rig.World.Count("Injure"));
    }

    [Fact]
    public void Mafuba_bottle_without_the_skill_is_logged_and_does_not_hurt()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1);
        Player victim = rig.Add(2);
        Quiet(rig, victim);
        victim.EffectSkill.IsBottled = true;
        victim.EffectSkill.MafubaUser = owner;
        victim.EffectSkin.Update();
        Assert.Equal(0, rig.World.Count("Injure"));
        Assert.Single(rig.Errors);
    }

    [Fact]
    public void Mafuba_bottle_without_an_owner_does_nothing()
    {
        var rig = new EffectRig();
        Player victim = rig.Add(2);
        Quiet(rig, victim);
        victim.EffectSkill.IsBottled = true;
        victim.EffectSkin.Update();
        Assert.Empty(rig.World.Calls);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Costume sets that grow with the number of wearers
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static FakeItem Costume(int templateId, params int[] optionIds)
        => new() { TemplateId = templateId, OptionList = optionIds.Select(id => (FakeOption?)new FakeOption(id, 0)).ToList() };

    [Fact]
    public void Pirate_set_bonus_counts_the_distinct_wearers_within_300_units()
    {
        FakeItem ownerItem = Costume(618, 147, 77, 103, 50);
        ownerItem.OptionList[3]!.Param = 99;
        FakeItem nearItem = Costume(620, 147);
        FakeItem farItem = Costume(622, 147);
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s => { s.Clothes = new EffectSetClothesState { CostumePirate = 618 }; s.Body[5] = ownerItem; });
        Player near = rig.Add(2, 300, 0, s => { s.Clothes = new EffectSetClothesState { CostumePirate = 620 }; s.Body[5] = nearItem; });
        rig.Add(3, 301, 0, s => { s.Clothes = new EffectSetClothesState { CostumePirate = 622 }; s.Body[5] = farItem; });
        Quiet(rig, owner);

        owner.EffectSkin.Update();

        Assert.Equal([6, 6, 6, 99], ownerItem.OptionList.Select(o => o!.Param).ToArray());
        Assert.Equal(6, nearItem.OptionList[0]!.Param);
        Assert.Equal(0, farItem.OptionList[0]!.Param);

        // Java quirk: the stamp that gates the item refresh is the owner's own and is renewed after the first wearer, so only the owner gets his items re-sent.
        Assert.Equal(["ItemBody:1"], rig.World.Calls);
        Assert.Equal(rig.Now, owner.EffectSkin.LastTimePirateCostumeUpdate);
        Assert.Equal(rig.Now, near.EffectSkin.LastTimePirateCostumeUpdate);
    }

    [Fact]
    public void Pirate_set_wearing_the_same_costume_counts_once_and_a_lone_wearer_gets_three()
    {
        FakeItem ownerItem = Costume(618, 147);
        FakeItem nearItem = Costume(618, 147);
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s => { s.Clothes = new EffectSetClothesState { CostumePirate = 618 }; s.Body[5] = ownerItem; });
        rig.Add(2, 10, 0, s => { s.Clothes = new EffectSetClothesState { CostumePirate = 618 }; s.Body[5] = nearItem; });
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Equal(3, ownerItem.OptionList[0]!.Param);
        Assert.Equal(3, nearItem.OptionList[0]!.Param);
    }

    [Fact]
    public void Pirate_set_waits_five_seconds_and_ignores_items_of_other_templates()
    {
        FakeItem wrongItem = Costume(700, 147);
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s => { s.Clothes = new EffectSetClothesState { CostumePirate = 618 }; s.Body[5] = wrongItem; });
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Equal(0, wrongItem.OptionList[0]!.Param);
        Assert.Equal(["ItemBody:1"], rig.World.Calls);

        rig.World.Calls.Clear();
        rig.Advance(5000);
        owner.EffectSkin.Update();
        Assert.Empty(rig.World.Calls);
        rig.Advance(1);
        owner.EffectSkin.Update();
        Assert.Equal(["ItemBody:1"], rig.World.Calls);
    }

    [Fact]
    public void Pirate_set_empty_body_slot_is_skipped()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Clothes(new EffectSetClothesState { CostumePirate = 618 }));
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Equal(["ItemBody:1"], rig.World.Calls);
    }

    [Fact]
    public void Pirate_set_with_a_costume_id_out_of_range_throws_out_of_update_like_Java()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Clothes(new EffectSetClothesState { CostumePirate = 700 }));
        Quiet(rig, owner);
        Assert.Throws<IndexOutOfRangeException>(() => owner.EffectSkin.Update());
    }

    [Fact]
    public void Pirate_set_neighbour_with_an_out_of_range_costume_is_logged_and_the_owner_still_gets_his_bonus()
    {
        FakeItem ownerItem = Costume(618, 147);
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s => { s.Clothes = new EffectSetClothesState { CostumePirate = 618 }; s.Body[5] = ownerItem; });
        rig.Add(2, 10, 0, Clothes(new EffectSetClothesState { CostumePirate = 900 }));
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Single(rig.Errors);
        Assert.Equal(3, ownerItem.OptionList[0]!.Param);
    }

    [Fact]
    public void Bulma_set_bonus_is_three_per_distinct_wearer()
    {
        FakeItem a = Costume(1208, 147, 50);
        FakeItem b = Costume(1209, 147);
        FakeItem c = Costume(1210, 147);
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s => { s.Clothes = new EffectSetClothesState { CostumeBulmaColoredHair = 1208 }; s.Body[5] = a; });
        rig.Add(2, 10, 0, s => { s.Clothes = new EffectSetClothesState { CostumeBulmaColoredHair = 1209 }; s.Body[5] = b; });
        rig.Add(3, 20, 0, s => { s.Clothes = new EffectSetClothesState { CostumeBulmaColoredHair = 1210 }; s.Body[5] = c; });
        Quiet(rig, owner);

        owner.EffectSkin.Update();

        Assert.Equal(9, a.OptionList[0]!.Param); // 3 wearers * 3 = 9 (< 10)
        Assert.Equal(0, a.OptionList[1]!.Param);
        Assert.Equal(9, b.OptionList[0]!.Param);
        Assert.Equal(9, c.OptionList[0]!.Param);
        Assert.Equal(["ItemBody:1"], rig.World.Calls);
    }

    [Fact]
    public void Bulma_set_one_and_two_wearers_give_three_and_six()
    {
        FakeItem a = Costume(1208, 147);
        FakeItem b = Costume(1209, 147);
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s => { s.Clothes = new EffectSetClothesState { CostumeBulmaColoredHair = 1208 }; s.Body[5] = a; });
        owner.EffectSkin.LastTimeSubTimeTrainArmor = rig.Now;
        owner.EffectSkin.LastTimeAddTimeTrainArmor = rig.Now;
        owner.EffectSkin.Update();
        Assert.Equal(3, a.OptionList[0]!.Param);

        rig.Add(2, 10, 0, s => { s.Clothes = new EffectSetClothesState { CostumeBulmaColoredHair = 1209 }; s.Body[5] = b; });
        rig.Advance(5001);
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Equal(6, a.OptionList[0]!.Param);
        Assert.Equal(6, b.OptionList[0]!.Param);
    }

    [Fact]
    public void Feast_set_never_changes_neighbours_because_its_own_exclusion_matches_every_wearer()
    {
        FakeItem nearItem = Costume(1235, 147);
        nearItem.OptionList[0]!.Param = 50;
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Clothes(new EffectSetClothesState { CostumeBeachParty = 1234 }));
        rig.Add(2, 10, 0, s => { s.Clothes = new EffectSetClothesState { CostumeBeachParty = 1235 }; s.Body[5] = nearItem; });
        Quiet(rig, owner);

        owner.EffectSkin.Update();

        Assert.Equal(50, nearItem.OptionList[0]!.Param);
        Assert.Empty(rig.World.Calls);
        Assert.Empty(rig.Errors);

        // the stamp was not renewed, so it is evaluated again at once
        owner.EffectSkin.Update();
        Assert.Empty(rig.Errors);
    }

    [Fact]
    public void Feast_set_neighbour_with_a_costume_outside_the_family_is_logged()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Clothes(new EffectSetClothesState { CostumeBeachParty = 1234 }));
        rig.Add(2, 10, 0, Clothes(new EffectSetClothesState { CostumeBeachParty = 1300 }));
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Single(rig.Errors);
        Assert.IsType<IndexOutOfRangeException>(rig.Errors[0].Exception);
    }

    [Fact]
    public void Feast_set_owner_with_an_out_of_range_costume_throws_like_Java()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Clothes(new EffectSetClothesState { CostumeBeachParty = 1000 }));
        Quiet(rig, owner);
        Assert.Throws<IndexOutOfRangeException>(() => owner.EffectSkin.Update());
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Training armor and invisibility armor
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static FakeItem Armor(int param, bool trainArmor = true, int optionId = 9)
        => new() { TemplateId = 529, TrainingArmor = trainArmor, OptionList = [new FakeOption(optionId, param)] };

    [Fact]
    public void Worn_training_armor_gains_a_minute_after_a_minute_of_not_fighting()
    {
        FakeItem worn = Armor(10);
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s =>
        {
            s.NPoint = s.NPoint with { WearingTrainArmor = true };
            s.TrainArmorWorn = worn;
        });
        owner.EffectSkin.LastTimeAttack = rig.Now; // fought just now
        owner.EffectSkin.LastTimeAddTimeTrainArmor = 0;
        owner.EffectSkin.LastTimeSubTimeTrainArmor = rig.Now;

        owner.EffectSkin.Update();

        // "!canDoWithTime(lastTimeAttack, 30000)": resting less than 30 s means the armor is worn and counts
        Assert.Equal(11, worn.OptionList[0]!.Param);
        Assert.Equal(["ItemBody:1"], rig.World.Calls);
        Assert.Equal(rig.Now, owner.EffectSkin.LastTimeAddTimeTrainArmor);
    }

    [Fact]
    public void Worn_training_armor_does_not_gain_when_the_last_attack_is_older_than_thirty_seconds_and_the_timer_is_not_renewed()
    {
        FakeItem worn = Armor(10);
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s =>
        {
            s.NPoint = s.NPoint with { WearingTrainArmor = true };
            s.TrainArmorWorn = worn;
        });
        owner.EffectSkin.LastTimeAttack = rig.Now - 30_001;
        owner.EffectSkin.LastTimeSubTimeTrainArmor = rig.Now;
        owner.EffectSkin.Update();
        Assert.Equal(10, worn.OptionList[0]!.Param);

        // Java quirk kept: the timer is stamped inside the same if, so a resting player keeps a stale stamp.
        Assert.Equal(0, owner.EffectSkin.LastTimeAddTimeTrainArmor);
    }

    [Fact]
    public void Worn_training_armor_attack_boundary_is_exactly_thirty_seconds()
    {
        FakeItem worn = Armor(10);
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s =>
        {
            s.NPoint = s.NPoint with { WearingTrainArmor = true };
            s.TrainArmorWorn = worn;
        });
        owner.EffectSkin.LastTimeAttack = rig.Now - 30_000; // exactly 30000: canDoWithTime is false, so the armor counts
        owner.EffectSkin.LastTimeSubTimeTrainArmor = rig.Now;
        owner.EffectSkin.Update();
        Assert.Equal(11, worn.OptionList[0]!.Param);
    }

    [Fact]
    public void Worn_training_armor_is_capped_at_1000_and_only_the_first_time_option_counts()
    {
        FakeItem capped = Armor(1000);
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s =>
        {
            s.NPoint = s.NPoint with { WearingTrainArmor = true };
            s.TrainArmorWorn = capped;
        });
        owner.EffectSkin.LastTimeAttack = rig.Now;
        owner.EffectSkin.LastTimeSubTimeTrainArmor = rig.Now;
        owner.EffectSkin.Update();
        Assert.Equal(1000, capped.OptionList[0]!.Param);
        Assert.Empty(rig.World.Calls);

        FakeItem twoOptions = new() { TemplateId = 529, TrainingArmor = true, OptionList = [new FakeOption(9, 5), new FakeOption(9, 7)] };
        var rig2 = new EffectRig();
        Player owner2 = rig2.Add(1, 0, 0, s =>
        {
            s.NPoint = s.NPoint with { WearingTrainArmor = true };
            s.TrainArmorWorn = twoOptions;
        });
        owner2.EffectSkin.LastTimeAttack = rig2.Now;
        owner2.EffectSkin.LastTimeSubTimeTrainArmor = rig2.Now;
        owner2.EffectSkin.Update();
        Assert.Equal([6, 7], twoOptions.OptionList.Select(o => o!.Param).ToArray());
    }

    [Fact]
    public void Training_armor_in_bag_and_box_loses_a_minute_per_minute_down_to_zero()
    {
        FakeItem inBag = Armor(5);
        FakeItem empty = Armor(0);
        FakeItem notArmor = Armor(5, trainArmor: false);
        FakeItem otherOption = Armor(5, optionId: 10);
        FakeItem noOptions = new() { TemplateId = 529, TrainingArmor = true, NoOptionList = true };
        FakeItem inBox = Armor(2);
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s =>
        {
            s.Bag.AddRange([inBag, null, empty, notArmor, otherOption, noOptions, FakeItem.Empty()]);
            s.Box.AddRange([inBox, null]);
        });
        owner.EffectSkin.LastTimeAddTimeTrainArmor = rig.Now;

        owner.EffectSkin.Update();

        Assert.Equal(4, inBag.OptionList[0]!.Param);
        Assert.Equal(0, empty.OptionList[0]!.Param);
        Assert.Equal(5, notArmor.OptionList[0]!.Param);
        Assert.Equal(5, otherOption.OptionList[0]!.Param);
        Assert.Equal(1, inBox.OptionList[0]!.Param);
        Assert.Equal(["ItemBag:1", "Point:1"], rig.World.Calls);
        Assert.Equal(rig.Now, owner.EffectSkin.LastTimeSubTimeTrainArmor);

        rig.World.Calls.Clear();
        rig.Advance(60_000);
        owner.EffectSkin.LastTimeAddTimeTrainArmor = rig.Now;
        owner.EffectSkin.Update();
        Assert.Equal(4, inBag.OptionList[0]!.Param);
        rig.Advance(1);
        owner.EffectSkin.LastTimeAddTimeTrainArmor = rig.Now;
        owner.EffectSkin.Update();
        Assert.Equal(3, inBag.OptionList[0]!.Param);
        Assert.Equal(["ItemBag:1", "Point:1"], rig.World.Calls);
    }

    [Theory]
    [InlineData("boss")]
    [InlineData("petfollow")]
    [InlineData("tangmonk")]
    [InlineData("nguoiyeu")]
    public void Training_armor_is_not_handled_for_bosses_pets_and_companions(string kind)
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, null, p =>
        {
            p.IsBoss = kind == "boss";
            p.IsPetFollow = kind == "petfollow";
            p.IsTangMonk = kind == "tangmonk";
            p.IsLover = kind == "nguoiyeu";
        });
        owner.EffectSkin.Update();
        Assert.Empty(rig.World.Calls);
        Assert.Equal(0, owner.EffectSkin.LastTimeSubTimeTrainArmor);
    }

    [Fact]
    public void Invisibility_needs_five_seconds_without_attacking_and_five_since_the_last_stamp()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { WearingInvisibilityArmor = true }));
        Quiet(rig, owner);
        EffectSkin s = owner.EffectSkin;

        s.Update();
        Assert.True(s.IsInvisible); // both stamps are 0 = long ago

        s.LastTimeAttack = rig.Now;
        s.Update();
        Assert.False(s.IsInvisible);

        rig.Advance(5000);
        Quiet(rig, owner);
        s.Update();
        Assert.False(s.IsInvisible); // exactly 5000 since the attack: not yet

        rig.Advance(1);
        s.Update();
        Assert.True(s.IsInvisible);

        s.LastTimeInvisible = rig.Now;
        s.Update();
        Assert.False(s.IsInvisible);
    }

    [Fact]
    public void Invisibility_ends_when_the_armor_is_taken_off()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { WearingInvisibilityArmor = true }));
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.True(owner.EffectSkin.IsInvisible);
        rig.World.SimOf(owner).NPoint = rig.World.SimOf(owner).NPoint with { WearingInvisibilityArmor = false };
        owner.EffectSkin.Update();
        Assert.False(owner.EffectSkin.IsInvisible);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Safety: failures and a changing zone
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void One_failing_effect_is_logged_and_the_next_effects_still_run()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, s =>
        {
            s.NPoint = s.NPoint with { IsFounder = true };
            s.Clothes = new EffectSetClothesState { CostumeNezuko = 1 };
        });
        rig.Add(99, 10, 0);
        Quiet(rig, owner);
        rig.Random.Enqueue(0);
        rig.World.OnChat = (player, _) =>
        {
            if (player.Id == 99)
            {
                throw new InvalidOperationException("chat failed");
            }
        };

        owner.EffectSkin.Update();

        Assert.Single(rig.Errors);
        Assert.True(owner.EffectSkin.IsNezuko); // the Demon Slayer aura came after the founder aura and still ran
        Assert.Contains("Chat:1:Grừuuuuuu............", rig.World.Calls);

        // the founder aura did not store its time, so it tries again at the next tick
        rig.Random.Enqueue(0);
        owner.EffectSkin.Update();
        Assert.Equal(2, rig.Errors.Count);
    }

    [Fact]
    public void A_neighbour_leaving_the_zone_while_the_update_walks_the_list_cannot_break_the_walk()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { IsFounder = true }));
        Player second = rig.Add(2, 10, 0);
        Player third = rig.Add(3, 10, 0);
        Quiet(rig, owner);
        rig.Random.Enqueue(0, 0);

        // The list is walked from the end (3, then 2). While 3 is greeted, 2 leaves the zone.
        rig.World.OnChat = (player, _) =>
        {
            if (player.Id == third.Id)
            {
                rig.World.SimOf(second).ZoneId = 2;
            }
        };

        owner.EffectSkin.Update();

        Assert.Empty(rig.Errors);
        Assert.Equal(["Chat:3:MaiTienDung", "Chat:2:MaiTienDung"], rig.World.Calls);
    }

    [Fact]
    public void A_player_added_during_the_walk_is_not_visited_until_the_next_round()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { IsFounder = true }));
        rig.Add(2, 10, 0);
        Quiet(rig, owner);
        rig.Random.Enqueue(0, 0, 0);
        bool added = false;
        rig.World.OnChat = (_, _) =>
        {
            if (!added)
            {
                added = true;
                rig.Add(3, 10, 0);
            }
        };

        owner.EffectSkin.Update();
        Assert.Equal(["Chat:2:MaiTienDung"], rig.World.Calls);
        Assert.Empty(rig.Errors);

        rig.Advance(5001);
        Quiet(rig, owner);
        owner.EffectSkin.Update();
        Assert.Equal(3, rig.World.Calls.Count);
    }

    [Fact]
    public void Repeated_updates_with_a_changing_crowd_never_throw()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1, 0, 0, Stat(n => n with { IsFounder = true, PercentDirtyHpLoss = 1, IsXinbato = true, CoolDamagePercent = 1 }));
        for (int round = 0; round < 100; round++)
        {
            Player extra = rig.Add(100 + round, round % 50, 0);
            if (round % 3 == 0)
            {
                rig.World.SimOf(extra).ZoneId = 9;
            }

            rig.Random.Enqueue(Enumerable.Repeat((object)0, 800).ToArray());
            rig.Advance(11_000);
            Quiet(rig, owner);
            owner.EffectSkin.Update();
            owner.EffectSkill.Update();
        }

        Assert.Empty(rig.Errors);
    }
}
