using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho bộ phận ngoại hình nhân vật trong hệ thống
// Bảng cơ sở dữ liệu: part (sinh tự động, không sửa tay)
[Table("part")]
public class PartRow
{
    // Khóa chính của bảng Part
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int PartId { get; set; }

    // Loại bộ phận (0: đầu, 1: thân, 2: chân)
    public int Type { get; set; }

    // Các mảnh hình ảnh của bộ phận (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Data { get; set; } = default!;
}
