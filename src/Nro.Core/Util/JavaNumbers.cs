using System.Globalization;

namespace Nro.Core.Util;

// Tái tạo các phép tính số của Java mà C# làm khác: ép kiểu số thực sang int/long, Math.round và %0Nd của String.format
// Nguồn Java: phép ép (int)/(long) double của ngôn ngữ, java.lang.Math.round(double), java.util.Formatter (%0Nd)
// Ép kiểu thì bão hòa ở giá trị nhỏ/lớn nhất, NaN thành 0; Math.round làm tròn nửa LÊN, cũng bão hòa; %0Nd tính cả dấu trừ vào độ rộng
public static class JavaNumbers
{
    // Ép double sang int như Java: cắt phần lẻ về phía 0, bão hòa ở int.MinValue/int.MaxValue, NaN cho 0 (Java: (int) double)
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

    // Ép double sang long như Java: cắt phần lẻ về phía 0, bão hòa ở long.MinValue/long.MaxValue, NaN cho 0 (Java: (long) double)
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

    // Số long gần nhất, đúng nửa thì làm tròn về phía dương vô cực (round(-2.5) == -2), NaN cho 0, bão hòa (Java: Math.round(double))
    // Không dùng Math.Round(double) vì nó làm tròn nửa về số chẵn, còn Math.Floor(x + 0.5) sai với 0.49999999999999994
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

    // Chữ số thập phân thêm số 0 bên trái cho đủ width ký tự, TÍNH CẢ dấu trừ (Java: String.format("%0{width}d", value))
    // (-1 với width 2 vẫn là -1, với width 3 thành -01; ToString("D2") của .NET sẽ cho -01 với width 2)
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
