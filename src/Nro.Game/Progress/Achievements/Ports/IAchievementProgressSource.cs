namespace Nro.Game.Progress.Achievements.Ports;

// Hợp đồng lấy dữ liệu tiến độ nằm ở lớp chưa port như MagicTree; thiếu nguồn thì cấp cây được tính là 0.
// Nguồn Java: nro/achievement/Achievement.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).





public interface IAchievementProgressSource
{

    int GetMagicTreeLevel(Player player);
}
