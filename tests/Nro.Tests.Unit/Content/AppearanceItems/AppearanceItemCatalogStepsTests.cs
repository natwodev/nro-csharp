using Nro.Content.Loaders;
using Nro.Data.Entities.StaticData;
using Nro.Game.Content;

namespace Nro.Tests.Unit.Content.AppearanceItems;

// Kiểm thử các bảng appearance/items bằng nguồn bộ nhớ, không có integration DB. Nguồn Java: nro/server/Manager.java (dòng 351–436, 560–600, 727–741).
public sealed class AppearanceItemCatalogStepsTests
{
    // Parse các kiểu dữ liệu, công bố tra cứu id và dẫn xuất mount đúng điều kiện Java. (Java: DataGame.MAP_MOUNT_NUM)
    [Fact]
    public async Task Loads_appearance_items_images_and_mount_map_from_memory()
    {
        var source = new InMemoryStaticRowSource
        {
            Parts = [new PartRow { PartId = 5, Type = 1, Data = "[[12,-2,3]]" }],
            BgItems = [new BgItemTemplateRow { BgItemTemplateId = 7, ImageId = 3, Layer = 1, Dx = -4, Dy = 5 }],
            ItemTemplates =
            [
                Item(0, 23, 5),
                Item(1, 22, 9),
            ],
            ItemOptionTemplates = [new ItemOptionTemplateRow { ItemOptionTemplateId = 0, Name = "Sức mạnh #", Type = 2 }],
            HeadAvatars = [new HeadAvatarRow { HeadId = 10, AvatarId = 11 }],
            FlagBags = [new FlagBagRow { FlagBagId = 12, IconData = "13,14", Name = "Cờ", IconId = 15, Gold = 16, Gem = 17 }],
            HeadAnimationFrames = [new HeadAnimationFrameRow { HeadAnimationFrameId = 0, Data = "[18,19]" }],
            ImagesByName = [new ImgByNameRow { ImgByNameId = 20, Name = "mount_5_0", NFrame = 2 }],
        };
        var loader = new StaticCatalogLoader(AppearanceItemCatalogSteps.Create(source));

        IStaticCatalogs catalog = await loader.LoadAsync();

        Assert.Equal((short)12, catalog.Appearance.GetPart(5).PartDetails[0].IconId);
        Assert.Equal((sbyte)-2, catalog.Appearance.GetPart(5).PartDetails[0].Dx);
        Assert.Equal(new short[] { 13, 14 }, catalog.Appearance.GetFlagBag(12).IconEffect);
        Assert.Equal(new[] { 18, 19 }, catalog.Appearance.GetHeadAnimationFrame(0).Frames);
        Assert.True(catalog.Images.TryGetFrame("MOUNT_5_0", out sbyte frames));
        Assert.Equal((sbyte)2, frames);
        Assert.True(catalog.Items.TryGetMountPart(0, out int mountPart));
        Assert.Equal(30005, mountPart);
        Assert.False(catalog.Items.TryGetMountPart(1, out _));
        Assert.Contains(loader.LastReport.Entries, entry => entry.StepName == ItemCatalogLoadStep.StepName && entry.RecordCount == 1);
    }

    // Từ chối JSON part sai trước khi công bố snapshot. (Java: new JSONArray(data))
    [Fact]
    public async Task Rejects_invalid_part_json()
    {
        var source = new InMemoryStaticRowSource
        {
            Parts = [new PartRow { PartId = 0, Type = 1, Data = "not-json" }],
        };
        var loader = new StaticCatalogLoader(AppearanceItemCatalogSteps.Create(source));

        CatalogLoadException exception = await Assert.ThrowsAsync<CatalogLoadException>(() => loader.LoadAsync());

        Assert.Equal(PartCatalogLoadStep.StepName, exception.StepName);
        Assert.Contains("JSON", exception.Message, StringComparison.Ordinal);
    }

