using Nro.Data.Entities.StaticData;
using Nro.Data.Entities.Runtime;
using Nro.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Unused;

// Entity đại diện cho vật phẩm rao bán trên website cũ trong hệ thống
// Bảng cơ sở dữ liệu: cvh_sell_item (sinh tự động, không sửa tay)
public class CvhSellItem
{
    // Khóa chính của bảng CvhSellItem (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int CvhSellItemId { get; set; }

    // Giá bán
    public int Price { get; set; }

    // Mẫu vật phẩm được bán
    public int Item { get; set; }

    // Ô hoặc số lượng
    public int Slot { get; set; }

    // Chỉ số vật phẩm (dạng text)
    [Required]
    [Column(TypeName = "text")]
    public string Options { get; set; } = default!;

    // Danh sách người mua
    [Required]
    [Column(TypeName = "text")]
    public string UsersBuy { get; set; } = default!;

    // Trạng thái tin rao bán
    public int Status { get; set; }

    // Thời gian đăng bán (dạng text)
    [Required]
    [MaxLength(999)]
    public string Time { get; set; } = default!;
}
