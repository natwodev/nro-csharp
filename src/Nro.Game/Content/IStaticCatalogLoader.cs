namespace Nro.Game.Content;

// Hợp đồng nạp và công bố duy nhất một snapshot danh mục tĩnh. Nguồn Java: nro/server/Manager.java (dòng 192–218).
public interface IStaticCatalogLoader
{
    // Snapshot hiện đang được công bố, không bao giờ bị sửa bởi lần nạp sau. (Java: Manager)
    IStaticCatalogs Current { get; }

    // Nạp toàn bộ danh mục rồi trả snapshot vừa công bố, hoặc ném lỗi mà không đổi snapshot cũ. (Java: loadDatabase)
    Task<IStaticCatalogs> LoadAsync(CancellationToken cancellationToken = default);
}
