using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Unused;

// Entity đại diện cho cấu hình website cũ trong hệ thống
// Bảng cơ sở dữ liệu: cvh_site_setting (sinh tự động, không sửa tay)
public class CvhSiteSetting
{
    // Khóa chính của bảng CvhSiteSetting (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CvhSiteSettingId { get; set; }

    // Tiêu đề website
    [Required]
    [MaxLength(999)]
    public string Title { get; set; } = default!;

    // Mô tả website (SEO)
    [Required]
    [MaxLength(999)]
    public string Description { get; set; } = default!;

    // Tác giả website
    [Required]
    [MaxLength(999)]
    public string Author { get; set; } = default!;

    // Từ khóa SEO
    [Required]
    [MaxLength(999)]
    public string Keywords { get; set; } = default!;

    // Ảnh logo
    [Required]
    [MaxLength(999)]
    public string Logo { get; set; } = default!;

    // Ảnh nền
    [Required]
    [MaxLength(999)]
    public string Background { get; set; } = default!;

    // Kích thước logo
    public int SizeLogo { get; set; }

    // Ảnh banner
    [Required]
    [MaxLength(999)]
    public string Banner { get; set; } = default!;

    // Kiểu thanh điều hướng
    [Required]
    [MaxLength(999)]
    public string Navbar { get; set; } = default!;

    // Liên kết tải game và mạng xã hội
    [Required]
    [JsonText]
    public string Download { get; set; } = default!;

    // Cờ kích hoạt thành viên
    [MaxLength(999)]
    public string? Mtv { get; set; }

    // Phí kích hoạt thành viên (dạng text)
    [Required]
    [MaxLength(999)]
    public string AmountMtv { get; set; } = default!;

    // Cờ bật thông báo
    [Required]
    [MaxLength(99)]
    public string Notification { get; set; } = default!;

    // Nội dung thông báo
    [Required]
    public string NdThongbao { get; set; } = default!;

    // Biểu tượng favicon
    [Required]
    [MaxLength(999)]
    public string Favicon { get; set; } = default!;
}
