using System.Globalization;
using System.Numerics;
using System.Text;

namespace Nro.Core;

/// <summary>The rounding modes <c>Util.java</c> uses with <c>java.text.DecimalFormat</c>: <c>RoundingMode.DOWN</c> (explicit) and <c>HALF_EVEN</c> (the default).</summary>
public enum JavaRoundingMode
{
    /// <summary>Truncate the decimal digits Java prints for the double (<c>RoundingMode.DOWN</c>).</summary>
    Down,

    /// <summary>Round to nearest, a tie goes to the even digit (<c>RoundingMode.HALF_EVEN</c>, the default of <c>DecimalFormat</c>).</summary>
    HalfEven,
}

/// <summary>The two symbols of a Java <c>DecimalFormatSymbols</c> that matter here. Spelled out explicitly: the result never depends on the machine's culture.</summary>
/// <param name="DecimalSeparator">The decimal separator.</param>
/// <param name="GroupingSeparator">The thousands separator.</param>
public readonly record struct JavaNumberSymbols(char DecimalSeparator, char GroupingSeparator)
{
    /// <summary>Locale <c>vi-VN</c> (what <c>new Locale("vi", "VN")</c> gives in JDK 17): decimal <c>,</c>, grouping <c>.</c>.</summary>
    public static readonly JavaNumberSymbols ViVn = new(',', '.');

    /// <summary>Locale <c>en-US</c>: decimal <c>.</c>, grouping <c>,</c>.</summary>
    public static readonly JavaNumberSymbols EnUs = new('.', ',');
}

/// <summary>
/// Reproduces, with the invariant culture only, the output of the Java number formatting that <c>Util.java</c> uses:
/// <c>NumberFormat.getInstance(vi-VN)</c> / <c>DecimalFormat</c> (<see cref="FormatDouble"/>, <see cref="FormatLong"/>) and <c>String.format("%.nf")</c> (<see cref="FormatFixed"/>).
/// <para>
/// .NET <c>ToString("N1")</c> cannot be used: it rounds on the exact binary value (Java rounds on the digits <c>Double.toString</c> prints, and only uses the exact value to
/// break ties), it has no <c>RoundingMode.DOWN</c>, and for doubles above 2^53 Java prints the exact integer where .NET prints the shortest round-trip digits followed by zeros.
/// This class is a port of <c>java.text.DigitList</c> and of the digit generation of <c>jdk.internal.math.FloatingDecimal</c> (JDK 17).
/// </para>
/// <para>
/// Known limit: for non-integral doubles the digits are the shortest round-trip digits. JDK 17's <c>Double.toString</c> is occasionally one digit longer than the shortest
/// (JDK-4511638, fixed in JDK 19) and prints <c>4.9E-324</c> for <c>Double.MIN_VALUE</c>; such values can differ in the last digit, only visible when more than 15 significant digits are printed.
/// </para>
/// </summary>
public static class JavaNumberFormat
{
    private const char Infinity = '∞';

    // jdk.internal.math.FloatingDecimal.insignificantDigitsNumber
    private static readonly int[] InsignificantDigitsNumber =
    [
        0, 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3,
        4, 4, 4, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7,
        8, 8, 8, 9, 9, 9, 9, 10, 10, 10, 11, 11, 11,
        12, 12, 12, 12, 13, 13, 13, 14, 14, 14,
        15, 15, 15, 15, 16, 16, 16, 17, 17, 17,
        18, 18, 18, 19,
    ];

