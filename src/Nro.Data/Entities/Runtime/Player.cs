using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Nro.Data.Entities.Logs;

namespace Nro.Data.Entities.Runtime;

// Entity đại diện cho nhân vật người chơi trong hệ thống
// Bảng cơ sở dữ liệu: player (sinh tự động, không sửa tay)
public class Player
{
    // Khóa chính của bảng Player (tự tăng)
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PlayerId { get; set; }

    // Khóa ngoại liên kết với Account (tài khoản sở hữu)
    [ForeignKey("Account")]
    public int? AccountId { get; set; }

    // Tên nhân vật
    [Required]
    [MaxLength(20)]
    [Column(TypeName = "citext")]
    public string Name { get; set; } = default!;

    // Mã bộ phận đầu (kiểu tóc)
    public int Head { get; set; }

    // Hành tinh (0: Trái Đất, 1: Namếc, 2: Xayda)
    public int Gender { get; set; }

    // Sở hữu tàu vũ trụ Tennis hay không
    public bool? HaveTennisSpaceShip { get; set; }

    // Khóa ngoại liên kết với Clan (bang hội (-1: chưa có))
    [ForeignKey("Clan")]
    public int ClanId { get; set; }

    // Tiền tệ (JSON: vàng, ngọc, hồng ngọc, điểm VIP)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataInventory { get; set; } = default!;

    // Vị trí cuối cùng (JSON: map, x, y)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataLocation { get; set; } = default!;

    // Chỉ số chiến đấu (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataPoint { get; set; } = default!;

    // Trạng thái cây đậu thần (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataMagicTree { get; set; } = default!;

    // Vật phẩm đang trang bị (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string ItemsBody { get; set; } = default!;

    // Vật phẩm trong hành trang (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string ItemsBag { get; set; } = default!;

    // Vật phẩm trong rương đồ cá nhân (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string ItemsBox { get; set; } = default!;

    // Vật phẩm trong rương sưu tầm (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string ItemsBoxCollection { get; set; } = default!;

    // Vật phẩm thưởng vòng quay may mắn (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string ItemsBoxLuckyRound { get; set; } = default!;

    // Vật phẩm đã bán để mua lại (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string ItemsDaban { get; set; } = default!;

    // Vật phẩm trong hòm thư (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string ItemMailsBox { get; set; } = default!;

    // Danh sách bạn bè (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Friends { get; set; } = default!;

    // Danh sách kẻ thù (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Enemies { get; set; } = default!;

    // Nội tại và trạng thái (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataIntrinsic { get; set; } = default!;

    // Hiệu ứng vật phẩm theo thời gian còn lại (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataItemTime { get; set; } = default!;

    // Tiến độ nhiệm vụ chính (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataTask { get; set; } = default!;

    // Trứng Mabu đang ấp (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataMabuEgg { get; set; } = default!;

    // Thời hạn bùa (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataCharm { get; set; } = default!;

    // Kỹ năng đã học (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Skills { get; set; } = default!;

    // Ô phím tắt kỹ năng (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string SkillsShortcut { get; set; } = default!;

    // Dữ liệu luyện tập và thách đấu Whis (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataLuyentap { get; set; } = default!;

    // Đệ tử: thông tin, chỉ số, trang bị, kỹ năng (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string Pet { get; set; } = default!;

    // Nhân vật gia đình là cha (JSON, không dùng)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string MyFather { get; set; } = default!;

    // Nhân vật gia đình là mẹ (JSON, không dùng)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string MyMother { get; set; } = default!;

    // Lần rớt đuôi khỉ cuối (epoch ms)
    public long LastTimeDropTail { get; set; }

    // Lần hóa khỉ sự kiện cuối (epoch ms)
    public long LastTimeKhiSuKien { get; set; }

    // Phần thưởng ngọc rồng đen (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataBlackBall { get; set; } = default!;

    // Nhiệm vụ phụ hằng ngày (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataSideTask { get; set; } = default!;

    // Nhiệm vụ KOL (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataKolTask { get; set; } = default!;

    // Nhiệm vụ sự kiện (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataEventTask { get; set; } = default!;

    // Thời gian tạo bản ghi
    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreateTime { get; set; }

    // Điểm PvP (không dùng)
    public int? PointPvp { get; set; }

    // Thẻ Rada đã thu thập (JSON)
    [Required]
    [MaxLength(10000)]
    [JsonText]
    public string DataCard { get; set; } = default!;

    // Trứng Bill đang ấp (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string BillData { get; set; } = default!;

    // Dưa hấu đang trồng (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataTrongDuaHau { get; set; } = default!;

    // Trạng thái hiệu ứng du hành (JSON)
    [Required]
    [MaxLength(100)]
    [JsonText]
    public string TimeTravelDuration { get; set; } = default!;

    // Giá trị thách đấu Whis (không dùng)
    public int WhisSparring { get; set; }

    // Trạng thái biến hình (0: không)
    public int Isbienhinh { get; set; }

