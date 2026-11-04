using System.Runtime.CompilerServices;
using Nro.Game;
using Nro.Tests.Unit.Game.Effects;
using static Nro.Tests.Unit.Game.Effects.Pkt;

namespace Nro.Tests.Unit.Game.Effects.Services;

/// <summary>
/// <see cref="EffectSkillService"/>: the state each set/remove method writes, the exact packets and the exact order of the client commands, with a fake clock and a scripted random
/// source. Expected values are written by hand from <c>EffectSkillService.java</c>.
/// </summary>
public class EffectSkillServiceTests
{
    // Skill ids by template: Tai Tao Nang Luong 8, Bien Khi 13, Ma Phong Ba 26 (Skill.java); the skillId the client sees is the instance id and differs on purpose.
    private static readonly EffectSkillRef TaiTao = new(800, 2);
    private static readonly EffectSkillRef BienKhi = new(1300, 3);
    private static readonly EffectSkillRef MaPhongBa = new(2600, 4);
    private static readonly EffectSkillRef Selected = new(900, 5);

    private static void GiveSkills(FakeWorld.Sim sim)
    {
        sim.Skills[8] = TaiTao;
        sim.Skills[13] = BienKhi;
        sim.Skills[26] = MaPhongBa;
        sim.Selected = Selected;
    }

