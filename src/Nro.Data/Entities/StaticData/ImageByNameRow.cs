using Nro.Data.Entities.StaticData;
using Nro.Data.Entities.Runtime;
using Nro.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho hình ảnh theo tên trong hệ thống
// Bảng cơ sở dữ liệu: img_by_name (sinh tự động, không sửa tay)
[Table("img_by_name")]
public class ImageByNameRow
{
    // Khóa chính của bảng ImageByName (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int ImageByNameId { get; set; }

    // Tên hình ảnh
    [Required]
    [MaxLength(55)]
    [Column(TypeName = "citext")]
    public string Name { get; set; } = default!;

    // Số khung hình
    public int NFrame { get; set; }
}
