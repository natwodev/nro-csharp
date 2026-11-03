using Npgsql;
using Nro.Data.Interfaces;

namespace Nro.Data;

/// <summary>
/// An open transaction of <see cref="NroDatabase.InTransactionAsync{T}"/>. Every statement run through this object uses the SAME
/// connection and transaction, so they commit or roll back together. Use it only inside the function passed to <c>InTransactionAsync</c>;
/// keeping it and using it after the transaction ends throws <see cref="InvalidOperationException"/>.
/// A transaction has only ONE connection, so statements must run one after another (<c>await</c> one before calling the next);
/// running several statements in parallel on this object makes Npgsql report "command is already in progress".
/// </summary>
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

    /// <summary>The transaction's connection (for raw operations such as COPY). Must not be closed or disposed.</summary>
    public NpgsqlConnection Connection { get; }

    /// <summary>The underlying Npgsql transaction. Do not commit/rollback by hand: <c>InTransactionAsync</c> does that.</summary>
    public NpgsqlTransaction Transaction { get; }

    internal void End() => _ended = true;

    /// <inheritdoc/>
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, params object?[] args)
        => ExecuteQueryAsync(sql, args, CancellationToken.None);

    /// <inheritdoc/>
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, object?[] args, CancellationToken cancellationToken)
    {
        ThrowIfEnded();
        return _database.QueryCoreAsync(Connection, Transaction, sql, args, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, CancellationToken cancellationToken)
        => ExecuteQueryAsync(sql, [], cancellationToken);

    /// <inheritdoc/>
    public Task<int> ExecuteUpdateAsync(string sql, params object?[] args)
        => ExecuteUpdateAsync(sql, args, CancellationToken.None);

    /// <inheritdoc/>
    public Task<int> ExecuteUpdateAsync(string sql, object?[] args, CancellationToken cancellationToken)
    {
        ThrowIfEnded();
        return _database.UpdateCoreAsync(Connection, Transaction, sql, args, cancellationToken);
    }

    /// <inheritdoc/>
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
