using System.Collections.ObjectModel;

namespace Nro.Content.Loaders;

// Báo cáo bất biến về các bước đã nạp và số bản ghi của từng bước. Nguồn Java: nro/server/Manager.java (dòng 1145–1199).
public sealed class CatalogLoadReport
{
    // Khởi tạo báo cáo từ các mục đã hoàn tất theo thứ tự phụ thuộc. (Java: Logger.success)
    public CatalogLoadReport(IEnumerable<CatalogLoadReportEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);
        Entries = new ReadOnlyCollection<CatalogLoadReportEntry>(entries.ToArray());
    }

    // Các mục báo cáo theo thứ tự nạp. (Java: loadDatabase)
    public IReadOnlyList<CatalogLoadReportEntry> Entries { get; }

    // Báo cáo rỗng dùng trước lần nạp đầu tiên. (Java: Manager)
    public static CatalogLoadReport Empty { get; } = new(Array.Empty<CatalogLoadReportEntry>());
}
