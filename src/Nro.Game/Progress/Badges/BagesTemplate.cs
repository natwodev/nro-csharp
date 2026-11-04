namespace Nro.Game.Progress.Badges;

// Template huy hiệu giữ nguyên cấu trúc dữ liệu Java; tên BagesTemplate là tên hiện có cần bảo toàn.
// Nguồn Java: nro/badges/BagesTemplate.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).






public class BagesTemplate
{
    // Mã định danh.
    public int Id { get; set; }

    // Mã hiệu ứng mà huy hiệu kích hoạt.
    public int IdEffect { get; set; }

    // Mã vật phẩm liên kết với hiệu ứng huy hiệu.
    public int IdItem { get; set; }


    // Tên hiển thị, giữ nguyên tên thuộc tính viết hoa NAME của Java.
    public string NAME { get; set; } = "";

    // Các tùy chọn vật phẩm do huy hiệu cấp.
    public List<ItemOption> Options { get; set; } = [];
}
