namespace Nro.Core.Consts;

// Hằng số loại đệ tử: giá trị typeDeTu của đệ tử, quyết định ngoại hình (đầu, avatar), chỉ số khởi tạo và khả năng biến hình
// Nguồn Java: consts/ConstDetu.java (tên lớp và tên hằng đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstPet
{
    // Đệ tử thường, ngoại hình theo hành tinh
    public const sbyte NORMAL = 0;

    // Đệ tử Mabư (đầu và avatar 297), biến hình được
    public const sbyte MABU = 1;

    // Đệ tử mới Ubu (đầu và avatar 946), biến hình được
    public const sbyte U_BU = 2;

    // Đệ tử mới Kid Jiren (đầu và avatar 876), biến hình được
    public const sbyte KID_JIREN = 3;

    // Đệ tử mới Kid Beer, tức Beerus nhí (đầu và avatar 1422), biến hình được
    public const sbyte KID_BEER = 4;

    // Đệ tử Black Goku Rose (đầu và avatar 1914), không biến hình
    public const sbyte BLACK = 5;
}
