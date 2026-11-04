namespace Nro.Core.Templates;

// Vật trang trí nền của bản đồ.
// Nguồn Java: nro/template/BgItem.java (bảo toàn byte layer có dấu thành sbyte; idImage, dx và dy là short).
public class BgItem
{
    // Mã vật trang trí nền (Java: id)
    public int Id { get; set; }

    // Mã hình ảnh nền (Java: idImage)
    public short IdImage { get; set; }

    // Độ lệch ngang của hình ảnh (Java: dx)
    public short Dx { get; set; }

    // Độ lệch dọc của hình ảnh (Java: dy)
    public short Dy { get; set; }

    // Lớp hiển thị nền, dùng số nguyên 8 bit có dấu (Java: layer)
    public sbyte Layer { get; set; }
}
