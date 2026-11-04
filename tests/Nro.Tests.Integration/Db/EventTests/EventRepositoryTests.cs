using Microsoft.Extensions.Logging;
using Nro.Data.Db;
using Nro.Data.Repositories.Event;
using Nro.Tests.Integration.Db.Players;
using Nro.Tests.Integration.Db.Top;

namespace Nro.Tests.Integration.Db.EventTests;

/// <summary>Logger of the repository under test (the shared <see cref="ListLogger"/> is typed for <see cref="NroDatabase"/>).</summary>
internal sealed class RepositoryLogger<T> : ILogger<T>
{
    public ListLogger Inner { get; } = new();

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull
        => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => ((ILogger)Inner).Log(logLevel, eventId, state, exception, formatter);
}

/// <summary>
/// <see cref="EventRepository"/> on a real PostgreSQL: the JSON read/written for each event row, Gson-like number reading, partial reads, missing rows,
/// case-insensitive names, duplicate rows, the exact JSON text saved, the warning for a missing row and the all-or-nothing save.
/// </summary>
[Collection(PlayerAccountDatabaseCollection.Name)]
public class EventRepositoryTests : IClassFixture<TopFixture>, IAsyncLifetime
{
    private const string InsertSql = "insert into event(name, data) values (?, ?)";

    private readonly TopFixture _fixture;
    private readonly RepositoryLogger<EventRepository> _logger = new();

    public EventRepositoryTests(TopFixture fixture)
    {
        _fixture = fixture;
    }

    private EventRepository Repository => new(_fixture.Db, _logger);

