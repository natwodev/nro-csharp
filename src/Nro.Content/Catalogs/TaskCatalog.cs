using System.Collections.ObjectModel;
using Nro.Game.Content;
using Nro.Game.Progress.Tasks;

namespace Nro.Content.Catalogs;

// Danh mục nhiệm vụ đóng băng các template và chỉ trả bản sao cho kiểu miền còn mutable. Nguồn Java: nro/server/Manager.java (dòng 565–705).
public sealed class TaskCatalog : Nro.Game.Content.ITaskCatalog
{
    private readonly IReadOnlyDictionary<int, TaskMain> mainsById;
    private readonly IReadOnlyList<SideTaskTemplate> side;
    private readonly IReadOnlyList<ClanTaskTemplate> clan;
    private readonly IReadOnlyList<KolTaskTemplate> kol;
    private readonly IReadOnlyList<EventTaskTemplate> events;
    private readonly IKolTaskItemFactory kolItems;

    // Đóng băng các bảng sau khi loader đã kiểm id/index và FK. (Java: TASKS)
    public TaskCatalog(IEnumerable<TaskMain> taskMains, IEnumerable<SideTaskTemplate> sideTasks, IEnumerable<ClanTaskTemplate> clanTasks, IEnumerable<KolTaskTemplate> kolTasks, IEnumerable<EventTaskTemplate> eventTasks, IKolTaskItemFactory kolItems)
    {
        ArgumentNullException.ThrowIfNull(taskMains);
        ArgumentNullException.ThrowIfNull(sideTasks);
        ArgumentNullException.ThrowIfNull(clanTasks);
        ArgumentNullException.ThrowIfNull(kolTasks);
        ArgumentNullException.ThrowIfNull(eventTasks);
        ArgumentNullException.ThrowIfNull(kolItems);
        this.kolItems = kolItems;
        mainsById = new ReadOnlyDictionary<int, TaskMain>(taskMains.ToDictionary(value => value.Id, Clone));
        side = Array.AsReadOnly(sideTasks.ToArray());
        clan = Array.AsReadOnly(clanTasks.ToArray());
        kol = Array.AsReadOnly(kolTasks.Select(Clone).ToArray());
        events = Array.AsReadOnly(eventTasks.ToArray());
    }

    public IReadOnlyList<TaskMain> TaskMains => Array.AsReadOnly(mainsById.Values.OrderBy(value => value.Id).Select(Clone).ToArray());
    public IReadOnlyList<SideTaskTemplate> SideTaskTemplates => side;
    public IReadOnlyList<ClanTaskTemplate> ClanTaskTemplates => clan;
    public IReadOnlyList<KolTaskTemplate> KolTaskTemplates => Array.AsReadOnly(kol.Select(Clone).ToArray());
    public IReadOnlyList<EventTaskTemplate> EventTaskTemplates => events;

    public TaskMain GetTaskMain(int id) => mainsById.TryGetValue(id, out TaskMain? value)
        ? Clone(value)
        : throw new KeyNotFoundException($"Không có nhiệm vụ chính id {id}.");

    public SubTaskMain GetSubTaskMain(int taskMainId, int index)
    {
        TaskMain task = GetTaskMain(taskMainId);
        return (uint)index < (uint)task.SubTasks.Count
            ? new SubTaskMain(task.SubTasks[index])
            : throw new ArgumentOutOfRangeException(nameof(index), index, "Không có bước nhiệm vụ tại chỉ mục đã cho.");
    }

    public SideTaskTemplate GetSideTaskTemplate(int index, int id) => At(side, index, id, value => value.Id, "nhiệm vụ phụ");
    public ClanTaskTemplate GetClanTaskTemplate(int index, int id) => At(clan, index, id, value => value.Id, "nhiệm vụ bang hội");
    public KolTaskTemplate GetKolTaskTemplate(int index, int id) => Clone(At(kol, index, id, value => value.Id, "nhiệm vụ KOL"));
    public EventTaskTemplate GetEventTaskTemplate(int index, int id) => At(events, index, id, value => value.Id, "nhiệm vụ sự kiện");

    private static T At<T>(IReadOnlyList<T> values, int index, int id, Func<T, int> getId, string kind)
    {
        if ((uint)index >= (uint)values.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, $"Không có {kind} tại chỉ mục đã cho.");
        }

        T value = values[index];
        if (getId(value) != id)
        {
            throw new KeyNotFoundException($"{kind} tại chỉ mục {index} có id {getId(value)}, không phải {id}.");
        }

        return value;
    }

    private static TaskMain Clone(TaskMain source) => new(source) { Index = source.Index, LastTime = source.LastTime };
    private KolTaskTemplate Clone(KolTaskTemplate source) => new(source.Id, source.Info, source.MaxCount, kolItems);
}
