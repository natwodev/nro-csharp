using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho hoạt ảnh đầu nhiều khung hình trong hệ thống
// Bảng cơ sở dữ liệu: head_animation_frame (sinh tự động, không sửa tay)
public class HeadAnimationFrameRow
{
    // Khóa chính của bảng HeadAnimationFrame (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int HeadAnimationFrameId { get; set; }

    // Mã bộ phận đầu của hoạt ảnh (mảng JSON)
    [Required]
    [JsonText]
    public string Data { get; set; } = default!;
}
