using Nro.Data.Entities.StaticData;

namespace Nro.Content.StaticData;

// Cổng đọc các hàng dữ liệu tĩnh; các bước nạp không biết hay gọi cơ sở dữ liệu. Nguồn Java: nro/server/Manager.java (dòng 351–436, 560–600, 727–741).
public interface IStaticRowSource
{
    // Đọc part theo id tăng dần. (Java: part)
    Task<IReadOnlyList<PartRow>> ReadPartsAsync(CancellationToken cancellationToken = default);

    // Đọc vật trang trí nền theo id tăng dần. (Java: bg_item_template)
    Task<IReadOnlyList<BgItemTemplateRow>> ReadBgItemsAsync(CancellationToken cancellationToken = default);

    // Đọc mẫu vật phẩm theo id tăng dần. (Java: item_template)
    Task<IReadOnlyList<ItemTemplateRow>> ReadItemTemplatesAsync(CancellationToken cancellationToken = default);

    // Đọc mẫu tùy chọn vật phẩm theo id tăng dần. (Java: item_option_template)
    Task<IReadOnlyList<ItemOptionTemplateRow>> ReadItemOptionTemplatesAsync(CancellationToken cancellationToken = default);

    // Đọc avatar đầu theo id đầu tăng dần. (Java: head_avatar)
    Task<IReadOnlyList<HeadAvatarRow>> ReadHeadAvatarsAsync(CancellationToken cancellationToken = default);

    // Đọc túi cờ theo id tăng dần. (Java: flag_bag)
    Task<IReadOnlyList<FlagBagRow>> ReadFlagBagsAsync(CancellationToken cancellationToken = default);

    // Đọc frame hoạt ảnh đầu theo id tăng dần. (Java: array_head_2_frames)
    Task<IReadOnlyList<HeadAnimationFrameRow>> ReadHeadAnimationFramesAsync(CancellationToken cancellationToken = default);

    // Đọc ảnh theo tên từ bảng ổn định theo id. (Java: img_by_name)
    Task<IReadOnlyList<ImgByNameRow>> ReadImagesByNameAsync(CancellationToken cancellationToken = default);
}
