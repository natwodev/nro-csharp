namespace Nro.Game.World.Maps.Ports;

// Danh mục Map runtime chỉ đọc, đã được host dựng từ IWorldCatalog; không nạp DB. (Java: Manager.MAPS)
// Nguồn Java: nro/services/MapService.java (dòng 310–325).
public interface IMapDirectory
{
    // Toàn bộ map runtime theo thứ tự Manager.MAPS. (Java: Manager.MAPS)
    IReadOnlyList<Map> Maps { get; }

    // Tra map runtime theo id, trả null khi không có. (Java: MapService.getMapById)
    Map? GetMapById(int mapId);
}
