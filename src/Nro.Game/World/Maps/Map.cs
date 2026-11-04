using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;
using Nro.Core.Templates;

namespace Nro.Game.World.Maps;

// Mô tả tĩnh bản đồ: ô nền, cửa chuyển, hành tinh và các zone được tạo từ nó.
// Nguồn Java: nro/map/Map.java (PARTIAL: NPC, initNpc và getNpc chưa chuyển vì Npc thuộc track M Wave 4; vị trí NPC vẫn nằm trong MapTemplate).
// Khác Java có chủ đích: Map không implements Runnable mà MapTickLoop điều khiển zone theo D1; đăng ký phó bản, số zone cố định và tạo boss đi qua IMapWorld/IMapRules.
public class Map
{
    // Cờ ô trống (Java: T_EMPTY).
    public const sbyte T_EMPTY = 0;

    // Cờ ô mặt đất phía trên (Java: T_TOP).
    public const sbyte T_TOP = 2;
    private const int SIZE = 24;

    private static readonly Dictionary<int, (int ItemId, int X, int Y)> InitialItems = new()
    {
        [21] = (74, 633, 315),
        [22] = (74, 56, 315),
        [23] = (74, 633, 320),
        [42] = (78, 70, 288),
        [43] = (78, 70, 264),
        [44] = (78, 70, 288),
        [85] = (372, 0, 0),
        [86] = (373, 0, 0),
        [87] = (374, 0, 0),
        [88] = (375, 0, 0),
        [89] = (376, 0, 0),
        [90] = (377, 0, 0),
        [91] = (378, 0, 0),
    };

    // Bảng map id sang BossID cho initBoss (Java: QuanLiBoss/BossID.java).
    private static readonly Dictionary<int, short> BossOfMap = new()
    {
        [111] = -89, // TAU_PAY_PAY_DONG_NAM_KARIN
        [114] = -52, // DRABURA
        [115] = -53, // BUI_BUI
        [117] = -57, // BUI_BUI_2
        [118] = -54, // YA_CON
        [119] = -56, // DRABURA_2
        [120] = -55, // MABU_12H
        [127] = -61, // MABU
        [128] = -62, // SUPERBU
        [131] = -72, // TAN_BINH_5
        [132] = -78, // CHIEN_BINH_5
        [133] = -84, // DOI_TRUONG_5
    };

    // Tạo mô tả map, nạp dữ liệu ô, zone, vật phẩm khởi tạo và bẫy.
    public Map(
        MapRuntime runtime,
        int mapId,
        string mapName,
        sbyte planetId,
        sbyte tileId,
        sbyte bgId,
        sbyte bgType,
        sbyte type,
        int[][] tileMap,
        int[] tileTop,
        int zones,
        int maxPlayer,
        List<WayPoint> wayPoints,
        List<EffectMap> effMap,
        sbyte genderType)
    {
        Runtime = runtime;
        MapId = mapId;
        MapName = mapName;
        PlanetId = planetId;
        PlanetName = runtime.Rules.GetPlanetName(planetId);
        TileId = tileId;
        BgId = bgId;
        BgType = bgType;
        Type = type;
        TileMap = tileMap;
        TileTop = tileTop;
        WayPoints = wayPoints;
        EffMap = effMap;
        GenderType = genderType;
        if (tileMap != null)
        {
            MapHeight = tileMap.Length * SIZE;
            if (tileMap.Length > 0 && tileMap[0] != null)
            {
                MapWidth = tileMap[0].Length * SIZE;
            }
        }

        ReadTileMap(mapId);
        InitZone(zones, maxPlayer);
        InitItem();
        InitTrapMap();
    }

    // Ngữ cảnh dịch vụ và thời gian dùng chung của map.
    public MapRuntime Runtime { get; }

    // Id map.
    public int MapId { get; }

    // Tên map.
    public string MapName { get; }

    // Id hành tinh.
    public sbyte PlanetId { get; }

    // Tên hành tinh đã tra từ quy tắc.
    public string PlanetName { get; }

    // Id bộ ô nền.
    public sbyte TileId { get; }

    // Id ảnh nền.
    public sbyte BgId { get; }

    // Kiểu nền.
    public sbyte BgType { get; }

    // Loại map.
    public sbyte Type { get; }

    // Ma trận ô nền.
    public int[][] TileMap { get; }

    // Danh sách id ô được coi là mặt đất.
    public int[] TileTop { get; }

    // Chiều rộng map theo pixel.
    public int MapWidth { get; private set; }

    // Chiều cao map theo pixel.
    public int MapHeight { get; private set; }

    // Các zone của map theo thứ tự tạo.
    public SnapshotList<Zone> Zones { get; } = new();

    // Danh sách cửa chuyển map.
    public List<WayPoint> WayPoints { get; }

    // Rộng map theo số ô từ tệp tile.
    public int Tmw { get; private set; }

    // Cao map theo số ô từ tệp tile.
    public int Tmh { get; private set; }

