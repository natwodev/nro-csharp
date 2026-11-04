using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Archive;

// Entity đại diện cho lớp nhân vật hoặc tên hành tinh trong hệ thống
// Bảng cơ sở dữ liệu: character_class (sinh tự động, không sửa tay)
public class CharacterClass
{
    // Khóa chính của bảng CharacterClass
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CharacterClassId { get; set; }

    // Tên lớp
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = default!;
}
