namespace Nro.Core.Consts;

// Hằng số loại buff toàn máy chủ: id trong bảng attribute_template, giá trị buff tính theo phần trăm
// Nguồn Java: consts/ConstAttribute.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstAttribute
{
    // Tăng % tiềm năng sức mạnh (TNSM) nhận được (Java: TNSM)
    public const int POWER_POTENTIAL = 1;

    // Tăng % vàng rơi từ quái (Java: VANG)
    public const int GOLD = 2;

    // Tăng % HP tối đa
    public const int HP = 4;

    // Tăng % KI tối đa
    public const int KI = 3;

    // Tăng % sức đánh (Java: SUC_DANH)
    public const int DAMAGE = 5;
}
