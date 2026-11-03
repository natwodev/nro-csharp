using Nro.Data;
using Xunit.Abstractions;
using Nro.Data.Interfaces;

namespace Nro.Tests.Db;

/// <summary>
/// Compares <see cref="ResultSnapshot"/> against the REAL Java <c>jbcd.ConnectResultSet</c>: builds the snapshot from the SAME synthetic data
/// in <c>tests/golden/result-snapshot.json</c>, replays every step of every scenario exactly, and compares the WHOLE result
/// (typed value, or exception kind; the message for Java's own custom errors).
/// </summary>
public class ResultSnapshotGoldenTests
{
    private static readonly Lazy<SnapshotGolden> GoldenData = new(SnapshotGolden.Load);

    private readonly ITestOutputHelper _output;

    public ResultSnapshotGoldenTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Steps where C# DELIBERATELY differs from Java. Key: (scenario id, step index). Each entry records both the recorded Java result and the expected C# result,
    /// so if the golden file changes (a step shifts, Java changes) or C# changes behaviour the test goes red instead of silently passing.
    /// </summary>
    private static readonly Dictionary<(string Scenario, int Step), (string Java, string CSharp, string Reason)> KnownDivergences = new()
    {
        [("nav_dispose_then_everything", 18)] = ("err:NPE", "void",
            "second dispose: Java throws NullPointerException (data is already null); C# is idempotent per the IDisposable convention"),
        [("nav_dispose_empty_dataset", 2)] = ("err:NPE", "void",
            "second dispose: Java throws NullPointerException; C# is idempotent"),
        [("name_expression_columns", 3)] = ("long:5", "null",
            "expression column (no table): Java also registers the key '.cnt'; C# has only 'cnt' (contract B-02 section 3)"),
        [("name_expression_columns", 4)] = ("int:2", "null",
            "expression column: Java has the key '.two'; C# does not"),
    };

    [Fact]
    public void Every_step_of_every_scenario_matches_real_Java()
    {
        var golden = GoldenData.Value;
        var mismatches = new List<string>();
        var compared = 0;
        var divergencesSeen = new HashSet<(string, int)>();

        foreach (var scenario in golden.Scenarios)
        {
            var dataset = golden.Datasets[scenario.Dataset];
            var columns = dataset.Columns
                .Select(c => new SnapshotColumn(c.Name, string.IsNullOrEmpty(c.Table) ? null : c.Table))
                .ToArray();
            using var snapshot = new ResultSnapshot(columns, dataset.Rows);

            for (var i = 0; i < scenario.Steps.Length; i++)
            {
                var step = scenario.Steps[i];
                var actual = Run(snapshot, step);
                compared++;

                if (KnownDivergences.TryGetValue((scenario.Id, i), out var known))
                {
                    divergencesSeen.Add((scenario.Id, i));
                    if (step.Expected != known.Java)
                    {
                        mismatches.Add($"{scenario.Id}#{i} {step.Describe()}: golden Java recorded {step.Expected}, but the declared divergence assumed {known.Java}");
                    }
                    else if (actual != known.CSharp)
                    {
                        mismatches.Add($"{scenario.Id}#{i} {step.Describe()}: C# returned {actual}, but the declared divergence expected {known.CSharp} ({known.Reason})");
                    }

                    continue;
                }

                if (actual != step.Expected)
                {
                    mismatches.Add($"{scenario.Id}#{i} {step.Describe()}: Java {step.Expected}, C# {actual}");
                }
            }
        }

        foreach (var key in KnownDivergences.Keys.Where(k => !divergencesSeen.Contains(k)))
        {
            mismatches.Add($"Declared divergence has no matching step in the golden file any more: {key}");
        }

        _output.WriteLine($"Compared {compared} steps of {golden.Scenarios.Count} scenarios with real Java; {divergencesSeen.Count} deliberate divergences.");
        Assert.Equal(golden.DeclaredSteps, compared);
        Assert.Empty(mismatches);
    }

