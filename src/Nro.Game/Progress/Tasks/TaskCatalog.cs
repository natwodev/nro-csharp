using System.Collections.ObjectModel;
using System.Globalization;
using Nro.Data.Db;

namespace Nro.Game.Progress.Tasks;

// Nạp nguyên khối và công bố snapshot bất biến để lần tra cứu đồng thời chỉ thấy bộ cũ hoặc mới. Thứ tự ORDER BY là dữ liệu: nhiệm vụ chính theo id, bước theo idmain và các template khác theo id; join trong nên nhiệm vụ không có bước bị bỏ.
// Nguồn Java: nro/task/Manager.java và nro/task/TaskService.java (giữ nguyên mã, tên, chữ ký, giá trị và thứ tự C# hiện có).














public sealed class TaskCatalog : ITaskCatalog
{
    private const string SelectMainTasksSql =
        "select m.id, m.name, m.detail, s.name as sub_name, s.max_count, s.notify, s.npc_id, s.map "
        + "from task_main_template m join task_sub_template s on m.id = s.task_main_id order by m.id, s.idmain";

    private const string SelectSideTasksSql = "select id, name, max_count_lv1, max_count_lv2, max_count_lv3, max_count_lv4, max_count_lv5 from side_task_template order by id";
    private const string SelectKolTasksSql = "select id, info, max_count from task_kol_template order by id";
    private const string SelectEventTasksSql = "select id, name, max_count_lv1, max_count_lv2, max_count_lv3, max_count_lv4, max_count_lv5 from task_event_template order by id";

    private Snapshot snapshot = Snapshot.Empty;


    // Các template nhiệm vụ chính theo thứ tự id.
    public IReadOnlyList<TaskMain> TaskMains => Volatile.Read(ref snapshot).TaskMains;


    // Các template nhiệm vụ phụ theo thứ tự id.
    public IReadOnlyList<SideTaskTemplate> SideTaskTemplates => Volatile.Read(ref snapshot).SideTaskTemplates;


    // Các template KOL theo thứ tự id.
    public IReadOnlyList<KolTaskTemplate> KolTaskTemplates => Volatile.Read(ref snapshot).KolTaskTemplates;


    // Các template nhiệm vụ sự kiện theo thứ tự id.
    public IReadOnlyList<EventTaskTemplate> EventTaskTemplates => Volatile.Read(ref snapshot).EventTaskTemplates;


    // Nạp toàn bộ dữ liệu rồi công bố snapshot mới nguyên khối. (Java: load)
    public async Task LoadAsync(INroSqlExecutor database, IKolTaskItemFactory kolItems, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(database);
        ArgumentNullException.ThrowIfNull(kolItems);

        var mainTasks = new List<TaskMain>();
        IResultSnapshot rs = await database.ExecuteQueryAsync(SelectMainTasksSql, cancellationToken);
        int taskId = -1;
        TaskMain? task = null;
        while (rs.Next())
        {
            int id = rs.GetInt("id");
            if (task is null || id != taskId)
            {
                taskId = id;
                task = new TaskMain { Id = taskId, Name = rs.GetString("name"), Detail = rs.GetString("detail") };
                mainTasks.Add(task);
            }

            task.SubTasks.Add(new SubTaskMain
            {
                Name = rs.GetString("sub_name"),
                MaxCount = rs.GetShort("max_count"),
                Notify = rs.GetString("notify"),
                NpcId = rs.GetByte("npc_id"),
                MapId = rs.GetShort("map"),
            });
        }

        var sideTasks = new List<SideTaskTemplate>();
        rs = await database.ExecuteQueryAsync(SelectSideTasksSql, cancellationToken);
        while (rs.Next())
        {
            int id = rs.GetInt("id");
            sideTasks.Add(new SideTaskTemplate(id, rs.GetString("name"), ReadRanges(rs, "side_task_template", id)));
        }

        var kolTasks = new List<KolTaskTemplate>();
        rs = await database.ExecuteQueryAsync(SelectKolTasksSql, cancellationToken);
        while (rs.Next())
        {

            int id = rs.GetInt("id");
            kolTasks.Add(new KolTaskTemplate(id, rs.GetString("info"), ParseInt(rs.GetString("max_count"), "task_kol_template", id, "max_count"), kolItems));
        }

        var eventTasks = new List<EventTaskTemplate>();
        rs = await database.ExecuteQueryAsync(SelectEventTasksSql, cancellationToken);
        while (rs.Next())
        {
            int id = rs.GetInt("id");
            eventTasks.Add(new EventTaskTemplate(id, rs.GetString("name"), ReadRanges(rs, "task_event_template", id)));
        }

        Volatile.Write(
            ref snapshot,
            new Snapshot(
                new ReadOnlyCollection<TaskMain>(mainTasks),
                new ReadOnlyCollection<SideTaskTemplate>(sideTasks),
                new ReadOnlyCollection<KolTaskTemplate>(kolTasks),
                new ReadOnlyCollection<EventTaskTemplate>(eventTasks)));
    }


