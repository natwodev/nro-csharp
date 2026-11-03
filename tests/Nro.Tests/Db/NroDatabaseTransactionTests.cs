using System.Data;
using Npgsql;
using Nro.Data;

namespace Nro.Tests.Db;

/// <summary>Transactions of <see cref="NroDatabase.InTransactionAsync{T}"/>: commit on success, rollback and rethrow the exact exception on failure.</summary>
public class NroDatabaseTransactionTests : IClassFixture<PeopleFixture>, IAsyncLifetime
{
    private readonly PeopleFixture _fixture;

    public NroDatabaseTransactionTests(PeopleFixture fixture)
    {
        _fixture = fixture;
    }

    private NroDatabase Db => _fixture.Db;

    public Task InitializeAsync() => _fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private Task<long> CountPeopleAsync() => _fixture.ScalarAsync<long>("select count(*) from people");

    [Fact]
    public async Task Success_commits_and_returns_the_function_result()
    {
        long seenOutsideBeforeCommit = -1;

        var inside = await Db.InTransactionAsync(async tx =>
        {
            await tx.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 1, "a");
            await tx.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 2, "b");

            // another (independent) connection sees nothing before the commit
            seenOutsideBeforeCommit = await CountPeopleAsync();

            // while the transaction itself sees its own changes
            var rs = await tx.ExecuteQueryAsync("select count(*) as n from people");
            rs.Next();
            return rs.GetLong("n");
        });

        Assert.Equal(2L, inside);
        Assert.Equal(0L, seenOutsideBeforeCommit);
        Assert.Equal(2L, await CountPeopleAsync());
    }

    [Fact]
    public async Task Function_that_throws_rolls_back_and_rethrows_that_exact_exception()
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Db.InTransactionAsync<int>(async tx =>
        {
            await tx.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 1, "a");
            await tx.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 2, "b");
            throw new InvalidOperationException("boom");
        }));

        Assert.Equal("boom", error.Message);
        Assert.Equal(0L, await CountPeopleAsync());
    }

    [Fact]
    public async Task SQL_error_midway_rolls_back_the_earlier_statements_too_and_rethrows_PostgresException()
    {
        var error = await Assert.ThrowsAsync<PostgresException>(() => Db.InTransactionAsync(async tx =>
        {
            await tx.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 1, "a");
            await tx.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 1, "trùng khoá chính");
        }));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, error.SqlState);
        Assert.Equal(0L, await CountPeopleAsync());   // the first successful insert is rolled back as well
    }

    [Fact]
    public async Task Connection_is_still_usable_after_a_rollback()
    {
        await using var single = _fixture.CreateDatabase(configureConnection: c =>
        {
            c.MaxPoolSize = 1;
            c.Timeout = 5;
        });

        await Assert.ThrowsAsync<PostgresException>(() => single.InTransactionAsync(async tx =>
        {
            await tx.ExecuteUpdateAsync("insert into people(id) values (?)", 1);
            await tx.ExecuteUpdateAsync("insert into people(id) values (?)", 1);
        }));

        // the pool's single connection must be reusable, not stuck in a failed transaction state
        Assert.Equal(1, await single.ExecuteUpdateAsync("insert into people(id) values (?)", 2));
        Assert.Equal(1L, await CountPeopleAsync());
    }

    [Fact]
    public async Task Cancelling_midway_rolls_back_and_throws_OperationCanceledException()
    {
        using var source = new CancellationTokenSource();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Db.InTransactionAsync(
            async tx =>
            {
                await tx.ExecuteUpdateAsync("insert into people(id) values (?)", 1);
                await source.CancelAsync();
                await tx.ExecuteUpdateAsync("insert into people(id) values (?)", [2], source.Token);
            },
            source.Token));

        Assert.Equal(0L, await CountPeopleAsync());
    }

    [Fact]
    public async Task Using_NroTransaction_after_the_transaction_ended_is_an_error()
    {
        NroTransaction? leaked = null;
        await Db.InTransactionAsync(tx =>
        {
            leaked = tx;
            return Task.CompletedTask;
        });

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => leaked!.ExecuteUpdateAsync("insert into people(id) values (?)", 1));

        Assert.Contains("ended", error.Message);
        Assert.Equal(0L, await CountPeopleAsync());
    }

    [Fact]
    public async Task Isolation_level_can_be_chosen_and_defaults_to_ReadCommitted()
    {
        var defaultLevel = await Db.InTransactionAsync(async tx =>
        {
            var rs = await tx.ExecuteQueryAsync("select current_setting('transaction_isolation') as level");
            rs.Next();
            return rs.GetString("level");
        });

        var serializable = await Db.InTransactionAsync(
            async tx =>
            {
                var rs = await tx.ExecuteQueryAsync("select current_setting('transaction_isolation') as level");
                rs.Next();
                return rs.GetString("level");
            },
            isolationLevel: IsolationLevel.Serializable);

        Assert.Equal("read committed", defaultLevel);
        Assert.Equal("serializable", serializable);
    }

    [Fact]
    public async Task Each_statement_outside_a_transaction_uses_its_own_connection_and_does_not_see_uncommitted_changes()
    {
        await Db.InTransactionAsync(async tx =>
        {
            await tx.ExecuteUpdateAsync("insert into people(id) values (?)", 1);

            var outside = await Db.ExecuteQueryAsync("select count(*) from people");
            outside.Next();
            Assert.Equal(0L, outside.GetLong(1));    // exactly like Java: ConnectDB.executeQuery takes another connection
        });

        Assert.Equal(1L, await CountPeopleAsync());
    }

    [Fact]
    public async Task Function_without_a_return_value_also_commits_on_success()
    {
        await Db.InTransactionAsync(async tx =>
        {
            await tx.ExecuteUpdateAsync("insert into pets(id, owner_id, name) values()", 1, 1, "x");
        });

        Assert.Equal(1L, await _fixture.ScalarAsync<long>("select count(*) from pets"));
    }
}
