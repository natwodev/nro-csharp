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
}
