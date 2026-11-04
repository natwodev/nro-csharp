namespace Nro.Core.Consts;

// Hằng số mã trạng thái tài khoản khi đăng nhập
// Nguồn Java: consts/ConstAccount.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstAccount
{
    // Mặc định, chưa xác định trạng thái (Java: DEFAUT)
    public const sbyte DEFAULT = -1;

    // Tài khoản đang ở trong game
    public const sbyte IS_INGAME = 0;

    // Được phép đăng nhập
    public const sbyte CAN_LOGIN = 1;

    // Sai tên tài khoản hoặc mật khẩu
    public const sbyte WRONG_ACCOUNT = 2;

    // Tài khoản đang bị khóa
    public const sbyte IS_BAN = 3;
}
