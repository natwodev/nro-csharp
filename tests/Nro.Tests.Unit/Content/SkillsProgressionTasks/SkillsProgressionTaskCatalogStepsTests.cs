using Nro.Content.Loaders;
using Nro.Content.StaticData;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;
using Nro.Game.Items.Core;
using Nro.Game.Progress.Tasks;

namespace Nro.Tests.Unit.Content.SkillsProgressionTasks;

// Kiểm thử catalog J-01C bằng nguồn hàng trong bộ nhớ, không mở DB. Nguồn Java: nro/server/Manager.java (dòng 199–202, 437–705).
public sealed class SkillsProgressionTaskCatalogStepsTests
{
    [Fact]
    public async Task Loads_skills_progression_and_tasks_from_in_memory_rows()
    {
        var loader = SkillsProgressionTaskCatalogSteps.CreateLoader(ValidRows(), new TestKolItems());

        IStaticCatalogs snapshot = await loader.LoadAsync();

        Assert.Equal(3, snapshot.Skills.Classes.Count);
        Assert.Equal(8, snapshot.Skills.GetTemplate(2, 8).Id);
        Assert.Equal(5, snapshot.Progression.AttributeTemplates.Count);
        Assert.Equal(27, snapshot.Progression.GetIntrinsics(0).Count);
        Assert.Equal(20, snapshot.Progression.Achievements.Count);
        Assert.Equal(33, snapshot.Tasks.TaskMains.Count);
        Assert.Equal(5, snapshot.Tasks.GetTaskMain(0).SubTasks.Count);
        Assert.Equal(0, snapshot.Tasks.GetSideTaskTemplate(0, 0).Id);
        Assert.Equal(58, snapshot.Tasks.GetEventTaskTemplate(58, 58).Id);
        Assert.Equal(27, loader.LastReport.Entries[0].RecordCount);
        Assert.Equal(87, loader.LastReport.Entries[1].RecordCount);
        Assert.Equal(359, loader.LastReport.Entries[2].RecordCount);
    }

