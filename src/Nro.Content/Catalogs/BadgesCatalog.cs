using Nro.Game.Content;
using Nro.Game.Items.Core;
using Nro.Game.Progress.Badges;

namespace Nro.Content.Catalogs;

// Danh mục huy hiệu đóng băng sau khi resolve option template. Nguồn Java: nro/server/Manager.java (dòng 777–810).
public sealed class BadgesCatalog : Nro.Game.Content.IBadgesCatalog
{
    private readonly IReadOnlyList<BadgeTaskTemplate> tasks;
    private readonly IReadOnlyList<BadgeTemplate> templates;

    public BadgesCatalog(IEnumerable<BadgeTaskTemplate> tasks, IEnumerable<BadgeTemplate> templates)
    {
        ArgumentNullException.ThrowIfNull(tasks);
        ArgumentNullException.ThrowIfNull(templates);
        this.tasks = Array.AsReadOnly(tasks.ToArray());
        this.templates = Array.AsReadOnly(templates.Select(Clone).ToArray());
    }

    public IReadOnlyList<BadgeTaskTemplate> TaskTemplates => tasks;

    public IReadOnlyList<BadgeTemplate> Templates => Array.AsReadOnly(templates.Select(Clone).ToArray());

    private static BadgeTemplate Clone(BadgeTemplate value) => new(value.Id, value.EffectId, value.ItemId, value.Name,
        Array.AsReadOnly(value.Options.Select(option => new ItemOption(option)).ToArray()));
}
