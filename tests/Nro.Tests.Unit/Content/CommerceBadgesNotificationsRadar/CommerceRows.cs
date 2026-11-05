using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;

namespace Nro.Tests.Unit.Content.CommerceBadgesNotificationsRadar;

// Nguồn hàng J-01D trong bộ nhớ; không mở kết nối hay gọi DB. Nguồn Java: nro/server/Manager.java (dòng 723–810, 953–994).
internal sealed record CommerceRows : IStaticRowSource
{
    public IReadOnlyList<ShopRow> Shops { get; init; } = [];
    public IReadOnlyList<TabShopRow> ShopTabs { get; init; } = [];
    public IReadOnlyList<ItemShopRow> ShopItems { get; init; } = [];
    public IReadOnlyList<ItemShopOptionRow> ShopItemOptions { get; init; } = [];
    public IReadOnlyList<TaskBadgesTemplateRow> BadgeTasks { get; init; } = [];
    public IReadOnlyList<DataBadgesRow> Badges { get; init; } = [];
    public IReadOnlyList<NotifyRow> Notifications { get; init; } = [];
    public IReadOnlyList<RadarRow> RadarCards { get; init; } = [];

    public Task<IReadOnlyList<PartRow>> ReadPartsAsync(CancellationToken cancellationToken = default) => Empty<PartRow>();
    public Task<IReadOnlyList<BgItemTemplateRow>> ReadBgItemsAsync(CancellationToken cancellationToken = default) => Empty<BgItemTemplateRow>();
    public Task<IReadOnlyList<ItemTemplateRow>> ReadItemTemplatesAsync(CancellationToken cancellationToken = default) => Empty<ItemTemplateRow>();
    public Task<IReadOnlyList<ItemOptionTemplateRow>> ReadItemOptionTemplatesAsync(CancellationToken cancellationToken = default) => Empty<ItemOptionTemplateRow>();
    public Task<IReadOnlyList<HeadAvatarRow>> ReadHeadAvatarsAsync(CancellationToken cancellationToken = default) => Empty<HeadAvatarRow>();
    public Task<IReadOnlyList<FlagBagRow>> ReadFlagBagsAsync(CancellationToken cancellationToken = default) => Empty<FlagBagRow>();
    public Task<IReadOnlyList<HeadAnimationFrameRow>> ReadHeadAnimationFramesAsync(CancellationToken cancellationToken = default) => Empty<HeadAnimationFrameRow>();
    public Task<IReadOnlyList<ImgByNameRow>> ReadImagesByNameAsync(CancellationToken cancellationToken = default) => Empty<ImgByNameRow>();
    public Task<IReadOnlyList<AttributeTemplateRow>> ReadAttributeTemplatesAsync(CancellationToken cancellationToken = default) => Empty<AttributeTemplateRow>();
    public Task<IReadOnlyList<PowerLimitRow>> ReadPowerLimitsAsync(CancellationToken cancellationToken = default) => Empty<PowerLimitRow>();
    public Task<IReadOnlyList<CaptionRow>> ReadCaptionsAsync(CancellationToken cancellationToken = default) => Empty<CaptionRow>();
    public Task<IReadOnlyList<SkillTemplateRow>> ReadSkillTemplatesAsync(CancellationToken cancellationToken = default) => Empty<SkillTemplateRow>();
    public Task<IReadOnlyList<IntrinsicRow>> ReadIntrinsicsAsync(CancellationToken cancellationToken = default) => Empty<IntrinsicRow>();
    public Task<IReadOnlyList<TaskMainTemplateRow>> ReadTaskMainsAsync(CancellationToken cancellationToken = default) => Empty<TaskMainTemplateRow>();
    public Task<IReadOnlyList<TaskSubTemplateRow>> ReadTaskSubsAsync(CancellationToken cancellationToken = default) => Empty<TaskSubTemplateRow>();
    public Task<IReadOnlyList<SideTaskTemplateRow>> ReadSideTaskTemplatesAsync(CancellationToken cancellationToken = default) => Empty<SideTaskTemplateRow>();
    public Task<IReadOnlyList<ClanTaskTemplateRow>> ReadClanTaskTemplatesAsync(CancellationToken cancellationToken = default) => Empty<ClanTaskTemplateRow>();
    public Task<IReadOnlyList<TaskKolTemplateRow>> ReadKolTaskTemplatesAsync(CancellationToken cancellationToken = default) => Empty<TaskKolTemplateRow>();
    public Task<IReadOnlyList<TaskEventTemplateRow>> ReadEventTaskTemplatesAsync(CancellationToken cancellationToken = default) => Empty<TaskEventTemplateRow>();
    public Task<IReadOnlyList<AchievementTemplateRow>> ReadAchievementTemplatesAsync(CancellationToken cancellationToken = default) => Empty<AchievementTemplateRow>();
    public Task<IReadOnlyList<ShopRow>> ReadShopsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Shops);
    public Task<IReadOnlyList<TabShopRow>> ReadShopTabsAsync(CancellationToken cancellationToken = default) => Task.FromResult(ShopTabs);
    public Task<IReadOnlyList<ItemShopRow>> ReadShopItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(ShopItems);
    public Task<IReadOnlyList<ItemShopOptionRow>> ReadShopItemOptionsAsync(CancellationToken cancellationToken = default) => Task.FromResult(ShopItemOptions);
    public Task<IReadOnlyList<TaskBadgesTemplateRow>> ReadBadgeTaskTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(BadgeTasks);
    public Task<IReadOnlyList<DataBadgesRow>> ReadBadgeTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Badges);
    public Task<IReadOnlyList<NotifyRow>> ReadNotificationsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Notifications);
    public Task<IReadOnlyList<RadarRow>> ReadRadarCardsAsync(CancellationToken cancellationToken = default) => Task.FromResult(RadarCards);

    private static Task<IReadOnlyList<T>> Empty<T>() => Task.FromResult<IReadOnlyList<T>>(Array.Empty<T>());
}