    // Từ chối PowerRequire long không thể tái hiện Java getInt. (Java: getInt("strRequire"))
    [Fact]
    public async Task Rejects_item_power_requirement_outside_java_int_range()
    {
        var source = new InMemoryStaticRowSource
        {
            ItemTemplates = [Item(0, 0, 0, (long)int.MaxValue + 1)],
        };
        var loader = new StaticCatalogLoader(AppearanceItemCatalogSteps.Create(source));

        CatalogLoadException exception = await Assert.ThrowsAsync<CatalogLoadException>(() => loader.LoadAsync());

        Assert.Equal(ItemTemplateCatalogLoadStep.StepName, exception.StepName);
        Assert.Contains("power_require", exception.Message, StringComparison.Ordinal);
    }

    // Từ chối ID frame có lỗ vì consumer Java tra theo vị trí danh sách. (Java: ARR_HEAD_2_FRAMES.get)
    [Fact]
    public async Task Rejects_a_gap_in_head_animation_frame_ids()
    {
        var source = new InMemoryStaticRowSource
        {
            HeadAnimationFrames =
            [
                new HeadAnimationFrameRow { HeadAnimationFrameId = 1, Data = "[]" },
                new HeadAnimationFrameRow { HeadAnimationFrameId = 3, Data = "[]" },
            ],
        };
        var loader = new StaticCatalogLoader(AppearanceItemCatalogSteps.Create(source));

        CatalogLoadException exception = await Assert.ThrowsAsync<CatalogLoadException>(() => loader.LoadAsync());

        Assert.Equal(HeadAnimationFrameCatalogLoadStep.StepName, exception.StepName);
        Assert.Contains("liên tục", exception.Message, StringComparison.Ordinal);
    }

    // Từ chối ID item trùng trước khi áp dụng tra cứu vị trí. (Java: ITEM_TEMPLATES.get)
    [Fact]
    public async Task Rejects_duplicate_item_template_ids()
    {
        var source = new InMemoryStaticRowSource
        {
            ItemTemplates = [Item(0, 0, 0), Item(0, 0, 0)],
        };
        var loader = new StaticCatalogLoader(AppearanceItemCatalogSteps.Create(source));

        CatalogLoadException exception = await Assert.ThrowsAsync<CatalogLoadException>(() => loader.LoadAsync());

        Assert.Equal(ItemTemplateCatalogLoadStep.StepName, exception.StepName);
        Assert.Contains("trùng", exception.Message, StringComparison.Ordinal);
    }

    // Giữ số count snapshot làm dữ liệu kỳ vọng của loader/report, không dùng DB. (Java: Logger.success)
    [Fact]
    public async Task Expected_counts_reject_a_non_snapshot_source_before_publish()
    {
        var loader = AppearanceItemCatalogSteps.CreateLoader(new InMemoryStaticRowSource());
        IStaticCatalogs before = loader.Current;

        CatalogLoadException exception = await Assert.ThrowsAsync<CatalogLoadException>(() => loader.LoadAsync());

        Assert.Equal(PartCatalogLoadStep.StepName, exception.StepName);
        Assert.Same(before, loader.Current);
        Assert.Equal(1944, ExpectedCounts.Part);
        Assert.Equal(74, ExpectedCounts.Mount);
    }

    private static ItemTemplateRow Item(int id, int type, int part, long powerRequire = 0) => new()
    {
        ItemTemplateId = id,
        Type = type,
        Gender = 0,
        Name = "Item",
        Description = "Mô tả",
        Level = 0,
        IconId = 0,
        Part = part,
        IsUpToUp = false,
        PowerRequire = powerRequire,
        Gold = 0,
        GoldSell = 0,
        Gem = 0,
        GemSell = 0,
        Ruby = 0,
        RubySell = 0,
        Head = 0,
        Body = 0,
        Leg = 0,
        TypeEvent = 0,
        IsGender = 0,
    };
}
