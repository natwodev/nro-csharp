using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Archive;

// Entity đại diện cho nhật ký nạp tiền qua ngân hàng trong hệ thống
// Bảng cơ sở dữ liệu: bank_history (sinh tự động, không sửa tay)
public class BankHistory
{
    // Khóa chính của bảng BankHistory (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BankHistoryId { get; set; }

    // Tên đăng nhập của người nạp
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = default!;

    // Số tiền chuyển (VND)
    public double? AmountVnd { get; set; }

    // Số cash được cộng
    public double? AmountCash { get; set; }

    // Nội dung chuyển khoản
    public string? Description { get; set; }

    // Mã giao dịch duy nhất
    [MaxLength(100)]
    public string? Code { get; set; }

    // Thời gian ghi nhận (giờ địa phương)
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? CreatedAt { get; set; }
}
