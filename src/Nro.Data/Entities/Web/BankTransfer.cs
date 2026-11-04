using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Web;

// Entity đại diện cho giao dịch chuyển khoản SePay trong hệ thống
// Bảng cơ sở dữ liệu: vp_bank (sinh tự động, không sửa tay)
[Table("vp_bank")]
public class BankTransfer
{
    // Khóa chính của bảng BankTransfer (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BankTransferId { get; set; }

    // Mã tham chiếu giao dịch SePay
    [Required]
    [MaxLength(100)]
    public string TransactionId { get; set; } = default!;

    // Nội dung chuyển khoản
    [MaxLength(255)]
    public string? Description { get; set; }

    // Số tiền chuyển (VND)
    public int Amount { get; set; }

    // Tên tài khoản được cộng tiền
    [MaxLength(50)]
    public string? Username { get; set; }

    // Trạng thái xử lý (1: đã xử lý)
    public short? Status { get; set; }

    // Thời điểm giao dịch ngân hàng
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? TransferredAt { get; set; }

    // Thời gian tạo dòng (UTC)
    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; }
}
