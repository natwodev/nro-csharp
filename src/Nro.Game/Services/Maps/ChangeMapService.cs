using Nro.Core.Consts;
using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Game.Services.Maps.Ports;
using Nro.Game.World.Maps;
using Nro.Network.Packets.Maps;

namespace Nro.Game.Services.Maps;

// Điều phối đổi zone/map; không singleton và mọi gói đi qua IChangeMapPacketGateway.
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 66–486).
public sealed class ChangeMapService
{
    // Kiểu đi map tự động bằng tàu (Java: AUTO_SPACE_SHIP).
    public const sbyte AutoSpaceShip = -1;

    // Kiểu đi map không dùng tàu (Java: NON_SPACE_SHIP).
    public const sbyte NonSpaceShip = 0;

    // Kiểu tàu mặc định (Java: DEFAULT_SPACE_SHIP).
    public const sbyte DefaultSpaceShip = 1;

    // Kiểu dịch chuyển Yardrat (Java: TELEPORT_YARDRAT).
    public const sbyte TeleportYardrat = 2;

    // Kiểu tàu tennis (Java: TENNIS_SPACE_SHIP).
    public const sbyte TennisSpaceShip = 3;

    // Cổng map/packet ngoài lõi (Java: các singleton MapService, Service, EffectSkillService).
    private readonly IChangeMapGateway _gateway;

    // Đồng hồ epoch tiêm được (Java: System.currentTimeMillis).
    private readonly IPlayerClock _clock;

    // Nguồn ngẫu nhiên tiêm được (Java: Util.nextInt).
    private readonly IPlayerRandom _random;

    // Ghép các cổng không trạng thái vào service scoped/transient, không dùng gI singleton Java.
    public ChangeMapService(IChangeMapGateway gateway, IPlayerClock clock, IPlayerRandom random)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    // Mở tab đích chuyển map -91 và làm mới cache đích đúng từng type Java (Java: openChangeMapTab, dòng 60–100).
    public void OpenChangeMapTab(Player? player)
    {
        if (player is null)
        {
            return;
        }

        IReadOnlyList<Zone>? zones = player.IDMark.MapChangeType switch
        {
            ConstMap.CHANGE_CAPSULE => player.MapCapsule = [.. _gateway.GetCapsuleZones(player)],
            ConstMap.CHANGE_BLACK_BALL => player.MapBlackBall ??= [.. _gateway.GetBlackBallZones()],
            ConstMap.CHANGE_MAP_MAJIN_BUU => player.MapMaBu ??= [.. _gateway.GetMajinBuuZones()],
            _ => null,
        };
        if (zones is null)
        {
            return;
        }

        var destinations = new List<MapTransportDestination>(zones.Count);
        for (int i = 0; i < zones.Count; i++)
        {
            Zone zone = zones[i];
            string mapName = player.IDMark.MapChangeType == ConstMap.CHANGE_CAPSULE && i == 0 && player.MapBeforeCapsule is not null
                ? "Về chỗ cũ: " + zone.Map.MapName
                : zone.Map.MapName is "Nhà Broly" or "Nhà Gôhan" or "Nhà Moori" ? "Về nhà" : zone.Map.MapName;
            destinations.Add(new MapTransportDestination(mapName, zone.Map.PlanetName));
        }

        // Java thiếu break sau CHANGE_CAPSULE, nên -91 có section Capsule rồi nối thêm section Black Ball.
        IReadOnlyList<IReadOnlyList<MapTransportDestination>> sections = player.IDMark.MapChangeType == ConstMap.CHANGE_CAPSULE
            ? [destinations, BuildDestinations(player.MapBlackBall ??= [.. _gateway.GetBlackBallZones()])]
            : [destinations];
        _gateway.SendMapTransport(player, new MapTransportPacket(sections));
    }

