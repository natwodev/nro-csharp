using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nro.Data.Entities;

// Entity đại diện cho tài khoản đăng nhập trong hệ thống
// Bảng cơ sở dữ liệu: account (sinh tự động, không sửa tay)
public class Account
{
    // Khóa chính của bảng Account (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("id")]
    public int AccountId { get; set; }

    // Tên đăng nhập
    [Required]
    [MaxLength(20)]
    [Column(TypeName = "citext")]
    public string Username { get; set; } = default!;

    // Mật khẩu tài khoản
    [Required]
    [MaxLength(100)]
    public string Password { get; set; } = default!;

    // Cờ quản trị viên
    public short IsAdmin { get; set; }

    // Số dư cash cũ (không còn dùng)
    public int? Cash { get; set; }

    // Email và thông tin xác thực (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Email { get; set; } = default!;

    // Thời gian tạo tài khoản (UTC)
    [Column(TypeName = "timestamp with time zone")]
    public DateTime? AccountCreatedTimes { get; set; }

    // Số ngày kích hoạt đặc quyền (mặc định 45)
    public int AccountAgeDays { get; set; }

    // Thời gian cập nhật gần nhất
    [Column(TypeName = "timestamp with time zone")]
    public DateTime? UpdateTime { get; set; }

    // Cờ người sáng lập (quản trị cao nhất)
    public short IsFounder { get; set; }

    // Cờ bị giam (1: bị giam)
    public int IsJail { get; set; }

    // Cờ bị khóa (1: bị khóa)
    public short Ban { get; set; }

    // Điểm bài đăng diễn đàn (cũ)
    public int PointPost { get; set; }

    // Thời gian đăng bài diễn đàn gần nhất (cũ)
    public int LastPost { get; set; }

    // Giá trị vai trò cũ (mặc định -1)
    public int Role { get; set; }

    // Cờ điều hành viên
    public short IsQuanTriVien { get; set; }

    // Số lần nâng cấp VIP đã mua (tối đa 20)
    public int VipPoint { get; set; }

    // Thời gian đăng nhập gần nhất (epoch ms)
    [Column(TypeName = "timestamp with time zone")]
    public DateTime LastTimeLogin { get; set; }

    // Thời gian đăng xuất gần nhất (epoch ms)
    [Column(TypeName = "timestamp with time zone")]
    public DateTime LastTimeLogout { get; set; }

    // Địa chỉ IP đăng nhập web gần nhất
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    // Cờ thành viên (1: đã kích hoạt)
    public int Active { get; set; }

    // Số thỏi vàng chưa nhận
    public long ThoiVang { get; set; }

    // Máy chủ đăng nhập gần nhất (-1: chưa có)
    public int ServerLogin { get; set; }

    // Hệ số bd của người chơi (mặc định 1)
    public double? BdPlayer { get; set; }

    // Tổng số tiền đã nạp (VND)
    public long Tongnap { get; set; }

    // Số dư ví VND
    public long? Vnd { get; set; }

    // Số dư coin
    public long? Coin { get; set; }

    // Số tiền đã nạp
    public long Danap { get; set; }

    // Phần thứ hai của khóa chính ghép
    [Required]
    [MaxLength(100)]
    [JsonText]
    public string Mkc2 { get; set; } = default!;

    // Thời gian tạo tài khoản (giờ địa phương)
    [Column(TypeName = "timestamp without time zone")]
    public DateTime? CreateTime { get; set; }

    // Tài khoản giới thiệu
    public int? RefId { get; set; }

    // Hoa hồng giới thiệu đã nhận (VND)
    public int? CommissionClaimed { get; set; }

    // Mốc nạp đã nhận thưởng (mảng JSON)
    [Required]
    [MaxLength(50)]
    [JsonText]
    public string Mocnap { get; set; } = default!;

    // Bộ đếm tổng nạp thứ hai
    public long TongNap2 { get; set; }

    // Số dư hồng ngọc
    public int? Ruby { get; set; }

    // Bộ đếm thẻ
    public int? CountCard { get; set; }

    // Cờ vé tuần (1: có)
    public int Vetuan { get; set; }

    // Cờ vé tháng (1: có)
    public int Vethang { get; set; }

    // Hạn vé tháng (UTC epoch ms, 0: không có)
    public long VethangExpire { get; set; }

    // Hạn vé tuần (UTC epoch ms, 0: không có)
    public long VetuanExpire { get; set; }

    // Collection các bản ghi Player thuộc Account này
    // Mối quan hệ one-to-many với Player
    public ICollection<Player> Players { get; set; } = new List<Player>();
}
