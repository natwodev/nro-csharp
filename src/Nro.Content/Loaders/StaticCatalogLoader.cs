using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Điều phối nạp catalog theo phụ thuộc rồi hoán đổi snapshot đúng một lần. Nguồn Java: nro/server/Manager.java (dòng 192–218, 1145–1199).
public sealed class StaticCatalogLoader : IStaticCatalogLoader
{
    private readonly IReadOnlyList<ICatalogLoadStep> steps;
    private readonly IReadOnlyDictionary<string, int> expectedRecordCounts;
    private IStaticCatalogs current = new StaticCatalogBuilder().Build();
    private CatalogLoadReport lastReport = CatalogLoadReport.Empty;

    // Khởi tạo loader với các bước và số bản ghi kỳ vọng tùy chọn. (Java: loadDatabase)
    public StaticCatalogLoader(IEnumerable<ICatalogLoadStep> steps, IReadOnlyDictionary<string, int>? expectedRecordCounts = null)
    {
        ArgumentNullException.ThrowIfNull(steps);
        this.steps = OrderSteps(steps).AsReadOnly();
        this.expectedRecordCounts = expectedRecordCounts is null
            ? new Dictionary<string, int>(StringComparer.Ordinal)
            : new Dictionary<string, int>(expectedRecordCounts, StringComparer.Ordinal);
        ValidateExpectedCounts();
    }

    // Snapshot hiện tại; mọi lần nạp lỗi hay bị hủy vẫn giữ nguyên tham chiếu này. (Java: Manager)
    public IStaticCatalogs Current => Volatile.Read(ref current);

    // Báo cáo của lần nạp công bố thành công gần nhất. (Java: Logger.success)
    public CatalogLoadReport LastReport => Volatile.Read(ref lastReport);

    // Nạp theo thứ tự phụ thuộc, kiểm tra số lượng rồi công bố snapshot duy nhất. (Java: loadDatabase)
    public async Task<IStaticCatalogs> LoadAsync(CancellationToken cancellationToken = default)
    {
        var builder = new StaticCatalogBuilder();
        var entries = new List<CatalogLoadReportEntry>(steps.Count);

        foreach (ICatalogLoadStep step in steps)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CatalogStepResult result;
            try
            {
                result = await step.LoadAsync(builder, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested)
            {
                // Chuẩn hóa ngoại lệ hủy của bước (TaskCanceledException) thành hợp đồng hủy của loader.
                throw new OperationCanceledException(exception.Message, exception, cancellationToken);
            }
            catch (CatalogLoadException)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new CatalogLoadException(step.Name, $"Không thể nạp bước danh mục '{step.Name}'.", exception);
            }

            if (result is null)
            {
                throw new CatalogLoadException(step.Name, $"Bước danh mục '{step.Name}' không trả kết quả.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (expectedRecordCounts.TryGetValue(step.Name, out int expectedCount) && result.RecordCount != expectedCount)
            {
                throw new CatalogLoadException(step.Name, $"Bước danh mục '{step.Name}' nạp {result.RecordCount} bản ghi, kỳ vọng {expectedCount}.");
            }

            entries.Add(new CatalogLoadReportEntry(step.Name, result.RecordCount));
        }

        cancellationToken.ThrowIfCancellationRequested();
        IStaticCatalogs next = builder.Build();
        var report = new CatalogLoadReport(entries);
        Volatile.Write(ref current, next);
        Volatile.Write(ref lastReport, report);
        return next;
    }

    private static List<ICatalogLoadStep> OrderSteps(IEnumerable<ICatalogLoadStep> source)
    {
        var byName = new Dictionary<string, ICatalogLoadStep>(StringComparer.Ordinal);
        foreach (ICatalogLoadStep step in source)
        {
            if (step is null)
            {
                throw new CatalogLoadException("validation", "Danh sách bước có phần tử null.");
            }

            if (String.IsNullOrWhiteSpace(step.Name))
            {
                throw new CatalogLoadException("validation", "Tên bước danh mục không được để trống.");
            }

            if (!byName.TryAdd(step.Name, step))
            {
                throw new CatalogLoadException(step.Name, $"Tên bước danh mục '{step.Name}' bị trùng.");
            }
        }

        foreach (ICatalogLoadStep step in byName.Values)
        {
            if (step.Dependencies is null)
            {
                throw new CatalogLoadException(step.Name, $"Bước danh mục '{step.Name}' có danh sách phụ thuộc null.");
            }

            foreach (string dependency in step.Dependencies)
            {
                if (!byName.ContainsKey(dependency))
                {
                    throw new CatalogLoadException(step.Name, $"Bước danh mục '{step.Name}' phụ thuộc bước không tồn tại '{dependency}'.");
                }
            }
        }

        var ordered = new List<ICatalogLoadStep>(byName.Count);
        var completed = new HashSet<string>(StringComparer.Ordinal);
        while (ordered.Count < byName.Count)
        {
            int countBefore = ordered.Count;
            foreach ((string name, ICatalogLoadStep step) in byName)
            {
                if (completed.Contains(name) || !step.Dependencies.All(completed.Contains))
                {
                    continue;
                }

                completed.Add(name);
                ordered.Add(step);
            }

            if (ordered.Count == countBefore)
            {
                string remaining = String.Join(", ", byName.Keys.Where(name => !completed.Contains(name)).OrderBy(name => name, StringComparer.Ordinal));
                throw new CatalogLoadException("validation", $"Phát hiện vòng phụ thuộc giữa các bước danh mục: {remaining}.");
            }
        }

        return ordered;
    }

    private void ValidateExpectedCounts()
    {
        foreach ((string name, int expectedCount) in expectedRecordCounts)
        {
            if (expectedCount < 0)
            {
                throw new CatalogLoadException(name, $"Số bản ghi kỳ vọng của bước '{name}' không được âm.");
            }

            if (!steps.Any(step => String.Equals(step.Name, name, StringComparison.Ordinal)))
            {
                throw new CatalogLoadException(name, $"Không có bước danh mục '{name}' để kiểm tra số bản ghi kỳ vọng.");
            }
        }
    }
}
