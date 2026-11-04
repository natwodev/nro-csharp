using Nro.Data.Entities.StaticData;
using Nro.Data.Entities.Runtime;
using Nro.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Unused;

// Entity đại diện cho mã quà tặng của website cũ trong hệ thống
// Bảng cơ sở dữ liệu: cvh_giftcode (sinh tự động, không sửa tay)
[Table("cvh_giftcode")]
public class CvhGiftCode
{
    // Khóa chính của bảng CvhGiftCode (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int CvhGiftCodeId { get; set; }

    // Mã quà tặng
    [Required]
    [MaxLength(255)]
    public string Code { get; set; } = default!;

    // Số lượt sử dụng
    public int Luot { get; set; }

    // Vật phẩm thưởng
    [Required]
    [Column(TypeName = "text")]
    public string Item { get; set; } = default!;

    // Chỉ số của vật phẩm thưởng
    [Required]
    [Column(TypeName = "text")]
    public string Option { get; set; } = default!;

    // Trạng thái mã (dạng text)
    [Required]
    [MaxLength(999)]
    public string Status { get; set; } = default!;

    // Hạn sử dụng (dạng text)
    [Required]
    [MaxLength(999)]
    public string Hsd { get; set; } = default!;

    // Thời gian tạo (dạng text)
    [Required]
    [MaxLength(999)]
    public string Time { get; set; } = default!;
}