    /// <summary>
    /// Java <c>DecimalFormat.format(double)</c> for a pattern <c>#,##0.0##</c>-like (grouping of 3, at least one integer digit):
    /// NaN gives <c>NaN</c>, infinities <c>∞</c>/<c>-∞</c>, and a negative value that rounds to zero keeps its sign (<c>-0</c>), as in Java.
    /// </summary>
    /// <param name="value">The number.</param>
    /// <param name="maxFractionDigits">Java <c>setMaximumFractionDigits</c>.</param>
    /// <param name="minFractionDigits">Java <c>setMinimumFractionDigits</c>.</param>
    /// <param name="mode">The rounding mode.</param>
    /// <param name="symbols">The decimal and grouping separators.</param>
    public static string FormatDouble(double value, int maxFractionDigits, int minFractionDigits, JavaRoundingMode mode, JavaNumberSymbols symbols)
    {
        if (double.IsNaN(value))
        {
            return "NaN";
        }

        bool negative = value < 0.0 || (value == 0.0 && double.IsNegative(value));
        if (double.IsInfinity(value))
        {
            return negative ? "-" + Infinity : Infinity.ToString();
        }

        DigitList list = DigitList.Of(Math.Abs(value), maxFractionDigits, mode);
        if (list.Count == 0)
        {
            list.DecimalAt = 0; // Java: a zero digit list is normalised
        }

        var sb = new StringBuilder();
        if (negative)
        {
            sb.Append('-');
        }

        int integerDigits = Math.Max(1, list.DecimalAt);
        int digitIndex = 0;
        for (int i = integerDigits - 1; i >= 0; i--)
        {
            sb.Append(i < list.DecimalAt && digitIndex < list.Count ? list.Digits[digitIndex++] : '0');
            if (i > 0 && i % 3 == 0)
            {
                sb.Append(symbols.GroupingSeparator);
            }
        }

        bool fractionPresent = minFractionDigits > 0 || digitIndex < list.Count;
        if (fractionPresent)
        {
            sb.Append(symbols.DecimalSeparator);
        }

        for (int i = 0; i < maxFractionDigits; i++)
        {
            if (i >= minFractionDigits && digitIndex >= list.Count)
            {
                break;
            }

            if (-1 - i > list.DecimalAt - 1)
            {
                sb.Append('0'); // zeros between the decimal point and the first significant digit
                continue;
            }

            sb.Append(digitIndex < list.Count ? list.Digits[digitIndex++] : '0');
        }

        return sb.ToString();
    }

