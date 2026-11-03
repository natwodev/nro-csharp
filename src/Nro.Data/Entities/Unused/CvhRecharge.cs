using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho lượt nạp thẻ trong hệ thống
// Bảng cơ sở dữ liệu: cvh_recharge (sinh tự động, không sửa tay)
public class CvhRecharge
{
    // Khóa chính của bảng CvhRecharge (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int CvhRechargeId { get; set; }

    // Tài khoản nạp thẻ (dạng text)
    [Required]
    [MaxLength(999)]
    public string AccountId { get; set; } = default!;

    // Mã thẻ (PIN)
    [Required]
    [MaxLength(999)]
    public string Code { get; set; } = default!;

    // Số sê-ri thẻ
    [Required]
    [MaxLength(999)]
    public string Serial { get; set; } = default!;

    // Mệnh giá thẻ (dạng text)
    [Required]
    [MaxLength(999)]
    public string Amount { get; set; } = default!;

    // Loại thẻ hoặc nhà mạng
    [Required]
    [MaxLength(999)]
    public string Type { get; set; } = default!;

    // Mã giao dịch của cổng nạp
    [Required]
    [MaxLength(999)]
    public string Tranid { get; set; } = default!;

    // Số tiền thực nhận (dạng text)
    [Required]
    [MaxLength(999)]
    public string AmountReal { get; set; } = default!;

    // Trạng thái xử lý
    public int Status { get; set; }

    // Thời gian nạp (dạng text)
    [Required]
    [MaxLength(999)]
    public string Time { get; set; } = default!;
}
