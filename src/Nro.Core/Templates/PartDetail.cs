namespace Nro.Core.Templates;

// Lưu chi tiết hình ảnh và độ lệch của một phần nhân vật.
// Nguồn Java: nro/template/PartDetail.java (giữ dx và dy là sbyte có dấu để biểu diễn độ lệch âm; Java chỉ có hàm tạo ba tham số, không có hàm tạo mặc định).
public class PartDetail
{
    // Mã biểu tượng của phần (Java: iconId)
    public short IconId { get; set; }

    // Độ lệch ngang có dấu (Java: dx)
    public sbyte Dx { get; set; }

    // Độ lệch dọc có dấu (Java: dy)
    public sbyte Dy { get; set; }

    public PartDetail(short iconId, sbyte dx, sbyte dy)
    {
        IconId = iconId;
        Dx = dx;
        Dy = dy;
    }
}
