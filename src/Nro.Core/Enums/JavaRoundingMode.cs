namespace Nro.Core.Enums;

// Các kiểu làm tròn mà Util.java dùng với java.text.DecimalFormat: RoundingMode.DOWN (đặt tường minh) và HALF_EVEN (mặc định)
// Nguồn Java: java.math.RoundingMode (chỉ hai giá trị Util.java dùng tới)
public enum JavaRoundingMode
{
    // Cắt bỏ các chữ số thập phân thừa trong dãy chữ số Java in ra cho double, không làm tròn (Java: RoundingMode.DOWN)
    Down,

    // Làm tròn về số gần nhất, đúng nửa thì về chữ số chẵn; là mặc định của DecimalFormat (Java: RoundingMode.HALF_EVEN)
    HalfEven,
}
