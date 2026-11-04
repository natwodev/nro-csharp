namespace Nro.Game.Progress.Tasks;

// Bản nạp là template dùng chung, không được giao trực tiếp cho người chơi; bản sao sâu đặt tiến độ từng bước về 0 và cố ý không chép LastTime theo Java.
// Nguồn Java: nro/task/TaskMain.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).






public class TaskMain
{
    // Mã định danh.
    public int Id { get; set; }

    // Chỉ mục trong danh sách.
    public int Index { get; set; }

    // Tên hiển thị.
    public string Name { get; set; } = "";

    // Mô tả chi tiết.
    public string Detail { get; set; } = "";

    // Các bước của nhiệm vụ chính.
    public List<SubTaskMain> SubTasks { get; set; }

    // Mốc thời gian xử lý gần nhất.
    public long LastTime { get; set; }

    // Khởi tạo danh sách bước trống cho template hoặc bản sao.
    public TaskMain()
    {
        SubTasks = [];
    }


    // Sao sâu mô tả và các bước, đồng thời đặt Count từng bước về 0. (Java: TaskMain(TaskMain))
    public TaskMain(TaskMain taskMain)
    {
        ArgumentNullException.ThrowIfNull(taskMain);
        Id = taskMain.Id;
        Index = taskMain.Index;
        Name = taskMain.Name;
        Detail = taskMain.Detail;
        SubTasks = [];
        foreach (SubTaskMain stm in taskMain.SubTasks)
        {
            SubTasks.Add(new SubTaskMain(stm));
        }
    }
}
