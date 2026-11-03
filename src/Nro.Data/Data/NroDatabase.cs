using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Nro.Data.Interfaces;

namespace Nro.Data;

/// <summary>
/// PostgreSQL access through Npgsql, replacing the Java <c>jbcd.ConnectDB</c>: NOT static (use it with DI), wraps one
/// <see cref="NpgsqlDataSource"/> (which already pools connections), and has no string-concatenated SQL (every value goes through a parameter).
/// </summary>
/// <remarks>
/// <para>Each <see cref="ExecuteQueryAsync(string, object?[])"/>/<see cref="ExecuteUpdateAsync(string, object?[])"/> takes a connection
/// from the pool, runs, then returns the connection (like Java <c>try-with-resources</c>). Use <see cref="InTransactionAsync{T}"/> when several
/// statements must run in the same transaction.</para>
/// <para>SQL is written with positional <c>?</c> placeholders as in Java (see <see cref="SqlPlaceholders"/>). On failure, the SQL text (WITHOUT the parameter values)
/// is logged through <see cref="ILogger"/> and then the original exception is rethrown unchanged; when
/// <see cref="NroDatabaseOptions.LogQuery"/> is on, successful statements are logged too.</para>
/// <para><c>string</c> parameters are sent as "unknown" so PostgreSQL infers the type the way MySQL coerces it (case-insensitive <c>citext</c> comparison,
/// strings into numeric/boolean/jsonb/timestamp columns); see <c>DbArguments.ToParameters</c>. Be careful with
/// <see cref="DateTime"/>: Kind=Utc is sent as <c>timestamptz</c>, so when written to a <c>timestamp</c> column it is converted by the session time zone;
/// Kind=Local/Unspecified is sent as <c>timestamp</c> and keeps the wall-clock time.</para>
/// <para>Deliberate differences from the Java version: not static; asynchronous; parameter values are never written to the log (Java prints
/// <c>PreparedStatement.toString()</c> with the values when <c>LOG_QUERY</c> is on); the connection comes from configuration instead of a properties file.</para>
/// </remarks>
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

    /// <inheritdoc/>
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, params object?[] args)
        => QueryCoreAsync(null, null, sql, args, CancellationToken.None);

    /// <inheritdoc/>
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, object?[] args, CancellationToken cancellationToken)
        => QueryCoreAsync(null, null, sql, args, cancellationToken);

    /// <inheritdoc/>
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, CancellationToken cancellationToken)
        => QueryCoreAsync(null, null, sql, [], cancellationToken);

    /// <inheritdoc/>
    public Task<int> ExecuteUpdateAsync(string sql, params object?[] args)
        => UpdateCoreAsync(null, null, sql, args, CancellationToken.None);

    /// <inheritdoc/>
    public Task<int> ExecuteUpdateAsync(string sql, object?[] args, CancellationToken cancellationToken)
        => UpdateCoreAsync(null, null, sql, args, cancellationToken);

    /// <inheritdoc/>
    public Task<int> ExecuteUpdateAsync(string sql, CancellationToken cancellationToken)
        => UpdateCoreAsync(null, null, sql, [], cancellationToken);

    // ------------------------------------------------------------------ raw connection and transactions

    /// <summary>
    /// Gets an open connection from the pool (replaces <c>ConnectDB.getConnection()</c>). The caller MUST dispose it
    /// (<c>await using</c>) to return it to the pool; forgetting to dispose leaks the connection (a common bug in the Java <c>PlayerDAO</c>).
    /// </summary>
    public ValueTask<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return _dataSource.OpenConnectionAsync(cancellationToken);
    }

    /// <summary>
    /// Runs <paramref name="work"/> in a transaction: on success it COMMITs and returns the result; if an exception is thrown (including cancellation) it
    /// ROLLBACKs and rethrows that exact exception. Inside <paramref name="work"/> you must use the <see cref="NroTransaction"/> object
    /// passed in (not <see cref="NroDatabase"/> itself, because that would run on another connection, outside the transaction).
    /// </summary>
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

    /// <inheritdoc cref="InTransactionAsync{T}"/>
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

    /// <summary>Rolls the transaction back; if the rollback itself fails it is only logged, so the original exception is not masked.</summary>
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

    // ------------------------------------------------------------------ disposal

    /// <summary>Closes the pool and every idle connection. Calling it repeatedly is fine; using the object afterwards throws <see cref="ObjectDisposedException"/>.</summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            await _dataSource.DisposeAsync();
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);

    // ------------------------------------------------------------------ statement execution core (shared with NroTransaction)

    /// <summary>
    /// Java <c>executeQuery(String, Object...)</c>: runs the query, then captures the whole result into a <see cref="ResultSnapshot"/>.
    /// A null <paramref name="connection"/> means: take a connection from the pool and return it afterwards.
    /// </summary>
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

    /// <summary>
    /// Java <c>executeUpdate(String, Object...)</c>: applies the <c>insert ... ()</c> quirk, runs the statement, returns the number of affected rows
    /// (Npgsql returns -1 for DDL, JDBC returns 0: normalised to 0).
    /// </summary>
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

        // Java reassigns the `query` variable, so the error log records the REWRITTEN SQL; the same happens here.
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

    /// <summary>
    /// Reads the whole result of <paramref name="command"/> into memory (Java: the <c>ConnectResultSet</c> constructor).
    /// A column's table name is looked up only AFTER the reader is closed, on the same connection (see <see cref="TableNameResolver"/>).
    /// </summary>
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
                // JDBC executeQuery also throws for a statement that returns no result set. Unlike JDBC, Npgsql only knows this after
                // running the statement, so any side effects still happen; use ExecuteUpdateAsync for INSERT/UPDATE/DELETE.
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

        // An empty result has no row to look keys up for, so skip the extra round trip (a wrong login is a very common case).
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

    /// <summary>
    /// Describes a failure for the log WITHOUT exposing parameter values. PostgreSQL copies user-supplied values into the primary message
    /// (<c>MessageText</c>) for many kinds of errors, e.g. 22P02 <c>invalid input syntax for type integer: "value"</c>, 22003
    /// <c>value "value" is out of range</c>, 22007 <c>invalid input syntax for type timestamp: "value"</c>; not to mention <c>Detail</c>
    /// (the violating key value). So for errors from PostgreSQL: class 22 (data exception, where values often appear) logs only the SQLSTATE code and condition name, NOT
    /// the message; other classes log the code and the primary message with every segment inside "..." or '...' replaced by an ellipsis.
    /// </summary>
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

    /// <summary>A segment inside <c>"..."</c> or <c>'...'</c> (including doubled quotes used as escapes): it may be a parameter value.</summary>
    [GeneratedRegex(@"(""|')(?:(?!\1).|\1\1)*\1", RegexOptions.Singleline)]
    private static partial Regex QuotedSegments();
}
