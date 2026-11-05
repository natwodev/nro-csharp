using Nro.Core.Templates;
using Nro.Content.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp mẫu tùy chọn vật phẩm vào trạng thái cục bộ. Nguồn Java: nro/server/Manager.java (dòng 421–436).
public sealed class ItemOptionTemplateCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước và khóa ExpectedCounts. (Java: item_option_template)
    public const string StepName = "item_option_template";
    private readonly IStaticRowSource source;
    private readonly AppearanceItemCatalogLoadState state;

    // Tạo bước nạp từ nguồn hàng được tiêm. (Java: item_option_template)
    internal ItemOptionTemplateCatalogLoadStep(IStaticRowSource source, AppearanceItemCatalogLoadState state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(state);
        this.source = source;
        this.state = state;
    }

    // Tên bước. (Java: item_option_template)
    public string Name => StepName;

    // Option không phụ thuộc catalog khác khi đọc. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    // Chuyển các hàng option và kiểm id liên tục. (Java: ITEM_OPTION_TEMPLATES)
    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<Nro.Data.Entities.StaticData.ItemOptionTemplateRow> rows = await source.ReadItemOptionTemplatesAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var options = new List<ItemOptionTemplate>(rows.Count);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StaticCatalogValidation.Unique(ids, row.ItemOptionTemplateId, StepName);
            options.Add(new ItemOptionTemplate(row.ItemOptionTemplateId, row.Name ?? String.Empty, row.Type));
        }

        StaticCatalogValidation.ContinuousZeroBasedIds(options.Select(option => option.Id), StepName);
        state.ItemOptionTemplates = options.AsReadOnly();
        return new CatalogStepResult(options.Count);
    }
}
