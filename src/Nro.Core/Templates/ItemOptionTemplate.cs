namespace Nro.Core.Templates;

// Mẫu tùy chọn vật phẩm; giữ hàm tạo mặc định để bộ nạp gán từng trường và hàm tạo ba tham số như Java.
// Nguồn Java: nro/template/ItemOptionTemplate.java (tên mặc định chuỗi rỗng thay cho null của Java).
public class ItemOptionTemplate
{
    // Mã tùy chọn vật phẩm (Java: id)
    public int Id { get; set; }

    // Tên tùy chọn, mặc định chuỗi rỗng thay cho null của Java (Java: name)
    public string Name { get; set; } = "";

    // Kiểu tùy chọn vật phẩm (Java: type)
    public int Type { get; set; }

    // Tạo mẫu trống để bộ nạp gán từng trường.
    public ItemOptionTemplate()
    {
    }

    // Tạo mẫu từ mã, tên và kiểu.
    public ItemOptionTemplate(int id, string name, int type)
    {
        Id = id;
        Name = name;
        Type = type;
    }
}
