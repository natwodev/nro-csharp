using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Runtime;

// Entity đại diện cho vật phẩm ký gửi trong hệ thống
// Bảng cơ sở dữ liệu: consignment_item (sinh tự động, không sửa tay)
public class ConsignmentItem
{
    // Khóa chính của bảng ConsignmentItem (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ConsignmentItemId { get; set; }

    // Khóa ngoại liên kết với Player (người chơi bán)
    [ForeignKey("Player")]
    public int PlayerId { get; set; }

    // Tên người bán
    [Required]
    public string PlayerName { get; set; } = default!;

    // Thẻ (0: trang bị, 1: phụ kiện, 2: hỗ trợ, 3: khác)
    public int Tab { get; set; }

    // Mẫu vật phẩm ký gửi
    public int ItemId { get; set; }

    // Giá bán bằng thỏi vàng
    public int Gold { get; set; }

    // Giá bán bằng hồng ngọc
    public int Gem { get; set; }

    // Số lượng vật phẩm
    public int Quantity { get; set; }

    // Chỉ số vật phẩm (JSON)
    [Required]
    [MaxLength(10000)]
    [JsonText]
    public string ItemOption { get; set; } = default!;

    // Thời gian đăng bán (epoch ms)
    public long LastTime { get; set; }

    // Cờ đã bán (0/1)
    public int IsBuy { get; set; }

    // Navigation property đến entity Player
    public Player Player { get; set; } = default!;
}