    // Mở UI chọn zone; payload cmd 29 chưa thuộc I-03 nên gateway chỉ đảm trách phần client (Java: openZoneUI, dòng 115–157).
    public void OpenZoneUi(Player? player)
    {
        if (player?.Zone is null)
        {
            if (player is not null)
            {
                _gateway.SendTutorial(player, "Không thể đổi khu vực trong map này");
            }

            return;
        }

        int mapId = player.Zone.Map.MapId;
        if (!player.IsModerator() && (_gateway.IsOfflineMap(mapId) || _gateway.IsDungeonMap(mapId)))
        {
            _gateway.SendTutorial(player, "Không thể đổi khu vực trong map này");
            return;
        }

        _gateway.SendZoneList(player, player.Zone.Map.Zones);
    }

    // Đổi zone với mọi nhánh từ chối/throttle Java; zone hợp lệ gọi ChangeMap nên thứ tự rời/vào không bị đảo (Java: changeZone, dòng 165–207).
    public void ChangeZone(Player? player, int zoneId)
    {
        if (player?.Zone is null)
        {
            if (player is not null)
            {
                _gateway.SendTutorial(player, "Không thể đến khu vực này");
            }

            return;
        }

        int mapId = player.Zone.Map.MapId;
        if (!player.IsModerator() && !player.IsBoss && (_gateway.IsOfflineMap(mapId) || _gateway.IsDungeonMap(mapId)
            || _gateway.IsMajinBuuMap(mapId) || _gateway.IsMajinBuuTwoHourMap(mapId)))
        {
            _gateway.SendTutorial(player, "Không thể đến khu vực này");
            return;
        }

        if (!player.IsModerator() && !player.IsBoss && !CanDoWithTime(player.IDMark.LastZoneChangeTime, 5_000))
        {
            _gateway.SendTutorial(player, "Chưa thể chuyển khu vực lúc này vui lòng chờ " + ((10_000 - (_clock.NowMilliseconds - player.IDMark.LastZoneChangeTime)) / 1000) + " nữa");
            return;
        }

        player.IDMark.LastZoneChangeTime = _clock.NowMilliseconds;
        Map map = player.Zone.Map;
        if (zoneId < 0 || zoneId >= map.Zones.Count)
        {
            _gateway.SendNotification(player, "Không thể thực hiện");
            return;
        }

        Zone? destination = map.GetZoneByIndex(zoneId);
        if (destination is not null && destination.GetNumOfPlayers() >= destination.MaxPlayer && !player.IsModerator() && !player.IsBoss)
        {
            _gateway.SendTutorial(player, "Khu vực này đã đầy");
            return;
        }

        if (destination is not null)
        {
            ChangeMap(player, destination, -1, -1, player.Location.X, player.Location.Y, NonSpaceShip);
        }
    }

    // Đổi map theo tàu; hồi sinh/đầy HP-MP chạy trước ChangeMap đúng Java (Java: changeMapBySpaceShip, dòng 217–249).
    public void ChangeMapBySpaceShip(Player? player, int mapId, int zoneId, int x)
    {
        if (player is null || player.IsBoss)
        {
            return;
        }

        ChangeMap(player, null, mapId, zoneId, x, 5, AutoSpaceShip);
        RestoreAfterSpaceShip(player);
    }

    // Đổi map theo tàu đến zone đã biết; khác overload Java: hồi phục xảy ra trước ChangeMap (Java: changeMapBySpaceShip, dòng 235–249).
    public void ChangeMapBySpaceShip(Player? player, Zone? destination, int x)
    {
        if (player is null || destination is null)
        {
            return;
        }

        RestoreAfterSpaceShip(player);
        ChangeMap(player, destination, -1, -1, x, 5, AutoSpaceShip);
    }

    // Đổi map đứng trên nền, chọn X ngẫu nhiên khi Java nhận -1 (Java: changeMapInYard, dòng 259–280).
    public void ChangeMapInYard(Player? player, int mapId, int zoneId, int x)
    {
        if (player is null)
        {
            return;
        }

        Zone? destination = _gateway.GetJoinableZone(player, mapId, zoneId);
        if (destination is null)
        {
            return;
        }

        int destinationX = x != -1 ? x : destination.Map.MapWidth - 100 < 100
            ? _random.NextInt(100, 700)
            : _random.NextInt(100, destination.Map.MapWidth - 100);
        ChangeMap(player, destination, -1, -1, destinationX, destination.Map.YPhysicInTop(destinationX, 100), NonSpaceShip);
    }

