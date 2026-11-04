using Nro.Core.Consts;
using Nro.Core.Templates;
using Nro.Game;

namespace Nro.Tests.Unit.Game.World.Maps;

public class MapTests
{
    [Fact]
    public void Constructor_SetsSizesPlanetName_AndCreatesTheZones()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g, 7, zones: 3, maxPlayer: 12);
        Assert.Equal(20 * 24, map.MapWidth);
        Assert.Equal(10 * 24, map.MapHeight);
        Assert.Equal("planet1", map.PlanetName);
        Assert.Equal(3, map.Zones.Count);
        Assert.All(map.Zones, z => Assert.Equal(12, z.MaxPlayer));
        Assert.Equal([0, 1, 2], map.Zones.Select(z => z.ZoneId));
        Assert.Same(map.Zones[1], map.GetZoneByIndex(1));
        Assert.Null(map.GetZoneByIndex(3));
        Assert.Null(map.GetZoneByIndex(-1));
        Assert.Same(g.Runtime, map.Zones[0].Runtime);
    }

    [Fact]
    public void Constructor_WithAnEmptyTileMap_LeavesTheSizesAsJavaDid()
    {
        FakeGame g = new();
        Map map = new(g.Runtime, 5, "x", 0, 0, 0, 0, 0, [], [2], 1, 5, [], [], 0);
        Assert.Equal(0, map.MapHeight);
        Assert.Equal(0, map.MapWidth);
    }

    [Theory]
    [InlineData(ConstMap.MAP_OFFLINE, 5, 1)]
    [InlineData(ConstMap.MAP_CLAN, 5, 1)]
    [InlineData(ConstMap.MAP_NORMAL, 5, 5)]
    public void Constructor_FixedZoneCountsOfOfflineAndClanMaps(sbyte type, int requested, int expected)
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g, 9, type, requested);
        Assert.Equal(expected, map.Zones.Count);
    }

    [Fact]
    public void Constructor_DungeonTypesTakeTheCountFromTheRulesAndRegisterEveryZone()
    {
        FakeGame g = new();
        g.FixedZoneCounts[ConstMap.MAP_RED_RIBBON_HQ] = 3;
        Map map = MapTestKit.BuildMap(g, 53, ConstMap.MAP_RED_RIBBON_HQ, 1);
        Assert.Equal(3, map.Zones.Count);
        Assert.Equal([$"register:{ConstMap.MAP_RED_RIBBON_HQ}:0", $"register:{ConstMap.MAP_RED_RIBBON_HQ}:1", $"register:{ConstMap.MAP_RED_RIBBON_HQ}:2"], g.Calls);

        g.Calls.Clear();
        MapTestKit.BuildMap(g, 54, ConstMap.MAP_NORMAL, 2);
        Assert.Empty(g.Calls);
    }

    [Fact]
    public void Constructor_PlacesTheStartingItemsOfTheTaskMaps()
    {
        FakeGame g = new();
        Map m21 = MapTestKit.BuildMap(g, 21, zones: 2);
        foreach (Zone z in m21.Zones)
        {
            ItemMap item = Assert.Single(z.Items);
            Assert.Equal((74, 633, 315, -1L), ((int)item.ItemTemplate!.Id, item.X, item.Y, item.PlayerId));
        }

        Map m88 = MapTestKit.BuildMap(g, 88);
        ItemMap star = Assert.Single(m88.Zones[0].Items);
        Assert.Equal(375, star.ItemTemplate!.Id);
        Assert.Empty(MapTestKit.BuildMap(g, 2).Zones[0].Items);
    }

    [Fact]
    public void InitBoss_CreatesTheBossOfTheMapInEveryZone()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g, 114, zones: 2);
        map.InitBoss();
        Assert.Equal(["boss:-52:114/0", "boss:-52:114/1"], g.Calls);
        g.Calls.Clear();
        MapTestKit.BuildMap(g, 3).InitBoss();
        Assert.Empty(g.Calls);
    }

    [Theory]
    [InlineData(114, 115)]
    [InlineData(115, 117)]
    [InlineData(117, 118)]
    [InlineData(118, 119)]
    [InlineData(119, 120)]
    [InlineData(120, -1)]
    [InlineData(5, -1)]
    public void MapIdNextMabu(short from, short expected)
    {
        FakeGame g = new();
        Assert.Equal(expected, MapTestKit.BuildMap(g).MapIdNextMabu(from));
    }

    // ---- physics ---------------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void YPhysicInTop_FindsTheGroundBelow_AndReturnsYWhenAlreadyOnIt()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g);
        Assert.Equal(144, map.YPhysicInTop(50, 0));
        Assert.Equal(144, map.YPhysicInTop(50, 100));
        Assert.Equal(150, map.YPhysicInTop(50, 150));
        Assert.Equal(0, map.YPhysicInTop(50, 200));
    }

    [Fact]
    public void YPhysicInTop_OutsideTheMap_ReturnsTheGivenY()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g);
        Assert.Equal(77, map.YPhysicInTop(-30, 77));
        Assert.Equal(77, map.YPhysicInTop(50000, 77));
        Assert.Equal(-90, map.YPhysicInTop(50, -90));
        Assert.Equal(5000, map.YPhysicInTop(50, 5000));
    }

    [Fact]
    public void YPhysicInTopBot_NeverMovesY()
    {
        FakeGame g = new();
        Assert.Equal(123, MapTestKit.BuildMap(g).YPhysicInTopBot(5, 123));
    }

    [Fact]
    public void TileTypeAtAndLastY_UseTheTypeTable()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g);
        Assert.False(map.TileTypeAt(10, 10, 2));
        Assert.Equal(720, map.LastY(10, 10));

        // a map with a tile file: 4 x 3 tiles, row 2 is ground (type 2)
        string root = Path.Combine(Path.GetTempPath(), "nro-tile-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "map", "tile_map_data"));
            byte[] file = new byte[3 + 12];
            file[1] = 4;
            file[2] = 3;
            for (int i = 0; i < 12; i++)
            {
                file[3 + i] = (byte)(i >= 8 ? 5 : 0);
            }

            File.WriteAllBytes(Path.Combine(root, "map", "tile_map_data", "31"), file);
            FakeGame g2 = new(dataRoot: root);
            Map m = MapTestKit.BuildMap(g2, 31);
            Assert.Equal((4, 3, 96, 72), (m.Tmw, m.Tmh, m.Pxw, m.Pxh));
            Assert.Equal(12, m.Maps!.Length);
            Assert.Equal(5, m.Maps[11]);
            int[] types = m.Types!;
            Assert.Equal(12, types.Length);
            types[(2 * 4) + 1] = 2;
            Assert.True(m.TileTypeAt(30, 50, 2));
            Assert.False(m.TileTypeAt(30, 10, 2));
            Assert.False(m.TileTypeAt(500, 500, 2));
            Assert.Equal(48, m.LastY(30, 0));
            Assert.Equal(50, m.LastY(30, 50));
            Assert.Equal(720, m.LastY(80, 0));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ReadTileMap_MissingOrTruncatedFile_IsIgnoredLikeTheJavaIOException()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g, 40);
        Assert.Null(map.Types);
        Assert.Equal(0, map.Tmw);

        string root = Path.Combine(Path.GetTempPath(), "nro-tile-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "map", "tile_map_data"));
            File.WriteAllBytes(Path.Combine(root, "map", "tile_map_data", "41"), [0, 5, 4, 1, 2, 3]);
            FakeGame g2 = new(dataRoot: root);
            Map truncated = MapTestKit.BuildMap(g2, 41);
            Assert.Equal((5, 4), (truncated.Tmw, truncated.Tmh));
            Assert.Null(truncated.Types);
            Assert.Equal(20, truncated.Maps!.Length);
            Assert.Equal([1, 2, 3], truncated.Maps.Take(3));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void MoveXY_ReturnsTheLastSentPositionWhenTheTileIsBlocked()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g);
        Player p = g.NewPlayer(1, 50, 170);
        p.XSend = 11;
        p.YSend = 22;
        // tile (row 7, col 2) is 0: not blocked; LastY with an empty type table is the 30 tile fall (720) which is past the bottom (Pxh = 0 - 24)
        Assert.Equal([11, 22], map.MoveXY(p));
        Player q = g.NewPlayer(2, 50, 150);
        q.XSend = 1;
        q.YSend = 2;
        // row 6 is ground (non zero): row 3 above is empty, so the answer is {x, row}
        Assert.Equal([50, 6], map.MoveXY(q));
    }

    // ---- mobs of the map -------------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void InitMob_CreatesAMobPerTemplateEntryInEveryZone_WithTheTemplateValues()
    {
        FakeGame g = new(seed: 3);
        g.MobTemplates[25] = new MobTemplate { Id = 25, Type = 4, PercentDame = 6, PercentTiemNang = 9, Percent_gold = 2 };
        Map map = MapTestKit.BuildMap(g, 60, zones: 2);
        map.InitMob([25, 25, 99], [3, 4, 5], [1000, 2000, 3000], [10, 20, 30], [40, 50, 60]);
        foreach (Zone z in map.Zones)
        {
            Assert.Equal(2, z.Mobs.Count);
            Mob first = z.Mobs[0];
            Assert.Equal((0, 25, (sbyte)3, 1000L, 10, 40), (first.Id, first.TempId, first.Level, first.Point.GetHpFull(), first.Location.X, first.Location.Y));
            Assert.Equal(1000, first.Point.Hp);
            Assert.Same(z, first.Zone);
            Assert.Equal(1, z.Mobs[1].Id);
        }

        Assert.NotSame(map.Zones[0].Mobs[0], map.Zones[1].Mobs[0]);
    }

    [Fact]
    public void InitMob_CopiesTheMobButNotItsPercentGold_AsJavaDid()
    {
        FakeGame g = new(seed: 3);
        g.MobTemplates[25] = new MobTemplate { Id = 25, Type = 4, PercentDame = 6, PercentTiemNang = 9, Percent_gold = 2 };
        Map map = MapTestKit.BuildMap(g, 60);
        map.InitMob([25], [3], [1000], [10], [40]);
        Mob zoneMob = map.Zones[0].Mobs[0];
        Assert.Equal(0, zoneMob.Percent_gold);
        Assert.Equal(4, zoneMob.Type);
        Assert.Equal(6, zoneMob.DamagePercent);
        Assert.Equal(9, zoneMob.PotentialPercent);
    }

    [Fact]
    public void InitMob_BigBossTemplatesGetTheirOwnClass()
    {
        FakeGame g = new();
        (sbyte Temp, Type Class)[] table =
        [
            (ConstMob.HIRUDEGARN, typeof(Hirudegarn)),
            (ConstMob.KING_OCTOPUS, typeof(KingOctopus)),
            (ConstMob.GUARD_ROBOT, typeof(GuardRobot)),
            (ConstMob.BEAR_THIEF, typeof(BearThief)),
            (ConstMob.NINE_TUSKED_ELEPHANT, typeof(NineTuskedElephant)),
            (ConstMob.NINE_SPURRED_ROOSTER, typeof(NineSpurredRooster)),
            (ConstMob.NINE_MANED_HORSE, typeof(NineManedHorse)),
            (ConstMob.PIANO, typeof(Piano)),
            (ConstMob.KONG, typeof(Kong)),
            (ConstMob.GODZILLA, typeof(Godzilla)),
            (25, typeof(Mob)),
        ];
        foreach ((sbyte temp, Type cls) in table)
        {
            g.MobTemplates[temp] = new MobTemplate { Id = temp };
        }

        Map map = MapTestKit.BuildMap(g, 61);
        map.InitMob(table.Select(t => t.Temp).ToArray(), table.Select(_ => (sbyte)1).ToArray(), table.Select(_ => 500).ToArray(), table.Select(_ => (short)1).ToArray(), table.Select(_ => (short)1).ToArray());
        Assert.Equal(table.Select(t => t.Class), map.Zones[0].Mobs.Select(m => m.GetType()));
    }

    [Fact]
    public void InitMob_FromAMobList_CopiesIntoEveryZone()
    {
        FakeGame g = new();
        Map map = MapTestKit.BuildMap(g, 62, zones: 2);
        Zone scratch = MapTestKit.BuildZone(g, 63);
        Mob proto = MapTestKit.NewMob(scratch, 4, 25, 800);
        map.InitMob([proto]);
        foreach (Zone z in map.Zones)
        {
            Mob copy = Assert.Single(z.Mobs);
            Assert.NotSame(proto, copy);
            Assert.Equal((4, 25, 800L), (copy.Id, copy.TempId, copy.Point.GetHpFull()));
            Assert.Same(z, copy.Zone);
        }
    }
}
