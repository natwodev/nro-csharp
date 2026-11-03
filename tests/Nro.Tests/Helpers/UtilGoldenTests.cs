using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;

using Nro.Core;
using Nro.Core.Consts;

using Xunit.Abstractions;

namespace Nro.Tests.Helpers;

/// <summary>
/// Runs EVERY case of EVERY included <c>Utils.Util</c> function of <c>tests/golden/util-pure.json</c> (generated from the real Java code) against the C# port and requires equality.
/// Arguments are decoded per the file's <c>typeEncoding</c> (long and double are strings carrying Java's <c>toString</c>, a <c>Date</c> is epoch milliseconds in a string, a <c>char</c> is a
/// one-character string). The test also fails when an <c>included</c> entry is not run or runs a different number of cases, so no function can be skipped silently.
/// <para>
/// One test method runs the whole file in order because <c>Util.Format(double)</c> shares a static format with <c>Util.FormatToSecondMinute</c> (a Java quirk the port keeps) and the golden
/// file has a phase BEFORE and a phase AFTER the first call of <c>formatToSecond_Minute</c>. Every test that touches that shared format must live in this class (xunit runs the methods of a
/// class one after another), so nothing else can change it during the run.
/// </para>
/// </summary>
public class UtilGoldenTests
{
    private const long NowSentinel = long.MinValue;

    private readonly ITestOutputHelper output;

