using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho mẫu vật phẩm trong hệ thống
// Bảng cơ sở dữ liệu: item_template (sinh tự động, không sửa tay)
[Table("item_template")]
public class ItemTemplateRow
{
    // Khóa chính của bảng ItemTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int ItemTemplateId { get; set; }

    // Mã loại vật phẩm
    public int Type { get; set; }

    // Hành tinh (0: Trái Đất, 1: Namếc, 2: Xayda, 3: tất cả)
    public short Gender { get; set; }

    // Tên vật phẩm
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;

    // Mô tả vật phẩm
    [Required]
    [MaxLength(255)]
    public string Description { get; set; } = default!;

    // Cấp độ vật phẩm
    public int Level { get; set; }

    // Mã biểu tượng (-1: không có)
    public int IconId { get; set; }

    // Mã bộ phận hình ảnh (-1: không có)
    public int Part { get; set; }

    // Cờ cho phép xếp chồng
    public bool IsUpToUp { get; set; }

    // Sức mạnh tối thiểu để dùng
    public long PowerRequire { get; set; }

    // Giá vàng
    public int Gold { get; set; }

    // Giá bán vàng
    public int GoldSell { get; set; }

    // Giá ngọc
    public int Gem { get; set; }

    // Giá bán ngọc
    public int GemSell { get; set; }

    // Giá hồng ngọc
    public int Ruby { get; set; }

    // Giá bán hồng ngọc
    public int RubySell { get; set; }

    // Mã bộ phận đầu
    public int Head { get; set; }

    // Mã bộ phận thân
    public int Body { get; set; }

    // Mã bộ phận chân
    public int Leg { get; set; }

    // Mã sự kiện (0: không có)
    public int TypeEvent { get; set; }

    // Cờ giới tính phụ (-1: mặc định)
    public short IsGender { get; set; }

    // Collection các bản ghi BadgeTemplateRow thuộc ItemTemplate này
    // Mối quan hệ one-to-many với BadgeTemplateRow
    public ICollection<BadgeTemplateRow> BadgeTemplates { get; set; } = new List<BadgeTemplateRow>();

    // Collection các bản ghi ItemShopRow thuộc ItemTemplate này
    // Mối quan hệ one-to-many với ItemShopRow
    public ICollection<ItemShopRow> ItemShops { get; set; } = new List<ItemShopRow>();
}
