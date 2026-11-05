namespace Nro.Content.Loaders;

// Kết quả số lượng của một bước nạp danh mục. Nguồn Java: nro/server/Manager.java (dòng 1145–1199).
public sealed class CatalogStepResult
{
    // Khởi tạo kết quả với số bản ghi thực tế đã nạp. (Java: Logger.success)
    public CatalogStepResult(int recordCount, IEnumerable<string>? warnings = null)
    {
        if (recordCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(recordCount));
        }

        RecordCount = recordCount;
        Warnings = (warnings ?? Array.Empty<string>()).ToArray();
    }

    // Số bản ghi thực tế mà bước đã nạp. (Java: size)
    public int RecordCount { get; }

    // Cảnh báo có cấu trúc dạng "step-name: chi tiết" phát sinh trong bước nạp. (Java: Logger.warning)
    public IReadOnlyList<string> Warnings { get; }
}
