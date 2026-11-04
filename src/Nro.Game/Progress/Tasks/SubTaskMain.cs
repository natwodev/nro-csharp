namespace Nro.Game.Progress.Tasks;

// Bước template có Count bằng 0; mỗi người chơi dùng bản sao riêng, cũng khởi đầu Count bằng 0.
// Nguồn Java: nro/task/SubTaskMain.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).





public class SubTaskMain
{

    // Tiến độ hoặc số lượng hiện có.
    public short Count { get; set; }

    // Tên hiển thị.
    public string Name { get; set; } = "";


    // Số lượng cần đạt.
    public short MaxCount { get; set; }

    // Nội dung hướng dẫn hoặc thông báo.
    public string Notify { get; set; } = "";


    // Mã NPC giao bước nhiệm vụ.
    public sbyte NpcId { get; set; }

    // Mã bản đồ của bước nhiệm vụ.
    public short MapId { get; set; }

    // Khởi tạo bước trống để nạp template.
    public SubTaskMain()
    {
    }


    // Sao mô tả bước và luôn bắt đầu tiến độ bằng 0. (Java: SubTaskMain(SubTaskMain))
    public SubTaskMain(SubTaskMain stm)
    {
        ArgumentNullException.ThrowIfNull(stm);
        Count = 0;
        Name = stm.Name;
        MaxCount = stm.MaxCount;
        NpcId = stm.NpcId;
        MapId = stm.MapId;
        Notify = stm.Notify;
    }
}
