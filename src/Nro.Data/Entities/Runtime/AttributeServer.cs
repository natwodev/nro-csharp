using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho trạng thái buff toàn máy chủ trong hệ thống
// Bảng cơ sở dữ liệu: attribute_server (sinh tự động, không sửa tay)
public class AttributeServer
{
    // Khóa chính của bảng AttributeServer (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int AttributeServerId { get; set; }

    // Khóa ngoại liên kết với AttributeTemplate (loại buff)
    [ForeignKey("AttributeTemplate")]
    public int AttributeTemplateId { get; set; }

    // Giá trị cộng thêm (%)
    public int Value { get; set; }

    // Số giây còn lại (-1: không giới hạn)
    public int Time { get; set; }

    // Navigation property đến entity AttributeTemplateRow
    public AttributeTemplateRow AttributeTemplate { get; set; } = default!;
}
