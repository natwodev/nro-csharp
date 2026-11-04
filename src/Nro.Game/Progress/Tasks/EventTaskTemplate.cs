namespace Nro.Game.Progress.Tasks;

// Template nhiệm vụ sự kiện hằng ngày bất biến, cùng cấu trúc năm mức {tối thiểu, tối đa} như nhiệm vụ phụ.
// Nguồn Java: nro/task/EventTaskTemplate.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).





public sealed class EventTaskTemplate
{
    // Mã định danh.
    public int Id { get; }

    // Tên hiển thị.
    public string Name { get; }


    // Tiến độ hoặc số lượng hiện có.
    public IReadOnlyList<IReadOnlyList<int>> Count { get; }

    // Tạo template sự kiện và đóng băng các khoảng số lượng đầu vào.
    public EventTaskTemplate(int id, string name, int[][] count)
    {
        Id = id;
        Name = name;
        Count = TaskRanges.Freeze(count);
    }
}
