using System.Collections.ObjectModel;
using Nro.Data.Db;

namespace Nro.Game.Skills.Attributes;

// Nạp và quản lý mẫu thuộc tính tăng cường toàn máy chủ.
// Sẽ thay bằng Nro.Content ở J-01F.
// Nguồn Java: nro/attribute/AttributeTemplateManager.java (không dùng singleton; copy-on-write dưới lock trả snapshot bất biến; thêm ORDER BY id và ném lỗi DB thay vì in).
public sealed class AttributeTemplateManager : IAttributeTemplateManager
{
    private const string SelectTemplatesSql = "select id, name from attribute_template order by id";

    private readonly object gate = new();
    private ReadOnlyCollection<AttributeTemplate> list = new(Array.Empty<AttributeTemplate>());

    // Snapshot bất biến của các mẫu theo thứ tự nạp.
    public IReadOnlyList<AttributeTemplate> Templates => Volatile.Read(ref list);

    // Nạp và nối thêm attribute_template theo id; lỗi DB làm thất bại toàn lượt (Java: load).
    public async Task LoadAsync(INroSqlExecutor database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        IResultSnapshot rs = await database.ExecuteQueryAsync(SelectTemplatesSql, cancellationToken);
        var loaded = new List<AttributeTemplate>();
        while (rs.Next())
        {
            int id = rs.GetInt("id");
            string name = rs.GetString("name");
            loaded.Add(new AttributeTemplate(id, name));
        }

        lock (gate)
        {
            list = new ReadOnlyCollection<AttributeTemplate>([.. list, .. loaded]);
        }
    }

    // Thêm mẫu bằng copy-on-write (Java: add).
    public void Add(AttributeTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);
        lock (gate)
        {
            list = new ReadOnlyCollection<AttributeTemplate>([.. list, template]);
        }
    }

    // Xóa tham chiếu mẫu đầu tiên bằng copy-on-write (Java: remove).
    public bool Remove(AttributeTemplate template)
    {
        lock (gate)
        {
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], template))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                return false;
            }

            var copy = new List<AttributeTemplate>(list);
            copy.RemoveAt(index);
            list = new ReadOnlyCollection<AttributeTemplate>(copy);
            return true;
        }
    }

    // Tìm mẫu đầu tiên theo id trong snapshot (Java: find).
    public AttributeTemplate? Find(int id)
    {
        foreach (AttributeTemplate template in Templates)
        {
            if (template.Id == id)
            {
                return template;
            }
        }

        return null;
    }
}
