using Nro.Core.Content;
using Nro.Core.Templates;

namespace Nro.Game.Content;

// Danh mục rỗng dùng cho snapshot trước lần nạp đầu tiên. Nguồn Java: nro/server/Manager.java (dòng 192–218).
internal sealed class EmptyStaticCatalogs : IItemCatalog, IWorldCatalog, IAppearanceCatalog, ISkillCatalog, ITaskCatalog, IShopCatalog, IProgressionCatalog, INotificationCatalog, IRadarCatalog
{
    internal static readonly EmptyStaticCatalogs Instance = new();

    // Danh sách mẫu vật phẩm rỗng trước lần nạp đầu tiên. (Java: ITEM_TEMPLATES)
    public IReadOnlyList<ItemTemplate> ItemTemplates { get; } = Array.Empty<ItemTemplate>();

    // Danh sách mẫu tùy chọn vật phẩm rỗng trước lần nạp đầu tiên. (Java: ITEM_OPTION_TEMPLATES)
    public IReadOnlyList<ItemOptionTemplate> ItemOptionTemplates { get; } = Array.Empty<ItemOptionTemplate>();

    // Danh sách bộ phận ngoại hình rỗng trước lần nạp đầu tiên. (Java: parts)
    public IReadOnlyList<Part> Parts { get; } = Array.Empty<Part>();

    // Danh sách avatar đầu rỗng trước lần nạp đầu tiên. (Java: HEAD_AVATARS)
    public IReadOnlyList<HeadAvatar> HeadAvatars { get; } = Array.Empty<HeadAvatar>();

    // Danh sách túi cờ rỗng trước lần nạp đầu tiên. (Java: FLAGS_BAGS)
    public IReadOnlyList<FlagBag> FlagBags { get; } = Array.Empty<FlagBag>();

    // Danh sách frame đầu rỗng trước lần nạp đầu tiên. (Java: ARR_HEAD_2_FRAMES)
    public IReadOnlyList<ArrHead2Frames> HeadFrames { get; } = Array.Empty<ArrHead2Frames>();
}
