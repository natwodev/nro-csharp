namespace Nro.Content.Loaders;

// Một mục bất biến trong báo cáo nạp danh mục. Nguồn Java: nro/server/Manager.java (dòng 1145–1199).
public sealed class CatalogLoadReportEntry
{
    // Khởi tạo mục báo cáo với tên bước và số bản ghi. (Java: Logger.success)
    public CatalogLoadReportEntry(string stepName, int recordCount)
    {
        if (String.IsNullOrWhiteSpace(stepName))
        {
            throw new ArgumentException("Tên bước không được để trống.", nameof(stepName));
        }

        if (recordCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(recordCount));
        }

        StepName = stepName;
        RecordCount = recordCount;
    }

    // Tên bước đã hoàn tất. (Java: loadDatabase)
    public string StepName { get; }

    // Số bản ghi bước đã nạp. (Java: size)
    public int RecordCount { get; }
}
