using Nro.Data.Db;
using Npgsql;

namespace Nro.Data.Db;

// Biểu diễn giao dịch đang mở, buộc các lệnh dùng cùng kết nối để cùng xác nhận hoặc hoàn tác.
// Nguồn Java: jbcd/ConnectDB.java (bổ sung đối tượng giao dịch .NET; chỉ dùng trong hàm InTransactionAsync, chạy tuần tự và không tự đóng kết nối).
public sealed class NroTransaction : INroSqlExecutor
{
    private readonly NroDatabase _database;
    private bool _ended;

    internal NroTransaction(NroDatabase database, NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _database = database;
        Connection = connection;
        Transaction = transaction;
    }

    // Kết nối của giao dịch, dùng cho thao tác thô như COPY; không được đóng hoặc hủy.
    public NpgsqlConnection Connection { get; }

    // Giao dịch Npgsql nền; InTransactionAsync tự xác nhận hoặc hoàn tác nên không thao tác trực tiếp.
    public NpgsqlTransaction Transaction { get; }

    internal void End() => _ended = true;

    // Chạy truy vấn bằng kết nối và giao dịch hiện tại.
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, params object?[] args)
        => ExecuteQueryAsync(sql, args, CancellationToken.None);

    // Chạy truy vấn với mã hủy bằng kết nối và giao dịch hiện tại.
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, object?[] args, CancellationToken cancellationToken)
    {
        ThrowIfEnded();
        return _database.QueryCoreAsync(Connection, Transaction, sql, args, cancellationToken);
    }

    // Chạy truy vấn không tham số với mã hủy bằng kết nối và giao dịch hiện tại.
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, CancellationToken cancellationToken)
        => ExecuteQueryAsync(sql, [], cancellationToken);

    // Chạy lệnh cập nhật bằng kết nối và giao dịch hiện tại.
    public Task<int> ExecuteUpdateAsync(string sql, params object?[] args)
        => ExecuteUpdateAsync(sql, args, CancellationToken.None);

    // Chạy lệnh cập nhật với mã hủy bằng kết nối và giao dịch hiện tại.
    public Task<int> ExecuteUpdateAsync(string sql, object?[] args, CancellationToken cancellationToken)
    {
        ThrowIfEnded();
        return _database.UpdateCoreAsync(Connection, Transaction, sql, args, cancellationToken);
    }

    // Chạy lệnh cập nhật không tham số với mã hủy bằng kết nối và giao dịch hiện tại.
    public Task<int> ExecuteUpdateAsync(string sql, CancellationToken cancellationToken)
        => ExecuteUpdateAsync(sql, [], cancellationToken);

    private void ThrowIfEnded()
    {
        if (_ended)
        {
            throw new InvalidOperationException("The transaction has ended; do not use the NroTransaction object outside the InTransactionAsync function.");
        }
    }
}
