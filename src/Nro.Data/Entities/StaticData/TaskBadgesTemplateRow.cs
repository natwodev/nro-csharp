using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu nhiệm vụ huy hiệu trong hệ thống
// Bảng cơ sở dữ liệu: task_badges_template (sinh tự động, không sửa tay)
public class TaskBadgesTemplateRow
{
    // Khóa chính của bảng TaskBadgesTemplate (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TaskBadgesTemplateId { get; set; }

    // Tên điều kiện
    [MaxLength(255)]
    public string? Name { get; set; }

    // Số lượng cần để hoàn thành
    public int MaxCount { get; set; }

    // Mã hiệu ứng huy hiệu thưởng
    public int IdBadgesReward { get; set; }
}
