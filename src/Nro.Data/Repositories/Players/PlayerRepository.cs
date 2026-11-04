using Nro.Data.Db;

namespace Nro.Data.Repositories.Players;

// Lưu và tải nguyên vẹn hàng người chơi, đồng thời tạo người chơi cùng số dư tài khoản ban đầu.
// Nguồn Java: jbcd/dao/PlayerDAO.java (dùng truy vấn tham số; tên so qua citext; văn bản JSON không tuần tự hóa lại; tạo người chơi và cập nhật tài khoản nằm trong một giao dịch).
public sealed class PlayerRepository : IPlayerRepository
{
    private const string SelectByIdSql = "select * from player where id = ?";
    // Giữ tham số chuỗi không ép kiểu để PostgreSQL chọn toán tử bằng của citext.
    private const string SelectByNameSql = "select * from player where name = ?";
    private const string SelectByAccountSql = "select * from player where account_id = ?";
    private const string UpdateAccountSql = "update account set vnd = ?, total_top_up = ?, coin = ? where id = ?";
    private static readonly string[] WritableColumns = PlayerPersistenceColumns.All.Where(static column => column != "id").ToArray();
    private static readonly string InsertSql = BuildInsertSql();
    private readonly NroDatabase _database;

    // Tạo kho người chơi với truy cập cơ sở dữ liệu.
    public PlayerRepository(NroDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
    }

    // Tìm người chơi theo id; trả null khi không có (Java: loadPlayer).
    public Task<PlayerPersistenceRecord?> FindByIdAsync(int playerId, CancellationToken cancellationToken = default) => FindOneAsync(SelectByIdSql, [playerId], cancellationToken);

    // Tìm người chơi theo tên không phân biệt hoa thường nhờ citext; trả null khi không có (Java: loadPlayer).
    public Task<PlayerPersistenceRecord?> FindByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        return FindOneAsync(SelectByNameSql, [name], cancellationToken);
    }

    // Tìm người chơi theo id tài khoản; trả null khi không có (Java: loadPlayer).
    public Task<PlayerPersistenceRecord?> FindByAccountIdAsync(int accountId, CancellationToken cancellationToken = default) => FindOneAsync(SelectByAccountSql, [accountId], cancellationToken);

    // Cập nhật số dư tài khoản rồi tạo người chơi trong cùng giao dịch; hai id tài khoản phải trùng nhau (Java: createPlayer).
    public Task<PlayerPersistenceRecord> CreateAsync(PlayerPersistenceRecord player, PlayerAccountBalance accountBalance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(accountBalance);
        if (!Equals(player["account_id"], accountBalance.AccountId))
        {
            throw new ArgumentException("The player and account balance must have the same account id.", nameof(accountBalance));
        }

        return _database.InTransactionAsync(async transaction =>
        {
            int accountRowsAffected = await transaction.ExecuteUpdateAsync(
                UpdateAccountSql,
                [accountBalance.Vnd, accountBalance.TotalTopUp, accountBalance.Coin, accountBalance.AccountId],
                cancellationToken);
            if (accountRowsAffected != 1)
            {
                throw new InvalidOperationException($"Expected to update exactly one account row before creating a player; updated {accountRowsAffected}.");
            }

            using IResultSnapshot rows = await transaction.ExecuteQueryAsync(InsertSql, Values(player, WritableColumns, includeId: false), cancellationToken);
            if (!rows.Next())
            {
                throw new InvalidOperationException("The player INSERT did not return a row.");
            }

            return Read(rows);
        }, cancellationToken);
    }

    // Lưu duy nhất các cột đã đổi của người chơi; không đổi gì khi không có cột thay đổi (Java: updatePlayer).
    public Task SaveAsync(PlayerPersistenceRecord player, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(player);
        string? updateSql = BuildUpdateSql(player.ChangedColumns);
        if (updateSql is null)
        {
            return Task.CompletedTask;
        }

        return _database.InTransactionAsync(async transaction =>
        {
            int affected = await transaction.ExecuteUpdateAsync(updateSql, Values(player, player.ChangedColumns, includeId: true), cancellationToken);
            if (affected != 1)
            {
                throw new InvalidOperationException($"Expected to update one player row, updated {affected}.");
            }
        }, cancellationToken);
    }

    // Lập UPDATE chỉ từ cột người chơi đã cho phép, giữ thứ tự schema để vị trí tham số ổn định và không cho dữ liệu đầu vào thành định danh SQL.
    public static string? BuildUpdateSql(IEnumerable<string> changedColumns)
    {
        ArgumentNullException.ThrowIfNull(changedColumns);
        var requested = new HashSet<string>(changedColumns, StringComparer.Ordinal);
        requested.Remove("id");
        if (!requested.IsSubsetOf(WritableColumns))
        {
            throw new ArgumentOutOfRangeException(nameof(changedColumns), "A changed player column is not writable.");
        }

        string[] columns = WritableColumns.Where(requested.Contains).ToArray();
        return columns.Length == 0 ? null : "update player set " + string.Join(',', columns.Select(static column => column + "=?")) + " where id=?"; // lint-ok: L08 chỉ dùng danh sách cột cố định đã cho phép
    }

    private static string BuildInsertSql()
    {
        // Hai danh sách cùng lấy từ whitelist cố định nên số thứ tự tham số không thể lệch nhau.
        return "insert into player (" + string.Join(',', WritableColumns) + ") values (" + string.Join(',', Enumerable.Repeat("?", WritableColumns.Length)) + ") returning *"; // lint-ok: L08 chỉ dùng danh sách cột cố định đã cho phép
    }

    private async Task<PlayerPersistenceRecord?> FindOneAsync(string sql, object?[] arguments, CancellationToken cancellationToken)
    {
        using IResultSnapshot rows = await _database.ExecuteQueryAsync(sql, arguments, cancellationToken);
        return rows.Next() ? Read(rows) : null;
    }

    private static object?[] Values(PlayerPersistenceRecord player, IEnumerable<string> columns, bool includeId)
    {
        IEnumerable<string> selected = columns.OrderBy(static column => Array.IndexOf(WritableColumns, column));
        if (includeId)
        {
            selected = selected.Append("id");
        }

        return selected.Select(column => player[column]).ToArray();
    }

    private static PlayerPersistenceRecord Read(IResultSnapshot row)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (string column in PlayerPersistenceColumns.All)
        {
            object? value = row.GetObject(column);
            if (column is "data_inventory" or "data_location" or "data_point" or "data_magic_tree" or "items_body" or "items_bag" or "items_box" or "items_box_collection" or "items_box_lucky_round" or "sold_items" or "item_mails_box" or "skills" or "pet")
            {
                PlayerJsonCodec.ReadArray(value as string, column);
            }

            values.Add(column, value);
        }

        return PlayerPersistenceRecord.FromDatabase(values);
    }
}
