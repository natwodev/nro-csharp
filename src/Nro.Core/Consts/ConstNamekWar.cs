namespace Nro.Core.Consts;

// Hằng số sự kiện Tranh Ngọc Namếc: hai phe Cadic và Fide giành ngọc rồng Namếc trên một bản đồ riêng, mang ngọc về NPC phe mình để ghi điểm
// Nguồn Java: consts/ConstTranhNgocNamek.java (tên lớp đổi sang tiếng Anh; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstNamekWar
{
    // Thời lượng một trận tính bằng giây (10 phút), gửi cho client để hiện đồng hồ
    public const int TIME_SECOND = 600;

    // Thời lượng một trận tính bằng mili giây; hết giờ thì phe nhiều điểm hơn thắng, bằng điểm thì hòa
    public const long TIME = 600000;

    // Khoảng cách giữa hai lần thả ngọc rồng Namếc xuống bản đồ, tính bằng mili giây (1 phút)
    public const long LAST_TIME_DROP_BALL = 60000;

    // Id bản đồ diễn ra Tranh Ngọc Namếc
    public const int MAP_ID = 164;

    // Tên phe Fide gửi cho client (đội 2)
    public const string FIDE = "Fide";

    // Tên phe Cadic gửi cho client (đội 1)
    public const string CADIC = "Ca đíc";

    // Điểm tối đa mỗi phe (mỗi viên ngọc mang về được 1 điểm); phe nào đủ 7 điểm thắng ngay
    public const sbyte MAX_POINT = 7;

    // Số mạng tối đa gửi cho client khi mở trận; thanh mạng hiện số người còn sống mỗi phe (suy từ tên)
    public const int MAX_LIFE = 10;

    // Kết quả trận: thua
    public const sbyte LOSE = 0;

    // Kết quả trận: thắng
    public const sbyte WIN = 1;

    // Kết quả trận: hòa
    public const sbyte DRAW = 2;
}
