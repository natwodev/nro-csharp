using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities.Archive;

// Entity đại diện cho bài viết diễn đàn của website cũ trong hệ thống
// Bảng cơ sở dữ liệu: cvh_article (sinh tự động, không sửa tay)
public class CvhArticle
{
    // Khóa chính của bảng CvhArticle (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CvhArticleId { get; set; }

    // Tiêu đề bài viết
    [Required]
    [MaxLength(999)]
    public string Title { get; set; } = default!;

    // Nội dung bài viết (HTML)
    [Required]
    public string Content { get; set; } = default!;

    // Cờ đăng bởi quản trị viên
    [Required]
    [MaxLength(999)]
    public string IfAdmin { get; set; } = default!;

    // Số lượt xem (dạng text)
    [Required]
    public string Views { get; set; } = default!;

    // Số lượt thích (dạng text)
    [Required]
    public string Likes { get; set; } = default!;

    // Bình luận của bài viết (dạng text)
    [Required]
    public string Comments { get; set; } = default!;

    // Trạng thái bài viết
    public int Status { get; set; }

    // Vai trò người đăng hoặc loại bài viết
    public int Role { get; set; }

    // Tên người đăng
    [Required]
    [MaxLength(999)]
    public string Poster { get; set; } = default!;

    // Ngày tạo
    public DateOnly Created { get; set; }

    // Thời gian tạo hoặc sửa (dạng text)
    [Required]
    [MaxLength(999)]
    public string Time { get; set; } = default!;
}
