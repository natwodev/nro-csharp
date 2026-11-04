using System.Globalization;
using System.Text.Json;
using Nro.Game;
using Xunit.Abstractions;

namespace Nro.Tests.Unit.Game.Effects;

/// <summary>
/// Characterization of the effect arithmetic against the REAL JDK 17: <c>effect-formulas.json</c> (next to this file) was produced by <c>EffectFormulaGenerator.java.txt</c>, which
/// holds the Java expressions copied from <c>EffectSkin.java</c>, <c>EffectSkillService.java</c>, <c>SkillUtil.java</c> and <c>Util.java</c> and runs them over grids that include the
/// edges (hp 1, a maximum above <c>int</c> and above 2^62, <c>long</c> overflow of <c>hp * 2</c>, <c>int</c> overflow of the level, both values of <c>Manager.readInt</c>).
/// Every test here drives the real C# effect code (not a copy of the formula) with the same inputs and compares the result with the Java value.
/// </summary>
public class EffectFormulaGoldenTests
{
    private const int MaxReported = 25;

    private static readonly Lazy<JsonDocument> Corpus = new(() =>
        JsonDocument.Parse(File.ReadAllBytes(Path.Combine(TestPaths.ProjectDirectory("Nro.Tests.Unit"), "Game", "Effects", "effect-formulas.json"))));

    private readonly ITestOutputHelper output;

    public EffectFormulaGoldenTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    private static long L(JsonElement cell) => long.Parse(cell.GetString()!, CultureInfo.InvariantCulture);

    private static int I(JsonElement cell) => int.Parse(cell.GetString()!, CultureInfo.InvariantCulture);

    private static bool B(JsonElement cell) => cell.GetString() == "1";

    private void Run(string table, Func<JsonElement[], string?> check, int minimumRows)
    {
        var failures = new List<string>();
        int rows = 0;
        foreach (JsonElement row in Corpus.Value.RootElement.GetProperty(table).EnumerateArray())
        {
            rows++;
            string? failure = check(row.EnumerateArray().ToArray());
            if (failure is not null)
            {
                failures.Add(failure);
            }
        }

        Assert.True(rows >= minimumRows, $"{table}: only {rows} rows");
        foreach (string failure in failures.Take(MaxReported))
        {
            output.WriteLine(failure);
        }

        Assert.True(failures.Count == 0, $"{table}: {failures.Count} of {rows} rows differ from Java; first: {failures.FirstOrDefault()}");
        output.WriteLine($"{table}: {rows} rows match the JDK");
    }

    private static Action<FakeWorld.Sim> Stat(Func<EffectNPointState, EffectNPointState> change) => s => s.NPoint = change(s.NPoint);

    [Fact]
    public void The_corpus_was_made_by_the_expected_jdk()
    {
        Assert.StartsWith("17.", Corpus.Value.RootElement.GetProperty("meta").GetProperty("jdk").GetString());
    }

    [Fact]
    public void Dirty_damage_matches_java()
    {
        Run("odo", c =>
        {
            long hpMax = L(c[0]);
            long hp = L(c[1]);
            int param = I(c[2]);
            bool readInt = B(c[3]);
            long expected = L(c[4]);

            var rig = new EffectRig(readInt);
            Player owner = rig.Add(1, 0, 0, Stat(n => n with { PercentDirtyHpLoss = param }));
            rig.Add(2, 10, 0, Stat(n => n with { Hp = hp, HpMax = hpMax }));
            owner.EffectSkin.LastTimeAddTimeTrainArmor = rig.Now;
            owner.EffectSkin.LastTimeSubTimeTrainArmor = rig.Now;
            rig.Random.Enqueue(0);
            owner.EffectSkin.Update();

            (long Victim, double Damage) hit = Assert.Single(rig.World.InjureCalls);
            return hit.Damage == (double)expected ? null : $"dirty hpMax={hpMax} hp={hp} param={param} readInt={readInt}: java {expected}, C# {hit.Damage:R}";
        }, 600);
    }

