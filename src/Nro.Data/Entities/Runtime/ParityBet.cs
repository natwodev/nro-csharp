using System.ComponentModel.DataAnnotations.Schema;
using Nro.Data.Entities.Logs;

namespace Nro.Data.Entities.Runtime;

// Entity đại diện cho lượt đặt cược của người chơi trong phiên Chẵn Lẻ trong hệ thống
// Bảng cơ sở dữ liệu: parity_bet (sinh tự động, không sửa tay)
public class ParityBet
{
    // Khóa ngoại liên kết với ParitySession (phiên minigame)
    [ForeignKey("ParitySession")]
    public int SessionId { get; set; }

    // Người chơi đặt cược
    public int PlayerId { get; set; }

    // Số thỏi vàng đã cược
    public int BetGoldBarCount { get; set; }

    // Lựa chọn cược (1: lẻ, 0: chẵn)
    public bool IsOdd { get; set; }

    // Kết quả cược (1: thắng, 0: thua)
    public bool IsWin { get; set; }

    // Cờ đã nhận thưởng
    public bool IsRewardClaimed { get; set; }

    // Thời điểm cược (cập nhật khi nhận thưởng)
    [Column(TypeName = "timestamp with time zone")]
    public DateTime BetAt { get; set; }

    // Navigation property đến entity ParitySession
    public ParitySession ParitySession { get; set; } = default!;
}
