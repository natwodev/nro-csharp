using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho bậc danh hiệu theo sức mạnh trong hệ thống
// Bảng cơ sở dữ liệu: caption (sinh tự động, không sửa tay)
public class CaptionRow
{
    // Khóa chính của bảng Caption (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CaptionId { get; set; }

    // Tên danh hiệu Trái Đất
    [Required]
    public string Earth { get; set; } = default!;

    // Tên danh hiệu Xayda
    [Required]
    public string Saiya { get; set; } = default!;

    // Tên danh hiệu Namếc
    [Required]
    public string Namek { get; set; } = default!;

    // Sức mạnh tối thiểu của bậc
    public long Power { get; set; }
}
