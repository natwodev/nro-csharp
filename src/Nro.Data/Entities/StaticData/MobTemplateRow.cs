using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho mẫu quái trong hệ thống
// Bảng cơ sở dữ liệu: mob_template (sinh tự động, không sửa tay)
[Table("mob_template")]
public class MobTemplateRow
{
    // Khóa chính của bảng MobTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int MobTemplateId { get; set; }

    // Loại hành vi của quái
    public int Type { get; set; }

    // Tên quái
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = default!;

    // HP của mẫu quái
    public int Hp { get; set; }

    // Phạm vi di chuyển
    public short RangeMove { get; set; }

    // Tốc độ di chuyển
    public short Speed { get; set; }

    // Loại đạn bắn
    public short DartType { get; set; }

    // Sức đánh (% HP tối đa)
    public short PercentDame { get; set; }

    // Tiềm năng thưởng (% HP)
    public short PercentTiemNang { get; set; }

    // Vàng rơi thêm
    public short PercentGold { get; set; }
}
