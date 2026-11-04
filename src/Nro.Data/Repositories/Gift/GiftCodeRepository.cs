using Nro.Data.Db;

namespace Nro.Data.Repositories.Gift;

// Đọc, cập nhật mã quà và lưu lượt nhập mã trên các bảng gift_code, gift_code_redemption.
// Nguồn Java: nro/inventory/InventoryService.java (thay SQL nối chuỗi mã quà bằng tham số để tránh injection; chọn hàng id nhỏ nhất vì PostgreSQL không có UPDATE ... LIMIT 1).
public sealed class GiftCodeRepository : IGiftCodeRepository
{
    private const string SelectCountLeftSql = "select count_left from gift_code where code = ? order by id limit 1";
    private const string UpdateCountLeftSql = "update gift_code set count_left = ? where id = (select id from gift_code where code = ? order by id limit 1)";
    private const string InsertUseSql = "insert into gift_code_redemption (id, player_id, redeemed_code, redeemed_at) values (?, ?, ?, ?)";

    private readonly NroDatabase _database;

    // Tạo kho mã quà với truy cập cơ sở dữ liệu.
    public GiftCodeRepository(NroDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    // Trả số lượt còn lại của mã, hoặc null khi không có mã (Java: SELECT giftcode).
    public async Task<int?> GetCountLeftAsync(string code, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        using IResultSnapshot rows = await _database.ExecuteQueryAsync(SelectCountLeftSql, [code], cancellationToken);
        return rows.Next() ? rows.GetInt("count_left") : null;
    }

    // Đặt số lượt còn lại cho hàng id nhỏ nhất có mã; trả số hàng đã cập nhật (Java: UPDATE giftcode ... LIMIT 1).
    public Task<int> SetCountLeftAsync(string code, int countLeft, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        return _database.ExecuteUpdateAsync(UpdateCountLeftSql, [countLeft, code], cancellationToken);
    }

    // Lưu lượt nhập mã với thời điểm giờ Việt Nam giữ nguyên thời gian tường trong cột timestamp (Java: INSERT giftcode_save).
    public async Task SaveUseAsync(int userId, long playerId, string code, DateTime whenVietnamTime, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(code);
        var unspecified = DateTime.SpecifyKind(whenVietnamTime, DateTimeKind.Unspecified);
        await _database.ExecuteUpdateAsync(InsertUseSql, [userId, playerId, code, unspecified], cancellationToken);
    }
}