    // Đổi map đứng trên nền đến zone đã biết (Java: changeMapInYard, dòng 278–280).
    public void ChangeMapInYard(Player? player, Zone? destination, int x)
    {
        if (player is not null && destination is not null)
        {
            ChangeMap(player, destination, -1, -1, x, destination.Map.YPhysicInTop(x, 100), NonSpaceShip);
        }
    }

    // Overload đổi map không tàu (Java: changeMap, dòng 291–316).
    public void ChangeMap(Player? player, int mapId, int zoneId, int x, int y)
    {
        if (player is not null)
        {
            ChangeMap(player, null, mapId, zoneId, x, y, NonSpaceShip);
        }
    }

    // Overload đổi map không tàu với zone đã biết (Java: changeMap, dòng 303–305).
    public void ChangeMap(Player? player, Zone? destination, int x, int y)
    {
        if (player is not null)
        {
            ChangeMap(player, destination, -1, -1, x, y, NonSpaceShip);
        }
    }

    // Dịch chuyển Yardrat, luật đích được tách cổng vì MapService chưa port (Java: changeMapYardrat, dòng 315–317).
    public void ChangeMapYardrat(Player? player, Zone? destination, int x, int y)
    {
        if (player is not null)
        {
            ChangeMap(player, destination, -1, -1, x, y, TeleportYardrat);
        }
    }

