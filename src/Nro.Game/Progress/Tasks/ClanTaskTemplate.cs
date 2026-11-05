namespace Nro.Game.Progress.Tasks;

// Template nhiệm vụ bang hội bất biến, gồm năm mức khoảng số lượng. Nguồn Java: nro/server/Manager.java (dòng 627–651).
public sealed class ClanTaskTemplate
{
    // Mã định danh. (Java: ClanTaskTemplate.id)
    public int Id { get; }

    // Tên hiển thị. (Java: ClanTaskTemplate.name)
    public string Name { get; }

    // Năm khoảng {tối thiểu, tối đa} chỉ đọc. (Java: ClanTaskTemplate.count)
    public IReadOnlyList<IReadOnlyList<int>> Count { get; }

    // Đóng băng dữ liệu khoảng đã kiểm trước khi công bố. (Java: ClanTaskTemplate)
    public ClanTaskTemplate(int id, string name, int[][] count)
    {
        Id = id;
        Name = name;
        Count = TaskRanges.Freeze(count);
    }
}
