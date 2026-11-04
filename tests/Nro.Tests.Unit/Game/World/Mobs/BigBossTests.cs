using Nro.Core.Consts;
using Nro.Core.IO;
using Nro.Game;

namespace Nro.Tests.Unit.Game.World.Mobs;

public class BigBossTests
{
    private static (FakeGame G, Zone Z, T Boss, Player P) Setup<T>(Func<Mob, T> make, sbyte tempId, int playerX, long seed = 1, int bossX = 100)
        where T : BigBoss
    {
        FakeGame g = new(seed);
        Zone z = MapTestKit.BuildZone(g);
        Mob proto = MapTestKit.NewMob(z, 0, tempId, 100_000, bossX, 144);
        z.Mobs.RemoveAt(0);
        proto.Zone = z;
        T boss = make(proto);
        z.Mobs.Add(boss);
        Player p = MapTestKit.NewPlayerIn(g, z, 1, playerX, 144);
        return (g, z, boss, p);
    }

    private static List<int> Actions(FakeGame g) => g.PacketsOf(102).Select(p => (int)p.Data[0]).ToList();

    [Fact]
    public void Piano_AttacksANearbyPlayer_WithTheDocumentedPacket_ThenWaitsThreeSeconds()
    {
        (FakeGame g, _, Piano boss, Player p) = Setup(m => new Piano(m), ConstMob.PIANO, 110);
        boss.Attack();
        SentPacket packet = Assert.Single(g.PacketsOf(102));
        Assert.InRange((int)packet.Data[0], 11, 14);
        Assert.Equal(0, packet.Data[1]);
        Assert.Equal(12, packet.Data.Length);
        Assert.Equal(1, packet.Data[2]);
        Assert.Equal(1, packet.Data[6]);
        Assert.Equal(1, packet.Data[^1]);
        Assert.Single(g.Calls, c => c.StartsWith("injured:1:", StringComparison.Ordinal));
        Assert.Equal(g.Runtime.Now, boss.LastBigBossAttackTime);

        boss.Attack();
        Assert.Single(g.PacketsOf(102));
        g.Time.Advance(TimeSpan.FromMilliseconds(3000));
        boss.Attack();
        Assert.Single(g.PacketsOf(102));
        g.Time.Advance(TimeSpan.FromMilliseconds(1));
        boss.Attack();
        Assert.Equal(2, g.PacketsOf(102).Count());
        Assert.NotNull(p);
    }

