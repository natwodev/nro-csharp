using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Runtime;

// Entity đại diện cho một ván minigame Tài Xỉu trong hệ thống
// Bảng cơ sở dữ liệu: sic_bo_round (sinh tự động, không sửa tay)
public class SicBoRound
{
    // Khóa chính của bảng SicBoRound (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SicBoRoundId { get; set; }

    // Xúc xắc thứ nhất (1-6)
    public int FirstDie { get; set; }

    // Xúc xắc thứ hai (1-6)
    public int SecondDie { get; set; }

    // Xúc xắc thứ ba (1-6)
    public int ThirdDie { get; set; }

    // Cờ kết quả Tài/Xỉu (nghĩa chưa chắc, cần chủ dự án duyệt)
    public bool IsHighResult { get; set; }

    // Thời điểm kết thúc ván
    public DateTime SettledAt { get; set; }
}
