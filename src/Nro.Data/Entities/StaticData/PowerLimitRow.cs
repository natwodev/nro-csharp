using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho bậc giới hạn sức mạnh trong hệ thống
// Bảng cơ sở dữ liệu: power_limit (sinh tự động, không sửa tay)
[Table("power_limit")]
public class PowerLimitRow
{
    // Khóa chính của bảng PowerLimit (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int PowerLimitId { get; set; }

    // Ngưỡng sức mạnh
    public long Power { get; set; }

    // Giới hạn HP cơ bản
    public int Hp { get; set; }

    // Giới hạn ki cơ bản
    public int Mp { get; set; }

    // Giới hạn sức đánh cơ bản
    public int Damage { get; set; }

    // Giới hạn giáp cơ bản
    public int Defense { get; set; }

    // Giới hạn chí mạng cơ bản
    public int Critical { get; set; }
}
