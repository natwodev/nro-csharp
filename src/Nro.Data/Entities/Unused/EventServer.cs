using Nro.Data.Entities.StaticData;
using Nro.Data.Entities.Runtime;
using Nro.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Unused;

// Entity đại diện cho cờ và điểm sự kiện (không dùng) trong hệ thống
// Bảng cơ sở dữ liệu: event_server (sinh tự động, không sửa tay)
public class EventServer
{
    // Khóa chính của bảng EventServer (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdEvent { get; set; }

    // Tên định danh sự kiện
    [Required]
    [Column(TypeName = "text")]
    public string NameEvent { get; set; } = default!;

    // Điểm và cờ sự kiện
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string Point { get; set; } = default!;
}
