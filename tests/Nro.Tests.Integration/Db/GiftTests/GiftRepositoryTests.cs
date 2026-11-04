using Nro.Data.Repositories.Gift;

namespace Nro.Tests.Integration.Db.GiftTests;

public sealed class GiftRepositoryTests : IClassFixture<GiftRepositoryFixture>, IAsyncLifetime
{
    private readonly GiftRepositoryFixture _fixture;

    public GiftRepositoryTests(GiftRepositoryFixture fixture) => _fixture = fixture;

    private GiftRepository Repository => new(_fixture.Db);

    public Task InitializeAsync() => _fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task LoadOrCreate_creates_false_flags_for_a_new_player()
    {
        GiftPlayerState state = await Repository.LoadOrCreateAsync(101);

        Assert.Equal(new GiftPlayerState(false, false), state);
        Assert.Equal(1L, await _fixture.ScalarAsync<long>("select count(*) from gift where player_id = 101"));
        Assert.False(await _fixture.ScalarAsync<bool>("select is_newbie_gold_claimed from gift where player_id = 101"));
        Assert.False(await _fixture.ScalarAsync<bool>("select is_newbie_gem_claimed from gift where player_id = 101"));
    }

    [Fact]
    public async Task Save_then_load_round_trips_both_gift_flags()
    {
        _ = await Repository.LoadOrCreateAsync(102);
        await Repository.SaveAsync(102, new GiftPlayerState(true, false));

        Assert.Equal(new GiftPlayerState(true, false), await Repository.LoadOrCreateAsync(102));
        Assert.True(await _fixture.ScalarAsync<bool>("select is_newbie_gold_claimed from gift where player_id = 102"));
        Assert.False(await _fixture.ScalarAsync<bool>("select is_newbie_gem_claimed from gift where player_id = 102"));
    }

    [Fact]
    public async Task Concurrent_LoadOrCreate_calls_do_not_duplicate_the_player_row()
    {
        Task<GiftPlayerState>[] loads = Enumerable.Range(0, 8)
            .Select(_ => Repository.LoadOrCreateAsync(103))
            .ToArray();

        GiftPlayerState[] states = await Task.WhenAll(loads);

        Assert.All(states, state => Assert.Equal(new GiftPlayerState(false, false), state));
        Assert.Equal(1L, await _fixture.ScalarAsync<long>("select count(*) from gift where player_id = 103"));
    }

    [Fact]
    public async Task InsertSql_ignores_an_existing_player_without_overwriting_saved_flags()
    {
        const int playerId = 106;
        await using var firstConnection = _fixture.CreateDatabase();
        await using var secondConnection = _fixture.CreateDatabase();

        Assert.Equal(1, await firstConnection.InTransactionAsync(
            transaction => transaction.ExecuteUpdateAsync(GiftRepository.InsertSql, playerId)));
        await Repository.SaveAsync(playerId, new GiftPlayerState(true, true));

        Assert.Equal(0, await secondConnection.InTransactionAsync(
            transaction => transaction.ExecuteUpdateAsync(GiftRepository.InsertSql, playerId)));

        Assert.Equal(1L, await _fixture.ScalarAsync<long>("select count(*) from gift where player_id = 106"));
        Assert.True(await _fixture.ScalarAsync<bool>("select is_newbie_gold_claimed from gift where player_id = 106"));
        Assert.True(await _fixture.ScalarAsync<bool>("select is_newbie_gem_claimed from gift where player_id = 106"));
    }

    [Fact]
    public async Task Save_without_a_row_throws_and_does_not_create_one()
    {
        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => Repository.SaveAsync(104, new GiftPlayerState(true, true)));

        Assert.Equal("Expected to update one gift row, updated 0.", error.Message);
        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select count(*) from gift where player_id = 104"));
    }

    [Fact]
    public async Task LoadOrCreate_rolls_back_the_transaction_when_insert_fails()
    {
        await _fixture.RunAsync("create or replace function reject_gift_insert() returns trigger language plpgsql as $$ begin insert into gift_attempt(player_id) values (new.player_id); raise exception 'synthetic gift failure'; end $$");
        await _fixture.RunAsync("create trigger gift_reject_insert before insert on gift for each row execute function reject_gift_insert()");

        await Assert.ThrowsAnyAsync<Exception>(() => Repository.LoadOrCreateAsync(105));

        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select count(*) from gift where player_id = 105"));
        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select count(*) from gift_attempt where player_id = 105"));
    }
}

public sealed class GiftRepositoryFixture : TestDatabase
{
    protected override async Task SeedAsync()
    {
        await RunAsync("create table gift(player_id integer not null primary key, is_newbie_gold_claimed boolean not null default false, is_newbie_gem_claimed boolean not null default false)");
        await RunAsync("create table gift_attempt(player_id integer not null)");
    }

    public async Task ResetAsync()
    {
        await RunAsync("drop trigger if exists gift_reject_insert on gift");
        await RunAsync("drop function if exists reject_gift_insert()");
        await RunAsync("delete from gift_attempt; delete from gift");
    }
}
