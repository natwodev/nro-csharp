using System.Reflection;
using Nro.Core.Consts;
using Nro.Core.IO;
using Nro.Game;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.World.Mobs;

public class MobTests
{
    private static (FakeGame G, Zone Z, Mob M, Player P) Arena(long seed = 1, int tempId = 25, long hp = 1000)
    {
        FakeGame g = new(seed);
        Zone z = MapTestKit.BuildZone(g);
        Mob mob = MapTestKit.NewMob(z, 0, tempId, hp);
        Player p = MapTestKit.NewPlayerIn(g, z, 1);
        return (g, z, mob, p);
    }

    private static void KillSilently(Mob mob)
    {
        mob.Point.Hp = 0;
        mob.Status = 0;
        mob.SetDie();
    }

    // ---- taking damage ---------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Injured_AFullHpMobSurvivesTheFirstHitWithOneHp_AndTheAttackerGetsExp()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena();
        mob.Injured(p, 5000, false);
        Assert.Equal(1, mob.Point.Hp);
        Assert.Equal(5, mob.Status);
        Assert.Single(g.PacketsOf(-9));
        Assert.Single(g.Calls, c => c.StartsWith("smtn:1:2:", StringComparison.Ordinal));
        Assert.Equal(2, g.Calls.Count(c => c.StartsWith("train:1:", StringComparison.Ordinal)));
        Assert.Contains(p, mob.TemporaryEnemies);
        Assert.DoesNotContain(g.Calls, c => c.StartsWith("killed", StringComparison.Ordinal));
    }

    [Fact]
    public void Injured_OnAPrivateMap_TheFirstHitMayKill()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena();
        g.SetMapSet("RiengTu", 1);
        mob.Injured(p, 5000, false);
        Assert.True(mob.IsDie());
        Assert.Equal(0, mob.Status);
        Assert.Equal(g.Runtime.Now, mob.LastTimeDie);
        Assert.Contains("killed:1:0", g.Calls);
        Assert.Empty(mob.TemporaryEnemies);
    }

    [Fact]
    public void Injured_DieWhenHpFull_KillsAtOnce_AndANullAttackerStillKills()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena();
        mob.Injured(p, 2000, true);
        Assert.True(mob.IsDie());
        (FakeGame g2, _, Mob mob2, _) = Arena();
        mob2.Injured(null, 2000, true);
        Assert.True(mob2.IsDie());
        Assert.DoesNotContain(g2.Calls, c => c.StartsWith("killed", StringComparison.Ordinal));
        Assert.Single(g.PacketsOf(-12));
    }

    [Fact]
    public void Injured_ADeadMobIgnoresFurtherHits()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena();
        KillSilently(mob);
        mob.Injured(p, 100, false);
        Assert.Empty(g.Packets);
        Assert.Empty(g.Calls);
    }

    [Fact]
    public void Injured_SecondHitFinishesTheMob_SendsOneDeathPacketWithTheDropList_AndFiresTheKillHooks()
    {
        (FakeGame g, Zone z, Mob mob, Player p) = Arena(seed: 9);
        mob.Injured(p, 5000, false);
        g.Packets.Clear();
        mob.Injured(p, 50, false);
        Assert.True(mob.IsDie());
        SentPacket death = Assert.Single(g.PacketsOf(-12));
        Message m = new(-12, death.Data);
        Assert.Equal(0, m.ReadByte());
        Assert.Equal(1, m.ReadInt());
        Assert.False(m.ReadBoolean());
        int dropped = m.ReadByte();
        Assert.Equal(z.Items.Count, dropped);
        for (int i = 0; i < dropped; i++)
        {
            m.ReadShort();
            m.ReadShort();
            m.ReadShort();
            m.ReadShort();
            Assert.Equal(1, m.ReadInt());
        }

        Assert.Contains("killed:1:0", g.Calls);
    }

    [Fact]
    public void Injured_AKillByANonPlayerSendsTheDeathWithoutDrops()
    {
        (FakeGame g, Zone z, Mob mob, Player p) = Arena();
        g.State(p).IsPl = false;
        mob.Injured(p, 5000, true);
        SentPacket death = Assert.Single(g.PacketsOf(-12));
        Assert.Equal(1 + 4 + 1, death.Data.Length);
        Assert.Empty(z.Items);
    }

    [Fact]
    public void Injured_BulonDeathsClearTheAliveFlagsOfTheZone()
    {
        (FakeGame _, Zone z, Mob mob, Player p) = Arena();
        mob.Id = 13;
        mob.Injured(p, 5000, true);
        Assert.False(z.Isbulon1Alive);
        Assert.True(z.Isbulon2Alive);
    }

    [Fact]
    public void Injured_SameSeed_GivesTheSameDropsAndPackets()
    {
        static (List<string> Calls, List<string> Packets) Run(long seed)
        {
            (FakeGame g, Zone z, Mob mob, Player p) = Arena(seed);
            g.SetMapSet("Cold", 1);
            for (int i = 0; i < 40; i++)
            {
                mob.Respawn();
                mob.Injured(p, 5000, true);
            }

            return ([.. g.Calls], [.. g.Packets.Select(x => x.Command + ":" + Convert.ToHexString(x.Data))]);
        }

        (List<string> callsA, List<string> packetsA) = Run(77);
        (List<string> callsB, List<string> packetsB) = Run(77);
        (List<string> callsC, List<string> packetsC) = Run(78);
        Assert.Equal(callsA, callsB);
        Assert.Equal(packetsA, packetsB);
        Assert.NotEqual(packetsA, packetsC);
        Assert.True(callsC.Count > 0);
    }

    [Theory]
    [InlineData(ConstMob.TRAINING_DUMMY, 500L, 900L)]
    [InlineData(ConstMob.HAUNTED_SCARECROW, 500L, 900L)]
    [InlineData(ConstMob.NINE_TUSKED_ELEPHANT, 500L, 900L)]
    [InlineData(25, 500L, 500L)]
    public void Injured_SomeMobsTakeAtMostATenthOfTheirMaxHpPerHit(int tempId, long damage, long hpAfter)
    {
        (_, _, Mob mob, Player p) = Arena(tempId: tempId);
        mob.Injured(p, damage, false);
        Assert.Equal(hpAfter, mob.Point.Hp);
    }

    [Fact]
    public void Injured_HungVuongEventMapsCapTheAnimalsByTemplate()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(tempId: ConstMob.NINE_TUSKED_ELEPHANT, hp: 10_000_000);
        g.SetMapSet("HungVuongEvent", 1);
        mob.Injured(p, 5_000_000, false);
        Assert.Equal(10_000_000 - 100_000, mob.Point.Hp);
        (FakeGame g2, _, Mob mob2, Player p2) = Arena(tempId: ConstMob.NINE_MANED_HORSE, hp: 10_000_000);
        g2.SetMapSet("HungVuongEvent", 1);
        mob2.Injured(p2, 5_000_000, false);
        Assert.Equal(10_000_000 - 75_000, mob2.Point.Hp);
        (FakeGame g3, _, Mob mob3, Player p3) = Arena(tempId: ConstMob.NINE_SPURRED_ROOSTER, hp: 10_000_000);
        g3.SetMapSet("HungVuongEvent", 1);
        mob3.Injured(p3, 5_000_000, false);
        Assert.Equal(10_000_000 - 60_000, mob3.Point.Hp);
    }

    [Fact]
    public void Injured_HellAndFiveElementMapsFixTheDamage()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena();
        g.SetMapSet("DiaNguc", 1);
        mob.Point.Hp = 900;
        mob.Injured(p, 3, false);
        Assert.Equal(800, mob.Point.Hp);
        (FakeGame g2, _, Mob mob2, Player p2) = Arena();
        g2.SetMapSet("NguHanhSon", 1);
        mob2.Point.Hp = 900;
        mob2.Injured(p2, 3, false);
        Assert.True(mob2.IsDie());
    }

    [Fact]
    public void Injured_Map179_OnlyDisciplesCanHurt()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g, 179);
        Mob mob = MapTestKit.NewMob(z);
        Player master = MapTestKit.NewPlayerIn(g, z, 1);
        Player disciple = MapTestKit.NewPlayerIn(g, z, 2);
        disciple.IsPet = true;
        mob.Point.Hp = 900;
        mob.Injured(master, 100, false);
        Assert.Equal(900, mob.Point.Hp);
        mob.Injured(disciple, 100, false);
        Assert.Equal(800, mob.Point.Hp);
    }

    [Fact]
    public void Injured_AttackerBonusesApplyToTheMatchingMobKind()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(hp: 100_000);
        g.State(p).ViewTransform = v => v with { RunningMobDamagePercent = 50, FlyingMobDamagePercent = 100 };
        mob.Point.Hp = 50_000;
        Assert.True(mob.IsGroundMob());
        mob.Injured(p, 1000, false);
        Assert.Equal(50_000 - 1500, mob.Point.Hp);
        mob.Type = 4;
        mob.Point.Hp = 50_000;
        mob.Injured(p, 1000, false);
        Assert.Equal(50_000 - 2000, mob.Point.Hp);
    }

    [Fact]
    public void Injured_ExperienceMachine_AddsToTheDamageCounter()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(tempId: ConstMob.POWER_METER, hp: 2_000_000_000);
        mob.Injured(p, 1234, false);
        Assert.Equal(1234, p.PunchingMachineTotalDamage);
        Assert.Equal(g.Runtime.Now, p.LastPunchingMachineDamageTime);
    }

    [Fact]
    public void Injured_KhiGasMapLimitsBasicSkillsWhileTheMachineLives()
    {
        FakeGame g = new();
        g.SetMapSet("DestronGas", 1);
        Zone z = MapTestKit.BuildZone(g);
        Mob machine = MapTestKit.NewMob(z, 5, ConstMob.DESTRUCTION_MACHINE, 100_000);
        Mob mob = MapTestKit.NewMob(z, 6, 25, 100_000);
        Player p = MapTestKit.NewPlayerIn(g, z, 1);
        g.State(p).ViewTransform = v => v with { SelectedSkillId = Skill.KAMEJOKO };
        mob.Point.Hp = 50_000;
        mob.Injured(p, 4000, false);
        Assert.Equal(49_999, mob.Point.Hp);
        g.State(p).ViewTransform = v => v with { SelectedSkillId = 99 };
        mob.Injured(p, 4000, false);
        Assert.Equal(45_999, mob.Point.Hp);
        KillSilently(machine);
        g.State(p).ViewTransform = v => v with { SelectedSkillId = Skill.KAMEJOKO };
        mob.Injured(p, 4000, false);
        Assert.Equal(41_999, mob.Point.Hp);
    }

    [Fact]
    public void Injured_SuperMobTakesTenPercentOfItsBoostedHpAndStrikesBack()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena();
        mob.LvMob = 1;
        mob.Injured(p, 50, false);
        Assert.True(mob.IsDie());
        Assert.Contains("injured:1:100", g.Calls);
    }

    [Fact]
    public void Injured_ReflectedDamage_HurtsTheMobAndSendsEffect36()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(hp: 100_000);
        g.State(p).ViewTransform = v => v with { ReflectDamagePercent = 50 };
        mob.AddTemporaryEnemies(p);
        mob.Attack();
        long damage = long.Parse(g.Calls.Single(c => c.StartsWith("injured:1:", StringComparison.Ordinal))[10..]);
        Assert.InRange(damage, 3000, 7000);
        Assert.True(mob.Point.Hp < 100_000);
        SentPacket reflect = g.PacketsOf(-9).Last();
        Assert.Equal(36, reflect.Data[^1]);
    }

    // ---- attack ----------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Attack_HitsAPlayerThatAttackedItWithinRange_ThenWaitsForTheCooldown()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(hp: 1000);
        g.State(p).X = 350;
        mob.AddTemporaryEnemies(p);
        mob.Attack();
        long damage = long.Parse(g.Calls.Single(c => c.StartsWith("injured:1:", StringComparison.Ordinal))[10..]);
        Assert.InRange(damage, 30, 70);
        Assert.Single(g.PacketsOf(-11));
        Assert.Single(g.PacketsOf(-10));

        g.Calls.Clear();
        mob.Attack();
        Assert.Empty(g.Calls);
        g.Time.Advance(TimeSpan.FromMilliseconds(1000));
        mob.Attack();
        Assert.Empty(g.Calls);
        g.Time.Advance(TimeSpan.FromMilliseconds(1));
        mob.Attack();
        Assert.Single(g.Calls);
    }

    [Fact]
    public void Attack_NoAttackWhenTheEnemyIsTooFarOrDeadOrInvisibleOrHasASatelliteGuard()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena();
        mob.AddTemporaryEnemies(p);
        g.State(p).X = 100 + 301;
        mob.Attack();
        Assert.Empty(g.Calls);
        g.State(p).X = 100;
        g.State(p).IsDie = true;
        mob.Attack();
        Assert.Empty(g.Calls);
        g.State(p).IsDie = false;
        g.State(p).ViewTransform = v => v with { IsInvisible = true };
        mob.Attack();
        Assert.Empty(g.Calls);
        g.State(p).ViewTransform = v => v with { SatelliteDefend = true };
        mob.Attack();
        Assert.Empty(g.Calls);
    }

    [Theory]
    [InlineData(25, 100, true)]
    [InlineData(25, 101, false)]
    [InlineData(5, 0, false)]
    [InlineData(12, 0, false)]
    public void Attack_StrangersAreAttackedOnlyWithinHundredPixelsAndByStrongEnoughTemplates(int tempId, int dx, bool attacked)
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(tempId: tempId);
        g.State(p).X = 100 + dx;
        mob.Attack();
        Assert.Equal(attacked, g.Calls.Any(c => c.StartsWith("injured", StringComparison.Ordinal)));
    }

    [Fact]
    public void Attack_PickyTemplatesNeverAttack_AndFlyingMidLevelMobsDo()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(tempId: ConstMob.TRAINING_DUMMY);
        mob.AddTemporaryEnemies(p);
        mob.Attack();
        Assert.Empty(g.Calls);
        (FakeGame g2, _, Mob flyer, Player p2) = Arena(tempId: 12);
        flyer.Type = 4;
        flyer.Attack();
        Assert.Contains(g2.Calls, c => c.StartsWith("injured", StringComparison.Ordinal));
        Assert.NotNull(p2);
    }

    [Fact]
    public void Attack_AStunnedMobDoesNotAttack_UntilTheStunEnds()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena();
        mob.AddTemporaryEnemies(p);
        mob.EffectSkill.StartStun(g.Runtime.Now, 3000);
        Assert.True(mob.EffectSkill.IsHaveEffectSkill());
        mob.Update();
        Assert.Empty(g.Calls);
        g.Time.Advance(TimeSpan.FromMilliseconds(3001));
        mob.Update();
        Assert.False(mob.EffectSkill.IsStun);
        Assert.Contains(g.Calls, c => c.StartsWith("injured", StringComparison.Ordinal));
        Assert.Single(g.PacketsOf(-124));
    }

    [Fact]
    public void Attack_ReductionsOfTheTargetApply()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(hp: 10_000);
        mob.AddTemporaryEnemies(p);
        long Hit()
        {
            g.Calls.Clear();
            g.Time.Advance(TimeSpan.FromSeconds(10));
            mob.Attack();
            return long.Parse(g.Calls.Single(c => c.StartsWith("injured:1:", StringComparison.Ordinal))[10..]);
        }

        long plain = Hit();
        Assert.InRange(plain, 390, 610);
        g.State(p).ViewTransform = v => v with { TdDaTrau = g.Runtime.Now + 10_000_000 };
        long halved = Hit();
        Assert.InRange(halved, 195, 305);
        g.State(p).ViewTransform = v => v with { IsCommenson = true };
        long tenth = Hit();
        Assert.InRange(tenth, 39, 61);
        g.State(p).ViewTransform = v => v with { ClanBuaDaTrau = g.Runtime.Now + 10_000_000, ClanLevel = 5 };
        Assert.Equal(0, Hit());

        // level 10 or more: the bonus reaches the 200 percent cap and Java subtracts twice the damage, so the hit is NEGATIVE (kept as a Java quirk)
        g.State(p).ViewTransform = v => v with { ClanBuaDaTrau = g.Runtime.Now + 10_000_000, ClanLevel = 10 };
        Assert.True(Hit() < 0);
    }

    // ---- respawn ---------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Update_ADeadMobRespawnsStrictlyAfterFiveSeconds()
    {
        (FakeGame g, _, Mob mob, _) = Arena(tempId: 10, hp: 700);
        KillSilently(mob);
        g.Time.Advance(TimeSpan.FromMilliseconds(5000));
        mob.Update();
        Assert.True(mob.IsDie());
        Assert.Empty(g.PacketsOf(-13));
        g.Time.Advance(TimeSpan.FromMilliseconds(1));
        mob.Update();
        Assert.Equal(700, mob.Point.Hp);
        Assert.Equal(5, mob.Status);
        SentPacket respawn = Assert.Single(g.PacketsOf(-13));
        Assert.Equal([0, 10, 0], respawn.Data[..3]);
        Assert.Single(g.PacketsOf(87));
    }

    [Fact]
    public void Update_NoRespawnDuringMaintenanceOrForBigBossesOrOnDungeonMaps()
    {
        (FakeGame g, _, Mob mob, _) = Arena(tempId: 10);
        g.MaintenanceRunning = true;
        KillSilently(mob);
        g.Time.Advance(TimeSpan.FromSeconds(60));
        mob.Update();
        Assert.True(mob.IsDie());
        g.MaintenanceRunning = false;
        mob.Update();
        Assert.False(mob.IsDie());

        FakeGame g2 = new();
        Zone dungeon = MapTestKit.BuildZone(g2, 80, ConstMap.MAP_TREASURE_UNDER_SEA);
        Mob inDungeon = MapTestKit.NewMob(dungeon, 0, 10);
        KillSilently(inDungeon);
        g2.Time.Advance(TimeSpan.FromSeconds(60));
        inDungeon.Update();
        Assert.True(inDungeon.IsDie());
    }

    [Fact]
    public void Update_AnAliveMobNeverHeals_BecauseTheJavaHealBranchSitsInsideTheDeadMobBlock()
    {
        (FakeGame g, _, Mob mob, _) = Arena(tempId: 10, hp: 1000);
        mob.Point.Hp = 500;
        g.Time.Advance(TimeSpan.FromMilliseconds(120_000));
        mob.Update();
        Assert.Equal(500, mob.Point.Hp);
        Assert.Empty(g.Packets);
    }

    [Fact]
    public void Hoi_hp_HealsAndBroadcasts_AndANegativeValueShowsItsAbsoluteValue()
    {
        (FakeGame g, _, Mob mob, _) = Arena(tempId: 10, hp: 1000);
        mob.Point.Hp = 500;
        mob.HealHp(100);
        Assert.Equal(600, mob.Point.Hp);
        SentPacket up = Assert.Single(g.PacketsOf(-9));
        Assert.Equal([0, 0, 0, 2, 88, 0, 0, 0, 1, 0, 255], up.Data);
        g.Packets.Clear();
        mob.HealHp(-250);
        Assert.Equal(350, mob.Point.Hp);
        Assert.Equal([0, 0, 0, 1, 94, 0, 0, 0, 250, 0, 255], Assert.Single(g.PacketsOf(-9)).Data);
    }

    [Fact]
    public void Update_DoanhTraiBulonComesBackAfterTenSecondsWhileTheBossLives_OtherMobsDoNot()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g, 53, ConstMap.MAP_RED_RIBBON_HQ);
        Mob bulon = MapTestKit.NewMob(z, 13, ConstMob.BULON, 800);
        Mob other = MapTestKit.NewMob(z, 14, 10, 800);
        z.Isbulon1Alive = false;
        KillSilently(bulon);
        KillSilently(other);
        g.Time.Advance(TimeSpan.FromMilliseconds(10_000));
        bulon.Update();
        Assert.True(bulon.IsDie());
        g.Time.Advance(TimeSpan.FromMilliseconds(1));
        bulon.Update();
        other.Update();
        Assert.Equal(800, bulon.Point.Hp);
        Assert.True(z.Isbulon1Alive);
        Assert.True(other.IsDie());
        z.IsTUTAlive = false;
        KillSilently(bulon);
        g.Time.Advance(TimeSpan.FromSeconds(30));
        bulon.Update();
        Assert.True(bulon.IsDie());
    }

    [Fact]
    public void Update_GoldenFriezaHourKillsLivingMobsAndStopsRespawns()
    {
        (FakeGame g, Zone z, Mob mob, _) = Arena(tempId: 10, hp: 500);
        z.IsGoldenFriezaAlive = true;
        g.Time.Set(ManualTimeProvider.Vn(2026, 11, 3, 21, 30));
        mob.Update();
        Assert.True(mob.IsDie());
        Assert.Equal(-1, mob.Point.Hp);
        Assert.Single(g.PacketsOf(-12));
        g.Time.Advance(TimeSpan.FromMinutes(5));
        mob.Update();
        Assert.True(mob.IsDie());
        g.Time.Set(ManualTimeProvider.Vn(2026, 11, 3, 22, 30));
        mob.Update();
        Assert.False(mob.IsDie());
    }

    [Fact]
    public void Update_Machines_KeepTheirStateAndSendTheirEffect()
    {
        (FakeGame g, _, Mob xp, _) = Arena(tempId: ConstMob.POWER_METER, hp: 5);
        xp.Update();
        Assert.Equal(2_000_000_000, xp.Point.Hp);
        (FakeGame g2, _, Mob cannon, _) = Arena(tempId: ConstMob.DESTRUCTION_MACHINE, hp: 5);
        g2.Time.Advance(TimeSpan.FromMilliseconds(1001));
        cannon.Update();
        SentPacket effect = Assert.Single(g2.PacketsOf(-9));
        Assert.Equal(55, effect.Data[^1]);
        cannon.Update();
        Assert.Single(g2.PacketsOf(-9));
        Assert.NotNull(g);
    }

    [Fact]
    public void HoiSinhMobTayKarin_ResetsHpAndMakesExpWorthOne()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(tempId: 10, hp: 900);
        KillSilently(mob);
        mob.RespawnWestKarinMob();
        Assert.Equal(900, mob.Point.Hp);
        Assert.Single(g.PacketsOf(-13));
        Assert.Equal(1, mob.GetPotentialForPlayer(p, 100));
    }

    [Fact]
    public void ComputeLvMob_OnlyOneSuperMobPerZone_AndBigBossesNever()
    {
        (_, Zone z, Mob first, _) = Arena(seed: 4, tempId: 30, hp: 1000);
        Mob second = MapTestKit.NewMob(z, 1, 30, 1000);
        int rolled = 0;
        for (int i = 0; i < 200 && rolled == 0; i++)
        {
            rolled = first.ComputeLvMob();
        }

        Assert.Equal(1, rolled);
        Assert.Equal(10_000, first.Point.Hp);
        Assert.Equal(0, second.ComputeLvMob());
        first.LvMob = 0;
        Mob boss = MapTestKit.NewMob(z, 2, ConstMob.PIANO, 1000);
        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(0, boss.ComputeLvMob());
            Assert.Equal(0, boss.LvMob);
        }
    }

    [Fact]
    public void StartDie_KillsAtOnceAndAnnouncesIt()
    {
        (FakeGame g, _, Mob mob, _) = Arena();
        mob.StartDie();
        Assert.Equal(-1, mob.Point.Hp);
        Assert.Equal(0, mob.Status);
        Assert.Equal(g.Runtime.Now, mob.LastTimeDie);
        Assert.Equal([0], Assert.Single(g.PacketsOf(-12)).Data);
    }

    [Fact]
    public void SmallPackets_UseTheDocumentedCommands()
    {
        (FakeGame g, _, Mob mob, _) = Arena();
        mob.Id = 7;
        mob.SendSuperMob(3);
        mob.SendDisable(true);
        mob.SendDoneMove(false);
        mob.SendFire(true);
        mob.SendIce(true);
        mob.SendWind(false);
        mob.SendMobMaxHp(77);
        mob.SendEffect(55);
        Assert.Equal([-75, 81, 82, 85, 86, 87, 87, -9], g.Packets.Select(x => (int)x.Command));
        Assert.Equal([7, 3], g.Packets[0].Data);
        Assert.Equal([7, 1], g.Packets[1].Data);
        Assert.Equal([7, 0, 0, 0, 77], g.Packets[6].Data);
    }

    [Fact]
    public void SendMobDieAfterMobMeAttackedPet_ListsTheDropsInThePacketAndStampsTheDeathTime()
    {
        (FakeGame g, Zone z, Mob mob, Player p) = Arena(seed: 3);
        mob.SendMobDieAfterPlayerOwnedMobAttackedPet(p, 10);
        Assert.Equal(0, mob.Status);
        Assert.Equal(g.Runtime.Now, mob.LastTimeDie);
        SentPacket packet = Assert.Single(g.PacketsOf(-12));
        Assert.Equal(1 + 4 + 1 + 1 + (z.Items.Count * 12), packet.Data.Length);
    }

    // ---- the copy constructor and the point ----------------------------------------------------------------------------------------------------------------

    [Fact]
    public void CopyConstructor_CopiesStatsButNotZoneOrGoldPercent()
    {
        (_, _, Mob mob, _) = Arena(hp: 700);
        mob.Percent_gold = 9;
        mob.Level = 12;
        mob.Type = 4;
        Mob copy = new(mob);
        Assert.Equal((mob.Id, mob.TempId, mob.Level, 700L, 700L, mob.DamagePercent, mob.PotentialPercent, 4), (copy.Id, copy.TempId, copy.Level, copy.Point.GetHpFull(), copy.Point.Hp, copy.DamagePercent, copy.PotentialPercent, copy.Type));
        Assert.Equal((100, 144), (copy.Location.X, copy.Location.Y));
        Assert.Null(copy.Zone);
        Assert.Equal(0, copy.Percent_gold);
        Assert.NotSame(mob.Location, copy.Location);
        Assert.NotSame(mob.Point, copy.Point);
    }

    [Fact]
    public void MobPoint_Sethp_ChecksTheOldHpNotTheArgument()
    {
        (_, _, Mob mob, _) = Arena();
        mob.Point.Hp = 10;
        mob.Point.Sethp(-5);
        Assert.Equal(-5, mob.Point.Hp);
        mob.Point.Sethp(40);
        Assert.Equal(0, mob.Point.Hp);
        mob.Point.Sethp(40);
        Assert.Equal(40, mob.Point.Hp);
    }

    [Fact]
    public void MobPoint_FixedDame_VariesByOnePercent()
    {
        (_, _, Mob mob, _) = Arena(seed: 21);
        mob.Point.Damage = 10_000;
        for (int i = 0; i < 200; i++)
        {
            Assert.InRange(mob.Point.GetDamageAttack(), 9_900, 10_100);
        }
    }

    [Fact]
    public void MobType_Predicates()
    {
        (_, _, Mob mob, _) = Arena();
        mob.TempId = ConstMob.RED_FURRED_MONKEY;
        Assert.True(mob.IsMonkeyMob());
        mob.TempId = ConstMob.SAIYAN_PIG_MOTHER;
        Assert.True(mob.IsPigMob());
        mob.TempId = ConstMob.GODZILLA;
        Assert.True(mob.IsBigBoss());
        Assert.True(mob.IsBigBossHungVuongEvent());
        mob.TempId = ConstMob.PIANO;
        Assert.True(mob.IsBigBoss());
        Assert.False(mob.IsBigBossHungVuongEvent());
        mob.LvMob = 2;
        Assert.True(mob.IsSuperMob());
        mob.Type = 4;
        Assert.True(mob.IsFlyingMob());
        Assert.False(mob.IsGroundMob());
    }

    // ---- experience ------------------------------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(ConstMob.POWER_METER)]
    [InlineData(ConstMob.HIRUDEGARN)]
    public void GetPotentialForPlayer_SpecialMobsAlwaysGiveOne(int tempId)
    {
        (_, _, Mob mob, Player p) = Arena(tempId: tempId, hp: 1000);
        Assert.Equal(1, mob.GetPotentialForPlayer(p, 1000));
    }

    // ---- drops -----------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void GetItemMobReward_ABossGetsNothing_ButTheBadgeTaskCountsTheKill()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena();
        p.IsBoss = true;
        Assert.Empty(mob.GetItemMobReward(p, 100, 144));
        Assert.Equal(["badge:9:1"], g.Calls);
    }

    [Fact]
    public void GetItemMobReward_ChristmasWithMaximumLuck_DropsTheEventItemsWithTheirOptions()
    {
        (FakeGame g, Zone z, Mob mob, Player p) = Arena(seed: 8);
        g.Christmas = true;
        g.State(p).Loot = new MobLootPlayerView { LuckPercent = 100_000 };
        List<ItemMap> reward = mob.GetItemMobReward(p, 100, 144);
        ItemMap sweet = reward.First(i => i.ItemTemplate!.Id == 533);
        Assert.Equal(30, Assert.Single(sweet.Options).Id);
        ItemMap sock = reward.First(i => i.ItemTemplate!.Id == 649);
        Assert.Equal([30, 93], sock.Options.Select(o => o.Id));
        Assert.Contains(reward, i => i.ItemTemplate!.Id is >= 925 and <= 927);
        Assert.Contains(reward, i => i.ItemTemplate!.Id is >= 928 and <= 931);
        Assert.All(reward, i => Assert.Equal(1L, i.PlayerId));
        Assert.True(z.Items.Count >= reward.Count);
    }

    [Fact]
    public void GetItemMobReward_EventItemsOnlyDropOnMapsThatAllowDrops_AndNeverWithoutTheFlag()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(seed: 8);
        g.State(p).Loot = new MobLootPlayerView { LuckPercent = 100_000 };
        g.Christmas = true;
        g.SetMapSet("NotDrop", 1);
        Assert.DoesNotContain(mob.GetItemMobReward(p, 100, 144), i => i.ItemTemplate!.Id == 533);
        g.Christmas = false;
        g.SetMapSet("NotDrop");
        Assert.DoesNotContain(mob.GetItemMobReward(p, 100, 144), i => i.ItemTemplate!.Id == 533);
    }

    [Fact]
    public void GetItemMobReward_LunarNewYear_PigsDropTheSpecialItem()
    {
        (FakeGame g, _, Mob pig, Player p) = Arena(seed: 2, tempId: ConstMob.FOREST_PIG);
        g.LunarNewYear = true;
        g.State(p).Loot = new MobLootPlayerView { LuckPercent = 100_000 };
        List<ItemMap> reward = pig.GetItemMobReward(p, 100, 144);
        ItemMap item = reward.First(i => i.ItemTemplate!.Id == 748);
        Assert.Equal([86, 93], item.Options.Select(o => o.Id));
        Assert.Equal(30, item.Options[1].Param);
    }

    [Fact]
    public void GetItemMobReward_GoldIsAlwaysBetweenOneAndThreeHundredThousand()
    {
        (FakeGame g, Zone z, Mob mob, Player p) = Arena(seed: 12, hp: 50_000_000);
        g.State(p).Loot = new MobLootPlayerView { GoldPercent = 5000, Power = 123456 };
        g.GoldAttribute = 200;
        for (int i = 0; i < 300; i++)
        {
            mob.GetItemMobReward(p, 100, 144);
        }

        List<ItemMap> gold = z.Items.Where(i => i.ItemTemplate!.Id == 190).ToList();
        Assert.NotEmpty(gold);
        Assert.All(gold, i => Assert.InRange(i.Quantity, 1, 300_000));
        Assert.Contains(gold, i => i.Quantity == 300_000);
    }

    [Fact]
    public void GetItemMobReward_TreasureMapGoldIsDroppedInChunksNextToTheMob()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(seed: 5, hp: 8000);
        g.SetMapSet("TreasureUnderSea", 1);
        g.State(p).Loot = new MobLootPlayerView { HasTreasureMapClan = true };
        mob.GetItemMobReward(p, 100, 144);
        List<string> drops = g.Calls.Where(c => c.StartsWith("drop:190x", StringComparison.Ordinal)).ToList();
        Assert.Equal(8, drops.Count);
        Assert.EndsWith("@25,144", drops[0], StringComparison.Ordinal);
        Assert.EndsWith("@130,144", drops[7], StringComparison.Ordinal);

        FakeGame g2 = new(5);
        g2.SetMapSet("TreasureUnderSea", 1);
        Zone z2 = MapTestKit.BuildZone(g2);
        Mob big = MapTestKit.NewMob(z2, 0, 25, 40_000_000_000);
        Player p2 = MapTestKit.NewPlayerIn(g2, z2, 1);
        g2.State(p2).Loot = new MobLootPlayerView { HasTreasureMapClan = true };
        big.GetItemMobReward(p2, 100, 144);
        List<string> chunks = g2.Calls.Where(c => c.StartsWith("drop:190x", StringComparison.Ordinal)).ToList();
        Assert.InRange(chunks.Count, 72, 88);
        Assert.All(chunks, c => Assert.InRange(int.Parse(c["drop:190x".Length..c.IndexOf('@', StringComparison.Ordinal)]), 1, 100_000_000));

        FakeGame g3 = new(5);
        g3.SetMapSet("TreasureUnderSea", 1);
        Zone z3 = MapTestKit.BuildZone(g3);
        Mob m3 = MapTestKit.NewMob(z3, 0, 25, 8000);
        Player p3 = MapTestKit.NewPlayerIn(g3, z3, 1);
        m3.GetItemMobReward(p3, 100, 144);
        Assert.DoesNotContain(g3.Calls, c => c.StartsWith("drop:190", StringComparison.Ordinal));
    }

    [Fact]
    public void Kill_WithAMagnetCharm_PullsEveryDroppedItemToTheOwner()
    {
        (FakeGame g, Zone z, Mob mob, Player p) = Arena(seed: 14);
        g.State(p).ViewTransform = v => v with { TdThuHut = g.Runtime.Now + 1_000_000 };
        for (int i = 0; i < 30 && z.Items.Count == 0; i++)
        {
            mob.Respawn();
            mob.Injured(p, 5000, true);
        }

        Assert.NotEmpty(z.Items);
        Assert.Equal(z.Items.Count, g.Calls.Count(c => c.StartsWith("pull:1:", StringComparison.Ordinal)));
    }

    private static int DropTaskItems(Zone z, int itemId) => z.Items.Count(i => i.ItemTemplate!.Id == itemId);

    [Theory]
    [InlineData(ConstMob.LIZARD_MOTHER, ConstTask.TASK_8_1, 3)]
    [InlineData(ConstMob.FLYING_DRAGON_MOTHER, ConstTask.TASK_8_1, 2)]
    [InlineData(ConstMob.FLYING_DEMON_MOTHER, ConstTask.TASK_8_1, 1)]
    [InlineData(ConstMob.HERMIT_CRAB, ConstTask.TASK_14_1, 3)]
    [InlineData(ConstMob.SAIYAN_PIG_MOTHER, ConstTask.TASK_14_1, 2)]
    [InlineData(ConstMob.SNAIL, ConstTask.TASK_14_1, 1)]
    [InlineData(ConstMob.LIZARD_MOTHER, ConstTask.TASK_14_1, 3)]
    [InlineData(ConstMob.FLYING_DRAGON_MOTHER, ConstTask.TASK_14_1, 3)]
    [InlineData(ConstMob.FLYING_DEMON_MOTHER, ConstTask.TASK_14_1, 3)]
    [InlineData(ConstMob.SNAIL, ConstTask.TASK_8_1, 0)]
    public void DropItemTask_TheJavaSwitchFallsThroughToTheLaterMotherMobs(int tempId, int task, int rolls)
    {
        (FakeGame g, Zone z, Mob mob, Player p) = Arena(seed: 31, tempId: tempId);
        g.TaskIds[p] = task;
        MethodInfo method = typeof(Mob).GetMethod("DropItemTask", BindingFlags.Instance | BindingFlags.NonPublic)!;
        method.Invoke(mob, [p]);
        int notices = g.Calls.Count(c => c.StartsWith("thongbao:1:", StringComparison.Ordinal));
        int items = DropTaskItems(z, 20) + DropTaskItems(z, 85);
        Assert.Equal(rolls, notices + items);
    }

    [Fact]
    public void DropItemTask_FirstQuestMobsDropTheOnlyQuestItemInTask2_0()
    {
        (FakeGame g, Zone z, Mob mob, Player p) = Arena(tempId: ConstMob.DINOSAUR);
        MethodInfo method = typeof(Mob).GetMethod("DropItemTask", BindingFlags.Instance | BindingFlags.NonPublic)!;
        g.TaskIds[p] = ConstTask.TASK_2_0;
        ItemMap? item = (ItemMap?)method.Invoke(mob, [p]);
        Assert.Equal(73, item!.ItemTemplate!.Id);
        Assert.Contains(item, z.Items);
        g.TaskIds[p] = ConstTask.TASK_3_0;
        Assert.Null(method.Invoke(mob, [p]));
    }

    [Fact]
    public void DropItemTask_ReturnsTheLastCreatedItem_OfTheFallThrough()
    {
        FakeGame g = new(seed: 1);
        Zone z = MapTestKit.BuildZone(g);
        Mob mob = MapTestKit.NewMob(z, 0, ConstMob.LIZARD_MOTHER);
        Player p = MapTestKit.NewPlayerIn(g, z, 1);
        g.TaskIds[p] = ConstTask.TASK_8_1;
        MethodInfo method = typeof(Mob).GetMethod("DropItemTask", BindingFlags.Instance | BindingFlags.NonPublic)!;
        int created = 0;
        for (int i = 0; i < 40; i++)
        {
            ItemMap? item = (ItemMap?)method.Invoke(mob, [p]);
            if (item != null)
            {
                created++;
                Assert.Equal(20, item.ItemTemplate!.Id);
                Assert.Equal((100, 144), (item.X, item.Y));
            }
        }

        Assert.True(created > 0);
    }

    [Fact]
    public void DropItemTask_MessagesAreTheOriginalVietnameseTexts()
    {
        (FakeGame g, _, Mob mob, Player p) = Arena(seed: 100, tempId: ConstMob.SNAIL);
        g.TaskIds[p] = ConstTask.TASK_14_1;
        MethodInfo method = typeof(Mob).GetMethod("DropItemTask", BindingFlags.Instance | BindingFlags.NonPublic)!;
        for (int i = 0; i < 60; i++)
        {
            method.Invoke(mob, [p]);
        }

        Assert.Contains("thongbao:1:Con ốc xên này không giữ truyện tranh, hãy thử tìm con ốc xên khác", g.Calls);
    }
}
