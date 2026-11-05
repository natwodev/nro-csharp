using Nro.Core.Consts;
using Nro.Core.Templates;
using Nro.Game.Content;
using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Game.Skills.Ports;
using Nro.Game.World.Maps;
using Nro.Game.World.Maps.Ports;
using Map = Nro.Game.World.Maps.Map;

namespace Nro.Game.Services.Maps;

// Tra cứu map runtime và luật phân loại; không singleton, không nạp DB.
// Nguồn Java: nro/services/MapService.java (dòng 21–703).
public sealed class MapService : IMapRules, ISkillMapRules
{
    // Danh mục Map runtime đã được host nạp. (Java: Manager.MAPS)
    private readonly IMapDirectory _maps;

    // Danh mục template thế giới chỉ đọc. (Java: Manager.getMobTemplateByTemp)
    private readonly IWorldCatalog _catalog;

    // Nguồn ngẫu nhiên đóng đoạn như Util.nextInt. (Java: Util.nextInt)
    private readonly IPlayerRandom _random;

    // Cổng instance phó bản bang hội chưa có model Clan hoàn chỉnh. (Java: player.clan.*)
    private readonly IPlayerMapDungeonGateway _dungeons;

    // Cổng phát chuyển động wire -7. (Java: Service.sendMessAllPlayerInMap)
    private readonly IPlayerMoveGateway _moves;

    // Ghép danh mục Map đã nạp cùng các cổng trạng thái/player. (Java: Manager.MAPS, Util.nextInt, Service)
    public MapService(IMapDirectory maps, IWorldCatalog catalog, IPlayerRandom random, IPlayerMapDungeonGateway dungeons, IPlayerMoveGateway moves)
    {
        _maps = maps ?? throw new ArgumentNullException(nameof(maps));
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _dungeons = dungeons ?? throw new ArgumentNullException(nameof(dungeons));
        _moves = moves ?? throw new ArgumentNullException(nameof(moves));
    }

    // Kiểm tra dải map Potara 189–192. (Java: isMapPotara, dòng 28–30)
    public bool IsMapPotara(int mapId) => InRange(mapId, MapIds.POTARA_FIRST, MapIds.POTARA_LAST);

    // Kiểm tra map Bình hút năng lượng 180. (Java: isMapBinhHutNangLuong, dòng 32–34)
    public bool IsMapBinhHutNangLuong(int mapId) => mapId == MapIds.ENERGY_ABSORPTION_BOTTLE_MAP;

    // Kiểm tra map cấm rơi vật phẩm theo biểu thức OR Java. (Java: isMapNotDropItems, dòng 36–40)
    public bool IsMapNotDropItems(int mapId) => IsTreasureUnderSeaMap(mapId) || IsBarracksMap(mapId) || IsMapConDuongRanDoc(mapId)
        || IsDestronGasMap(mapId) || IsMapGiaiCuuMiNuong(mapId) || IsMapTayKarin(mapId) || IsMapBossBangHoi(mapId)
        || IsMapHirudegarn(mapId) || IsMapTranhNgocNamec(mapId) || IsMapRiengTu(mapId);

    // Trả waypoint chứa đúng tọa độ player, biên đóng. (Java: getWaypointPlayerIn, dòng 42–49)
    public WayPoint? GetWaypointPlayerIn(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (player.Zone is null)
        {
            return null;
        }

        foreach (WayPoint waypoint in player.Zone.Map.WayPoints)
        {
            if (player.Location.X >= waypoint.MinX && player.Location.X <= waypoint.MaxX
                && player.Location.Y >= waypoint.MinY && player.Location.Y <= waypoint.MaxY)
            {
                return waypoint;
            }
        }

        return null;
    }