    // Lõi đổi map: giữ nguyên thứ tự Java cancel trade → Tranh Ngọc → resolve/check → -65 → rời zone/vào zone → clear → -24 (Java: changeMap, dòng 486–680).
    private void ChangeMap(Player player, Zone? destination, int mapId, int zoneId, int x, int y, sbyte transportType)
    {
        if (player.IdNRNM != -1 && !CanDoWithTime(player.LastTimePickNRNM, 30_000))
        {
            ResetPoint(player);
            _gateway.SendNotification(player, "Không thể chuyển map quá nhanh khi đeo Ngọc Rồng Namếc");
            return;
        }

        if (player.IdNRNM != -1 && destination is not null)
        {
            _gateway.UpdateNamekBallLocation(player, destination);
            player.LastTimePickNRNM = _clock.NowMilliseconds;
        }

        if (player.IdNRNM != -1 && destination is not null && !_gateway.IsNamekBallMap(destination.Map.MapId))
        {
            _gateway.DropNamekBall(player);
        }

        _gateway.CancelTrade(player);
        destination = ApplyNamekWarMapChange(player, destination);
        destination ??= mapId != -1 ? _gateway.GetJoinableZone(player, mapId, zoneId) : null;
        if (transportType == TeleportYardrat)
        {
            destination = _gateway.CheckYardratDestination(player, destination);
        }

        destination = _gateway.CheckMapCanJoin(player, destination);
        if (destination is null || player.Zone is null)
        {
            ResetPoint(player);
            _gateway.SendNotification(player, "Bạn chưa thể đến khu vực này");
            return;
        }

        bool currentMapIsCold = _gateway.IsColdMap(player.Zone.Map);
        bool nextMapIsCold = _gateway.IsColdMap(destination.Map);
        bool currentMapIsChristmas = _gateway.IsChristmasEventMap(player.Zone.Map.MapId);
        bool nextMapIsChristmas = _gateway.IsChristmasEventMap(destination.Map.MapId);
        bool currentMapIsCereal = _gateway.IsCerealMap(player.Zone.Map);
        bool nextMapIsCereal = _gateway.IsCerealMap(destination.Map);
        bool currentMapIs5000YearsAgo = _gateway.Is5000YearsAgoMap(player.Zone.Map.MapId);
        bool nextMapIs5000YearsAgo = _gateway.Is5000YearsAgoMap(destination.Map.MapId);
        bool currentMapIsFuture = _gateway.IsFutureMap(player.Zone.Map);
        bool nextMapIsFuture = _gateway.IsFutureMap(destination.Map);
        bool nextMapIsMajinBuu = _gateway.IsMajinBuuMap(destination.Map.MapId);
        bool sameZone = player.Zone.Map.MapId == destination.Map.MapId;

        if (transportType == AutoSpaceShip)
        {
            if (destination.Map.MapId == 0)
            {
                player.IDMark.SpaceShipId = NonSpaceShip;
                int spawnX = x != -1 ? x : destination.Map.MapWidth - 100 > 100 ? _random.NextInt(100, destination.Map.MapWidth - 100) : 100;
                int ground = destination.Map.YPhysicInTop(spawnX, 100);
                x = spawnX;
                y = ground <= 0 || ground > destination.Map.MapHeight ? 432 : ground;
            }
            else
            {
                sbyte ship = player.HaveTennisSpaceShip ? TennisSpaceShip : DefaultSpaceShip;
                _gateway.SendSpaceShipArrival(player, new SpaceShipArrivalPacket(unchecked((int)player.Id), ship), SpaceShipArrivalAudience.AllInMap);
                player.IDMark.SpaceShipId = ship;
            }
        }
        else
        {
            player.IDMark.SpaceShipId = transportType;
        }

        if (player.EffectSkill.IsCharging)
        {
            _gateway.StopCharge(player);
        }

        if (player.EffectSkill.IsBinding)
        {
            _gateway.RemoveBinding(player);
        }

        player.Location.X = x != -1 ? x : destination.Map.MapWidth - 100 > 100 ? _random.NextInt(100, destination.Map.MapWidth - 100) : 100;
        if (destination.Map.MapId == 111 && player.NPoint.Power >= 1_500_000L)
        {
            int safeX = player.Location.X >= player.Zone.Map.MapWidth - 900 ? player.Zone.Map.MapWidth - 100
                : player.Location.X <= 100 ? 100 : player.Location.X;
            _gateway.ResetPoint(player, safeX, player.Location.Y);
            _gateway.SendNotification(player, "Sức mạnh phải dưới 1,5 triệu mới vào được");
            return;
        }

        player.Location.Y = y;
        LeaveAndEnterZone(player, destination);
        _gateway.JoinPetMap(player);
        _gateway.ClearMap(player);
        if (!player.IsPl())
        {
            destination.LoadMeToAnother(player);
        }
        else
        {
            _gateway.SendMapInfo(player, destination);
            player.TimeChangeZone = _clock.NowMilliseconds;
        }

        player.IDMark.SpaceShipId = NonSpaceShip;
        if (player.IsPl() && nextMapIsMajinBuu)
        {
            if (destination.Map.MapId == 117)
            {
                _gateway.SendNotification(player, "Đây là không gian cao trọng lực, hãy cẩn thận");
            }

            if (!sameZone && destination.Map.MapId != 114)
            {
                _gateway.ClearMajinBuuFight(player);
                int minutes = destination.Map.MapId - 114;
                if (destination.Map.MapId > 116)
                {
                    minutes--;
                }

                _gateway.GrantMajinBuuEntryReward(player, minutes * 5);
            }
        }

        if (currentMapIsCold != nextMapIsCold && !player.IsBoss && !player.NPoint.IsColdImmune)
        {
            if (!currentMapIsCold && nextMapIsCold)
            {
                _gateway.SendNotification(player, "Bạn đã đến hành tinh Cold");
                _gateway.SendNotification(player, "Sức tấn công và HP của bạn bị giảm 50% vì quá lạnh");
            }
            else
            {
                _gateway.SendNotification(player, "Bạn đã rời hành tinh Cold");
                _gateway.SendNotification(player, "Sức tấn công và HP của bạn đã trở lại bình thường");
            }

            _gateway.SendPoint(player);
            _gateway.SendCharacterInfo(player);
        }

        if (currentMapIsChristmas != nextMapIsChristmas && !player.IsBoss && !player.NPoint.IsColdImmune)
        {
            if (!currentMapIsChristmas && nextMapIsChristmas)
            {
                _gateway.SendNotification(player, "Bạn đã đến map sự kiện");
                _gateway.SendNotification(player, "Sức tấn công và HP của bạn bị giảm 50% vì quá lạnh");
            }
            else
            {
                _gateway.SendNotification(player, "Bạn đã rời map sự kiện");
                _gateway.SendNotification(player, "Sức tấn công và HP của bạn đã trở lại bình thường");
            }

            _gateway.SendPoint(player);
            _gateway.SendCharacterInfo(player);
        }

        if (player.Gender == ConstPlayer.SAIYAN && currentMapIsCereal != nextMapIsCereal)
        {
            if (!currentMapIsCereal && nextMapIsCereal)
            {
                _gateway.SendNotification(player, "Bạn Đã Đến Hành Tinh Cereal");
                _gateway.SendNotification(player, "Vì Là Người Xayda Nên Bạn Bị Giảm 50% Sức Mạnh Từ Lời Nguyền Của Dân Tộc Cereal!");
            }
            else
            {
                _gateway.SendNotification(player, "Bạn Đã Rời Hành Tinh Cereal");
                _gateway.SendNotification(player, "Sức Mạnh Của Bạn Đã Trở Lại Bình Thường!");
            }

            _gateway.SendPoint(player);
            _gateway.SendCharacterInfo(player);
        }

        if (currentMapIsFuture != nextMapIsFuture && !player.IsBoss)
        {
            if (!currentMapIsFuture && nextMapIsFuture)
            {
                _gateway.SendNotification(player, "Bạn đã đến tương lai, khi chết ở đây bạn sẽ bị giảm sức mạnh và tiềm năng");
            }

            _gateway.SendPoint(player);
            _gateway.SendCharacterInfo(player);
        }

        if (currentMapIs5000YearsAgo != nextMapIs5000YearsAgo && !player.IsBoss)
        {
            if (!currentMapIs5000YearsAgo && nextMapIs5000YearsAgo)
            {
                _gateway.SendTutorial(player, "Bạn đã quay về Quá Khứ\bSức mạnh của bạn sẽ bị giảm 90%");
            }
            else
            {
                _gateway.SendNotification(player, "Bạn đã quay về hiện tại");
                _gateway.SendNotification(player, "Sức mạnh của bạn đã trở về bình thường!");
            }

            _gateway.SendPoint(player);
            _gateway.SendCharacterInfo(player);
        }

        if (destination.Map.MapId == 47 && _gateway.GetTaskId(player) == ConstTask.TASK_10_1)
        {
            _gateway.CallTauPayPay(player);
        }

        _gateway.CheckJoinSpecialMap(player);
        _gateway.CheckJoinMajinBuuMap(player);
        _gateway.SendNamekWarLiftUpdate(player);
    }

