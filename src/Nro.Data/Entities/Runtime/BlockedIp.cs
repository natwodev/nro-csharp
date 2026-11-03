using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho địa chỉ IP bị chặn trong hệ thống
// Bảng cơ sở dữ liệu: blockip_list (sinh tự động, không sửa tay)
[Table("blockip_list")]
public class BlockedIp
{
    // Khóa chính của bảng BlockedIp (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int BlockedIpId { get; set; }

    // Địa chỉ IP bị chặn
    [Required]
    [MaxLength(64)]
    public string BlockerIp { get; set; } = default!;

    // Lý do chặn
    [MaxLength(255)]
    public string? BlockReason { get; set; }

    // Thời gian chặn
    [Column(TypeName = "timestamp without time zone")]
    public DateTime BlockedAt { get; set; }
}
