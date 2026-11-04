using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Templates;
using Nro.Core.Util;
using Nro.Game;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.World;

/// <summary>State the fake keeps for one <see cref="Player"/> (the real shell has no position or hp yet).</summary>
internal sealed class FakePlayerState
{
    public int X { get; set; }

    public int Y { get; set; }

    public long Hp { get; set; } = 1000;

    public long HpMax { get; set; } = 1000;

    public bool IsDie { get; set; }

    public bool IsPl { get; set; } = true;

    public bool NonInteractiveNpc { get; set; }

    public Func<MobPlayerView, MobPlayerView>? ViewTransform { get; set; }

    public MobLootPlayerView Loot { get; set; } = new();
}

/// <summary>One packet recorded by <see cref="FakeGame"/>.</summary>
internal sealed record SentPacket(string Target, sbyte Command, byte[] Data);

/// <summary>
/// A recording fake of every seam the map model uses (world, rules, player gateway, loot host, pick-up handler, flags). Nothing here is shared between tests: each test
/// builds its own, with its own <see cref="ManualTimeProvider"/> and <see cref="SeededRandomProvider"/>.
/// </summary>
internal sealed class FakeGame : IMapWorld, IMapRules, IMapPlayerGateway, IMobLootHost, IItemPickupHandler, IGameFlags
{
    private readonly Dictionary<Player, FakePlayerState> _players = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<string, HashSet<int>> _mapSets = [];

    public FakeGame(long seed = 12345, long startMs = 1_700_000_000_000, string? dataRoot = null)
    {
        DataRoot = dataRoot ?? Path.Combine(Path.GetTempPath(), "nro-no-such-data-root");
        Time = new ManualTimeProvider(DateTimeOffset.FromUnixTimeMilliseconds(startMs));
        Runtime = new MapRuntime(this, this, this, this, this, this, new SeededRandomProvider(seed), Time, true, DataRoot);
    }

    public string DataRoot { get; }

    public ManualTimeProvider Time { get; }

    public MapRuntime Runtime { get; }

    public List<SentPacket> Packets { get; } = [];

    public List<string> Calls { get; } = [];

    public Dictionary<int, Map> Maps { get; } = [];

    public Dictionary<int, MobTemplate> MobTemplates { get; } = [];

    public Dictionary<int, int> FixedZoneCounts { get; } = [];

    public Dictionary<short, sbyte> ItemTypes { get; } = [];

    public Dictionary<Player, int> TaskIds { get; } = new(ReferenceEqualityComparer.Instance);

    public ItemPickupResult PickResult { get; set; } = ItemPickupResult.Picked;

    public double? GoldAttribute { get; set; }

    public bool NamekWar { get; set; }

    public int TaskIdDefault { get; set; }

    public Func<Player, Mob, bool>? OnKill { get; set; }

    // ---- flags
    public bool LunarNewYear { get; set; }

    public bool Christmas { get; set; }

    public bool VuLanFestival { get; set; }

    public bool Halloween { get; set; }

    public bool InternationalWomansDay { get; set; }

    public bool MoonFestival { get; set; }

    public bool HungVuong { get; set; }

    public bool ValentineDay { get; set; }

    public bool Day2010 { get; set; }

    public bool MaintenanceRunning { get; set; }

    // ---- helpers for tests
    public Player NewPlayer(long id, int x = 0, int y = 0, Zone? zone = null)
    {
        Player p = new() { Id = id, Zone = zone, Name = "p" + id };
        _players[p] = new FakePlayerState { X = x, Y = y };
        return p;
    }

    public FakePlayerState State(Player p) => _players[p];

    public void SetMapSet(string name, params int[] mapIds) => _mapSets[name] = [.. mapIds];

    public IEnumerable<SentPacket> PacketsOf(sbyte command) => Packets.Where(p => p.Command == command);

    private bool In(string name, int mapId) => _mapSets.TryGetValue(name, out HashSet<int>? set) && set.Contains(mapId);

    private void Record(string target, Message message) => Packets.Add(new SentPacket(target, message.Command, message.GetData()));

    // ---- IMapWorld
    public void SendMessAllPlayerInMap(Zone zone, Message message) => Record("zone:" + zone.Map.MapId + "/" + zone.ZoneId, message);

    public void SendMessAllPlayerInMap(Player player, Message message) => Record("zoneOf:" + player.Id, message);

    public void SendMessAnotherNotMeInMap(Player player, Message message) => Record("others:" + player.Id, message);

    public void SendNotification(Player player, string text) => Calls.Add("thongbao:" + player.Id + ":" + text);

    public void DropItemMap(Zone zone, ItemMap itemMap) => Calls.Add("drop:" + itemMap.ItemTemplate?.Id + "x" + itemMap.Quantity + "@" + itemMap.X + "," + itemMap.Y);

    public void SendBigBoss(Zone zone, int action, int type, int playerId, long damage) => Calls.Add($"bigboss:{action}:{type}:{playerId}:{damage}");