    [Fact]
    public void Xen_hut_amounts_match_java()
    {
        Run("xen", c =>
        {
            long mobFull = L(c[0]);
            long mobHp = L(c[1]);
            long plMax = L(c[2]);
            long plHp = L(c[3]);
            long plMp = L(c[4]);
            int param = I(c[5]);
            bool readInt = B(c[6]);
            long mobSub = L(c[7]);
            long subHp = L(c[8]);
            long subMp = L(c[9]);

            var rig = new EffectRig(readInt);
            Player owner = rig.Add(1, 0, 0, Stat(n => n with { Hp = 1, HpMax = 1000, Mp = 1, MpMax = 1000, AroundHpMpAbsorptionPercent = param }));
            rig.Add(2, 10, 0, Stat(n => n with { Hp = plHp, HpMax = plMax, Mp = plMp, MpMax = plMax }));
            var mob = new FakeMob { Hp = mobHp, HpFull = mobFull, X = 10, Y = 0 };
            rig.World.Mobs.Add(mob);
            owner.EffectSkin.LastTimeAddTimeTrainArmor = rig.Now;
            owner.EffectSkin.LastTimeSubTimeTrainArmor = rig.Now;
            owner.EffectSkin.Update();

            double seenMob = Assert.Single(mob.InjuredCalls).Damage;
            double seenHp = Assert.Single(rig.World.InjureCalls).Damage;
            long seenAddHp = Assert.Single(rig.World.AddHpAmounts);
            long seenAddMp = Assert.Single(rig.World.AddMpAmounts);
            bool ok = seenMob == (double)mobSub && seenHp == (double)subHp && seenAddHp == mobSub + subHp && seenAddMp == subMp;
            return ok ? null : $"xen mobFull={mobFull} mobHp={mobHp} plMax={plMax} plHp={plHp} plMp={plMp} param={param} readInt={readInt}: java mob={mobSub} hp={subHp} mp={subMp}, C# mob={seenMob:R} hp={seenHp:R} addHp={seenAddHp} addMp={seenAddMp}";
        }, 3000);
    }

    [Fact]
    public void Mafuba_bottle_damage_matches_java()
    {
        Run("mafuba", c =>
        {
            long ownerMax = L(c[0]);
            int point = I(c[1]);
            int typeBinh = I(c[2]);
            long hp = L(c[3]);
            bool readInt = B(c[4]);
            long expected = L(c[5]);

            var rig = new EffectRig(readInt);
            Player owner = rig.Add(1, 0, 0, s =>
            {
                s.Skills[26] = new EffectSkillRef(2600, point);
                s.NPoint = s.NPoint with { HpMax = ownerMax };
            });
            Player victim = rig.Add(2, 0, 0, Stat(n => n with { Hp = hp }));
            victim.EffectSkin.LastTimeAddTimeTrainArmor = rig.Now;
            victim.EffectSkin.LastTimeSubTimeTrainArmor = rig.Now;
            victim.EffectSkill.IsBottled = true;
            victim.EffectSkill.BottledType = typeBinh;
            victim.EffectSkill.MafubaUser = owner;
            victim.EffectSkin.Update();

            (long Victim, double Damage) hit = Assert.Single(rig.World.InjureCalls);
            return hit.Damage == (double)expected ? null : $"mafuba ownerMax={ownerMax} point={point} typeBinh={typeBinh} hp={hp} readInt={readInt}: java {expected}, C# {hit.Damage:R}";
        }, 700);
    }

