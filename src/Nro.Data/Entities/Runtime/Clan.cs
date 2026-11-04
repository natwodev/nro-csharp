using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Runtime;

// Entity đại diện cho bang hội trong hệ thống
// Bảng cơ sở dữ liệu: clan (sinh tự động, không sửa tay)
public class Clan
{
    // Khóa chính của bảng Clan
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int ClanId { get; set; }

    // Tên bang hội
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;

    // Tên viết tắt của bang hội
    [Required]
    [MaxLength(4)]
    public string ShortName { get; set; } = default!;

    // Khẩu hiệu bang hội
    [Required]
    [MaxLength(255)]
    public string Slogan { get; set; } = default!;

    // Mã biểu tượng bang hội
    public int ImgId { get; set; }

    // Sức mạnh bang hội
    public long PowerPoint { get; set; }

    // Số thành viên tối đa
    public short MaxMember { get; set; }

    // Số capsule bang hội hiện có
    public int ClanPoint { get; set; }

    // Cấp độ bang hội
    public int Level { get; set; }

    // Thành viên bang hội (JSON)
    [Required]
    [JsonText]
    public string Members { get; set; } = default!;

    // Thành viên nổi bật (không dùng)
    [Required]
    public string Tops { get; set; } = default!;

    // Thông tin bang chủ (không dùng)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string LeaderInfo { get; set; } = default!;

    // Thành tích hầm Hủy Diệt Khí Gas
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string DestronGasAchievement { get; set; } = default!;

    // Thành tích hầm Bản Đồ Kho Báu
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string TreasureUnderSeaAchievement { get; set; } = default!;

    // Thành tích hầm Con Đường Rắn Độc
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string SnakeWayAchievement { get; set; } = default!;

    // Hệ số tăng kinh nghiệm của bang hội
    public int BuffExp { get; set; }

    // Thời điểm bắt đầu tăng kinh nghiệm (epoch ms)
    public long ExperienceBuffStartedAt { get; set; }

    // Thời lượng tăng kinh nghiệm (ms)
    public long ExperienceBuffDuration { get; set; }

    // Vật phẩm trong rương bang hội (JSON)
    [Required]
    [JsonText]
    public string ItemsBoxClan { get; set; } = default!;

    // Lượt boss bang hội hiện tại trong ngày
    public int BossClan { get; set; }

    // Thời hạn bùa bang hội (JSON)
    [Required]
    [JsonText]
    public string DataCharms { get; set; } = default!;

    // Thời gian tạo bang hội
    public DateTime CreateTime { get; set; }

    // Collection các bản ghi Player thuộc Clan này
    // Mối quan hệ one-to-many với Player
    public ICollection<Player> Players { get; set; } = new List<Player>();
}