    public async Task InitializeAsync()
    {
        await _fixture.RunAsync("truncate table event restart identity");
        await _fixture.RunAsync("alter table event drop constraint if exists no_poison");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private Task Insert(string name, string data) => _fixture.Db.ExecuteUpdateAsync(InsertSql, name, data);

    private Task<string> DataOf(string name) => _fixture.ScalarAsync<string>("select data from event where name = $1 order by id limit 1", name);

    private async Task InsertAllEventRows()
    {
        await Insert("international_womens_day", "{}");
        await Insert("luna_new_year", "{}");
        await Insert("christ_mas", "{}");
        await Insert("hung_vuong", "{}");
    }

    [Fact]
    public async Task Loading_when_the_rows_do_not_exist_gives_null_and_does_not_throw()
    {
        Assert.Null(await Repository.LoadHungVuongEventAsync());
        Assert.Null(await Repository.LoadInternationalWomensDayEventAsync());
        Assert.Null(await Repository.LoadLunaNewYearEventAsync());
        Assert.Null(await Repository.LoadChristMasEventAsync());
        Assert.Null(await Repository.LoadEventAsync("something_else"));
    }

    [Fact]
    public async Task Hung_vuong_reads_the_melon_seed_as_a_long()
    {
        await Insert("hung_vuong", "{\"ReceiveMelonSeed\":1700000000123,\"other\":1}");

        var data = await Repository.LoadHungVuongEventAsync();

        Assert.Equal(1700000000123L, data!.ReceiveMelonSeed);
    }

    [Theory]
    [InlineData("{}", null)]
    [InlineData("{\"ReceiveMelonSeed\":null}", null)] // Gson: JsonNull.getAsLong throws, nothing applied
    [InlineData("{\"ReceiveMelonSeed\":true}", null)]
    [InlineData("{\"ReceiveMelonSeed\":{\"a\":1}}", null)]
    [InlineData("{\"ReceiveMelonSeed\":\"12\"}", 12L)] // a string is parsed (Long.parseLong)
    [InlineData("{\"ReceiveMelonSeed\":\" 12\"}", null)]
    [InlineData("{\"ReceiveMelonSeed\":\"abc\"}", null)]
    [InlineData("{\"ReceiveMelonSeed\":12.9}", 12L)] // LazilyParsedNumber.longValue truncates
    [InlineData("{\"ReceiveMelonSeed\":1.5e3}", 1500L)]
    [InlineData("{\"ReceiveMelonSeed\":-7}", -7L)]
    [InlineData("{\"ReceiveMelonSeed\":[5]}", 5L)] // JsonArray.getAsLong of a one-element array
    [InlineData("{\"ReceiveMelonSeed\":[5,6]}", null)]
    [InlineData("{\"ReceiveMelonSeed\":9223372036854775808}", -9223372036854775808L)] // BigDecimal.longValue keeps the low 64 bits
    [InlineData("not json", null)]
    [InlineData("[1,2]", null)]
    [InlineData("", null)]
    [InlineData("null", null)]
    public async Task Hung_vuong_tolerates_unreadable_data_like_the_java_catch_block(string json, long? expected)
    {
        await Insert("hung_vuong", json);

        var data = await Repository.LoadHungVuongEventAsync();

        Assert.NotNull(data);
        Assert.Equal(expected, data.ReceiveMelonSeed);
    }

    [Fact]
    public async Task Unreadable_data_is_logged_as_an_error_without_its_content()
    {
        await Insert("hung_vuong", "{\"ReceiveMelonSeed\":\"secret-marker\"}");

        await Repository.LoadHungVuongEventAsync();

        var entry = Assert.Single(_logger.Inner.Entries, e => e.Level == LogLevel.Error);
        Assert.Contains("hung_vuong", entry.Message);
        Assert.DoesNotContain("secret-marker", _logger.Inner.Everything());
    }

    [Fact]
    public async Task Womens_day_reads_the_five_keys_with_the_java_misspellings()
    {
        await Insert("international_womens_day", "{\"damePrecent\":1,\"hpPrecent\":2,\"mpPrecent\":3,\"eventPoint\":4,\"lastExpRewardStage\":5}");

        var data = await Repository.LoadInternationalWomensDayEventAsync();

        Assert.Equal(new InternationalWomensDayEventData(1, 2, 3, 4, 5), data);
    }

    [Fact]
    public async Task Womens_day_keeps_the_keys_read_before_a_failure_and_skips_absent_keys()
    {
        await Insert("international_womens_day", "{\"damePrecent\":1,\"hpPrecent\":\"x\",\"mpPrecent\":3,\"eventPoint\":4}");

        var data = await Repository.LoadInternationalWomensDayEventAsync();

        Assert.Equal(new InternationalWomensDayEventData(1, null, null, null, null), data);
    }

    [Fact]
    public async Task Womens_day_with_only_some_keys_leaves_the_others_null()
    {
        await Insert("international_womens_day", "{\"eventPoint\":40}");

        Assert.Equal(new InternationalWomensDayEventData(null, null, null, 40, null), await Repository.LoadInternationalWomensDayEventAsync());
    }

    [Theory]
    [InlineData("{\"eventPoint\":7}", 7)]
    [InlineData("{\"eventPoint\":2147483648}", int.MinValue)] // int overflow wraps like (int) of the long
    [InlineData("{\"eventPoint\":3.99}", 3)]
    [InlineData("{\"eventPoint\":\"8\"}", 8)]
    [InlineData("{\"eventPoint\":\"3000000000\"}", null)] // Integer.parseInt fails
    [InlineData("{\"eventPoint\":-2147483649}", int.MaxValue)]
    public async Task Event_points_are_read_like_gson_getAsInt(string json, int? expectedPoint)
    {
        await Insert("luna_new_year", json.Replace("}", ",\"lastExpRewardStage\":9}"));

        var data = await Repository.LoadLunaNewYearEventAsync();

        Assert.Equal(expectedPoint, data?.EventPoint);
        Assert.Equal(expectedPoint is null ? (int?)null : 9, data?.LastExpRewardStage);
    }

    [Theory]
    [InlineData("{\"eventPoint\":1}")] // second key missing: Java NullPointerException before any consumer was called
    [InlineData("{\"lastExpRewardStage\":1}")]
    [InlineData("{}")]
    [InlineData("[]")]
    [InlineData("oops")]
    [InlineData("{\"eventPoint\":1,\"lastExpRewardStage\":null}")]
    public async Task Load_event_needs_both_keys_or_returns_null(string json)
    {
        await Insert("christ_mas", json);

        Assert.Null(await Repository.LoadChristMasEventAsync());
    }

    [Fact]
    public async Task Event_names_are_looked_up_case_insensitively_like_the_mysql_collation()
    {
        await Insert("Luna_New_Year", "{\"eventPoint\":11,\"lastExpRewardStage\":22}");

        Assert.Equal(new EventPointData(11, 22), await Repository.LoadLunaNewYearEventAsync());
        Assert.Equal(new EventPointData(11, 22), await Repository.LoadEventAsync("LUNA_NEW_YEAR"));
    }

    [Fact]
    public async Task With_duplicate_rows_the_lowest_id_is_read()
    {
        await Insert("christ_mas", "{\"eventPoint\":1,\"lastExpRewardStage\":1}");
        await Insert("christ_mas", "{\"eventPoint\":2,\"lastExpRewardStage\":2}");

        Assert.Equal(new EventPointData(1, 1), await Repository.LoadChristMasEventAsync());
    }

    [Fact]
    public async Task A_hostile_event_name_is_only_a_value()
    {
        await Insert("christ_mas", "{\"eventPoint\":1,\"lastExpRewardStage\":1}");

        Assert.Null(await Repository.LoadEventAsync("x' or '1'='1"));
        Assert.Null(await Repository.LoadEventAsync("'; drop table event; --"));
        Assert.Equal(1L, await _fixture.ScalarAsync<long>("select count(*) from event"));
    }

    [Fact]
    public async Task Save_writes_the_exact_json_text_of_the_java_gson_objects()
    {
        await InsertAllEventRows();

        var result = await Repository.SaveAsync(new EventSaveState(1, 2, 3, 4, 5, 6, 7, 8, 9, 1700000000123));

        Assert.Equal(new EventSaveResult(1, 1, 1, 1), result);
        Assert.Equal("{\"damePrecent\":1,\"hpPrecent\":2,\"mpPrecent\":3,\"eventPoint\":4,\"lastExpRewardStage\":5}", await DataOf("international_womens_day"));
        Assert.Equal("{\"eventPoint\":6,\"lastExpRewardStage\":7}", await DataOf("luna_new_year"));
        Assert.Equal("{\"eventPoint\":8,\"lastExpRewardStage\":9}", await DataOf("christ_mas"));
        Assert.Equal("{\"ReceiveMelonSeed\":1700000000123}", await DataOf("hung_vuong"));
        Assert.Empty(_logger.Inner.Entries);
    }

    [Fact]
    public async Task Save_then_load_round_trips_every_event_including_negative_and_extreme_numbers()
    {
        await InsertAllEventRows();
        var state = new EventSaveState(-1, int.MaxValue, 0, int.MinValue, 12, 345, -6, 7, 8, long.MinValue);

        await Repository.SaveAsync(state);

        Assert.Equal(new InternationalWomensDayEventData(-1, int.MaxValue, 0, int.MinValue, 12), await Repository.LoadInternationalWomensDayEventAsync());
        Assert.Equal(new EventPointData(345, -6), await Repository.LoadLunaNewYearEventAsync());
        Assert.Equal(new EventPointData(7, 8), await Repository.LoadChristMasEventAsync());
        Assert.Equal(long.MinValue, (await Repository.LoadHungVuongEventAsync())!.ReceiveMelonSeed);
    }

    [Fact]
    public async Task Save_warns_for_each_missing_row_and_still_updates_the_others()
    {
        await Insert("luna_new_year", "{}");
        await Insert("hung_vuong", "{}");

        var result = await Repository.SaveAsync(new EventSaveState(0, 0, 0, 0, 0, 1, 2, 0, 0, 3));

        Assert.Equal(new EventSaveResult(0, 1, 0, 1), result);
        Assert.Equal("{\"eventPoint\":1,\"lastExpRewardStage\":2}", await DataOf("luna_new_year"));
        var warnings = _logger.Inner.Entries.Where(e => e.Level == LogLevel.Warning).Select(e => e.Message).ToList();
        Assert.Equal(2, warnings.Count);
        Assert.Contains(warnings, w => w.Contains("international_womens_day"));
        Assert.Contains(warnings, w => w.Contains("christ_mas"));
    }

    [Fact]
    public async Task Save_updates_every_row_that_has_the_name()
    {
        await InsertAllEventRows();
        await Insert("luna_new_year", "{}");

        var result = await Repository.SaveAsync(new EventSaveState(0, 0, 0, 0, 0, 5, 6, 0, 0, 0));

        Assert.Equal(2, result.LunaNewYearRows);
        Assert.Equal(2L, await _fixture.ScalarAsync<long>("select count(*) from event where data = '{\"eventPoint\":5,\"lastExpRewardStage\":6}'"));
    }

    [Fact]
    public async Task A_failure_in_the_middle_of_save_rolls_all_four_rows_back_and_is_not_swallowed()
    {
        await InsertAllEventRows();
        await _fixture.RunAsync("alter table event add constraint no_poison check (name <> 'christ_mas' or data not like '%99999%')");

        await Assert.ThrowsAnyAsync<Exception>(() => Repository.SaveAsync(new EventSaveState(1, 1, 1, 1, 1, 1, 1, 99999, 1, 1)));

        Assert.Equal(4L, await _fixture.ScalarAsync<long>("select count(*) from event where data = '{}'"));
    }

    [Fact]
    public async Task Cancelled_token_stops_a_load()
    {
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Repository.LoadHungVuongEventAsync(cancelled.Token));
    }
}
