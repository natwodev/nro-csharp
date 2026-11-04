using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho chỉ số của vật phẩm trong cửa hàng trong hệ thống
// Bảng cơ sở dữ liệu: item_shop_option (sinh tự động, không sửa tay)
[Table("item_shop_option")]
public class ItemShopOptionRow
{
    // Khóa chính của bảng ItemShopOption (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ItemShopOptionId { get; set; }

    // Khóa ngoại liên kết với ItemShop (mục cửa hàng sở hữu)
    [ForeignKey("ItemShop")]
    public int ItemShopId { get; set; }

    // Khóa ngoại liên kết với ItemOptionTemplate (mẫu chỉ số)
    [ForeignKey("ItemOptionTemplate")]
    public int OptionId { get; set; }

    // Giá trị chỉ số
    public int Param { get; set; }

    // Navigation property đến entity ItemShopRow
    public ItemShopRow ItemShop { get; set; } = default!;

    // Navigation property đến entity ItemOptionTemplateRow
    public ItemOptionTemplateRow ItemOptionTemplate { get; set; } = default!;
}
