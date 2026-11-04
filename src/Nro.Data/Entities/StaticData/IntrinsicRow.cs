using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu nội tại trong hệ thống
// Bảng cơ sở dữ liệu: intrinsic (sinh tự động, không sửa tay)
[Table("intrinsic")]
public class IntrinsicRow
{
    // Khóa chính của bảng Intrinsic
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int IntrinsicId { get; set; }

    // Mẫu mô tả nội tại (p0..p3)
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;

    // Giá trị nhỏ nhất của tham số 1
    [Column("param_from_1")]
    public int ParamFrom1 { get; set; }

    // Giá trị lớn nhất của tham số 1
    [Column("param_to_1")]
    public int ParamTo1 { get; set; }

    // Giá trị nhỏ nhất của tham số 2
    [Column("param_from_2")]
    public int ParamFrom2 { get; set; }

    // Giá trị lớn nhất của tham số 2
    [Column("param_to_2")]
    public int ParamTo2 { get; set; }

    // Mã biểu tượng
    public int Icon { get; set; }

    // Hành tinh (0: Trái Đất, 1: Namếc, 2: Xayda)
    public short Gender { get; set; }
}
