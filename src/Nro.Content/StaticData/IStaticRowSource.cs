using Nro.Data.Entities.StaticData;

namespace Nro.Content.StaticData;

// Cổng đọc các hàng dữ liệu tĩnh; các bước nạp không biết hay gọi cơ sở dữ liệu. Nguồn Java: nro/server/Manager.java (dòng 351–705).
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

    // Đọc mẫu thuộc tính theo id tăng dần. (Java: attribute_template)
    Task<IReadOnlyList<AttributeTemplateRow>> ReadAttributeTemplatesAsync(CancellationToken cancellationToken = default);

    // Đọc giới hạn sức mạnh theo id tăng dần. (Java: power_limit)
    Task<IReadOnlyList<PowerLimitRow>> ReadPowerLimitsAsync(CancellationToken cancellationToken = default);

    // Đọc danh hiệu theo id tăng dần. (Java: caption)
    Task<IReadOnlyList<CaptionRow>> ReadCaptionsAsync(CancellationToken cancellationToken = default);

    // Đọc template kỹ năng theo lớp, slot rồi id. (Java: skill_template)
    Task<IReadOnlyList<SkillTemplateRow>> ReadSkillTemplatesAsync(CancellationToken cancellationToken = default);

    // Đọc nội tại theo id tăng dần. (Java: intrinsic)
    Task<IReadOnlyList<IntrinsicRow>> ReadIntrinsicsAsync(CancellationToken cancellationToken = default);

    // Đọc nhiệm vụ chính theo id tăng dần. (Java: task_main_template)
    Task<IReadOnlyList<TaskMainTemplateRow>> ReadTaskMainsAsync(CancellationToken cancellationToken = default);

    // Đọc bước nhiệm vụ theo task_main_id rồi idmain. (Java: task_sub_template)
    Task<IReadOnlyList<TaskSubTemplateRow>> ReadTaskSubsAsync(CancellationToken cancellationToken = default);

    // Đọc template nhiệm vụ phụ theo id tăng dần. (Java: side_task_template)
    Task<IReadOnlyList<SideTaskTemplateRow>> ReadSideTaskTemplatesAsync(CancellationToken cancellationToken = default);

    // Đọc template nhiệm vụ bang hội theo id tăng dần. (Java: clan_task_template)
    Task<IReadOnlyList<ClanTaskTemplateRow>> ReadClanTaskTemplatesAsync(CancellationToken cancellationToken = default);

    // Đọc template nhiệm vụ KOL theo id tăng dần. (Java: task_kol_template)
    Task<IReadOnlyList<TaskKolTemplateRow>> ReadKolTaskTemplatesAsync(CancellationToken cancellationToken = default);

    // Đọc template nhiệm vụ sự kiện theo id tăng dần. (Java: task_event_template)
    Task<IReadOnlyList<TaskEventTemplateRow>> ReadEventTaskTemplatesAsync(CancellationToken cancellationToken = default);

    // Đọc template thành tựu theo id tăng dần. (Java: achievement_template)
    Task<IReadOnlyList<AchievementTemplateRow>> ReadAchievementTemplatesAsync(CancellationToken cancellationToken = default);

    // Đọc cửa hàng theo NPC rồi id để phá tie ổn định. (Java: ShopDAO.getShops)
    Task<IReadOnlyList<ShopRow>> ReadShopsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ShopRow>>(Array.Empty<ShopRow>());

    // Đọc tab cửa hàng theo shop rồi id. (Java: ShopDAO.loadShopTab)
    Task<IReadOnlyList<TabShopRow>> ReadShopTabsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TabShopRow>>(Array.Empty<TabShopRow>());

    // Đọc mọi item theo tab để loader phân loại hàng Java không bao giờ đọc. (Java: ShopDAO.loadItemShop)
    Task<IReadOnlyList<ItemShopRow>> ReadShopItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ItemShopRow>>(Array.Empty<ItemShopRow>());

    // Đọc option item shop theo item rồi id. (Java: ShopDAO.loadItemShopOption)
    Task<IReadOnlyList<ItemShopOptionRow>> ReadShopItemOptionsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<ItemShopOptionRow>>(Array.Empty<ItemShopOptionRow>());

    // Đọc điều kiện nhiệm vụ huy hiệu theo id. (Java: TASKS_BADGES_TEMPLATE)
    Task<IReadOnlyList<TaskBadgesTemplateRow>> ReadBadgeTaskTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TaskBadgesTemplateRow>>(Array.Empty<TaskBadgesTemplateRow>());

    // Đọc mẫu huy hiệu theo id. (Java: BAGES_TEMPLATES)
    Task<IReadOnlyList<DataBadgesRow>> ReadBadgeTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<DataBadgesRow>>(Array.Empty<DataBadgesRow>());

    // Đọc thông báo theo id giảm dần. (Java: NOTIFY)
    Task<IReadOnlyList<NotifyRow>> ReadNotificationsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<NotifyRow>>(Array.Empty<NotifyRow>());

    // Đọc thẻ radar theo id. (Java: RadarService.RADAR_TEMPLATE)
    Task<IReadOnlyList<RadarRow>> ReadRadarCardsAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<RadarRow>>(Array.Empty<RadarRow>());

    // Đọc mẫu quái theo id tăng dần để id còn là chỉ số catalog. (Java: mob_template)
    Task<IReadOnlyList<MobTemplateRow>> ReadMobTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<MobTemplateRow>>(Array.Empty<MobTemplateRow>());

    // Đọc mẫu NPC theo id tăng dần để id còn là chỉ số catalog. (Java: npc_template)
    Task<IReadOnlyList<NpcTemplateRow>> ReadNpcTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<NpcTemplateRow>>(Array.Empty<NpcTemplateRow>());

    // Đọc mẫu map theo id tăng dần để id còn là chỉ số catalog. (Java: map_template)
    Task<IReadOnlyList<MapTemplateRow>> ReadMapTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<MapTemplateRow>>(Array.Empty<MapTemplateRow>());
}
