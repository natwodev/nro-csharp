namespace Nro.Core.Templates;

// Danh sách các frame dùng cho đầu nhân vật.
// Nguồn Java: nro/template/ArrHead2Frames.java (khởi tạo danh sách rỗng ngay tại khai báo như new ArrayList(), nên không null sau khi tạo đối tượng).
public class ArrHead2Frames
{
    // Danh sách mã frame của đầu nhân vật (Java: frames)
    public List<int> Frames { get; set; } = new();
}
