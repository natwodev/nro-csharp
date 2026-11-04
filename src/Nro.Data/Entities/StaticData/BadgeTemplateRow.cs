using Nro.Data.Entities.StaticData;
using Nro.Data.Entities.Runtime;
using Nro.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu huy hiệu trong hệ thống
// Bảng cơ sở dữ liệu: data_badges (sinh tự động, không sửa tay)
[Table("data_badges")]
public class BadgeTemplateRow
{
    // Khóa chính của bảng BadgeTemplate (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int BadgeTemplateId { get; set; }

    // Mã hiệu ứng huy hiệu
    public int IdEffect { get; set; }

    // Khóa ngoại liên kết với ItemTemplate (vật phẩm huy hiệu trong cửa hàng)
    [ForeignKey("ItemTemplate")]
    public int IdItem { get; set; }

    // Tên huy hiệu
    [Required]
    [Column(TypeName = "text")]
    public string Name { get; set; } = default!;

    // Chỉ số cộng thêm (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Options { get; set; } = default!;

    // Navigation property đến entity ItemTemplateRow
    public ItemTemplateRow ItemTemplate { get; set; } = default!;
}
