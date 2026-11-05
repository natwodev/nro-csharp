using Nro.Game.Progress.Tasks;

namespace Nro.Game.Content;

// Danh mục nhiệm vụ tĩnh chỉ đọc; bảng theo index cũng kiểm id để không che dữ liệu sai. Nguồn Java: nro/server/Manager.java (dòng 565–705).
public interface ITaskCatalog
{
    // Các nhiệm vụ chính có ít nhất một bước. (Java: TASKS)
    IReadOnlyList<TaskMain> TaskMains { get; }

    // Template nhiệm vụ phụ theo id/index. (Java: SIDE_TASKS_TEMPLATE)
    IReadOnlyList<SideTaskTemplate> SideTaskTemplates { get; }

    // Template nhiệm vụ bang hội theo id/index. (Java: CLAN_TASKS_TEMPLATE)
    IReadOnlyList<ClanTaskTemplate> ClanTaskTemplates { get; }

    // Template nhiệm vụ KOL theo id/index. (Java: KOL_TASKS_TEMPLATE)
    IReadOnlyList<KolTaskTemplate> KolTaskTemplates { get; }

    // Template nhiệm vụ sự kiện theo id/index. (Java: EVENT_TASKS_TEMPLATE)
    IReadOnlyList<EventTaskTemplate> EventTaskTemplates { get; }

    // Lấy nhiệm vụ chính theo id. (Java: TASKS)
    TaskMain GetTaskMain(int id);

    // Lấy bước theo nhiệm vụ chính và chỉ mục bước. (Java: TaskMain.subTasks)
    SubTaskMain GetSubTaskMain(int taskMainId, int index);

    // Lấy template nhiệm vụ phụ, yêu cầu index và id nhất quán. (Java: SIDE_TASKS_TEMPLATE.get)
    SideTaskTemplate GetSideTaskTemplate(int index, int id);

    // Lấy template nhiệm vụ bang hội, yêu cầu index và id nhất quán. (Java: CLAN_TASKS_TEMPLATE.get)
    ClanTaskTemplate GetClanTaskTemplate(int index, int id);

    // Lấy template nhiệm vụ KOL, yêu cầu index và id nhất quán. (Java: KOL_TASKS_TEMPLATE.get)
    KolTaskTemplate GetKolTaskTemplate(int index, int id);

    // Lấy template nhiệm vụ sự kiện, yêu cầu index và id nhất quán. (Java: EVENT_TASKS_TEMPLATE.get)
    EventTaskTemplate GetEventTaskTemplate(int index, int id);
}
