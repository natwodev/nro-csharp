using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho mẫu nhiệm vụ KOL trong hệ thống
// Bảng cơ sở dữ liệu: task_kol_template (sinh tự động, không sửa tay)
[Table("task_kol_template")]
public class TaskKolTemplateRow
{
    // Khóa chính của bảng TaskKolTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
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
