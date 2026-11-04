using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho ánh xạ bộ phận đầu sang avatar trong hệ thống
// Bảng cơ sở dữ liệu: head_avatar (sinh tự động, không sửa tay)
[Table("head_avatar")]
public class HeadAvatarRow
{
    // Khóa chính của bảng HeadAvatar
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int HeadId { get; set; }

    // Mã biểu tượng avatar
    public int AvatarId { get; set; }
}
