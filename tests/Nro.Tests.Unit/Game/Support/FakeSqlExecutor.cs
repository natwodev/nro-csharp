using Nro.Data.Db;

namespace Nro.Tests.Unit.Game.Support;

/// <summary>
/// An <see cref="INroSqlExecutor"/> for tests that must not need a database: each query is answered from a table registered under a fragment of its text (the first registered
/// fragment that the SQL contains), every SQL text and every update is recorded. A query nobody registered fails the test, so a changed SQL cannot go unnoticed.
/// Cell types follow the database read path: <see cref="int"/> for smallint/integer, <see cref="long"/> for bigint, <see cref="string"/> for text.
/// </summary>
internal sealed class FakeSqlExecutor : INroSqlExecutor
{
    private readonly List<(string Fragment, string[] Columns, object?[][] Rows)> queries = [];

    /// <summary>Every query text received, in order.</summary>
    public List<string> QueryLog { get; } = [];

    /// <summary>Every update received, in order: the SQL and its arguments.</summary>
    public List<(string Sql, object?[] Args)> Updates { get; } = [];

    /// <summary>When set, the next query whose text contains it throws this exception (to test that a failing load changes nothing).</summary>
    public (string Fragment, Exception Error)? FailQueryContaining { get; set; }

    public FakeSqlExecutor Table(string fragment, string[] columns, params object?[][] rows)
    {
        queries.Add((fragment, columns, rows));
        return this;
    }

    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, params object?[] args) => Query(sql);

    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, object?[] args, CancellationToken cancellationToken) => Query(sql);

    public Task<IResultSnapshot> ExecuteQueryAsync(string sql, CancellationToken cancellationToken) => Query(sql);

    public Task<int> ExecuteUpdateAsync(string sql, params object?[] args) => Update(sql, args);

    public Task<int> ExecuteUpdateAsync(string sql, object?[] args, CancellationToken cancellationToken) => Update(sql, args);

    public Task<int> ExecuteUpdateAsync(string sql, CancellationToken cancellationToken) => Update(sql, []);

    private Task<IResultSnapshot> Query(string sql)
    {
        QueryLog.Add(sql);
        if (FailQueryContaining is { } failure && sql.Contains(failure.Fragment, StringComparison.Ordinal))
        {
            return Task.FromException<IResultSnapshot>(failure.Error);
        }

        foreach (var (fragment, columns, rows) in queries)
        {
            if (sql.Contains(fragment, StringComparison.Ordinal))
            {
                var snapshot = new ResultSnapshot(columns.Select(c => new SnapshotColumn(c)).ToArray(), rows);
                return Task.FromResult<IResultSnapshot>(snapshot);
            }
        }

        throw new InvalidOperationException("The test registered no table for this SQL: " + sql);
    }

    private Task<int> Update(string sql, object?[] args)
    {
        Updates.Add((sql, args));
        return Task.FromResult(1);
    }
}
