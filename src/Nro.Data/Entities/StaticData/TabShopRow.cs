using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho thẻ trong cửa hàng trong hệ thống
// Bảng cơ sở dữ liệu: tab_shop (sinh tự động, không sửa tay)
[Table("tab_shop")]
public class TabShopRow
{
    // Khóa chính của bảng TabShop (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TabShopId { get; set; }

    // Khóa ngoại liên kết với Shop (cửa hàng sở hữu thẻ)
    [ForeignKey("Shop")]
    public int ShopId { get; set; }

    // Tên thẻ
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = default!;

    // Navigation property đến entity ShopRow
    public ShopRow Shop { get; set; } = default!;

    // Collection các bản ghi ItemShopRow thuộc TabShop này
    // Mối quan hệ one-to-many với ItemShopRow
    public ICollection<ItemShopRow> ItemShops { get; set; } = new List<ItemShopRow>();
}
