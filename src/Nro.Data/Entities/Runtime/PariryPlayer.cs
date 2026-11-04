using Nro.Data.Entities.StaticData;
using Nro.Data.Entities.Runtime;
using Nro.Data.Entities.Logs;
using Nro.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Runtime;

// Entity đại diện cho lượt đặt cược của người chơi trong phiên Chẵn Lẻ trong hệ thống
// Bảng cơ sở dữ liệu: pariry_players (sinh tự động, không sửa tay)
[Table("pariry_players")]
public class PariryPlayer
{
    // Khóa ngoại liên kết với ParirySession (phiên minigame)
    [ForeignKey("ParirySession")]
    public int IdSession { get; set; }

    // Người chơi đặt cược
    public int IdPlayer { get; set; }

    // Số thỏi vàng đã cược
    public int RubyQuantity { get; set; }

    // Lựa chọn cược (1: lẻ, 0: chẵn)
    public bool IsOdd { get; set; }

    // Kết quả cược (1: thắng, 0: thua)
    public bool IsWin { get; set; }

    // Cờ đã nhận thưởng
    public bool Reward { get; set; }

    // Thời gian cược (thời gian nhận sau khi có thưởng)
    [Column(TypeName = "timestamp with time zone")]
    public DateTime Time { get; set; }

    // Navigation property đến entity ParirySession
    public ParirySession ParirySession { get; set; } = default!;
}
