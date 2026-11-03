using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho lớp nhân vật hoặc tên hành tinh trong hệ thống
// Bảng cơ sở dữ liệu: nclass (sinh tự động, không sửa tay)
[Table("nclass")]
public class NClass
{
    // Khóa chính của bảng NClass
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int NClassId { get; set; }

    // Tên lớp
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = default!;
}
