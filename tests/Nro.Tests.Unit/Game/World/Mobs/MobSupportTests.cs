using Nro.Core.IO;
using Nro.Game;

namespace Nro.Tests.Unit.Game.World.Mobs;

public class MobSupportTests
{
    // ---- EggMob ----------------------------------------------------------------------------------------------------------------------------------------------

    private static (FakeGame G, Zone Z, Player Owner, EggMob Egg) Egg()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Player owner = MapTestKit.NewPlayerIn(g, z, 42);
        g.State(owner).HpMax = 1000;
        EggMob egg = new(owner, g.Runtime);
        owner.EggMob = egg;
        return (g, z, owner, egg);
    }

    [Fact]
    public void EggMob_StatsComeFromTheSkillLevelTables_AndSpawnIsAnnounced()
    {
        (FakeGame g, _, _, EggMob egg) = Egg();
        Assert.Equal((42, 32, 500L, 500L, 200L), (egg.Id, egg.TempId, egg.Point.MaxHp, egg.Point.Hp, egg.Point.Damage));
        SentPacket spawn = Assert.Single(g.PacketsOf(-95));
        Assert.Equal("zone:1/0", spawn.Target);
        Message m = new(-95, spawn.Data);
        Assert.Equal(0, m.ReadByte());
        Assert.Equal(42, m.ReadInt());
        Assert.Equal(32, m.ReadShort());
        Assert.Equal(500, m.ReadInt());
    }

    [Fact]
    public void EggMob_LivesForTheSkillTime_ThenDiesAndLetsGoOfItsOwner()
    {
        (FakeGame g, _, Player owner, EggMob egg) = Egg();
        g.Time.Advance(TimeSpan.FromMilliseconds(160_000));
        egg.Update();
        Assert.Same(egg, owner.EggMob);
        g.Time.Advance(TimeSpan.FromMilliseconds(1));
        egg.Update();
        Assert.Null(owner.EggMob);
        Assert.Equal([6, 0, 0, 0, 42], g.PacketsOf(-95).Last().Data);
        g.Packets.Clear();
        egg.Update();
        Assert.Empty(g.Packets);
    }

    [Fact]
    public void EggMob_Injured_CapsAtFivePercentOfMaxHp_AndDiesAtZero()
    {
        (FakeGame g, _, Player owner, EggMob egg) = Egg();
        Player attacker = g.NewPlayer(7);
        g.Packets.Clear();
        egg.Injured(attacker, 1_000_000, false);
        Assert.Equal(475, egg.Point.Hp);
        SentPacket hit = Assert.Single(g.PacketsOf(-95));
        Assert.Equal(5, hit.Data[0]);
        for (int i = 0; i < 19; i++)
        {
            egg.Injured(attacker, 1_000_000, false);
        }

        Assert.True(egg.IsDie());
        Assert.Null(owner.EggMob);
    }

    [Fact]
    public void EggMob_Attack_HurtsAPlayerOrMob_AndGivesExpOnlyForMobs()
    {
        (FakeGame g, Zone z, Player owner, EggMob egg) = Egg();
        Player victim = g.NewPlayer(8);
        g.State(victim).Hp = 5000;
        g.State(victim).HpMax = 10_000;
        g.Packets.Clear();
        egg.Attack(victim, null, false);
        Assert.Contains("injured:8:200", g.Calls);
        Assert.Equal(2, Assert.Single(g.PacketsOf(-95)).Data[0]);

        g.Calls.Clear();
        g.Packets.Clear();
        egg.Attack(victim, null, true);
        Assert.Contains("injured:8:0", g.Calls);

        g.Calls.Clear();
        g.Packets.Clear();
        Mob mob = MapTestKit.NewMob(z, 3, 25, 10_000);
        egg.Attack(null, mob, false);
        Assert.Equal(9_800, mob.Point.Hp);
        Assert.Single(g.Calls, c => c.StartsWith("smtn:42:2:", StringComparison.Ordinal));
        Assert.Equal(3, Assert.Single(g.PacketsOf(-95)).Data[0]);
        Assert.NotNull(owner);
    }

    [Fact]
    public void EggMob_Attack_SparesAWeakPlayer()
    {
        (FakeGame g, _, _, EggMob egg) = Egg();
        Player weak = g.NewPlayer(8);
        g.State(weak).Hp = 40;
        g.State(weak).HpMax = 10_000;
        g.Packets.Clear();
        egg.Attack(weak, null, false);
        Assert.Empty(g.Calls);
        Assert.Empty(g.Packets);
    }

    [Fact]
    public void EggMob_GoToMap_AnnouncesTheRemovalInTheOldZoneAndMoves()
    {
        (FakeGame g, Zone z, _, EggMob egg) = Egg();
        Zone other = MapTestKit.BuildZone(g, 2);
        g.Packets.Clear();
        egg.GoToMap(other);
        Assert.Same(other, egg.Zone);
        Assert.Equal([7, 0, 0, 0, 42], Assert.Single(g.PacketsOf(-95)).Data);
        egg.GoToMap(null);
        Assert.Same(other, egg.Zone);
        Assert.NotSame(z, egg.Zone);
    }

    // ---- MobDrop ---------------------------------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(662, false)]
    [InlineData(663, true)]
    [InlineData(667, true)]
    [InlineData(668, false)]
    public void IsItemUntradeable_Range(int id, bool expected) => Assert.Equal(expected, MobDrop.IsItemUntradeable(id));

    [Fact]
    public void Tables_HaveTheJavaValues()
    {
        Assert.Equal([663, 664, 665, 666, 667], MobDrop.FoodItemTemplateIds);
        Assert.Equal([220, 221, 222, 223, 224], MobDrop.StoneItemTemplateIds);
        Assert.Equal(3, MobDrop.DestructionSetTemplateIds.Length);
        Assert.All(MobDrop.DestructionSetTemplateIds, row => Assert.Equal(5, row.Length));
        Assert.Equal(656, MobDrop.DestructionSetTemplateIds[1][4]);
    }

    [Fact]
    public void DropRewardItem_DropsUntradeableItemsWithin50PixelsOfX()
    {
        FakeGame g = new(seed: 4);
        Zone z = MapTestKit.BuildZone(g);
        Player p = MapTestKit.NewPlayerIn(g, z, 5);
        for (int i = 0; i < 100; i++)
        {
            MobDrop.DropRewardItem(g.Runtime, p, 77, 3, 500, 144);
        }

        Assert.Equal(100, z.Items.Count);
        Assert.All(z.Items, it =>
        {
            Assert.InRange(it.X, 450, 550);
            Assert.Equal(144, it.Y);
            Assert.Equal(5L, it.PlayerId);
            Assert.Equal(3, it.Quantity);
            Assert.Equal(30, Assert.Single(it.Options).Id);
        });
        Assert.Equal(100, g.Calls.Count(c => c.StartsWith("drop:77x3@", StringComparison.Ordinal)));
        Assert.True(z.Items.Select(i => i.X).Distinct().Count() > 20);
    }

    [Theory]
    [InlineData(650, 47, 1800, 2800, 2)]
    [InlineData(651, 22, 85, 100, 0)]
    [InlineData(657, 0, 8500, 8649, 2)]
    [InlineData(658, 23, 80, 90, 1)]
    [InlineData(656, 14, 17, 19, -1)]
    public void CreateRandomDestructionSetItem_RollsTheStatInItsRange_AndRaisesItForTheMatchingGender(int itemId, int optionId, int low, int high, int goodGender)
    {
        FakeGame g = new(seed: 11);
        for (int gender = 0; gender < 3; gender++)
        {
            for (int i = 0; i < 100; i++)
            {
                DhdItem item = MobDrop.CreateRandomDestructionSetItem(g.Runtime, itemId, gender);
                Assert.Equal(itemId, item.TemplateId);
                ItemMapOption stat = item.Options.Single(o => o.Id == optionId);
                int max = gender == goodGender ? (int)(high * 1.1) : high;
                Assert.InRange(stat.Param, low, max);
                Assert.Equal(1, item.Options[0].Param);
                Assert.Equal(80, item.Options.Single(o => o.Id == 21).Param);
                Assert.Equal(1, item.Options.Single(o => o.Id == 30).Param);
            }
        }
    }

    [Fact]
    public void CreateRandomDestructionSetItem_ForeignItem_OnlyGetsTheCommonOptions()
    {
        FakeGame g = new();
        DhdItem item = MobDrop.CreateRandomDestructionSetItem(g.Runtime, 999, 0);
        Assert.Equal([1, 21, 30], item.Options.Select(o => o.Id));
    }

    [Fact]
    public void DropActivatedDestructionSetReward_DropsAPieceOfThePlayersGender()
    {
        FakeGame g = new(seed: 2);
        Zone z = MapTestKit.BuildZone(g);
        Player p = MapTestKit.NewPlayerIn(g, z, 5);
        p.Gender = 2;
        for (int i = 0; i < 50; i++)
        {
            MobDrop.DropActivatedDestructionSetReward(g.Runtime, p, 1, 300, 144);
        }

        Assert.All(z.Items, it =>
        {
            Assert.Contains((int)it.ItemTemplate!.Id, MobDrop.DestructionSetTemplateIds[2]);
            Assert.Contains(it.Options, o => o.Id == 21);
            Assert.InRange(it.X, 250, 350);
        });
        Assert.Equal(50, z.Items.Count);
    }

    // ---- MobEffectSkill --------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Effects_StartFlagsAndExpireAfterTheirDuration_WithOnePacketEach()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Mob mob = MapTestKit.NewMob(z, 9);
        MobEffectSkill fx = mob.EffectSkill;
        Assert.False(fx.IsHaveEffectSkill());
        long now = g.Runtime.Now;
        fx.SetHypnosis(now, 1000);
        fx.SetBind(now, 2000);
        fx.SetStartBlindDCTT(now, 3000);
        fx.SetChocolate(now, 4000);
        Assert.True(fx.IsHaveEffectSkill());
        Assert.True(fx.IsChocolate);

        g.Time.Advance(TimeSpan.FromMilliseconds(1001));
        fx.Update();
        Assert.False(fx.IsHypnotized);
        Assert.True(fx.IsBound);
        Assert.Equal([0, 1, 41, 9], Assert.Single(g.PacketsOf(-124)).Data);

        g.Packets.Clear();
        g.Time.Advance(TimeSpan.FromMilliseconds(1000));
        fx.Update();
        Assert.False(fx.IsBound);
        Assert.Equal([0, 1, 32, 9], Assert.Single(g.PacketsOf(-124)).Data);

        g.Packets.Clear();
        g.Time.Advance(TimeSpan.FromMilliseconds(1000));
        fx.Update();
        Assert.False(fx.IsBlindDCTT);
        Assert.Equal([0, 1, 40, 9], Assert.Single(g.PacketsOf(-124)).Data);
        g.Time.Advance(TimeSpan.FromMilliseconds(1000));
        fx.Update();
        Assert.False(fx.IsChocolate);
        Assert.Equal([0, 9], Assert.Single(g.PacketsOf(-112)).Data);
        Assert.False(fx.IsHaveEffectSkill());
    }

    [Fact]
    public void Effects_EndWhenTheMobDies()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Mob mob = MapTestKit.NewMob(z, 9);
        mob.EffectSkill.StartStun(g.Runtime.Now, 100_000);
        mob.EffectSkill.SetBind(g.Runtime.Now, 100_000);
        mob.Point.Hp = 0;
        mob.EffectSkill.Update();
        Assert.False(mob.EffectSkill.IsStun);
        Assert.False(mob.EffectSkill.IsBound);
    }

    [Fact]
    public void Effects_JavaQuirk_ADeadMobSendsTheBlindEndPacketOnEveryUpdate()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Mob mob = MapTestKit.NewMob(z, 9);
        mob.Point.Hp = 0;
        mob.EffectSkill.Update();
        mob.EffectSkill.Update();
        mob.EffectSkill.Update();
        Assert.Equal(3, g.PacketsOf(-124).Count());
        Assert.All(g.PacketsOf(-124), pk => Assert.Equal([0, 1, 40, 9], pk.Data));
    }

    [Fact]
    public void Effects_MafubaBottleHurtsEvery500msAndLeavesOneHp()
    {
        FakeGame g = new();
        Zone z = MapTestKit.BuildZone(g);
        Mob mob = MapTestKit.NewMob(z, 9, 25, 100_000);
        Player owner = MapTestKit.NewPlayerIn(g, z, 1);
        g.State(owner).HpMax = 10_000;
        mob.EffectSkill.SetBottle(owner, g.Runtime.Now, 5000);
        Assert.True(mob.EffectSkill.IsBottled);
        Assert.Contains("bodymob:1:11166", g.Calls);
        g.Time.Advance(TimeSpan.FromMilliseconds(501));
        mob.EffectSkill.Update();
        // hpMax 10000 * skill point 10 * 1 / 100 = 1000 per hit (the fake reports typeBinh 0)
        Assert.Equal(99_000, mob.Point.Hp);
        mob.EffectSkill.Update();
        Assert.Equal(99_000, mob.Point.Hp);
        g.Time.Advance(TimeSpan.FromMilliseconds(501));
        mob.EffectSkill.Update();
        Assert.Equal(98_000, mob.Point.Hp);
        g.Time.Advance(TimeSpan.FromMilliseconds(5000));
        mob.EffectSkill.Update();
        Assert.False(mob.EffectSkill.IsBottled);
        Assert.Contains("bodymob:0:-1", g.Calls);
    }
}
