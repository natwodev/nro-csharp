using Nro.Content.StaticData;

namespace Nro.Content.Loaders;

// Nguồn Java: nro/server/Manager.java (dòng 742–855, 981–1067).
// Tập bước J-01E luôn tạo trạng thái mới; lần reload tạo snapshot mới thay vì sửa snapshot cũ. (Java: loadMap)
public static class WorldCatalogSteps
{
    // Tạo loader J-01E với số snapshot đã xác nhận. (Java: Logger.success)
    public static StaticCatalogLoader CreateLoader(IStaticRowSource source) => new(Create(source), ExpectedCounts.WorldTemplates);

    // Tạo ba bước world từ một nguồn hàng, mỗi lời gọi cô lập trạng thái reload. (Java: loadMap)
    public static IReadOnlyList<ICatalogLoadStep> Create(IStaticRowSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var state = new WorldCatalogLoadState();
        return [new MobTemplateCatalogLoadStep(source, state), new NpcTemplateCatalogLoadStep(source, state), new MapTemplateCatalogLoadStep(source, state)];
    }
}
