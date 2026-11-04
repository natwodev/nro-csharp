using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu nhiệm vụ chính trong hệ thống
// Bảng cơ sở dữ liệu: task_main_template (sinh tự động, không sửa tay)
[Table("task_main_template")]
public class TaskMainTemplateRow
{
    // Khóa chính của bảng TaskMainTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int TaskMainTemplateId { get; set; }

    // Tên nhiệm vụ chính
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;

    // Mô tả nhiệm vụ (nhiều dòng)
    [Required]
    [MaxLength(500)]
    public string Detail { get; set; } = default!;

    // Collection các bản ghi TaskSubTemplateRow thuộc TaskMainTemplate này
    // Mối quan hệ one-to-many với TaskSubTemplateRow
    public ICollection<TaskSubTemplateRow> TaskSubTemplates { get; set; } = new List<TaskSubTemplateRow>();
}
