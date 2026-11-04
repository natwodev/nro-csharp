namespace Nro.Core.Consts;

// Hằng số tên nhân vật bị cấm: tạo nhân vật trùng một tên trong danh sách sẽ bị báo "Tên nhân vật đã tồn tại"
// Nguồn Java: consts/ConstIgnoreName.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstIgnoreName
{
    // Danh sách tên cấm đặt khi tạo nhân vật; bản Java để trống
    public static readonly string[] IGNORE_NAME = new string[] { };
}