    // Bộ đếm chống gian lận và khóa tài khoản (JSON)
    [Required]
    [MaxLength(100)]
    [JsonText]
    public string CheckBandAccount { get; set; } = default!;

    // Cờ sư phụ không tấn công
    [Required]
    [Column(TypeName = "text")]
    public string MasterDoesNotAttack { get; set; } = default!;

    // Cờ cha không tấn công
    [Required]
    [Column(TypeName = "text")]
    public string MasterDoesNotAttackBo { get; set; } = default!;

    // Cờ mẹ không tấn công
    [Required]
    [Column(TypeName = "text")]
    public string MasterDoesNotAttackMe { get; set; } = default!;

    // Thông báo đăng nhập một lần
    [Column(TypeName = "text")]
    public string? Notify { get; set; }

    // Lượt dùng bản đồ kho báu trong ngày (JSON)
    [Required]
    [MaxLength(250)]
    [JsonText]
    public string Bandokhobau { get; set; } = default!;

    // Lần vào Doanh Trại cuối (epoch ms)
    public long BarracksState { get; set; }

    // Tiến độ Con Đường Rắn Độc (JSON)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string Conduongrandoc { get; set; } = default!;

    // Nhiệm vụ bang hội hằng ngày (JSON)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string DataClanTask { get; set; } = default!;

    // Tiến độ thành tựu (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataAchievement { get; set; } = default!;

    // Lượt dùng đặc quyền VIP (JSON)
    [JsonText]
    [Column(TypeName = "text")]
    public string? DataVip { get; set; }

    // Lần đánh boss Commeson cuối (epoch ms)
    public long Lasttimepkcommeson { get; set; }

    // Trạng thái Siêu Thần Thủy (JSON)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string Sieuthanthuy { get; set; } = default!;

    // Bảo vệ tài khoản (JSON)
    [Required]
    [MaxLength(1000)]
    [JsonText]
    public string Baovetaikhoan { get; set; } = default!;

    // Thời gian đăng nhập ban đầu
    [Column(TypeName = "timestamp without time zone")]
    public DateTime Lastimelogin { get; set; }

    // Lần đăng nhập đầu của ngày gần nhất
    [Column(TypeName = "timestamp with time zone")]
    public DateTime FirstTimeLogin { get; set; }

    // Thời gian hoạt động cuối trong game
    [Column(TypeName = "timestamp with time zone")]
    public DateTime LastTimeLoginGame { get; set; }

    // Huy hiệu đang sở hữu (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataBadges { get; set; } = default!;

    // Nhiệm vụ huy hiệu (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataTaskBadges { get; set; } = default!;

    // Rương gỗ của giải đấu 23 (JSON)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string Ruonggo { get; set; } = default!;

    // Trạng thái võ đài sinh tử (JSON)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string Vodaisinhtu { get; set; } = default!;

    // Lần gọi rồng Giáng Sinh cuối (epoch ms)
    public long DragonChristmas { get; set; }

    // Lần gọi rồng Halloween cuối (epoch ms)
    public long DragonHalloween { get; set; }

    // Bậc Siêu Hạng
    public int Rank { get; set; }

    // Dữ liệu đấu trường xếp hạng (JSON)
    [JsonText]
    [Column(TypeName = "text")]
    public string? DataSuperRank { get; set; }

    // Bộ đếm vật phẩm sự kiện (JSON)
    [Required]
    [MaxLength(1000)]
    [JsonText]
    public string DataItemEvent { get; set; } = default!;

    // Loại thẻ theo thời hạn (JSON)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string DataTheCard { get; set; } = default!;

    // Điểm danh theo từng kỳ (JSON)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string DataDiemDanh { get; set; } = default!;

    // Điểm cao máy đấm (JSON)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string DataMayDam { get; set; } = default!;

    // Dữ liệu người yêu (JSON, không dùng)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataNguoiYeu { get; set; } = default!;

    // Cờ người yêu không tấn công
    [Required]
    [Column(TypeName = "text")]
    public string LoverDoesNotAttack { get; set; } = default!;

    // Cờ con thứ nhất không tấn công
    [Required]
    [Column(TypeName = "text")]
    public string CononeDoesNotAttack { get; set; } = default!;

    // Cờ con thứ hai không tấn công
    [Required]
    [Column(TypeName = "text")]
    public string ContwoDoesNotAttack { get; set; } = default!;

    // Cờ con thứ ba không tấn công
    [Required]
    [Column(TypeName = "text")]
    public string ConthreeDoesNotAttack { get; set; } = default!;

    // Thành tích bang hội Kho Báu Dưới Biển
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string ThanhTichBang { get; set; } = default!;

    // Thành tích bang hội Hủy Diệt Khí Gas
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string ThanhTichBang2 { get; set; } = default!;

    // Thành tích bang hội Con Đường Rắn Độc
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string ThanhTichBang3 { get; set; } = default!;

    // Quà hằng ngày (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DailyGift { get; set; } = default!;

    // Mã sách kỹ năng đã học đủ (JSON)
    [JsonText]
    [Column(TypeName = "text")]
    public string? BoughtSkill { get; set; }

