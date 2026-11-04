using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Web;

// Entity đại diện cho giao dịch nạp thẻ cào trong hệ thống
// Bảng cơ sở dữ liệu: napthe (sinh tự động, không sửa tay)
[Table("napthe")]
public class CardTopup
{
    // Khóa chính của bảng CardTopup (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CardTopupId { get; set; }

    // Tên đăng nhập của người nạp
    [Required]
    [MaxLength(100)]
    public string TopUpUsername { get; set; } = default!;

    // Nhà mạng hoặc loại thẻ
    [Required]
    [MaxLength(255)]
    public string Telco { get; set; } = default!;

    // Số sê-ri thẻ
    [Required]
    [MaxLength(255)]
    public string Serial { get; set; } = default!;

    // Mã thẻ
    [Required]
    [MaxLength(255)]
    public string Code { get; set; } = default!;

    // Mệnh giá thẻ (VND)
    public int Amount { get; set; }

    // Trạng thái thẻ (99: chờ, 1: xong, 0: chưa xử lý)
    public int Status { get; set; }

    // Thời gian gửi (UTC)
    [Column(TypeName = "timestamp with time zone")]
    public DateTime? CreatedAt { get; set; }

    // Mã yêu cầu của cổng thanh toán
    [Required]
    [MaxLength(50)]
    public string RequestId { get; set; } = default!;
}
