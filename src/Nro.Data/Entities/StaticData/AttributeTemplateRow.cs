using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nro.Data.Entities.Runtime;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho loại buff toàn máy chủ trong hệ thống
// Bảng cơ sở dữ liệu: attribute_template (sinh tự động, không sửa tay)
[Table("attribute_template")]
public class AttributeTemplateRow
{
    // Khóa chính của bảng AttributeTemplate (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AttributeTemplateId { get; set; }

    // Nội dung hiển thị buff (#value)
    [Required]
    [Column(TypeName = "text")]
    public string Name { get; set; } = default!;

    // Collection các bản ghi AttributeServer thuộc AttributeTemplate này
    // Mối quan hệ one-to-many với AttributeServer
    public ICollection<AttributeServer> AttributeServers { get; set; } = new List<AttributeServer>();
}
