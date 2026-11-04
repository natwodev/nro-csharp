namespace Nro.Core.Consts;

// Hằng số mẫu số tỉ lệ: tham số typeRatio của Util.isTrue(ratio, typeRatio), xác suất trúng = ratio / typeRatio
// Nguồn Java: consts/ConstRatio.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstRatio
{
    // Mẫu số 100: tỉ lệ tính theo phần trăm
    public const int PER100 = 100;

    // Mẫu số 1.000: tỉ lệ tính theo phần nghìn
    public const int PER1000 = 1000;

    // Mẫu số 10.000: tỉ lệ tính theo phần vạn
    public const int PER10000 = 10000;

    // Mẫu số 100.000
    public const int PER100000 = 100000;

    // Mẫu số 1.000.000: tỉ lệ tính theo phần triệu
    public const int PER1000000 = 1000000;

    // Mẫu số 10.000.000
    public const int PER10000000 = 10000000;

    // Mẫu số 100.000.000
    public const int PER100000000 = 100000000;
}
