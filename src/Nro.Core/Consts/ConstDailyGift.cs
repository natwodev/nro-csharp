namespace Nro.Core.Consts;

// Hằng số loại quà tặng hằng ngày: id trong danh sách dailyGiftData của người chơi, mỗi loại nhận được một lần mỗi ngày
// Nguồn Java: consts/ConstDailyGift.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstDailyGift
{
    // Nhận ngọc miễn phí: trong ngày được 1 đến 2 viên ngọc mỗi khi hạ một con quái (Java: NHAN_NGOC_MIEN_PHI)
    public const sbyte FREE_GEMS = 0;

    // Nhận bùa miễn phí: mỗi ngày nhận ngẫu nhiên một bùa 1 giờ tại Bà Hạt Mít (Java: NHAN_BUA_MIEN_PHI)
    public const sbyte FREE_CHARM = 1;

    // Điểm danh hằng ngày tại Ông Gôhan, Ông Moori hoặc Ông Paragus tùy hành tinh (Java: DIEM_DANH_HANG_NGAY)
    public const sbyte DAILY_CHECK_IN = 2;
}
