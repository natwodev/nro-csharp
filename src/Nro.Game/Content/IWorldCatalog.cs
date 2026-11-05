using Nro.Core.Templates;
using Nro.Game.World.Maps;

namespace Nro.Game.Content;

// Nguồn Java: nro/server/Manager.java (dòng 742–855).
// Danh mục mẫu thế giới chỉ đọc; các tra cứu giữ ngữ nghĩa mảng theo id của Java. (Java: MOB_TEMPLATES, NPC_TEMPLATES, MAP_TEMPLATES)
public interface IWorldCatalog
{
    // Số mẫu quái đã xác thực. (Java: MOB_TEMPLATES.size)
    int MobCount { get; }

    // Số mẫu NPC đã xác thực. (Java: NPC_TEMPLATES.size)
    int NpcCount { get; }

    // Số mẫu bản đồ đã xác thực. (Java: MAP_TEMPLATES.length)
    int MapCount { get; }

    // Lấy bản sao mẫu quái tại chỉ số id. (Java: MOB_TEMPLATES.get)
    MobTemplate GetMob(int id);

    // Lấy bản sao mẫu NPC tại chỉ số id. (Java: NPC_TEMPLATES.get)
    NpcTemplate GetNpc(int id);

    // Lấy bản sao mẫu bản đồ tại chỉ số id. (Java: MAP_TEMPLATES[index])
    MapTemplate GetMap(int id);
}