    public UtilGoldenTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Every_golden_case_of_Utils_Util_matches_the_port()
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(TestPaths.Golden("util-pure.json")));
        JsonElement root = document.RootElement;
        Util.ResetSharedNumberFormat();

        var failures = new List<string>();
        var executed = new List<(string Key, int Cases, string Phase)>();
        foreach (JsonElement function in root.GetProperty("functions").EnumerateArray())
        {
            if (function.GetProperty("class").GetString() != "Utils.Util")
            {
                continue;
            }

            string key = function.GetProperty("key").GetString()!;
            string returnType = function.GetProperty("returnType").GetString()!;
            JsonElement context = function.TryGetProperty("context", out JsonElement c) ? c : default;
            bool readInt = context.ValueKind == JsonValueKind.Object && context.TryGetProperty("Manager.readInt", out JsonElement ri) && ri.GetBoolean();
            string phase = string.Empty;
            if (context.ValueKind == JsonValueKind.Object && context.TryGetProperty("numMaximumFractionDigits", out JsonElement digits))
            {
                int expectedDigits = digits.GetInt32();
                Assert.Equal(expectedDigits, Util.NumberFormatMaxFractionDigits); // the golden phase must match the state of the shared format
                phase = $"maxFrac={expectedDigits}";
            }

            int count = 0;
            foreach (JsonElement testCase in function.GetProperty("cases").EnumerateArray())
            {
                count++;
                string expected = Normalize(testCase.GetProperty("result"), returnType);
                string actual = Invoke(key, testCase.GetProperty("args"), readInt, viVnLocale: false);
                if (actual != expected)
                {
                    failures.Add($"{key} {phase} args={testCase.GetProperty("args").GetRawText()} expected=<{expected}> actual=<{actual}>");
                }

                if (testCase.TryGetProperty("resultWhenDefaultLocaleViVN", out JsonElement viVn))
                {
                    string expectedViVn = Normalize(viVn, returnType);
                    string actualViVn = Invoke(key, testCase.GetProperty("args"), readInt, viVnLocale: true);
                    if (actualViVn != expectedViVn)
                    {
                        failures.Add($"{key} (vi-VN) args={testCase.GetProperty("args").GetRawText()} expected=<{expectedViVn}> actual=<{actualViVn}>");
                    }
                }
            }

            executed.Add((key, count, phase));
        }

        // Every included entry of Utils.Util must have been run, with the same number of cases.
        var included = new List<(string Key, int Cases, string Phase)>();
        foreach (JsonElement entry in root.GetProperty("included").EnumerateArray())
        {
            if (entry.GetProperty("class").GetString() != "Utils.Util")
            {
                continue;
            }

            string phase = string.Empty;
            if (entry.TryGetProperty("context", out JsonElement ctx) && ctx.TryGetProperty("numMaximumFractionDigits", out JsonElement d))
            {
                phase = $"maxFrac={d.GetInt32()}";
            }

            included.Add((entry.GetProperty("key").GetString()!, entry.GetProperty("cases").GetInt32(), phase));
        }

        Assert.Equal(included, executed);
        int total = executed.Sum(e => e.Cases);
        foreach ((string key, int cases, string phase) in executed)
        {
            output.WriteLine($"{key} {phase}: {cases} cases");
        }

        output.WriteLine($"TOTAL Utils.Util: {executed.Count} entries, {total} cases, {failures.Count} failures");
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures.Take(40)));
        Assert.True(total > 0);
    }

    [Fact]
    public void Shared_number_format_starts_at_three_fraction_digits_and_formatToSecond_Minute_changes_it_for_good()
    {
        Util.ResetSharedNumberFormat();
        Assert.Equal("1.234,568", Util.Format(1234.5678));
        Assert.Equal("1.234.567", Util.Format(1234567));
        Assert.Equal("1.234.567", Util.Format(1234567L));
        Assert.Equal("1 phút", Util.FormatToSecondMinute(60));
        Assert.Equal(1, Util.NumberFormatMaxFractionDigits);
        Assert.Equal("1.234,6", Util.Format(1234.5678));
        Assert.Equal("1.234.567", Util.Format(1234567)); // integers are not affected
        Util.ResetSharedNumberFormat();
        Assert.Equal("1.234,568", Util.Format(1234.5678));
    }

    [Fact]
    public void RemoveAccent_char_matches_the_golden_table_for_all_65536_code_units()
    {
        using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(TestPaths.Golden("util-pure.json")));
        JsonElement map = document.RootElement.GetProperty("removeAccentMap");
        var changed = new Dictionary<int, int>();
        foreach (JsonElement entry in map.GetProperty("changed").EnumerateArray())
        {
            changed[entry.GetProperty("from").GetInt32()] = entry.GetProperty("to").GetInt32();
        }

        Assert.Equal(map.GetProperty("changedCount").GetInt32(), changed.Count);

        var allResults = new byte[65536 * 2];
        for (int unit = 0; unit <= 0xFFFF; unit++)
        {
            int expected = changed.TryGetValue(unit, out int to) ? to : unit;
            char actual = Util.RemoveAccent((char)unit);
            Assert.True(expected == actual, $"U+{unit:X4}: expected U+{expected:X4}, actual U+{(int)actual:X4}");
            allResults[unit * 2] = (byte)(actual >> 8);
            allResults[(unit * 2) + 1] = (byte)(actual & 0xFF);
        }

        // The golden file also carries the SHA-256 of the 65536 result units as UTF-16 big endian.
        Assert.Equal(map.GetProperty("allResultsSha256Utf16BE").GetString(), Convert.ToHexStringLower(SHA256.HashData(allResults)));
    }

    [Fact]
    public void Deliberate_differences_and_error_cases_are_as_documented()
    {
        // +Infinity: the Java loop never ends; the port returns the infinity sign.
        Assert.Equal("∞", Util.FormatNumber(double.PositiveInfinity, FormatStyle.KMB));
        Assert.Throws<ArgumentNullException>(() => Util.AddSlashes(null!));
        Assert.Throws<ArgumentNullException>(() => Util.StrSql(null!));
        Assert.Throws<ArgumentNullException>(() => Util.RemoveAccent((string)null!));
        Assert.Throws<ArgumentNullException>(() => Util.HaveSpecialCharacter(null!));
        Assert.Throws<ArgumentNullException>(() => Util.Replace(null!, "a", "b"));
        Assert.Throws<ArgumentNullException>(() => Util.Contains(["a"], null!));

        // Unparsable text, null and empty give the "now" argument (Java: new Date()).
        Assert.Equal(777, Util.GetDateMilliseconds(null, 777));
        Assert.Equal(777, Util.GetDateMilliseconds(string.Empty, 777));
        Assert.Equal(777, Util.GetDateMilliseconds("not a date", 777));
        Assert.Equal(1705289445000, Util.GetDateMilliseconds("2024-01-15 10:30:45", 777));

        DateTimeOffset parsed = Util.GetDate("2024-01-15 10:30:45", TimeProvider.System);
        Assert.Equal(1705289445000, parsed.ToUnixTimeMilliseconds());
        Assert.Equal(TimeSpan.FromHours(7), parsed.Offset);
        DateTimeOffset before = DateTimeOffset.UtcNow;
        DateTimeOffset fallback = Util.GetDate("garbage", TimeProvider.System);
        Assert.InRange(fallback.ToUnixTimeMilliseconds(), before.ToUnixTimeMilliseconds(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

        // A Java Date shown in Vietnam time does not depend on the machine zone: 2024-01-15T03:30:45Z is 10:30:45 at +07:00.
        Assert.Equal("2024-01-15 10:30:45", Util.ToDateString(DateTimeOffset.Parse("2024-01-15T03:30:45Z", CultureInfo.InvariantCulture)));
        Assert.Equal("2025-01-01 01:01:00", Util.ToDateString((DateTimeOffset?)null));
        Assert.True(Util.IsAfterDay(DateTimeOffset.Parse("2024-01-15T17:30:00Z", CultureInfo.InvariantCulture), DateTimeOffset.Parse("2024-01-15T16:00:00Z", CultureInfo.InvariantCulture)));
    }

    // ---- decoding of the golden arguments (typeEncoding) and the call of the port ----

    private static string Invoke(string key, JsonElement args, bool readInt, bool viVnLocale)
    {
        JsonElement a0 = args.GetArrayLength() > 0 ? args[0] : default;
        JsonElement a1 = args.GetArrayLength() > 1 ? args[1] : default;
        JsonElement a2 = args.GetArrayLength() > 2 ? args[2] : default;
        JsonElement a3 = args.GetArrayLength() > 3 ? args[3] : default;
        switch (key)
        {
            case "createIdBossClone(int)": return Text(Util.CreateIdBossClone(a0.GetInt32()));
            case "contains(String[],String)": return Text(Util.Contains(Strings(a0), a1.GetString()!));
            case "formatTimeHMS(int)": return Util.FormatTimeHMS(a0.GetInt32());
            case "formatNumber(double,FormatStyle)": return Util.FormatNumber(Dbl(a0), Enum.Parse<FormatStyle>(a1.GetString()!));
            case "numberToMoney(long)": return Util.NumberToMoney(Lng(a0));
            case "getDistance(int,int,int,int)": return Text(Util.GetDistance(a0.GetInt32(), a1.GetInt32(), a2.GetInt32(), a3.GetInt32()));
            case "replace(String,String,String)": return Util.Replace(a0.GetString()!, a1.GetString()!, a2.GetString()!);
            case "haveSpecialCharacter(String)": return Text(Util.HaveSpecialCharacter(a0.GetString()!));
            case "removeAccent(char)": return Util.RemoveAccent(a0.GetString()![0]).ToString();
            case "removeAccent(String)": return Util.RemoveAccent(a0.GetString()!);
            case "addArray(Object[][])": return Text(Util.AddArray(Arrays(a0)));
            case "formatBytes(long)": return Util.FormatBytes(Lng(a0));
            case "addSlashes(String)": return Util.AddSlashes(a0.GetString()!);
            case "isNumeric(String)": return Text(Util.IsNumeric(a0.GetString()));
            case "roundToTwoDecimals(double)": return viVnLocale ? Util.RoundToTwoDecimalsViVn(Dbl(a0)) : Util.RoundToTwoDecimals(Dbl(a0));
            case "strSQL(String)": return Util.StrSql(a0.GetString()!);
            case "getPercent(long,int)": return Text(Util.GetPercent(Lng(a0), a1.GetInt32()));
            case "CrisGH(double)": return Text(Util.CrisGH(Dbl(a0), readInt));
            case "format(double)": return Util.Format(Dbl(a0));
            case "format(int)": return Util.Format(a0.GetInt32());
            case "format(long)": return Util.Format(Lng(a0));
            case "formatToSecond_Minute(long)": return Util.FormatToSecondMinute(Lng(a0));
            case "toDateString(Date)": return a0.ValueKind == JsonValueKind.Null ? Util.ToDateString((DateTimeOffset?)null) : Util.ToDateString(Lng(a0));
            case "getDate(String)": return Text(Util.GetDateMilliseconds(a0.GetString(), NowSentinel));
            case "highlightsItem(boolean,int)": return Text(Util.HighlightsItem(a0.GetBoolean(), a1.GetInt32()));
            case "checkDo(ItemOption)": return Text(Util.CheckDo(a0.GetProperty("optionId").GetInt32(), a0.GetProperty("param").GetInt32()));
            case "tinhLuyThua(int,int)": return Text(Util.TinhLuyThua(a0.GetInt32(), a1.GetInt32()));
            case "convertMillisecondsDouble(long)": return Util.ConvertMillisecondsDouble(Lng(a0));
            case "convertSecondsToTime(long)": return Util.ConvertSecondsToTime(Lng(a0));
            case "convertMilliseconds(long)": return Util.ConvertMilliseconds(Lng(a0));
            case "convertMilliseconds_ms(long)": return Util.ConvertMillisecondsMs(Lng(a0));
            case "convertMilliseconds_Giay(long)": return Util.ConvertMillisecondsGiay(Lng(a0));
            case "isAfterDay(Date,Date)": return Text(Util.IsAfterDay(Lng(a0), Lng(a1)));
            case "getHead(byte)": return Text(Util.GetHead((sbyte)a0.GetInt32()));
            case "getLeg(byte)": return Text(Util.GetLeg((sbyte)a0.GetInt32()));
            case "getBody(byte)": return Text(Util.GetBody((sbyte)a0.GetInt32()));
            default: throw new InvalidOperationException($"The golden file has a function this test does not know: {key}");
        }
    }

    private static long Lng(JsonElement element) => long.Parse(element.GetString()!, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);

    // Java Double.toString text: "NaN", "Infinity", "-Infinity", "1.0E10", "-0.0", "5.0E-4".
    private static double Dbl(JsonElement element) => double.Parse(element.GetString()!, NumberStyles.Float, CultureInfo.InvariantCulture);

    private static string?[]? Strings(JsonElement element) =>
        element.ValueKind == JsonValueKind.Null ? null : element.EnumerateArray().Select(e => e.GetString()).ToArray();

    private static object?[]?[]? Arrays(JsonElement element) =>
        element.ValueKind == JsonValueKind.Null
            ? null
            : element.EnumerateArray().Select(inner => inner.ValueKind == JsonValueKind.Null ? null : inner.EnumerateArray().Select(e => (object?)e.GetString()).ToArray()).ToArray();

    private static string Text(bool value) => value ? "true" : "false";

    private static string Text(long value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Text(object?[]? array) =>
        array is null ? "<null>" : "[" + string.Join("|", array.Select(e => e is null ? "<null>" : e.ToString())) + "]";

    // The expected value of a case as the same text the invocation produces.
    private static string Normalize(JsonElement result, string returnType)
    {
        switch (returnType)
        {
            case "int":
            case "byte":
                return result.GetInt64().ToString(CultureInfo.InvariantCulture);
            case "long":
                return result.GetString()!;
            case "boolean":
                return Text(result.GetBoolean());
            case "String":
            case "char":
                return result.GetString()!;
            case "Date":
                return result.GetString()!;
            case "Object[]":
                return result.ValueKind == JsonValueKind.Null
                    ? "<null>"
                    : "[" + string.Join("|", result.EnumerateArray().Select(e => e.ValueKind == JsonValueKind.Null ? "<null>" : e.GetString())) + "]";
            default:
                throw new InvalidOperationException($"Unknown return type {returnType}");
        }
    }
}
