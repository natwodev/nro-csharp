using Nro.Data.Entities.StaticData;
using Nro.Data.Entities.Runtime;
using Nro.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu nhiệm vụ bang hội hằng ngày trong hệ thống
// Bảng cơ sở dữ liệu: clan_task_template (sinh tự động, không sửa tay)
[Table("clan_task_template")]
public class ClanTaskTemplateRow
{
    // Khóa chính của bảng ClanTaskTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int ClanTaskTemplateId { get; set; }

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
