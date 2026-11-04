using System.Collections.ObjectModel;

namespace Nro.Game.Progress.Tasks;

// Template bất biến, dùng chung được; Count[level][0] là tối thiểu và [1] là tối đa. Khác Java: mảng public có thể sửa đã được đóng băng.
// Nguồn Java: nro/task/SideTaskTemplate.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).








public sealed class SideTaskTemplate
{
    // Mã định danh.
    public int Id { get; }

    // Tên hiển thị.
    public string Name { get; }


    // Tiến độ hoặc số lượng hiện có.
    public IReadOnlyList<IReadOnlyList<int>> Count { get; }

    // Tạo template và đóng băng các khoảng số lượng đầu vào.
    public SideTaskTemplate(int id, string name, int[][] count)
    {
        Id = id;
        Name = name;
        Count = TaskRanges.Freeze(count);
    }
}


internal static class TaskRanges
{

    // Số mức khó cố định của Java new int[5][2].
    public const int Levels = 5;


    // Đóng băng năm khoảng {tối thiểu, tối đa} để không ai sửa template dùng chung.
    public static IReadOnlyList<IReadOnlyList<int>> Freeze(int[][] count)
    {
        ArgumentNullException.ThrowIfNull(count);
        if (count.Length != Levels)
        {
            throw new ArgumentException($"A task template needs exactly {Levels} difficulty levels", nameof(count));
        }

        var levels = new List<IReadOnlyList<int>>(Levels);
        foreach (int[] range in count)
        {
            if (range is null || range.Length != 2)
            {
                throw new ArgumentException("Every difficulty level needs exactly two numbers (minimum, maximum)", nameof(count));
            }

            levels.Add(new ReadOnlyCollection<int>([range[0], range[1]]));
        }

        return new ReadOnlyCollection<IReadOnlyList<int>>(levels);
    }
}
