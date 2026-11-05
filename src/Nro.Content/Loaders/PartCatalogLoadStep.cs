using Nro.Core.Templates;
using Nro.Content.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp part và JSON chi tiết vào trạng thái cục bộ, không ghi file wire như Java. Nguồn Java: nro/server/Manager.java (dòng 351–380).
public sealed class PartCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước và khóa ExpectedCounts. (Java: part)
    public const string StepName = "part";
    private readonly IStaticRowSource source;
    private readonly AppearanceItemCatalogLoadState state;

    // Tạo bước nạp từ nguồn hàng được tiêm. (Java: part)
    internal PartCatalogLoadStep(IStaticRowSource source, AppearanceItemCatalogLoadState state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(state);
        this.source = source;
        this.state = state;
    }

    // Tên bước. (Java: part)
    public string Name => StepName;

    // Part không phụ thuộc catalog khác khi đọc. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    // Parse JSON data thành Part và PartDetail, không sinh data/update_data/part. (Java: parts)
    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<Nro.Data.Entities.StaticData.PartRow> rows = await source.ReadPartsAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var parts = new List<Part>(rows.Count);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StaticCatalogValidation.Unique(ids, row.PartId, StepName);
            parts.Add(new Part
            {
                Id = row.PartId,
                Type = row.Type,
                PartDetails = StaticCatalogJson.PartDetails(row.Data, StepName, row.PartId),
            });
        }

        state.Parts = parts.AsReadOnly();
        return new CatalogStepResult(parts.Count);
    }
}
