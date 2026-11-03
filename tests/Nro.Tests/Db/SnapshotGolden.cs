using System.Globalization;
using System.Text.Json;

namespace Nro.Tests.Db;

/// <summary>
/// Reads <c>tests/golden/result-snapshot.json</c> (produced by the Java harness in <c>port-csharp/tools/db</c> by running the REAL
/// <c>jbcd.ConnectResultSet</c> and <c>jbcd.ConnectDB</c>) and turns the typed {t,v} values into correctly typed C# objects.
/// </summary>
internal sealed class SnapshotGolden
{
    public required IReadOnlyDictionary<string, GoldenDataset> Datasets { get; init; }

    public required IReadOnlyList<GoldenScenario> Scenarios { get; init; }

    public required IReadOnlyList<GoldenExecuteCase> ExecuteCases { get; init; }

    public required int DeclaredSteps { get; init; }

    public static SnapshotGolden Load()
    {
        using var stream = File.OpenRead(TestPaths.Golden("result-snapshot.json"));
        using var document = JsonDocument.Parse(stream);
        var root = document.RootElement;

        var datasets = new Dictionary<string, GoldenDataset>();
        foreach (var d in root.GetProperty("datasets").EnumerateArray())
        {
            var columns = d.GetProperty("columns").EnumerateArray()
                .Select(c => new GoldenColumn(c.GetProperty("name").GetString()!, c.GetProperty("table").GetString()!))
                .ToArray();
            var rows = d.GetProperty("rows").EnumerateArray()
                .Select(r => r.EnumerateArray().Select(DecodeValue).ToArray())
                .ToArray();
            datasets[d.GetProperty("id").GetString()!] = new GoldenDataset(d.GetProperty("id").GetString()!, columns, rows);
        }

        var scenarios = new List<GoldenScenario>();
        foreach (var s in root.GetProperty("scenarios").EnumerateArray())
        {
            var steps = s.GetProperty("steps").EnumerateArray().Select(ParseStep).ToArray();
            scenarios.Add(new GoldenScenario(s.GetProperty("id").GetString()!, s.GetProperty("dataset").GetString()!, steps));
        }

        var cases = new List<GoldenExecuteCase>();
        foreach (var c in root.GetProperty("executeUpdate").GetProperty("cases").EnumerateArray())
        {
            cases.Add(new GoldenExecuteCase(
                c.GetProperty("id").GetString()!,
                c.GetProperty("api").GetString()!,
                c.GetProperty("sql").GetString()!,
                c.GetProperty("args").EnumerateArray().Select(DecodeValue).ToArray(),
                c.GetProperty("prepared").EnumerateArray().Select(p => p.GetString()!).ToArray(),
                c.GetProperty("log").EnumerateArray().Select(p => p.GetString()!).ToArray(),
                c.TryGetProperty("err", out _)));
        }

        return new SnapshotGolden
        {
            Datasets = datasets,
            Scenarios = scenarios,
            ExecuteCases = cases,
            DeclaredSteps = root.GetProperty("meta").GetProperty("counts").GetProperty("steps").GetInt32(),
        };
    }

    private static GoldenStep ParseStep(JsonElement step)
    {
        string expected;
        if (step.TryGetProperty("ok", out var ok))
        {
            expected = Canon.Value(DecodeValue(ok), isVoid: ok.GetProperty("t").GetString() == "void");
        }
        else
        {
            var err = step.GetProperty("err");
            expected = Canon.JavaError(err.GetProperty("class").GetString()!, err.GetProperty("message").GetString());
        }

        return new GoldenStep(
            step.GetProperty("op").GetString()!,
            step.TryGetProperty("col", out var col) ? col.GetString() : null,
            step.TryGetProperty("idx", out var idx) ? idx.GetInt32() : null,
            step.TryGetProperty("arg", out var arg) ? arg.GetInt32() : null,
            expected);
    }

    /// <summary>{"t":..., "v":...} to a correctly typed C# object (Integer to int, Long to long, Short to short, Byte to sbyte, ...).</summary>
    public static object? DecodeValue(JsonElement node)
    {
        var tag = node.GetProperty("t").GetString();
        if (tag is "null" or "void")
        {
            return null;
        }

        var v = node.GetProperty("v");
        return tag switch
        {
            "int" => v.GetInt32(),
            "long" => v.GetInt64(),
            "short" => v.GetInt16(),
            "byte" => v.GetSByte(),
            "float" => float.Parse(v.GetString()!, NumberStyles.Float, CultureInfo.InvariantCulture),
            "double" => double.Parse(v.GetString()!, NumberStyles.Float, CultureInfo.InvariantCulture),
            "bool" => v.GetBoolean(),
            "string" => v.GetString(),
            "decimal" => decimal.Parse(v.GetString()!, NumberStyles.Float, CultureInfo.InvariantCulture),   // BigDecimal.toString may be "1E-7"
            "timestamp" => DateTime.ParseExact(v.GetString()!, "yyyy-MM-dd HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture, DateTimeStyles.None),
            "date" => DateOnly.ParseExact(v.GetString()!, "yyyy-MM-dd", CultureInfo.InvariantCulture),
            "bytes" => Convert.FromHexString(v.GetString()!),
            _ => throw new InvalidOperationException($"Unknown type in golden: {tag}"),
        };
    }
}

internal sealed record GoldenColumn(string Name, string Table);

internal sealed record GoldenDataset(string Id, GoldenColumn[] Columns, object?[][] Rows);

internal sealed record GoldenStep(string Op, string? Col, int? Idx, int? Arg, string Expected)
{
    public string Describe() => Op + (Col is not null ? $"(\"{Col}\")" : string.Empty) + (Idx is not null ? $"[{Idx}]" : string.Empty)
        + (Arg is not null ? $"<{Arg}>" : string.Empty);
}

internal sealed record GoldenScenario(string Id, string Dataset, GoldenStep[] Steps);

internal sealed record GoldenExecuteCase(
    string Id,
    string Api,
    string Sql,
    object?[] Args,
    string[] Prepared,
    string[] Log,
    bool Failed);
