using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Unused;

// Entity đại diện cho tin nhắn của website cũ trong hệ thống
// Bảng cơ sở dữ liệu: cvh_messages (sinh tự động, không sửa tay)
[Table("cvh_messages")]
public class CvhMessage
{
    // Khóa chính của bảng CvhMessage (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CvhMessageId { get; set; }

    // Tên đăng nhập người gửi
    [Required]
    [MaxLength(255)]
    public string Username { get; set; } = default!;

    // Nội dung tin nhắn
    [Required]
    [Column(TypeName = "text")]
    public string Message { get; set; } = default!;

    // Thời gian gửi (dạng text)
    [Required]
    [MaxLength(999)]
    public string CreatedAt { get; set; } = default!;
}