    // Cao map theo pixel từ tệp tile.
    public int Pxh { get; private set; }

    // Rộng map theo pixel từ tệp tile.
    public int Pxw { get; private set; }

    // Cờ loại ô tại y / 24 * tmw + x / 24; ReadTileMap cấp phát, nơi nạp loại ô điền giá trị (Java: types).
    public int[]? Types { get; set; }

    // Id ô đã đọc từ tệp tile map.
    public int[]? Maps { get; private set; }

    // Hiệu ứng nền của map.
    public List<EffectMap> EffMap { get; }

    // Kiểu giới tính được phép vào map.
    public sbyte GenderType { get; }

    private void InitZone(int nZone, int maxPlayer)
    {
        if (Type == ConstMap.MAP_OFFLINE || Type == ConstMap.MAP_CLAN)
        {
            nZone = 1;
        }
        else
        {
            nZone = Runtime.Rules.GetFixedZoneCount(Type) ?? nZone;
        }

        bool registered = Type == ConstMap.MAP_BLACK_BALL_WAR || Type == ConstMap.MAP_MAJIN_BUU_14H || Type == ConstMap.MAP_RED_RIBBON_HQ
            || Type == ConstMap.MAP_TREASURE_UNDER_SEA || Type == ConstMap.MAP_SNAKE_WAY || Type == ConstMap.MAP_DESTRON_GAS
            || Type == ConstMap.MAP_WEST_KARIN || Type == ConstMap.MAP_CLAN_BOSS;
        for (int i = 0; i < nZone; i++)
        {
            Zone zone = new(this, i, maxPlayer);
            Zones.Add(zone);
            if (registered)
            {
                Runtime.World.RegisterZone(this, i, zone);
            }
        }
    }

    // Tạo quái mẫu trong mọi zone; đại boss dùng lớp riêng (Java: initMob với các mảng).
    public void InitMob(sbyte[] mobTemp, sbyte[] mobLevel, int[] mobHp, short[] mobX, short[] mobY)
    {
        for (int i = 0; i < mobTemp.Length; i++)
        {
            int mobTempId = mobTemp[i];
            MobTemplate? temp = Runtime.Rules.GetMobTemplate(mobTempId);
            if (temp != null)
            {
                Mob mob = new(Runtime)
                {
                    Id = i,
                    TempId = mobTemp[i],
                    Level = mobLevel[i],
                };
                mob.Point.SetHpFull(mobHp[i]);
                mob.Location.X = mobX[i];
                mob.Location.Y = mobY[i];
                mob.Point.Sethp(mob.Point.GetHpFull());
                mob.DamagePercent = temp.PercentDame;
                mob.PotentialPercent = temp.PercentPotential;
                mob.Percent_gold = temp.Percent_gold;
                mob.Type = temp.Type;
                mob.SetPotential();
                foreach (Zone zone in Zones)
                {
                    Mob mobZone = mob.TempId switch
                    {
                        ConstMob.HIRUDEGARN => new Hirudegarn(mob),
                        ConstMob.KING_OCTOPUS => new KingOctopus(mob),
                        ConstMob.GUARD_ROBOT => new GuardRobot(mob),
                        ConstMob.BEAR_THIEF => new BearThief(mob),
                        ConstMob.NINE_TUSKED_ELEPHANT => new NineTuskedElephant(mob),
                        ConstMob.NINE_SPURRED_ROOSTER => new NineSpurredRooster(mob),
                        ConstMob.NINE_MANED_HORSE => new NineManedHorse(mob),
                        ConstMob.PIANO => new Piano(mob),
                        ConstMob.KONG => new Kong(mob),
                        ConstMob.GODZILLA => new Godzilla(mob),
                        _ => new Mob(mob),
                    };
                    mobZone.Zone = zone;
                    zone.Mobs.Add(mobZone);
                }
            }
        }
    }

    // Sao chép danh sách quái đã cho vào mọi zone (Java: initMob(List<Mob>)).
    public void InitMob(List<Mob> mobs)
    {
        foreach (Zone zone in Zones)
        {
            foreach (Mob m in mobs)
            {
                Mob mob = new(m)
                {
                    Zone = zone,
                };
                zone.Mobs.Add(mob);
            }
        }
    }

    private void InitTrapMap()
    {
        foreach (Zone zone in Zones)
        {
            if (MapId == 135)
            {
                zone.TrapMaps.Add(new TrapMap { X = 260, Y = 960, W = 740, H = 72, EffectId = 49 });
            }
        }
    }

    private void InitItem()
    {
        if (InitialItems.TryGetValue(MapId, out (int ItemId, int X, int Y) item))
        {
            foreach (Zone zone in Zones)
            {
                _ = new ItemMap(zone, item.ItemId, 1, item.X, item.Y, -1);
            }
        }
    }

