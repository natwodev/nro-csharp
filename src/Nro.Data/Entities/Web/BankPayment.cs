using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Web;

// Entity đại diện cho giao dịch nạp qua chuyển khoản ngân hàng trong hệ thống
// Bảng cơ sở dữ liệu: bank_payment (sinh tự động, không sửa tay)
public class BankPayment
{
    // Khóa chính của bảng BankPayment (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BankPaymentId { get; set; }

    // Mã tài khoản người nạp (dạng văn bản)
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;

    // Nội dung chuyển khoản
    [MaxLength(50)]
    public string? Description { get; set; }

    // Số giao dịch ngân hàng
    [MaxLength(255)]
    public string? RefNo { get; set; }

    // Số tiền chuyển (VND)
    public int Amount { get; set; }

    // Trạng thái (văn bản)
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = default!;

    // Thời gian giao dịch (văn bản, UTC+7)
    [Column(TypeName = "text")]
    public string? Date { get; set; }

    // Tên ngân hàng
    [MaxLength(200)]
    public string? Bank { get; set; }
}
