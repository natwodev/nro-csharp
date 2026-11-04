namespace Nro.Core.Consts;

// Hằng số đạo lữ (người yêu đi theo như đệ tử): cấp bậc và tu vi tối đa, hạng đạo lữ; bản Java chỉ dùng trong NguoiYeuService đã bị comment
// Nguồn Java: consts/ConstFamily.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstFamily
{
    // Cấp bậc đạo lữ tối đa (suy từ tên) (Java: MAX_CAP_BAC_DAO_LU)
    public const sbyte MAX_PARTNER_RANK = 10;

    // Tu vi đạo lữ tối đa (suy từ tên) (Java: MAX_TU_VI_DAO_LU)
    public const int MAX_PARTNER_CULTIVATION = 1000;

    // Đạo lữ hạng 1: chỉ số khởi tạo thấp nhất (Java: DAO_LU_TYPE_1)
    public const sbyte PARTNER_TYPE_1 = 0;

    // Đạo lữ hạng 2: sức đánh khởi tạo cao hơn hạng 1 (Java: DAO_LU_TYPE_2)
    public const sbyte PARTNER_TYPE_2 = 1;

    // Đạo lữ hạng 3: HP, KI và sức đánh khởi tạo cao nhất (Java: DAO_LU_TYPE_3)
    public const sbyte PARTNER_TYPE_3 = 2;
}
