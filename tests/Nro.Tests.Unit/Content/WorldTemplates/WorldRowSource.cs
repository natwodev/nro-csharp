using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;

namespace Nro.Tests.Unit.Content.WorldTemplates;

// Nguồn Java: nro/server/Manager.java (dòng 742–855).
// Nguồn hàng world trong bộ nhớ cho unit test, không mở kết nối DB. (Java: ConnectDB)
internal sealed class WorldRowSource : IStaticRowSource
{
    // Các catalog ngoài World không thuộc fixture này; trả tập rỗng để đáp ứng hợp đồng nguồn tĩnh dùng chung.
    private static Task<IReadOnlyList<T>> Empty<T>() => Task.FromResult<IReadOnlyList<T>>(Array.Empty<T>());

    // Hàng quái thử nghiệm. (Java: mob_template)
    public IReadOnlyList<MobTemplateRow> Mobs { get; set; } = Array.Empty<MobTemplateRow>();

    // Hàng NPC thử nghiệm. (Java: npc_template)
    public IReadOnlyList<NpcTemplateRow> Npcs { get; set; } = Array.Empty<NpcTemplateRow>();

    // Hàng map thử nghiệm. (Java: map_template)
    public IReadOnlyList<MapTemplateRow> Maps { get; set; } = Array.Empty<MapTemplateRow>();

    // Trả hàng quái theo thứ tự đã dựng. (Java: mob_template)
    public Task<IReadOnlyList<MobTemplateRow>> ReadMobTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Mobs);

    // Trả hàng NPC theo thứ tự đã dựng. (Java: npc_template)
    public Task<IReadOnlyList<NpcTemplateRow>> ReadNpcTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Npcs);

    // Trả hàng map theo thứ tự đã dựng. (Java: map_template)
    public Task<IReadOnlyList<MapTemplateRow>> ReadMapTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Maps);

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
}
