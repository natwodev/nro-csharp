namespace Nro.Game.Progress.Tasks;

// Mỗi người chơi sở hữu trạng thái riêng; TaskMain phải là bản sao sâu. ClanTask chưa port (DEFERRED track I-04); Dispose giữ ba thành phần do constructor tạo, chỉ hủy KOL và quên nhiệm vụ chính.
// Nguồn Java: nro/task/TaskPlayer.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).











public class TaskPlayer
{

    // Nhiệm vụ chính hiện tại; null khi chưa nhận.
    public TaskMain? TaskMain { get; set; }

    // Trạng thái nhiệm vụ phụ riêng của người chơi.
    public SideTask SideTask { get; set; }

    // Trạng thái chuỗi nhiệm vụ KOL riêng của người chơi.
    public KolTask KolTask { get; set; }

    // Trạng thái nhiệm vụ sự kiện riêng của người chơi.
    public EventTask EventTask { get; set; }

    // Khởi tạo ba trạng thái nhiệm vụ riêng cho người chơi.
    public TaskPlayer()
    {
        EventTask = new EventTask();
        SideTask = new SideTask();
        KolTask = new KolTask();
    }


    // Dọn trạng thái theo quy tắc thành phần không bị gán null. (Java: dispose)
    public void Dispose()
    {
        TaskMain = null;
        KolTask.Dispose();
    }
}
