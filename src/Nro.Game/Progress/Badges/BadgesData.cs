using System.Globalization;

namespace Nro.Game.Progress.Badges;

// Dữ liệu tiến độ huy hiệu theo từng người chơi.
// Nguồn Java: nro/badges/BadgesData.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).










public class BadgesData
{

    // Mã huy hiệu sở hữu theo chính tả IdBadGes của Java.
    public int IdBadGes { get; set; }


    // Thời điểm hết hạn hoặc bắt đầu dùng huy hiệu, tính bằng mili giây.
    public long TimeofUseBadges { get; set; }

    // Cho biết huy hiệu hiện đang được dùng.
    public bool IsUse { get; set; }


    // Khởi tạo trạng thái huy hiệu mặc định của Java.
    public BadgesData()
    {
        IdBadGes = -1;
        TimeofUseBadges = -1;
        IsUse = false;
    }


    // Phục hồi trạng thái huy hiệu từ các giá trị đã lưu.
    public BadgesData(int id, long time, bool isuse)
    {
        IdBadGes = id;
        TimeofUseBadges = time;
        IsUse = isuse;
    }






    // Tạo huy hiệu có hạn dùng từ thời điểm được truyền vào, không tự đọc đồng hồ hệ thống.
    public BadgesData(Player player, int id, int days, long nowMs)
    {
        ArgumentNullException.ThrowIfNull(player);
        IdBadGes = id;
        TimeofUseBadges = unchecked(nowMs + (unchecked(days * 24 * 60 * 60) * 1000L));
        foreach (BadgesData other in player.DataBadges)
        {
            other.IsUse = false;
        }

        IsUse = true;
        player.DataBadges.Add(this);
    }

    // Xuất trạng thái theo đúng chuỗi lưu trữ Java.
    public override string ToString()
    {
        const string n = "\"";
        return "{"
            + n + "idBadGes" + n + ":" + n + IdBadGes.ToString(CultureInfo.InvariantCulture) + n + ","
            + n + "timeofUseBadges" + n + ":" + n + TimeofUseBadges.ToString(CultureInfo.InvariantCulture) + n + ","
            + n + "isUse" + n + ":" + n + (IsUse ? "true" : "false") + n
            + "}";
    }
}