    [Fact]
    public void Golden_has_enough_scenarios_and_covers_every_member_of_the_interface()
    {
        var golden = GoldenData.Value;
        Assert.True(golden.Scenarios.Count >= 40, $"Only {golden.Scenarios.Count} scenarios, the contract requires >= 40");

        var used = golden.Scenarios.SelectMany(s => s.Steps).Select(s => (s.Op, ByName: s.Col is not null)).ToHashSet();

        var navigation = new[] { "next", "first", "gotoResult", "gotoFirst", "gotoBeforeFirst", "gotoLast", "getRows", "dispose" };
        foreach (var op in navigation)
        {
            Assert.Contains((op, false), used);
        }

        var getters = new[]
        {
            "getByte", "getInt", "getShort", "getFloat", "getDouble", "getLong", "getString", "getBoolean", "getObject", "getTimestamp",
        };
        foreach (var op in getters)
        {
            Assert.Contains((op, true), used);   // by name
            Assert.Contains((op, false), used);  // by index
        }
    }

    [Fact]
    public void Golden_has_all_the_kinds_of_Java_errors_that_must_be_preserved()
    {
        var expected = GoldenData.Value.Scenarios.SelectMany(s => s.Steps).Select(s => s.Expected).ToList();

        Assert.Contains("err:NPE", expected);
        Assert.Contains("err:CCE", expected);
        Assert.Contains("err:AIOOBE", expected);
        Assert.Contains("err:EXC:No data available", expected);
        Assert.Contains("err:EXC:Results need to be prepared in advance", expected);
        Assert.Contains("err:EXC:Index out of bound", expected);
    }

    // ------------------------------------------------------------------ running one step

    private static string Run(IResultSnapshot snapshot, GoldenStep step)
    {
        try
        {
            return Canon.Value(Invoke(snapshot, step));
        }
        catch (Exception exception)
        {
            return Canon.Error(exception);
        }
    }

    /// <summary>Calls the right overload (by name or by index). The return type is boxed with the exact C# type of each method.</summary>
    private static object? Invoke(IResultSnapshot rs, GoldenStep step)
    {
        var name = step.Col;
        var index = step.Idx.GetValueOrDefault();
        switch (step.Op)
        {
            case "next":
                return rs.Next();
            case "first":
                return rs.First();
            case "gotoResult":
                return rs.GotoResult(step.Arg!.Value);
            case "gotoFirst":
                return rs.GotoFirst();
            case "gotoBeforeFirst":
                rs.GotoBeforeFirst();
                return Canon.Void;
            case "gotoLast":
                return rs.GotoLast();
            case "getRows":
                return rs.GetRows();
            case "dispose":
                rs.Dispose();
                return Canon.Void;
            case "getByte":
                return name is not null ? rs.GetByte(name) : rs.GetByte(index);
            case "getInt":
                return name is not null ? rs.GetInt(name) : rs.GetInt(index);
            case "getShort":
                return name is not null ? rs.GetShort(name) : rs.GetShort(index);
            case "getFloat":
                return name is not null ? rs.GetFloat(name) : rs.GetFloat(index);
            case "getDouble":
                return name is not null ? rs.GetDouble(name) : rs.GetDouble(index);
            case "getLong":
                return name is not null ? rs.GetLong(name) : rs.GetLong(index);
            case "getString":
                return name is not null ? rs.GetString(name) : rs.GetString(index);
            case "getBoolean":
                return name is not null ? rs.GetBoolean(name) : rs.GetBoolean(index);
            case "getObject":
                return name is not null ? rs.GetObject(name) : rs.GetObject(index);
            case "getTimestamp":
                return name is not null ? rs.GetTimestamp(name) : rs.GetTimestamp(index);
            default:
                throw new InvalidOperationException($"Unknown operation in golden: {step.Op}");
        }
    }
}
