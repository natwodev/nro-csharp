using Nro.Content.Loaders;
using Nro.Core.Content;
using Nro.Core.Templates;
using Nro.Game.Content;

namespace Nro.Tests.Unit.Content.StaticCatalogLoading;

// Kiểm thử nạp snapshot nguyên khối thay cho System.exit của Manager Java. Nguồn Java: nro/server/Manager.java (dòng 192–218, 1145–1199).
public sealed class StaticCatalogLoaderTests
{
    // Không công bố builder dở dang khi một bước sau thất bại. (Java: loadDatabase)
    [Fact]
    public async Task Failed_step_keeps_the_previous_snapshot()
    {
        var newItems = new TestItemCatalog("new");
        var loader = new StaticCatalogLoader(
        [
            new TestStep("items", [], (builder, _) =>
            {
                builder.SetItems(newItems);
                return Task.FromResult(new CatalogStepResult(1));
            }),
            new TestStep("fails", ["items"], (_, _) => Task.FromException<CatalogStepResult>(new InvalidOperationException("boom"))),
        ]);
        IStaticCatalogs before = loader.Current;

        await Assert.ThrowsAnyAsync<CatalogLoadException>(() => loader.LoadAsync());

        Assert.Same(before, loader.Current);
        Assert.NotSame(newItems, loader.Current.Items);
    }

    // Chạy bước phụ thuộc trước bước yêu cầu nó dù thứ tự cung cấp bị đảo. (Java: loadDatabase)
    [Fact]
    public async Task Orders_steps_by_dependencies()
    {
        var order = new List<string>();
        var loader = new StaticCatalogLoader(
        [
            new TestStep("second", ["first"], (_, _) =>
            {
                order.Add("second");
                return Task.FromResult(new CatalogStepResult(2));
            }),
            new TestStep("first", [], (_, _) =>
            {
                order.Add("first");
                return Task.FromResult(new CatalogStepResult(1));
            }),
        ]);

        await loader.LoadAsync();

        Assert.Equal(["first", "second"], order);
    }

    // Từ chối vòng phụ thuộc trước khi nạp và công bố dữ liệu. (Java: loadDatabase)
    [Fact]
    public void Rejects_dependency_cycles()
    {
        CatalogLoadException exception = Assert.Throws<CatalogLoadException>(() => new StaticCatalogLoader(
        [
            new TestStep("first", ["second"], (_, _) => Task.FromResult(new CatalogStepResult(0))),
            new TestStep("second", ["first"], (_, _) => Task.FromResult(new CatalogStepResult(0))),
        ]));

        Assert.Contains("vòng phụ thuộc", exception.Message, StringComparison.Ordinal);
    }

