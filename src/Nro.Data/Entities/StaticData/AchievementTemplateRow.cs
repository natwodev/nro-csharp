using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu thành tựu trong hệ thống
// Bảng cơ sở dữ liệu: achievement_template (sinh tự động, không sửa tay)
public class AchievementTemplateRow
{
    // Khóa chính của bảng AchievementTemplate (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AchievementTemplateId { get; set; }

    // Tên thành tựu
    [Required]
    public string Info1 { get; set; } = default!;

    // Mô tả điều kiện
    [Required]
    public string Info2 { get; set; } = default!;

    // Số thưởng (hồng ngọc và thỏi vàng)
    public int Money { get; set; }

    // Ngưỡng hoàn thành
    public long MaxCount { get; set; }
}
