namespace Nro.Content.Loaders;

// Lỗi nạp danh mục rõ nghĩa để Host quyết định cách dừng hay ghi log. Nguồn Java: nro/server/Manager.java (dòng 1145–1199).
public sealed class CatalogLoadException : Exception
{
    // Khởi tạo lỗi gắn với bước đã thất bại hoặc phần xác thực. (Java: loadDatabase)
    public CatalogLoadException(string stepName, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        StepName = stepName;
    }

    // Tên bước gây lỗi; dùng validation khi lỗi xảy ra trước lúc chạy bước. (Java: loadDatabase)
    public string StepName { get; }
}