    // Tạo boss của map nếu có trong mọi zone (Java: initBoss).
    public void InitBoss()
    {
        if (BossOfMap.TryGetValue(MapId, out short bossId))
        {
            foreach (Zone zone in Zones)
            {
                Runtime.World.CreateBossInZone(bossId, zone);
            }
        }
    }

    // Trả map Ma Bư tiếp theo hoặc -1 (Java: mapIdNextMabu).
    public short MapIdNextMabu(short mapId) => mapId switch
    {
        114 => 115,
        115 => 117,
        117 => 118,
        118 => 119,
        119 => 120,
        _ => -1,
    };

    // ------------------------------------------------------------------------------------------------------------------------------------------------------------
    // Tiện ích vật lý
    // ------------------------------------------------------------------------------------------------------------------------------------------------------------

    // Trả Y mặt đất tại hoặc dưới (x, y), 0 khi không có và chính y khi ngoài map (Java: yPhysicInTop).
    public int YPhysicInTop(int x, int y)
    {
        try
        {
            int rX = x / SIZE;
            int rY = 0;
            if (IsTileTop(TileMap[y / SIZE][rX]))
            {
                return y;
            }

            for (int i = y / SIZE; i < TileMap.Length; i++)
            {
                if (IsTileTop(TileMap[i][rX]))
                {
                    rY = i * SIZE;
                    break;
                }
            }

            return rY;
        }
        catch (Exception e) when (e is IndexOutOfRangeException or NullReferenceException)
        {
            return y;
        }
    }

    // Luôn trả y vì Java không dịch chuyển giá trị này (Java: yPhysicInTopBot).
    public int YPhysicInTopBot(int x, int y) => y;

    // Tìm Y ô đất đầu tiên phía dưới (cx, cy), quét tối đa 30 ô (Java: LastY).
    public int LastY(int cx, int cy)
    {
        int num = 0;
        int ySd = 0;
        int xSd = cx;
        if (TileTypeAt(cx, cy, 2))
        {
            return cy;
        }

        while (num < 30)
        {
            num++;
            ySd += 24;
            if (TileTypeAt(xSd, ySd, 2))
            {
                if (ySd % 24 != 0)
                {
                    ySd -= ySd % 24;
                }

                break;
            }
        }

        return ySd;
    }

    // Kiểm tra loại ô; trả false khi vị trí ngoài bảng loại (Java: tileTypeAt).
    public bool TileTypeAt(int x, int y, int type)
    {
        try
        {
            return (Types![(y / 24 * Tmw) + (x / 24)] & type) == type;
        }
        catch (Exception e) when (e is IndexOutOfRangeException or NullReferenceException)
        {
            return false;
        }
    }

    // Trả {x, y} khi di chuyển; y nhánh thường là hàng ô ysd, không phải pixel, giữ nguyên Java (Java: moveXY).
    public int[] MoveXY(Player player)
    {
        int px = Runtime.Players.GetX(player);
        int py = Runtime.Players.GetY(player);
        int xsd = px / 24;
        int ysd = py / 24;
        int p = MapId == 103 ? 4 : 3;
        if (TileMap[ysd][xsd] != 0)
        {
            if (TileMap[ysd - p][xsd] != 0)
            {
                if (TileMap[LastY(px, py - (p * 24)) / 24][xsd] != 0)
                {
                    return [player.XSend, player.YSend];
                }

                return [player.XSend, LastY(px, 120)];
            }

            return [px, ysd];
        }

        if (LastY(px, py) >= Pxh - 24)
        {
            return [player.XSend, player.YSend];
        }

        return [px, ysd];
    }

    private bool IsTileTop(int tile)
    {
        for (int i = 0; i < TileTop.Length; i++)
        {
            if (TileTop[i] == tile)
            {
                return true;
            }
        }

        return false;
    }

    // Đọc map/tile_map_data/mapId: bỏ byte đầu, rồi rộng, cao và một byte có dấu mỗi ô; tệp thiếu/cụt bị bỏ qua như IOException Java, phần đã đọc giữ lại và Types vẫn null (Java: readTileMap).
    public void ReadTileMap(int mapId)
    {
        byte[]? data = FileIO.ReadFile(Path.Combine(Runtime.DataRoot, "map", "tile_map_data", mapId.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        if (data == null || data.Length < 3)
        {
            return;
        }

        Tmw = (sbyte)data[1];
        Tmh = (sbyte)data[2];
        Pxw = Tmw * SIZE;
        Pxh = Tmh * SIZE;
        Maps = new int[Tmw * Tmh];
        int pos = 3;
        for (int j = 0; j < Maps.Length; j++)
        {
            if (pos >= data.Length)
            {
                return;
            }

            Maps[j] = (sbyte)data[pos++];
        }

        Types = new int[Maps.Length];
    }

    // Tra zone theo chỉ số; trả null nếu ngoài phạm vi.
    public Zone? GetZoneByIndex(int zoneIndex)
    {
        if (zoneIndex >= 0 && zoneIndex < Zones.Count)
        {
            return Zones[zoneIndex];
        }

        return null;
    }
}
