using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Archive;

// Entity đại diện cho lượt nhập mã quà tặng trong hệ thống
// Bảng cơ sở dữ liệu: cvh_gift_code_history (sinh tự động, không sửa tay)
public class CvhGiftCodeHistory
{
    // Khóa chính của bảng CvhGiftCodeHistory (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
