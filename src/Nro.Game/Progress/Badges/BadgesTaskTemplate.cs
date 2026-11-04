namespace Nro.Game.Progress.Badges;

// Template điều kiện huy hiệu dùng chung, chỉ đọc sau khi nạp.
// Nguồn Java: nro/badges/BadgesTaskTemplate.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).





public class BadgesTaskTemplate
{
    // Mã định danh.
    public int Id { get; set; } = -1;

    // Tên hiển thị.
    public string Name { get; set; } = "";


    // Tiến độ hoặc số lượng hiện có.
    public int Count { get; set; } = -1;


    // Mã huy hiệu thưởng, giữ nguyên chính tả IdbadgesReward của Java.
    public int IdbadgesReward { get; set; } = -1;
}
