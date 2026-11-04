using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu nhiệm vụ KOL trong hệ thống
// Bảng cơ sở dữ liệu: task_kol_template (sinh tự động, không sửa tay)
public class TaskKolTemplateRow
{
    // Khóa chính của bảng TaskKolTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int TaskKolTemplateId { get; set; }

    // Mục tiêu nhiệm vụ (%1: số lượng cần)
    [Required]
    [MaxLength(255)]
    public string Info { get; set; } = default!;

    // Số lượng cần đạt
    [Required]
    [MaxLength(255)]
    public string MaxCount { get; set; } = default!;
}
