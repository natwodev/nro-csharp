using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;

namespace Nro.Tests.Unit.Content.AppearanceItems;

// Nguồn hàng trong bộ nhớ cho unit test; không mở kết nối hay gọi DB. Nguồn Java: nro/server/Manager.java (dòng 351–436, 560–600, 727–741).
internal sealed class InMemoryStaticRowSource : IStaticRowSource
{
    // Hàng part thử nghiệm. (Java: part)
    public IReadOnlyList<PartRow> Parts { get; init; } = Array.Empty<PartRow>();

    // Hàng nền thử nghiệm. (Java: bg_item_template)
    public IReadOnlyList<BgItemTemplateRow> BgItems { get; init; } = Array.Empty<BgItemTemplateRow>();

    // Hàng item thử nghiệm. (Java: item_template)
    public IReadOnlyList<ItemTemplateRow> ItemTemplates { get; init; } = Array.Empty<ItemTemplateRow>();

    // Hàng option thử nghiệm. (Java: item_option_template)
    public IReadOnlyList<ItemOptionTemplateRow> ItemOptionTemplates { get; init; } = Array.Empty<ItemOptionTemplateRow>();

    // Hàng avatar thử nghiệm. (Java: head_avatar)
    public IReadOnlyList<HeadAvatarRow> HeadAvatars { get; init; } = Array.Empty<HeadAvatarRow>();

    // Hàng cờ thử nghiệm. (Java: flag_bag)
    public IReadOnlyList<FlagBagRow> FlagBags { get; init; } = Array.Empty<FlagBagRow>();

    // Hàng frame đầu thử nghiệm. (Java: head_animation_frame)
    public IReadOnlyList<HeadAnimationFrameRow> HeadAnimationFrames { get; init; } = Array.Empty<HeadAnimationFrameRow>();

    // Hàng ảnh thử nghiệm. (Java: img_by_name)
    public IReadOnlyList<ImgByNameRow> ImagesByName { get; init; } = Array.Empty<ImgByNameRow>();

    // Trả part trong bộ nhớ. (Java: part)
    public Task<IReadOnlyList<PartRow>> ReadPartsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Parts);

    // Trả nền trong bộ nhớ. (Java: bg_item_template)
    public Task<IReadOnlyList<BgItemTemplateRow>> ReadBgItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(BgItems);

    // Trả item trong bộ nhớ. (Java: item_template)
    public Task<IReadOnlyList<ItemTemplateRow>> ReadItemTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(ItemTemplates);

    // Trả option trong bộ nhớ. (Java: item_option_template)
    public Task<IReadOnlyList<ItemOptionTemplateRow>> ReadItemOptionTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(ItemOptionTemplates);

    // Trả avatar trong bộ nhớ. (Java: head_avatar)
    public Task<IReadOnlyList<HeadAvatarRow>> ReadHeadAvatarsAsync(CancellationToken cancellationToken = default) => Task.FromResult(HeadAvatars);

    // Trả cờ trong bộ nhớ. (Java: flag_bag)
    public Task<IReadOnlyList<FlagBagRow>> ReadFlagBagsAsync(CancellationToken cancellationToken = default) => Task.FromResult(FlagBags);

    // Trả frame đầu trong bộ nhớ. (Java: head_animation_frame)
    public Task<IReadOnlyList<HeadAnimationFrameRow>> ReadHeadAnimationFramesAsync(CancellationToken cancellationToken = default) => Task.FromResult(HeadAnimationFrames);

    // Trả ảnh trong bộ nhớ. (Java: img_by_name)
    public Task<IReadOnlyList<ImgByNameRow>> ReadImagesByNameAsync(CancellationToken cancellationToken = default) => Task.FromResult(ImagesByName);
}
