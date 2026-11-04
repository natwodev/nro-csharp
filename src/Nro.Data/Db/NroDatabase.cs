using Nro.Data.Db;
using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace Nro.Data.Db;

// Cung cấp truy cập PostgreSQL bất đồng bộ qua Npgsql, thực thi SQL tham số và quản lý kết nối, giao dịch.
// Nguồn Java: jbcd/ConnectDB.java (không static, dùng NpgsqlDataSource thay MySQL/HikariCP; luôn đóng kết nối mượn từ pool và không log giá trị tham số).
public sealed partial class NroDatabase : INroSqlExecutor, IAsyncDisposable
{
    private static readonly Action<ILogger, string, Exception?> SqlSucceeded = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(1, nameof(SqlSucceeded)),
        "Statement executed successfully: {Sql}");

    private static readonly Action<ILogger, string, string, Exception?> SqlFailed = LoggerMessage.Define<string, string>(
        LogLevel.Error,
        new EventId(2, nameof(SqlFailed)),
        "An error occurred while executing the statement: {Sql} [{Reason}]");

    private static readonly Action<ILogger, string, Exception?> RollbackFailed = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(3, nameof(RollbackFailed)),
        "Could not roll back the transaction after a failure [{Reason}]");

    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger _logger;
    private readonly bool _logQuery;
    private readonly int _commandTimeoutSeconds;
    private readonly TableNameResolver _tableNames = new();
    private int _disposed;

    // Tạo truy cập cơ sở dữ liệu từ cấu hình và bộ ghi nhật ký tùy chọn.
    public NroDatabase(NroDatabaseOptions options, ILogger<NroDatabase>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new ArgumentException("The database connection string (ConnectionString) is missing.", nameof(options));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(options.CommandTimeoutSeconds);

        _logger = logger ?? NullLogger<NroDatabase>.Instance;
        _logQuery = options.LogQuery;
        _commandTimeoutSeconds = options.CommandTimeoutSeconds;
        _dataSource = new NpgsqlDataSourceBuilder(options.ConnectionString).Build();
    }

    // ------------------------------------------------------------------ INroSqlExecutor

    // Chạy truy vấn, tự mượn rồi trả kết nối pool.
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, params object?[] args)
        => QueryCoreAsync(null, null, sql, args, CancellationToken.None);

    // Chạy truy vấn với mã hủy, tự mượn rồi trả kết nối pool.
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, object?[] args, CancellationToken cancellationToken)
        => QueryCoreAsync(null, null, sql, args, cancellationToken);

    // Chạy truy vấn không tham số với mã hủy, tự mượn rồi trả kết nối pool.
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, CancellationToken cancellationToken)
        => QueryCoreAsync(null, null, sql, [], cancellationToken);

    // Chạy lệnh cập nhật, tự mượn rồi trả kết nối pool.
    public Task<int> ExecuteUpdateAsync(string sql, params object?[] args)
        => UpdateCoreAsync(null, null, sql, args, CancellationToken.None);

    // Chạy lệnh cập nhật với mã hủy, tự mượn rồi trả kết nối pool.
    public Task<int> ExecuteUpdateAsync(string sql, object?[] args, CancellationToken cancellationToken)
        => UpdateCoreAsync(null, null, sql, args, cancellationToken);

    // Chạy lệnh cập nhật không tham số với mã hủy, tự mượn rồi trả kết nối pool.
    public Task<int> ExecuteUpdateAsync(string sql, CancellationToken cancellationToken)
        => UpdateCoreAsync(null, null, sql, [], cancellationToken);

    // ------------------------------------------------------------------ Kết nối thô và giao dịch

    // Mở kết nối từ pool; bên gọi phải hủy kết nối để trả nó về pool (Java: getConnection).
    public ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _dataSource.OpenConnectionAsync(cancellationToken);
    }

    // Chạy công việc trong một giao dịch; xác nhận khi thành công, hoàn tác và ném lại đúng ngoại lệ khi lỗi hoặc hủy.
    public async Task<T> InTransactionAsync<T>(
        Func<NroTransaction, Task<T>> work,
        CancellationToken cancellationToken = default,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(work);

        await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(isolationLevel, cancellationToken);
        var scope = new NroTransaction(this, connection, transaction);
        T result;
        try
        {
            result = await work(scope);
        }
        catch (Exception ex)
        {
            scope.End();
            await RollbackQuietlyAsync(transaction, ex);
            throw;
        }

        scope.End();
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    // Chạy công việc không trả kết quả trong một giao dịch.
    public Task InTransactionAsync(
        Func<NroTransaction, Task> work,
        CancellationToken cancellationToken = default,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        ArgumentNullException.ThrowIfNull(work);
        return InTransactionAsync<bool>(
            async transaction =>
            {
                await work(transaction);
                return true;
            },
            cancellationToken,
            isolationLevel);
    }

    // Hoàn tác giao dịch; nếu hoàn tác lỗi thì chỉ ghi nhật ký để không che ngoại lệ gốc.
    private async Task RollbackQuietlyAsync(NpgsqlTransaction transaction, Exception original)
    {
        try
        {
            await transaction.RollbackAsync(CancellationToken.None);
        }
        catch (Exception rollbackError)
        {
            RollbackFailed(_logger, $"{DescribeFailure(original)}; rollback: {DescribeFailure(rollbackError)}", null);
        }
    }

    // ------------------------------------------------------------------ Hủy tài nguyên

    // Đóng pool và các kết nối rỗi; gọi lại an toàn, dùng đối tượng sau đó sẽ ném ObjectDisposedException.
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            await _dataSource.DisposeAsync();
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

    // ------------------------------------------------------------------ Lõi thực thi lệnh dùng chung với NroTransaction

    // Chạy truy vấn và chụp toàn bộ kết quả; kết nối null nghĩa là tự mượn rồi trả kết nối pool (Java: executeQuery).
    internal async Task<IResultSnapshot> QueryCoreAsync(
        NpgsqlConnection? connection,
        NpgsqlTransaction? transaction,
        string sql,
        object?[] args,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(args);

        try
        {
            var translated = SqlPlaceholders.Translate(sql, args.Length);
            var parameters = DbArguments.ToParameters(args);

            var owned = connection is null;
            var conn = connection ?? await _dataSource.OpenConnectionAsync(cancellationToken);
            try
            {
                ResultSnapshot snapshot;
                await using (var command = CreateCommand(conn, transaction, translated, parameters))
                {
                    snapshot = await ReadSnapshotAsync(conn, transaction, command, cancellationToken);
                }

                if (_logQuery)
                {
                    SqlSucceeded(_logger, sql, null);
                }

                return snapshot;
            }
            finally
            {
                if (owned)
                {
                    await conn.DisposeAsync();
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SqlFailed(_logger, sql, DescribeFailure(ex), null);
            throw;
        }
    }

    // Áp dụng quy tắc insert ... (), chạy lệnh và trả số dòng tác động; DDL -1 của Npgsql được chuẩn hóa thành 0 (Java: executeUpdate).
    internal async Task<int> UpdateCoreAsync(
        NpgsqlConnection? connection,
        NpgsqlTransaction? transaction,
        string sql,
        object?[] args,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(sql);
        ArgumentNullException.ThrowIfNull(args);

        // Java gán lại biến query nên nhật ký lỗi ghi SQL đã viết lại; nơi đây giữ cùng hành vi.
        var effective = SqlPlaceholders.ExpandInsertValues(sql, args.Length);
        try
        {
            var translated = SqlPlaceholders.Translate(effective, args.Length);
            var parameters = DbArguments.ToParameters(args);

            var owned = connection is null;
            var conn = connection ?? await _dataSource.OpenConnectionAsync(cancellationToken);
            try
            {
                int affected;
                await using (var command = CreateCommand(conn, transaction, translated, parameters))
                {
                    affected = await command.ExecuteNonQueryAsync(cancellationToken);
                }

                if (_logQuery)
                {
                    SqlSucceeded(_logger, effective, null);
                }

                return Math.Max(affected, 0);
            }
            finally
            {
                if (owned)
                {
                    await conn.DisposeAsync();
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            SqlFailed(_logger, effective, DescribeFailure(ex), null);
            throw;
        }
    }

    private NpgsqlCommand CreateCommand(NpgsqlConnection connection, NpgsqlTransaction? transaction, string sql, NpgsqlParameter[] parameters)
    {
        var command = new NpgsqlCommand(sql, connection, transaction) { CommandTimeout = _commandTimeoutSeconds };
        command.Parameters.AddRange(parameters);
        return command;
    }

    // Đọc toàn bộ kết quả vào bộ nhớ rồi tra tên bảng sau khi đóng reader, trên chính kết nối đó (Java: hàm tạo ConnectResultSet).
    private async Task<ResultSnapshot> ReadSnapshotAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        NpgsqlCommand command,
        CancellationToken cancellationToken)
    {
        string[] names;
        uint[] tableOids;
        List<object?[]> rows;

        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            var fieldCount = reader.FieldCount;
            if (fieldCount == 0)
            {
                // JDBC cũng ném lỗi khi lệnh không trả tập kết quả; Npgsql chỉ biết sau khi chạy nên tác dụng phụ vẫn xảy ra.
                throw new InvalidOperationException(
                    "The statement returned no result set; use ExecuteUpdateAsync for INSERT/UPDATE/DELETE (the statement was executed).");
            }

            var schema = reader.GetColumnSchema();
            names = new string[fieldCount];
            tableOids = new uint[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                names[i] = reader.GetName(i);
                tableOids[i] = schema[i].TableOID;
            }

            rows = [];
            var buffer = new object[fieldCount];
            while (await reader.ReadAsync(cancellationToken))
            {
                reader.GetValues(buffer);
                var row = new object?[fieldCount];
                for (var i = 0; i < fieldCount; i++)
                {
                    row[i] = DbArguments.FromDriverValue(buffer[i]);
                }

                rows.Add(row);
            }
        }

        // Kết quả rỗng không cần tra khóa nên bỏ lượt đi-về bổ sung.
        var tables = rows.Count == 0
            ? new string?[tableOids.Length]
            : await _tableNames.ResolveAsync(connection, transaction, tableOids, cancellationToken);
        var columns = new SnapshotColumn[names.Length];
        for (var i = 0; i < columns.Length; i++)
        {
            columns[i] = new SnapshotColumn(names[i], tables[i]);
        }

        return ResultSnapshot.Adopt(columns, [.. rows]);
    }

    // Mô tả lỗi để ghi nhật ký nhưng không lộ giá trị tham số; lỗi PostgreSQL lớp 22 chỉ ghi SQLSTATE và tên điều kiện.
    private static string DescribeFailure(Exception exception) => exception switch
    {
        PostgresException postgres => DescribePostgres(postgres),
        _ => $"{exception.GetType().Name}: {exception.Message}",
    };

    private static string DescribePostgres(PostgresException postgres)
    {
        var sqlState = postgres.SqlState;
        if (sqlState.StartsWith("22", StringComparison.Ordinal))
        {
            return $"{nameof(PostgresException)} {sqlState}: {DataExceptionName(sqlState)}";
        }

        return $"{nameof(PostgresException)} {sqlState}: {QuotedSegments().Replace(postgres.MessageText, "$1…$1")}";
    }

    private static string DataExceptionName(string sqlState) => sqlState switch
    {
        PostgresErrorCodes.InvalidTextRepresentation => "invalid text representation",
        PostgresErrorCodes.InvalidDatetimeFormat => "invalid datetime format",
        PostgresErrorCodes.NumericValueOutOfRange => "numeric value out of range",
        PostgresErrorCodes.StringDataRightTruncation => "string data right truncation",
        PostgresErrorCodes.DatetimeFieldOverflow => "datetime field overflow",
        PostgresErrorCodes.DivisionByZero => "division by zero",
        _ => "data exception",
    };

    // Nhận diện đoạn trong dấu nháy có thể là giá trị tham số để che khỏi nhật ký.
    [GeneratedRegex(@"(""|')(?:(?!\1).|\1\1)*\1", RegexOptions.Singleline)]
    private static partial Regex QuotedSegments();
}
