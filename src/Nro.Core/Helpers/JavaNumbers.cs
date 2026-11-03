using System.Globalization;

namespace Nro.Core;

/// <summary>
/// Java numeric semantics that C# does not share: the narrowing casts of floating point values (saturating, NaN to 0), <c>Math.round</c> (rounds half UP, saturating)
/// and the zero-padded <c>%0Nd</c> of <c>String.format</c> (the sign counts in the width).
/// </summary>
public static class JavaNumbers
{
    /// <summary>Java <c>(int) double</c>: truncates toward zero, saturates at <see cref="int.MinValue"/>/<see cref="int.MaxValue"/>, NaN gives 0.</summary>
    public static int ToInt(double value)
    {
        if (double.IsNaN(value))
        {
            return 0;
        }

        if (value >= int.MaxValue)
        {
            return int.MaxValue;
        }

        if (value <= int.MinValue)
        {
            return int.MinValue;
        }

        return (int)value;
    }

    /// <summary>Java <c>(long) double</c>: truncates toward zero, saturates at <see cref="long.MinValue"/>/<see cref="long.MaxValue"/>, NaN gives 0.</summary>
    public static long ToLong(double value)
    {
        if (double.IsNaN(value))
        {
            return 0;
        }

        if (value >= 9.2233720368547758E18)
        {
            return long.MaxValue;
        }

        if (value <= -9.2233720368547758E18)
        {
            return long.MinValue;
        }

        return (long)value;
    }

    /// <summary>
    /// Java <c>Math.round(double)</c>: the closest <c>long</c>, ties rounded toward positive infinity (<c>round(-2.5) == -2</c>), NaN gives 0, saturating.
    /// (<see cref="Math.Round(double)"/> rounds ties to even and <c>Math.Floor(x + 0.5)</c> is wrong for 0.49999999999999994.)
    /// </summary>
    public static long Round(double value)
    {
        if (double.IsNaN(value))
        {
            return 0;
        }

        if (value >= 9.2233720368547758E18)
        {
            return long.MaxValue;
        }

        if (value <= -9.2233720368547758E18)
        {
            return long.MinValue;
        }

        double floor = Math.Floor(value);
        long whole = (long)floor;
        return value - floor >= 0.5 ? whole + 1 : whole;
    }

    /// <summary>
    /// Java <c>String.format("%0{width}d", value)</c>: decimal digits padded with zeros to <paramref name="width"/> characters INCLUDING the minus sign
    /// (<c>-1</c> with width 2 stays <c>-1</c>, with width 3 gives <c>-01</c>; .NET <c>ToString("D2")</c> would give <c>-01</c> for width 2).
    /// </summary>
    public static string FormatZeroPadded(long value, int width)
    {
        string text = value.ToString(CultureInfo.InvariantCulture);
        if (value >= 0)
        {
            return text.PadLeft(width, '0');
        }

        return "-" + text[1..].PadLeft(width - 1, '0');
    }
}
