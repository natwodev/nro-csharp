using Nro.Game.Cards;

namespace Nro.Game.Content;

// Danh mục thẻ radar chỉ đọc. Nguồn Java: nro/server/Manager.java (dòng 961–994).
public interface IRadarCatalog
{
    // Toàn bộ thẻ radar theo id. (Java: RadarService.RADAR_TEMPLATE)
    IReadOnlyList<RadarCard> Cards { get; }

    // Lấy thẻ radar theo mã. (Java: RadarService.RADAR_TEMPLATE)
    RadarCard Get(int id);
}
