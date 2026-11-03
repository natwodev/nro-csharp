using System.Globalization;
using System.Text.Json;

using Nro.Core;
using Nro.Core.Consts;
using Nro.Core.Interfaces;

using Xunit.Abstractions;

namespace Nro.Tests.Helpers;

/// <summary>
/// Compares the port with the REAL JDK 17 on a few thousand more inputs than the golden file has: <c>util-differential.json</c> next to this file, produced by
/// <c>UtilDifferentialGenerator.java.txt</c> (run it with <c>java UtilDifferentialGenerator.java out.json</c>; the formatting bodies in it are copied from <c>Utils/Util.java</c>).
/// It covers the parts where .NET and Java differ most: <c>DecimalFormat</c> (vi-VN, HALF_EVEN and DOWN), <c>String.format("%.2f")</c>, <c>Math.round</c>, doubles above 2^53 and the
/// bit-exact sequence of <c>java.util.Random</c> behind <see cref="SeededRandomProvider"/>.
/// </summary>
public class UtilDifferentialTests
{
    private const int MaxReportedFailures = 25;

    private static readonly Lazy<JsonDocument> Corpus = new(() => JsonDocument.Parse(File.ReadAllBytes(Path.Combine(TestPaths.RepoRoot(), "tests", "Nro.Tests", "Helpers", "util-differential.json"))));

    private readonly ITestOutputHelper output;

    public UtilDifferentialTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void FormatNumber_matches_java()
    {
        Check("formatNumber", row => (Util.FormatNumber(Dbl(row[0]), Enum.Parse<FormatStyle>(row[1].GetString()!)), row[2].GetString()!, $"{row[0].GetString()} {row[1].GetString()}"), BeyondDigitLimit);
    }

    [Fact]
    public void NumberToMoney_matches_java()
    {
        Check("numberToMoney", row => (Util.NumberToMoney(Lng(row[0])), row[1].GetString()!, row[0].GetString()!));
    }

    [Fact]
    public void Format_double_with_three_and_with_one_fraction_digits_matches_java()
    {
        Check("formatDouble", row => (JavaNumberFormat.FormatDouble(Dbl(row[0]), 3, 0, JavaRoundingMode.HalfEven, JavaNumberSymbols.ViVn), row[1].GetString()!, $"{row[0].GetString()} maxFrac=3"), BeyondDigitLimit);
        Check("formatDouble", row => (JavaNumberFormat.FormatDouble(Dbl(row[0]), 1, 0, JavaRoundingMode.HalfEven, JavaNumberSymbols.ViVn), row[2].GetString()!, $"{row[0].GetString()} maxFrac=1"), BeyondDigitLimit);
    }

    [Fact]
    public void Format_long_matches_java()
    {
        Check("formatLong", row => (JavaNumberFormat.FormatLong(Lng(row[0]), JavaNumberSymbols.ViVn), row[1].GetString()!, row[0].GetString()!));
    }

    [Fact]
    public void RoundToTwoDecimals_matches_java_for_both_default_locales()
    {
        Check("roundToTwoDecimals", row => (Util.RoundToTwoDecimals(Dbl(row[0])), row[1].GetString()!, $"{row[0].GetString()} en-US"), BeyondDigitLimit);
        Check("roundToTwoDecimals", row => (Util.RoundToTwoDecimalsViVn(Dbl(row[0])), row[2].GetString()!, $"{row[0].GetString()} vi-VN"), BeyondDigitLimit);
    }

    [Fact]
    public void FormatFixed_matches_java_String_format_2f()
    {
        Check("fixed2", row => (JavaNumberFormat.FormatFixed(Dbl(row[0]), 2), row[1].GetString()!, row[0].GetString()!), BeyondDigitLimit);
    }

    [Fact]
    public void ConvertMillisecondsDouble_matches_java()
    {
        Check("convertMillisecondsDouble", row => (Util.ConvertMillisecondsDouble(Lng(row[0])), row[1].GetString()!, row[0].GetString()!));
    }

