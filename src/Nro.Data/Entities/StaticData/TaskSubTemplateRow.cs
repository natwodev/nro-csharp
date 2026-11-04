using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu bước của nhiệm vụ chính trong hệ thống
// Bảng cơ sở dữ liệu: task_sub_template (sinh tự động, không sửa tay)
public class TaskSubTemplateRow
{
    // Khóa ngoại liên kết với TaskMainTemplate (nhiệm vụ chính cha)
    [ForeignKey("TaskMainTemplate")]
    public int TaskMainId { get; set; }

    // Tên bước
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;

    // Số lượng cần để hoàn thành bước
    public int MaxCount { get; set; }

    // Thông báo khi đổi bước
    [Required]
    [MaxLength(255)]
    public string Notify { get; set; } = default!;

    // NPC cần gặp ở bước này
    public int NpcId { get; set; }

    // Map thực hiện bước
    public int Map { get; set; }

    // Khóa chính của bảng TaskSubTemplate (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Idmain { get; set; }

    // Navigation property đến entity TaskMainTemplateRow
    public TaskMainTemplateRow TaskMainTemplate { get; set; } = default!;
}