    public void SendBigBoss2(Zone zone, int action, Mob mob) => Calls.Add($"bigboss2:{action}:{mob.LvMob}");

    public void SendBigBoss2(Player player, int action, Mob mob) => Calls.Add($"bigboss2p:{player.Id}:{action}");

    public void SendBodyMob(Mob mob, int type, int id) => Calls.Add($"bodymob:{type}:{id}");

    public ItemTemplate? GetItemTemplate(short id) =>
        id < 0 ? null : new ItemTemplate { Id = id, Type = ItemTypes.TryGetValue(id, out sbyte t) ? t : (sbyte)0, Name = "item" + id };

    public bool IsBlackBall(int itemTemplateId) => itemTemplateId >= 14 && itemTemplateId <= 20 && In("BlackBall", itemTemplateId);

    public bool IsNamecBall(int itemTemplateId) => In("NamecBall", itemTemplateId);

    public bool IsNamecBallStone(int itemTemplateId) => In("NamecStone", itemTemplateId);

    public void RemoveItemMapAndSendClient(ItemMap item)
    {
        Calls.Add("removeItem:" + item.ItemTemplate?.Id);
        item.Zone?.RemoveItemMap(item);
    }

    public void SendItemMapDisappear(ItemMap item) => Calls.Add("itemDisappear:" + item.ItemTemplate?.Id);

    public IReadOnlyList<MapNpcInfo> GetNpcsForPlayer(Player player) => [new MapNpcInfo(1, 100, 200, 5, 7)];

    public int GetTaskId(Player player) => TaskIds.TryGetValue(player, out int id) ? id : TaskIdDefault;

    public void OnMobKilled(Player killer, Mob mob) => Calls.Add("killed:" + killer.Id + ":" + mob.Id);

    public void AddSMTN(Player player, sbyte type, long amount, bool isOri) => Calls.Add($"smtn:{player.Id}:{type}:{amount}");

    public void AddTrainingPowerPotential(Player player, long potential) => Calls.Add($"train:{player.Id}:{potential}");

    public int CurrLevel { get; set; } = 10;

    public int GetCurrLevel(Player player) => CurrLevel;

    public void CreateBossInZone(short bossId, Zone zone) => Calls.Add($"boss:{bossId}:{zone.Map.MapId}/{zone.ZoneId}");

    public double? GetActiveGoldAttributeValue() => GoldAttribute;

    public bool IsNamekWarTime() => NamekWarFailure != null ? throw NamekWarFailure : NamekWar;

    public void SendNamekWarEnd(Zone zone, int result, bool flag) => Calls.Add($"namekWarEnd:{result}:{flag}");

    public void GiveNamekWarPrize(IReadOnlyList<Player> players, int result, int point) => Calls.Add($"namekWarPrize:{result}:{players.Count}:{point}");

    public void PickNamekWarBall(Player player, ItemMap itemMap) => Calls.Add("namekWarPick:" + player.Id);

    public void SetPos(Player player, int x, int y)
    {
        Calls.Add($"setPos:{player.Id}:{x}:{y}");
        FakePlayerState s = _players[player];
        s.X = x;
        s.Y = y;
    }

    public void RegisterZone(Map map, int zoneIndex, Zone zone) => Calls.Add($"register:{map.Type}:{zoneIndex}");

    public void UpdateBadgesTaskCount(Player player, int taskType, int count) => Calls.Add($"badge:{taskType}:{count}");

    public void PullItemToPlayer(Player player, int itemMapId) => Calls.Add($"pull:{player.Id}:{itemMapId}");

    // ---- IMapRules
    public bool IsMapRiengTu(int mapId) => In("RiengTu", mapId);

    public bool IsMapDiaNguc(int mapId) => In("DiaNguc", mapId);

    public bool IsMapNguHanhSon(int mapId) => In("NguHanhSon", mapId);

    public bool IsMapHungVuongEvent(int mapId) => In("HungVuongEvent", mapId);

    public bool IsDestronGasMap(int mapId) => In("DestronGas", mapId);

    public bool IsDungeonMap(int mapId) => In("Dungeon", mapId);

    public bool IsMapNotDropItems(int mapId) => In("NotDrop", mapId);

    public bool IsMapChristMasEvent(int mapId) => In("ChristMas", mapId);

    public bool IsMapUpSKH(int mapId) => In("UpSKH", mapId);

    public bool IsMapSauLang(int mapId) => In("SauLang", mapId);

    public bool IsMapMobBangHoiPart1(int mapId) => In("MobBangHoi1", mapId);

    public bool IsMapKongvsGodzila(int mapId) => In("KongGodzila", mapId);

    public bool IsMapBang(int mapId) => In("Bang", mapId);

    public bool IsTreasureUnderSeaMap(int mapId) => In("TreasureUnderSea", mapId);

    public bool IsMapLang(int mapId) => In("Lang", mapId);

    public bool IsMapHanhTinhNgucTu(int mapId) => In("NgucTu", mapId);

    public bool IsMapTuongLai(int mapId) => In("TuongLai", mapId);

