using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho mẫu NPC trong hệ thống
// Bảng cơ sở dữ liệu: npc_template (sinh tự động, không sửa tay)
[Table("npc_template")]
public class NpcTemplateRow
{
    // Khóa chính của bảng NpcTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Column("id")]
    public int NpcTemplateId { get; set; }

    // Tên NPC
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = default!;

    // Mã bộ phận đầu
    public int Head { get; set; }

    // Mã bộ phận thân (-1: không có)
    public int Body { get; set; }

    // Mã bộ phận chân (-1: không có)
    public int Leg { get; set; }

    // Mã ảnh đại diện hội thoại
    public int? Avatar { get; set; }

    // Collection các bản ghi ShopRow thuộc NpcTemplate này
    // Mối quan hệ one-to-many với ShopRow
    public ICollection<ShopRow> Shops { get; set; } = new List<ShopRow>();
}
