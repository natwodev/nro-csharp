using System.Collections.ObjectModel;

namespace Nro.Content.Loaders;

// Các số bản ghi snapshot dùng để kiểm tra catalog và ghi CatalogLoadReport. Nguồn Java: nro/server/Manager.java (dòng 351–436, 560–600, 727–741).
public static class ExpectedCounts
{
    // Số part trong dump đã duyệt. (Java: part)
    public const int Part = 1944;

    // Số nền trong dump đã duyệt. (Java: bg_item_template)
    public const int BgItem = 470;

    // Số mẫu vật phẩm trong dump đã duyệt. (Java: item_template)
    public const int Item = 2434;

    // Số mẫu tùy chọn vật phẩm trong dump đã duyệt. (Java: item_option_template)
    public const int ItemOption = 273;

    // Số avatar đầu trong dump đã duyệt. (Java: head_avatar)
    public const int HeadAvatar = 522;

    // Số túi cờ trong dump đã duyệt. (Java: flag_bag)
    public const int FlagBag = 220;

    // Số hoạt ảnh đầu trong dump đã duyệt. (Java: array_head_2_frames)
    public const int HeadAnimationFrame = 57;

    // Số ảnh theo tên trong dump đã duyệt. (Java: img_by_name)
    public const int Image = 341;

    // Số thú cưỡi dẫn xuất trong dump đã duyệt. (Java: DataGame.MAP_MOUNT_NUM)
    public const int Mount = 74;

    // Số mẫu thuộc tính tĩnh trong dump đã duyệt. (Java: attribute_template)
    public const int AttributeTemplate = 5;

    // Số mốc giới hạn sức mạnh trong dump đã duyệt. (Java: power_limit)
    public const int PowerLimit = 14;

    // Số danh hiệu trong dump đã duyệt. (Java: caption)
    public const int Caption = 21;

    // Số template kỹ năng trong dump đã duyệt. (Java: skill_template)
    public const int SkillTemplate = 27;

    // Số lớp kỹ năng dẫn xuất từ skill_template trong dump đã duyệt. (Java: NCLASS)
    public const int SkillClass = 3;

    // Số nội tại trong dump đã duyệt. (Java: intrinsic)
    public const int Intrinsic = 27;

    // Số nhiệm vụ chính có bước trong dump đã duyệt. (Java: task_main_template)
    public const int TaskMain = 33;

    // Số bước nhiệm vụ chính trong dump đã duyệt. (Java: task_sub_template)
    public const int TaskSub = 142;

    // Số nhiệm vụ phụ trong dump đã duyệt. (Java: side_task_template)
    public const int SideTask = 59;

    // Số nhiệm vụ bang hội trong dump đã duyệt. (Java: clan_task_template)
    public const int ClanTask = 59;

    // Số nhiệm vụ KOL trong dump đã duyệt. (Java: task_kol_template)
    public const int KolTask = 7;

    // Số nhiệm vụ sự kiện trong dump đã duyệt. (Java: task_event_template)
    public const int EventTask = 59;

    // Số thành tựu trong dump đã duyệt. (Java: achievement_template)
    public const int Achievement = 20;

    // Bảng số lượng gắn trực tiếp với tên bước, truyền cho StaticCatalogLoader để đưa vào CatalogLoadReport. (Java: loadDatabase)
    public static IReadOnlyDictionary<string, int> AppearanceItems { get; } = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(StringComparer.Ordinal)
    {
        [PartCatalogLoadStep.StepName] = Part,
        [BgItemCatalogLoadStep.StepName] = BgItem,
        [ItemTemplateCatalogLoadStep.StepName] = Item,
        [ItemOptionTemplateCatalogLoadStep.StepName] = ItemOption,
        [HeadAvatarCatalogLoadStep.StepName] = HeadAvatar,
        [FlagBagCatalogLoadStep.StepName] = FlagBag,
        [HeadAnimationFrameCatalogLoadStep.StepName] = HeadAnimationFrame,
        [ImageCatalogLoadStep.StepName] = Image,
        [ItemCatalogLoadStep.StepName] = Mount,
    });

    // Số bản ghi J-01C gắn trực tiếp với từng bước nạp. (Java: loadDatabase)
    public static IReadOnlyDictionary<string, int> SkillsProgressionTasks { get; } = new ReadOnlyDictionary<string, int>(new Dictionary<string, int>(StringComparer.Ordinal)
    {
        [SkillCatalogLoadStep.StepName] = SkillTemplate,
        [ProgressionCatalogLoadStep.StepName] = AttributeTemplate + PowerLimit + Caption + Intrinsic + Achievement,
        [TaskCatalogLoadStep.StepName] = TaskMain + TaskSub + SideTask + ClanTask + KolTask + EventTask,
    });
}
