using Nro.Data.Db;
using Nro.Data.Models.Gift;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Nro.Tests.Integration")]

namespace Nro.Data.Repositories.Gift;

// Lưu hai cờ quà tân thủ một lần của người chơi trong bảng gift.
// Nguồn Java: jbcd/dao/GiftDAO.java (truy vấn có tham số; tải hoặc tạo trong giao dịch và lỗi cơ sở dữ liệu được truyền lên thay vì bị nuốt).
public sealed class GiftRepository : IGiftRepository
{
    private const string SelectSql = "select is_newbie_gold_claimed, is_newbie_gem_claimed from gift where player_id = ?";
    internal const string InsertSql = "insert into gift (player_id) values (?) on conflict (player_id) do nothing";
    private const string UpdateSql = "update gift set is_newbie_gold_claimed = ?, is_newbie_gem_claimed = ? where player_id = ?";
    private readonly NroDatabase _database;

    // Tạo kho quà tân thủ với truy cập cơ sở dữ liệu.
    public GiftRepository(NroDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    // Tải cờ quà hoặc tạo hàng mới với hai cờ false trong cùng giao dịch (Java: loadGift).
    public Task<GiftPlayerState> LoadOrCreateAsync(int playerId, CancellationToken cancellationToken = default) =>
        _database.InTransactionAsync(async transaction =>
        {
            using IResultSnapshot rows = await transaction.ExecuteQueryAsync(SelectSql, [playerId], cancellationToken);
            if (rows.Next())
            {
                return new GiftPlayerState(rows.GetBoolean("is_newbie_gold_claimed"), rows.GetBoolean("is_newbie_gem_claimed"));
            }

            await transaction.ExecuteUpdateAsync(InsertSql, [playerId], cancellationToken);
            return new GiftPlayerState(false, false);
        }, cancellationToken);

    // Lưu hai cờ quà và ném lỗi nếu không cập nhật đúng một hàng (Java: updateGift).
    public async Task SaveAsync(int playerId, GiftPlayerState gift, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(gift);
        int affected = await _database.ExecuteUpdateAsync(UpdateSql, [gift.GoldTanThu, gift.GemTanThu, playerId], cancellationToken);
        if (affected != 1)
        {
            throw new InvalidOperationException($"Expected to update one gift row, updated {affected}.");
        }
    }
}
