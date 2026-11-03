using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho cấu hình thưởng mốc nạp trong hệ thống
// Bảng cơ sở dữ liệu: moc_nap (sinh tự động, không sửa tay)
[Table("moc_nap")]
public class TopupMilestoneRewardRow
{
    // Khóa chính của bảng TopupMilestoneReward (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int TopupMilestoneRewardId { get; set; }

    // Tài khoản người dùng
    public int UserId { get; set; }

    // Số tiền nạp
    public long Amount { get; set; }

    // Vật phẩm thưởng của mốc (mảng JSON)
    [JsonText]
    [Column(TypeName = "text")]
    public string? RewardJson { get; set; }

    // Thời gian tạo (UTC)
    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; }
}
