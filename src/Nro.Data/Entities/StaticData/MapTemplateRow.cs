using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho mẫu bản đồ trong hệ thống
// Bảng cơ sở dữ liệu: map_template (sinh tự động, không sửa tay)
[Table("map_template")]
public class MapTemplateRow
{
    // Khóa chính của bảng MapTemplate
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int MapTemplateId { get; set; }

    // Tên bản đồ
    [Required]
    [MaxLength(55)]
    public string Name { get; set; } = default!;

    // Số khu
    public int Zones { get; set; }

    // Số người chơi tối đa mỗi khu
    public int MaxPlayer { get; set; }

    // Dữ liệu cũ (không dùng)
    [Required]
    [MaxLength(1000)]
    [JsonText]
    public string Data { get; set; } = default!;

    // Loại bản đồ
    public int Type { get; set; }

    // Hành tinh (0: Trái Đất, 1: Namếc, 2: Xayda)
    public int PlanetId { get; set; }

    // Loại hình nền
    public int BgType { get; set; }

    // Mã bộ ô nền
    public int TileId { get; set; }

    // Mã hình nền (-1: không có)
    public int BgId { get; set; }

    // Các điểm chuyển map (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Waypoints { get; set; } = default!;

    // Quái đặt sẵn (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Mobs { get; set; } = default!;

    // NPC đặt sẵn (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Npcs { get; set; } = default!;

    // Hành tinh sở hữu (0: Trái Đất, 1: Namếc, 2: Xayda)
    public int GenderType { get; set; }
}
