using Nro.Core.Templates;
using Nro.Content.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp vật trang trí nền vào trạng thái cục bộ. Nguồn Java: nro/server/Manager.java (dòng 397–401).
public sealed class BgItemCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước và khóa ExpectedCounts. (Java: bg_item_template)
    public const string StepName = "bg_item_template";
    private readonly IStaticRowSource source;
    private readonly AppearanceItemCatalogLoadState state;

    // Tạo bước nạp từ nguồn hàng được tiêm. (Java: bg_item_template)
    internal BgItemCatalogLoadStep(IStaticRowSource source, AppearanceItemCatalogLoadState state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(state);
        this.source = source;
        this.state = state;
    }

    // Tên bước. (Java: bg_item_template)
    public string Name => StepName;

    // Nền không phụ thuộc catalog khác khi đọc. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    // Chuyển các hàng nền và kiểm id không trùng. (Java: BG_ITEMS)
    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<Nro.Data.Entities.StaticData.BgItemTemplateRow> rows = await source.ReadBgItemsAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var items = new List<BgItem>(rows.Count);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StaticCatalogValidation.Unique(ids, row.BgItemTemplateId, StepName);
            items.Add(new BgItem
            {
                Id = row.BgItemTemplateId,
                IdImage = StaticCatalogValidation.Short(row.ImageId, StepName, "image_id", row.BgItemTemplateId),
                Layer = StaticCatalogValidation.SByte(row.Layer, StepName, "layer", row.BgItemTemplateId),
                Dx = StaticCatalogValidation.Short(row.Dx, StepName, "dx", row.BgItemTemplateId),
                Dy = StaticCatalogValidation.Short(row.Dy, StepName, "dy", row.BgItemTemplateId),
            });
        }

        state.BgItems = items.AsReadOnly();
        return new CatalogStepResult(items.Count);
    }
}
