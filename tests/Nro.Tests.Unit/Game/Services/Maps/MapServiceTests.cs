using Nro.Core.Consts;
using Nro.Core.Templates;
using Nro.Game.Content;
using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Game.Services.Maps;
using Nro.Game.World.Maps;
using Nro.Game.World.Maps.Ports;
using Nro.Tests.Unit.Game.World;
using Map = Nro.Game.World.Maps.Map;

namespace Nro.Tests.Unit.Game.Services.Maps;

// Kiểm tra port MapService theo các biên dải Java và luật chọn zone.
// Nguồn Java: nro/services/MapService.java (dòng 21–703).
public sealed class MapServiceTests
{
    // Mỗi hàng kiểm dải đóng và phần tử sát ngoài dải theo các điều kiện phân loại Java.
    [Theory]
    [InlineData(MapIds.HOME_MAP_FIRST - 1, false, MapIds.HOME_MAP_FIRST, true, MapIds.HOME_MAP_LAST, true, MapIds.HOME_MAP_LAST + 1, false)]
    [InlineData(MapIds.BLACK_BALL_WAR_FIRST - 1, false, MapIds.BLACK_BALL_WAR_FIRST, true, MapIds.BLACK_BALL_WAR_LAST, true, MapIds.BLACK_BALL_WAR_LAST + 1, false)]
    [InlineData(MapIds.MAJIN_BUU_TWELVE_HOUR_FIRST - 1, false, MapIds.MAJIN_BUU_TWELVE_HOUR_FIRST, true, MapIds.MAJIN_BUU_TWELVE_HOUR_LAST, true, MapIds.MAJIN_BUU_TWELVE_HOUR_LAST + 1, false)]
    [InlineData(MapIds.BARRACKS_FIRST - 1, false, MapIds.BARRACKS_FIRST, true, MapIds.BARRACKS_LAST, true, MapIds.BARRACKS_LAST + 1, false)]
    [InlineData(MapIds.TREASURE_UNDER_SEA_FIRST - 1, false, MapIds.TREASURE_UNDER_SEA_FIRST, true, MapIds.TREASURE_UNDER_SEA_LAST, true, MapIds.TREASURE_UNDER_SEA_LAST + 1, false)]
    [InlineData(MapIds.SNAKE_WAY_FIRST - 1, false, MapIds.SNAKE_WAY_FIRST, true, MapIds.SNAKE_WAY_LAST, true, MapIds.SNAKE_WAY_LAST + 1, false)]
    [InlineData(MapIds.FIVE_THOUSAND_YEARS_AGO_FIRST - 1, false, MapIds.FIVE_THOUSAND_YEARS_AGO_FIRST, true, MapIds.FIVE_THOUSAND_YEARS_AGO_LAST, true, MapIds.FIVE_THOUSAND_YEARS_AGO_LAST + 1, false)]
    public void Range_classifiers_keep_java_boundaries(int before, bool expectedBefore, int first, bool expectedFirst, int last, bool expectedLast, int after, bool expectedAfter)
    {
        MapService service = CreateService();

        Func<int, bool> classifier = first switch
        {
            MapIds.HOME_MAP_FIRST => service.IsHome,
            MapIds.BLACK_BALL_WAR_FIRST => service.IsMapBlackBallWar,
            MapIds.MAJIN_BUU_TWELVE_HOUR_FIRST => service.IsMapMaBu12H,
            MapIds.BARRACKS_FIRST => service.IsBarracksMap,
            MapIds.TREASURE_UNDER_SEA_FIRST => service.IsTreasureUnderSeaMap,
            MapIds.SNAKE_WAY_FIRST => service.IsMapConDuongRanDoc,
            MapIds.FIVE_THOUSAND_YEARS_AGO_FIRST => service.IsMap5000NamTruoc,
            _ => throw new InvalidOperationException(),
        };

        Assert.Equal(expectedBefore, classifier(before));
        Assert.Equal(expectedFirst, classifier(first));
        Assert.Equal(expectedLast, classifier(last));
        Assert.Equal(expectedAfter, classifier(after));
    }

