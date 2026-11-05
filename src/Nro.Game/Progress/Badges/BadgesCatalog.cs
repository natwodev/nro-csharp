using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using Nro.Data.Db;

namespace Nro.Game.Progress.Badges;

// Catalog template huy hiệu nạp từ cơ sở dữ liệu theo thứ tự ổn định.
// Nguồn Java: nro/badges/BagesTemplate.java và nro/task/Manager.java (file phụ gom tra cứu static và nạp Manager; thứ tự id, snapshot nguyên khối được bảo toàn).





public sealed class BadgesCatalog : IBadgesCatalog
{
    // Sẽ thay bằng Nro.Content ở J-01F.
    private const string SelectBadgesSql = "select id, id_effect, id_item, name, options from data_badges order by id";
    private const string SelectTasksSql = "select id, name, max_count, id_badges_reward from task_badges_template order by id";

    private Snapshot snapshot = Snapshot.Empty;


    // Nạp huy hiệu và điều kiện theo id, sau đó thay snapshot trong một lần.
    public IReadOnlyList<BagesTemplate> BagesTemplates => Volatile.Read(ref snapshot).Badges;


    // Tìm mã vật phẩm theo hiệu ứng trong snapshot hiện hành.
    public IReadOnlyList<BadgesTaskTemplate> TaskTemplates => Volatile.Read(ref snapshot).Tasks;


    // Nạp toàn bộ dữ liệu rồi công bố snapshot mới nguyên khối. (Java: load)
    public async Task LoadAsync(INroSqlExecutor database, ItemCatalog itemCatalog, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(itemCatalog);


        var tasks = new List<BadgesTaskTemplate>();
        IResultSnapshot rs = await database.ExecuteQueryAsync(SelectTasksSql, cancellationToken);
        while (rs.Next())
        {
            tasks.Add(new BadgesTaskTemplate
            {
                Id = rs.GetInt("id"),
                Name = rs.GetString("name"),
                Count = rs.GetInt("max_count"),
                IdbadgesReward = rs.GetInt("id_badges_reward"),
            });
        }

        var badges = new List<BagesTemplate>();
        rs = await database.ExecuteQueryAsync(SelectBadgesSql, cancellationToken);
        while (rs.Next())
        {
            var template = new BagesTemplate
            {
                Id = rs.GetInt("id"),
                IdEffect = rs.GetInt("id_effect"),
                IdItem = rs.GetInt("id_item"),
                NAME = rs.GetString("name"),
            };
            template.Options.AddRange(ReadOptions(rs.GetString("options"), itemCatalog, template.Id));
            badges.Add(template);
        }

        Volatile.Write(ref snapshot, new Snapshot(new ReadOnlyCollection<BagesTemplate>(badges), new ReadOnlyCollection<BadgesTaskTemplate>(tasks)));
    }


    // Tìm mã vật phẩm theo mã hiệu ứng.
    public int FindIdItemByIdIdEffect(int idEffect)
    {
        foreach (BagesTemplate data in BagesTemplates)
        {
            if (data.IdEffect == idEffect)
            {
                return data.IdItem;
            }
        }

        return -1;
    }


    // Tìm mã hiệu ứng theo mã vật phẩm.
    public int FineIdEffectbyIdItem(int idItem)
    {
        foreach (BagesTemplate data in BagesTemplates)
        {
            if (data.IdItem == idItem)
            {
                return data.IdEffect;
            }
        }

        return -1;
    }


    // Liệt kê hiệu ứng huy hiệu đang áp dụng.
    public IReadOnlyList<int> ListEffect(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        var seen = new HashSet<int>();
        var result = new List<int>();
        foreach (BadgesData data in player.DataBadges)
        {
            foreach (BagesTemplate temp in BagesTemplates)
            {
                if (temp.IdEffect == data.IdBadGes && seen.Add(temp.IdItem))
                {
                    result.Add(temp.IdItem);
                }
            }
        }

        return result;
    }


    // Tạo danh sách tùy chọn vật phẩm do huy hiệu cấp.
    public IReadOnlyList<ItemOption> SendListItemOption(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        IReadOnlyList<ItemOption> listOptions = [];
        foreach (BadgesData data in player.DataBadges)
        {
            foreach (BagesTemplate temp in BagesTemplates)
            {
                if (data.IdBadGes == temp.IdEffect && data.IsUse)
                {
                    listOptions = temp.Options.AsReadOnly();
                }
            }
        }

        return listOptions;
    }





    private static List<ItemOption> ReadOptions(string text, ItemCatalog itemCatalog, int badgeId)
    {
        var options = new List<ItemOption>();
        JsonDocument? document = TryParse(text);
        if (document is null)
        {
            return options;
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Null)
            {
                return options;
            }

            if (root.ValueKind != JsonValueKind.Array)
            {
                throw new InvalidDataException($"data_badges id {badgeId}: the options column is not a JSON array");
            }

            foreach (JsonElement element in root.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    throw new InvalidDataException($"data_badges id {badgeId}: an option is not a JSON object");
                }

                int optionId = ReadInt(element, "id", badgeId);
                int param = ReadInt(element, "param", badgeId);
                options.Add(new ItemOption(itemCatalog.GetItemOptionTemplate(optionId), param));
            }
        }

        return options;
    }

    private static JsonDocument? TryParse(string text)
    {
        try
        {
            return JsonDocument.Parse(text);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static int ReadInt(JsonElement option, string name, int badgeId)
    {
        if (!option.TryGetProperty(name, out JsonElement value))
        {
            throw new InvalidDataException($"data_badges id {badgeId}: an option has no '{name}'");
        }

        string raw = value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : value.GetRawText();
        if (!int.TryParse(raw, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int number))
        {
            throw new InvalidDataException($"data_badges id {badgeId}: option '{name}' is not a whole number");
        }

        return number;
    }

    private sealed record Snapshot(IReadOnlyList<BagesTemplate> Badges, IReadOnlyList<BadgesTaskTemplate> Tasks)
    {
        // Snapshot rỗng dùng trước lần nạp catalog đầu tiên.
        public static readonly Snapshot Empty = new([], []);
    }
}
