using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho lượt nhập mã quà tặng trong hệ thống
// Bảng cơ sở dữ liệu: cvh_history_giftcode (sinh tự động, không sửa tay)
[Table("cvh_history_giftcode")]
public class CvhGiftCodeHistory
{
    // Khóa chính của bảng CvhGiftCodeHistory (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public long CvhGiftCodeHistoryId { get; set; }

    // Người chơi nhập mã
    public int? PlayerId { get; set; }

    // Mã quà tặng đã nhập
    [MaxLength(50)]
    public string? Code { get; set; }

    // Thời gian nhập mã
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? Time { get; set; }
}
