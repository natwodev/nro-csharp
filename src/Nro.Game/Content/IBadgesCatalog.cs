using Nro.Game.Progress.Badges;

namespace Nro.Game.Content;

// Danh mục huy hiệu chỉ đọc của dữ liệu tĩnh. Nguồn Java: nro/server/Manager.java (dòng 777–810).
public interface IBadgesCatalog
{
    // Điều kiện nhiệm vụ huy hiệu theo thứ tự tải. (Java: TASKS_BADGES_TEMPLATE)
    IReadOnlyList<BadgeTaskTemplate> TaskTemplates { get; }

    // Mẫu huy hiệu theo thứ tự tải. (Java: BAGES_TEMPLATES)
    IReadOnlyList<BadgeTemplate> Templates { get; }
}
