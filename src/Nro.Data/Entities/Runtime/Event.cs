using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Runtime;

// Entity đại diện cho trạng thái sự kiện theo mùa toàn máy chủ trong hệ thống
// Bảng cơ sở dữ liệu: event (sinh tự động, không sửa tay)
public class Event
{
    // Khóa chính của bảng Event (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int EventId { get; set; }

    // Khóa định danh sự kiện
    [Required]
    [Column(TypeName = "citext")]
    public string Name { get; set; } = default!;

    // Trạng thái sự kiện (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Data { get; set; } = default!;
}