    // Các ngoại lệ và dải rời giữ nguyên các điều kiện Java.
    [Fact]
    public void Classifiers_keep_java_exceptions_and_disjoint_ranges()
    {
        MapService service = CreateService();

        Assert.True(service.IsDestronGasMap(MapIds.DESTRON_GAS_FIRST));
        Assert.False(service.IsDestronGasMap(MapIds.DESTRON_GAS_EXCLUDED));
        Assert.True(service.IsDestronGasMap(MapIds.DESTRON_GAS_LAST));
        Assert.True(service.IsMapLuyenTap(MapIds.TRAINING_FIRST));
        Assert.False(service.IsMapLuyenTap(MapIds.TRAINING_EXCLUDED));
        Assert.True(service.IsMapLuyenTap(MapIds.TRAINING_LAST));
        Assert.True(service.IsMapTuongLai(MapIds.FUTURE_EARLY_LAST));
        Assert.False(service.IsMapTuongLai(MapIds.FUTURE_EARLY_LAST + 1));
        Assert.True(service.IsMapTuongLai(MapIds.FUTURE_LATE_FIRST));
        Assert.True(service.IsMapTuongLai(MapIds.FUTURE_SINGLE_FIRST));
        Assert.True(service.IsMapCold(MapIds.COLD_FIRST));
        Assert.True(service.IsMapCold(MapIds.COLD_DESTRON_GAS_MAP));
        Assert.True(service.IsMapCold(MapIds.COLD_SANCTUARY_LAST));
        Assert.False(service.IsMapCold(MapIds.COLD_SANCTUARY_LAST + 1));
        Assert.True(service.IsMapGiaiCuuMiNuong(MapIds.RESCUE_MI_NUONG_FIRST_LAST));
        Assert.False(service.IsMapGiaiCuuMiNuong(MapIds.RESCUE_MI_NUONG_FIRST_LAST + 1));
        Assert.True(service.IsMapGiaiCuuMiNuong(MapIds.RESCUE_MI_NUONG_SECOND));
        Assert.True(service.IsMapWar(MapIds.WAR_MAP_FIRST));
        Assert.True(service.IsMapWar(MapIds.WAR_MAP_LAST));
        Assert.False(service.IsMapWar(MapIds.WAR_MAP_LAST - 1));
    }

    // Map offline luôn vào zone 0, bỏ qua zoneId theo Java.
    [Fact]
    public void Get_map_can_join_uses_zone_zero_for_offline_map()
    {
        var game = new FakeGame();
        Map map = CreateMap(game, MapIds.CLAN_BOSS_FALLBACK_MAP, ConstMap.MAP_OFFLINE, 2);
        var directory = new TestDirectory(map);
        MapService service = CreateService(directory);
        var player = new Player { Location = new Location() };

        Assert.Same(map.Zones[0], service.GetMapCanJoin(player, map.MapId, 1));
    }

    // Map thường tôn trọng zoneId, còn -1 chọn zone còn chỗ đầu tiên đúng getZone Java.
    [Fact]
    public void Get_map_can_join_selects_requested_or_available_zone_for_regular_map()
    {
        var game = new FakeGame();
        Map map = CreateMap(game, 40, ConstMap.MAP_NORMAL, 2);
        MapService service = CreateService(new TestDirectory(map));
        var player = new Player { Location = new Location() };

        // Java: MapService.getMapCanJoin, dòng 250–251 — map thường gọi getZoneByMapIDAndZoneID.
        Assert.Same(map.Zones[1], service.GetMapCanJoin(player, map.MapId, 1));
        // Java: MapService.getMapCanJoin, dòng 248–249 — zoneId -1 gọi getZone.
        Assert.Same(map.Zones[0], service.GetMapCanJoin(player, map.MapId, -1));
    }

    // Danh sách map Ngọc đen phải fail-fast thay vì nuốt map runtime thiếu.
    [Fact]
    public void Black_ball_list_fails_fast_when_required_map_is_missing()
    {
        MapService service = CreateService();

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(service.GetMapBlackBall);
        Assert.Contains(MapIds.BLACK_BALL_WAR_FIRST.ToString(), error.Message);
    }

    private static MapService CreateService(IMapDirectory? directory = null) => new(directory ?? new TestDirectory(), new TestCatalog(), new TestRandom(), new TestDungeons(), new TestMoves());

    private static Map CreateMap(FakeGame game, int mapId, sbyte type, int zones) => new(game.Runtime, mapId, "test", MapIds.PLANET_EARTH, 0, 0, 0, type, [new int[10]], [], zones, 7, [], [], 0);

    private sealed class TestDirectory(params Map[] maps) : IMapDirectory
    {
        public IReadOnlyList<Map> Maps { get; } = maps;
        public Map? GetMapById(int mapId) => Maps.FirstOrDefault(map => map.MapId == mapId);
    }

    private sealed class TestCatalog : IWorldCatalog
    {
        public int MobCount => 0;
        public int NpcCount => 0;
        public int MapCount => 0;
        public IReadOnlyList<MapTemplate> Maps { get; } = Array.Empty<MapTemplate>();
        public MobTemplate GetMob(int id) => throw new ArgumentOutOfRangeException(nameof(id));
        public NpcTemplate GetNpc(int id) => throw new ArgumentOutOfRangeException(nameof(id));
        public MapTemplate? GetMap(int id) => null;
        public bool TryGetMap(int id, out MapTemplate? map)
        {
            map = null;
            return false;
        }
    }

    private sealed class TestRandom : IPlayerRandom
    {
        public int NextInt(int minInclusive, int maxInclusive) => minInclusive;
    }

    private sealed class TestDungeons : IPlayerMapDungeonGateway
    {
        public bool HasDungeonInstance(Player player, int mapId) => false;
        public Zone? GetDungeonZone(Player player, int mapId) => null;
    }

    private sealed class TestMoves : IPlayerMoveGateway
    {
        public void SendPlayerMove(Player player) { }
    }
}
