using System.Collections.ObjectModel;
using Nro.Game.Cards;
using Nro.Game.Content;

namespace Nro.Content.Catalogs;

// Danh mục radar đóng băng theo id. Nguồn Java: nro/server/Manager.java (dòng 961–994).
public sealed class RadarCatalog : IRadarCatalog
{
    private readonly IReadOnlyList<RadarCard> cards;
    private readonly IReadOnlyDictionary<int, RadarCard> byId;

    public RadarCatalog(IEnumerable<RadarCard> cards)
    {
        ArgumentNullException.ThrowIfNull(cards);
        this.cards = Array.AsReadOnly(cards.Select(Clone).ToArray());
        byId = new ReadOnlyDictionary<int, RadarCard>(this.cards.ToDictionary(value => value.Id));
    }

    public IReadOnlyList<RadarCard> Cards => cards;

    public RadarCard Get(int id) => byId.TryGetValue(id, out RadarCard? value)
        ? value
        : throw new KeyNotFoundException($"Không có thẻ radar id {id}.");

    private static RadarCard Clone(RadarCard value) => value with { Options = Array.AsReadOnly(value.Options.ToArray()) };
}
