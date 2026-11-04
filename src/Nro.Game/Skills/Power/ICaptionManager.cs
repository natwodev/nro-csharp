using Nro.Data.Db;

namespace Nro.Game.Skills.Power;

// Hợp đồng các danh hiệu theo sức mạnh.
// Nguồn Java: nro/power/CaptionManager.java (lớp chính thay CaptionManager singleton bằng dependency injection).
public interface ICaptionManager
{
    // Snapshot danh hiệu theo thứ tự nạp; Add/Remove sau đó không đổi snapshot đã trả (Java: getCaptions).
    IReadOnlyList<Caption> Captions { get; }

    // Nạp nối thêm caption theo id; lỗi DB được ném thay vì in/bỏ qua nên không có nạp một phần (Java: load).
    Task LoadAsync(INroSqlExecutor database, CancellationToken cancellationToken = default);

    // Thêm danh hiệu (Java: add).
    void Add(Caption caption);

    // Xóa danh hiệu trùng tham chiếu đầu tiên và trả trạng thái xóa (Java: remove).
    bool Remove(Caption caption);

    // Tìm danh hiệu đầu tiên theo id, không có trả null (Java: find).
    Caption? Find(int id);

    // Lấy danh hiệu tại vị trí zero-based, ngoài phạm vi trả null (Java: findLevel).
    Caption? FindLevel(int level);

    // Vị trí cuối khi quét ngược có ngưỡng không vượt power; danh sách rỗng/không khớp trả 0, kể cả khi dữ liệu không sắp sức mạnh (Java: getLevel).
    int GetLevel(long power);
}
