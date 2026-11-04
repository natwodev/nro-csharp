using System.Globalization;
using System.Numerics;
using System.Text;

using Nro.Core.Enums;

namespace Nro.Core.Util;

// Hai ký hiệu của DecimalFormatSymbols trong Java có ảnh hưởng ở đây; ghi rõ ra để kết quả không bao giờ phụ thuộc văn hóa (culture) của máy
// Nguồn Java: java.text.DecimalFormatSymbols (chỉ dấu thập phân và dấu ngăn cách hàng nghìn)
// DecimalSeparator: dấu thập phân; GroupingSeparator: dấu ngăn cách hàng nghìn
public readonly record struct JavaNumberSymbols(char DecimalSeparator, char GroupingSeparator)
{
    // Locale vi-VN (đúng như new Locale("vi", "VN") cho ra trong JDK 17): dấu thập phân ',', dấu hàng nghìn '.'
    public static readonly JavaNumberSymbols ViVn = new(',', '.');

    // Locale en-US: dấu thập phân '.', dấu hàng nghìn ','
    public static readonly JavaNumberSymbols EnUs = new('.', ',');
}

// Tái tạo đúng kết quả định dạng số của Java mà Util.java dùng, chỉ với InvariantCulture: FormatDouble/FormatLong như DecimalFormat, FormatFixed như String.format("%.nf")
// Nguồn Java: NumberFormat.getInstance(vi-VN) / DecimalFormat và String.format("%.nf"); port java.text.DigitList và phần sinh chữ số của jdk.internal.math.FloatingDecimal (JDK 17)
// Không dùng được ToString("N1") của .NET: .NET làm tròn trên giá trị nhị phân chính xác (Java làm tròn trên dãy chữ số Double.toString in ra, chỉ dùng giá trị
// chính xác để phân xử khi đúng nửa), .NET không có RoundingMode.DOWN, và với double trên 2^53 Java in đúng số nguyên chính xác còn .NET in dãy chữ số
// ngắn nhất đủ khôi phục giá trị (round-trip) rồi thêm các số 0.
// Giới hạn đã biết: với double không nguyên, chữ số là dãy ngắn nhất đủ khôi phục giá trị. Double.toString của JDK 17 đôi khi dài hơn dãy ngắn nhất một chữ số
// (JDK-4511638, sửa ở JDK 19) và in 4.9E-324 cho Double.MIN_VALUE; các giá trị đó có thể khác ở chữ số cuối, chỉ thấy được khi in quá 15 chữ số có nghĩa.
public static class JavaNumberFormat
{
    // Ký hiệu vô cực DecimalFormat in ra (Java: DecimalFormatSymbols.getInfinity)
    private const char Infinity = '∞';

    // Số chữ số thập phân không có nghĩa ứng với từng lũy thừa của 2, chép nguyên từ Java (Java: jdk.internal.math.FloatingDecimal.insignificantDigitsNumber)
    private static readonly int[] InsignificantDigitsNumber =
    [
        0, 0, 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3,
        4, 4, 4, 5, 5, 5, 6, 6, 6, 6, 7, 7, 7,
        8, 8, 8, 9, 9, 9, 9, 10, 10, 10, 11, 11, 11,
        12, 12, 12, 12, 13, 13, 13, 14, 14, 14,
        15, 15, 15, 15, 16, 16, 16, 17, 17, 17,
        18, 18, 18, 19,
    ];