    // Kỹ năng đang học (JSON)
    [JsonText]
    [Column(TypeName = "text")]
    public string? LearnSkill { get; set; }

    // Điểm sự kiện chung
    public int EventPoint { get; set; }

    // Điểm sự kiện boss
    public int EventPointBoss { get; set; }

    // Điểm sự kiện Ngũ Hành Sơn
    public int EventPointNhs { get; set; }

    // Điểm sự kiện săn quái
    public int EventPointQuai { get; set; }

    // Điểm Hủy Diệt (Hakai)
    public int DiemHuyDiet { get; set; }

    // Điểm chiến trường Namếc
    public int DiemChienTruongNamek { get; set; }

    // Điểm sự kiện Tết Nguyên Đán
    public int DiemSuKienTet { get; set; }

    // Điểm sự kiện Giáng Sinh
    public int DiemSuKienGiangsinh { get; set; }

    // Điểm sự kiện Halloween
    public int DiemSuKienHalloween { get; set; }

    // Điểm sự kiện Quốc tế Phụ nữ (8/3)
    [Column("diem_su_kien_8_3")]
    public int DiemSuKien83 { get; set; }

    // Điểm sự kiện Trung Thu
    public int DiemSuKienTrungthu { get; set; }

    // Điểm sự kiện Giỗ Tổ Hùng Vương
    public int DiemSuKienHungVuong { get; set; }

    // Điểm sự kiện Black Friday
    public int DiemSuKienBlackFriday { get; set; }

    // Điểm sự kiện Phụ nữ Việt Nam (20/10)
    [Column("diem_su_kien_20_10")]
    public int DiemSuKien2010 { get; set; }

    // Bộ đếm sự kiện Năm Mới (JSON)
    [Required]
    [MaxLength(100)]
    [JsonText]
    public string DataEventNewYear { get; set; } = default!;

    // Bộ đếm sự kiện Giáng Sinh (JSON)
    [Required]
    [MaxLength(100)]
    [JsonText]
    public string DataEventChristMas { get; set; } = default!;

    // Bộ đếm sự kiện Vu Lan (JSON)
    [Required]
    [MaxLength(100)]
    [JsonText]
    public string DataEventVulan { get; set; } = default!;

    // Bộ đếm sự kiện Halloween (JSON)
    [Required]
    [MaxLength(100)]
    [JsonText]
    public string DataEventHalloween { get; set; } = default!;

    // Bộ đếm sự kiện Quốc tế Phụ nữ 8/3 (JSON)
    [Required]
    [MaxLength(100)]
    [JsonText]
    [Column("data_event_8_3")]
    public string DataEvent83 { get; set; } = default!;

    // Bộ đếm sự kiện Trung Thu (JSON)
    [Required]
    [MaxLength(100)]
    [JsonText]
    public string DataEventTrungThu { get; set; } = default!;

    // Bộ đếm sự kiện Hùng Vương (JSON)
    [Required]
    [MaxLength(100)]
    [JsonText]
    public string DataEventHungVuong { get; set; } = default!;

    // Bộ đếm sự kiện Black Friday (JSON)
    [Required]
    [MaxLength(100)]
    [JsonText]
    public string DataEventBlackFriday { get; set; } = default!;

    // Bộ đếm hoạt động quà hằng ngày (JSON)
    [Required]
    [JsonText]
    [Column(TypeName = "text")]
    public string DataEventModels { get; set; } = default!;

    // Bộ đếm và cờ hằng ngày (JSON)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string DataModels { get; set; } = default!;

    // Điểm cộng HP từ hợp thể
    public int HpPointFusion { get; set; }

    // Điểm cộng KI từ hợp thể
    public int MpPointFusion { get; set; }

    // Điểm cộng sức đánh từ hợp thể
    public int PointFusionDamage { get; set; }

    // Lần cập nhật nhiệm vụ cuối (epoch ms)
    public long LastTimeUpdateTask { get; set; }

    // Danh hiệu theo thời hạn (JSON)
    [Required]
    [MaxLength(255)]
    [JsonText]
    public string Title { get; set; } = default!;

    // Navigation property đến entity Account
    public Account? Account { get; set; }

    // Navigation property đến entity Clan
    public Clan Clan { get; set; } = default!;

    // Collection các bản ghi GiftCodeRedemption thuộc Player này
    // Mối quan hệ one-to-many với GiftCodeRedemption
    public ICollection<GiftCodeRedemption> GiftCodeRedemptions { get; set; } = new List<GiftCodeRedemption>();

    // Collection các bản ghi GoldBarReceiveHistory thuộc Player này
    // Mối quan hệ one-to-many với GoldBarReceiveHistory
    public ICollection<GoldBarReceiveHistory> GoldBarReceiveHistories { get; set; } = new List<GoldBarReceiveHistory>();

    // Collection các bản ghi ConsignItem thuộc Player này
    // Mối quan hệ one-to-many với ConsignItem
    public ICollection<ConsignItem> ConsignItems { get; set; } = new List<ConsignItem>();
}
