using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho thẻ radar trong hệ thống
// Bảng cơ sở dữ liệu: radar (sinh tự động, không sửa tay)
[Table("radar")]
public class RadarCardRow
{
    // Khóa chính của bảng RadarCard
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int RadarCardId { get; set; }

    // Mã biểu tượng thẻ
    public int? IconId { get; set; }

    // Hạng thẻ (0..5)
    public short? Rank { get; set; }

    // Số thẻ cần để lên cấp
    public int? Max { get; set; }

    // Loại thẻ (0: quái, 1: ngoại hình)
    public int? Type { get; set; }

    // Mẫu quái (thẻ quái)
    public int? MobId { get; set; }

    // Mã bộ phận ngoại hình (JSON)
    [MaxLength(500)]
    [JsonText]
    public string? Body { get; set; }

    // Tên thẻ
    [MaxLength(500)]
    public string? Name { get; set; }

    // Mô tả thẻ
    [MaxLength(2000)]
    public string? Info { get; set; }

    // Chỉ số cộng thêm mỗi cấp (JSON)
    [MaxLength(2000)]
    [JsonText]
    public string? Options { get; set; }

    // Mã hào quang (0: không có)
    public short? AuraId { get; set; }
}
