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
}