    private static Player AddSkilled(EffectRig rig, long id, int x = 0, int y = 0, Action<FakeWorld.Sim>? configure = null, Action<Player>? configurePlayer = null)
        => rig.Add(id, x, y, s => { GiveSkills(s); configure?.Invoke(s); }, configurePlayer);

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Packets
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void SendEffectUseSkill_goes_to_everybody_but_the_player_with_the_instance_skill_id()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 5);
        rig.Env.Skills.SendEffectUseSkill(p, 13);
        SentPacket packet = Assert.Single(rig.World.Packets);
        Assert.Equal("Except:5", packet.Scope);
        Assert.Equal(-45, packet.Command);
        Assert.Equal(Of(B(8), 5, S(1300)), packet.Data);
    }

    [Fact]
    public void SendEffectUseSkill_with_an_unknown_skill_logs_and_sends_nothing()
    {
        var rig = new EffectRig();
        Player p = rig.Add(5);
        rig.Env.Skills.SendEffectUseSkill(p, 13);
        Assert.Empty(rig.World.Packets);
        Assert.Single(rig.Errors);
    }

    [Fact]
    public void Plain_effect_packets_have_the_Java_layout()
    {
        var rig = new EffectRig();
        Player a = rig.Add(10);
        Player b = rig.Add(20);

        rig.Env.Skills.SendSelfEffect35(a);
        rig.Env.Skills.SendSelfEffect(a, 77);
        rig.Env.Skills.SendEffectPlayer(a, b, EffectSkillService.TURN_ON_EFFECT, EffectSkillService.HOLD_EFFECT);
        rig.Env.Skills.SendEffectPlayer(a, b, EffectSkillService.TURN_OFF_ALL_EFFECT, EffectSkillService.HOLD_EFFECT);

        Assert.All(rig.World.Packets, p => Assert.Equal(-124, p.Command));
        Assert.Equal(Of(B(1), B(0), B(35), 10, 10), rig.World.Packets[0].Data);
        Assert.Equal(Of(B(1), B(0), B(77), 10, 10), rig.World.Packets[1].Data);
        Assert.Equal(Of(B(1), B(0), B(32), 20, 10), rig.World.Packets[2].Data);
        Assert.Equal(Of(B(2), B(0), 20), rig.World.Packets[3].Data);
        Assert.All(rig.World.Packets, p => Assert.Equal("ZoneOf:10", p.Scope));
    }

    [Fact]
    public void SendEffectMob_writes_the_mob_id_as_a_byte_and_goes_to_the_mob_zone()
    {
        var rig = new EffectRig();
        Player a = rig.Add(10);
        rig.Env.Skills.SendEffectMob(a, new FakeMob { Id = 300 }, EffectSkillService.TURN_ON_EFFECT, 3);
        SentPacket packet = Assert.Single(rig.World.Packets);
        Assert.Equal("ZoneOfMob:300", packet.Scope);
        Assert.Equal(Of(B(1), B(1), B(3), B(300), 10), packet.Data);
    }

    [Fact]
    public void SendEffectBlindThaiDuongHaSan_lists_mobs_then_players_with_seconds()
    {
        var rig = new EffectRig();
        Player a = AddSkilled(rig, 10);
        Player b = rig.Add(20);
        Player c = rig.Add(30);
        rig.Env.Skills.SendSolarFlareBlindEffect(a, [b, c], [new FakeMob { Id = 4 }, new FakeMob { Id = 5 }], 4500);
        SentPacket packet = Assert.Single(rig.World.Packets);
        Assert.Equal(-45, packet.Command);
        Assert.Equal(Of(B(0), 10, S(900), B(2), B(4), B(4), B(5), B(4), B(2), 20, B(4), 30, B(4)), packet.Data);
    }

    [Fact]
    public void SendEffectBlindThaiDuongHaSan_without_a_selected_skill_logs_and_sends_nothing()
    {
        var rig = new EffectRig();
        Player a = rig.Add(10);
        rig.Env.Skills.SendSolarFlareBlindEffect(a, [], [], 1000);
        Assert.Empty(rig.World.Packets);
        Assert.Single(rig.Errors);
    }

    [Fact]
    public void Charge_packets_use_the_charge_skill_and_the_selected_skill()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 5);
        rig.Env.Skills.SendEffectStartCharge(p);
        rig.Env.Skills.SendEffectCharge(p);
        rig.Env.Skills.SendEffectStopCharge(p);
        rig.Env.Skills.SendEffectEndCharge(p);
        rig.Env.Skills.SendEffectMonkey(p);
        rig.Env.Skills.SendTransformationOrItemEffect(p);
        rig.Env.Skills.SendTransformationEffect(p);
        rig.Env.Skills.SendEvolutionEffect(p);

        Assert.All(rig.World.Packets, x => Assert.Equal(-45, x.Command));
        Assert.All(rig.World.Packets, x => Assert.Equal("ZoneOf:5", x.Scope));
        Assert.Equal(Of(B(6), 5, S(800)), rig.World.Packets[0].Data);
        Assert.Equal(Of(B(1), 5, S(800)), rig.World.Packets[1].Data);
        Assert.Equal(Of(B(3), 5, S(-1)), rig.World.Packets[2].Data);
        Assert.Equal(Of(B(5), 5, S(900)), rig.World.Packets[3].Data);
        Assert.Equal(Of(B(6), 5, S(1300)), rig.World.Packets[4].Data);
        Assert.Equal(Of(B(6), 5, S(95)), rig.World.Packets[5].Data);
        Assert.Equal(Of(B(6), 5, S(95)), rig.World.Packets[6].Data);
        Assert.Equal(Of(B(6), 5, S(95)), rig.World.Packets[7].Data);
    }

    [Fact]
    public void Charge_packets_for_a_player_without_the_skills_are_logged_not_thrown()
    {
        var rig = new EffectRig();
        Player p = rig.Add(5);
        rig.Env.Skills.SendEffectStartCharge(p);
        rig.Env.Skills.SendEffectCharge(p);
        rig.Env.Skills.SendEffectEndCharge(p);
        rig.Env.Skills.SendEffectMonkey(p);
        Assert.Empty(rig.World.Packets);
        Assert.Equal(4, rig.Errors.Count);
        rig.Env.Skills.SendEffectStopCharge(p); // needs no skill
        Assert.Single(rig.World.Packets);
    }

    [Fact]
    public void SendMobToSocola_sends_the_icon_then_marks_the_mob()
    {
        var rig = new EffectRig();
        Player p = rig.Add(5);
        var mob = new FakeMob { Id = 9 };
        rig.Env.Skills.TransformMobToChocolate(p, mob, 7000);
        SentPacket packet = Assert.Single(rig.World.Packets);
        Assert.Equal(-112, packet.Command);
        Assert.Equal(Of(B(1), B(9), S(4133)), packet.Data);
        Assert.Equal([(rig.Now, 7000)], mob.SetChocolateCalls);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Simple timed flags
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Pumpkin_set_stores_the_time_and_remove_clears_it_both_refresh_the_costume()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skills.SetPumpkin(p, 5000);
        Assert.True(p.EffectSkill.IsPumpkin);
        Assert.Equal((5000, rig.Now), (p.EffectSkill.TimePumpkin, p.EffectSkill.LastTimePumpkin));
        rig.Env.Skills.RemovePumpkin(p);
        Assert.False(p.EffectSkill.IsPumpkin);
        Assert.Equal(["Caitrang:1", "Caitrang:1"], rig.World.Calls);
    }

    [Fact]
    public void Flags_without_client_commands_set_and_remove_silently()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        EffectSkill e = p.EffectSkill;
        long now = rig.Now;

        rig.Env.Skills.SetSnowflake(p, 11);
        Assert.True(e.IsSnowflake);
        Assert.Equal((11, now), (e.TimeSnowflake, e.LastTimeSnowflake));
        rig.Env.Skills.RemoveSnowflake(p);
        Assert.False(e.IsSnowflake);

        rig.Env.Skills.SetVirus(p, 12);
        Assert.True(e.IsVirus);
        Assert.Equal((12, now), (e.TimeVirus, e.LastTimeVirus));
        rig.Env.Skills.RemoveVirus(p);
        Assert.False(e.IsVirus);

        rig.Env.Skills.SetDamageBuff(p, 13, 40);
        Assert.True(e.IsDamageBuff);
        Assert.Equal((13, 40, now), (e.TimeDamageBuff, e.DamageBuffPercent, e.LastTimeDamageBuff));
        rig.Env.Skills.RemoveDamageBuff(p);
        Assert.False(e.IsDamageBuff);

        rig.Env.Skills.SetXinbato(p, 14);
        Assert.True(e.IsXinbato);
        Assert.Equal((14, now), (e.TimeXinbato, e.LastTimeXinbato));
        rig.Env.Skills.RemoveXinbato(p);
        Assert.False(e.IsXinbato);

        rig.Env.Skills.SetFideLeader(p, 15);
        Assert.True(e.IsFideLeader);
        Assert.Equal((15, now), (e.TimeFideLeader, e.LastTimeFideLeader));
        rig.Env.Skills.RemoveFideLeader(p);
        Assert.False(e.IsFideLeader);

        Assert.Empty(rig.World.Calls);
        Assert.Empty(rig.World.Packets);
    }

    [Fact]
    public void MabuHold_set_and_remove()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1, x: 640);
        var hold = (MaBuHold)RuntimeHelpers.GetUninitializedObject(typeof(MaBuHold));
        p.PrecentMabuHold = 7;

        rig.Env.Skills.SetMabuHold(p, hold);
        Assert.Same(hold, p.MaBuHold);
        Assert.Equal(0, p.PrecentMabuHold);
        Assert.True(p.EffectSkill.IsMabuHold);
        Assert.Equal(["TypePk:1:5", "MabuHold:1:1:111:222"], rig.World.Calls);

        p.PrecentMabuHold = 9;
        rig.World.Calls.Clear();
        rig.Env.Skills.RemoveMabuHold(p);
        Assert.Null(p.MaBuHold);
        Assert.Equal(0, p.PrecentMabuHold);
        Assert.False(p.EffectSkill.IsMabuHold);
        Assert.Equal(["TypePk:1:0", "MabuHold:1:0:640:312"], rig.World.Calls);
    }

    [Fact]
    public void Pvp_super_divine_water_and_pvp_commeson_show_and_hide_the_same_item_time_icon()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skills.SetPvpSuperDivineWater(p, 90_000);
        Assert.True(p.EffectSkill.IsPvpSuperDivineWater);
        Assert.Equal((90_000, rig.Now), (p.EffectSkill.TimePvpSuperDivineWater, p.EffectSkill.LastTimePvpSuperDivineWater));
        rig.Env.Skills.RemovePvpSuperDivineWater(p);
        Assert.False(p.EffectSkill.IsPvpSuperDivineWater);

        rig.Env.Skills.SetPvpCommeson(p, 61_999);
        Assert.True(p.EffectSkill.IsPvpCommeson);
        Assert.Equal((61_999, rig.Now), (p.EffectSkill.TimePvpCommeson, p.EffectSkill.LastTimePvpCommeson));
        rig.Env.Skills.RemovePvpCommeson(p);
        Assert.False(p.EffectSkill.IsPvpCommeson);

        Assert.Equal(["ItemTime:1:2295:90", "ItemTime:1:2295:0", "ItemTime:1:2295:61", "ItemTime:1:2295:0"], rig.World.Calls);
    }

    [Fact]
    public void Halloween_with_a_fixed_outfit_does_not_draw_a_random_number()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skills.SetIsHalloween(p, 2, 1_800_000);
        Assert.True(p.EffectSkill.IsHalloween);
        Assert.Equal(2, p.EffectSkill.HalloweenOutfitId);
        Assert.Equal((1_800_000, rig.Now), (p.EffectSkill.TimeHalloween, p.EffectSkill.LastTimeHalloween));
        Assert.Equal(["ItemTime:1:5101:1800", "Caitrang:1"], rig.World.Calls);
        Assert.Empty(rig.Random.Calls);
    }

    [Fact]
    public void Halloween_with_minus_one_draws_the_outfit_from_nextInt_3()
    {
        var rig = new EffectRig();
        rig.Random.Enqueue(1);
        Player p = rig.Add(1);
        rig.Env.Skills.SetIsHalloween(p, -1, 1000);
        Assert.Equal(1, p.EffectSkill.HalloweenOutfitId);
        Assert.Equal([("NextInt", (object?)3)], rig.Random.Calls);
    }

    [Fact]
    public void Halloween_is_blocked_and_unblocked_by_the_resistance_stat()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1, configure: s => s.NPoint = s.NPoint with { IsBoneTransformationImmune = true });
        p.EffectSkill.IsHalloween = true;
        rig.Env.Skills.SetIsHalloween(p, 0, 1000);
        Assert.Equal(0, p.EffectSkill.TimeHalloween);
        rig.Env.Skills.RemoveHalloween(p);
        Assert.True(p.EffectSkill.IsHalloween); // resistant players keep the flag, as in Java
        Assert.Empty(rig.World.Calls);

        Player q = rig.Add(2);
        q.EffectSkill.IsHalloween = true;
        rig.Env.Skills.RemoveHalloween(q);
        Assert.False(q.EffectSkill.IsHalloween);
        Assert.Equal(["Caitrang:2"], rig.World.Calls);
    }

    [Fact]
    public void Chibi_with_a_draw_of_three_recalculates_and_fills_hp()
    {
        var rig = new EffectRig();
        rig.Random.Enqueue(3);
        Player p = rig.Add(1, configure: s => s.NPoint = s.NPoint with { Hp = 10, HpMax = 777 });
        rig.Env.Skills.SetChibiBird(p, 60_000);
        Assert.True(p.EffectSkill.IsChibi);
        Assert.Equal(3, p.TypeChibi);
        Assert.Equal((60_000, rig.Now), (p.EffectSkill.TimeChibi, p.EffectSkill.LastTimeChibi));
        Assert.Equal(["CalPoint:1", "SetHp:1:777", "Point:1", "InfoNv:1", "ItemTime:1:433:60", "Chibi:1", "ChibiFollow:1"], rig.World.Calls);
        Assert.Equal([("NextInt", (object?)4)], rig.Random.Calls);
    }

    [Fact]
    public void Chibi_with_another_draw_does_not_touch_hp_and_remove_resets_the_type()
    {
        var rig = new EffectRig();
        rig.Random.Enqueue(2);
        Player p = rig.Add(1);
        rig.Env.Skills.SetChibiBird(p, 5000);
        Assert.Equal(2, p.TypeChibi);
        Assert.Equal(["ItemTime:1:433:5", "Chibi:1", "ChibiFollow:1"], rig.World.Calls);

        rig.World.Calls.Clear();
        rig.Env.Skills.RemoveChibiBird(p);
        Assert.False(p.EffectSkill.IsChibi);
        Assert.Equal(-1, p.TypeChibi);
        Assert.Equal(["ItemTime:1:433:0", "Chibi:1", "ChibiFollow:1"], rig.World.Calls);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Mafuba
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(0, 11175)]
    [InlineData(1, 11166)]
    [InlineData(2, 11166)]
    public void Set_bottled_picks_the_icon_from_the_attackers_skill_variant(int typeItem, int icon)
    {
        var rig = new EffectRig();
        Player attacker = rig.Add(1, configure: s => s.NewSkill = new EffectNewSkillState { TypeItem = typeItem });
        Player victim = rig.Add(2);
        rig.Env.Skills.SetBottled(attacker, victim, 33_000);

        Assert.True(victim.EffectSkill.IsBottled);
        Assert.Equal(typeItem, victim.EffectSkill.BottledType);
        Assert.Equal(33_000, victim.EffectSkill.TimeBottled);
        Assert.Same(attacker, victim.EffectSkill.MafubaUser);
        Assert.Equal(rig.Now, victim.EffectSkill.LastTimeBottled);
        Assert.Equal([$"ItemTime:2:{icon}:33", "Caitrang:2"], rig.World.Calls);
    }

    [Fact]
    public void Bottled_down_clears_the_flag_but_keeps_the_owner_reference()
    {
        var rig = new EffectRig();
        Player attacker = rig.Add(1);
        Player victim = rig.Add(2);
        rig.Env.Skills.SetBottled(attacker, victim, 1000);
        rig.World.Calls.Clear();
        rig.Env.Skills.RemoveBottled(victim);
        Assert.False(victim.EffectSkill.IsBottled);
        Assert.Same(attacker, victim.EffectSkill.MafubaUser);
        Assert.Equal(["Caitrang:2"], rig.World.Calls);
    }

    [Fact]
    public void Start_using_mafuba_stores_the_charge_time()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skills.StartUseMafuba(p, 2500);
        Assert.True(p.EffectSkill.IsUsingMafuba);
        Assert.Equal((2500, rig.Now), (p.EffectSkill.TimeUsingMafuba, p.EffectSkill.LastTimeUsingMafuba));
    }

    [Fact]
    public void Finish_using_mafuba_bottles_the_targets_and_pulls_players_next_to_the_owner()
    {
        var rig = new EffectRig();
        Player t1 = rig.Add(2, 10, 10);
        Player t2 = rig.Add(3, 20, 20);
        var mob = new FakeMob { Id = 8 };
        rig.World.Mobs.Add(mob);
        Player owner = rig.Add(1, 100, 50, s => s.NewSkill = new EffectNewSkillState
        {
            TypeItem = 1,
            Dir = -1,
            PlayersTarget = [t1, t2],
            MobsTarget = [mob],
        });
        owner.EffectSkill.IsUsingMafuba = true;

        rig.Env.Skills.FinishUseMafuba(owner);

        Assert.False(owner.EffectSkill.IsUsingMafuba);
        Assert.True(t1.EffectSkill.IsBottled);
        Assert.Equal(22_000, t1.EffectSkill.TimeBottled);
        Assert.Equal(22_000, t2.EffectSkill.TimeBottled);
        Assert.Equal(
            ["ItemTime:2:11166:22", "Caitrang:2", "SetPos:2:25:50", "ItemTime:3:11166:22", "Caitrang:3", "SetPos:3:25:50"],
            rig.World.Calls);

        // Java quirk: the mob's duration comes from the OWNER's own BottledType (0, never set), not from the skill variant used for the players.
        Assert.Equal([(1L, rig.Now, 11_000)], mob.SetBottleCalls);
    }

    [Fact]
    public void Finish_using_mafuba_facing_right_pulls_75_to_the_right_and_normal_variant_lasts_11_s()
    {
        var rig = new EffectRig();
        Player t = rig.Add(2);
        Player owner = rig.Add(1, 100, 50, s => s.NewSkill = new EffectNewSkillState { TypeItem = 0, Dir = 1, PlayersTarget = [t] });
        rig.Env.Skills.FinishUseMafuba(owner);
        Assert.Equal(11_000, t.EffectSkill.TimeBottled);
        Assert.Contains("SetPos:2:175:50", rig.World.Calls);
    }

    [Fact]
    public void Finish_using_mafuba_does_not_move_players_inside_the_black_ball_war_maps()
    {
        var rig = new EffectRig();
        Player t = rig.Add(2);
        Player owner = rig.Add(1, 100, 50, s =>
        {
            s.MapId = 85;
            s.NewSkill = new EffectNewSkillState { PlayersTarget = [t] };
        });
        rig.Env.Skills.FinishUseMafuba(owner);
        Assert.True(t.EffectSkill.IsBottled);
        Assert.DoesNotContain(rig.World.Calls, c => c.StartsWith("SetPos", StringComparison.Ordinal));
    }

    [Fact]
    public void Finish_using_mafuba_with_no_targets_only_clears_the_flag_and_one_bad_target_does_not_stop_the_rest()
    {
        var rig = new EffectRig();
        Player owner = rig.Add(1);
        owner.EffectSkill.IsUsingMafuba = true;
        rig.Env.Skills.FinishUseMafuba(owner);
        Assert.False(owner.EffectSkill.IsUsingMafuba);
        Assert.Empty(rig.World.Calls);

        var stranger = new Player { Id = 99, EffectSkill = null! }; // a broken target: using it throws, like a Java NullPointerException
        Player good = rig.Add(2);
        Player owner2 = rig.Add(3, configure: s => s.NewSkill = new EffectNewSkillState { PlayersTarget = [stranger, good] });
        rig.Env.Skills.FinishUseMafuba(owner2);
        Assert.Single(rig.Errors);
        Assert.True(good.EffectSkill.IsBottled);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Intrinsic, stone, body change, invisibility
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Intrinsic_cooldown_set_and_release_through_the_skill_lookup()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1);
        rig.Env.Skills.SetIntrinsic(p, 13, 4000, 555);
        Assert.True(p.EffectSkill.IsIntrinsic);
        Assert.Equal((13, 4000, 555L), (p.EffectSkill.SkillId, p.EffectSkill.Cooldown, p.EffectSkill.LastTimeUseSkill));
        rig.Env.Skills.ReleaseCooldownSkillByIntrinsic(p);
        Assert.False(p.EffectSkill.IsIntrinsic);

        rig.Env.Skills.SetIntrinsicVip(p, 8, 6000, 777);
        Assert.True(p.EffectSkill.IsIntrinsicVip);
        Assert.Equal((8, 6000, 777L), (p.EffectSkill.SkillIdVip, p.EffectSkill.CooldownVip, p.EffectSkill.LastTimeUseSkillVip));
        rig.Env.Skills.ReleaseCooldownSkillByIntrinsicVip(p);
        Assert.False(p.EffectSkill.IsIntrinsicVip);

        Assert.Equal(["ReleaseCooldown:1:1300", "ReleaseCooldown:1:800"], rig.World.Calls);
    }

    [Fact]
    public void Releasing_an_intrinsic_cooldown_of_an_unknown_skill_passes_null()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skills.SetIntrinsic(p, 40, 1, 0);
        rig.Env.Skills.ReleaseCooldownSkillByIntrinsic(p);
        Assert.Equal(["ReleaseCooldown:1:none"], rig.World.Calls);
    }

    [Fact]
    public void Stone_in_a_normal_map_only_sets_state_icon_costume_and_effect()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skills.SetIsStone(p, 6000);
        Assert.True(p.EffectSkill.IsStone);
        Assert.Equal((6000, rig.Now), (p.EffectSkill.TimeStone, p.EffectSkill.LastTimeStone));
        Assert.Equal(["ItemTime:1:4392:6", "Caitrang:1"], rig.World.Calls);
        Assert.Equal(Of(B(1), B(0), B(42), 1, 1), Assert.Single(rig.World.Packets).Data);

        rig.World.Calls.Clear();
        rig.Env.Skills.RemoveStone(p);
        Assert.False(p.EffectSkill.IsStone);
        Assert.Equal(["Caitrang:1"], rig.World.Calls);
        Assert.Equal(Of(B(0), B(0), B(42), 1, 1), rig.World.Packets[1].Data);
    }

    [Theory]
    [InlineData(113, false)]
    [InlineData(114, true)]
    [InlineData(120, true)]
    [InlineData(121, false)]
    public void Stone_halves_hp_only_in_the_majin_buu_12h_maps(int mapId, bool halves)
    {
        var rig = new EffectRig();
        Player p = rig.Add(1, configure: s =>
        {
            s.MapId = mapId;
            s.NPoint = s.NPoint with { Hp = 1001 };
        });
        rig.Env.Skills.SetIsStone(p, 1000);
        Assert.Equal(halves ? 500 : 1001, rig.World.SimOf(p).NPoint.Hp);
        Assert.Equal(halves, rig.World.Calls.Contains("HalveHp:1"));
        Assert.Equal(halves, rig.World.Calls.Contains("InfoHp:1"));
    }

    [Fact]
    public void Stone_for_a_player_without_a_zone_throws_like_the_Java_null_pointer()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1, configure: s => s.HasZone = false);
        Assert.Throws<InvalidOperationException>(() => rig.Env.Skills.SetIsStone(p, 1000));
        Assert.False(p.EffectSkill.IsStone);
    }

    [Fact]
    public void Body_change_technique_notifies_every_player_of_the_zone_from_the_last_to_the_first()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Add(2);
        rig.Add(3);
        rig.Env.Skills.SetIsBodyChangeTechnique(p);
        Assert.True(p.EffectSkill.IsBodyChangeTechnique);
        Assert.Equal(
            [
                "Notice:1:Bạn đã bị biến thành Tiểu Đội Trưởng",
                "TypePk:1:5",
                "InfoUpdate:1:3:!Tiểu đội trưởng:180:181:182",
                "InfoUpdate:1:2:!Tiểu đội trưởng:180:181:182",
                "InfoUpdate:1:1:!Tiểu đội trưởng:180:181:182",
            ],
            rig.World.Calls);
    }

    [Fact]
    public void Removing_the_body_change_clears_the_invisibility_flag_not_its_own_flag()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        p.EffectSkill.IsBodyChangeTechnique = true;
        p.EffectSkill.IsInvisible = true;
        rig.Env.Skills.RemoveBodyChangeTechnique(p);
        Assert.True(p.EffectSkill.IsBodyChangeTechnique);
        Assert.False(p.EffectSkill.IsInvisible);
        Assert.Equal(["TypePk:1:0"], rig.World.Calls);
    }

    [Fact]
    public void Invisible_moves_the_player_far_below_the_map_and_back()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1, 321, 654);
        rig.Env.Skills.SetInvisible(p, 1500);
        Assert.True(p.EffectSkill.IsInvisible);
        Assert.Equal((1500, rig.Now), (p.EffectSkill.TimeInvisible, p.EffectSkill.LastTimeInvisible));
        rig.Env.Skills.RemoveInvisibility(p);
        Assert.False(p.EffectSkill.IsInvisible);
        Assert.Equal(["SetPos2:1:321:10000", "SetPos2:1:321:654"], rig.World.Calls);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Monkey
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void SetIsMonkey_doubles_hp_and_sets_time_and_level_from_the_selected_skill()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1);
        rig.Env.Skills.SetIsMonkey(p);

        Assert.True(p.EffectSkill.IsMonkey);
        Assert.Equal(100_000, p.EffectSkill.TimeMonkey); // (5 + 5) * 10000
        Assert.Equal(5, p.EffectSkill.LevelMonkey);
        Assert.Equal(rig.Now, p.EffectSkill.LastTimeUpMonkey);
        Assert.Equal(["SetHp:1:2000", "Caitrang:1", "InfoHpMp:1", "Point:1", "InfoNv:1", "EatPea:1"], rig.World.Calls);
        Assert.Equal(Of(B(6), 1, S(1300)), Assert.Single(rig.World.Packets).Data);
    }

    [Fact]
    public void SetIsMonkey_with_the_full_cadic_set_lasts_five_times_longer()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1, configure: s => s.Clothes = new EffectSetClothesState { Cadic = 5 });
        rig.Env.Skills.SetIsMonkey(p);
        Assert.Equal(500_000, p.EffectSkill.TimeMonkey); // 100000 * 5
    }

    [Fact]
    public void SetIsMonkey_skips_the_hp_mp_refresh_for_pets_and_clones()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1, configurePlayer: x => x.IsPet = true);
        rig.Env.Skills.SetIsMonkey(p);
        Assert.DoesNotContain("InfoHpMp:1", rig.World.Calls);
        Assert.Contains("Point:1", rig.World.Calls);
    }

    [Fact]
    public void SetIsMonkey_level_wraps_to_a_signed_byte_like_the_Java_cast()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1, configure: s =>
        {
            s.Skills[13] = BienKhi;
            s.Selected = new EffectSkillRef(1, 200);
        });
        rig.Env.Skills.SetIsMonkey(p);
        Assert.Equal(-56, p.EffectSkill.LevelMonkey);
    }

    [Fact]
    public void SetIsMonkey_without_a_selected_skill_throws()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1, configure: s => s.Skills[13] = BienKhi);
        Assert.Throws<InvalidOperationException>(() => rig.Env.Skills.SetIsMonkey(p));
        Assert.False(p.EffectSkill.IsMonkey);
    }

    [Theory]
    [InlineData(false, 6_000_000_000L)]
    [InlineData(true, 2_147_483_647L)]
    public void Hp_amounts_follow_the_read_int_switch(bool readInt, long expected)
    {
        var rig = new EffectRig(readInt);
        Player p = AddSkilled(rig, 1, configure: s => s.NPoint = s.NPoint with { Hp = 3_000_000_000L, HpMax = 4_000_000_000L });
        rig.Env.Skills.SetIsMonkey(p);
        Assert.Equal($"SetHp:1:{expected}", rig.World.Calls[0]);
    }

    [Fact]
    public void MonkeyDown_clamps_hp_to_the_maximum_and_refreshes_everything()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1, configure: s => s.NPoint = s.NPoint with { Hp = 2000, HpMax = 1000 });
        p.EffectSkill.IsMonkey = true;
        p.EffectSkill.LevelMonkey = 4;
        rig.Env.Skills.MonkeyDown(p);

        Assert.False(p.EffectSkill.IsMonkey);
        Assert.Equal(0, p.EffectSkill.LevelMonkey);
        Assert.Equal(["SetHp:1:1000", "NotMonkey:1", "Caitrang:1", "Point:1", "InfoHpMp:1", "InfoNv:1", "EatPea:1"], rig.World.Calls);
        Assert.Equal(Of(B(5), 1, S(900)), rig.World.Packets[0].Data);
        Assert.Equal(Of(B(6), 1, S(1300)), rig.World.Packets[1].Data);
    }

    [Fact]
    public void MonkeyDown_with_hp_at_or_below_the_maximum_does_not_set_hp()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1, configure: s => s.NPoint = s.NPoint with { Hp = 1000, HpMax = 1000 });
        rig.Env.Skills.MonkeyDown(p);
        Assert.DoesNotContain(rig.World.Calls, c => c.StartsWith("SetHp", StringComparison.Ordinal));
    }

    [Fact]
    public void StartUseSkillMonkey_for_a_player_freezes_for_1_5_seconds()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1);
        rig.Env.Skills.StartUseSkillMonkey(p);
        Assert.True(p.EffectSkill.IsUseSkillMonkey);
        Assert.Equal((1500, rig.Now), (p.EffectSkill.TimeUseSkillMonkey, p.EffectSkill.LastTimeUseSkillMonkey));
        Assert.False(p.EffectSkill.IsMonkey);
        Assert.Equal(["Speed:1:0"], rig.World.Calls);
    }

    [Fact]
    public void StartUseSkillMonkey_for_a_boss_turns_into_a_monkey_at_once()
    {
        var rig = new EffectRig();
        Player boss = AddSkilled(rig, 1, configurePlayer: x => x.IsBoss = true);
        rig.Env.Skills.StartUseSkillMonkey(boss);
        Assert.True(boss.EffectSkill.IsMonkey);
        Assert.False(boss.EffectSkill.IsUseSkillMonkey);
        Assert.DoesNotContain("Speed:1:0", rig.World.Calls);
    }

    [Fact]
    public void FinishUseMonkey_restores_speed_and_transforms_only_a_living_player()
    {
        var rig = new EffectRig();
        Player alive = AddSkilled(rig, 1);
        alive.EffectSkill.IsUseSkillMonkey = true;
        rig.Env.Skills.FinishUseMonkey(alive);
        Assert.False(alive.EffectSkill.IsUseSkillMonkey);
        Assert.True(alive.EffectSkill.IsMonkey);
        Assert.Equal("Speed:1:-1", rig.World.Calls[0]);

        Player dead = AddSkilled(rig, 2, configure: s => s.NPoint = s.NPoint with { Hp = 0 });
        dead.EffectSkill.IsUseSkillMonkey = true;
        rig.Env.Skills.FinishUseMonkey(dead);
        Assert.False(dead.EffectSkill.IsUseSkillMonkey);
        Assert.False(dead.EffectSkill.IsMonkey);
    }

    [Theory]
    [InlineData(0, 60_000, 1)]
    [InlineData(3, 80_000, 3)]
    public void Mid_autumn_monkey_uses_a_default_level_when_the_skill_has_no_point(int point, int expectedTime, int expectedLevel)
    {
        var rig = new EffectRig();
        Player p = rig.Add(1, configure: s => s.Skills[13] = new EffectSkillRef(1300, point));
        rig.Env.Skills.SetMoonFestivalMonkey(p);
        Assert.True(p.EffectSkill.IsMonkey);
        Assert.Equal(expectedTime, p.EffectSkill.TimeMonkey);
        Assert.Equal(expectedLevel, p.EffectSkill.LevelMonkey);
        Assert.Equal(["SetHp:1:2000"], rig.World.Calls);
    }

    [Fact]
    public async Task Mid_autumn_monkey_async_waits_200_ms_before_applying()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1, configure: s => s.Skills[13] = BienKhi);
        Task task = rig.Env.Skills.SetMoonFestivalMonkeyAsync(p);
        Assert.False(task.IsCompleted);
        Assert.False(p.EffectSkill.IsMonkey);
        Assert.Equal([TimeSpan.FromMilliseconds(200)], rig.World.PendingDelays);

        rig.Advance(200);
        rig.World.CompleteAllDelays();
        await task;
        Assert.True(p.EffectSkill.IsMonkey);
        Assert.Equal(rig.Now, p.EffectSkill.LastTimeUpMonkey);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Bind, hypnosis, stun, chocolate, teleport blind, whistle, charge, shield
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Binding_set_methods_store_their_data()
    {
        var rig = new EffectRig();
        Player binder = rig.Add(1);
        Player bound = rig.Add(2);
        rig.Env.Skills.SetBinding(binder, 500, 6000);
        Assert.True(binder.EffectSkill.IsBinding);
        Assert.Equal((500L, 6000), (binder.EffectSkill.LastTimeBinding, binder.EffectSkill.TimeBinding));
        rig.Env.Skills.SetBound(bound, binder, 999, 888);
        Assert.True(bound.EffectSkill.IsBound);
        Assert.Same(binder, bound.EffectSkill.BindingPlayer);
    }

    [Fact]
    public void Remove_binding_frees_the_mob_the_player_and_the_clone_in_that_order()
    {
        var rig = new EffectRig();
        Player binder = rig.Add(1);
        Player bound = rig.Add(2);
        var clone = new BodyClone { Id = 3 };
        rig.World.Adopt(rig.Env, clone);
        var mob = new FakeMob { Id = 4 };

        binder.BodyClone = clone;
        binder.EffectSkill.IsBinding = true;
        binder.EffectSkill.BoundPlayer = bound;
        binder.EffectSkill.BoundMob = mob;
        bound.EffectSkill.IsBound = true;
        bound.EffectSkill.BindingPlayer = binder;
        clone.EffectSkill.IsBinding = true;

        rig.Env.Skills.RemoveBinding(binder);

        Assert.Equal(1, mob.RemoveBindCalls);
        Assert.False(binder.EffectSkill.IsBinding);
        Assert.Null(binder.EffectSkill.BoundPlayer);
        Assert.Null(binder.EffectSkill.BoundMob);
        Assert.False(bound.EffectSkill.IsBound);
        Assert.Null(bound.EffectSkill.BindingPlayer);
        Assert.False(clone.EffectSkill.IsBinding);

        // Packets: bound freed (hold off, on bound), binder hold off, clone hold off.
        Assert.Equal(3, rig.World.Packets.Count);
        Assert.Equal(Of(B(0), B(0), B(32), 2, 2), rig.World.Packets[0].Data);
        Assert.Equal(Of(B(0), B(0), B(32), 1, 1), rig.World.Packets[1].Data);
        Assert.Equal(Of(B(0), B(0), B(32), 3, 3), rig.World.Packets[2].Data);
    }

    [Fact]
    public void Remove_binding_leaves_the_clone_alone_when_it_is_not_binding()
    {
        var rig = new EffectRig();
        Player binder = rig.Add(1);
        var clone = new BodyClone { Id = 3 };
        rig.World.Adopt(rig.Env, clone);
        binder.BodyClone = clone;
        binder.EffectSkill.IsBinding = true;
        rig.Env.Skills.RemoveBinding(binder);
        Assert.Single(rig.World.Packets);
    }

    [Fact]
    public void Remove_bound_for_a_null_player_does_nothing()
    {
        var rig = new EffectRig();
        rig.Env.Skills.RemoveBound(null!);
        Assert.Empty(rig.World.Packets);
    }

    [Fact]
    public void Hypnosis_stun_and_teleport_blind_send_their_effect_when_removed_or_started()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skills.SetHypnosis(p, 100, 2000);
        Assert.True(p.EffectSkill.IsHypnosis);
        Assert.Equal((100L, 2000), (p.EffectSkill.LastTimeHypnosis, p.EffectSkill.TimeHypnosis));
        Assert.Empty(rig.World.Packets);
        rig.Env.Skills.RemoveHypnosis(p);
        Assert.False(p.EffectSkill.IsHypnosis);
        Assert.Equal(Of(B(0), B(0), B(41), 1, 1), rig.World.Packets[0].Data);

        rig.Env.Skills.StartStun(p, 200, 3000);
        Assert.True(p.EffectSkill.IsStun);
        Assert.Equal((200L, 3000), (p.EffectSkill.LastTimeStartStun, p.EffectSkill.TimeStun));
        Assert.Equal(Of(B(1), B(0), B(40), 1, 1), rig.World.Packets[1].Data);
        rig.Env.Skills.RemoveStun(p);
        Assert.False(p.EffectSkill.IsStun);
        Assert.Equal(Of(B(0), B(0), B(40), 1, 1), rig.World.Packets[2].Data);

        rig.Env.Skills.SetTeleportBlind(p, 300, 500);
        Assert.True(p.EffectSkill.IsTeleportBlind);
        Assert.Equal((300L, 500), (p.EffectSkill.LastTimeTeleportBlind, p.EffectSkill.TimeTeleportBlind));
        Assert.Equal(3, rig.World.Packets.Count);
        rig.Env.Skills.RemoveTeleportBlind(p);
        Assert.False(p.EffectSkill.IsTeleportBlind);
        Assert.Equal(Of(B(0), B(0), B(40), 1, 1), rig.World.Packets[3].Data);
    }

    [Fact]
    public void Chocolate_resets_the_one_hp_hit_counter_and_remove_refreshes_the_costume()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        p.EffectSkill.OneHpHitCount = 9;
        rig.Env.Skills.SetChocolate(p, 400, 5000);
        Assert.True(p.EffectSkill.IsChocolate);
        Assert.Equal((400L, 5000, 0), (p.EffectSkill.LastTimeChocolate, p.EffectSkill.TimeChocolate, p.EffectSkill.OneHpHitCount));
        rig.Env.Skills.RemoveChocolate(p);
        Assert.False(p.EffectSkill.IsChocolate);
        Assert.Equal(["Caitrang:1"], rig.World.Calls);
    }

    [Fact]
    public void Whistle_start_and_remove()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skills.StartWhistle(p, 30);
        Assert.Equal((30, rig.Now), (p.EffectSkill.HpWhistlePercent, p.EffectSkill.LastTimeWhistle));
        rig.Env.Skills.RemoveWhistle(p);
        Assert.Equal(0, p.EffectSkill.HpWhistlePercent);
        Assert.Equal(Of(B(0), B(0), B(39), 1, 1), Assert.Single(rig.World.Packets).Data);
        Assert.Equal(["Point:1", "InfoNv:1"], rig.World.Calls);
    }

    [Fact]
    public void Charge_starts_once_and_stop_resets_the_counter()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1);
        rig.Env.Skills.StartCharge(p);
        rig.Env.Skills.StartCharge(p);
        Assert.True(p.EffectSkill.IsCharging);
        Assert.Single(rig.World.Packets);
        Assert.Equal(Of(B(1), 1, S(800)), rig.World.Packets[0].Data);

        p.EffectSkill.CountCharging = 8;
        rig.Env.Skills.StopCharge(p);
        Assert.False(p.EffectSkill.IsCharging);
        Assert.Equal(0, p.EffectSkill.CountCharging);
        Assert.Equal(Of(B(3), 1, S(-1)), rig.World.Packets[1].Data);
    }

    [Fact]
    public void Shield_duration_comes_from_the_selected_skill_and_breaking_it_tells_the_player()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1);
        rig.Env.Skills.SetStartShield(p);
        Assert.True(p.EffectSkill.IsShielding);
        Assert.Equal((35_000, rig.Now), (p.EffectSkill.TimeShield, p.EffectSkill.LastTimeShieldUp));

        rig.Env.Skills.BreakShield(p);
        Assert.False(p.EffectSkill.IsShielding);
        Assert.Equal(Of(B(0), B(0), B(33), 1, 1), Assert.Single(rig.World.Packets).Data);
        Assert.Equal(["Notice:1:Khiên năng lượng đã bị vỡ!", "RemoveItemTime:1:3784"], rig.World.Calls);
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Transformations
    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------

    private static readonly string[] CommonDownCalls = ["NotMonkey:1", "Caitrang:1", "Point:1", "InfoHpMp:1", "InfoNv:1", "EatPea:1"];

    [Fact]
    public void Cumber_pain_kefla_and_duoi_khi_go_down_with_the_same_sequence()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1);
        p.EffectSkill.IsCumber = true;
        p.EffectSkill.IsCumber2 = true;
        p.EffectSkill.IsPain = true;
        p.EffectSkill.IsKefla = true;

        rig.Env.Skills.RemoveCumber(p);
        Assert.False(p.EffectSkill.IsCumber);
        Assert.Equal(CommonDownCalls, rig.World.Calls);
        Assert.Equal(2, rig.World.Packets.Count);
        Assert.Equal(Of(B(5), 1, S(900)), rig.World.Packets[0].Data);
        Assert.Equal(Of(B(6), 1, S(95)), rig.World.Packets[1].Data);

        rig.World.Calls.Clear();
        rig.Env.Skills.RemoveCumber2(p);
        Assert.False(p.EffectSkill.IsCumber2);
        Assert.Equal(CommonDownCalls, rig.World.Calls);

        rig.World.Calls.Clear();
        rig.Env.Skills.RemovePain(p);
        Assert.False(p.EffectSkill.IsPain);
        Assert.Equal(CommonDownCalls, rig.World.Calls);

        rig.World.Calls.Clear();
        rig.Env.Skills.RemoveKefla(p);
        Assert.False(p.EffectSkill.IsKefla);
        Assert.Equal(CommonDownCalls, rig.World.Calls);

        rig.World.Calls.Clear();
        rig.World.SimOf(p).DuoiKhi = true;
        rig.Env.Skills.RemoveMonkeyTail(p);
        Assert.False(rig.World.SimOf(p).DuoiKhi);
        Assert.Equal(["StopDuoiKhi:1", .. CommonDownCalls], rig.World.Calls);
    }

    [Fact]
    public void Going_down_clamps_hp_only_when_it_is_above_the_maximum()
    {
        var rig = new EffectRig();
        Player above = AddSkilled(rig, 1, configure: s => s.NPoint = s.NPoint with { Hp = 1001, HpMax = 1000 });
        rig.Env.Skills.RemoveCumber(above);
        Assert.Equal("SetHp:1:1000", rig.World.Calls[0]);

        rig.World.Calls.Clear();
        Player equal = AddSkilled(rig, 2, configure: s => s.NPoint = s.NPoint with { Hp = 1000, HpMax = 1000 });
        rig.Env.Skills.RemoveCumber(equal);
        Assert.DoesNotContain(rig.World.Calls, c => c.StartsWith("SetHp", StringComparison.Ordinal));
    }

    [Fact]
    public void Going_down_without_a_selected_skill_still_runs_every_other_step()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skills.RemoveCumber(p);
        Assert.Equal(CommonDownCalls, rig.World.Calls);
        Assert.Single(rig.Errors); // the end-charge packet needs the selected skill
        Assert.Single(rig.World.Packets); // only the 95 packet
    }

    [Fact]
    public void Cumber_set_stores_three_minutes_and_refreshes_item_times()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skills.SetCumber(p);
        Assert.True(p.EffectSkill.IsCumber);
        Assert.Equal((180_000, rig.Now), (p.EffectSkill.TimeCumber, p.EffectSkill.LastTimeCumber));
        rig.Env.Skills.SetCumber2(p);
        Assert.True(p.EffectSkill.IsCumber2);
        Assert.Equal((180_000, rig.Now), (p.EffectSkill.TimeCumber2, p.EffectSkill.LastTimeCumber2));
        rig.Env.Skills.SetPain(p);
        Assert.True(p.EffectSkill.IsPain);
        Assert.Equal((180_000, rig.Now), (p.EffectSkill.TimePain, p.EffectSkill.LastTimeUpPain));
        rig.Env.Skills.SetKefla(p);
        Assert.True(p.EffectSkill.IsKefla);
        Assert.Equal((300_000, rig.Now), (p.EffectSkill.TimeKefla, p.EffectSkill.LastTimeKefla));
        Assert.Equal(["AllItemTime:1", "AllItemTime:1", "AllItemTime:1", "AllItemTime:1"], rig.World.Calls);
    }

    [Fact]
    public void Duoi_khi_set_stamps_the_item_time_with_now()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        rig.Env.Skills.SetMonkeyTail(p);
        Assert.True(rig.World.SimOf(p).DuoiKhi);
        Assert.Equal(rig.Now, rig.World.SimOf(p).MonkeyTailStartTime);
    }

    [Fact]
    public async Task Async_transformations_wait_before_applying_and_stamp_the_time_after_the_wait()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        long before = rig.Now;

        Task cumber = rig.Env.Skills.SetCumberAsync(p);
        Task cumber2 = rig.Env.Skills.SetCumber2Async(p);
        Task pain = rig.Env.Skills.SetPainAsync(p);
        Task kefla = rig.Env.Skills.SetKeflaAsync(p);
        Task duoiKhi = rig.Env.Skills.SetMonkeyTailAsync(p);

        Assert.All(new[] { cumber, cumber2, pain, kefla, duoiKhi }, t => Assert.False(t.IsCompleted));
        Assert.Equal(
            [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)],
            rig.World.PendingDelays);
        Assert.False(p.EffectSkill.IsCumber);
        Assert.False(rig.World.SimOf(p).DuoiKhi);

        rig.Advance(2000);
        rig.World.CompleteAllDelays();
        await Task.WhenAll(cumber, cumber2, pain, kefla, duoiKhi);

        Assert.True(p.EffectSkill.IsCumber && p.EffectSkill.IsCumber2 && p.EffectSkill.IsPain && p.EffectSkill.IsKefla);
        Assert.Equal(before + 2000, p.EffectSkill.LastTimeCumber);
        Assert.Equal(before + 2000, rig.World.SimOf(p).MonkeyTailStartTime);
    }

    [Fact]
    public async Task A_cancelled_wait_changes_nothing()
    {
        var rig = new EffectRig();
        Player p = rig.Add(1);
        using var cts = new CancellationTokenSource();
        Task task = rig.Env.Skills.SetCumberAsync(p, cts.Token);
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        Assert.False(p.EffectSkill.IsCumber);
        Assert.Empty(rig.World.Calls);
    }

    [Fact]
    public void Transformation_set_saves_the_form_and_sends_speed_and_effect()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1);
        p.TransformationState = 7;
        rig.Env.Skills.SetTransformation(p);

        Assert.Equal(0, p.TransformationState);
        Assert.Equal([(1L, 0)], rig.World.SavedBienHinh);
        Assert.True(p.EffectSkill.IsTransformation);
        Assert.Equal(300_000, p.EffectSkill.TimeTransformation);
        Assert.Equal(5, p.EffectSkill.TransformationLevel);
        Assert.Equal(0, p.EffectSkill.LastTimeTransformation); // Java never stamps it here
        Assert.Equal(["Speed:1:7", "Caitrang:1", "Point:1"], rig.World.Calls);
        Assert.Equal(Of(B(6), 1, S(95)), Assert.Single(rig.World.Packets).Data);
    }

    [Fact]
    public void Transformation_down_resets_the_level_and_clamps_hp()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1, configure: s => s.NPoint = s.NPoint with { Hp = 5000, HpMax = 1000 });
        p.TransformationState = 3;
        p.EffectSkill.IsTransformation = true;
        p.EffectSkill.TransformationLevel = 4;
        rig.Env.Skills.RemoveTransformation(p);

        Assert.Equal(0, p.TransformationState);
        Assert.Equal([(1L, 0)], rig.World.SavedBienHinh);
        Assert.False(p.EffectSkill.IsTransformation);
        Assert.Equal(0, p.EffectSkill.TransformationLevel);
        Assert.Equal(["SetHp:1:1000", "NotTranformation:1", "Caitrang:1", "Point:1", "InfoHpMp:1", "InfoNv:1"], rig.World.Calls);
        Assert.Equal(2, rig.World.Packets.Count);
    }

    [Fact]
    public void Evolution_set_and_down()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1, configure: s => s.NPoint = s.NPoint with { Hp = 5000, HpMax = 1000 });
        p.TransformationState = 3;
        rig.Env.Skills.SetEvolution(p);
        Assert.True(p.EffectSkill.IsEvolution);
        Assert.Equal((300_000, rig.Now), (p.EffectSkill.TimeEvolution, p.EffectSkill.LastTimeEvolution));
        Assert.Equal(["Caitrang:1", "Point:1"], rig.World.Calls);
        Assert.Equal(3, p.TransformationState); // setting does not touch it

        rig.World.Calls.Clear();
        rig.Env.Skills.RemoveEvolution(p);
        Assert.False(p.EffectSkill.IsEvolution);
        Assert.Equal(0, p.TransformationState);
        Assert.Equal(["SetHp:1:1000", "NotVolution:1", "Caitrang:1", "Point:1", "InfoHpMp:1", "InfoNv:1"], rig.World.Calls);
    }

    [Fact]
    public void Burst_power_set_and_remove()
    {
        var rig = new EffectRig();
        Player p = AddSkilled(rig, 1);
        rig.Env.Skills.StartPowerBurst(p);
        Assert.True(p.EffectSkill.IsPowerBurst);
        Assert.Equal((50_000, rig.Now), (p.EffectSkill.TimePowerBurst, p.EffectSkill.LastTimePowerBurst));
        rig.Env.Skills.RemovePowerBurst(p);
        Assert.False(p.EffectSkill.IsPowerBurst);
        Assert.Equal(Of(B(0), B(0), B(58), 1, 1), Assert.Single(rig.World.Packets).Data);
        Assert.Equal(["Point:1", "InfoNv:1", "InfoHpMp:1"], rig.World.Calls);
    }

    [Fact]
    public void Constants_have_the_Java_values()
    {
        Assert.Equal(1, EffectSkillService.TURN_ON_EFFECT);
        Assert.Equal(0, EffectSkillService.TURN_OFF_EFFECT);
        Assert.Equal(2, EffectSkillService.TURN_OFF_ALL_EFFECT);
        Assert.Equal(32, EffectSkillService.HOLD_EFFECT);
        Assert.Equal(33, EffectSkillService.SHIELD_EFFECT);
        Assert.Equal(215, EffectSkillService.DANH_HIEU);
        Assert.Equal(39, EffectSkillService.HUYT_SAO_EFFECT);
        Assert.Equal(40, EffectSkillService.BLIND_EFFECT);
        Assert.Equal(41, EffectSkillService.SLEEP_EFFECT);
        Assert.Equal(42, EffectSkillService.STONE_EFFECT);
        Assert.Equal(42, EffectSkillService.ICE_EFFECT);
        Assert.Equal(4, EffectSkillService.DOKINH_EFFECT);
        Assert.Equal(15, EffectSkillService.DOKINHCAOCAP_EFFECT);
        Assert.Equal(58, EffectSkillService.SUC_MANH_BOC_PHA_EFFECT);
    }

    [Fact]
    public void Environment_creation_rejects_missing_parts()
    {
        var rig = new EffectRig();
        Assert.Throws<ArgumentNullException>(() => EffectEnvironment.Create(null!, new Nro.Core.Util.RandomUtil(rig.Random), new Nro.Core.Logging.GameLogger(rig.Log), rig.Settings, rig.World, rig.World, rig.World, rig.World, rig.World, rig.World));
        Assert.Throws<ArgumentNullException>(() => EffectEnvironment.Create(rig.Clock, new Nro.Core.Util.RandomUtil(rig.Random), new Nro.Core.Logging.GameLogger(rig.Log), rig.Settings, rig.World, rig.World, null!, rig.World, rig.World, rig.World));
    }
}
