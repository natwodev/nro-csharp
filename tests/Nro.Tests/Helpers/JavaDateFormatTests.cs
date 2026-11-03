using System.Globalization;
using Nro.Core;
using Nro.Tests.IO;

namespace Nro.Tests.Helpers;

/// <summary>
/// <see cref="JavaDateFormat"/> beyond the golden file: values measured on the real JDK 17 (<see cref="JdkMeasuredDateCases"/>), a differential test of the calendar
/// arithmetic against .NET for several time zone offsets, and round trips.
/// </summary>
public class JavaDateFormatTests
{
    [Fact]
    public void Format_gives_the_values_measured_on_the_JDK()
    {
        var failures = new List<string>();
        foreach ((long millis, string pattern, string expected) in JdkMeasuredDateCases.Formats)
        {
            string actual = JavaDateFormat.Format(millis, pattern);
            if (actual != expected)
            {
                failures.Add($"format({millis}, \"{pattern}\"): JDK '{expected}', C# '{actual}'");
            }
        }

        Assert.True(failures.Count == 0, $"{failures.Count} of {JdkMeasuredDateCases.Formats.Length} differ:\n" + string.Join("\n", failures));
    }

    [Fact]
    public void Parse_gives_the_values_measured_on_the_JDK_including_lenient_and_failing_inputs()
    {
        var failures = new List<string>();
        foreach ((string text, string pattern, string expected) in JdkMeasuredDateCases.Parses)
        {
            bool ok = JavaDateFormat.TryParse(text, pattern, out long millis);
            string actual = ok ? millis.ToString(CultureInfo.InvariantCulture) : "FAIL";
            if (actual != expected)
            {
                failures.Add($"parse(\"{text}\", \"{pattern}\"): JDK '{expected}', C# '{actual}'");
            }
        }

        Assert.True(failures.Count == 0, $"{failures.Count} of {JdkMeasuredDateCases.Parses.Length} differ:\n" + string.Join("\n", failures));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    [InlineData(-8)]
    [InlineData(14)]
    [InlineData(-12)]
    public void Format_matches_dotnet_in_any_time_zone_offset_so_it_cannot_depend_on_the_machine_zone(int offsetHours)
    {
        var offset = TimeSpan.FromHours(offsetHours);
        var rng = new SplitMix64(12345 + offsetHours);
        for (int i = 0; i < 4000; i++)
        {
            // instants from year 1 to year 9998 (second half: the 20th-22nd century where the game lives)
            long millis = i % 2 == 0
                ? DateTimeOffset.MinValue.ToUnixTimeMilliseconds() + 86_400_000L + (long)(rng.Next() % (ulong)(253_402_300_800_000L - 2 * 86_400_000L))
                : 189_302_400_000L + (long)(rng.Next() % 4_000_000_000_000UL);
            DateTimeOffset reference = DateTimeOffset.FromUnixTimeMilliseconds(millis).ToOffset(offset);

            string expectedDate = reference.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            Assert.Equal(expectedDate, JavaDateFormat.Format(millis, "yyyy-MM-dd HH:mm:ss.SSS", offset));
            Assert.Equal(reference.DayOfYear.ToString(CultureInfo.InvariantCulture), JavaDateFormat.Format(millis, "D", offset));
            Assert.Equal(reference.ToString("ddd", CultureInfo.InvariantCulture), JavaDateFormat.Format(millis, "EEE", offset));
            Assert.Equal(reference.ToString("dddd MMMM", CultureInfo.InvariantCulture), JavaDateFormat.Format(millis, "EEEE MMMM", offset));
            Assert.Equal(reference.ToString("hh:mm tt", CultureInfo.InvariantCulture), JavaDateFormat.Format(millis, "hh:mm a", offset));
        }
    }

    [Fact]
    public void Format_prints_the_requested_zone_offset_for_Z_and_X()
    {
        long now = 1705289445000L;
        Assert.Equal("+0700", JavaDateFormat.Format(now, "Z"));
        Assert.Equal("-0800", JavaDateFormat.Format(now, "Z", TimeSpan.FromHours(-8)));
        Assert.Equal("+0530", JavaDateFormat.Format(now, "Z", new TimeSpan(5, 30, 0)));
        Assert.Equal("Z", JavaDateFormat.Format(now, "X", TimeSpan.Zero));
        Assert.Equal("Z", JavaDateFormat.Format(now, "XXX", TimeSpan.Zero));
        Assert.Equal("+0000", JavaDateFormat.Format(now, "Z", TimeSpan.Zero));
        Assert.Equal("+05:30", JavaDateFormat.Format(now, "XXX", new TimeSpan(5, 30, 0)));
        Assert.Equal("-08", JavaDateFormat.Format(now, "X", TimeSpan.FromHours(-8)));
    }

    [Fact]
    public void Parse_of_a_formatted_instant_returns_the_same_instant()
    {
        var rng = new SplitMix64(777);
        for (int i = 0; i < 5000; i++)
        {
            long millis = 189_302_400_000L + (long)(rng.Next() % 4_000_000_000_000UL);
            string text = JavaDateFormat.Format(millis, "yyyy-MM-dd HH:mm:ss.SSS");
            Assert.True(JavaDateFormat.TryParse(text, "yyyy-MM-dd HH:mm:ss.SSS", out long parsed), text);
            Assert.Equal(millis, parsed);

            string compact = JavaDateFormat.Format(millis, "yyyyMMddHHmmssSSS");
            Assert.True(JavaDateFormat.TryParse(compact, "yyyyMMddHHmmssSSS", out parsed), compact);
            Assert.Equal(millis, parsed);

            string twelve = JavaDateFormat.Format(millis, "dd/MM/yyyy hh:mm:ss.SSS a");
            Assert.True(JavaDateFormat.TryParse(twelve, "dd/MM/yyyy hh:mm:ss.SSS a", out parsed), twelve);
            Assert.Equal(millis, parsed);
        }
    }

    [Fact]
    public void The_hostile_test_culture_really_changes_dotnet_formatting()
    {
        HostileCulture.Run(() =>
        {
            Assert.Equal("~5", (-5).ToString());
            Assert.Equal("04.03.2024", new DateTime(2024, 3, 4).ToString("dd/MM/yyyy"));
            Assert.Equal("Thang 3", new DateTime(2024, 3, 4).ToString("MMMM"));
        });
    }

    [Fact]
    public void Format_and_parse_do_not_depend_on_the_current_culture()
    {
        HostileCulture.Run(() =>
        {
            Assert.True(JavaDateFormat.TryParse("15/01/2024 10:30:45", "dd/MM/yyyy HH:mm:ss", out long t));
            Assert.Equal(1705289445000L, t);
            Assert.Equal("15/01/2024 10:30:45", JavaDateFormat.Format(1705289445000L, "dd/MM/yyyy HH:mm:ss"));
            Assert.Equal("Mon, 15 Jan 2024 10:30:45 AM", JavaDateFormat.Format(1705289445000L, "EEE, d MMM yyyy hh:mm:ss a"));
            Assert.Equal("Monday January", JavaDateFormat.Format(1705289445000L, "EEEE MMMM"));
            Assert.True(JavaDateFormat.TryParse("Jan 5, 2024 11:00 PM", "MMM d, yyyy hh:mm a", out t));
            Assert.Equal("2024-01-05 23:00", JavaDateFormat.Format(t, "yyyy-MM-dd HH:mm"));
        });
    }

    [Theory]
    [InlineData("bbbb", "Illegal pattern character 'b'")]
    [InlineData("yyyy-MM-dd Q", "Illegal pattern character 'Q'")]
    [InlineData("HH:mm B", "Illegal pattern character 'B'")]
    [InlineData("yyyy'", "Unterminated quote")]
    [InlineData("'abc", "Unterminated quote")]
    [InlineData("XXXX", "invalid ISO 8601 format: length=4")]
    public void An_invalid_pattern_throws_ArgumentException_with_the_Java_message(string pattern, string message)
    {
        ArgumentException formatError = Assert.Throws<ArgumentException>(() => JavaDateFormat.Format(0, pattern));
        Assert.Equal(message, formatError.Message);
        ArgumentException parseError = Assert.Throws<ArgumentException>(() => JavaDateFormat.TryParse("2024", pattern, out _));
        Assert.Equal(message, parseError.Message);
    }

    [Theory]
    [InlineData("z")]
    [InlineData("w")]
    [InlineData("W")]
    [InlineData("F")]
    [InlineData("Y")]
    public void A_valid_Java_letter_that_is_not_implemented_throws_NotSupported_instead_of_a_wrong_result(string pattern)
    {
        Assert.Throws<NotSupportedException>(() => JavaDateFormat.Format(0, pattern));
        Assert.Throws<NotSupportedException>(() => JavaDateFormat.TryParse("1", pattern, out _));
    }

    [Theory]
    [InlineData("E")]
    [InlineData("Z")]
    [InlineData("X")]
    [InlineData("D")]
    [InlineData("G")]
    [InlineData("u")]
    public void Letters_that_can_be_formatted_but_not_parsed_throw_NotSupported_when_parsing(string pattern)
    {
        Assert.Throws<NotSupportedException>(() => JavaDateFormat.TryParse("1", pattern, out _));
    }

    [Fact]
    public void A_two_digit_year_cannot_be_parsed_because_Java_resolves_it_against_the_current_date()
    {
        Assert.Throws<NotSupportedException>(() => JavaDateFormat.TryParse("24-01-15", "yy-MM-dd", out _));
        Assert.Throws<NotSupportedException>(() => JavaDateFormat.TryParse("24", "y", out _));

        // a pattern with yy still FORMATS, and four digits for a short pattern are a plain year (as in Java)
        Assert.Equal("24", JavaDateFormat.Format(1705289445000L, "yy"));
        Assert.True(JavaDateFormat.TryParse("2024", "y", out long ms));
        Assert.Equal(1704042000000L, ms);
    }

    [Fact]
    public void Null_arguments_throw_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => JavaDateFormat.Format(0, null!));
        Assert.Throws<ArgumentNullException>(() => JavaDateFormat.TryParse(null!, "yyyy", out _));
        Assert.Throws<ArgumentNullException>(() => JavaDateFormat.TryParse("2024", null!, out _));
    }

    [Fact]
    public void Dates_far_outside_the_range_of_DateTime_still_format_and_parse()
    {
        // 99999-01-01 00:00:00 in Vietnam time, measured on the JDK
        Assert.True(JavaDateFormat.TryParse("99999-01-01", "yyyy-MM-dd", out long ms));
        Assert.Equal(3093496419600000L, ms);
        Assert.Equal("99999-01-01", JavaDateFormat.Format(ms, "yyyy-MM-dd"));
    }
}
