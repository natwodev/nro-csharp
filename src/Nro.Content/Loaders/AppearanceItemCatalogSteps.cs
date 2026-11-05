using Nro.Content.StaticData;

namespace Nro.Content.Loaders;

// Tập hợp graph bước J-01B với trạng thái riêng cho mỗi lần tạo loader. Nguồn Java: nro/server/Manager.java (dòng 351–436, 560–600, 727–741).
public static class AppearanceItemCatalogSteps
{
    // Tạo loader J-01B với số snapshot bắt buộc để CatalogLoadReport chỉ công bố dump đầy đủ. (Java: Logger.success)
    public static StaticCatalogLoader CreateLoader(IStaticRowSource source) => new(Create(source), ExpectedCounts.AppearanceItems);

    // Tạo các bước nạp appearance, items, images và mount từ một nguồn hàng. (Java: loadDatabase)
    public static IReadOnlyList<ICatalogLoadStep> Create(IStaticRowSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var state = new AppearanceItemCatalogLoadState();
        return
        [
            new PartCatalogLoadStep(source, state),
            new BgItemCatalogLoadStep(source, state),
            new ItemTemplateCatalogLoadStep(source, state),
            new ItemOptionTemplateCatalogLoadStep(source, state),
            new HeadAvatarCatalogLoadStep(source, state),
            new FlagBagCatalogLoadStep(source, state),
            new HeadAnimationFrameCatalogLoadStep(source, state),
            new ImageCatalogLoadStep(source, state),
            new AppearanceCatalogLoadStep(state),
            new ItemCatalogLoadStep(state),
        ];
    }
}
