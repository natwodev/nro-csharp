using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Game.Services.Maps.Ports;
using Nro.Game.World.Maps;
using Nro.Network.Packets.Maps;
using Nro.Tests.Unit.Game.World;
using Nro.Tests.Unit.Game.World.Maps;

namespace Nro.Tests.Unit.Game.Services.Maps;

// Kiểm tra I-03 theo thứ tự/lối rẽ ChangeMapService Java; fake chỉ ghi cổng ra ngoài.
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 66–454).
public sealed class ChangeMapServiceTests
{
    [Fact]
    public void Change_zone_refuses_null_zone_offline_dungeon_mabu_and_throttle_in_java_order()
    {
        var gateway = new FakeChangeMapGateway();
        var clock = new TestClock { NowMilliseconds = 10_000 };
        var service = new Nro.Game.Services.Maps.ChangeMapService(gateway, clock, new TestRandom());
        Player noZone = NewPlayer();
        service.ChangeZone(noZone, 0);
        Assert.Equal(["tutorial:Không thể đến khu vực này"], gateway.Calls);

        gateway.Calls.Clear();
        FakeGame game = new();
        Zone zone = MapTestKit.BuildZone(game, mapId: 7);
        Player player = NewPlayer(zone);
        gateway.Offline.Add(7);
        service.ChangeZone(player, 0);
        Assert.Equal(["tutorial:Không thể đến khu vực này"], gateway.Calls);

        gateway.Calls.Clear();
        gateway.Offline.Clear();
        gateway.Dungeon.Add(7);
        service.ChangeZone(player, 0);
        Assert.Equal(["tutorial:Không thể đến khu vực này"], gateway.Calls);

        gateway.Calls.Clear();
        gateway.Dungeon.Clear();
        gateway.MajinBuu.Add(7);
        service.ChangeZone(player, 0);
        Assert.Equal(["tutorial:Không thể đến khu vực này"], gateway.Calls);

        gateway.Calls.Clear();
        gateway.MajinBuu.Clear();
        player.IDMark.LastZoneChangeTime = 5_001;
        service.ChangeZone(player, 0);
        Assert.Single(gateway.Calls);
        Assert.StartsWith("tutorial:Chưa thể chuyển khu vực", gateway.Calls[0], StringComparison.Ordinal);
    }

    [Fact]
    public void Change_zone_rejects_invalid_or_full_destination_before_map_change()
    {
        var gateway = new FakeChangeMapGateway();
        var clock = new TestClock { NowMilliseconds = 10_000 };
        var service = new Nro.Game.Services.Maps.ChangeMapService(gateway, clock, new TestRandom());
        FakeGame game = new();
        Map map = MapTestKit.BuildMap(game, zones: 2, maxPlayer: 1);
        Zone source = map.Zones[0];
        Zone full = map.Zones[1];
        Player occupant = NewPlayer(full);
        full.AddPlayer(occupant);
        Player player = NewPlayer(source);
        source.AddPlayer(player);

        service.ChangeZone(player, 9);
        Assert.Equal(["notice:Không thể thực hiện"], gateway.Calls);
        gateway.Calls.Clear();
        clock.NowMilliseconds = 20_000;
        service.ChangeZone(player, 1);
        Assert.Equal(["tutorial:Khu vực này đã đầy"], gateway.Calls);
        Assert.Same(source, player.Zone);
    }

    [Fact]
    public void Space_ship_leaves_then_enters_and_emits_teleport_before_map_info()
    {
        var gateway = new FakeChangeMapGateway();
        var clock = new TestClock { NowMilliseconds = 20_000 };
        var service = new Nro.Game.Services.Maps.ChangeMapService(gateway, clock, new TestRandom());
        FakeGame game = new();
        Map sourceMap = MapTestKit.BuildMap(game, mapId: 1);
        Map destinationMap = MapTestKit.BuildMap(game, mapId: 2);
        Zone source = sourceMap.Zones[0];
        Zone destination = destinationMap.Zones[0];
        Player player = NewPlayer(source, id: 0x01020304);
        source.AddPlayer(player);

        service.ChangeMapBySpaceShip(player, destination, 200);

        Assert.DoesNotContain(player, source.Players);
        Assert.Contains(player, destination.Players);
        Assert.Same(destination, player.Zone);
        // Java ChangeMapService.changeMap dòng 504 luôn hủy giao dịch trước hiệu ứng tàu.
        Assert.Equal(["trade", "ship:-65", "pet", "clear", "map:-24", "special", "mabu", "namek-lift"], gateway.Calls);
        Assert.Equal(Nro.Game.Services.Maps.ChangeMapService.NonSpaceShip, player.IDMark.SpaceShipId);
    }

