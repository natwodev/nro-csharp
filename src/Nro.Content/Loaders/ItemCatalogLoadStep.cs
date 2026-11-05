using Nro.Content.Catalogs;
using Nro.Core.Templates;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Đóng catalog vật phẩm và dẫn xuất ánh xạ thú cưỡi sau khi có ảnh. Nguồn Java: nro/server/Manager.java (dòng 397–436, 727–741).
public sealed class ItemCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước đồng thời là khóa số thú cưỡi ExpectedCounts. (Java: DataGame.MAP_MOUNT_NUM)
    public const string StepName = "mount_map";
    private readonly AppearanceItemCatalogLoadState state;

    // Tạo bước dựng catalog từ trạng thái cùng graph nạp. (Java: loadDatabase)
    internal ItemCatalogLoadStep(AppearanceItemCatalogLoadState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        this.state = state;
    }

    // Tên bước. (Java: DataGame.MAP_MOUNT_NUM)
    public string Name => StepName;

    // Mount cần item và catalog ảnh; catalog item cần toàn bộ bảng vật phẩm. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = [ItemTemplateCatalogLoadStep.StepName, ItemOptionTemplateCatalogLoadStep.StepName, BgItemCatalogLoadStep.StepName, ImageCatalogLoadStep.StepName];

    // Dẫn xuất item type 23 có ảnh mount_{part}_0 thành part + 30000. (Java: DataGame.MAP_MOUNT_NUM)
    public Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<ItemTemplate> items = state.ItemTemplates ?? throw Missing(ItemTemplateCatalogLoadStep.StepName);
        var mounts = new Dictionary<short, int>();
        foreach (ItemTemplate item in items)
        {
            if (item.Type == 23 && (state.Images ?? throw Missing(ImageCatalogLoadStep.StepName)).TryGetFrame($"mount_{item.Part}_0", out _))
            {
                if (!mounts.TryAdd(item.Id, item.Part + 30000))
                {
                    throw new CatalogLoadException(StepName, $"Mẫu thú cưỡi id {item.Id} bị trùng.");
                }
            }
        }

        builder.SetItems(new ItemCatalog(
            items,
            state.ItemOptionTemplates ?? throw Missing(ItemOptionTemplateCatalogLoadStep.StepName),
            state.BgItems ?? throw Missing(BgItemCatalogLoadStep.StepName),
            mounts));
        return Task.FromResult(new CatalogStepResult(mounts.Count));
    }

    private static CatalogLoadException Missing(string stepName) => new(StepName, $"Không có dữ liệu từ bước '{stepName}'.");
}
