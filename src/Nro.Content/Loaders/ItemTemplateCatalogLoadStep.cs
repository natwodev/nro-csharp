using Nro.Core.Templates;
using Nro.Content.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp mẫu vật phẩm vào trạng thái cục bộ, không truy cập DB trực tiếp. Nguồn Java: nro/server/Manager.java (dòng 402–419).
public sealed class ItemTemplateCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước và khóa ExpectedCounts. (Java: item_template)
    public const string StepName = "item_template";
    private readonly IStaticRowSource source;
    private readonly AppearanceItemCatalogLoadState state;

    // Tạo bước nạp từ nguồn hàng được tiêm. (Java: item_template)
    internal ItemTemplateCatalogLoadStep(IStaticRowSource source, AppearanceItemCatalogLoadState state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(state);
        this.source = source;
        this.state = state;
    }

    // Tên bước. (Java: item_template)
    public string Name => StepName;

    // Item không phụ thuộc catalog khác khi đọc. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    // Chuyển các hàng item thành kiểu miền và kiểm id liên tục. (Java: ITEM_TEMPLATES)
    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<Nro.Data.Entities.StaticData.ItemTemplateRow> rows = await source.ReadItemTemplatesAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var items = new List<ItemTemplate>(rows.Count);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StaticCatalogValidation.Unique(ids, row.ItemTemplateId, StepName);
            items.Add(new ItemTemplate
            {
                Id = StaticCatalogValidation.Short(row.ItemTemplateId, StepName, "id", row.ItemTemplateId),
                Type = StaticCatalogValidation.SByte(row.Type, StepName, "type", row.ItemTemplateId),
                Gender = StaticCatalogValidation.SByte(row.Gender, StepName, "gender", row.ItemTemplateId),
                Name = row.Name ?? String.Empty,
                Description = row.Description ?? String.Empty,
                Level = StaticCatalogValidation.SByte(row.Level, StepName, "level", row.ItemTemplateId),
                IconID = StaticCatalogValidation.Short(row.IconId, StepName, "icon_id", row.ItemTemplateId),
                Part = StaticCatalogValidation.Short(row.Part, StepName, "part", row.ItemTemplateId),
                IsUpToUp = row.IsUpToUp,
                StrRequire = StaticCatalogValidation.Int32(row.PowerRequire, StepName, "power_require", row.ItemTemplateId),
                Gold = row.Gold,
                GoldSell = row.GoldSell,
                Gem = row.Gem,
                GemSell = row.GemSell,
                Ruby = row.Ruby,
                Ruby_sell = row.RubySell,
                Head = row.Head,
                Body = row.Body,
                Leg = row.Leg,
                TypeEvent = row.TypeEvent,
                IsGender = StaticCatalogValidation.SByte(row.IsGender, StepName, "is_gender", row.ItemTemplateId),
            });
        }

        StaticCatalogValidation.ContinuousZeroBasedIds(items.Select(item => (int)item.Id), StepName);
        state.ItemTemplates = items.AsReadOnly();
        return new CatalogStepResult(items.Count);
    }
}
