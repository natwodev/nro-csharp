using Nro.Data.Entities.StaticData;
using Nro.Data.Entities.Runtime;
using Nro.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho cửa hàng của NPC trong hệ thống
// Bảng cơ sở dữ liệu: shop (sinh tự động, không sửa tay)
[Table("shop")]
public class ShopRow
{
    // Khóa chính của bảng Shop (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int ShopId { get; set; }

    // Khóa ngoại liên kết với NpcTemplate (nPC sở hữu cửa hàng)
    [ForeignKey("NpcTemplate")]
    public int NpcId { get; set; }

    // Mã định danh cửa hàng
    [MaxLength(50)]
    public string? TagName { get; set; }

    // Loại shop (0: thường, 1: học skill, 2: điểm, 3: đặc biệt)
    public int? TypeShop { get; set; }

    // Navigation property đến entity NpcTemplateRow
    public NpcTemplateRow NpcTemplate { get; set; } = default!;

    // Collection các bản ghi TabShopRow thuộc Shop này
    // Mối quan hệ one-to-many với TabShopRow
    public ICollection<TabShopRow> TabShops { get; set; } = new List<TabShopRow>();
}