    public bool IsMapCold(int mapId) => In("Cold", mapId);

    public bool IsMapHirudegarn(int mapId) => In("Hirudegarn", mapId);

    public bool IsMapBinhHutNangLuong(int mapId) => In("BinhHut", mapId);

    public bool IsMapPotara(int mapId) => In("Potara", mapId);

    public bool IsBarracksMap(int mapId) => In("Barracks", mapId);

    public bool IsMapOffline(int mapId) => In("Offline", mapId);

    public bool IsMapBangHoi(int mapId) => In("BangHoi", mapId);

    public Map? GetMapById(int mapId) => Maps.GetValueOrDefault(mapId);

    public MobTemplate? GetMobTemplate(int mobTempId) => MobTemplates.GetValueOrDefault(mobTempId);

    public string GetPlanetName(sbyte planetId) => "planet" + planetId;

    public int? GetFixedZoneCount(sbyte mapType) => FixedZoneCounts.TryGetValue(mapType, out int n) ? n : null;

    // ---- IMapPlayerGateway
    public int GetX(Player player) => _players[player].X;

    public int GetY(Player player) => _players[player].Y;

    public bool IsDie(Player player) => _players[player].IsDie;

    public bool IsPl(Player player) => _players[player].IsPl;

    public bool RecordPlayerUpdates { get; set; } = true;

    public Exception? NamekWarFailure { get; set; }

    public void UpdatePlayer(Player player)
    {
        if (RecordPlayerUpdates)
        {
            Calls.Add("updatePlayer:" + player.Id);
        }
    }

    public double Injured(Player victim, Player? attacker, double damage, bool piercing, bool isMobAttack)
    {
        FakePlayerState s = _players[victim];
        long hit = (long)damage;
        s.Hp = Math.Max(0, s.Hp - hit);
        Calls.Add($"injured:{victim.Id}:{hit}");
        return hit;
    }

    public void SendMessage(Player player, Message message) => Record("player:" + player.Id, message);

    public void SendPlayerInfo(Player receiver, Player info) => Calls.Add($"info:{receiver.Id}<-{info.Id}");

    public int GetIdSpaceShip(Player player) => 3;

    public bool SameClan(Player a, Player b) => a.Name == b.Name;

    public int GetClanId(Player player) => -1;

    public bool IsNonInteractiveNpc(Player player) => _players.TryGetValue(player, out FakePlayerState? s) && s.NonInteractiveNpc;

    public bool IsDamageTestPlayer(Player player) => false;

    public bool IsTrainingBossOf(Player candidate, Player owner) => false;

    public MobPlayerView GetMobPlayerView(Player player)
    {
        FakePlayerState s = _players[player];
        MobPlayerView view = new() { IsDie = s.IsDie, X = s.X, Y = s.Y, Hp = s.Hp, HpMax = s.HpMax, ThuHutOwner = player };
        return s.ViewTransform != null ? s.ViewTransform(view) : view;
    }

    public void ClearSatelliteDefend(Player player) => Calls.Add("clearDefend:" + player.Id);

    public void ApplySatelliteEffect(Player player, int itemTemplateId, long nowMs) => Calls.Add($"satellite:{player.Id}:{itemTemplateId}");

    public bool HasSatellite(Player player) => true;

    public double CalculatePowerPotential(Player player, long potential) => potential;

    public MafubaInfo? GetMafubaInfo(Player player) => new(10, _players[player].HpMax, 0, 1);

    public EggMobSource GetEggMobSource(Player player) => new(3, _players[player].HpMax, 400, 0);

    // ---- IMobLootHost
    public MobLootPlayerView GetLootView(Player player) => _players[player].Loot;

    public bool HasChristmasHat(Player player) => true;

    public bool CanDropChristmasCandy(Player player, int limit) => true;

    public bool CanDropTatVoGiangSinh(Player player, int limit) => true;

    public bool CanDropBinhNuoc(Player player, int limit) => true;

    public void AddEventPointNHS(Player player, int points) => Calls.Add("nhs:" + points);

    public bool HasNamekianEquipment(Player player) => false;

    public int CountBodySetPieces(Player player, BodySetKind kind) => 0;

    public bool HasEnergyAbsorbingBottle(Player player) => false;

    public IReadOnlyList<int> GetBinhHutNangLuongParams(Player player) => [];

    public void IncreaseBinhHutNangLuong(Player player, int index) => Calls.Add("binhhut:" + index);

    public List<ItemMapOption> CreateActivationSetOptions(int itemId) => [new ItemMapOption(1, 1)];

    public Player GetBinhNuocOwner(Player player) => player;

    // ---- IItemPickupHandler
    public ItemPickupResult TryPick(Player player, ItemMap itemMap, int itemMapId)
    {
        Calls.Add("tryPick:" + player.Id + ":" + itemMapId);
        return PickResult;
    }

    public void OnPicked(Player player, ItemMap itemMap) => Calls.Add("onPicked:" + player.Id);
}
