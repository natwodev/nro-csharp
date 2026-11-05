namespace Nro.Content.Loaders;

// Tổng hợp các lỗi độc lập của một lượt nạp; snapshot chỉ được công bố khi danh sách này rỗng.
public sealed class CatalogLoadAggregateException : CatalogLoadException
{
    public CatalogLoadAggregateException(IReadOnlyList<CatalogLoadException> errors, IReadOnlyList<CatalogLoadSkippedStep> skippedSteps)
        : base(FirstStep(errors), BuildMessage(errors, skippedSteps), errors.Count > 0 ? errors[0] : null)
    {
        ArgumentNullException.ThrowIfNull(errors);
        ArgumentNullException.ThrowIfNull(skippedSteps);
        if (errors.Count == 0)
        {
            throw new ArgumentException("Lỗi gộp phải chứa ít nhất một lỗi.", nameof(errors));
        }

        Errors = errors.ToArray();
        SkippedSteps = skippedSteps.ToArray();
    }

    // Lỗi gốc theo từng bước, giữ InnerException để chẩn đoán nội bộ.
    public IReadOnlyList<CatalogLoadException> Errors { get; }

    // Bước không chạy vì phụ thuộc trực tiếp hoặc gián tiếp đã lỗi.
    public IReadOnlyList<CatalogLoadSkippedStep> SkippedSteps { get; }

    private static string FirstStep(IReadOnlyList<CatalogLoadException> errors) => errors is { Count: > 0 } ? errors[0].StepName : "aggregate";

    private static string BuildMessage(IReadOnlyList<CatalogLoadException> errors, IReadOnlyList<CatalogLoadSkippedStep> skippedSteps)
    {
        int errorCount = errors?.Count ?? 0;
        int skippedCount = skippedSteps?.Count ?? 0;
        string details = errors is { Count: > 0 }
            ? " " + String.Join(" | ", errors.Select(error => $"[{error.StepName}] {error.Message}"))
            : String.Empty;
        return $"Nạp static catalog có {errorCount} lỗi và {skippedCount} bước bị bỏ qua.{details}";
    }
}

// Lý do bỏ qua được tách riêng để Host có thể log an toàn, không in chuỗi kết nối từ ngoại lệ gốc.
public sealed record CatalogLoadSkippedStep(string StepName, string FailedDependencyStepName)
{
    public string Message => $"Bước '{StepName}' bị bỏ qua vì phụ thuộc '{FailedDependencyStepName}' lỗi.";
}
