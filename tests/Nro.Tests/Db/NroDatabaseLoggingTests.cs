using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using Nro.Data;

namespace Nro.Tests.Db;

/// <summary>
/// A SQL error is logged (SQL, WITHOUT parameter values) and then rethrown unchanged; with <c>LogQuery</c> on, successful statements are logged too;
/// statement timeout, cancellation, disposal.
/// </summary>
public class NroDatabaseLoggingTests : IClassFixture<PeopleFixture>, IAsyncLifetime
{
    private const string Secret = "GIA-TRI-NHAY-CAM-9f3a";

    private readonly PeopleFixture _fixture;

    public NroDatabaseLoggingTests(PeopleFixture fixture)
    {
        _fixture = fixture;
    }

    public Task InitializeAsync() => _fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private NroDatabase Create(ListLogger logger, bool logQuery = false, Action<NpgsqlConnectionStringBuilder>? connection = null)
        => _fixture.CreateDatabase(o => o.LogQuery = logQuery, connection, logger);

    [Fact]
    public async Task SQL_error_is_logged_with_the_SQL_then_the_original_exception_is_rethrown()
    {
        var logger = new ListLogger();
        await using var db = Create(logger);

        var error = await Assert.ThrowsAsync<PostgresException>(
            () => db.ExecuteQueryAsync("select * from bang_khong_ton_tai where id = ?", Secret));

        Assert.Equal(PostgresErrorCodes.UndefinedTable, error.SqlState);
        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.StartsWith("An error occurred while executing the statement: select * from bang_khong_ton_tai where id = ?", entry.Message);
        Assert.Contains("42P01", entry.Message);
        Assert.DoesNotContain(Secret, logger.Everything());
    }

    [Fact]
    public async Task ExecuteUpdate_error_logs_the_SQL_as_rewritten_by_the_insert_quirk()
    {
        var logger = new ListLogger();
        await using var db = Create(logger);

        await Assert.ThrowsAsync<PostgresException>(
            () => db.ExecuteUpdateAsync("insert into bang_khong_ton_tai values()", 1, Secret));

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("insert into bang_khong_ton_tai values(?,?)", entry.Message);   // Java reassigns the query variable, so it also logs the rewritten text
        Assert.DoesNotContain("$1", entry.Message);
        Assert.DoesNotContain(Secret, logger.Everything());
    }

    [Fact]
    public async Task Unique_violation_does_not_leak_parameter_values_to_the_log_even_with_Include_Error_Detail()
    {
        await _fixture.RunAsync("insert into uniq(id, code) values (1, 'ma-co-san')");

        var logger = new ListLogger();
        await using var db = Create(logger, connection: c => c.IncludeErrorDetail = true);

        var error = await Assert.ThrowsAsync<PostgresException>(
            () => db.ExecuteUpdateAsync("insert into uniq(id, code) values (?, ?)", 2, "ma-co-san"));

        // proves this setting DOES make PostgreSQL copy the key value into the exception (so logging the whole exception would leak it)
        Assert.Contains("ma-co-san", error.Detail);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, error.SqlState);

