using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu nhiệm vụ sự kiện hằng ngày trong hệ thống
// Bảng cơ sở dữ liệu: task_event_template (sinh tự động, không sửa tay)
public class TaskEventTemplateRow
{
    // Khóa chính của bảng TaskEventTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int TaskEventTemplateId { get; set; }

    // Tên nhiệm vụ (%1: số lượng)
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;

    // Khoảng số lượng độ khó 1 (dễ)
    [Required]
    [MaxLength(255)]
    public string MaxCountLv1 { get; set; } = default!;

    // Khoảng số lượng độ khó 2 (thường)
    [Required]
    [MaxLength(255)]
    public string MaxCountLv2 { get; set; } = default!;

    // Khoảng số lượng độ khó 3 (khó)
    [Required]
    [MaxLength(255)]
    public string MaxCountLv3 { get; set; } = default!;

    // Khoảng số lượng độ khó 4 (rất khó)
    [Required]
    [MaxLength(255)]
    public string MaxCountLv4 { get; set; } = default!;

    // Khoảng số lượng độ khó 5 (địa ngục)
    [Required]
    [MaxLength(255)]
    public string MaxCountLv5 { get; set; } = default!;
}