    // Ép người chơi vào zone Tranh Ngọc, ghi phe, khởi động zone lần đầu, đổi cờ và tạo phó bản (Java: changeMap dòng 505–518).
    private Zone? ApplyNamekWarMapChange(Player player, Zone? destination)
    {
        sbyte team = player.IDMark.GetNamekWarState();
        if (team == -1)
        {
            return destination;
        }

        Zone? namekWarZone = _gateway.GetNamekWarZone(team);
        if (namekWarZone is null)
        {
            return null;
        }

        if (team == 1)
        {
            namekWarZone.AddPlayersCadic(player);
        }
        else if (team == 2)
        {
            namekWarZone.AddplayersFide(player);
        }

        if (!namekWarZone.IsNamekWarZoneStarted)
        {
            namekWarZone.LastNamekWarStartTime = _clock.NowMilliseconds;
            namekWarZone.IsNamekWarZoneStarted = true;
        }

        _gateway.ChangeNamekWarFlag(player, team);
        _gateway.SendCreateNamekWarInstance(player);
        return namekWarZone;
    }

    // Đi qua waypoint thường và giữ nhánh map 45/46, khí gas và phó bản Java (Java: changeMapWaypoint, dòng 456–513).
    public void ChangeMapWaypoint(Player? player)
    {
        if (player?.Zone is null)
        {
            return;
        }

        Zone? destination = null;
        int x = player.Location.X;
        int y = player.Location.Y;
        if ((player.Zone.Map.MapId is 45 or 46) && x is >= 35 and <= 685 && y is >= 550 and <= 560)
        {
            x = player.Zone.Map.MapId == 45 ? 420 : 636;
            y = 150;
            destination = _gateway.GetJoinableZone(player, player.Zone.Map.MapId + 1, -1);
        }

        if (destination is null)
        {
            WayPoint? waypoint = _gateway.GetWaypointPlayerIn(player);
            if (waypoint is not null)
            {
                destination = _gateway.GetJoinableZone(player, waypoint.GoMap, -1);
                if (destination is not null)
                {
                    x = waypoint.GoX;
                    y = waypoint.GoY;
                }
            }
        }

        if (destination is null)
        {
            ResetPoint(player);
            _gateway.SendNotification(player, _gateway.IsDungeonMap(player.Zone.Map.MapId) ? "Chưa hạ hết đối thủ" : "Bạn chưa thể đến khu vực này");
            return;
        }

        if (_gateway.ShouldChangeDestronGasMap(player.Zone.Map.MapId, destination.Map.MapId))
        {
            _gateway.BeginDestronGasMapChange(player, destination, x, y);
            return;
        }

        ChangeMap(player, destination, -1, -1, x, y, NonSpaceShip);
    }

