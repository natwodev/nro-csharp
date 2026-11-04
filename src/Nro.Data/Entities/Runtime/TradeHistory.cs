using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Runtime;

// Entity đại diện cho nhật ký giao dịch giữa người chơi trong hệ thống
// Bảng cơ sở dữ liệu: trade_history (sinh tự động, không sửa tay)
public class TradeHistory
{
    // Khóa chính của bảng TradeHistory (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TradeHistoryId { get; set; }

    // Bên thứ nhất (tên và id)
    [Required]
    [MaxLength(255)]
    [Column("player_1")]
    public string Player1 { get; set; } = default!;

    // Bên thứ hai (tên và id)
    [Required]
    [MaxLength(255)]
    [Column("player_2")]
    public string Player2 { get; set; } = default!;

    // Vật phẩm và vàng bên 1 đưa ra
    [Required]
    [Column("item_player_1")]
    public string ItemPlayer1 { get; set; } = default!;

    // Vật phẩm và vàng bên 2 đưa ra
    [Required]
    [Column("item_player_2")]
    public string ItemPlayer2 { get; set; } = default!;

    // Hành trang bên 1 trước giao dịch
    [Required]
    public string FirstPlayerBagBeforeTransaction { get; set; } = default!;

    // Hành trang bên 2 trước giao dịch
    [Required]
    public string SecondPlayerBagBeforeTransaction { get; set; } = default!;

    // Hành trang bên 1 sau giao dịch
    [Required]
    public string FirstPlayerBagAfterTransaction { get; set; } = default!;

    // Hành trang bên 2 sau giao dịch
    [Required]
    public string SecondPlayerBagAfterTransaction { get; set; } = default!;

    // Thời gian giao dịch (UTC)
    public DateTime TransactedAt { get; set; }
}
