using Nro.Game;

namespace Nro.Tests.Unit.Game.Effects;

/// <summary>
/// <see cref="EffectSkill"/> (Java <c>EffectSkill.java</c>): state defaults, <c>update()</c> expiry at the exact millisecond for every timed effect, the order and precedence of the
/// checks, <c>removeSkillEffectWhenDie</c>, <c>isHaveEffectSkill</c> and <c>dispose</c>, all on a fake clock.
/// </summary>
public class EffectSkillTests
{
    private sealed record TimedCase(
        string Name,
        Action<EffectSkill> SetFlag,
        Action<EffectSkill, long> SetLast,
        Action<EffectSkill, int> SetTime,
        Func<EffectSkill, bool> IsOn);

    private static readonly TimedCase[] Cases =
    [
        new("Monkey", e => e.IsMonkey = true, (e, t) => e.LastTimeUpMonkey = t, (e, d) => e.TimeMonkey = d, e => e.IsMonkey),
        new("UseSkillMonkey", e => e.IsUseSkillMonkey = true, (e, t) => e.LastTimeUseSkillMonkey = t, (e, d) => e.TimeUseSkillMonkey = d, e => e.IsUseSkillMonkey),
        new("PvpSuperDivineWater", e => e.IsPvpSuperDivineWater = true, (e, t) => e.LastTimePvpSuperDivineWater = t, (e, d) => e.TimePvpSuperDivineWater = d, e => e.IsPvpSuperDivineWater),
        new("PvpCommeson", e => e.IsPvpCommeson = true, (e, t) => e.LastTimePvpCommeson = t, (e, d) => e.TimePvpCommeson = d, e => e.IsPvpCommeson),
        new("Bottled", e => e.IsBottled = true, (e, t) => e.LastTimeBottled = t, (e, d) => e.TimeBottled = d, e => e.IsBottled),
        new("UsingMafuba", e => e.IsUsingMafuba = true, (e, t) => e.LastTimeUsingMafuba = t, (e, d) => e.TimeUsingMafuba = d, e => e.IsUsingMafuba),
        new("DamageBuff", e => e.IsDamageBuff = true, (e, t) => e.LastTimeDamageBuff = t, (e, d) => e.TimeDamageBuff = d, e => e.IsDamageBuff),
        new("Xinbato", e => e.IsXinbato = true, (e, t) => e.LastTimeXinbato = t, (e, d) => e.TimeXinbato = d, e => e.IsXinbato),
        new("FideLeader", e => e.IsFideLeader = true, (e, t) => e.LastTimeFideLeader = t, (e, d) => e.TimeFideLeader = d, e => e.IsFideLeader),
        new("Halloween", e => e.IsHalloween = true, (e, t) => e.LastTimeHalloween = t, (e, d) => e.TimeHalloween = d, e => e.IsHalloween),
        new("Chibi", e => e.IsChibi = true, (e, t) => e.LastTimeChibi = t, (e, d) => e.TimeChibi = d, e => e.IsChibi),
        new("Cumber", e => e.IsCumber = true, (e, t) => e.LastTimeCumber = t, (e, d) => e.TimeCumber = d, e => e.IsCumber),
        new("Cumber2", e => e.IsCumber2 = true, (e, t) => e.LastTimeCumber2 = t, (e, d) => e.TimeCumber2 = d, e => e.IsCumber2),
        new("Pain", e => e.IsPain = true, (e, t) => e.LastTimeUpPain = t, (e, d) => e.TimePain = d, e => e.IsPain),
        new("Kefla", e => e.IsKefla = true, (e, t) => e.LastTimeKefla = t, (e, d) => e.TimeKefla = d, e => e.IsKefla),
        new("Shield", e => e.IsShielding = true, (e, t) => e.LastTimeShieldUp = t, (e, d) => e.TimeShield = d, e => e.IsShielding),
        new("Evolution", e => e.IsEvolution = true, (e, t) => e.LastTimeEvolution = t, (e, d) => e.TimeEvolution = d, e => e.IsEvolution),
        new("PowerBurst", e => e.IsPowerBurst = true, (e, t) => e.LastTimePowerBurst = t, (e, d) => e.TimePowerBurst = d, e => e.IsPowerBurst),
        new("Binding", e => e.IsBinding = true, (e, t) => e.LastTimeBinding = t, (e, d) => e.TimeBinding = d, e => e.IsBinding),
        new("Stun", e => e.IsStun = true, (e, t) => e.LastTimeStartStun = t, (e, d) => e.TimeStun = d, e => e.IsStun),
        new("Hypnosis", e => e.IsHypnosis = true, (e, t) => e.LastTimeHypnosis = t, (e, d) => e.TimeHypnosis = d, e => e.IsHypnosis),
        new("TeleportBlind", e => e.IsTeleportBlind = true, (e, t) => e.LastTimeTeleportBlind = t, (e, d) => e.TimeTeleportBlind = d, e => e.IsTeleportBlind),
        new("Chocolate", e => e.IsChocolate = true, (e, t) => e.LastTimeChocolate = t, (e, d) => e.TimeChocolate = d, e => e.IsChocolate),
        new("Stone", e => e.IsStone = true, (e, t) => e.LastTimeStone = t, (e, d) => e.TimeStone = d, e => e.IsStone),
        new("Invisible", e => e.IsInvisible = true, (e, t) => e.LastTimeInvisible = t, (e, d) => e.TimeInvisible = d, e => e.IsInvisible),
        new("Intrinsic", e => e.IsIntrinsic = true, (e, t) => e.LastTimeUseSkill = t, (e, d) => e.Cooldown = d, e => e.IsIntrinsic),
        new("IntrinsicVip", e => e.IsIntrinsicVip = true, (e, t) => e.LastTimeUseSkillVip = t, (e, d) => e.CooldownVip = d, e => e.IsIntrinsicVip),
        new("Virus", e => e.IsVirus = true, (e, t) => e.LastTimeVirus = t, (e, d) => e.TimeVirus = d, e => e.IsVirus),
        new("Snowflake", e => e.IsSnowflake = true, (e, t) => e.LastTimeSnowflake = t, (e, d) => e.TimeSnowflake = d, e => e.IsSnowflake),
        new("Pumpkin", e => e.IsPumpkin = true, (e, t) => e.LastTimePumpkin = t, (e, d) => e.TimePumpkin = d, e => e.IsPumpkin),
    ];

