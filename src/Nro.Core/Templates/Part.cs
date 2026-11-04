namespace Nro.Core.Templates;

// Bộ phận ảnh nhân vật; hàm tạo mặc định luôn khởi tạo danh sách chi tiết nên danh sách không null sau khi tạo đối tượng.
// Nguồn Java: nro/template/Part.java (giữ việc khởi tạo partDetails bằng danh sách rỗng trong hàm tạo mặc định).
public class Part
{
    // Mã bộ phận ảnh (Java: id)
    public int Id { get; set; }

    // Kiểu bộ phận ảnh (Java: type)
    public int Type { get; set; }

    // Danh sách chi tiết của bộ phận, luôn được hàm tạo mặc định khởi tạo (Java: partDetails)
    public List<PartDetail> PartDetails { get; set; }

    // Tạo bộ phận với danh sách chi tiết rỗng.
    public Part()
    {
        PartDetails = new List<PartDetail>();
    }
}
