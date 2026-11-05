using Nro.Content.Catalogs;
using Nro.Content.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp ánh xạ ảnh theo tên và số khung. Nguồn Java: nro/server/Manager.java (dòng 727–731).
public sealed class ImageCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước và khóa ExpectedCounts. (Java: img_by_name)
    public const string StepName = "img_by_name";
    private readonly IStaticRowSource source;
    private readonly AppearanceItemCatalogLoadState state;

    // Tạo bước nạp từ nguồn hàng được tiêm. (Java: img_by_name)
    internal ImageCatalogLoadStep(IStaticRowSource source, AppearanceItemCatalogLoadState state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(state);
        this.source = source;
        this.state = state;
    }

    // Tên bước. (Java: img_by_name)
    public string Name => StepName;

    // Ảnh không phụ thuộc catalog khác khi đọc. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    // Đóng catalog ảnh và công bố vào builder cho consumer không cần mount. (Java: IMAGES_BY_NAME)
    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<Nro.Data.Entities.StaticData.ImgByNameRow> rows = await source.ReadImagesByNameAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var frames = new Dictionary<string, sbyte>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StaticCatalogValidation.Unique(ids, row.ImgByNameId, StepName);
            if (String.IsNullOrWhiteSpace(row.Name))
            {
                throw new CatalogLoadException(StepName, $"Hàng id {row.ImgByNameId} có name rỗng.");
            }

            if (!frames.TryAdd(row.Name, StaticCatalogValidation.SByte(row.NFrame, StepName, "n_frame", row.ImgByNameId)))
            {
                throw new CatalogLoadException(StepName, $"Tên ảnh bị trùng: '{row.Name}'.");
            }
        }

        state.Images = new ImageCatalog(frames);
        builder.SetImages(state.Images);
        return new CatalogStepResult(frames.Count);
    }
}
