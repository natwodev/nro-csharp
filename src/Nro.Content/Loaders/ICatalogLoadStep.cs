using Nro.Game.Content;

namespace Nro.Content.Loaders;

// Một bước nạp dữ liệu tĩnh vào bộ dựng cục bộ. Nguồn Java: nro/server/Manager.java (dòng 192–218).
public interface ICatalogLoadStep
{
    // Tên duy nhất của bước để sắp xếp và báo lỗi. (Java: loadDatabase)
    string Name { get; }

    // Các tên bước phải hoàn tất trước bước này. (Java: loadDatabase)
    IReadOnlyCollection<string> Dependencies { get; }

    // Nạp dữ liệu vào builder cục bộ và trả số bản ghi đã đọc. (Java: loadDatabase)
    Task<CatalogStepResult> LoadAsync(StaticCatalogBuilder builder, CancellationToken cancellationToken);
}
