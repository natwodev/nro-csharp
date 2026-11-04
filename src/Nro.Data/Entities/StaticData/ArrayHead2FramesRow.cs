using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho hoạt ảnh đầu nhiều khung hình trong hệ thống
// Bảng cơ sở dữ liệu: array_head_2_frames (sinh tự động, không sửa tay)
[Table("array_head_2_frames")]
public class ArrayHead2FramesRow
{
    // Khóa chính của bảng ArrayHead2Frames (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ArrayHead2FramesId { get; set; }

    // Mã bộ phận đầu của hoạt ảnh (mảng JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Data { get; set; } = default!;
}