    [Fact]
    public void Attack_NobodyInTheZone_SendsNothingAndDoesNotStartTheCooldown()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Mob proto = MapTestKit.NewMob(z, 0, ConstMob.KONG, 1000);
        Kong boss = new(proto);
        boss.Attack();
        Assert.Empty(g.Packets);
        Assert.Equal(0, boss.LastBigBossAttackTime);
    }

    [Fact]
    public void Attack_FarPlayer_TheBossJumpsNextToItAndSendsAMovePacket()
    {
        (FakeGame g, _, Piano boss, Player p) = Setup(m => new Piano(m), ConstMob.PIANO, 1000);
        g.State(p).Y = 77;
        boss.Attack();
        SentPacket packet = Assert.Single(g.PacketsOf(102));
        Assert.Equal(10, packet.Data[0]);
        Assert.Equal(6, packet.Data.Length);
        Assert.InRange(boss.Location.X, 990, 1010);
        Assert.Equal(77, boss.Location.Y);
        Assert.DoesNotContain(g.Calls, c => c.StartsWith("injured", StringComparison.Ordinal));
        Assert.Equal(1, p.Id);
    }

    [Theory]
    [MemberData(nameof(SingleTargetBosses))]
    public void SingleTargetBosses_OnlyUseActions11Or12(Func<Mob, BigBoss> make, sbyte tempId)
    {
        (FakeGame g, _, BigBoss boss, _) = Setup(make, tempId, 120);
        for (int i = 0; i < 40; i++)
        {
            g.Time.Advance(TimeSpan.FromMilliseconds(3001));
            boss.Attack();
        }

        List<int> actions = Actions(g);
        Assert.Equal(40, actions.Count);
        Assert.All(actions, a => Assert.True(a == 11 || a == 12));
        Assert.Contains(11, actions);
        Assert.Contains(12, actions);
    }

    public static IEnumerable<object[]> SingleTargetBosses() =>
    [
        [(Func<Mob, BigBoss>)(m => new NineSpurredRooster(m)), ConstMob.NINE_SPURRED_ROOSTER],
        [(Func<Mob, BigBoss>)(m => new Godzilla(m)), ConstMob.GODZILLA],
        [(Func<Mob, BigBoss>)(m => new Kong(m)), ConstMob.KONG],
        [(Func<Mob, BigBoss>)(m => new NineManedHorse(m)), ConstMob.NINE_MANED_HORSE],
        [(Func<Mob, BigBoss>)(m => new NineTuskedElephant(m)), ConstMob.NINE_TUSKED_ELEPHANT],
    ];

    [Fact]
    public void BearThief_Action15ReachesPlayersTwoHundredPixelsAway()
    {
        (FakeGame g, _, BearThief boss, Player p) = Setup(m => new BearThief(m), ConstMob.BEAR_THIEF, 280, seed: 6);
        for (int i = 0; i < 80; i++)
        {
            g.Time.Advance(TimeSpan.FromMilliseconds(3001));
            boss.Location.X = 100;
            boss.Attack();
        }

        List<int> actions = Actions(g);
        Assert.All(actions, a => Assert.True(a == 15 || a == 10));
        Assert.Contains(15, actions);
        Assert.Contains(10, actions);
        Assert.NotNull(p);
    }

    [Fact]
    public void PianoMove_TurnsADeadTargetIntoAPlainMove_ButAttackDoesNot()
    {
        (FakeGame g, _, Piano boss, Player p) = Setup(m => new Piano(m), ConstMob.PIANO, 105, seed: 3);
        g.State(p).IsDie = true;
        for (int i = 0; i < 20; i++)
        {
            g.Time.Advance(TimeSpan.FromMilliseconds(3001));
            boss.PianoMove();
        }

        Assert.All(Actions(g), a => Assert.Equal(10, a));
        g.Packets.Clear();
        for (int i = 0; i < 20; i++)
        {
            g.Time.Advance(TimeSpan.FromMilliseconds(3001));
            boss.Attack();
        }

        Assert.All(Actions(g), a => Assert.InRange(a, 11, 14));
    }

    [Fact]
    public void Update_EveryBigBossAlsoRunsPianoMove_AndTheAnimalMovesOnlyDuringTheHungVuongEvent()
    {
        (FakeGame g, _, Kong boss, _) = Setup(m => new Kong(m), ConstMob.KONG, 110);
        boss.Update();
        Assert.Equal(2, g.PacketsOf(102).Count());
        g.Packets.Clear();
        g.HungVuong = true;
        g.Time.Advance(TimeSpan.FromMilliseconds(3001));
        boss.Update();
        Assert.Equal(5, g.PacketsOf(102).Count());
    }

    [Fact]
    public void Update_FixedDamageDependsOnTheHungVuongMap()
    {
        (FakeGame g, _, NineTuskedElephant boss, _) = Setup(m => new NineTuskedElephant(m), ConstMob.NINE_TUSKED_ELEPHANT, 5000);
        boss.Update();
        Assert.Equal(100, boss.Point.Damage);
        g.SetMapSet("HungVuongEvent", 1);
        boss.Update();
        Assert.Equal(100_000, boss.Point.Damage);
        (FakeGame g2, _, Piano piano, _) = Setup(m => new Piano(m), ConstMob.PIANO, 5000);
        piano.Update();
        Assert.Equal(0, piano.Point.Damage);
        Assert.NotNull(g2);
    }

    [Fact]
    public void Update_HungVuongAnimalsRiseAgainAfterTheirDeath_OtherBigBossesStayDead()
    {
        (FakeGame g, _, Kong kong, _) = Setup(m => new Kong(m), ConstMob.KONG, 5000);
        kong.Point.Hp = 0;
        kong.Update();
        Assert.False(kong.IsDie());
        Assert.Single(g.PacketsOf(-13));
        Assert.Equal(g.Runtime.Now, kong.LastBigBossDeathTime);
        kong.Point.Hp = 0;
        g.Time.Advance(TimeSpan.FromMilliseconds(1_800_000));
        kong.Update();
        Assert.True(kong.IsDie());
        g.Time.Advance(TimeSpan.FromMilliseconds(1));
        kong.Update();
        Assert.False(kong.IsDie());

        (FakeGame g2, _, Piano piano, _) = Setup(m => new Piano(m), ConstMob.PIANO, 5000);
        piano.Point.Hp = 0;
        g2.Time.Advance(TimeSpan.FromDays(1));
        piano.Update();
        Assert.True(piano.IsDie());
    }

    [Fact]
    public void Update_GoldenFriezaHourKillsABigBossToo()
    {
        (FakeGame g, Zone z, Piano piano, _) = Setup(m => new Piano(m), ConstMob.PIANO, 5000);
        z.IsGoldenFriezaAlive = true;
        g.Time.Set(Nro.Tests.Unit.Helpers.ManualTimeProvider.Vn(2026, 11, 3, 21, 5));
        piano.Update();
        Assert.True(piano.IsDie());
        Assert.Single(g.PacketsOf(-12));
    }

    // ---- Guard robot, octopus king ---------------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(50, 0)]
    [InlineData(100, 0)]
    [InlineData(101, 1)]
    [InlineData(200, 1)]
    [InlineData(201, 2)]
    public void GuardRobot_ChoosesTheActionByDistance_AndSnapsToThePlayer(int distance, int action)
    {
        (FakeGame g, _, GuardRobot boss, Player p) = Setup(m => new GuardRobot(m), ConstMob.GUARD_ROBOT, 100 + distance);
        boss.Attack();
        string call = g.Calls.Single(c => c.StartsWith("bigboss:", StringComparison.Ordinal));
        Assert.StartsWith($"bigboss:{action}:1:1:", call, StringComparison.Ordinal);
        Assert.Equal(100 + distance, boss.Location.X);
        Assert.Equal(g.Runtime.Now, boss.LastBigBossAttackTime);
        g.Calls.Clear();
        boss.Attack();
        Assert.Empty(g.Calls);
        Assert.NotNull(p);
    }

    [Fact]
    public void GuardRobot_WithoutATargetStillStartsItsCooldown_AndAnnouncesItsDeath()
    {
        (FakeGame g, _, GuardRobot boss, Player p) = Setup(m => new GuardRobot(m), ConstMob.GUARD_ROBOT, 5000);
        g.State(p).IsDie = true;
        boss.Attack();
        Assert.DoesNotContain(g.Calls, c => c.StartsWith("bigboss:", StringComparison.Ordinal));
        Assert.Equal(g.Runtime.Now, boss.LastBigBossAttackTime);
        boss.Point.Hp = 0;
        g.Calls.Clear();
        boss.Update();
        Assert.Contains("bigboss:6:0:-1:-1", g.Calls);
    }

    [Theory]
    [InlineData(480, 3)]
    [InlineData(580, 4)]
    [InlineData(700, 5)]
    public void KingOctopus_ActionsByDistance(int playerX, int action)
    {
        (FakeGame g, _, KingOctopus boss, _) = Setup(m => new KingOctopus(m), ConstMob.KING_OCTOPUS, playerX, bossX: playerX - (action == 3 ? 30 : action == 4 ? 150 : 400));
        boss.Attack();
        SentPacket packet = Assert.Single(g.PacketsOf(102));
        Assert.Equal(action, packet.Data[0]);
        if (action == 5)
        {
            Assert.Equal(3, packet.Data.Length);
            Assert.Equal(playerX, boss.Location.X);
        }
        else
        {
            Assert.Equal(1 + 1 + 4 + 4, packet.Data.Length);
        }
    }

    [Fact]
    public void KingOctopus_FarAwayPlayersGetTheNormalAttackAction_AndPlayersOutsideTheArenaAreIgnored()
    {
        (FakeGame g, _, KingOctopus boss, _) = Setup(m => new KingOctopus(m), ConstMob.KING_OCTOPUS, 800, bossX: 100);
        boss.Attack();
        Assert.Equal(3, Assert.Single(g.PacketsOf(102)).Data[0]);
        (FakeGame g2, _, KingOctopus boss2, _) = Setup(m => new KingOctopus(m), ConstMob.KING_OCTOPUS, 440, bossX: 440);
        boss2.Attack();
        Assert.Empty(g2.Packets);
        Assert.Equal(g2.Runtime.Now, boss2.LastBigBossAttackTime);
        boss2.Point.Hp = 0;
        boss2.Update();
        Assert.Contains("bigboss:7:0:-1:-1", g2.Calls);
    }

    // ---- Hirudegarn ------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Hirudegarn_EveryHitTakesOnePercentOfTheCurrentHpAsTheBasis()
    {
        (FakeGame g, _, Hirudegarn boss, Player p) = Setup(m => new Hirudegarn(m), ConstMob.HIRUDEGARN, 5000, seed: 4);
        boss.Point.Hp = 100_000;
        boss.Injured(p, 99_999_999, false);
        Assert.InRange(boss.Point.Hp, 100_000 - 20_000_000, 100_000 - 10_000_000);
        Assert.True(boss.IsDie());
        Assert.Single(g.PacketsOf(-12));
    }

    [Fact]
    public void Hirudegarn_GoesThroughFourStagesAndComesBackAfterTenMinutes()
    {
        (FakeGame g, Zone z, Hirudegarn boss, _) = Setup(m => new Hirudegarn(m), ConstMob.HIRUDEGARN, 5000, seed: 2);
        boss.Point.SetHpFull(1000);
        boss.Point.Hp = 1000;
        for (int stage = 1; stage <= 2; stage++)
        {
            boss.Point.Hp = 0;
            boss.SetDie();
            g.Time.Advance(TimeSpan.FromMilliseconds(5000));
            boss.Update();
            Assert.Equal(stage - 1, boss.LvMob);
            g.Time.Advance(TimeSpan.FromMilliseconds(1));
            g.Calls.Clear();
            boss.Update();
            Assert.Equal(stage, boss.LvMob);
            Assert.Equal(stage == 1 ? 6 : 5, boss.Action);
            Assert.Equal(1000, boss.Point.Hp);
            Assert.Equal(10, g.Calls.Count(c => c.StartsWith("drop:190x30000@", StringComparison.Ordinal)));
            Assert.Contains($"bigboss2:{boss.Action}:{stage}", g.Calls);
            Assert.NotEmpty(g.PacketsOf(-9));
            g.Packets.Clear();
        }

        // third death: stage 3 = gone, parked outside the map
        boss.Point.Hp = 0;
        boss.SetDie();
        g.Time.Advance(TimeSpan.FromMilliseconds(5001));
        boss.Update();
        Assert.Equal(3, boss.LvMob);
        Assert.Equal(9, boss.Action);
        Assert.Equal((-1000, -1000), (boss.Location.X, boss.Location.Y));
        Assert.True(boss.IsDie());
        boss.Update();
        Assert.Equal(3, boss.LvMob);

        g.Time.Advance(TimeSpan.FromMilliseconds(600_000 - 5_001));
        boss.Update();
        Assert.Equal(3, boss.LvMob);
        g.Time.Advance(TimeSpan.FromMilliseconds(1));
        boss.Update();
        Assert.Equal(0, boss.LvMob);
        Assert.False(boss.IsDie());
        Assert.InRange(boss.Location.X, 100, 900);
        Assert.Equal(360, boss.Location.Y);
        Assert.Equal(boss.Point.GetHpFull(), boss.Point.Hp);
        Assert.NotNull(z);
    }

    [Fact]
    public void Hirudegarn_GoldPilesFallSymmetricallyAroundTheBoss()
    {
        (FakeGame g, _, Hirudegarn boss, _) = Setup(m => new Hirudegarn(m), ConstMob.HIRUDEGARN, 5000, bossX: 300);
        boss.Point.Hp = 0;
        boss.SetDie();
        g.Time.Advance(TimeSpan.FromMilliseconds(5001));
        boss.Update();
        // Java: i=0 left (-15*0 = 0), i=1 right (15*1), i=2 left (-15*1), i=3 right (15*2) ...
        int[] expected = [0, 15, -15, 30, -30, 45, -45, 60, -60, 75];
        List<int> xs = g.Calls.Where(c => c.StartsWith("drop:190x30000@", StringComparison.Ordinal)).Select(c => int.Parse(c.Split('@')[1].Split(',')[0])).ToList();
        Assert.Equal(expected.Select(e => 300 + e), xs);
    }

    [Fact]
    public void Hirudegarn_AttackPicksARandomLivingPlayer_AndSkipsTheTurnWhenItIsDead()
    {
        (FakeGame g, _, Hirudegarn boss, Player p) = Setup(m => new Hirudegarn(m), ConstMob.HIRUDEGARN, 5000, seed: 8);
        g.State(p).IsDie = true;
        boss.Attack();
        Assert.Empty(g.Packets);
        Assert.Equal(0, boss.LastBigBossAttackTime);
        g.State(p).IsDie = false;
        bool sawAttack = false;
        for (int i = 0; i < 60; i++)
        {
            g.Time.Advance(TimeSpan.FromMilliseconds(3001));
            boss.Attack();
            sawAttack |= g.Calls.Any(c => c.StartsWith("injured:1:", StringComparison.Ordinal));
        }

        Assert.True(sawAttack);
        Assert.All(g.PacketsOf(101), pk => Assert.InRange((int)pk.Data[0], 0, 8));
        Assert.Contains(g.Calls, c => c == "bigboss2:8:0");
    }

    [Fact]
    public void Hirudegarn_OnlyTwoActionsInTheLaterStages()
    {
        (FakeGame g, _, Hirudegarn boss, _) = Setup(m => new Hirudegarn(m), ConstMob.HIRUDEGARN, 5000, seed: 8);
        boss.LvMob = 2;
        for (int i = 0; i < 80; i++)
        {
            g.Time.Advance(TimeSpan.FromMilliseconds(3001));
            boss.Attack();
        }

        List<int> actions = g.PacketsOf(101).Select(pk => (int)pk.Data[0]).ToList();
        Assert.All(actions, a => Assert.True(a is 1 or 2 or 0 or 6 or 8));
        Assert.DoesNotContain(3, actions);
        Assert.DoesNotContain(7, actions);
    }

    // ---- Vegeta (unused in Java, ported for completeness) -------------------------------------------------------------------------------------------------

    [Fact]
    public void VegetaM_WalksTenPixelsPerTurnTowardsAFarPlayer()
    {
        (FakeGame g, _, VegetaM boss, Player p) = Setup(m => new VegetaM(m), ConstMob.KONG, 400, seed: 3);
        g.State(p).Y = 144;
        boss.Attack();
        Assert.Equal(1, boss.Status);
        Assert.Equal((110, 144), (boss.Location.X, boss.Location.Y));
        Assert.Single(g.PacketsOf(103));
        Assert.Equal(10, Assert.Single(g.PacketsOf(102)).Data[0]);
        g.Time.Advance(TimeSpan.FromMilliseconds(3001));
        boss.Attack();
        Assert.Equal(120, boss.Location.X);
    }

    [Fact]
    public void BigBoss_PacketsCanBeReadBack()
    {
        (FakeGame g, _, Piano boss, _) = Setup(m => new Piano(m), ConstMob.PIANO, 5000);
        boss.Id = 5;
        boss.Attack();
        Message m = new(102, g.PacketsOf(102).Single().Data);
        Assert.Equal(10, m.ReadByte());
        Assert.Equal(5, m.ReadByte());
        Assert.InRange(m.ReadShort(), 4990, 5010);
        Assert.Equal(144, m.ReadShort());
    }
}
