using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu kỹ năng trong hệ thống
// Bảng cơ sở dữ liệu: skill_template (sinh tự động, không sửa tay)
public class SkillTemplateRow
{
    // Một phần khóa chính ghép của bảng SkillTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column(Order = 0)]
    public int NclassId { get; set; }

    // Một phần khóa chính ghép của bảng SkillTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id", Order = 1)]
    public int SkillTemplateId { get; set; }

    // Tên kỹ năng
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = default!;

    // Cấp kỹ năng tối đa
    public short MaxPoint { get; set; }

    // Kiểu tiêu hao ki (0, 1, 2)
    public short ManaUseType { get; set; }

    // Loại kỹ năng (1: tấn công, 2: buff, 3: tự thân, 4: đặc biệt)
    public short Type { get; set; }

    // Mã biểu tượng kỹ năng
    public int IconId { get; set; }

    // Mô tả hiệu ứng (# là giá trị)
    [Required]
    [MaxLength(255)]
    public string DamInfo { get; set; } = default!;

    // Thứ tự hiển thị trong lớp
    public int Slot { get; set; }

    // Chỉ số kỹ năng theo cấp (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Skills { get; set; } = default!;
}
