using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho cấu hình website trong hệ thống
// Bảng cơ sở dữ liệu: settings (sinh tự động, không sửa tay)
[Table("settings")]
public class SiteSetting
{
    // Tiêu đề website
    [MaxLength(100)]
    public string? Title { get; set; }

    // Mô tả website (SEO)
    [Column(TypeName = "text")]
    public string? Description { get; set; }

    // Từ khóa SEO
    [Column(TypeName = "text")]
    public string? Keywords { get; set; }

    // Khóa công khai reCAPTCHA của website
    [MaxLength(100)]
    public string? SiteKey { get; set; }

    // Khóa bí mật reCAPTCHA của website
    [MaxLength(100)]
    public string? SecretKey { get; set; }

    // Tên máy chủ
    [MaxLength(100)]
    public string? ServerName { get; set; }

    // Liên kết fanpage
    [MaxLength(100)]
    public string? Fanpage { get; set; }

    // Liên kết nhóm cộng đồng
    [MaxLength(100)]
    public string? Group { get; set; }

    // Zalo hỗ trợ
    [MaxLength(100)]
    public string? Zalo { get; set; }

    // Email hỗ trợ
    [MaxLength(50)]
    public string? EmailSupport { get; set; }

    // Tài khoản đăng nhập ngân hàng
    [MaxLength(50)]
    public string? AccountBank { get; set; }

    // Mật khẩu tài khoản ngân hàng
    [MaxLength(50)]
    public string? PasswordBank { get; set; }

    // Số tài khoản ngân hàng nhận tiền
    [MaxLength(50)]
    public string? NumberBank { get; set; }

    // Tên chủ tài khoản ngân hàng
    [MaxLength(50)]
    public string? NameBank { get; set; }

    // Liên kết tải bản Android
    [MaxLength(50)]
    public string? Android { get; set; }

    // Liên kết tải bản Windows
    [MaxLength(50)]
    public string? Windows { get; set; }

    // Liên kết tải bản iPhone
    [MaxLength(50)]
    public string? IPhone { get; set; }

    // Liên kết tải bản Java
    [MaxLength(50)]
    public string? Java { get; set; }
}