    [Theory]
    [InlineData((sbyte)1)]
    [InlineData((sbyte)2)]
    public void Namek_war_change_joins_the_selected_team_starts_once_and_emits_flag_and_instance(sbyte team)
    {
        var gateway = new FakeChangeMapGateway();
        var clock = new TestClock { NowMilliseconds = 20_000 };
        var service = new Nro.Game.Services.Maps.ChangeMapService(gateway, clock, new TestRandom());
        FakeGame game = new();
        Zone source = MapTestKit.BuildMap(game, mapId: 1).Zones[0];
        Zone requestedDestination = MapTestKit.BuildMap(game, mapId: 2).Zones[0];
        Zone namekWarZone = MapTestKit.BuildMap(game, mapId: 164).Zones[0];
        gateway.NamekWarZone = namekWarZone;
        Player player = NewPlayer(source);
        player.IDMark.NamekWarState = team;
        source.AddPlayer(player);

        service.ChangeMap(player, requestedDestination, 200, 100);

        Assert.Same(namekWarZone, player.Zone);
        Assert.Equal(team == 1, namekWarZone.PlayersCadic.Contains(player));
        Assert.Equal(team == 2, namekWarZone.PlayersFide.Contains(player));
        Assert.True(namekWarZone.IsNamekWarZoneStarted);
        Assert.Equal(clock.NowMilliseconds, namekWarZone.LastNamekWarStartTime);
        Assert.Contains("flag:" + team, gateway.Calls);
        Assert.Contains("namek-create", gateway.Calls);

        gateway.Calls.Clear();
        clock.NowMilliseconds = 30_000;
        service.ChangeMap(player, requestedDestination, 200, 100);

        Assert.Equal(20_000, namekWarZone.LastNamekWarStartTime);
        Assert.Contains("flag:" + team, gateway.Calls);
        Assert.Contains("namek-create", gateway.Calls);
    }

    [Fact]
    public void No_namek_war_state_does_not_lookup_or_emit_namek_war_calls()
    {
        var gateway = new FakeChangeMapGateway();
        var service = new Nro.Game.Services.Maps.ChangeMapService(gateway, new TestClock(), new TestRandom());
        FakeGame game = new();
        Zone source = MapTestKit.BuildMap(game, mapId: 1).Zones[0];
        Zone destination = MapTestKit.BuildMap(game, mapId: 2).Zones[0];
        Player player = NewPlayer(source);
        source.AddPlayer(player);

        service.ChangeMap(player, destination, 200, 100);

        Assert.DoesNotContain(gateway.Calls, call => call.StartsWith("namek-zone", StringComparison.Ordinal) || call.StartsWith("flag:", StringComparison.Ordinal) || call == "namek-create");
    }

    [Fact]
    public void Map_transport_uses_java_capsule_home_labels_and_null_player_is_ignored()
    {
        var gateway = new FakeChangeMapGateway();
        var service = new Nro.Game.Services.Maps.ChangeMapService(gateway, new TestClock(), new TestRandom());
        service.OpenChangeMapTab(null);
        FakeGame game = new();
        Zone home = MapTestKit.BuildMap(game, mapId: 1).Zones[0];
        Zone other = MapTestKit.BuildMap(game, mapId: 2).Zones[0];
        Zone blackBall = MapTestKit.BuildMap(game, mapId: 3).Zones[0];
        Player player = NewPlayer();
        player.IDMark.MapChangeType = Nro.Core.Consts.ConstMap.CHANGE_CAPSULE;
        player.MapBeforeCapsule = home;
        gateway.Capsule = [home, other];
        gateway.BlackBall = [blackBall];

        service.OpenChangeMapTab(player);

        MapTransportPacket packet = Assert.Single(gateway.Transports);
        Assert.Equal("Về chỗ cũ: " + home.Map.MapName, packet.Sections[0][0].MapName);
        Assert.Equal(other.Map.MapName, packet.Sections[0][1].MapName);
        Assert.Equal(blackBall.Map.MapName, packet.Sections[1][0].MapName);
    }

    private static Player NewPlayer(Zone? zone = null, long id = 1)
    {
        var player = new Player { Id = id, IsPlayer = true, Zone = zone };
        player.NPoint.Hp = 10;
        player.NPoint.Mp = 10;
        player.NPoint.HpMax = 10;
        player.NPoint.MpMax = 10;
        return player;
    }

    private sealed class TestClock : IPlayerClock
    {
        public long NowMilliseconds { get; set; } = 10_000;
    }

