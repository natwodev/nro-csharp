using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho cờ bang hội hoặc vật phẩm đeo lưng trong hệ thống
// Bảng cơ sở dữ liệu: flag_bag (sinh tự động, không sửa tay)
[Table("flag_bag")]
public class FlagBagRow
{
    // Khóa chính của bảng FlagBag
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int FlagBagId { get; set; }

    // Mã biểu tượng hiệu ứng (JSON)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string IconData { get; set; } = default!;

    // Tên cờ
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;

    // Giá vàng
    public int Gold { get; set; }

    // Giá ngọc
    public int Gem { get; set; }

    // Mã biểu tượng tĩnh
    public int IconId { get; set; }
}
