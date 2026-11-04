using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Web;

// Entity đại diện cho nhật ký đổi VND sang thỏi vàng trong hệ thống
// Bảng cơ sở dữ liệu: exchange_history (sinh tự động, không sửa tay)
public class ExchangeHistory
{
    // Khóa chính của bảng ExchangeHistory (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ExchangeHistoryId { get; set; }

    // Tên đăng nhập tài khoản
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = default!;

    // Số VND đã dùng
    public long AmountVnd { get; set; }

    // Số thỏi vàng nhận được
    public long GoldReceived { get; set; }

    // Tỷ lệ quy đổi
    [Column(TypeName = "numeric(5,2)")]
    public decimal Rate { get; set; }

    // Thời gian đổi (giờ địa phương)
    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; }
}
