using Nro.Content.StaticData;
using Nro.Game.Progress.Tasks;

namespace Nro.Content.Loaders;

// Tập hợp graph J-01C; ba bước công bố ba catalog tĩnh riêng. Nguồn Java: nro/server/Manager.java (dòng 199–202, 437–705).
public static class SkillsProgressionTaskCatalogSteps
{
    // Tạo loader có kiểm số bản ghi tổng cho từng catalog. (Java: Logger.success)
    public static StaticCatalogLoader CreateLoader(IStaticRowSource source, IKolTaskItemFactory kolItems) => new(Create(source, kolItems), ExpectedCounts.SkillsProgressionTasks);

    // Tạo các bước dùng chung nguồn hàng static. (Java: loadDatabase)
    public static IReadOnlyList<ICatalogLoadStep> Create(IStaticRowSource source, IKolTaskItemFactory kolItems)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(kolItems);
        return [new SkillCatalogLoadStep(source), new ProgressionCatalogLoadStep(source), new TaskCatalogLoadStep(source, kolItems)];
    }
}
