using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho nhật ký kích hoạt thành viên trong hệ thống
// Bảng cơ sở dữ liệu: history_active (sinh tự động, không sửa tay)
[Table("history_active")]
public class ActivationHistory
{
    // Khóa chính của bảng ActivationHistory (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int ActivationHistoryId { get; set; }

    // Tên đăng nhập được kích hoạt
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = default!;

    // Thời gian kích hoạt
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }
}