    [Fact]
    public void SeededRandomProvider_replays_java_util_Random_exactly()
    {
        int steps = 0;
        foreach (JsonElement script in Corpus.Value.RootElement.GetProperty("random").EnumerateArray())
        {
            long seed = Lng(script.GetProperty("seed"));
            IRandomProvider provider = new SeededRandomProvider(seed);
            int index = 0;
            foreach (JsonElement step in script.GetProperty("steps").EnumerateArray())
            {
                string op = step[0].GetString()!;
                string where = $"seed {seed} step {index++} op {op}({step[1].GetString()})";
                switch (op)
                {
                    case "I":
                        Assert.True(int.Parse(step[2].GetString()!, CultureInfo.InvariantCulture) == provider.NextInt(), where);
                        break;
                    case "B":
                        Assert.True(int.Parse(step[2].GetString()!, CultureInfo.InvariantCulture) == provider.NextInt(int.Parse(step[1].GetString()!, CultureInfo.InvariantCulture)), where);
                        break;
                    case "L":
                        Assert.True(Lng(step[2]) == provider.NextLong(Lng(step[1])), where);
                        break;
                    case "D":
                        Assert.True(Dbl(step[2]).Equals(provider.NextDouble(Dbl(step[1]))), where);
                        break;
                    case "F":
                        Assert.True(float.Parse(step[2].GetString()!, NumberStyles.Float, CultureInfo.InvariantCulture).Equals(provider.NextFloat(float.Parse(step[1].GetString()!, NumberStyles.Float, CultureInfo.InvariantCulture))), where);
                        break;
                    default:
                        throw new InvalidOperationException(op);
                }

                steps++;
            }
        }

        output.WriteLine($"random: {steps} steps compared with java.util.Random");
        Assert.True(steps > 1000);
    }

    // Runs every row of one corpus section; the row mapper returns (actual, expected, description). A difference is tolerated ONLY for a row that
    // beyondDigitLimit selects (see JavaNumberFormat: JDK 17 prints some doubles of 1e20 and above with a digit string longer than the shortest one) and only
    // while they stay a tiny fraction of those rows.
    private void Check(string section, Func<JsonElement, (string Actual, string Expected, string Description)> run, Func<JsonElement, bool>? beyondDigitLimit = null)
    {
        int rows = 0;
        int limitedRows = 0;
        var failures = new List<string>();
        var tolerated = new List<string>();
        foreach (JsonElement row in Corpus.Value.RootElement.GetProperty(section).EnumerateArray())
        {
            rows++;
            bool limited = beyondDigitLimit is not null && beyondDigitLimit(row);
            if (limited)
            {
                limitedRows++;
            }

            (string actual, string expected, string description) = run(row);
            if (actual != expected)
            {
                (limited ? tolerated : failures).Add($"{section} {description}: expected <{expected}> actual <{actual}>");
            }
        }

        output.WriteLine($"{section}: {rows} rows, {failures.Count} differences, {tolerated.Count} tolerated of {limitedRows} rows beyond 1e20");
        foreach (string line in tolerated)
        {
            output.WriteLine("  tolerated: " + line);
        }

        Assert.True(rows > 100);
        Assert.True(failures.Count == 0, $"{failures.Count} of {rows} differ from Java:{Environment.NewLine}{string.Join(Environment.NewLine, failures.Take(MaxReportedFailures))}");
        Assert.True(tolerated.Count * 20 <= limitedRows, $"{tolerated.Count} of {limitedRows} rows beyond 1e20 differ (more than 5%)");
    }

    private static bool BeyondDigitLimit(JsonElement row) => Math.Abs(Dbl(row[0])) >= 1e20;

    private static long Lng(JsonElement element) => long.Parse(element.GetString()!, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

    private static double Dbl(JsonElement element) => double.Parse(element.GetString()!, NumberStyles.Float, CultureInfo.InvariantCulture);
}
