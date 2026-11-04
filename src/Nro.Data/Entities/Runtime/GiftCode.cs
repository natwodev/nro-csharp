using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Runtime;

// Entity đại diện cho mã quà tặng trong hệ thống
// Bảng cơ sở dữ liệu: giftcode (sinh tự động, không sửa tay)
[Table("giftcode")]
public class GiftCode
{
    // Khóa chính của bảng GiftCode (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int GiftCodeId { get; set; }

    // Mã quà tặng
    [Required]
    [MaxLength(255)]
    [Column(TypeName = "citext")]
    public string Code { get; set; } = default!;

    // Vật phẩm thưởng (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Item { get; set; } = default!;

    // Chỉ số của vật phẩm thưởng
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Option { get; set; } = default!;

    // Danh sách người chơi đã nhập (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string ListIdPlayers { get; set; } = default!;

    // Thời gian tạo (dạng text)
    [Required]
    [Column(TypeName = "text")]
    public string CreatedAt { get; set; } = default!;

    // Thời gian hết hạn (dạng text)
    [Required]
    [Column(TypeName = "text")]
    public string Expired { get; set; } = default!;

    // Số lượt sử dụng còn lại
    public int CountLeft { get; set; }
}
