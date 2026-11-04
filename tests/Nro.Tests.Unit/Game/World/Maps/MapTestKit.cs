using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;
using Nro.Game;

namespace Nro.Tests.Unit.Game.World.Maps;

/// <summary>Builders shared by the map, zone, mob and tick-loop tests.</summary>
internal static class MapTestKit
{
    /// <summary>A 10 row by 20 column tile map with a ground row (tile value 2, listed in tileTop) at row 6, i.e. y = 144.</summary>
    public static int[][] FlatTiles()
    {
        int[][] tiles = new int[10][];
        for (int r = 0; r < tiles.Length; r++)
        {
            tiles[r] = new int[20];
            if (r == 6)
            {
                Array.Fill(tiles[r], 2);
            }
        }

        return tiles;
    }

    public static Map BuildMap(FakeGame g, int mapId = 1, sbyte type = ConstMap.MAP_NORMAL, int zones = 1, int maxPlayer = 10, List<WayPoint>? wayPoints = null)
    {
        Map map = new(g.Runtime, mapId, "Map" + mapId, 1, 2, 3, 4, type, FlatTiles(), [2], zones, maxPlayer, wayPoints ?? [], [], 0);
        g.Maps[mapId] = map;
        return map;
    }

    public static Zone BuildZone(FakeGame g, int mapId = 1, sbyte type = ConstMap.MAP_NORMAL, int maxPlayer = 10) => BuildMap(g, mapId, type, 1, maxPlayer).Zones[0];

    /// <summary>A plain mob in <paramref name="zone"/>: template 25 (a normal mob above template 18), level 1, <paramref name="hp"/> hit points.</summary>
    public static Mob NewMob(Zone zone, int id = 0, int tempId = 25, long hp = 1000, int x = 100, int y = 144)
    {
        Mob mob = new(zone.Runtime)
        {
            Id = id,
            TempId = tempId,
            Level = 1,
            Zone = zone,
            DamagePercent = 5,
            PotentialPercent = 3,
        };
        mob.Point.SetHpFull(hp);
        mob.Point.Hp = hp;
        mob.Location.X = x;
        mob.Location.Y = y;
        mob.SetPotential();
        zone.Mobs.Add(mob);
        return mob;
    }

    /// <summary>A player standing in <paramref name="zone"/> (registered in it as well).</summary>
    public static Player NewPlayerIn(FakeGame g, Zone zone, long id, int x = 100, int y = 144)
    {
        Player p = g.NewPlayer(id, x, y, zone);
        zone.AddPlayer(p);
        return p;
    }
}
