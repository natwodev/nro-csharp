using Nro.Core;

namespace Nro.Tests.Helpers;

/// <summary>Java numeric semantics: casts, <c>Math.round</c>, <c>%0Nd</c>, and the edges of the <c>DecimalFormat</c> / <c>String.format</c> emulation not covered by the golden file.</summary>
public class JavaNumbersTests
{
    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(1.9, 1)]
    [InlineData(-1.9, -1)]
    [InlineData(3e9, int.MaxValue)]
    [InlineData(-3e9, int.MinValue)]
    [InlineData(double.PositiveInfinity, int.MaxValue)]
    [InlineData(double.NegativeInfinity, int.MinValue)]
    [InlineData(2147483647.0, int.MaxValue)]
    [InlineData(-2147483648.0, int.MinValue)]
    public void ToInt_truncates_and_saturates_like_java_cast(double value, int expected)
    {
        Assert.Equal(expected, JavaNumbers.ToInt(value));
    }

    [Theory]
    [InlineData(double.NaN, 0L)]
    [InlineData(1e19, long.MaxValue)]
    [InlineData(-1e19, long.MinValue)]
    [InlineData(9.223372036854775807E18, long.MaxValue)]
    [InlineData(-9.223372036854775808E18, long.MinValue)]
    [InlineData(123456789012.99, 123456789012L)]
    [InlineData(-0.5, 0L)]
    public void ToLong_truncates_and_saturates_like_java_cast(double value, long expected)
    {
        Assert.Equal(expected, JavaNumbers.ToLong(value));
    }

    [Theory]
    [InlineData(0.5, 1L)]
    [InlineData(1.5, 2L)]
    [InlineData(2.5, 3L)]
    [InlineData(-0.5, 0L)]
    [InlineData(-1.5, -1L)]
    [InlineData(-2.5, -2L)]
    [InlineData(0.49999999999999994, 0L)]
    [InlineData(double.NaN, 0L)]
    [InlineData(1e300, long.MaxValue)]
    [InlineData(-1e300, long.MinValue)]
    [InlineData(4503599627370497.0, 4503599627370497L)]
    public void Round_is_math_round_half_up(double value, long expected)
    {
        Assert.Equal(expected, JavaNumbers.Round(value));
    }

    [Theory]
    [InlineData(5L, 2, "05")]
    [InlineData(-5L, 2, "-5")]
    [InlineData(-5L, 3, "-05")]
    [InlineData(123L, 2, "123")]
    [InlineData(0L, 2, "00")]
    [InlineData(long.MinValue, 2, "-9223372036854775808")]
    public void FormatZeroPadded_counts_the_sign_in_the_width(long value, int width, string expected)
    {
        Assert.Equal(expected, JavaNumbers.FormatZeroPadded(value, width));
    }

    [Theory]
    [InlineData(-0.04, 1, 0, JavaRoundingMode.HalfEven, "-0")]
    [InlineData(-0.04, 1, 0, JavaRoundingMode.Down, "-0")]
    [InlineData(0.04, 1, 0, JavaRoundingMode.HalfEven, "0")]
    [InlineData(0.05, 1, 0, JavaRoundingMode.HalfEven, "0,1")] // the double 0.05 is 0.05000000000000000277: above the tie
    [InlineData(0.25, 1, 0, JavaRoundingMode.HalfEven, "0,2")] // exact tie: even digit
    [InlineData(0.35, 1, 0, JavaRoundingMode.HalfEven, "0,3")] // 0.34999999999999997 is below the tie
    [InlineData(0.75, 1, 0, JavaRoundingMode.HalfEven, "0,8")]
    [InlineData(0.99, 1, 0, JavaRoundingMode.HalfEven, "1")]
    [InlineData(0.99, 1, 0, JavaRoundingMode.Down, "0,9")]
    [InlineData(0.75, 1, 0, JavaRoundingMode.Down, "0,7")]
    [InlineData(999999.96, 1, 0, JavaRoundingMode.HalfEven, "1.000.000")]
    [InlineData(1.0, 2, 2, JavaRoundingMode.HalfEven, "1,00")]
    [InlineData(1234.5, 3, 0, JavaRoundingMode.HalfEven, "1.234,5")]
    [InlineData(0.0, 2, 2, JavaRoundingMode.HalfEven, "0,00")]
    public void FormatDouble_edge_cases_follow_DecimalFormat(double value, int maxFraction, int minFraction, JavaRoundingMode mode, string expected)
    {
        Assert.Equal(expected, JavaNumberFormat.FormatDouble(value, maxFraction, minFraction, mode, JavaNumberSymbols.ViVn));
    }

    [Fact]
    public void FormatDouble_uses_the_symbols_given_and_never_the_machine_culture()
    {
        Assert.Equal("1,234.57", JavaNumberFormat.FormatDouble(1234.567, 2, 2, JavaRoundingMode.HalfEven, JavaNumberSymbols.EnUs));
        Assert.Equal("1.234,57", JavaNumberFormat.FormatDouble(1234.567, 2, 2, JavaRoundingMode.HalfEven, JavaNumberSymbols.ViVn));
        Assert.Equal("-1,234,567", JavaNumberFormat.FormatLong(-1234567, JavaNumberSymbols.EnUs));
    }

    [Theory]
    [InlineData(1.005, 2, "1.01")]
    [InlineData(2.675, 2, "2.68")]
    [InlineData(0.0, 2, "0.00")]
    [InlineData(-0.001, 2, "-0.00")]
    [InlineData(0.006, 2, "0.01")]
    [InlineData(0.004, 2, "0.00")]
    [InlineData(999.995, 2, "1000.00")]
    [InlineData(12.5, 0, "13")]
    [InlineData(0.5, 0, "1")]
    [InlineData(0.4, 0, "0")]
    [InlineData(1234567.891, 1, "1234567.9")]
    public void FormatFixed_rounds_half_up_on_the_printed_digits(double value, int precision, string expected)
    {
        Assert.Equal(expected, JavaNumberFormat.FormatFixed(value, precision));
    }

    [Fact]
    public void FormatFixed_prints_the_java_texts_for_non_finite_values()
    {
        Assert.Equal("NaN", JavaNumberFormat.FormatFixed(double.NaN, 2));
        Assert.Equal("Infinity", JavaNumberFormat.FormatFixed(double.PositiveInfinity, 2));
        Assert.Equal("-Infinity", JavaNumberFormat.FormatFixed(double.NegativeInfinity, 2));
        Assert.Equal("-0.00", JavaNumberFormat.FormatFixed(-0.0, 2));
    }
}
