using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho mẫu chỉ số vật phẩm trong hệ thống
// Bảng cơ sở dữ liệu: item_option_template (sinh tự động, không sửa tay)
[Table("item_option_template")]
public class ItemOptionTemplateRow
{
    // Khóa chính của bảng ItemOptionTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int ItemOptionTemplateId { get; set; }

    // Nội dung hiển thị chỉ số (# là giá trị)
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;

    // Nhóm chỉ số
    public short Type { get; set; }

    // Collection các bản ghi ItemShopOptionRow thuộc ItemOptionTemplate này
    // Mối quan hệ one-to-many với ItemShopOptionRow
    public ICollection<ItemShopOptionRow> ItemShopOptions { get; set; } = new List<ItemShopOptionRow>();
}
