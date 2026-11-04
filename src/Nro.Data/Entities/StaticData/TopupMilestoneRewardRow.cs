using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho cấu hình thưởng mốc nạp trong hệ thống
// Bảng cơ sở dữ liệu: top_up_milestone_reward (sinh tự động, không sửa tay)
public class TopUpMilestoneRewardRow
{
    // Khóa chính của bảng TopUpMilestoneReward (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TopUpMilestoneRewardId { get; set; }

    // Tài khoản người dùng
    public int UserId { get; set; }

    // Số tiền nạp
    public long Amount { get; set; }

    // Vật phẩm thưởng của mốc (mảng JSON)
    [JsonText]
    public string? RewardJson { get; set; }

    // Thời gian tạo (UTC)
    public DateTime CreatedAt { get; set; }
}