    private sealed class TestRandom : IPlayerRandom
    {
        public int NextInt(int minInclusive, int maxInclusive) => minInclusive;
    }

    private sealed class FakeChangeMapGateway : IChangeMapGateway
    {
        public List<string> Calls { get; } = [];
        public List<MapTransportPacket> Transports { get; } = [];
        public IReadOnlyList<Zone> Capsule { get; set; } = [];
        public IReadOnlyList<Zone> BlackBall { get; set; } = [];
        public HashSet<int> Offline { get; } = [];
        public HashSet<int> Dungeon { get; } = [];
        public HashSet<int> MajinBuu { get; } = [];
        public Zone? NamekWarZone { get; set; }

        public IReadOnlyList<Zone> GetCapsuleZones(Player player) => Capsule;
        public IReadOnlyList<Zone> GetBlackBallZones() => BlackBall;
        public IReadOnlyList<Zone> GetMajinBuuZones() => [];
        public bool IsOfflineMap(int mapId) => Offline.Contains(mapId);
        public bool IsDungeonMap(int mapId) => Dungeon.Contains(mapId);
        public bool IsMajinBuuMap(int mapId) => MajinBuu.Contains(mapId);
        public bool IsMajinBuuTwoHourMap(int mapId) => false;
        public bool IsColdMap(Map map) => false;
        public bool IsFutureMap(Map map) => false;
        public bool IsChristmasEventMap(int mapId) => false;
        public bool IsCerealMap(Map map) => false;
        public bool Is5000YearsAgoMap(int mapId) => false;
        public Zone? GetJoinableZone(Player player, int mapId, int zoneId) => null;
        public Zone? CheckMapCanJoin(Player player, Zone? zone) => zone;
        public Zone? CheckYardratDestination(Player player, Zone? zone) => zone;
        public void CancelTrade(Player player) => Calls.Add("trade");
        public Zone? GetNamekWarZone(sbyte team)
        {
            Calls.Add("namek-zone:" + team);
            return NamekWarZone;
        }
        public void ChangeNamekWarFlag(Player player, sbyte team) => Calls.Add("flag:" + team);
        public void SendCreateNamekWarInstance(Player player) => Calls.Add("namek-create");
        public void UpdateNamekBallLocation(Player player, Zone destination) => Calls.Add("namek");
        public void DropNamekBall(Player player) => Calls.Add("drop-namek");
        public bool IsNamekBallMap(int mapId) => true;
        public void ResetPoint(Player player, int x, int y) => Calls.Add("reset");
        public void StopCharge(Player player) => Calls.Add("stop-charge");
        public void RemoveBinding(Player player) => Calls.Add("remove-binding");
        public void Revive(Player player, long hp, long mp) => Calls.Add("revive");
        public void SendInfoHpMp(Player player) => Calls.Add("hpmp");
        public void SendNotification(Player player, string text) => Calls.Add("notice:" + text);
        public void SendTutorial(Player player, string text) => Calls.Add("tutorial:" + text);
        public void SendZoneList(Player player, IReadOnlyList<Zone> zones) => Calls.Add("zone-list");
        public void ClearMap(Player player) => Calls.Add("clear");
        public void JoinPetMap(Player player) => Calls.Add("pet");
        public void SendMapInfo(Player player, Zone zone) => Calls.Add("map:-24");
        public void CheckJoinSpecialMap(Player player) => Calls.Add("special");
        public void CheckJoinMajinBuuMap(Player player) => Calls.Add("mabu");
        public void ClearMajinBuuFight(Player player) => Calls.Add("clear-mabu");
        public void GrantMajinBuuEntryReward(Player player, int minutes) => Calls.Add("reward-mabu:" + minutes);
        public void SendPoint(Player player) => Calls.Add("point");
        public void SendCharacterInfo(Player player) => Calls.Add("character");
        public int GetTaskId(Player player) => 0;
        public void CallTauPayPay(Player player) => Calls.Add("paypay");
        public void SendNamekWarLiftUpdate(Player player) => Calls.Add("namek-lift");
        public WayPoint? GetWaypointPlayerIn(Player player) => null;
        public bool ShouldChangeDestronGasMap(int fromMapId, int toMapId) => false;
        public void BeginDestronGasMapChange(Player player, Zone destination, int x, int y) => Calls.Add("destron");
        public void SendMapTransport(Player player, MapTransportPacket packet) => Transports.Add(packet);
        public void SendSpaceShipArrival(Player player, SpaceShipArrivalPacket packet, SpaceShipArrivalAudience audience) => Calls.Add("ship:-65");
    }
}
