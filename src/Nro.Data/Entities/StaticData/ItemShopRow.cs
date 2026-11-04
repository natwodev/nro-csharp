using Nro.Data.Entities.StaticData;
using Nro.Data.Entities.Runtime;
using Nro.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho một mục trong cửa hàng trong hệ thống
// Bảng cơ sở dữ liệu: item_shop (sinh tự động, không sửa tay)
[Table("item_shop")]
public class ItemShopRow
{
    // Khóa chính của bảng ItemShop (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int ItemShopId { get; set; }

    // Khóa ngoại liên kết với TabShop (thẻ cửa hàng)
    [ForeignKey("TabShop")]
    public int TabId { get; set; }

    // Khóa ngoại liên kết với ItemTemplate (mẫu vật phẩm được bán)
    [ForeignKey("ItemTemplate")]
    public int TempId { get; set; }

    // Cờ nhãn mới
    public short IsNew { get; set; }

    // Cờ đang bán
    public bool IsSell { get; set; }

    // Loại tiền (0: vàng, 1: ngọc, 3: hồng ngọc, 4: sự kiện)
    public int? TypeSell { get; set; }

    // Giá bán
    public int? Cost { get; set; }

    // Giá vàng phụ
    public int? Costgold { get; set; }

    // Mã biểu tượng tiền tệ đặc biệt
    public int? IconSpec { get; set; }

    // Thời gian tạo
    [Column(TypeName = "timestamp with time zone")]
    public DateTime? CreateTime { get; set; }

    // Navigation property đến entity TabShopRow
    public TabShopRow TabShop { get; set; } = default!;

    // Navigation property đến entity ItemTemplateRow
    public ItemTemplateRow ItemTemplate { get; set; } = default!;

    // Collection các bản ghi ItemShopOptionRow thuộc ItemShop này
    // Mối quan hệ one-to-many với ItemShopOptionRow
    public ICollection<ItemShopOptionRow> ItemShopOptions { get; set; } = new List<ItemShopOptionRow>();
}
