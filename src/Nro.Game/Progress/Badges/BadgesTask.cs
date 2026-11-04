using System.Globalization;

namespace Nro.Game.Progress.Badges;

// Tiến độ một điều kiện huy hiệu của người chơi.
// Nguồn Java: nro/badges/BadgesTask.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).











public class BadgesTask
{
    // Mã định danh.
    public int Id { get; set; } = -1;

    // Tiến độ hoặc số lượng hiện có.
    public int Count { get; set; } = -1;

    // Ngưỡng tiến độ phải đạt để hoàn tất điều kiện.
    public int CountMax { get; set; } = -1;

    // Mã huy hiệu được thưởng khi hoàn tất.
    public int IdBadgesReward { get; set; } = -1;

    // Kiểm tra tiến độ đã đạt ngưỡng của template hiện tại. (Java: isDone)
    public bool IsDone() => Count >= CountMax;



    // Tính phần trăm; giữ phép nhân int 32-bit có thể tràn như Java. (Java: getPercentProcess)
    public int GetPercentProcess()
    {
        if (Count >= CountMax)
        {
            return 100;
        }

        return unchecked((int)((long)Count * 100 / CountMax));
    }


    // Xuất trạng thái theo đúng chuỗi lưu trữ Java.
    public override string ToString()
    {
        const string n = "\"";
        return "{" + n + "id" + n + ":" + n + Id.ToString(CultureInfo.InvariantCulture) + n + ","
            + n + "count" + n + ":" + n + Count.ToString(CultureInfo.InvariantCulture) + n + ","
            + n + "countMax" + n + ":" + n + CountMax.ToString(CultureInfo.InvariantCulture) + n + ","
            + n + "idBadgesReward" + n + ":" + n + IdBadgesReward.ToString(CultureInfo.InvariantCulture) + n + "}";
    }
}
