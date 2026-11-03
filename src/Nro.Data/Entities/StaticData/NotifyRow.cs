using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho thông báo gửi đến client trong hệ thống
// Bảng cơ sở dữ liệu: notify (sinh tự động, không sửa tay)
[Table("notify")]
public class NotifyRow
{
    // Khóa chính của bảng Notify (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int NotifyId { get; set; }

    // Tiêu đề thông báo
    [Required]
    [Column(TypeName = "text")]
    public string Name { get; set; } = default!;

    // Nội dung thông báo
    [Required]
    [Column(TypeName = "text")]
    public string Text { get; set; } = default!;
}
