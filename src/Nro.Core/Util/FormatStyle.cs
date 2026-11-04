namespace Nro.Core.Util;

// Kiểu hậu tố khi rút gọn số lớn trong Util.FormatNumber; thứ tự thành viên giữ như Java (ordinal 0 và 1)
// Nguồn Java: Utils/FormatStyle.java (tên thành viên giữ nguyên như Java; enum này không được ConstsParityTests đối chiếu)
public enum FormatStyle
{
    // Hậu tố tiếng Việt: Nghìn, Triệu, Tỷ
    VIETNAMESE,

    // Hậu tố chữ cái: K, M, B
    KMB,
}
