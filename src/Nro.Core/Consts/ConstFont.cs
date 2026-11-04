namespace Nro.Core.Consts;

// Hằng số định dạng dòng chữ: mã font "|n|" và căn lề mà máy chủ đặt đầu dòng gửi cho client; dòng không có mã dùng lại font của dòng trước
// Nguồn Java: consts/ConstFont.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstFont
{
    // Dòng bắt đầu bằng "--" được client vẽ thành một đường kẻ ngang
    public const string LINE = "--";

    // Chữ thường tahoma_7 (mã -1)
    public const string DEFAULT = "|-1|";

    // Chữ đậm màu tối (mã 0)
    public const string BOLD_DARK = "|0|";

    // Chữ đậm xanh lá (mã 1)
    public const string BOLD_GREEN = "|1|";

    // Chữ đậm xanh dương (mã 2)
    public const string BOLD_BLUE = "|2|";

    // Chữ đỏ thường (mã 3)
    public const string RED = "|3|";

    // Chữ xanh lá thường (mã 4)
    public const string GREEN = "|4|";

    // Chữ xanh dương thường (mã 5)
    public const string BLUE = "|5|";

    // Chữ đậm đỏ (mã 7)
    public const string BOLD_RED = "|7|";

    // Chữ đậm vàng (mã 8)
    public const string BOLD_YELLOW = "|8|";

    // Ghép sau mã màu để căn giữa dòng ("|n|2|"); client vốn mặc định căn giữa
    public const string CENTER = "2|";

    // Ghép sau mã màu để căn phải dòng ("|n|1|")
    public const string RIGHT = "1|";
}