    // Tạo bản sao sâu nhiệm vụ chính có các bước khởi đầu tiến độ 0. (Java: getTaskMainById)
    public TaskMain? CreateTaskMainCopy(int id)
    {
        foreach (TaskMain tm in TaskMains)
        {
            if (tm.Id == id)
            {
                return new TaskMain(tm);
            }
        }

        return null;
    }


    // Trả null cho -1, còn lại tra template theo vị trí. (Java: getSideTaskTemplateById)
    public SideTaskTemplate? GetSideTaskTemplateById(int id) => id != -1 ? At(SideTaskTemplates, id) : null;


    // Trả null cho -1, còn lại tra template theo vị trí. (Java: getEventTaskTemplateById)
    public EventTaskTemplate? GetEventTaskTemplateById(int id) => id != -1 ? At(EventTaskTemplates, id) : null;


    // Trả null khi không có hoặc ngoài bảng; đây là khác biệt có chủ đích để kết thúc KOL an toàn. (Java: getKolTaskTemplateById)
    public KolTaskTemplate? GetKolTaskTemplateById(int id)
    {
        IReadOnlyList<KolTaskTemplate> list = KolTaskTemplates;
        if (id == -1 || (uint)id >= (uint)list.Count)
        {
            return null;
        }

        return list[id];
    }

    private static T At<T>(IReadOnlyList<T> list, int index)
    {
        if ((uint)index >= (uint)list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "no task template at this position (Java: IndexOutOfBoundsException)");
        }

        return list[index];
    }


    private static int[][] ReadRanges(IResultSnapshot rs, string table, int id)
    {
        var count = new int[TaskRanges.Levels][];
        for (int level = 0; level < count.Length; level++)
        {
            string column = "max_count_lv" + (level + 1).ToString(CultureInfo.InvariantCulture);
            string[] parts = rs.GetString(column).Split('-');
            if (parts.Length < 2)
            {
                throw new InvalidDataException($"{table} id {id}: column {column} is not of the form min-max");
            }

            count[level] = [ParseInt(parts[0], table, id, column), ParseInt(parts[1], table, id, column)];
        }

        return count;
    }

    private static int ParseInt(string text, string table, int id, string column)
    {
        if (!TryParseJavaInt(text, out int value))
        {
            throw new InvalidDataException($"{table} id {id}: column {column} is not a whole number");
        }

        return value;
    }





    internal static bool TryParseJavaInt(string text, out int value)
    {
        value = 0;
        if (text.Length == 0)
        {
            return false;
        }

        int index = 0;
        bool negative = false;
        if (text[0] == '-' || text[0] == '+')
        {
            negative = text[0] == '-';
            index = 1;
            if (text.Length == 1)
            {
                return false;
            }
        }

        long accumulated = 0;
        for (; index < text.Length; index++)
        {
            int digit = CharUnicodeInfo.GetDecimalDigitValue(text[index]);
            if (digit < 0)
            {
                return false;
            }

            accumulated = (accumulated * 10) + digit;
            if (accumulated > 2147483648L)
            {
                return false;
            }
        }

        if (!negative && accumulated > int.MaxValue)
        {
            return false;
        }

        value = unchecked((int)(negative ? -accumulated : accumulated));
        return true;
    }

    private sealed record Snapshot(
        IReadOnlyList<TaskMain> TaskMains,
        IReadOnlyList<SideTaskTemplate> SideTaskTemplates,
        IReadOnlyList<KolTaskTemplate> KolTaskTemplates,
        IReadOnlyList<EventTaskTemplate> EventTaskTemplates)
    {
        // Snapshot rỗng dùng trước lần nạp dữ liệu đầu tiên.
        public static readonly Snapshot Empty = new([], [], [], []);
    }
}
