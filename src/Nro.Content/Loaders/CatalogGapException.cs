namespace Nro.Content.Loaders;

// Nguồn Java: nro/server/Manager.java (dòng 742–855).
// Báo lỗi danh mục bị khuyết id dù Java dùng id làm chỉ số mảng. (Java: List.get, MAP_TEMPLATES[index])
public sealed class CatalogGapException : CatalogLoadException
{
    // Khởi tạo lỗi và đóng băng các id bị thiếu để log không phụ thuộc dữ liệu gọi. (Java: MAP_TEMPLATES)
    public CatalogGapException(string stepName, IEnumerable<int> missingIds)
        : this(stepName, missingIds?.ToArray() ?? throw new ArgumentNullException(nameof(missingIds)))
    {
    }

    // Khởi tạo từ snapshot id đã vật hóa một lần. (Java: MAP_TEMPLATES)
    private CatalogGapException(string stepName, int[] missingIds)
        : base(stepName, $"Danh mục {stepName} thiếu id liên tục bắt buộc: {string.Join(", ", missingIds)}.") => MissingIds = Array.AsReadOnly(missingIds);

    // Các id thiếu theo thứ tự tăng dần. (Java: MAP_TEMPLATES[index])
    public IReadOnlyList<int> MissingIds { get; }
}
