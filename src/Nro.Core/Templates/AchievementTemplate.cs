namespace Nro.Core.Templates;

// Mẫu dữ liệu mô tả một thành tựu, được nạp từ bảng thành tựu.
// Nguồn Java: nro/template/AchievementTemplate.java (giữ constructor bốn tham số duy nhất của Java, không có constructor mặc định).
public class AchievementTemplate
{
    // Thông tin hiển thị thứ nhất của thành tựu (Java: info1)
    public string Info1 { get; set; }

    // Thông tin hiển thị thứ hai của thành tựu (Java: info2)
    public string Info2 { get; set; }

    // Phần thưởng tiền của thành tựu (Java: money)
    public int Money { get; set; }

    // Số lần hoặc mức tối đa cần đạt (Java: maxCount)
    public long MaxCount { get; set; }

    public AchievementTemplate(string info1, string info2, int money, long maxCount)
    {
        Info1 = info1;
        Info2 = info2;
        Money = money;
        MaxCount = maxCount;
    }
}
