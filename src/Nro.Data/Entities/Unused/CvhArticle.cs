using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho bài viết diễn đàn của website cũ trong hệ thống
// Bảng cơ sở dữ liệu: cvh_baiviet (sinh tự động, không sửa tay)
[Table("cvh_baiviet")]
public class CvhArticle
{
    // Khóa chính của bảng CvhArticle (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int CvhArticleId { get; set; }

    // Tiêu đề bài viết
    [Required]
    [MaxLength(999)]
    public string Title { get; set; } = default!;

    // Nội dung bài viết (HTML)
    [Required]
    [Column(TypeName = "text")]
    public string Content { get; set; } = default!;

    // Cờ đăng bởi quản trị viên
    [Required]
    [MaxLength(999)]
    public string IfAdmin { get; set; } = default!;

    // Số lượt xem (dạng text)
    [Required]
    [Column(TypeName = "text")]
    public string Views { get; set; } = default!;

    // Số lượt thích (dạng text)
    [Required]
    [Column(TypeName = "text")]
    public string Likes { get; set; } = default!;

    // Bình luận của bài viết (dạng text)
    [Required]
    [Column(TypeName = "text")]
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
    [Column(TypeName = "date")]
    public DateOnly Created { get; set; }

    // Thời gian tạo hoặc sửa (dạng text)
    [Required]
    [MaxLength(999)]
    public string Time { get; set; } = default!;
}
