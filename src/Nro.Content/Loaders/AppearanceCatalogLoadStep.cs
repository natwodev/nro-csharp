using Nro.Content.Catalogs;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Đóng catalog ngoại hình sau khi các hàng đã được parse. Nguồn Java: nro/server/Manager.java (dòng 351–380, 560–600).
public sealed class AppearanceCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước dựng catalog, không có bảng nguồn riêng. (Java: parts)
    public const string StepName = "appearance_catalog";
    private readonly AppearanceItemCatalogLoadState state;

    // Tạo bước dựng catalog từ trạng thái cùng graph nạp. (Java: loadDatabase)
    internal AppearanceCatalogLoadStep(AppearanceItemCatalogLoadState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        this.state = state;
    }

    // Tên bước. (Java: parts)
    public string Name => StepName;

    // Chỉ dựng sau mọi bảng ngoại hình. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = [PartCatalogLoadStep.StepName, HeadAvatarCatalogLoadStep.StepName, FlagBagCatalogLoadStep.StepName, HeadAnimationFrameCatalogLoadStep.StepName];

    // Đặt catalog ngoại hình bất biến vào builder. (Java: parts)
    public Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        cancellationToken.ThrowIfCancellationRequested();
        builder.SetAppearance(new AppearanceCatalog(
            state.Parts ?? throw Missing(PartCatalogLoadStep.StepName),
            state.HeadAvatars ?? throw Missing(HeadAvatarCatalogLoadStep.StepName),
            state.FlagBags ?? throw Missing(FlagBagCatalogLoadStep.StepName),
            state.HeadFrames ?? throw Missing(HeadAnimationFrameCatalogLoadStep.StepName)));
        return Task.FromResult(new CatalogStepResult(0));
    }

    private static CatalogLoadException Missing(string stepName) => new(StepName, $"Không có dữ liệu từ bước '{stepName}'.");
}
