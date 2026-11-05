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

    // Danh sách lớp kỹ năng rỗng trước khi nạp. (Java: NCLASS)
    public IReadOnlyList<NClass> Classes { get; } = Array.Empty<NClass>();

    // Danh sách nhiệm vụ chính rỗng trước khi nạp. (Java: TASKS)
    public IReadOnlyList<TaskMain> TaskMains { get; } = Array.Empty<TaskMain>();

    // Danh sách template nhiệm vụ phụ rỗng trước khi nạp. (Java: SIDE_TASKS_TEMPLATE)
    public IReadOnlyList<SideTaskTemplate> SideTaskTemplates { get; } = Array.Empty<SideTaskTemplate>();

    // Danh sách template nhiệm vụ bang hội rỗng trước khi nạp. (Java: CLAN_TASKS_TEMPLATE)
    public IReadOnlyList<ClanTaskTemplate> ClanTaskTemplates { get; } = Array.Empty<ClanTaskTemplate>();

    // Danh sách template KOL rỗng trước khi nạp. (Java: KOL_TASKS_TEMPLATE)
    public IReadOnlyList<KolTaskTemplate> KolTaskTemplates { get; } = Array.Empty<KolTaskTemplate>();

    // Danh sách template nhiệm vụ sự kiện rỗng trước khi nạp. (Java: EVENT_TASKS_TEMPLATE)
    public IReadOnlyList<EventTaskTemplate> EventTaskTemplates { get; } = Array.Empty<EventTaskTemplate>();

    // Danh sách mẫu thuộc tính rỗng trước khi nạp. (Java: AttributeTemplateManager.list)
    public IReadOnlyList<AttributeTemplate> AttributeTemplates { get; } = Array.Empty<AttributeTemplate>();

    // Danh sách mốc sức mạnh rỗng trước khi nạp. (Java: PowerLimitManager.powers)
    public IReadOnlyList<PowerLimit> PowerLimits { get; } = Array.Empty<PowerLimit>();

    // Danh sách danh hiệu rỗng trước khi nạp. (Java: CaptionManager.captions)
    public IReadOnlyList<Caption> Captions { get; } = Array.Empty<Caption>();

    // Danh sách nội tại rỗng trước khi nạp. (Java: INTRINSICS)
    public IReadOnlyList<Intrinsic> Intrinsics { get; } = Array.Empty<Intrinsic>();

    // Danh sách thành tựu rỗng trước khi nạp. (Java: ACHIEVEMENT_TEMPLATE)
    public IReadOnlyList<AchievementTemplate> Achievements { get; } = Array.Empty<AchievementTemplate>();

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

    // Từ chối tra lớp kỹ năng khi snapshot chưa được nạp. (Java: NCLASS.get)
    public NClass GetClass(int classId) => throw new KeyNotFoundException();

    // Từ chối tra template kỹ năng khi snapshot chưa được nạp. (Java: NClass.getSkillTemplate)
    public SkillTemplate GetTemplate(int classId, int templateId) => throw new KeyNotFoundException();

    // Từ chối tra nhiệm vụ chính khi snapshot chưa được nạp. (Java: TASKS)
    public TaskMain GetTaskMain(int id) => throw new KeyNotFoundException();

    // Từ chối tra bước nhiệm vụ khi snapshot chưa được nạp. (Java: TaskMain.subTasks)
    public SubTaskMain GetSubTaskMain(int taskMainId, int index) => throw new ArgumentOutOfRangeException(nameof(index));

    // Từ chối tra nhiệm vụ phụ khi snapshot chưa được nạp. (Java: SIDE_TASKS_TEMPLATE.get)
    public SideTaskTemplate GetSideTaskTemplate(int index, int id) => throw new ArgumentOutOfRangeException(nameof(index));

    // Từ chối tra nhiệm vụ bang hội khi snapshot chưa được nạp. (Java: CLAN_TASKS_TEMPLATE.get)
    public ClanTaskTemplate GetClanTaskTemplate(int index, int id) => throw new ArgumentOutOfRangeException(nameof(index));

    // Từ chối tra nhiệm vụ KOL khi snapshot chưa được nạp. (Java: KOL_TASKS_TEMPLATE.get)
    public KolTaskTemplate GetKolTaskTemplate(int index, int id) => throw new ArgumentOutOfRangeException(nameof(index));

    // Từ chối tra nhiệm vụ sự kiện khi snapshot chưa được nạp. (Java: EVENT_TASKS_TEMPLATE.get)
    public EventTaskTemplate GetEventTaskTemplate(int index, int id) => throw new ArgumentOutOfRangeException(nameof(index));

    // Từ chối tra mẫu thuộc tính khi snapshot chưa được nạp. (Java: AttributeTemplateManager.find)
    public AttributeTemplate GetAttributeTemplate(int id) => throw new KeyNotFoundException();

    // Từ chối tra mốc sức mạnh khi snapshot chưa được nạp. (Java: PowerLimitManager.get)
    public PowerLimit GetPowerLimit(int index) => throw new ArgumentOutOfRangeException(nameof(index));

    // Không có caption khi snapshot chưa được nạp. (Java: CaptionManager.find)
    public Caption? FindCaption(int id) => null;

    // Không có nội tại khi snapshot chưa được nạp. (Java: INTRINSIC_TD/NM/XD)
    public IReadOnlyList<Intrinsic> GetIntrinsics(sbyte gender) => Array.Empty<Intrinsic>();

    // Từ chối tra thành tựu khi snapshot chưa được nạp. (Java: ACHIEVEMENT_TEMPLATE.get)
    public AchievementTemplate GetAchievement(int index) => throw new ArgumentOutOfRangeException(nameof(index));
}
