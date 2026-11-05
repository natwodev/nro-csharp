using System.Globalization;
using Nro.Content.Catalogs;
using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;
using Nro.Game.Progress.Tasks;

namespace Nro.Content.Loaders;

// Nạp và group nhiệm vụ theo parent; TaskMain không có bước bị loại như INNER JOIN Java. Nguồn Java: nro/server/Manager.java (dòng 565–705).
public sealed class TaskCatalogLoadStep : ICatalogLoadStep
{
    public const string StepName = "task_catalog";
    private readonly IStaticRowSource source;
    private readonly IKolTaskItemFactory kolItems;

    public TaskCatalogLoadStep(IStaticRowSource source, IKolTaskItemFactory kolItems)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(kolItems);
        this.source = source;
        this.kolItems = kolItems;
    }

    public string Name => StepName;
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<TaskMainTemplateRow> mainRows = await source.ReadTaskMainsAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<TaskSubTemplateRow> subRows = await source.ReadTaskSubsAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<SideTaskTemplateRow> sideRows = await source.ReadSideTaskTemplatesAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<ClanTaskTemplateRow> clanRows = await source.ReadClanTaskTemplatesAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<TaskKolTemplateRow> kolRows = await source.ReadKolTaskTemplatesAsync(cancellationToken).ConfigureAwait(false);
        IReadOnlyList<TaskEventTemplateRow> eventRows = await source.ReadEventTaskTemplatesAsync(cancellationToken).ConfigureAwait(false);
        var mainIds = new HashSet<int>();
        var mains = new Dictionary<int, TaskMain>();
        foreach (TaskMainTemplateRow row in mainRows)
        {
            StaticCatalogValidation.Unique(mainIds, row.TaskMainTemplateId, StepName);
            mains.Add(row.TaskMainTemplateId, new TaskMain { Id = row.TaskMainTemplateId, Name = row.Name, Detail = row.Detail });
        }

        var subIds = new HashSet<int>();
        foreach (TaskSubTemplateRow row in subRows)
        {
            StaticCatalogValidation.Unique(subIds, row.Idmain, StepName);
            if (!mains.TryGetValue(row.TaskMainId, out TaskMain? main))
            {
                throw new CatalogLoadException(StepName, $"task_sub_template idmain {row.Idmain} tham chiếu task_main_id {row.TaskMainId} không tồn tại.");
            }

            main.SubTasks.Add(new SubTaskMain
            {
                Name = row.Name,
                MaxCount = StaticCatalogValidation.Short(row.MaxCount, StepName, "max_count", row.Idmain),
                Notify = row.Notify,
                NpcId = StaticCatalogValidation.SByte(row.NpcId, StepName, "npc_id", row.Idmain),
                MapId = StaticCatalogValidation.Short(row.Map, StepName, "map", row.Idmain),
            });
        }

        // Java JOIN chỉ thêm main khi có sub; giữ đúng quirk này, nhưng source đã có FK được kiểm riêng.
        List<TaskMain> joinedMains = mains.Values.Where(value => value.SubTasks.Count != 0).OrderBy(value => value.Id).ToList();
        List<SideTaskTemplate> side = CreateSide(sideRows);
        List<ClanTaskTemplate> clan = CreateClan(clanRows);
        List<KolTaskTemplate> kol = CreateKol(kolRows);
        List<EventTaskTemplate> events = CreateEvent(eventRows);
        Continuous(side.Select(value => value.Id), "side_task_template");
        Continuous(clan.Select(value => value.Id), "clan_task_template");
        Continuous(kol.Select(value => value.Id), "task_kol_template");
        Continuous(events.Select(value => value.Id), "task_event_template");
        builder.SetTasks(new Nro.Content.Catalogs.TaskCatalog(joinedMains, side, clan, kol, events, kolItems));
        return new CatalogStepResult(joinedMains.Count + subRows.Count + sideRows.Count + clanRows.Count + kolRows.Count + eventRows.Count);
    }

    private static void Continuous(IEnumerable<int> ids, string table) => StaticCatalogValidation.ContinuousZeroBasedIds(ids, StepName + ":" + table);
    private static List<SideTaskTemplate> CreateSide(IEnumerable<SideTaskTemplateRow> rows)
    {
        var ids = new HashSet<int>();
        var result = new List<SideTaskTemplate>();
        foreach (SideTaskTemplateRow row in rows) { StaticCatalogValidation.Unique(ids, row.SideTaskTemplateId, StepName); result.Add(new SideTaskTemplate(row.SideTaskTemplateId, row.Name, Ranges(row, row.SideTaskTemplateId))); }
        return result;
    }
    private static List<ClanTaskTemplate> CreateClan(IEnumerable<ClanTaskTemplateRow> rows)
    {
        var ids = new HashSet<int>();
        var result = new List<ClanTaskTemplate>();
        foreach (ClanTaskTemplateRow row in rows) { StaticCatalogValidation.Unique(ids, row.ClanTaskTemplateId, StepName); result.Add(new ClanTaskTemplate(row.ClanTaskTemplateId, row.Name, Ranges(row, row.ClanTaskTemplateId))); }
        return result;
    }
    private List<KolTaskTemplate> CreateKol(IEnumerable<TaskKolTemplateRow> rows)
    {
        var ids = new HashSet<int>();
        var result = new List<KolTaskTemplate>();
        foreach (TaskKolTemplateRow row in rows) { StaticCatalogValidation.Unique(ids, row.TaskKolTemplateId, StepName); result.Add(new KolTaskTemplate(row.TaskKolTemplateId, row.Info, ParseInt(row.MaxCount, "task_kol_template", row.TaskKolTemplateId, "max_count"), kolItems)); }
        return result;
    }
    private static List<EventTaskTemplate> CreateEvent(IEnumerable<TaskEventTemplateRow> rows)
    {
        var ids = new HashSet<int>();
        var result = new List<EventTaskTemplate>();
        foreach (TaskEventTemplateRow row in rows) { StaticCatalogValidation.Unique(ids, row.TaskEventTemplateId, StepName); result.Add(new EventTaskTemplate(row.TaskEventTemplateId, row.Name, Ranges(row, row.TaskEventTemplateId))); }
        return result;
    }
    private static int[][] Ranges(SideTaskTemplateRow row, int id) => Ranges(row.MaxCountLv1, row.MaxCountLv2, row.MaxCountLv3, row.MaxCountLv4, row.MaxCountLv5, "side_task_template", id);
    private static int[][] Ranges(ClanTaskTemplateRow row, int id) => Ranges(row.MaxCountLv1, row.MaxCountLv2, row.MaxCountLv3, row.MaxCountLv4, row.MaxCountLv5, "clan_task_template", id);
    private static int[][] Ranges(TaskEventTemplateRow row, int id) => Ranges(row.MaxCountLv1, row.MaxCountLv2, row.MaxCountLv3, row.MaxCountLv4, row.MaxCountLv5, "task_event_template", id);
    private static int[][] Ranges(string one, string two, string three, string four, string five, string table, int id) => [Range(one, table, id, 1), Range(two, table, id, 2), Range(three, table, id, 3), Range(four, table, id, 4), Range(five, table, id, 5)];
    private static int[] Range(string text, string table, int id, int level)
    {
        string[] pieces = text.Split('-', StringSplitOptions.None);
        if (pieces.Length != 2)
        {
            throw new CatalogLoadException(StepName, $"{table} id {id} có max_count_lv{level} không theo dạng min-max.");
        }

        return [ParseInt(pieces[0], table, id, $"max_count_lv{level}"), ParseInt(pieces[1], table, id, $"max_count_lv{level}")];
    }

    private static int ParseInt(string text, string table, int id, string field) => Int32.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int value)
        ? value
        : throw new CatalogLoadException(StepName, $"{table} id {id} có {field} không phải Int32 Java hợp lệ.");
}