    [Fact]
    public async Task Rejects_invalid_skill_json_without_publishing_snapshot()
    {
        var rows = ValidRows() with { SkillTemplates = [new SkillTemplateRow { NclassId = 0, SkillTemplateId = 0, Name = "K", MaxPoint = 1, ManaUseType = 1, Type = 1, IconId = 1, DamInfo = "D", Slot = 0, Skills = "{" }] };
        var loader = new StaticCatalogLoader([new SkillCatalogLoadStep(rows)]);

        CatalogLoadException error = await Assert.ThrowsAsync<CatalogLoadException>(() => loader.LoadAsync());

        Assert.Equal(SkillCatalogLoadStep.StepName, error.StepName);
        Assert.Contains("JSON", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Rejects_out_of_range_task_byte_and_missing_parent()
    {
        var invalidByte = new InMemoryRows
        {
            TaskMains = [new TaskMainTemplateRow { TaskMainTemplateId = 0, Name = "main", Detail = "detail" }],
            TaskSubs = [new TaskSubTemplateRow { Idmain = 1, TaskMainId = 0, Name = "sub", MaxCount = 1, Notify = "n", NpcId = 128, Map = 1 }],
        };
        var loader = new StaticCatalogLoader([new TaskCatalogLoadStep(invalidByte, new TestKolItems())]);
        CatalogLoadException range = await Assert.ThrowsAsync<CatalogLoadException>(() => loader.LoadAsync());
        Assert.Contains("npc_id", range.Message, StringComparison.Ordinal);

        var missingParent = invalidByte with { TaskSubs = [new TaskSubTemplateRow { Idmain = 1, TaskMainId = 9, Name = "sub", MaxCount = 1, Notify = "n", NpcId = 1, Map = 1 }] };
        loader = new StaticCatalogLoader([new TaskCatalogLoadStep(missingParent, new TestKolItems())]);
        CatalogLoadException foreignKey = await Assert.ThrowsAsync<CatalogLoadException>(() => loader.LoadAsync());
        Assert.Contains("không tồn tại", foreignKey.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Excludes_main_without_subtask_like_java_inner_join()
    {
        var rows = new InMemoryRows
        {
            TaskMains = [new TaskMainTemplateRow { TaskMainTemplateId = 0, Name = "kept", Detail = "d" }, new TaskMainTemplateRow { TaskMainTemplateId = 1, Name = "excluded", Detail = "d" }],
            TaskSubs = [new TaskSubTemplateRow { Idmain = 1, TaskMainId = 0, Name = "sub", MaxCount = 1, Notify = "n", NpcId = 1, Map = 1 }],
        };
        var loader = new StaticCatalogLoader([new TaskCatalogLoadStep(rows, new TestKolItems())]);

        IStaticCatalogs snapshot = await loader.LoadAsync();

        Assert.Single(snapshot.Tasks.TaskMains);
        Assert.Equal(0, snapshot.Tasks.TaskMains[0].Id);
    }

    private static InMemoryRows ValidRows()
    {
        const string skillJson = "[{\"id\":0,\"point\":1,\"power_require\":2,\"mana_use\":3,\"cool_down\":4,\"dx\":5,\"dy\":6,\"max_fight\":7,\"damage\":8,\"price\":9,\"info\":\"ok\"}]";
        return new InMemoryRows
        {
            AttributeTemplates = Enumerable.Range(0, 5).Select(id => new AttributeTemplateRow { AttributeTemplateId = id, Name = $"a{id}" }).ToArray(),
            PowerLimits = Enumerable.Range(0, 14).Select(id => new PowerLimitRow { PowerLimitId = id }).ToArray(),
            Captions = Enumerable.Range(0, 21).Select(id => new CaptionRow { CaptionId = id, Earth = "e", Saiya = "s", Namek = "n" }).ToArray(),
            SkillTemplates = Enumerable.Range(0, 3).SelectMany(classId => Enumerable.Range(0, 9).Select(id => new SkillTemplateRow { NclassId = classId, SkillTemplateId = id, Name = $"s{id}", MaxPoint = 1, ManaUseType = 1, Type = 1, IconId = 1, DamInfo = "d", Slot = id, Skills = skillJson })).ToArray(),
            Intrinsics = Enumerable.Range(0, 27).Select(id => new IntrinsicRow { IntrinsicId = id, Name = "i", Gender = 3 }).ToArray(),
            AchievementTemplates = Enumerable.Range(1, 20).Select(id => new AchievementTemplateRow { AchievementTemplateId = id, Info1 = "a", Info2 = "b" }).ToArray(),
            TaskMains = Enumerable.Range(0, 33).Select(id => new TaskMainTemplateRow { TaskMainTemplateId = id, Name = $"m{id}", Detail = "d" }).ToArray(),
            TaskSubs = Enumerable.Range(0, 142).Select(id => new TaskSubTemplateRow { Idmain = id, TaskMainId = id % 33, Name = "sub", MaxCount = 1, Notify = "n", NpcId = 1, Map = 1 }).ToArray(),
            SideTaskTemplates = Enumerable.Range(0, 59).Select(Side).ToArray(),
            ClanTaskTemplates = Enumerable.Range(0, 59).Select(Clan).ToArray(),
            KolTaskTemplates = Enumerable.Range(0, 7).Select(id => new TaskKolTemplateRow { TaskKolTemplateId = id, Info = "k", MaxCount = "1" }).ToArray(),
            EventTaskTemplates = Enumerable.Range(0, 59).Select(Event).ToArray(),
        };
    }

    private static SideTaskTemplateRow Side(int id) => new() { SideTaskTemplateId = id, Name = "s", MaxCountLv1 = "1-2", MaxCountLv2 = "1-2", MaxCountLv3 = "1-2", MaxCountLv4 = "1-2", MaxCountLv5 = "1-2" };
    private static ClanTaskTemplateRow Clan(int id) => new() { ClanTaskTemplateId = id, Name = "c", MaxCountLv1 = "1-2", MaxCountLv2 = "1-2", MaxCountLv3 = "1-2", MaxCountLv4 = "1-2", MaxCountLv5 = "1-2" };
    private static TaskEventTemplateRow Event(int id) => new() { TaskEventTemplateId = id, Name = "e", MaxCountLv1 = "1-2", MaxCountLv2 = "1-2", MaxCountLv3 = "1-2", MaxCountLv4 = "1-2", MaxCountLv5 = "1-2" };

    private sealed record InMemoryRows : IStaticRowSource
    {
        public IReadOnlyList<PartRow> Parts { get; init; } = [];
        public IReadOnlyList<BgItemTemplateRow> BgItems { get; init; } = [];
        public IReadOnlyList<ItemTemplateRow> ItemTemplates { get; init; } = [];
        public IReadOnlyList<ItemOptionTemplateRow> ItemOptionTemplates { get; init; } = [];
        public IReadOnlyList<HeadAvatarRow> HeadAvatars { get; init; } = [];
        public IReadOnlyList<FlagBagRow> FlagBags { get; init; } = [];
        public IReadOnlyList<HeadAnimationFrameRow> HeadAnimationFrames { get; init; } = [];
        public IReadOnlyList<ImgByNameRow> ImagesByName { get; init; } = [];
        public IReadOnlyList<AttributeTemplateRow> AttributeTemplates { get; init; } = [];
        public IReadOnlyList<PowerLimitRow> PowerLimits { get; init; } = [];
        public IReadOnlyList<CaptionRow> Captions { get; init; } = [];
        public IReadOnlyList<SkillTemplateRow> SkillTemplates { get; init; } = [];
        public IReadOnlyList<IntrinsicRow> Intrinsics { get; init; } = [];
        public IReadOnlyList<TaskMainTemplateRow> TaskMains { get; init; } = [];
        public IReadOnlyList<TaskSubTemplateRow> TaskSubs { get; init; } = [];
        public IReadOnlyList<SideTaskTemplateRow> SideTaskTemplates { get; init; } = [];
        public IReadOnlyList<ClanTaskTemplateRow> ClanTaskTemplates { get; init; } = [];
        public IReadOnlyList<TaskKolTemplateRow> KolTaskTemplates { get; init; } = [];
        public IReadOnlyList<TaskEventTemplateRow> EventTaskTemplates { get; init; } = [];
        public IReadOnlyList<AchievementTemplateRow> AchievementTemplates { get; init; } = [];
        public Task<IReadOnlyList<PartRow>> ReadPartsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Parts);
        public Task<IReadOnlyList<BgItemTemplateRow>> ReadBgItemsAsync(CancellationToken cancellationToken = default) => Task.FromResult(BgItems);
        public Task<IReadOnlyList<ItemTemplateRow>> ReadItemTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(ItemTemplates);
        public Task<IReadOnlyList<ItemOptionTemplateRow>> ReadItemOptionTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(ItemOptionTemplates);
        public Task<IReadOnlyList<HeadAvatarRow>> ReadHeadAvatarsAsync(CancellationToken cancellationToken = default) => Task.FromResult(HeadAvatars);
        public Task<IReadOnlyList<FlagBagRow>> ReadFlagBagsAsync(CancellationToken cancellationToken = default) => Task.FromResult(FlagBags);
        public Task<IReadOnlyList<HeadAnimationFrameRow>> ReadHeadAnimationFramesAsync(CancellationToken cancellationToken = default) => Task.FromResult(HeadAnimationFrames);
        public Task<IReadOnlyList<ImgByNameRow>> ReadImagesByNameAsync(CancellationToken cancellationToken = default) => Task.FromResult(ImagesByName);
        public Task<IReadOnlyList<AttributeTemplateRow>> ReadAttributeTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(AttributeTemplates);
        public Task<IReadOnlyList<PowerLimitRow>> ReadPowerLimitsAsync(CancellationToken cancellationToken = default) => Task.FromResult(PowerLimits);
        public Task<IReadOnlyList<CaptionRow>> ReadCaptionsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Captions);
        public Task<IReadOnlyList<SkillTemplateRow>> ReadSkillTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(SkillTemplates);
        public Task<IReadOnlyList<IntrinsicRow>> ReadIntrinsicsAsync(CancellationToken cancellationToken = default) => Task.FromResult(Intrinsics);
        public Task<IReadOnlyList<TaskMainTemplateRow>> ReadTaskMainsAsync(CancellationToken cancellationToken = default) => Task.FromResult(TaskMains);
        public Task<IReadOnlyList<TaskSubTemplateRow>> ReadTaskSubsAsync(CancellationToken cancellationToken = default) => Task.FromResult(TaskSubs);
        public Task<IReadOnlyList<SideTaskTemplateRow>> ReadSideTaskTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(SideTaskTemplates);
        public Task<IReadOnlyList<ClanTaskTemplateRow>> ReadClanTaskTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(ClanTaskTemplates);
        public Task<IReadOnlyList<TaskKolTemplateRow>> ReadKolTaskTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(KolTaskTemplates);
        public Task<IReadOnlyList<TaskEventTemplateRow>> ReadEventTaskTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(EventTaskTemplates);
        public Task<IReadOnlyList<AchievementTemplateRow>> ReadAchievementTemplatesAsync(CancellationToken cancellationToken = default) => Task.FromResult(AchievementTemplates);
    }

    private sealed class TestKolItems : IKolTaskItemFactory
    {
        public Item CreateNewItemLock(int itemTemplateId, int quantity) => new() { Quantity = quantity };
        public ItemOption CreateOption(int optionTemplateId, int param) => new() { Param = param };
        public Item CopyItem(Item item) => new() { Quantity = item.Quantity };
    }
}