    // Chọn zone vào map, giữ fallback và luật quét hết quái/boss của bốn phó bản bang hội. (Java: getMapCanJoin, dòng 51–253)
    public Zone? GetMapCanJoin(Player player, int mapId, int zoneId)
    {
        ArgumentNullException.ThrowIfNull(player);
        if (IsMapOffline(mapId) || IsMapBangHoi(mapId))
        {
            return GetMapById(mapId)?.GetZoneByIndex(0);
        }

        if (IsBarracksMap(mapId))
        {
            Zone? zone = GetDungeonOrFallback(player, mapId, MapIds.HOME_CAPSULE_FIRST + player.Gender);
            if (zone is not null || player.Zone is null || !_dungeons.HasDungeonInstance(player, mapId)) return zone;
            return ResolveDungeonZone(player, mapId);
        }

        if (IsTreasureUnderSeaMap(mapId))
        {
            Zone? zone = GetDungeonOrFallback(player, mapId, MapIds.TREASURE_CAPSULE_MAP);
            if (zone is not null || player.Zone is null || !_dungeons.HasDungeonInstance(player, mapId)) return zone;
            return ResolveDungeonZone(player, mapId);
        }

        if (IsDestronGasMap(mapId))
        {
            Zone? zone = GetDungeonOrFallback(player, mapId, MapIds.TREASURE_CAPSULE_MAP);
            if (zone is not null || player.Zone is null || !_dungeons.HasDungeonInstance(player, mapId)) return zone;
            return ResolveDungeonZone(player, mapId);
        }

        if (IsMapConDuongRanDoc(mapId))
        {
            Zone? zone = GetDungeonOrFallback(player, mapId, MapIds.SNAKE_WAY_FALLBACK_MAP);
            if (zone is not null || player.Zone is null || !_dungeons.HasDungeonInstance(player, mapId)) return zone;
            return ResolveDungeonZone(player, mapId);
        }

        if (IsMapBossBangHoi(mapId) && (player.Zone is null || !_dungeons.HasDungeonInstance(player, mapId)))
        {
            Zone? fallback = GetZone(MapIds.CLAN_BOSS_FALLBACK_MAP);
            SetFallbackPosition(player, fallback);
            return fallback;
        }

        return zoneId == -1 ? GetZone(mapId) : GetZoneByMapIdAndZoneId(mapId, zoneId);
    }

    // Trả chỉ số zone ít người nhất; khởi tạo ngẫu nhiên như Java khi mọi zone đều từ 20 người. (Java: GetZone, dòng 256–267)
    public int GetZoneIndex(int mapId)
    {
        Map? map = GetMapById(mapId);
        if (map is null || map.Zones.Count == 0)
        {
            return -1;
        }

        int fewest = 20;
        int chosen = _random.NextInt(0, map.Zones.Count - 1);
        for (int i = 0; i < map.Zones.Count; i++)
        {
            if (map.Zones[i].GetNumOfPlayers() < fewest)
            {
                chosen = i;
                fewest = map.Zones[i].GetNumOfPlayers();
            }
        }

        return chosen;
    }

    // Chọn zone còn chỗ; giữ vòng lặp Java khi mọi zone đầy. (Java: getZone, dòng 269–280)
    public Zone? GetZone(int mapId)
    {
        Map? map = GetMapById(mapId);
        if (map is null)
        {
            return null;
        }

        if (map.Zones.Count == 0)
        {
            throw new InvalidOperationException($"Map {mapId} không có zone để vào.");
        }

        int index = 0;
        while (map.Zones[index].GetNumOfPlayers() >= map.Zones[index].MaxPlayer)
        {
            index = _random.NextInt(0, map.Zones.Count - 1);
        }

        return map.Zones[index];
    }

    // Lấy zone theo index, trả null khi index sai. (Java: getZoneJoinByMapIdAndZoneId, dòng 282–294)
    public Zone? GetZoneJoinByMapIdAndZoneId(Player player, int mapId, int zoneId)
    {
        ArgumentNullException.ThrowIfNull(player);
        return GetZoneByMapIdAndZoneId(mapId, zoneId);
    }

    // Tra Map runtime theo id. (Java: getMapById, dòng 310–317)
    public Map? GetMapById(int mapId) => _maps.GetMapById(mapId);

    // Tra Map runtime đầu tiên có gender type khớp. (Java: getMapByGender, dòng 319–326)
    public Map? GetMapByGender(int genderType)
    {
        foreach (Map map in _maps.Maps)
        {
            if (map.GenderType == genderType)
            {
                return map;
            }
        }

        return null;
    }

    // Chọn zone ngẫu nhiên của map. (Java: getMapWithRandZone, dòng 335–346)
    public Zone? GetMapWithRandomZone(int mapId)
    {
        Map? map = GetMapById(mapId);
        return map is null || map.Zones.Count == 0 ? null : map.Zones[_random.NextInt(0, map.Zones.Count - 1)];
    }