        var text = logger.Everything();
        Assert.DoesNotContain("ma-co-san", text);
        Assert.Contains("23505", text);
        Assert.Contains("insert into uniq(id, code) values (?, ?)", text);
    }

    /// <summary>
    /// PostgreSQL copies the user's value VERBATIM into the primary message of a data error (class 22), e.g.
    /// <c>invalid input syntax for type integer: "..."</c>. Each case below forces a secret value into a column of the wrong type / too large and
    /// checks that the log does NOT contain it, while the exception thrown to the caller is still the original PostgreSQL error.
    /// </summary>
    [Theory]
    [InlineData("insert into people(id) values (?)", new object[] { "SECRETVAL123" }, "22P02")]                      // string into an integer column
    [InlineData("insert into people(id, title_not_exist) values (?, ?)", new object[] { 1, "SECRETVAL123" }, "42703")]   // an error outside class 22
    [InlineData("insert into people(id, age) values (?, ?)", new object[] { 1, "SECRETVAL123" }, "22P02")]          // smallint
    [InlineData("insert into people(id, age) values (?, ?)", new object[] { 1, "SECRETVAL99999123" }, "22P02")]
    [InlineData("insert into people(id, age) values (?, ?)", new object[] { 1, "4000123" }, "22003")]               // number too large for smallint
    [InlineData("insert into people(id, born) values (?, ?)", new object[] { 1, "SECRETVAL123" }, "22007")]         // timestamp
    [InlineData("insert into people(id, vip) values (?, ?)", new object[] { 1, "SECRETVAL123" }, "22P02")]          // boolean
    [InlineData("insert into people(id, amount) values (?, ?)", new object[] { 1, "98765432109876.55" }, "22003")]  // numeric(10,2) overflow
    [InlineData("insert into people(id, level) values (?, ?)", new object[] { 1, "99999999999999999999123" }, "22003")] // bigint overflow
    [InlineData("select ?::uuid", new object[] { "SECRETVAL123" }, "22P02")]
    [InlineData("select ?::jsonb", new object[] { "{SECRETVAL123" }, "22P02")]
    [InlineData("select ?::integer", new object[] { "SECRETVAL123" }, "22P02")]
    public async Task Data_error_does_not_leak_parameter_values_to_the_log_even_though_PostgreSQL_copies_values_into_the_message(string sql, object[] args, string sqlState)
    {
        var logger = new ListLogger();
        await using var db = Create(logger, logQuery: true);

        var error = await Assert.ThrowsAsync<PostgresException>(async () =>
        {
            if (sql.StartsWith("select", StringComparison.Ordinal))
            {
                await db.ExecuteQueryAsync(sql, args);
            }
            else
            {
                await db.ExecuteUpdateAsync(sql, args);
            }
        });

        Assert.Equal(sqlState, error.SqlState);
        var text = logger.Everything();
        Assert.Contains(sqlState, text);
        Assert.Contains(sql, text);
        foreach (var secret in args.OfType<string>())
        {
            Assert.DoesNotContain(secret, text);
        }
    }

    [Fact]
    public async Task Data_error_inside_a_transaction_does_not_leak_values_to_the_log_including_the_rollback_line()
    {
        var logger = new ListLogger();
        await using var db = Create(logger);

        await Assert.ThrowsAsync<PostgresException>(() => db.InTransactionAsync(async tx =>
        {
            await tx.ExecuteUpdateAsync("insert into people(id) values (?)", 1);
            await tx.ExecuteUpdateAsync("insert into people(id, born) values (?, ?)", 2, Secret);
        }));

        Assert.DoesNotContain(Secret, logger.Everything());
        Assert.Contains("22007", logger.Everything());
        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select count(*) from people"));
    }

    [Fact]
    public async Task Non_class_22_error_still_logs_the_primary_message_but_blanks_the_quoted_segments()
    {
        var logger = new ListLogger();
        await using var db = Create(logger);

        await Assert.ThrowsAsync<PostgresException>(() => db.ExecuteQueryAsync("select * from bang_khong_ton_tai where id = ?", Secret));

        // keeps the context for the log reader (the kind of error) without printing the name/value inside the quotes
        Assert.Contains("42P01: relation \"…\" does not exist", logger.Everything());
    }

    [Fact]
    public async Task Placeholder_count_mismatch_is_also_logged_and_opens_no_connection()
    {
        var logger = new ListLogger();
        await using var db = Create(logger, connection: c => c.Port = 1);   // a port nobody listens on: trying to open a connection would fail differently

        await Assert.ThrowsAsync<SqlPlaceholderMismatchException>(() => db.ExecuteQueryAsync("select ?, ?", 1));

        var entry = Assert.Single(logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Contains("select ?, ?", entry.Message);
        Assert.Contains(nameof(SqlPlaceholderMismatchException), entry.Message);
    }

    [Fact]
    public async Task LogQuery_off_logs_only_failed_statements_and_on_logs_successful_ones_too()
    {
        var quiet = new ListLogger();
        await using (var db = Create(quiet, logQuery: false))
        {
            await db.ExecuteQueryAsync("select 1 as one");
            await db.ExecuteUpdateAsync("insert into people(id) values (?)", 1);
        }

        Assert.Empty(quiet.Entries);

        var verbose = new ListLogger();
        await using (var db = Create(verbose, logQuery: true))
        {
            await db.ExecuteQueryAsync("select ? as one", Secret);
            await db.ExecuteUpdateAsync("insert into people(id, name) values()", 2, Secret);
            await db.InTransactionAsync(async tx => await tx.ExecuteUpdateAsync("update people set name = ? where id = ?", Secret, 2));
        }

        Assert.Equal(3, verbose.Entries.Count);
        Assert.All(verbose.Entries, e => Assert.Equal(LogLevel.Information, e.Level));
        Assert.Equal("Statement executed successfully: select ? as one", verbose.Entries[0].Message);
        Assert.Equal("Statement executed successfully: insert into people(id, name) values(?,?)", verbose.Entries[1].Message);
        Assert.Equal("Statement executed successfully: update people set name = ? where id = ?", verbose.Entries[2].Message);
        Assert.DoesNotContain(Secret, verbose.Everything());   // Java prints PreparedStatement.toString() with the values; this version does not
    }

    [Fact]
    public async Task Failed_statement_inside_a_transaction_is_logged_and_the_transaction_rolls_back()
    {
        var logger = new ListLogger();
        await using var db = Create(logger);

        await Assert.ThrowsAsync<PostgresException>(() => db.InTransactionAsync(async tx =>
        {
            await tx.ExecuteUpdateAsync("insert into people(id) values (?)", 1);
            await tx.ExecuteUpdateAsync("insert into bang_khong_ton_tai(id) values (?)", 2);
        }));

        var entry = Assert.Single(logger.Entries);
        Assert.Contains("insert into bang_khong_ton_tai(id) values (?)", entry.Message);
        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select count(*) from people"));
    }

    [Fact]
    public async Task Cancelled_statement_is_not_logged_as_a_SQL_error()
    {
        var logger = new ListLogger();
        await using var db = Create(logger);
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => db.ExecuteQueryAsync("select 1", source.Token));

        Assert.Empty(logger.Entries);
    }

    [Fact]
    public async Task Statement_timeout_comes_from_the_options()
    {
        var logger = new ListLogger();
        await using var db = _fixture.CreateDatabase(o => o.CommandTimeoutSeconds = 1, logger: logger);

        var stopwatch = Stopwatch.StartNew();
        await Assert.ThrowsAnyAsync<Exception>(() => db.ExecuteQueryAsync("select pg_sleep(?)", 6));
        stopwatch.Stop();

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5), $"A 6-second statement must be cut off after ~1 second, but it ran for {stopwatch.Elapsed}");
        Assert.Contains(logger.Entries, e => e.Level == LogLevel.Error && e.Message.Contains("select pg_sleep(?)", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Disposing_repeatedly_is_fine_and_using_it_afterwards_throws_ObjectDisposedException()
    {
        var db = _fixture.CreateDatabase();

        await db.DisposeAsync();
        await db.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => db.ExecuteQueryAsync("select 1"));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => db.ExecuteUpdateAsync("select 1"));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => db.InTransactionAsync(_ => Task.CompletedTask));
        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await db.OpenConnectionAsync());
    }
}
