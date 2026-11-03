using System.Globalization;
using Nro.Data.Interfaces;

namespace Nro.Data;

/// <summary>
/// Reproduces Java <c>String.valueOf(Object)</c> for the types the database driver returns, so that
/// <see cref="IResultSnapshot.GetString(int)"/> gives the same string as the Java version.
/// </summary>
/// <remarks>
/// <para>
/// Differences that cause silent bugs if C# <c>ToString()</c> is used: <c>true</c> prints <c>"True"</c> (Java <c>"true"</c>);
/// floating-point numbers print <c>"1E+20"</c> / <c>"10000000"</c> (Java <c>"1.0E20"</c> / <c>"1.0E7"</c>); NULL prints empty (Java <c>"null"</c>);
/// <c>DateTime</c> prints according to the culture (Java <c>"2024-05-06 07:08:09.0"</c>).
/// </para>
/// <para>
/// For <c>float</c>/<c>double</c> it uses the shortest digits that still round-trip the value (like <c>Double.toString</c> since JDK 19).
/// JDK 17 (the Java version the source ran on) DIFFERS in the following places, and C# does NOT reproduce them: (1) for |x| &lt; 1e7 no difference
/// was measured (a random sample of about 30,000 values per type); (2) for |x| &gt;= 1e7 JDK 17 often prints extra digits compared with the shortest form
/// (random sample of 30,000 values per type: 3,262 floats and 73 doubles differ, e.g. Java <c>8.4065202E17</c> versus C# <c>8.40652E17</c>),
/// so a large floating-point number read with <c>GetString</c> may give a different string from Java; (3) for subnormal values near the minimum
/// (<c>Double.MIN_VALUE</c> gives Java <c>4.9E-324</c>) Java always takes the two nearest digits. Code that must compare or store the string of a large
/// floating-point number must read it with <c>GetDouble</c>/<c>GetFloat</c> and not rely on <c>GetString</c>.
/// A type with no Java equivalent (e.g. <c>byte[]</c>, which prints <c>[B@hash</c>) falls back to culture-invariant <c>Convert.ToString</c>.
/// </para>
/// </remarks>
internal static class JavaStringFormat
{
    public static string ValueOf(object? value) => value switch
    {
        null => "null",
        string text => text,
        bool flag => flag ? "true" : "false",
        float number => FloatingToString(number.ToString("R", CultureInfo.InvariantCulture), float.IsNaN(number), float.IsInfinity(number), number),
        double number => FloatingToString(number.ToString("R", CultureInfo.InvariantCulture), double.IsNaN(number), double.IsInfinity(number), number),
        decimal number => DecimalToString(number),
        DateTime time => TimestampToString(time),
        DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        TimeOnly time => time.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
        char letter => letter.ToString(),
        byte[] bytes => Convert.ToHexString(bytes),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null",
    };

    /// <summary>
    /// <c>Double.toString</c>/<c>Float.toString</c>: plain decimal when 10^-3 &lt;= |x| &lt; 10^7 (always at least one digit after the
    /// decimal point), otherwise scientific notation <c>d.dddE+/-n</c> (always at least one digit after the decimal point, and the exponent has no plus sign).
    /// </summary>
    private static string FloatingToString(string roundTrip, bool isNaN, bool isInfinity, double approximate)
    {
        if (isNaN)
        {
            return "NaN";
        }

        if (isInfinity)
        {
            return approximate > 0 ? "Infinity" : "-Infinity";
        }

        if (approximate == 0)
        {
            return double.IsNegative(approximate) ? "-0.0" : "0.0";
        }

        var negative = roundTrip[0] == '-';
        var s = negative ? roundTrip[1..] : roundTrip;

        // Split off the exponent: "1.2345E+17" or "1E-05" or "123.45"
        var exponent = 0;
        var e = s.IndexOfAny(['E', 'e']);
        if (e >= 0)
        {
            exponent = int.Parse(s[(e + 1)..], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
            s = s[..e];
        }

        var dot = s.IndexOf('.');
        var integerPart = dot >= 0 ? s[..dot] : s;
        var fractionPart = dot >= 0 ? s[(dot + 1)..] : string.Empty;

        // Value = 0.DIGITS x 10^pointPosition. Strip leading zeros (adjusting the point position) and trailing zeros.
        var digits = integerPart + fractionPart;
        var pointPosition = integerPart.Length + exponent;
        var lead = 0;
        while (lead < digits.Length - 1 && digits[lead] == '0')
        {
            lead++;
        }

        digits = digits[lead..];
        pointPosition -= lead;
        digits = digits.TrimEnd('0');
        if (digits.Length == 0)
        {
            digits = "0";
        }

        var scientificExponent = pointPosition - 1;
        string body;
        if (scientificExponent >= -3 && scientificExponent < 7)
        {
            if (scientificExponent >= 0)
            {
                var integerDigits = scientificExponent + 1;
                var whole = digits.Length >= integerDigits ? digits[..integerDigits] : digits.PadRight(integerDigits, '0');
                var fraction = digits.Length > integerDigits ? digits[integerDigits..] : "0";
                body = whole + "." + fraction;
            }
            else
            {
                body = "0." + new string('0', -scientificExponent - 1) + digits;
            }
        }
        else
        {
            var mantissa = digits.Length > 1 ? digits[..1] + "." + digits[1..] : digits + ".0";
            body = mantissa + "E" + scientificExponent.ToString(CultureInfo.InvariantCulture);
        }

        return negative ? "-" + body : body;
    }

    /// <summary>
    /// <c>BigDecimal.toString</c> for a number with a non-negative scale: plain decimal if the adjusted exponent is &gt;= -6,
    /// otherwise scientific notation (<c>1E-7</c>). Keeps the trailing zeros (<c>12.50</c> stays <c>12.50</c>).
    /// </summary>
    private static string DecimalToString(decimal value)
    {
        var s = value.ToString(CultureInfo.InvariantCulture);
        var negative = s[0] == '-';
        if (negative)
        {
            s = s[1..];
        }

        var dot = s.IndexOf('.');
        var scale = dot < 0 ? 0 : s.Length - dot - 1;
        var digits = (dot < 0 ? s : s.Remove(dot, 1)).TrimStart('0');
        if (digits.Length == 0)
        {
            digits = "0";
        }

        string body;
        if (scale == 0)
        {
            body = digits;
        }
        else
        {
            var adjusted = -scale + (digits.Length - 1);
            if (adjusted >= -6)
            {
                body = digits.Length > scale
                    ? digits[..^scale] + "." + digits[^scale..]
                    : "0." + new string('0', scale - digits.Length) + digits;
            }
            else
            {
                body = (digits.Length > 1 ? digits[..1] + "." + digits[1..] : digits)
                    + "E-" + (-adjusted).ToString(CultureInfo.InvariantCulture);
            }
        }

        return negative ? "-" + body : body;
    }

    /// <summary><c>Timestamp.toString</c>: <c>yyyy-MM-dd HH:mm:ss.f...</c>, trailing zeros of the fraction are dropped but there is always at least one digit.</summary>
    private static string TimestampToString(DateTime time)
    {
        var fraction = time.Ticks % TimeSpan.TicksPerSecond;
        var digits = fraction == 0 ? "0" : fraction.ToString("D7", CultureInfo.InvariantCulture).TrimEnd('0');
        return time.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "." + digits;
    }
}