    // Rời zone cũ trước, sau đó gắn Zone và add vào zone mới; gọi load của Java xảy ra ở caller (Java: goToMap/exitMap).
    private static void LeaveAndEnterZone(Player player, Zone destination)
    {
        player.Zone?.RemovePlayer(player);
        player.Zone = destination;
        destination.AddPlayer(player);
    }

    // Tạo section -91 thường; label Capsule chỉ được xử lý riêng ở OpenChangeMapTab.
    private static IReadOnlyList<MapTransportDestination> BuildDestinations(IReadOnlyList<Zone> zones)
    {
        var destinations = new List<MapTransportDestination>(zones.Count);
        for (int i = 0; i < zones.Count; i++)
        {
            Zone zone = zones[i];
            destinations.Add(new MapTransportDestination(zone.Map.MapName, zone.Map.PlanetName));
        }

        return destinations;
    }

    // Khôi phục sau tàu có đúng hai nhánh chết/sống và tàu tennis Java.
    private void RestoreAfterSpaceShip(Player player)
    {
        if (player.IsDead())
        {
            _gateway.Revive(player, player.HaveTennisSpaceShip ? player.NPoint.HpMax : 1, player.HaveTennisSpaceShip ? player.NPoint.MpMax : 1);
        }
        else if (player.HaveTennisSpaceShip)
        {
            player.NPoint.SetFullHpMp();
            _gateway.SendInfoHpMp(player);
        }
    }

    // Java Util.canDoWithTime: last + delay < now; bằng đúng delay vẫn bị từ chối.
    private bool CanDoWithTime(long lastTime, long delayMilliseconds) => lastTime + delayMilliseconds < _clock.NowMilliseconds;

    // resetPoint Java kẹp X theo map hiện tại trước khi gửi cổng (Java: changeMap dòng 445–452).
    private void ResetPoint(Player player)
    {
        if (player.Zone is null)
        {
            return;
        }

        int x = player.Location.X >= player.Zone.Map.MapWidth - 60 ? player.Zone.Map.MapWidth - 60
            : player.Location.X <= 60 ? 60 : player.Location.X;
        _gateway.ResetPoint(player, x, player.Location.Y);
    }
}
