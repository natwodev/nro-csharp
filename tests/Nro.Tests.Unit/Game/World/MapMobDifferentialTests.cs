using System.Reflection;
using System.Text.Json;
using Nro.Game;

namespace Nro.Tests.Unit.Game.World;

/// <summary>
/// Compares the port with the REAL JDK 17 (<c>map-mob-differential.json</c>, produced by <c>MapMobDifferentialGenerator.java.txt</c> from method bodies copied verbatim from
/// Map.java, Mob.java, MobPoint.java and Util.java): ground search and tile types on 40 random maps (2,400 queries), the experience arithmetic (300 cases), the mob attack
/// roll (60 seeds x 3 draws, same <c>java.util.Random</c> sequence) and the Hirudegarn damage (80 cases).
/// </summary>
public class MapMobDifferentialTests
{
    private static readonly Lazy<JsonDocument> Corpus = new(() =>
        JsonDocument.Parse(File.ReadAllBytes(Path.Combine(TestPaths.ProjectDirectory("Nro.Tests.Unit"), "Game", "World", "map-mob-differential.json"))));

    private static JsonElement Section(string name) => Corpus.Value.RootElement.GetProperty(name);

    [Fact]
    public void GroundSearchAndTileTypes_MatchTheJdk()
    {
        string root = Path.Combine(Path.GetTempPath(), "nro-diff-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "map", "tile_map_data"));
        try
        {
            FakeGame g = new(dataRoot: root);
            int mapIndex = 0;
            int queries = 0;
            foreach (JsonElement m in Section("physics").EnumerateArray())
            {
                int w = m.GetProperty("w").GetInt32();
                int h = m.GetProperty("h").GetInt32();
                int[][] tiles = m.GetProperty("tileMap").EnumerateArray().Select(r => r.EnumerateArray().Select(v => v.GetInt32()).ToArray()).ToArray();
                int[] tileTop = m.GetProperty("tileTop").EnumerateArray().Select(v => v.GetInt32()).ToArray();
                int[] types = m.GetProperty("types").EnumerateArray().Select(v => v.GetInt32()).ToArray();
                int mapId = 1000 + mapIndex++;
                byte[] file = new byte[3 + (w * h)];
                file[1] = (byte)w;
                file[2] = (byte)h;
                File.WriteAllBytes(Path.Combine(root, "map", "tile_map_data", mapId.ToString(System.Globalization.CultureInfo.InvariantCulture)), file);
                Map map = new(g.Runtime, mapId, "m", 0, 0, 0, 0, 0, tiles, tileTop, 1, 5, [], [], 0);
                Assert.Equal(w, map.Tmw);
                map.Types = types;
                foreach (JsonElement q in m.GetProperty("queries").EnumerateArray())
                {
                    int x = q[0].GetInt32();
                    int y = q[1].GetInt32();
                    string at = $"map {mapIndex - 1} ({x},{y})";
                    Assert.True(q[2].GetInt32() == map.YPhysicInTop(x, y), "yPhysicInTop " + at);
                    Assert.True(q[3].GetInt32() == map.LastY(x, y), "LastY " + at);
                    Assert.True(q[4].GetInt32() == (map.TileTypeAt(x, y, 2) ? 1 : 0), "tileTypeAt 2 " + at);
                    Assert.True(q[5].GetInt32() == (map.TileTypeAt(x, y, 1) ? 1 : 0), "tileTypeAt 1 " + at);
                    queries++;
                }
            }

            Assert.Equal(40 * 60, queries);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [Fact]
    public void ExperienceFormula_MatchesTheJdk()
    {
        FieldInfo maxPotential = typeof(Mob).GetField("_maxPotential", BindingFlags.Instance | BindingFlags.NonPublic)!;
        int count = 0;
        foreach (JsonElement c in Section("tiemNang").EnumerateArray())
        {
            long hpFull = c[0].GetInt64();
            long maxTn = c[1].GetInt64();
            int levelPlayer = c[2].GetInt32();
            int mobLevel = c[3].GetInt32();
            double damage = c[4].GetDouble();
            long expected = c[5].GetInt64();
            FakeGame g = new() { CurrLevel = levelPlayer };
            Zone z = MapTestKit.BuildZone(g);
            Mob mob = MapTestKit.NewMob(z, 0, 25, hpFull);
            mob.Level = (sbyte)mobLevel;
            maxPotential.SetValue(mob, maxTn);
            Player p = MapTestKit.NewPlayerIn(g, z, 1);
            Assert.True(expected == mob.GetPotentialForPlayer(p, damage), $"case {count}: hp {hpFull} max {maxTn} lv {levelPlayer}/{mobLevel} damage {damage}");
            count++;
        }

        Assert.Equal(300, count);
    }

    [Fact]
    public void MobAttackRoll_MatchesTheJdkRandomSequence()
    {
        int count = 0;
        foreach (JsonElement c in Section("dameAttack").EnumerateArray())
        {
            long seed = c[0].GetInt64();
            FakeGame g = new(seed);
            Zone z = MapTestKit.BuildZone(g);
            Mob mob = new(g.Runtime) { Zone = z, DamagePercent = (sbyte)c[3].GetInt32(), Level = (sbyte)c[4].GetInt32() };
            mob.Point.SetHpFull(c[2].GetInt64());
            mob.Point.Damage = c[1].GetInt64();
            long[] actual = [mob.Point.GetDamageAttack(), mob.Point.GetDamageAttack(), mob.Point.GetDamageAttack()];
            long[] expected = [c[5].GetInt64(), c[6].GetInt64(), c[7].GetInt64()];
            Assert.True(expected.SequenceEqual(actual), $"seed {seed}: expected [{string.Join(",", expected)}] got [{string.Join(",", actual)}]");
            count++;
        }

        Assert.Equal(60, count);
    }

    [Fact]
    public void HirudegarnDamage_MatchesTheJdk()
    {
        int count = 0;
        foreach (JsonElement c in Section("hirudegarn").EnumerateArray())
        {
            long seed = c[0].GetInt64();
            long maxHp = c[1].GetInt64();
            long hp = c[2].GetInt64();
            long expected = c[3].GetInt64();
            FakeGame g = new(seed);
            Zone z = MapTestKit.BuildZone(g);

            // the prototype draws nothing; the Hirudegarn copy constructor draws once (Java: setTiemNang), exactly like the Java constructor the generator mimics
            Mob proto = new(g.Runtime) { Id = 0, TempId = Nro.Core.Consts.ConstMob.HIRUDEGARN, Zone = z };
            proto.Point.SetHpFull(maxHp);
            proto.Point.Hp = maxHp;
            Hirudegarn boss = new(proto);
            boss.Point.Hp = hp;
            boss.Injured(null, 0, false);
            Assert.True(expected == boss.Point.Hp, $"seed {seed}: max {maxHp} hp {hp}: expected {expected} got {boss.Point.Hp}");
            count++;
        }

        Assert.Equal(80, count);
    }
}