    /// <summary>Java <c>DecimalFormat.format(long)</c> (grouping of 3, no fraction): <c>-1.234.567</c> for vi-VN. Handles <see cref="long.MinValue"/>.</summary>
    public static string FormatLong(long value, JavaNumberSymbols symbols)
    {
        string digits = value.ToString(CultureInfo.InvariantCulture);
        bool negative = value < 0;
        if (negative)
        {
            digits = digits[1..];
        }

        var sb = new StringBuilder(digits.Length + (digits.Length / 3) + 1);
        if (negative)
        {
            sb.Append('-');
        }

        for (int i = 0; i < digits.Length; i++)
        {
            sb.Append(digits[i]);
            int remaining = digits.Length - 1 - i;
            if (remaining > 0 && remaining % 3 == 0)
            {
                sb.Append(symbols.GroupingSeparator);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Java <c>String.format("%.{precision}f", value)</c> with a locale whose decimal separator is <c>.</c> and no grouping: the digits <c>Double.toString</c> prints are rounded
    /// HALF_UP (so <c>1.005</c> gives <c>1.01</c>, unlike .NET <c>ToString("F2")</c> which gives <c>1.00</c>), the sign of a negative value is always kept (<c>-0.00</c>).
    /// NaN gives <c>NaN</c>, infinities <c>Infinity</c>/<c>-Infinity</c>.
    /// </summary>
    public static string FormatFixed(double value, int precision)
    {
        if (double.IsNaN(value))
        {
            return "NaN";
        }

        bool negative = value < 0.0 || (value == 0.0 && double.IsNegative(value));
        if (double.IsInfinity(value))
        {
            return negative ? "-Infinity" : "Infinity";
        }

        DoubleDigits d = DoubleDigits.Of(Math.Abs(value));
        char[] digits = d.Digits.ToCharArray();
        int decimalAt = d.DecimalAt;
        int keep = decimalAt + precision; // FormattedFloatingDecimal.applyPrecision
        if (digits.Length > 0 && keep >= 0 && keep < digits.Length)
        {
            if (keep == 0)
            {
                bool up = digits[0] >= '5';
                digits = up ? ['1'] : [];
                if (up)
                {
                    decimalAt++;
                }
            }
            else if (digits[keep] >= '5')
            {
                int i = keep - 1;
                while (i >= 0 && digits[i] == '9')
                {
                    i--;
                }

                if (i < 0)
                {
                    digits = ['1'];
                    decimalAt++;
                }
                else
                {
                    digits[i]++;
                    digits = digits[..(i + 1)];
                }
            }
            else
            {
                digits = digits[..keep];
            }
        }

        var sb = new StringBuilder();
        if (negative)
        {
            sb.Append('-');
        }

        if (decimalAt <= 0 || digits.Length == 0)
        {
            sb.Append('0');
        }
        else
        {
            for (int i = 0; i < decimalAt; i++)
            {
                sb.Append(i < digits.Length ? digits[i] : '0');
            }
        }

        if (precision > 0)
        {
            sb.Append('.');
            for (int k = 0; k < precision; k++)
            {
                int index = decimalAt + k;
                sb.Append(digits.Length > 0 && index >= 0 && index < digits.Length ? digits[index] : '0');
            }
        }

        return sb.ToString();
    }

    // Digits of a finite non-negative double as Java's FloatingDecimal produces them: value = 0.DIGITS x 10^DecimalAt, no leading or trailing zeros ("" for 0).
    internal readonly record struct DoubleDigits(string Digits, int DecimalAt)
    {
        internal static DoubleDigits Of(double abs)
        {
            if (abs == 0.0)
            {
                return new DoubleDigits(string.Empty, 0);
            }

            long bits = BitConverter.DoubleToInt64Bits(abs);
            int biased = (int)((bits >> 52) & 0x7FF);
            long fraction = bits & 0xFFFFFFFFFFFFFL;
            if (biased != 0)
            {
                // FloatingDecimal.dtoa "easy case": an integer below 2^63 is printed exactly (minus the digits below the precision of the double).
                long mantissa = fraction | (1L << 52);
                int binExp = biased - 1023;
                int tailZeros = BitOperations.TrailingZeroCount(mantissa);
                int fractionBits = 53 - tailZeros;
                int tinyBits = Math.Max(0, fractionBits - binExp - 1);
                if (binExp <= 62 && tinyBits == 0)
                {
                    long integer = binExp >= 52 ? mantissa << (binExp - 52) : (long)((ulong)mantissa >> (52 - binExp));
                    int insignificant = binExp > 53 ? InsignificantDigitsForPow2(binExp - 53 - 1) : 0;
                    if (insignificant != 0)
                    {
                        long pow10 = 1;
                        for (int i = 0; i < insignificant; i++)
                        {
                            pow10 *= 10;
                        }

                        long residue = integer % pow10;
                        integer /= pow10;
                        if (residue >= (pow10 >> 1))
                        {
                            integer++;
                        }
                    }

                    string text = integer.ToString(CultureInfo.InvariantCulture);
                    return new DoubleDigits(text.TrimEnd('0'), text.Length + insignificant);
                }
            }

            return FromShortest(abs);
        }

        // The shortest digits that round-trip, from .NET's "R" format ("123.456", "1E+23", "1.2345E-05").
        private static DoubleDigits FromShortest(double abs)
        {
            string text = abs.ToString("R", CultureInfo.InvariantCulture);
            int exponent = 0;
            int e = text.IndexOf('E', StringComparison.Ordinal);
            if (e >= 0)
            {
                exponent = int.Parse(text.AsSpan(e + 1), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture);
                text = text[..e];
            }

            int dot = text.IndexOf('.', StringComparison.Ordinal);
            int integerLength = dot >= 0 ? dot : text.Length;
            string all = dot >= 0 ? text.Remove(dot, 1) : text;
            int leadingZeros = 0;
            while (leadingZeros < all.Length && all[leadingZeros] == '0')
            {
                leadingZeros++;
            }

            string digits = all[leadingZeros..].TrimEnd('0');
            return digits.Length == 0 ? new DoubleDigits(string.Empty, 0) : new DoubleDigits(digits, integerLength - leadingZeros + exponent);
        }

        private static int InsignificantDigitsForPow2(int p2) => p2 > 1 && p2 < InsignificantDigitsNumber.Length ? InsignificantDigitsNumber[p2] : 0;
    }

    // Port of java.text.DigitList (fixed point only), the part of DecimalFormat that cuts a number to the maximum fraction digits.
    private sealed class DigitList
    {
        private readonly JavaRoundingMode mode;
        private readonly double source;
        private DoubleDigits exact;

        private DigitList(JavaRoundingMode mode, double source)
        {
            this.mode = mode;
            this.source = source;
            Digits = [];
        }

        internal char[] Digits { get; private set; }

        internal int Count { get; private set; }

        internal int DecimalAt { get; set; }

        internal static DigitList Of(double abs, int maximumFractionDigits, JavaRoundingMode mode)
        {
            var list = new DigitList(mode, abs);
            list.exact = DoubleDigits.Of(abs);
            list.Digits = list.exact.Digits.ToCharArray();
            list.Count = list.Digits.Length;
            list.DecimalAt = list.exact.DecimalAt;
            if (list.Count == 0)
            {
                return list;
            }

            if (-list.DecimalAt > maximumFractionDigits)
            {
                list.Count = 0; // too small: zero
                return list;
            }

            if (-list.DecimalAt == maximumFractionDigits)
            {
                if (list.ShouldRoundUp(0))
                {
                    list.Count = 1;
                    list.DecimalAt++;
                    list.Digits = ['1'];
                }
                else
                {
                    list.Count = 0;
                }

                return list;
            }

            list.Round(maximumFractionDigits + list.DecimalAt);
            return list;
        }

        private void Round(int maximumDigits)
        {
            if (maximumDigits < 0 || maximumDigits >= Count)
            {
                return;
            }

            if (ShouldRoundUp(maximumDigits))
            {
                int position = maximumDigits;
                while (true)
                {
                    position--;
                    if (position < 0)
                    {
                        Digits[0] = '1'; // all nines: one digit, one position higher
                        DecimalAt++;
                        position = 0;
                        break;
                    }

                    Digits[position]++;
                    if (Digits[position] <= '9')
                    {
                        break;
                    }
                }

                maximumDigits = position + 1;
            }

            Count = maximumDigits;
            while (Count > 1 && Digits[Count - 1] == '0')
            {
                Count--;
            }
        }

        // java.text.DigitList.shouldRoundUp for DOWN and HALF_EVEN. For a tie the JDK asks FloatingDecimal whether its digits are exact or were rounded up.
        private bool ShouldRoundUp(int maximumDigits)
        {
            if (mode == JavaRoundingMode.Down || maximumDigits >= Count)
            {
                return false;
            }

            char digit = Digits[maximumDigits];
            if (digit > '5')
            {
                return true;
            }

            if (digit < '5')
            {
                return false;
            }

            if (maximumDigits == Count - 1)
            {
                // the digit is exactly the last one printed for the double: compare the printed value with the exact binary value
                int cmp = CompareDigitsWithExactValue();
                if (cmp > 0)
                {
                    return false; // the printed digits were rounded up: the exact value is below the tie
                }

                if (cmp < 0)
                {
                    return true; // the printed digits were rounded down: the exact value is above the tie
                }

                return maximumDigits > 0 && (Digits[maximumDigits - 1] % 2) != 0;
            }

            for (int i = maximumDigits + 1; i < Count; i++)
            {
                if (Digits[i] != '0')
                {
                    return true;
                }
            }

            return false;
        }

        // Sign of (decimal value of the digits Java printed) - (exact value of the double).
        private int CompareDigitsWithExactValue()
        {
            long bits = BitConverter.DoubleToInt64Bits(source);
            int biased = (int)((bits >> 52) & 0x7FF);
            long fraction = bits & 0xFFFFFFFFFFFFFL;
            BigInteger mantissa = biased == 0 ? fraction : fraction | (1L << 52);
            int binaryExponent = (biased == 0 ? 1 : biased) - 1075; // value = mantissa * 2^binaryExponent

            BigInteger decimalValue = BigInteger.Parse(exact.Digits, CultureInfo.InvariantCulture);
            int decimalExponent = exact.DecimalAt - exact.Digits.Length; // value = decimalValue * 10^decimalExponent

            BigInteger left = mantissa;
            BigInteger right = decimalValue;
            if (binaryExponent >= 0)
            {
                left <<= binaryExponent;
            }
            else
            {
                right <<= -binaryExponent;
            }

            if (decimalExponent >= 0)
            {
                right *= BigInteger.Pow(10, decimalExponent);
            }
            else
            {
                left *= BigInteger.Pow(10, -decimalExponent);
            }

            return right.CompareTo(left);
        }
    }
}
