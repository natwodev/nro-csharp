using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Runtime;

// Entity đại diện cho lượt nhập mã quà tặng trong hệ thống
// Bảng cơ sở dữ liệu: gift_code_redemption (sinh tự động, không sửa tay)
public class GiftCodeRedemption
{
    // Tài khoản nhập mã
    [Column("id")]
    public int AccountId { get; set; }

    // Khóa ngoại liên kết với Player (người chơi nhập mã)
    [ForeignKey("Player")]
    public int? PlayerId { get; set; }

    // Mã quà tặng đã nhập
    [MaxLength(255)]
    public string? RedeemedCode { get; set; }

    // Thời gian nhập mã
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? RedeemedAt { get; set; }

    // Navigation property đến entity Player
    public Player? Player { get; set; }
}
