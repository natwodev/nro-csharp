using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu vật trang trí nền bản đồ trong hệ thống
// Bảng cơ sở dữ liệu: bg_item_template (sinh tự động, không sửa tay)
[Table("bg_item_template")]
public class BgItemTemplateRow
{
    // Khóa chính của bảng BgItemTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int BgItemTemplateId { get; set; }

    // Hình ảnh trang trí
    public int ImageId { get; set; }

    // Lớp vẽ (1..4)
    public int Layer { get; set; }

    // Độ lệch vẽ theo chiều ngang
    public int Dx { get; set; }

    // Độ lệch vẽ theo chiều dọc
    public int Dy { get; set; }
}
