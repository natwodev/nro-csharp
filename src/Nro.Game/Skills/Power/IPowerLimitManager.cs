using Nro.Data.Db;

namespace Nro.Game.Skills.Power;

// Hợp đồng các mốc giới hạn sức mạnh.
// Nguồn Java: nro/power/PowerLimitManager.java (lớp chính thay PowerLimitManager singleton bằng dependency injection).
public interface IPowerLimitManager
{
    // Snapshot các dòng theo thứ tự nạp (Java: getPowers).
    IReadOnlyList<PowerLimit> Powers { get; }

    // Nạp nối thêm power_limit theo id; lỗi DB được ném thay vì in và không nối gì khi lỗi (Java: load).
    Task LoadAsync(INroSqlExecutor database, CancellationToken cancellationToken = default);

    // Thêm mốc giới hạn (Java: add).
    void Add(PowerLimit powerLimit);

    // Xóa mốc trùng tham chiếu đầu tiên và trả trạng thái xóa (Java: remove).
    bool Remove(PowerLimit powerLimit);

    // Lấy dòng theo vị trí chứ không theo id; chỉ số âm/ngoài phạm vi trả null (Java: get).
    PowerLimit? Get(int index);
}