    // Định dạng double với mẫu kiểu #,##0.0## (nhóm 3 chữ số, ít nhất một chữ số phần nguyên): NaN cho NaN, vô cực cho ∞/-∞,
    // số âm làm tròn về 0 vẫn giữ dấu (-0) như Java. maxFractionDigits/minFractionDigits là setMaximumFractionDigits/setMinimumFractionDigits của Java,
    // mode là kiểu làm tròn, symbols là dấu thập phân và dấu hàng nghìn (Java: DecimalFormat.format(double))
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
            list.DecimalAt = 0; // Java: dãy chữ số rỗng (số 0) được chuẩn hóa
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
                sb.Append('0'); // các số 0 nằm giữa dấu thập phân và chữ số có nghĩa đầu tiên
                continue;
            }

            sb.Append(digitIndex < list.Count ? list.Digits[digitIndex++] : '0');
        }

        return sb.ToString();
    }

    // Định dạng số nguyên, nhóm 3 chữ số, không có phần thập phân: -1.234.567 với vi-VN; xử lý được cả long.MinValue (Java: DecimalFormat.format(long))
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

    // Định dạng với đúng precision chữ số thập phân, locale có dấu thập phân '.' và không nhóm hàng nghìn: dãy chữ số Double.toString in ra được làm tròn
    // HALF_UP (nên 1.005 cho 1.01, khác ToString("F2") của .NET cho 1.00), số âm luôn giữ dấu (-0.00); NaN cho NaN, vô cực cho Infinity/-Infinity
    // (Java: String.format("%.{precision}f", value))
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
        int keep = decimalAt + precision; // số chữ số giữ lại (Java: FormattedFloatingDecimal.applyPrecision)
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

    // Dãy chữ số của một double hữu hạn không âm đúng như FloatingDecimal của Java sinh ra: giá trị = 0.DIGITS x 10^DecimalAt, không có số 0 ở đầu hay cuối ("" với số 0)
    // Nguồn Java: jdk.internal.math.FloatingDecimal.BinaryToASCIIBuffer (dtoa, JDK 17)
    internal readonly record struct DoubleDigits(string Digits, int DecimalAt)
    {
        // Lấy dãy chữ số của abs: số nguyên dưới 2^63 in chính xác, còn lại lấy dãy ngắn nhất đủ khôi phục giá trị (Java: FloatingDecimal.getBinaryToASCIIConverter)
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
                // "Trường hợp dễ" của FloatingDecimal.dtoa: số nguyên dưới 2^63 được in chính xác (trừ các chữ số nằm dưới độ chính xác của double).
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

        // Dãy chữ số ngắn nhất đủ khôi phục giá trị, lấy từ định dạng "R" của .NET ("123.456", "1E+23", "1.2345E-05").
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

        // Số chữ số không có nghĩa của lũy thừa 2^p2, 0 nếu nằm ngoài bảng (Java: FloatingDecimal.insignificantDigitsForPow2)
        private static int InsignificantDigitsForPow2(int p2) => p2 > 1 && p2 < InsignificantDigitsNumber.Length ? InsignificantDigitsNumber[p2] : 0;
    }

    // Phần của DecimalFormat cắt một số về số chữ số thập phân tối đa
    // Nguồn Java: java.text.DigitList (chỉ phần dấu chấm cố định - fixed point)
    private sealed class DigitList
    {
        // Kiểu làm tròn
        private readonly JavaRoundingMode mode;
        // Giá trị double gốc (không âm), dùng để so với giá trị nhị phân chính xác khi gặp trường hợp đúng nửa
        private readonly double source;
        // Dãy chữ số Java in ra cho source, trước khi làm tròn
        private DoubleDigits exact;

        // Chỉ tạo qua Of; Digits ban đầu rỗng
        private DigitList(JavaRoundingMode mode, double source)
        {
            this.mode = mode;
            this.source = source;
            Digits = [];
        }

        // Các chữ số dạng ký tự '0'-'9', chỉ Count phần tử đầu có nghĩa (Java: DigitList.digits)
        internal char[] Digits { get; private set; }

        // Số chữ số đang dùng (Java: DigitList.count)
        internal int Count { get; private set; }

        // Vị trí dấu thập phân: giá trị = 0.Digits x 10^DecimalAt (Java: DigitList.decimalAt)
        internal int DecimalAt { get; set; }

        // Tạo dãy chữ số cho abs rồi cắt về maximumFractionDigits chữ số thập phân theo mode (Java: DigitList.set(boolean, double, int, boolean) với fixedPoint = true)
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
                list.Count = 0; // quá nhỏ: thành 0
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

        // Cắt dãy về maximumDigits chữ số, làm tròn lên nếu cần rồi bỏ các số 0 thừa ở cuối (Java: DigitList.round)
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
                        Digits[0] = '1'; // toàn số 9: chỉ còn một chữ số, lên cao hơn một bậc
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

        // Có làm tròn lên khi cắt ở vị trí maximumDigits không, cho DOWN và HALF_EVEN; khi đúng nửa thì JDK hỏi FloatingDecimal xem dãy chữ số
        // là chính xác hay đã được làm tròn lên (Java: java.text.DigitList.shouldRoundUp)
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
                // chữ số này đúng là chữ số cuối Java in cho double: so giá trị đã in với giá trị nhị phân chính xác
                int cmp = CompareDigitsWithExactValue();
                if (cmp > 0)
                {
                    return false; // dãy đã in bị làm tròn lên: giá trị chính xác nằm dưới điểm giữa
                }

                if (cmp < 0)
                {
                    return true; // dãy đã in bị làm tròn xuống: giá trị chính xác nằm trên điểm giữa
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

        // Dấu của (giá trị thập phân của dãy chữ số Java in ra) - (giá trị chính xác của double)
        // (thay cho digitsRoundedUp()/decimalDigitsExact() của FloatingDecimal trong Java)
        private int CompareDigitsWithExactValue()
        {
            long bits = BitConverter.DoubleToInt64Bits(source);
            int biased = (int)((bits >> 52) & 0x7FF);
            long fraction = bits & 0xFFFFFFFFFFFFFL;
            BigInteger mantissa = biased == 0 ? fraction : fraction | (1L << 52);
            int binaryExponent = (biased == 0 ? 1 : biased) - 1075; // giá trị = mantissa * 2^binaryExponent

            BigInteger decimalValue = BigInteger.Parse(exact.Digits, CultureInfo.InvariantCulture);
            int decimalExponent = exact.DecimalAt - exact.Digits.Length; // giá trị = decimalValue * 10^decimalExponent

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
