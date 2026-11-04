using System.Collections.ObjectModel;
using Nro.Core.Templates;
using Nro.Data.Db;

namespace Nro.Game.Progress.Achievements;

// Catalog template thành tựu từ PostgreSQL, thay danh sách static Manager và công bố danh sách chỉ đọc nguyên khối.
// Nguồn Java: nro/achievement/Manager.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).


public sealed class AchievementCatalog : IAchievementCatalog
{
    private const string SelectTemplatesSql = "select id, info1, info2, money, max_count from achievement_template order by id";

    private ReadOnlyCollection<AchievementTemplate> templates = new(Array.Empty<AchievementTemplate>());


    // Các template theo chỉ mục dữ liệu.
    public IReadOnlyList<AchievementTemplate> Templates => Volatile.Read(ref templates);


    // Lấy template tại vị trí; chỉ mục ngoài bảng ném ArgumentOutOfRangeException như IndexOutOfBoundsException Java. (Java: get)
    public AchievementTemplate GetTemplate(int index)
    {
        IReadOnlyList<AchievementTemplate> list = Templates;
        if ((uint)index >= (uint)list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "no achievement template at this position (Java: IndexOutOfBoundsException)");
        }

        return list[index];
    }


    // Nạp toàn bộ dữ liệu rồi công bố snapshot mới nguyên khối. (Java: load)
    public async Task LoadAsync(INroSqlExecutor database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        IResultSnapshot rs = await database.ExecuteQueryAsync(SelectTemplatesSql, cancellationToken);
        var loaded = new List<AchievementTemplate>();
        while (rs.Next())
        {
            loaded.Add(new AchievementTemplate(rs.GetString("info1"), rs.GetString("info2"), rs.GetInt("money"), rs.GetLong("max_count")));
        }

        Volatile.Write(ref templates, new ReadOnlyCollection<AchievementTemplate>(loaded));
    }
}
