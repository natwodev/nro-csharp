using Nro.Data.Db;
namespace Nro.Data.Repositories.Gift;

// Cung cấp hợp đồng truy xuất và ghi nhận sử dụng giftcode cho kho đồ người chơi.
// Nguồn Java: InventoryService.addItemGiftCodeToPlayer (mọi giá trị được tham số hoá; code dùng citext không phân biệt hoa thường; PostgreSQL chọn id nhỏ nhất thay UPDATE ... LIMIT 1 và rs.first không xác định của Java).
public interface IGiftCodeRepository
{
    // Lấy lượt dùng còn lại; không có mã thì trả null (Java: addItemGiftCodeToPlayer).
    Task<int?> GetCountLeftAsync(string code, CancellationToken cancellationToken = default);

    // Đặt lượt dùng còn lại và trả số dòng đổi, 0 hoặc 1 (Java: addItemGiftCodeToPlayer).
    Task<int> SetCountLeftAsync(string code, int countLeft, CancellationToken cancellationToken = default);

    // Lưu lần dùng mã với thời điểm giờ Việt Nam nguyên trạng, không đổi múi giờ (Java: addItemGiftCodeToPlayer).
    Task SaveUseAsync(int userId, long playerId, string code, DateTime whenVietnamTime, CancellationToken cancellationToken = default);
}
