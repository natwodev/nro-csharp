using Nro.Core.Templates;
using Nro.Game.World.Maps;

namespace Nro.Game.Content;

// Nguồn Java: nro/server/Manager.java (dòng 742–855).
// Danh mục mẫu thế giới chỉ đọc; mob/NPC tra theo chỉ số, map tra theo mapId như Java. (Java: MOB_TEMPLATES, NPC_TEMPLATES, Manager.MAPS)
public interface IWorldCatalog
{
    // Số mẫu quái đã xác thực. (Java: MOB_TEMPLATES.size)
    int MobCount { get; }

    // Số mẫu NPC đã xác thực. (Java: NPC_TEMPLATES.size)
    int NpcCount { get; }

    // Số mẫu bản đồ đã xác thực. (Java: MAP_TEMPLATES.length)
    int MapCount { get; }

    // Map theo thứ tự id để các consumer duyệt tuần tự. (Java: for (Map map : Manager.MAPS))
    IReadOnlyList<MapTemplate> Maps { get; }

    // Lấy bản sao mẫu quái tại chỉ số id. (Java: MOB_TEMPLATES.get)
    MobTemplate GetMob(int id);

    // Lấy bản sao mẫu NPC tại chỉ số id. (Java: NPC_TEMPLATES.get)
    NpcTemplate GetNpc(int id);

    // Tìm bản sao map theo mapId; trả null khi id không có. (Java: MapService.getMapById)
    MapTemplate? GetMap(int id);

    // Tìm map theo mapId mà không dùng ngoại lệ điều khiển luồng. (Java: MapService.getMapById)
    bool TryGetMap(int id, out MapTemplate? map);
}