    // Hủy trong khi nạp không được làm thay snapshot đã công bố. (Java: loadDatabase)
    [Fact]
    public async Task Cancellation_between_steps_keeps_the_previous_snapshot()
    {
        var began = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        bool ranAfterCancellation = false;
        var loader = new StaticCatalogLoader(
        [
            new TestStep("first", [], (_, _) => Task.FromResult(new CatalogStepResult(1))),
            new TestStep("wait", ["first"], async (_, cancellationToken) =>
            {
                began.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                return new CatalogStepResult(0);
            }),
            new TestStep("independent-after", [], (_, _) =>
            {
                ranAfterCancellation = true;
                return Task.FromResult(new CatalogStepResult(1));
            }),
        ]);
        IStaticCatalogs before = loader.Current;
        using var cancellation = new CancellationTokenSource();
        Task<IStaticCatalogs> loading = loader.LoadAsync(cancellation.Token);
        await began.Task;

        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => loading);
        Assert.Same(before, loader.Current);
        Assert.False(ranAfterCancellation);
    }

    // Lưu số bản ghi của từng bước theo đúng thứ tự đã chạy. (Java: Logger.success)
    [Fact]
    public async Task Reports_each_step_record_count()
    {
        var loader = new StaticCatalogLoader(
        [
            new TestStep("items", [], (_, _) => Task.FromResult(new CatalogStepResult(7))),
            new TestStep("world", ["items"], (_, _) => Task.FromResult(new CatalogStepResult(3))),
        ]);

        await loader.LoadAsync();

        Assert.Collection(
            loader.LastReport.Entries,
            entry => Assert.Equal(("items", 7), (entry.StepName, entry.RecordCount)),
            entry => Assert.Equal(("world", 3), (entry.StepName, entry.RecordCount)));
    }

    // Gom cảnh báo từ bước vào báo cáo công bố cùng snapshot. (Java: Logger.warning)
    [Fact]
    public async Task Reports_step_warnings()
    {
        var loader = new StaticCatalogLoader(
        [
            new TestStep("world", [], (_, _) => Task.FromResult(new CatalogStepResult(1, ["map-template: thiếu id 205"]))),
        ]);

        await loader.LoadAsync();

        Assert.Equal(["map-template: thiếu id 205"], loader.LastReport.Warnings);
    }

    // Không công bố snapshot khi số bản ghi khác số đã xác nhận. (Java: Logger.success)
    [Fact]
    public async Task Expected_record_count_mismatch_is_rejected()
    {
        var loader = new StaticCatalogLoader(
            [new TestStep("items", [], (_, _) => Task.FromResult(new CatalogStepResult(4)))],
            new Dictionary<string, int>(StringComparer.Ordinal) { ["items"] = 5 });
        IStaticCatalogs before = loader.Current;

        CatalogLoadException exception = await Assert.ThrowsAnyAsync<CatalogLoadException>(() => loader.LoadAsync());

        Assert.Equal("items", exception.StepName);
        Assert.Same(before, loader.Current);
    }

    // Các nhánh độc lập vẫn chạy để một lượt bootstrap trả đủ lỗi; nhánh phụ thuộc bị ghi nhận là bỏ qua.
    [Fact]
    public async Task Aggregates_independent_failures_and_skips_dependents_without_publishing()
    {
        var ran = new List<string>();
        var loader = new StaticCatalogLoader(
        [
            new TestStep("first", [], (_, _) => Task.FromException<CatalogStepResult>(new InvalidOperationException("first boom"))),
            new TestStep("dependent", ["first"], (_, _) => { ran.Add("dependent"); return Task.FromResult(new CatalogStepResult(1)); }),
            new TestStep("second", [], (_, _) => Task.FromException<CatalogStepResult>(new InvalidOperationException("second boom"))),
        ]);
        IStaticCatalogs before = loader.Current;

        CatalogLoadAggregateException error = await Assert.ThrowsAsync<CatalogLoadAggregateException>(() => loader.LoadAsync());

        Assert.Equal(["first", "second"], error.Errors.Select(value => value.StepName));
        CatalogLoadSkippedStep skipped = Assert.Single(error.SkippedSteps);
        Assert.Equal(("dependent", "first"), (skipped.StepName, skipped.FailedDependencyStepName));
        Assert.Empty(ran);
        Assert.Same(before, loader.Current);
    }

    private sealed class TestStep : ICatalogLoadStep
    {
        private readonly Func<StaticCatalogBuilder, CancellationToken, Task<CatalogStepResult>> load;

        // Tạo bước giả với hàm nạp do ca kiểm thử cung cấp. (Java: loadDatabase)
        public TestStep(string name, IReadOnlyCollection<string> dependencies, Func<StaticCatalogBuilder, CancellationToken, Task<CatalogStepResult>> load)
        {
            Name = name;
            Dependencies = dependencies;
            this.load = load;
        }

        // Tên bước giả. (Java: loadDatabase)
        public string Name { get; }

        // Phụ thuộc của bước giả. (Java: loadDatabase)
        public IReadOnlyCollection<string> Dependencies { get; }

        // Chạy hàm nạp giả. (Java: loadDatabase)
        public Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken) => load(builder, cancellationToken);
    }

    private sealed class TestItemCatalog(string label) : IItemCatalog
    {
        // Nhãn nhận diện catalog giả. (Java: ITEM_TEMPLATES)
        public string Label { get; } = label;

        // Danh sách mẫu vật phẩm giả. (Java: ITEM_TEMPLATES)
        public IReadOnlyList<ItemTemplate> ItemTemplates { get; } = Array.Empty<ItemTemplate>();

        // Danh sách tùy chọn vật phẩm giả. (Java: ITEM_OPTION_TEMPLATES)
        public IReadOnlyList<ItemOptionTemplate> ItemOptionTemplates { get; } = Array.Empty<ItemOptionTemplate>();

        // Danh sách nền giả. (Java: BG_ITEMS)
        public IReadOnlyList<BgItem> BgItems { get; } = Array.Empty<BgItem>();

        // Ánh xạ thú cưỡi giả. (Java: MAP_MOUNT_NUM)
        public IReadOnlyDictionary<short, int> MountItemParts { get; } = new Dictionary<short, int>();

        // Không có item giả theo id. (Java: ITEM_TEMPLATES.get)
        public ItemTemplate GetItemTemplate(int id) => throw new ArgumentOutOfRangeException(nameof(id));

        // Không có option giả theo id. (Java: ITEM_OPTION_TEMPLATES.get)
        public ItemOptionTemplate GetItemOptionTemplate(int id) => throw new ArgumentOutOfRangeException(nameof(id));

        // Không có nền giả theo id. (Java: BG_ITEMS)
        public BgItem GetBgItem(int id) => throw new KeyNotFoundException();

        // Không có thú cưỡi giả. (Java: MAP_MOUNT_NUM.get)
        public bool TryGetMountPart(short itemId, out int part)
        {
            part = default;
            return false;
        }

    }
}
