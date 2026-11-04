using Nro.Data.Db;
using System.Globalization;

namespace Nro.Data.Db;

// Định dạng giá trị DB theo String.valueOf(Object) của Java để IResultSnapshot.GetString trả chuỗi tương thích.
// Nguồn Java: java.lang.String.valueOf(Object) (giữ true/null, ngày giờ và quy tắc số kiểu Java; float/double lớn có thể khác JDK 17 vì C# dùng chữ số ngắn nhất khôi phục được giá trị).
internal static class JavaStringFormat
{
    // Đổi giá trị trả về từ DB thành chuỗi theo quy tắc Java (Java: String.valueOf).
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

    // Định dạng float/double kiểu Java: số thập phân trong [10^-3, 10^7), ngoài khoảng dùng dạng khoa học.
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

        // Tách số mũ: "1.2345E+17", "1E-05" hoặc "123.45".
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

        // Giá trị = 0.CHỮ_SỐ x 10^vị_trí_dấu_phẩy; bỏ số 0 đầu/cuối.
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

    // Định dạng decimal kiểu BigDecimal.toString, giữ số 0 cuối và dùng dạng khoa học khi số mũ điều chỉnh nhỏ hơn -6.
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

    // Định dạng DateTime kiểu Timestamp.toString: luôn có một chữ số phần lẻ giây, bỏ các số 0 cuối.
    private static string TimestampToString(DateTime time)
    {
        var fraction = time.Ticks % TimeSpan.TicksPerSecond;
        var digits = fraction == 0 ? "0" : fraction.ToString("D7", CultureInfo.InvariantCulture).TrimEnd('0');
        return time.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + "." + digits;
    }
}
