using Nro.Core.Content;
using Nro.Core.Templates;

namespace Nro.Game.Content;

// Danh mục rỗng dùng cho snapshot trước lần nạp đầu tiên. Nguồn Java: nro/server/Manager.java (dòng 192–218).
internal sealed class EmptyStaticCatalogs : IItemCatalog, IWorldCatalog, IAppearanceCatalog, IImageCatalog, ISkillCatalog, ITaskCatalog, IShopCatalog, IProgressionCatalog, INotificationCatalog, IRadarCatalog
{
    internal static readonly EmptyStaticCatalogs Instance = new();

    // Danh sách mẫu vật phẩm rỗng trước lần nạp đầu tiên. (Java: ITEM_TEMPLATES)
    public IReadOnlyList<ItemTemplate> ItemTemplates { get; } = Array.Empty<ItemTemplate>();

    // Danh sách mẫu tùy chọn vật phẩm rỗng trước lần nạp đầu tiên. (Java: ITEM_OPTION_TEMPLATES)
    public IReadOnlyList<ItemOptionTemplate> ItemOptionTemplates { get; } = Array.Empty<ItemOptionTemplate>();

    // Danh sách vật trang trí nền rỗng trước lần nạp đầu tiên. (Java: BG_ITEMS)
    public IReadOnlyList<BgItem> BgItems { get; } = Array.Empty<BgItem>();

    // Ánh xạ thú cưỡi rỗng trước lần nạp đầu tiên. (Java: MAP_MOUNT_NUM)
    public IReadOnlyDictionary<short, int> MountItemParts { get; } = new Dictionary<short, int>();

    // Danh sách bộ phận ngoại hình rỗng trước lần nạp đầu tiên. (Java: parts)
    public IReadOnlyList<Part> Parts { get; } = Array.Empty<Part>();

    // Danh sách avatar đầu rỗng trước lần nạp đầu tiên. (Java: HEAD_AVATARS)
    public IReadOnlyList<HeadAvatar> HeadAvatars { get; } = Array.Empty<HeadAvatar>();

    // Danh sách túi cờ rỗng trước lần nạp đầu tiên. (Java: FLAGS_BAGS)
    public IReadOnlyList<FlagBag> FlagBags { get; } = Array.Empty<FlagBag>();

    // Danh sách frame đầu rỗng trước lần nạp đầu tiên. (Java: ARR_HEAD_2_FRAMES)
    public IReadOnlyList<ArrHead2Frames> HeadFrames { get; } = Array.Empty<ArrHead2Frames>();

    // Danh mục ảnh rỗng trước lần nạp đầu tiên. (Java: IMAGES_BY_NAME)
    public IReadOnlyDictionary<string, sbyte> FramesByName { get; } = new Dictionary<string, sbyte>(StringComparer.OrdinalIgnoreCase);

    // Từ chối tra cứu item khi snapshot chưa được nạp. (Java: ITEM_TEMPLATES.get)
    public ItemTemplate GetItemTemplate(int id) => throw new ArgumentOutOfRangeException(nameof(id));

    // Từ chối tra cứu option khi snapshot chưa được nạp. (Java: ITEM_OPTION_TEMPLATES.get)
    public ItemOptionTemplate GetItemOptionTemplate(int id) => throw new ArgumentOutOfRangeException(nameof(id));

    // Từ chối tra cứu nền khi snapshot chưa được nạp. (Java: BG_ITEMS)
    public BgItem GetBgItem(int id) => throw new KeyNotFoundException();

    // Không có thú cưỡi trước khi nạp. (Java: MAP_MOUNT_NUM.get)
    public bool TryGetMountPart(short itemId, out int part)
    {
        part = default;
        return false;
    }

    // Từ chối tra cứu part khi snapshot chưa được nạp. (Java: parts)
    public Part GetPart(int id) => throw new KeyNotFoundException();

    // Từ chối tra cứu avatar khi snapshot chưa được nạp. (Java: HEAD_AVATARS)
    public HeadAvatar GetHeadAvatar(int headId) => throw new KeyNotFoundException();

    // Từ chối tra cứu cờ khi snapshot chưa được nạp. (Java: FLAGS_BAGS)
    public FlagBag GetFlagBag(int id) => throw new KeyNotFoundException();

    // Từ chối tra cứu frame khi snapshot chưa được nạp. (Java: ARR_HEAD_2_FRAMES.get)
    public ArrHead2Frames GetHeadAnimationFrame(int id) => throw new ArgumentOutOfRangeException(nameof(id));

    // Không có ảnh trước khi nạp. (Java: IMAGES_BY_NAME.get)
    public bool TryGetFrame(string name, out sbyte frameCount)
    {
        frameCount = default;
        return false;
    }
}