    private static string? CheckMonkey(long point, int cadic, long hp, bool readInt, int expectedTime, int expectedLevel, long expectedHp, bool trungThu)
    {
        var rig = new EffectRig(readInt);
        Player p = rig.Add(1, 0, 0, s =>
        {
            s.Skills[13] = new EffectSkillRef(1300, (int)point);
            s.Selected = new EffectSkillRef(900, (int)point);
            s.Clothes = new EffectSetClothesState { Cadic = (sbyte)cadic };
            s.NPoint = s.NPoint with { Hp = hp };
        });
        if (trungThu)
        {
            rig.Env.Skills.SetMoonFestivalMonkey(p);
        }
        else
        {
            rig.Env.Skills.SetIsMonkey(p);
        }

        string setHp = rig.World.Calls.Single(x => x.StartsWith("SetHp:1:", StringComparison.Ordinal));
        long seenHp = long.Parse(setHp["SetHp:1:".Length..], CultureInfo.InvariantCulture);
        bool ok = p.EffectSkill.TimeMonkey == expectedTime && p.EffectSkill.LevelMonkey == expectedLevel && seenHp == expectedHp;
        return ok ? null : $"{(trungThu ? "trungthu" : "monkey")} point={point} cadic={cadic} hp={hp} readInt={readInt}: java time={expectedTime} level={expectedLevel} hp={expectedHp}, C# time={p.EffectSkill.TimeMonkey} level={p.EffectSkill.LevelMonkey} hp={seenHp}";
    }

    [Fact]
    public void Monkey_time_level_and_hp_match_java()
    {
        Run("monkey", c => CheckMonkey(I(c[0]), I(c[1]), L(c[2]), B(c[3]), I(c[4]), I(c[5]), L(c[6]), trungThu: false), 300);
    }

    [Fact]
    public void Mid_autumn_monkey_time_level_and_hp_match_java()
    {
        Run("trungthu", c => CheckMonkey(I(c[0]), I(c[1]), L(c[2]), B(c[3]), I(c[4]), I(c[5]), L(c[6]), trungThu: true), 300);
    }

    [Fact]
    public void Pirate_set_bonus_matches_java_for_one_to_nine_wearers()
    {
        Run("costume", c =>
        {
            int count = I(c[0]);
            if (count < 1)
            {
                return null;
            }

            int expected = I(c[1]);
            var rig = new EffectRig();
            var items = new List<FakeItem>();
            Player? owner = null;
            for (int i = 0; i < count; i++)
            {
                int template = 618 + i;
                FakeItem item = new() { TemplateId = template, OptionList = [new FakeOption(147, 0)] };
                items.Add(item);
                Player p = rig.Add(1 + i, i * 10, 0, s =>
                {
                    s.Clothes = new EffectSetClothesState { CostumePirate = template };
                    s.Body[5] = item;
                });
                owner ??= p;
            }

            owner!.EffectSkin.LastTimeAddTimeTrainArmor = rig.Now;
            owner.EffectSkin.LastTimeSubTimeTrainArmor = rig.Now;
            owner.EffectSkin.Update();

            int seen = items[0].OptionList[0]!.Param;
            return seen == expected ? null : $"pirate count={count}: java {expected}, C# {seen}";
        }, 10);
    }

    [Fact]
    public void Bulma_set_bonus_matches_java_for_one_to_three_wearers()
    {
        Run("costume", c =>
        {
            int count = I(c[0]);
            if (count < 1 || count > 3)
            {
                return null;
            }

            int expected = I(c[2]);
            var rig = new EffectRig();
            var items = new List<FakeItem>();
            Player? owner = null;
            for (int i = 0; i < count; i++)
            {
                int template = 1208 + i;
                FakeItem item = new() { TemplateId = template, OptionList = [new FakeOption(147, 0)] };
                items.Add(item);
                Player p = rig.Add(1 + i, i * 10, 0, s =>
                {
                    s.Clothes = new EffectSetClothesState { CostumeBulmaColoredHair = template };
                    s.Body[5] = item;
                });
                owner ??= p;
            }

            owner!.EffectSkin.LastTimeAddTimeTrainArmor = rig.Now;
            owner.EffectSkin.LastTimeSubTimeTrainArmor = rig.Now;
            owner.EffectSkin.Update();

            int seen = items[0].OptionList[0]!.Param;
            return seen == expected ? null : $"bunma count={count}: java {expected}, C# {seen}";
        }, 10);
    }
}
