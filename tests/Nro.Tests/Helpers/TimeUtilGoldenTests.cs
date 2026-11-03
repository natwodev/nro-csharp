using System.Globalization;
using System.Text.Json;
using Nro.Core;
using Nro.Tests.IO;

namespace Nro.Tests.Helpers;

/// <summary>
/// Runs EVERY case of EVERY <c>Utils.TimeUtil</c> function listed under <c>included</c> in <c>tests/golden/util-pure.json</c> (generated from the real Java code,
/// JDK 17, zone Asia/Ho_Chi_Minh, locale en-US) against <see cref="TimeUtil"/> and requires equality (result string, or the exception type and message).
/// Each function is also run under a few unusual cultures: the result must not depend on the machine's culture.
/// </summary>
public class TimeUtilGoldenTests
{
    private const string FileName = "util-pure.json";

    // golden key -> C# call. Dates arrive as epoch-millisecond strings, longs as strings, ints as JSON numbers.
    private static readonly Dictionary<string, Func<JsonElement, string>> Calls = new()
    {
        ["diffDate(Date,Date,byte)"] = a =>
        {
            long d1 = Long(a[0]);
            long d2 = Long(a[1]);
            sbyte type = (sbyte)a[2].GetInt32();
            long viaLong = TimeUtil.DiffDate(d1, d2, type);
            long viaDate = TimeUtil.DiffDate(DateTimeOffset.FromUnixTimeMilliseconds(d1), DateTimeOffset.FromUnixTimeMilliseconds(d2), type);
            Assert.Equal(viaLong, viaDate);
            return Str(viaLong);
        },
        ["getTime(String,String)"] = a => Str(TimeUtil.GetTime(a[0].GetString()!, a[1].GetString()!)),
        ["getTime(long)"] = a => TimeUtil.GetTime(Long(a[0])),
        ["convertTime(int)"] = a => TimeUtil.ConvertTime(a[0].GetInt32()),
        ["formatTime(Date,String)"] = a =>
        {
            long ms = Long(a[0]);
            string viaDate = TimeUtil.FormatTime(DateTimeOffset.FromUnixTimeMilliseconds(ms), a[1].GetString()!);
            Assert.Equal(TimeUtil.FormatTime(ms, a[1].GetString()!), viaDate);
            return viaDate;
        },
        ["formatTime(long,String)"] = a => TimeUtil.FormatTime(Long(a[0]), a[1].GetString()!),
        ["getTimeAgo(int)"] = a => TimeUtil.GetTimeAgo(a[0].GetInt32()),
        ["convertMillisecondToMinute(long)"] = a => TimeUtil.ConvertMillisecondToMinute(Long(a[0])),
        ["convertMillisecondToHour(long)"] = a => TimeUtil.ConvertMillisecondToHour(Long(a[0])),
        ["convertMillisecondToDay(long)"] = a => TimeUtil.ConvertMillisecondToDay(Long(a[0])),
        ["getDeadline(long,long)"] = a => TimeUtil.GetDeadline(Long(a[0]), Long(a[1])),
    };

    public static IEnumerable<object[]> Keys() => Calls.Keys.Select(k => new object[] { k });

    private static long Long(JsonElement e) => long.Parse(e.GetString()!, CultureInfo.InvariantCulture);

    private static string Str(long v) => v.ToString(CultureInfo.InvariantCulture);

    private static JsonElement[] TimeUtilFunctions(JsonDocument doc)
        => doc.RootElement.GetProperty("functions").EnumerateArray().Where(f => f.GetProperty("class").GetString() == "Utils.TimeUtil").ToArray();

    [Fact]
    public void Every_included_TimeUtil_function_of_the_golden_file_has_a_test_call()
    {
        using JsonDocument doc = OpenGolden();
        string[] golden = TimeUtilFunctions(doc).Select(f => f.GetProperty("key").GetString()!).OrderBy(k => k, StringComparer.Ordinal).ToArray();
        string[] mapped = Calls.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray();
        Assert.Equal(golden, mapped);

        // the 'included' list of the file agrees with 'functions' on the case counts
        foreach (JsonElement inc in doc.RootElement.GetProperty("included").EnumerateArray().Where(i => i.GetProperty("class").GetString() == "Utils.TimeUtil"))
        {
            string key = inc.GetProperty("key").GetString()!;
            JsonElement fn = TimeUtilFunctions(doc).Single(f => f.GetProperty("key").GetString() == key);
            Assert.Equal(inc.GetProperty("cases").GetInt32(), fn.GetProperty("cases").GetArrayLength());
        }
    }

    [Theory]
    [MemberData(nameof(Keys))]
    public void All_golden_cases_of_the_function_give_the_Java_result(string key)
    {
        using JsonDocument doc = OpenGolden();
        JsonElement fn = TimeUtilFunctions(doc).Single(f => f.GetProperty("key").GetString() == key);
        int expectedCount = fn.GetProperty("cases").GetArrayLength();

        // a hostile culture (other signs, separators, digits, month names) must not change anything: the Java code ran with en-US
        foreach (bool hostile in new[] { false, true })
        {
            string label = hostile ? "hostile culture" : "default culture";
            void RunAll()
            {
                int run = 0;
                var failures = new List<string>();
                foreach (JsonElement c in fn.GetProperty("cases").EnumerateArray())
                {
                    run++;
                    CheckCase(key, c, label, failures);
                }

                Assert.Equal(expectedCount, run);
                Assert.True(failures.Count == 0, $"{key}: {failures.Count} of {run} cases differ from Java:\n" + string.Join("\n", failures));
            }

            if (hostile)
            {
                HostileCulture.Run(RunAll);
            }
            else
            {
                RunAll();
            }
        }
    }

    private static void CheckCase(string key, JsonElement c, string culture, List<string> failures)
    {
        JsonElement args = c.GetProperty("args");
        string argText = args.GetRawText();
        if (c.TryGetProperty("throws", out JsonElement throws))
        {
            string javaClass = throws.GetProperty("class").GetString()!;
            string message = throws.GetProperty("message").GetString()!;
            try
            {
                string unexpected = Calls[key](args);
                failures.Add($"[{culture}] {argText}: Java throws {javaClass}, C# returned '{unexpected}'");
            }
            catch (Exception ex)
            {
                Type expectedType = javaClass switch
                {
                    "java.lang.Exception" => typeof(InvalidTimeException),
                    "java.lang.IllegalArgumentException" => typeof(ArgumentException),
                    _ => throw new InvalidOperationException("Unmapped Java exception class " + javaClass),
                };
                if (ex.GetType() != expectedType || ex.Message != message)
                {
                    failures.Add($"[{culture}] {argText}: Java {javaClass}('{message}'), C# {ex.GetType().Name}('{ex.Message}')");
                }
            }

            return;
        }

        JsonElement expected = c.GetProperty("result");
        string expectedText = expected.ValueKind == JsonValueKind.String ? expected.GetString()! : expected.GetRawText();
        try
        {
            string actual = Calls[key](args);
            if (actual != expectedText)
            {
                failures.Add($"[{culture}] {argText}: Java '{expectedText}', C# '{actual}'");
            }
        }
        catch (Exception ex) when (ex is not Xunit.Sdk.XunitException)
        {
            failures.Add($"[{culture}] {argText}: Java '{expectedText}', C# threw {ex.GetType().Name}('{ex.Message}')");
        }
    }

    private static JsonDocument OpenGolden() => GoldenFile.Open(FileName);
}
