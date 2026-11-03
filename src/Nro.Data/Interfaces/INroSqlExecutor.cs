namespace Nro.Data.Interfaces;

/// <summary>
/// Runs <c>ConnectDB</c>-style SQL statements (positional <c>?</c> placeholders, results as <see cref="IResultSnapshot"/>).
/// <see cref="NroDatabase"/> runs each statement on its own connection taken from the pool; <see cref="NroTransaction"/> runs every statement
/// on the same connection and transaction.
/// </summary>
public interface INroSqlExecutor
{
    /// <summary>Runs a query and reads the whole result into memory. A statement that returns no result set (INSERT/UPDATE without RETURNING) throws <see cref="InvalidOperationException"/>, like JDBC <c>executeQuery</c>.</summary>
    Task<IResultSnapshot> ExecuteQueryAsync(string sql, params object?[] args);

    /// <inheritdoc cref="ExecuteQueryAsync(string, object?[])"/>
    Task<IResultSnapshot> ExecuteQueryAsync(string sql, object?[] args, CancellationToken cancellationToken);

    /// <inheritdoc cref="ExecuteQueryAsync(string, object?[])"/>
    Task<IResultSnapshot> ExecuteQueryAsync(string sql, CancellationToken cancellationToken);

    /// <summary>Runs INSERT/UPDATE/DELETE/DDL and returns the number of affected rows (0 for DDL, like JDBC). Preserves the Java <c>insert ... ()</c> quirk, see <see cref="SqlPlaceholders.ExpandInsertValues"/>.</summary>
    Task<int> ExecuteUpdateAsync(string sql, params object?[] args);

    /// <inheritdoc cref="ExecuteUpdateAsync(string, object?[])"/>
    Task<int> ExecuteUpdateAsync(string sql, object?[] args, CancellationToken cancellationToken);

    /// <inheritdoc cref="ExecuteUpdateAsync(string, object?[])"/>
    Task<int> ExecuteUpdateAsync(string sql, CancellationToken cancellationToken);
}
