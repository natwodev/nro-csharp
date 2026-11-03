using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho bài viết diễn đàn của website trong hệ thống
// Bảng cơ sở dữ liệu: posts (sinh tự động, không sửa tay)
[Table("posts")]
public class ForumPost
{
    // Khóa chính của bảng ForumPost (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int ForumPostId { get; set; }

    // Tiêu đề bài viết
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = default!;

    // Nội dung bài viết (HTML)
    [Required]
    [Column(TypeName = "text")]
    public string Description { get; set; } = default!;

    // Cờ ghim bài (1: ghim, 0: thường)
    [MaxLength(50)]
    public string? Category { get; set; }

    // Bình luận của bài viết (mảng JSON)
    [JsonText]
    [Column(TypeName = "text")]
    public string? Comments { get; set; }

    // Thời gian tạo
    [Column(TypeName = "timestamp with time zone")]
    public DateTime? CreatedAt { get; set; }
}
