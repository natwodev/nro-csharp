namespace Nro.Core.Templates;

// Dữ liệu cờ đeo sau lưng nhân vật.
// Nguồn Java: nro/template/FlagBag.java (C# dùng chuỗi rỗng và mảng rỗng thay cho name và iconEffect Java ban đầu null, vì mã Java không kiểm tra null trước khi dùng).
public class FlagBag
{
    // Mã cờ đeo sau lưng (Java: id)
    public int Id { get; set; }

    // Mã biểu tượng của cờ (Java: iconId)
    public short IconId { get; set; }

    // Danh sách hiệu ứng biểu tượng, mặc định là mảng rỗng thay vì null của Java (Java: iconEffect)
    public short[] IconEffect { get; set; } = [];

    // Tên cờ, mặc định là chuỗi rỗng thay vì null của Java (Java: name)
    public string Name { get; set; } = "";

    // Giá mua bằng vàng (Java: gold)
    public int Gold { get; set; }

    // Giá mua bằng ngọc (Java: gem)
    public int Gem { get; set; }
}
