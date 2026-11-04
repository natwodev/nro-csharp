using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Runtime;

// Entity đại diện cho một ván minigame Tài Xỉu trong hệ thống
// Bảng cơ sở dữ liệu: tai_xiu (sinh tự động, không sửa tay)
[Table("tai_xiu")]
public class SicBoRound
{
    // Khóa chính của bảng SicBoRound (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SicBoRoundId { get; set; }

    // Xúc xắc thứ nhất (1-6)
    [Column("number_1")]
    public int Number1 { get; set; }

    // Xúc xắc thứ hai (1-6)
    [Column("number_2")]
    public int Number2 { get; set; }

    // Xúc xắc thứ ba (1-6)
    [Column("number_3")]
    public int Number3 { get; set; }

    // Cờ kết quả (không dùng)
    public bool Result { get; set; }

    // Thời gian kết thúc ván
    [Column(TypeName = "timestamp with time zone")]
    public DateTime Time { get; set; }
}