    // Đổi id hành tinh sang tên hiển thị Java, kể cả chính tả Namếc. (Java: getPlanetName, dòng 348–359)
    public string GetPlanetName(sbyte planetId) => planetId switch { MapIds.PLANET_EARTH => "Trái đất", MapIds.PLANET_NAMEK => "Namếc", MapIds.PLANET_SAIYAN => "Xayda", _ => "" };

    // Lập danh sách đích Capsule, loại trùng map và map hiện tại. (Java: getMapCapsule, dòng 367–395)
    public IReadOnlyList<Zone> GetMapCapsule(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        var zones = new List<Zone>();
        if (player.MapBeforeCapsule is not null && !IsHome(player.MapBeforeCapsule.Map.MapId) && !IsMapTuongLai(player.MapBeforeCapsule.Map.MapId))
        {
            AddCapsuleZone(player, zones, player.MapBeforeCapsule);
        }

        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.HOME_CAPSULE_FIRST + player.Gender, 0));
        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.TRAINING_CAPSULE_MAP, 0));
        if (player.TrainingLevel > 1) AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.TRAINING_FIRST, 0));
        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.EARTH_CAPSULE_MAP, 0));
        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.NAMEK_CAPSULE_MAP, 0));
        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.SAIYAN_CAPSULE_MAP, 0));
        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.TREASURE_CAPSULE_MAP, 0));
        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.CAPSULE_MAP_20, 0));
        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.CAPSULE_MAP_13, 0));
        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.GENDER_CAPSULE_FIRST + player.Gender, 0));
        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.CAPSULE_MAP_27, 0));
        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.CAPSULE_MAP_19, 0));
        AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.CAPSULE_MAP_79, 0));
        if (player.NPoint.Power > 20_000_000) AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.HIGH_POWER_CAPSULE_MAP, 0));
        if (player.TrainingLevel > 5) AddCapsuleZone(player, zones, GetMapCanJoin(player, MapIds.HIGH_TRAINING_CAPSULE_MAP, 0));
        return zones;
    }

    // Lấy zone 0 của bảy map Ngọc đen, fail-fast nếu dữ liệu runtime thiếu. (Java: getMapBlackBall, dòng 397–403)
    public IReadOnlyList<Zone> GetMapBlackBall()
    {
        var zones = new List<Zone>();
        for (int mapId = MapIds.BLACK_BALL_WAR_FIRST; mapId <= MapIds.BLACK_BALL_WAR_LAST; mapId++) zones.Add(GetRequiredZoneZero(mapId));
        return zones;
    }

    // Lấy zone 0 của bảy map Mabư 12 giờ, fail-fast nếu dữ liệu runtime thiếu. (Java: getMapMaBu, dòng 405–411)
    public IReadOnlyList<Zone> GetMapMaBu()
    {
        var zones = new List<Zone>();
        for (int mapId = MapIds.MAJIN_BUU_TWELVE_HOUR_FIRST; mapId <= MapIds.MAJIN_BUU_TWELVE_HOUR_LAST; mapId++) zones.Add(GetRequiredZoneZero(mapId));
        return zones;
    }

    // Phát vị trí -7 qua cổng adapter; narrowing long id nằm ở adapter như Java cast. (Java: sendPlayerMove, dòng 424–436)
    public void SendPlayerMove(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        _moves.SendPlayerMove(player);
    }

    // Kiểm tra map offline từ Type runtime, unknown là false. (Java: isMapOffline, dòng 438–445)
    public bool IsMapOffline(int mapId) => GetMapById(mapId)?.Type == ConstMap.MAP_OFFLINE;

    // Kiểm tra map bang hội từ Type runtime, unknown là false. (Java: isMapBangHoi, dòng 447–454)
    public bool IsMapBangHoi(int mapId) => GetMapById(mapId)?.Type == ConstMap.MAP_CLAN;

    // Kiểm tra map cấm pet theo biểu thức OR Java. (Java: isMapNotCanJoinPet, dòng 456–459)
    public bool IsMapNotCanJoinPet(int mapId) => IsMapOffline(mapId) || IsMapBangHoi(mapId) || IsMapPotaufeu(mapId) || IsMapWar(mapId)
        || IsMapTestDame(mapId) || IsMapTranhNgocNamec(mapId) || IsMapHungVuongEvent(mapId) || IsMapRiengTu(mapId);

    // Kiểm tra chuyển giữa hai map khí gas, trừ map 148. (Java: shouldChangeMap, dòng 461–465)
    public bool ShouldChangeDestronGasMap(int currentMapId, int newMapId) => IsDestronGasMap(currentMapId) && IsDestronGasMap(newMapId)
        && currentMapId != MapIds.DESTRON_GAS_TRANSITION_MAP && newMapId != MapIds.DESTRON_GAS_TRANSITION_MAP;

    // Kiểm tra map không thông báo theo biểu thức OR Java. (Java: isMapNoNottify, dòng 467–469)
    public bool IsMapNoNotify(int mapId) => IsDungeonMap(mapId) || IsMapBlackBallWar(mapId) || IsMapMaBu12H(mapId) || IsMapMabu14H(mapId)
        || IsMapLuyenTap(mapId) || IsMapDongNamKarin(mapId) || IsMapYardart(mapId) || IsMapOffline(mapId) || IsMapBangHoi(mapId) || IsMapRiengTu(mapId);

    // Kiểm tra map nhà 21–23. (Java: isHome, dòng 471–473)
    public bool IsHome(int mapId) => InRange(mapId, MapIds.HOME_MAP_FIRST, MapIds.HOME_MAP_LAST);

    // Kiểm tra ba map làng. (Java: isMapLang, dòng 475–477)
    public bool IsMapLang(int mapId) => mapId is MapIds.VILLAGE_EARTH or MapIds.VILLAGE_NAMEK or MapIds.VILLAGE_SAIYAN;

    // Kiểm tra dải map băng 156–159. (Java: isMapBang, dòng 478–480)
    public bool IsMapBang(int mapId) => InRange(mapId, MapIds.ICE_MAP_FIRST, MapIds.ICE_MAP_LAST);

    // Kiểm tra dải map thử sát thương. (Java: isMapTestDame, dòng 482–484)
    public bool IsMapTestDame(int mapId) => InRange(mapId, MapIds.DAMAGE_TEST_FIRST, MapIds.DAMAGE_TEST_LAST);

    // Kiểm tra dải hành tinh thực vật. (Java: isMapHanhTinhThucVat, dòng 486–488)
    public bool IsMapHanhTinhThucVat(int mapId) => InRange(mapId, MapIds.PLANT_PLANET_FIRST, MapIds.PLANT_PLANET_LAST);

    // Kiểm tra dải sự kiện Hùng Vương. (Java: isMapHungVuongEvent, dòng 490–492)
    public bool IsMapHungVuongEvent(int mapId) => InRange(mapId, MapIds.HUNG_KING_EVENT_FIRST, MapIds.HUNG_KING_EVENT_LAST);

    // Kiểm tra dải map riêng tư. (Java: isMapRiengTu, dòng 494–496)
    public bool IsMapRiengTu(int mapId) => InRange(mapId, MapIds.PRIVATE_MAP_FIRST, MapIds.PRIVATE_MAP_LAST);

    // Kiểm tra map Tranh ngọc Namec. (Java: isMapTranhNgocNamec, dòng 498–500)
    public bool IsMapTranhNgocNamec(int mapId) => mapId == MapIds.NAMEK_JEWEL_WAR_MAP;

    // Kiểm tra map luyện tập 45–50, trừ 47. (Java: isMapLuyenTap, dòng 502–504)
    public bool IsMapLuyenTap(int mapId) => InRange(mapId, MapIds.TRAINING_FIRST, MapIds.TRAINING_LAST) && mapId != MapIds.TRAINING_EXCLUDED;

    // Kiểm tra map Hirudegarn. (Java: isMapHirudegarn, dòng 506–508)
    public bool IsMapHirudegarn(int mapId) => mapId == MapIds.HIRUDEGARN_MAP;

    // Kiểm tra map Granola. (Java: isMapgranola, dòng 510–512)
    public bool IsMapGranola(int mapId) => mapId == MapIds.GRANOLA_MAP;

    // Kiểm tra map Sơn Tinh. (Java: isMapsontinh, dòng 514–516)
    public bool IsMapSonTinh(int mapId) => mapId == MapIds.SON_TINH_MAP;

    // Kiểm tra map Thủy Tinh. (Java: isMapthuytinh, dòng 517–519)
    public bool IsMapThuyTinh(int mapId) => mapId == MapIds.THUY_TINH_MAP;

    // Kiểm tra hai map Mabư 14 giờ. (Java: isMapMabu14H, dòng 521–523)
    public bool IsMapMabu14H(int mapId) => InRange(mapId, MapIds.MAJIN_BUU_FOURTEEN_HOUR_FIRST, MapIds.MAJIN_BUU_FOURTEEN_HOUR_LAST);

    // Kiểm tra map Đông Nam Karin. (Java: isMapDongNamKarin, dòng 525–527)
    public bool IsMapDongNamKarin(int mapId) => mapId == MapIds.EAST_KARIN_MAP;

    // Kiểm tra map Tây Karin. (Java: isMapTayKarin, dòng 528–530)
    public bool IsMapTayKarin(int mapId) => mapId == MapIds.WEST_KARIN_MAP;

    // Kiểm tra bốn map Địa ngục rời. (Java: isMapDiaNguc, dòng 532–534)
    public bool IsMapDiaNguc(int mapId) => mapId is MapIds.HELL_FIRST or MapIds.HELL_SECOND or MapIds.HELL_THIRD or MapIds.HELL_LAST;

    // Kiểm tra chín map nâng sức khỏe. (Java: isMapUpSKH, dòng 536–538)
    public bool IsMapUpSKH(int mapId) => mapId is MapIds.UP_HEALTH_FIRST or MapIds.UP_HEALTH_SECOND or MapIds.UP_HEALTH_THIRD or MapIds.UP_HEALTH_FOURTH
        or MapIds.UP_HEALTH_FIFTH or MapIds.UP_HEALTH_SIXTH or MapIds.UP_HEALTH_SEVENTH or MapIds.UP_HEALTH_EIGHTH or MapIds.UP_HEALTH_LAST;

    // Kiểm tra sáu map quái bang hội phần một. (Java: isMapMobBangHoi_Part1, dòng 540–542)
    public bool IsMapMobBangHoiPart1(int mapId) => mapId is MapIds.CLAN_MOB_FIRST or MapIds.CLAN_MOB_SECOND or MapIds.CLAN_MOB_THIRD
        or MapIds.CLAN_MOB_FOURTH or MapIds.CLAN_MOB_FIFTH or MapIds.CLAN_MOB_LAST;

    // Kiểm tra map Kong đấu Godzilla. (Java: isMapKongvsGodzila, dòng 544–546)
    public bool IsMapKongvsGodzila(int mapId) => mapId == MapIds.KONG_VS_GODZILLA_MAP;

    // Kiểm tra ba map sau làng. (Java: isMapSauLang, dòng 548–550)
    public bool IsMapSauLang(int mapId) => mapId is MapIds.BEHIND_VILLAGE_EARTH or MapIds.BEHIND_VILLAGE_NAMEK or MapIds.BEHIND_VILLAGE_SAIYAN;

    // Kiểm tra map chiến tranh Ngọc đen 85–91. (Java: isMapBlackBallWar, dòng 563–565)
    public bool IsMapBlackBallWar(int mapId) => InRange(mapId, MapIds.BLACK_BALL_WAR_FIRST, MapIds.BLACK_BALL_WAR_LAST);

    // Kiểm tra map Mabư 12 giờ 114–120. (Java: isMapMaBu12H, dòng 567–569)
    public bool IsMapMaBu12H(int mapId) => InRange(mapId, MapIds.MAJIN_BUU_TWELVE_HOUR_FIRST, MapIds.MAJIN_BUU_TWELVE_HOUR_LAST);

    // Kiểm tra map Doanh trại 53–62. (Java: isMapDoanhTrai, dòng 571–573)
    public bool IsBarracksMap(int mapId) => InRange(mapId, MapIds.BARRACKS_FIRST, MapIds.BARRACKS_LAST);

    // Kiểm tra map Bản đồ kho báu 135–138. (Java: isMapBanDoKhoBau, dòng 575–577)
    public bool IsTreasureUnderSeaMap(int mapId) => InRange(mapId, MapIds.TREASURE_UNDER_SEA_FIRST, MapIds.TREASURE_UNDER_SEA_LAST);

    // Kiểm tra map KVTH 179. (Java: isMapKVTH, dòng 578–580)
    public bool IsMapKvth(int mapId) => mapId == MapIds.CHRISTMAS_LAST;

    // Kiểm tra map Con đường rắn độc 141–144. (Java: isMapConDuongRanDoc, dòng 582–584)
    public bool IsMapConDuongRanDoc(int mapId) => InRange(mapId, MapIds.SNAKE_WAY_FIRST, MapIds.SNAKE_WAY_LAST);

    // Kiểm tra map khí gas 147–152, trừ 150. (Java: isMapKhiGasHuyDiet, dòng 586–588)
    public bool IsDestronGasMap(int mapId) => InRange(mapId, MapIds.DESTRON_GAS_FIRST, MapIds.DESTRON_GAS_LAST) && mapId != MapIds.DESTRON_GAS_EXCLUDED;

    // Kiểm tra map boss bang hội. (Java: isMapBossBangHoi, dòng 590–592)
    public bool IsMapBossBangHoi(int mapId) => mapId == MapIds.CLAN_BOSS_MAP;

    // Kiểm tra map phó bản theo biểu thức OR Java. (Java: isMapPhoBan, dòng 594–596)
    public bool IsDungeonMap(int mapId) => IsTreasureUnderSeaMap(mapId) || IsBarracksMap(mapId) || IsMapConDuongRanDoc(mapId)
        || IsDestronGasMap(mapId) || IsMapGiaiCuuMiNuong(mapId) || IsMapTayKarin(mapId) || IsMapBossBangHoi(mapId);

    // Kiểm tra map Yardrat 131–133. (Java: isMapYardart, dòng 598–600)
    public bool IsMapYardart(int mapId) => InRange(mapId, MapIds.YARDRAT_FIRST, MapIds.YARDRAT_LAST);

    // Kiểm tra hai map hành tinh ngục tù. (Java: isMapHanhTinhNgucTu, dòng 602–604)
    public bool IsMapHanhTinhNgucTu(int mapId) => mapId is MapIds.PRISON_PLANET_FIRST or MapIds.PRISON_PLANET_SECOND;

    // Kiểm tra map PVP. (Java: isMapPVP, dòng 606–608)
    public bool IsMapPvp(int mapId) => mapId == MapIds.PVP_MAP;

    // Kiểm tra map lạnh theo object Map. (Java: isMapCold(Map), dòng 610–613)
    public bool IsMapCold(Map map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return IsMapCold(map.MapId);
    }

    // Kiểm tra map lạnh theo id. (Java: isMapCold(int), dòng 614–617)
    public bool IsMapCold(int mapId) => InRange(mapId, MapIds.COLD_FIRST, MapIds.COLD_LAST) || mapId == MapIds.COLD_DESTRON_GAS_MAP
        || InRange(mapId, MapIds.COLD_SANCTUARY_FIRST, MapIds.COLD_SANCTUARY_LAST);

    // Kiểm tra map sự kiện Giáng sinh 174–179. (Java: isMapChristMasEvent, dòng 619–621)
    public bool IsMapChristMasEvent(int mapId) => InRange(mapId, MapIds.CHRISTMAS_FIRST, MapIds.CHRISTMAS_LAST);

    // Kiểm tra map lạnh cho bot: 105–110 hoặc 174–179. (Java: isMapColdforBot, dòng 623–625)
    public bool IsMapColdForBot(int mapId) => InRange(mapId, MapIds.COLD_FIRST, MapIds.COLD_LAST) || IsMapChristMasEvent(mapId);

    // Kiểm tra Thánh địa 156–159. (Java: isMapThanhDia, dòng 627–629)
    public bool IsMapThanhDia(int mapId) => InRange(mapId, MapIds.ICE_MAP_FIRST, MapIds.ICE_MAP_LAST);

    // Kiểm tra map Cereal theo object Map. (Java: isMapCereal, dòng 631–634)
    public bool IsMapCereal(Map map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return InRange(map.MapId, MapIds.CEREAL_FIRST, MapIds.CEREAL_LAST);
    }

    // Kiểm tra map vô hình theo object Map. (Java: isMapVohinh, dòng 636–639)
    public bool IsMapVohinh(Map map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return map.MapId == MapIds.INVISIBLE_CEREAL_MAP;
    }

    // Kiểm tra map Potaufeu 139–140. (Java: isMapPotaufeu, dòng 641–643)
    public bool IsMapPotaufeu(int mapId) => InRange(mapId, MapIds.POTAUFEU_FIRST, MapIds.POTAUFEU_LAST);

    // Kiểm tra map tân thủ 0–38. (Java: isMapTanThu, dòng 645–647)
    public bool IsMapTanThu(int mapId) => InRange(mapId, MapIds.BEGINNER_FIRST, MapIds.BEGINNER_LAST);

    // Kiểm tra map Ngũ Hành Sơn 122–124. (Java: isMapNguHanhSon, dòng 649–651)
    public bool IsMapNguHanhSon(int mapId) => InRange(mapId, MapIds.FIVE_ELEMENTS_FIRST, MapIds.FIVE_ELEMENTS_LAST);

    // Kiểm tra lãnh địa bang hội 156–159. (Java: islanhdiabanghoi, dòng 653–655)
    public bool IsClanTerritoryMap(int mapId) => InRange(mapId, MapIds.ICE_MAP_FIRST, MapIds.ICE_MAP_LAST);

    // Kiểm tra map giải cứu Mị Nương: 185–188 hoặc 207–209. (Java: isMapGiaiCuuMiNuong, dòng 657–659)
    public bool IsMapGiaiCuuMiNuong(int mapId) => InRange(mapId, MapIds.RESCUE_MI_NUONG_FIRST, MapIds.RESCUE_MI_NUONG_FIRST_LAST)
        || InRange(mapId, MapIds.RESCUE_MI_NUONG_SECOND, MapIds.RESCUE_MI_NUONG_LAST);

    // Kiểm tra map tương lai, gồm hai dải và 102/103. (Java: isMapTuongLai, dòng 661–663)
    public bool IsMapTuongLai(int mapId) => InRange(mapId, MapIds.FUTURE_FIRST, MapIds.FUTURE_EARLY_LAST)
        || InRange(mapId, MapIds.FUTURE_LATE_FIRST, MapIds.FUTURE_LATE_LAST) || InRange(mapId, MapIds.FUTURE_SINGLE_FIRST, MapIds.FUTURE_SINGLE_LAST);

    // Kiểm tra dải map 5.000 năm trước. (Java: isMap5000NamTruoc, dòng 665–667)
    public bool IsMap5000NamTruoc(int mapId) => InRange(mapId, MapIds.FIVE_THOUSAND_YEARS_AGO_FIRST, MapIds.FIVE_THOUSAND_YEARS_AGO_LAST);

    // Kiểm tra bốn map chiến tranh rời. (Java: isMapWar, dòng 669–671)
    public bool IsMapWar(int mapId) => mapId is MapIds.WAR_MAP_FIRST or MapIds.WAR_MAP_SECOND or MapIds.WAR_MAP_THIRD or MapIds.WAR_MAP_LAST;

    // Tra mob template như Manager, id ngoài catalog trả null. (Java: Manager.getMobTemplateByTemp)
    public MobTemplate? GetMobTemplate(int mobTempId) => mobTempId >= 0 && mobTempId < _catalog.MobCount ? _catalog.GetMob(mobTempId) : null;

    // Trả số zone cố định theo loại map. (Java: Map.initZone, dòng 114–145)
    public int? GetFixedZoneCount(sbyte mapType) => mapType switch
    {
        ConstMap.MAP_BLACK_BALL_WAR => MapIds.BLACK_BALL_WAR_ZONE_COUNT,
        ConstMap.MAP_MAJIN_BUU_14H => MapIds.MAJIN_BUU_FOURTEEN_HOUR_ZONE_COUNT,
        ConstMap.MAP_MAJIN_BUU_12H => MapIds.MAJIN_BUU_TWELVE_HOUR_ZONE_COUNT,
        ConstMap.MAP_RED_RIBBON_HQ or ConstMap.MAP_TREASURE_UNDER_SEA or ConstMap.MAP_SNAKE_WAY or ConstMap.MAP_DESTRON_GAS => MapIds.CLAN_DUNGEON_ZONE_COUNT,
        ConstMap.MAP_CLAN_BOSS => MapIds.CLAN_BOSS_ZONE_COUNT,
        _ => null,
    };

    // Trả true khi id nằm trong dải đóng Java.
    private static bool InRange(int value, int first, int last) => value >= first && value <= last;

    // Áp dụng nhánh fallback khi player chưa ở zone hoặc chưa có instance dungeon. (Java: getMapCanJoin, dòng 56–89)
    private Zone? GetDungeonOrFallback(Player player, int mapId, int fallbackMapId)
    {
        if (player.Zone is not null && _dungeons.HasDungeonInstance(player, mapId)) return null;
        Zone? fallback = GetZone(fallbackMapId);
        SetFallbackPosition(player, fallback);
        return fallback;
    }

    // Giữ luật Java: đích đã sạch thì cho vào, còn map hiện tại có quái/boss sống thì chặn. (Java: getMapCanJoin, dòng 91–245)
    private Zone? ResolveDungeonZone(Player player, int mapId)
    {
        Zone? destination = _dungeons.GetDungeonZone(player, mapId);
        if (destination is null || player.Zone is null || !IsSameDungeonGroup(player.Zone.Map.MapId, mapId)) return destination;
        if (AllCharactersDead(destination)) return destination;
        return AllCharactersDead(player.Zone) ? destination : null;
    }

    // Java chỉ áp dụng block quét sống khi map hiện tại và đích cùng nhóm dungeon. (Java: isMapDoanhTrai/isMapBanDoKhoBau/isMapKhiGasHuyDiet/isMapConDuongRanDoc)
    private bool IsSameDungeonGroup(int currentMapId, int destinationMapId) => (IsBarracksMap(currentMapId) && IsBarracksMap(destinationMapId))
        || (IsTreasureUnderSeaMap(currentMapId) && IsTreasureUnderSeaMap(destinationMapId)) || (IsDestronGasMap(currentMapId) && IsDestronGasMap(destinationMapId))
        || (IsMapConDuongRanDoc(currentMapId) && IsMapConDuongRanDoc(destinationMapId));

    // Quét quái rồi boss đúng thứ tự Java. (Java: getMapCanJoin, dòng 95–125)
    private static bool AllCharactersDead(Zone zone)
    {
        foreach (var mob in zone.Mobs) if (!mob.IsDie()) return false;
        foreach (Player boss in zone.Bosses) if (!boss.IsDead()) return false;
        return true;
    }

    // Đặt vị trí fallback random X và Y nền. (Java: getMapCanJoin, dòng 57–60)
    private void SetFallbackPosition(Player player, Zone? zone)
    {
        if (zone is null) return;
        player.Location.X = _random.NextInt(100, zone.Map.MapWidth - 100);
        player.Location.Y = zone.Map.YPhysicInTop(player.Location.X, 100);
    }

    // Lấy zone index với ngữ nghĩa null khi map/index sai. (Java: getZoneByMapIDAndZoneID, dòng 297–308)
    private Zone? GetZoneByMapIdAndZoneId(int mapId, int zoneId) => GetMapById(mapId)?.GetZoneByIndex(zoneId);

    // Fail-fast rõ map/zone thiếu; map 205 thiếu dump vì thế không bị nuốt im lặng. (Java: getMapBlackBall/getMapMaBu dùng .zones.get(0))
    private Zone GetRequiredZoneZero(int mapId) => GetMapById(mapId)?.GetZoneByIndex(0)
        ?? throw new InvalidOperationException($"Map {mapId} chưa được nạp hoặc không có zone 0; không thể dựng danh sách chuyển map.");

    // Bỏ zone trùng map và map hiện tại. (Java: addListMapCapsule, dòng 413–422)
    private static void AddCapsuleZone(Player player, List<Zone> zones, Zone? zone)
    {
        if (zone is null || player.Zone is null || player.Zone.Map.MapId == zone.Map.MapId) return;
        foreach (Zone existing in zones) if (existing.Map.MapId == zone.Map.MapId) return;
        zones.Add(zone);
    }
}
