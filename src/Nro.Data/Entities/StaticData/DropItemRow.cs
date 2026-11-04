using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.StaticData;

// Entity đại diện cho quy tắc rơi vật phẩm trong hệ thống
// Bảng cơ sở dữ liệu: drop_item (sinh tự động, không sửa tay)
public class DropItemRow
{
    // Khóa chính của bảng DropItem (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DropItemId { get; set; }

    // Cờ bật quy tắc (1: bật)
    public short Active { get; set; }

    // Mẫu quái
    public int MobId { get; set; }

    // Mẫu bản đồ
    public int MapId { get; set; }

    // Vật phẩm rơi ra
    public int ItemId { get; set; }

    // Số lượng mỗi lần rơi
    public int Quantity { get; set; }

    // Tử số tỷ lệ rơi
    public int RateNum { get; set; }

    // Mẫu số tỷ lệ rơi (mặc định 100)
    public int RateDen { get; set; }

    // Lọc hành tinh (-1: tất cả, 0: Trái Đất, 1: Namếc, 2: Xayda)
    public int Family { get; set; }

    // Ghi chú của quản trị viên
    [MaxLength(255)]
    public string? Note { get; set; }

    // Chỉ số vật phẩm (JSON)
    [JsonText]
    public string? Options { get; set; }

    // Điều kiện rơi bổ sung (JSON)
    [JsonText]
    public string? Conditions { get; set; }

    // Thời gian tạo
    public DateTime CreatedAt { get; set; }

    // Thời gian cập nhật gần nhất
    public DateTime UpdatedAt { get; set; }
}