    public static TheoryData<string> CaseNames => [.. Cases.Select(c => c.Name)];

    private static TimedCase Find(string name) => Cases.Single(c => c.Name == name);

    private static Player Skilled(EffectRig rig, long id = 1, Action<FakeWorld.Sim>? configure = null, Action<Player>? configurePlayer = null)
        => rig.Add(id, 0, 0, s =>
        {
            s.Skills[8] = new EffectSkillRef(800, 2);
            s.Skills[13] = new EffectSkillRef(1300, 3);
            s.Selected = new EffectSkillRef(900, 5);
            configure?.Invoke(s);
        }, configurePlayer);

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // State
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void A_new_effect_state_has_nothing_switched_on()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        EffectSkill e = p.EffectSkill;
        Assert.Same(p, e.Player);
        Assert.All(Cases, c => Assert.False(c.IsOn(e), c.Name));
        Assert.False(e.IsMabuHold || e.IsCharging || e.IsBound || e.IsTransformation || e.IsBodyChangeTechnique);
        Assert.Equal((0, 0, 0), (e.HpWhistlePercent, e.CountCharging, e.OneHpHitCount));
        Assert.Equal(0, e.LevelMonkey);
        Assert.Equal(0, e.TransformationLevel);
        Assert.Equal(0, e.LevelEvolution);
        Assert.Null(e.BindingPlayer);
        Assert.Null(e.BoundPlayer);
        Assert.Null(e.BoundMob);
        Assert.Null(e.MafubaUser);
    }

    [Fact]
    public void The_constructor_rejects_a_null_player()
    {
        Assert.Throws<ArgumentNullException>(() => new EffectSkill(null!));
    }

    [Fact]
    public void The_player_can_be_replaced_like_the_Java_setter()
    {
        var rig = new EffectRig();
        Player a = rig.Add(1);
        Player b = rig.Add(2);
        a.EffectSkill.Player = b;
        Assert.Same(b, a.EffectSkill.Player);
    }

    [Fact]
    public void Using_the_effects_before_the_environment_is_set_fails_clearly()
    {
        var player = new Player { Id = 1 };
        Assert.Throws<InvalidOperationException>(() => player.EffectSkill.Update());
        Assert.Throws<InvalidOperationException>(() => player.EffectSkill.RemoveSkillEffectWhenDie());
        Assert.False(player.EffectSkill.IsHaveEffectSkill()); // no holding effect: answered without needing the environment (as in Java)
        player.EffectSkill.IsStun = true;
        Assert.Throws<InvalidOperationException>(() => player.EffectSkill.IsHaveEffectSkill());
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Update: expiry boundaries
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void An_effect_stays_on_until_its_duration_has_passed_strictly(string name)
    {
        TimedCase c = Find(name);
        var rig = new EffectRig();
        Player p = Skilled(rig);
        EffectSkill e = p.EffectSkill;
        c.SetFlag(e);
        c.SetLast(e, rig.Now);
        c.SetTime(e, 5000);

        e.Update();
        Assert.True(c.IsOn(e), name + " right after start");

        rig.Advance(4999);
        e.Update();
        Assert.True(c.IsOn(e), name + " 1 ms before the end");

        rig.Advance(1);
        e.Update();
        Assert.True(c.IsOn(e), name + " exactly at the end");

        rig.Advance(1);
        e.Update();
        Assert.False(c.IsOn(e), name + " 1 ms after the end");
        Assert.Empty(rig.Errors);
    }

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void An_effect_whose_start_time_is_in_the_future_is_not_ended(string name)
    {
        TimedCase c = Find(name);
        var rig = new EffectRig();
        Player p = Skilled(rig);
        c.SetFlag(p.EffectSkill);
        c.SetLast(p.EffectSkill, rig.Now + 60_000); // the clock moved back after the effect started
        c.SetTime(p.EffectSkill, 1000);
        p.EffectSkill.Update();
        Assert.True(c.IsOn(p.EffectSkill), name);
    }

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void An_effect_that_is_switched_off_is_never_touched(string name)
    {
        TimedCase c = Find(name);
        var rig = new EffectRig();
        Player p = Skilled(rig);
        c.SetLast(p.EffectSkill, 1); // very old, but the flag is off
        c.SetTime(p.EffectSkill, 1);
        p.EffectSkill.Update();
        Assert.False(c.IsOn(p.EffectSkill), name);
        Assert.Empty(rig.World.Calls);
        Assert.Empty(rig.World.Packets);
    }

    [Fact]
    public void An_effect_with_a_zero_start_time_ends_at_the_first_update()
    {
        var rig = new EffectRig();
        Player p = Skilled(rig);
        p.EffectSkill.IsStun = true;
        p.EffectSkill.TimeStun = 5000;
        p.EffectSkill.Update();
        Assert.False(p.EffectSkill.IsStun);
    }

    [Fact]
    public void The_transformation_always_lasts_300_seconds_whatever_its_own_time_says()
    {
        var rig = new EffectRig();
        Player p = Skilled(rig);
        p.EffectSkill.IsTransformation = true;
        p.EffectSkill.TimeTransformation = 1;
        p.EffectSkill.LastTimeTransformation = rig.Now;

        rig.Advance(300_000);
        p.EffectSkill.Update();
        Assert.True(p.EffectSkill.IsTransformation);
        rig.Advance(1);
        p.EffectSkill.Update();
        Assert.False(p.EffectSkill.IsTransformation);
        Assert.Equal([(1L, 0)], rig.World.SavedBienHinh);
    }

    [Fact]
    public void The_whistle_effect_lasts_30_seconds()
    {
        var rig = new EffectRig();
        Player p = Skilled(rig);
        p.EffectSkill.HpWhistlePercent = 40;
        p.EffectSkill.LastTimeWhistle = rig.Now;

        rig.Advance(30_000);
        p.EffectSkill.Update();
        Assert.Equal(40, p.EffectSkill.HpWhistlePercent);
        rig.Advance(1);
        p.EffectSkill.Update();
        Assert.Equal(0, p.EffectSkill.HpWhistlePercent);
        Assert.Contains("InfoNv:1", rig.World.Calls);
    }

    [Fact]
    public void Several_effects_ending_together_are_all_removed_in_one_update_in_the_Java_order()
    {
        var rig = new EffectRig();
        Player p = Skilled(rig);
        EffectSkill e = p.EffectSkill;
        e.IsMonkey = true;
        e.IsStun = true;
        e.IsPumpkin = true;
        e.IsBottled = true;

        e.Update();

        Assert.False(e.IsMonkey || e.IsStun || e.IsPumpkin || e.IsBottled);

        // Java if-chain order: monkey (6 calls), then binh (costume), then the stun packet (no call), then bi ngo (costume).
        Assert.Equal(
            ["NotMonkey:1", "Caitrang:1", "Point:1", "InfoHpMp:1", "InfoNv:1", "EatPea:1", "Caitrang:1", "Caitrang:1"],
            rig.World.Calls);
        Assert.Equal([-45, -45, -124], rig.World.Packets.Select(pk => (int)pk.Command).ToArray());
    }

    [Fact]
    public void A_removal_that_creates_a_new_effect_does_not_end_it_in_the_same_update()
    {
        var rig = new EffectRig();
        Player p = Skilled(rig);
        p.EffectSkill.IsUseSkillMonkey = true;
        p.EffectSkill.TimeUseSkillMonkey = 1500;
        p.EffectSkill.LastTimeUseSkillMonkey = rig.Now - 5000;

        p.EffectSkill.Update();

        Assert.False(p.EffectSkill.IsUseSkillMonkey);
        Assert.True(p.EffectSkill.IsMonkey);
        Assert.Equal(rig.Now, p.EffectSkill.LastTimeUpMonkey);
    }

    [Fact]
    public void A_failing_removal_propagates_like_in_Java()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1); // no skills at all: SetIsMonkey needs the selected skill
        p.EffectSkill.IsUseSkillMonkey = true;
        p.EffectSkill.LastTimeUseSkillMonkey = 1;
        Assert.Throws<InvalidOperationException>(() => p.EffectSkill.Update());
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Update: bind (troi)
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Bind_is_released_when_the_bound_player_dies_even_if_the_time_is_not_up()
    {
        var rig = new EffectRig();
        Player binder = Skilled(rig, 1);
        Player bound = Skilled(rig, 2, configure: s => s.NPoint = s.NPoint with { Hp = 0 });
        binder.EffectSkill.IsBinding = true;
        binder.EffectSkill.LastTimeBinding = rig.Now;
        binder.EffectSkill.TimeBinding = 60_000;
        binder.EffectSkill.BoundPlayer = bound;
        bound.EffectSkill.IsBound = true;

        binder.EffectSkill.Update();

        Assert.False(binder.EffectSkill.IsBinding);
        Assert.Null(binder.EffectSkill.BoundPlayer);
        Assert.False(bound.EffectSkill.IsBound);
    }

    [Fact]
    public void A_dead_bound_player_triggers_the_release_even_if_the_is_binding_flag_is_off()
    {
        var rig = new EffectRig();
        Player binder = Skilled(rig, 1);
        Player bound = Skilled(rig, 2, configure: s => s.NPoint = s.NPoint with { Hp = 0 });
        binder.EffectSkill.BoundPlayer = bound;
        binder.EffectSkill.Update();
        Assert.Null(binder.EffectSkill.BoundPlayer);
        Assert.NotEmpty(rig.World.Packets);
    }

    [Fact]
    public void Bind_is_released_when_the_binder_is_held_by_another_effect_and_alive()
    {
        var rig = new EffectRig();
        Player binder = Skilled(rig, 1);
        binder.EffectSkill.IsBinding = true;
        binder.EffectSkill.LastTimeBinding = rig.Now;
        binder.EffectSkill.TimeBinding = 60_000;
        binder.EffectSkill.IsStun = true;
        binder.EffectSkill.LastTimeStartStun = rig.Now;
        binder.EffectSkill.TimeStun = 60_000;

        binder.EffectSkill.Update();

        Assert.False(binder.EffectSkill.IsBinding);
        Assert.True(binder.EffectSkill.IsStun); // the stun itself is not over
    }

    [Fact]
    public void A_dead_binder_is_not_released_by_the_held_rule()
    {
        var rig = new EffectRig();
        Player binder = Skilled(rig, 1, configure: s => s.NPoint = s.NPoint with { Hp = 0 });
        binder.EffectSkill.IsBinding = true;
        binder.EffectSkill.LastTimeBinding = rig.Now;
        binder.EffectSkill.TimeBinding = 60_000;
        binder.EffectSkill.IsStun = true;
        binder.EffectSkill.LastTimeStartStun = rig.Now;
        binder.EffectSkill.TimeStun = 60_000;

        binder.EffectSkill.Update();

        Assert.True(binder.EffectSkill.IsBinding);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // isHaveEffectSkill
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData("Stun")]
    [InlineData("TeleportBlind")]
    [InlineData("IsBound")]
    [InlineData("Hypnosis")]
    [InlineData("Stone")]
    [InlineData("MabuHold")]
    [InlineData("UseSkillMonkey")]
    public void IsHaveEffectSkill_is_true_for_the_seven_holding_effects_only_while_alive(string which)
    {
        var rig = new EffectRig();
        Player alive = rig.Add(1);
        Player dead = rig.Add(2, configure: s => s.NPoint = s.NPoint with { Hp = 0 });
        foreach (Player p in new[] { alive, dead })
        {
            EffectSkill e = p.EffectSkill;
            switch (which)
            {
                case "Stun": e.IsStun = true; break;
                case "TeleportBlind": e.IsTeleportBlind = true; break;
                case "IsBound": e.IsBound = true; break;
                case "Hypnosis": e.IsHypnosis = true; break;
                case "Stone": e.IsStone = true; break;
                case "MabuHold": e.IsMabuHold = true; break;
                default: e.IsUseSkillMonkey = true; break;
            }
        }

        Assert.True(alive.EffectSkill.IsHaveEffectSkill());
        Assert.False(dead.EffectSkill.IsHaveEffectSkill());
    }

    [Fact]
    public void IsHaveEffectSkill_is_false_for_effects_that_do_not_hold_the_player()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        Assert.False(p.EffectSkill.IsHaveEffectSkill());
        p.EffectSkill.IsMonkey = true;
        p.EffectSkill.IsBottled = true;
        p.EffectSkill.IsBinding = true;
        p.EffectSkill.IsShielding = true;
        Assert.False(p.EffectSkill.IsHaveEffectSkill());
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // removeSkillEffectWhenDie
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Death_removes_the_listed_effects_and_keeps_the_others()
    {
        var rig = new EffectRig();
        Player p = Skilled(rig, configure: s => s.NPoint = s.NPoint with { Hp = 0 }); // the player is dead, so finishing the monkey wind-up does not start a monkey
        EffectSkill e = p.EffectSkill;
        // listed in removeSkillEffectWhenDie
        e.IsMonkey = e.IsUseSkillMonkey = e.IsBottled = e.IsTransformation = e.IsEvolution = e.IsShielding = e.IsBinding = e.IsStun = true;
        e.IsHypnosis = e.IsTeleportBlind = e.IsDamageBuff = e.IsXinbato = e.IsFideLeader = e.IsStone = e.IsInvisible = e.IsMabuHold = true;
        e.IsVirus = e.IsSnowflake = e.IsPumpkin = e.IsPowerBurst = true;
        // not listed
        e.IsChibi = e.IsHalloween = e.IsChocolate = e.IsCumber = e.IsPain = e.IsKefla = e.IsUsingMafuba = e.IsPvpSuperDivineWater = e.IsPvpCommeson = e.IsIntrinsic = true;
        e.HpWhistlePercent = 9;

        e.RemoveSkillEffectWhenDie();

        Assert.False(e.IsMonkey || e.IsUseSkillMonkey || e.IsBottled || e.IsTransformation || e.IsEvolution || e.IsShielding || e.IsBinding || e.IsStun);
        Assert.False(e.IsHypnosis || e.IsTeleportBlind || e.IsDamageBuff || e.IsXinbato || e.IsFideLeader || e.IsStone || e.IsInvisible || e.IsMabuHold);
        Assert.False(e.IsVirus || e.IsSnowflake || e.IsPumpkin || e.IsPowerBurst);
        Assert.True(e.IsChibi && e.IsHalloween && e.IsChocolate && e.IsCumber && e.IsPain && e.IsKefla && e.IsUsingMafuba && e.IsPvpSuperDivineWater && e.IsPvpCommeson && e.IsIntrinsic);
        Assert.Equal(9, e.HpWhistlePercent);
    }

    [Fact]
    public void Death_with_nothing_on_does_nothing()
    {
        var rig = new EffectRig();
        Player p = Skilled(rig);
        p.EffectSkill.RemoveSkillEffectWhenDie();
        Assert.Empty(rig.World.Calls);
        Assert.Empty(rig.World.Packets);
    }

    [Theory]
    [InlineData(0, 11997)]
    [InlineData(1, 12000)]
    [InlineData(2, 12003)]
    public void Death_removes_the_burst_power_icon_of_the_players_gender(int gender, int itemTime)
    {
        var rig = new EffectRig();
        Player p = Skilled(rig, configurePlayer: x => x.Gender = (sbyte)gender);
        p.EffectSkill.IsPowerBurst = true;
        p.EffectSkill.RemoveSkillEffectWhenDie();
        Assert.Contains($"RemoveItemTime:1:{itemTime}", rig.World.Calls);
    }

    [Fact]
    public void Death_removes_no_burst_power_icon_for_an_unknown_gender()
    {
        var rig = new EffectRig();
        Player p = Skilled(rig, configurePlayer: x => x.Gender = 3);
        p.EffectSkill.IsPowerBurst = true;
        p.EffectSkill.RemoveSkillEffectWhenDie();
        Assert.False(p.EffectSkill.IsPowerBurst);
        Assert.DoesNotContain(rig.World.Calls, c => c.StartsWith("RemoveItemTime", StringComparison.Ordinal));
    }

    [Fact]
    public void Death_removes_the_shield_icon()
    {
        var rig = new EffectRig();
        Player p = Skilled(rig);
        p.EffectSkill.IsShielding = true;
        p.EffectSkill.RemoveSkillEffectWhenDie();
        Assert.Contains("RemoveItemTime:1:3784", rig.World.Calls);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // dispose
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Dispose_drops_the_references_keeps_the_player_and_stops_the_updates()
    {
        var rig = new EffectRig();
        Player p = Skilled(rig);
        Player other = rig.Add(2);
        EffectSkill e = p.EffectSkill;
        e.BoundPlayer = other;
        e.BindingPlayer = other;
        e.MafubaUser = other;
        e.BoundMob = new FakeMob();
        e.IsStun = true;

        e.Dispose();

        Assert.Null(e.BoundPlayer);
        Assert.Null(e.BindingPlayer);
        Assert.Null(e.MafubaUser);
        Assert.Null(e.BoundMob);
        Assert.Same(p, e.Player);
        e.Update();
        e.RemoveSkillEffectWhenDie();
        Assert.True(e.IsStun);
        Assert.Empty(rig.World.Packets);
    }

    [Fact]
    public void Dispose_twice_is_harmless()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        p.EffectSkill.Dispose();
        p.EffectSkill.Dispose();
        p.EffectSkill.Update();
    }
}
