using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Web;

// Entity đại diện cho nhật ký kích hoạt thành viên trong hệ thống
// Bảng cơ sở dữ liệu: activation_history (sinh tự động, không sửa tay)
public class ActivationHistory
{
    // Khóa chính của bảng ActivationHistory (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ActivationHistoryId { get; set; }

    // Tên đăng nhập được kích hoạt
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = default!;

    // Thời gian kích hoạt
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }
}
