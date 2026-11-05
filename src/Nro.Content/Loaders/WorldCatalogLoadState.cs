using Nro.Core.Templates;

namespace Nro.Content.Loaders;

// Nguồn Java: nro/server/Manager.java (dòng 742–855).
// Trạng thái cục bộ của ba bước J-01E, không được dùng lại giữa các lần dựng snapshot. (Java: loadDatabase)
internal sealed class WorldCatalogLoadState
{
    // Mẫu quái đã parse nhưng chưa công bố. (Java: MOB_TEMPLATES)
    public IReadOnlyList<MobTemplate>? Mobs { get; set; }

    // Mẫu NPC đã parse nhưng chưa công bố. (Java: NPC_TEMPLATES)
    public IReadOnlyList<NpcTemplate>? Npcs { get; set; }
}
