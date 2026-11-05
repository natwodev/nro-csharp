using System.Globalization;
using Nro.Core.Templates;
using Nro.Content.StaticData;
using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Nạp túi cờ và parse icon_data CSV vào trạng thái cục bộ. Nguồn Java: nro/server/Manager.java (dòng 570–589).
public sealed class FlagBagCatalogLoadStep : ICatalogLoadStep
{
    // Tên bước và khóa ExpectedCounts. (Java: flag_bag)
    public const string StepName = "flag_bag";
    private readonly IStaticRowSource source;
    private readonly AppearanceItemCatalogLoadState state;

    // Tạo bước nạp từ nguồn hàng được tiêm. (Java: flag_bag)
    internal FlagBagCatalogLoadStep(IStaticRowSource source, AppearanceItemCatalogLoadState state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(state);
        this.source = source;
        this.state = state;
    }

    // Tên bước. (Java: flag_bag)
    public string Name => StepName;

    // Túi cờ không phụ thuộc catalog khác khi đọc. (Java: loadDatabase)
    public IReadOnlyCollection<string> Dependencies { get; } = Array.Empty<string>();

    // Chuyển các hàng cờ và parse icon_data CSV thành short[]. (Java: FLAGS_BAGS)
    public async Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(builder);
        IReadOnlyList<Nro.Data.Entities.StaticData.FlagBagRow> rows = await source.ReadFlagBagsAsync(cancellationToken).ConfigureAwait(false);
        var ids = new HashSet<int>();
        var flags = new List<FlagBag>(rows.Count);
        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            StaticCatalogValidation.Unique(ids, row.FlagBagId, StepName);
            flags.Add(new FlagBag
            {
                Id = row.FlagBagId,
                IconId = StaticCatalogValidation.Short(row.IconId, StepName, "icon_id", row.FlagBagId),
                IconEffect = ParseIcons(row.IconData, row.FlagBagId),
                Name = row.Name ?? String.Empty,
                Gold = row.Gold,
                Gem = row.Gem,
            });
        }

        state.FlagBags = flags.AsReadOnly();
        return new CatalogStepResult(flags.Count);
    }

    private static short[] ParseIcons(string? csv, int rowId)
    {
        if (String.IsNullOrEmpty(csv))
        {
            throw new CatalogLoadException(StepName, $"Hàng id {rowId} có icon_data CSV rỗng.");
        }

        string[] values = csv.Split(',', StringSplitOptions.None);
        int count = csv.Length == 0 ? 1 : values.Length;
        // String.split(",") của Java loại các phần tử rỗng ở cuối, nhưng vẫn giữ phần tử rỗng ở giữa.
        while (count > 0 && values[count - 1].Length == 0)
        {
            count--;
        }

        var icons = new short[count];
        for (int index = 0; index < count; index++)
        {
            if (!Int16.TryParse(values[index].Trim(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out short icon))
            {
                throw new CatalogLoadException(StepName, $"Hàng id {rowId} có icon_data[{index}]='{values[index]}' không phải Int16.");
            }

            icons[index] = icon;
        }

        return icons;
    }
}
