using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Data.Db;

namespace Nro.Game.Skills.Attributes;

// Nạp, lưu và cập nhật các tăng cường toàn máy chủ.
// Sẽ thay bằng Nro.Content ở J-01F.
// Nguồn Java: nro/attribute/AttributeManager.java, Manager.loadAttributeServer/updateAttributeServer (tiêm VietnamClock; SQL PostgreSQL có tham số và ORDER BY id; một lock cùng snapshot tránh lỗi duyệt đa luồng).
public sealed class AttributeManager : IAttributeManager
{
    private const string SelectAttributesSql = "select id, attribute_template_id, value, time from attribute_server order by id";
    private const string UpdateAttributeSql = "update attribute_server set attribute_template_id = ?, value = ?, time = ? where id = ?";

    // Khoảng cách tick tối thiểu độc quyền 1 giây theo Util.canDoWithTime Java.
    private const long UpdateGapMs = 1000;

    private readonly object gate = new();
    private readonly List<Attribute> attributes = [];
    private readonly VietnamClock clock;
    private long lastUpdate;

    public AttributeManager(VietnamClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        this.clock = clock;
    }

    // Trả snapshot danh sách sống để có thể duyệt khi luồng khác thêm/xóa (Java: getAttributes).
    public IReadOnlyList<Attribute> Attributes
    {
        get
        {
            lock (gate)
            {
                return attributes.ToArray();
            }
        }
    }

    // Nạp và nối thêm attribute_server theo id; mẫu không tồn tại làm hỏng toàn lượt nạp (Java: loadAttributeServer).
    public async Task LoadAsync(INroSqlExecutor database, IAttributeTemplateManager templates, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(templates);
        IResultSnapshot rs = await database.ExecuteQueryAsync(SelectAttributesSql, cancellationToken);
        var loaded = new List<Attribute>();
        while (rs.Next())
        {
            int id = rs.GetInt("id");
            int templateId = rs.GetInt("attribute_template_id");
            int value = rs.GetInt("value");
            int time = rs.GetInt("time");
            AttributeTemplate template = templates.Find(templateId)
                ?? throw new InvalidOperationException($"attribute_server row {id} refers to attribute template {templateId}, which does not exist");
            loaded.Add(new Attribute(id, template, value, time));
        }

        lock (gate)
        {
            attributes.AddRange(loaded);
        }
    }

    // Ghi các bản ghi đã đổi nhưng cố ý không xóa cờ changed, trả số dòng ghi (Java: updateAttributeServer).
    public async Task<int> SaveChangedAsync(INroSqlExecutor database, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        var toSave = new List<(int Id, int TemplateId, int Value, int Time)>();
        lock (gate)
        {
            foreach (Attribute at in attributes)
            {
                if (at.IsChanged)
                {
                    toSave.Add((at.Id, at.Template.Id, at.Value, at.Time));
                }
            }
        }

        int written = 0;
        foreach ((int id, int templateId, int value, int time) in toSave)
        {
            written += await database.ExecuteUpdateAsync(UpdateAttributeSql, [templateId, value, time, id], cancellationToken);
        }

        return written;
    }

    // Thêm thuộc tính dưới cùng khóa (Java: add).
    public void Add(Attribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        lock (gate)
        {
            attributes.Add(attribute);
        }
    }

    // Xóa đối tượng trùng tham chiếu đầu tiên dưới khóa (Java: remove).
    public bool Remove(Attribute attribute)
    {
        lock (gate)
        {
            int index = attributes.FindIndex(a => ReferenceEquals(a, attribute));
            if (index < 0)
            {
                return false;
            }

            attributes.RemoveAt(index);
            return true;
        }
    }

    // Tìm thuộc tính đầu tiên theo mã mẫu; Template luôn khác null nhờ lúc nạp (Java: find).
    public Attribute? Find(int templateId)
    {
        lock (gate)
        {
            foreach (Attribute at in attributes)
            {
                if (at.Template.Id == templateId)
                {
                    return at;
                }
            }
        }

        return null;
    }

    // Mỗi hơn 1 giây trừ thời hạn mọi thuộc tính chưa hết hạn, dưới một khóa (Java: update).
    public void Update()
    {
        long now = clock.CurrentTimeMillis();
        lock (gate)
        {
            if (now - lastUpdate > UpdateGapMs)
            {
                lastUpdate = now;
                foreach (Attribute at in attributes)
                {
                    if (!at.IsExpired())
                    {
                        at.Update();
                    }
                }
            }
        }
    }

    // Đổi thời hạn thuộc tính đầu tiên nhưng luôn trả false để giữ quirk Java (Java: setTime).
    public bool SetTime(int templateId, int time)
    {
        Attribute? attr = Find(templateId);
        if (attr is not null)
        {
            attr.SetTime(time);
        }

        return false;
    }
}
    // Tạo quản lý với đồng hồ Việt Nam được tiêm, không dùng đồng hồ hệ thống.
