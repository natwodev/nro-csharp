namespace Nro.Game.Progress.Badges;

// Trạng thái huy hiệu của người chơi, giữ tách biệt giữa người chơi và các quy tắc hoàn thành của Java.
// Nguồn Java: nro/badges/Badges.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).




public class Badges
{

    // Mã huy hiệu đang bật; -1 nghĩa là chưa chọn.
    public int IdBadges { get; set; } = -1;

    // Mốc gửi thông tin huy hiệu gần nhất.
    public long LastTimeSendBadges { get; set; }
}
