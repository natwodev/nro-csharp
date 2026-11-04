using Nro.Data.Db;
using Nro.Data.Repositories.Top;
using Nro.Tests.Integration.Db.Players;

namespace Nro.Tests.Integration.Db.Top;

/// <summary>
/// Behaviour of <see cref="TopRepository"/> on small hand-made synthetic data in a real PostgreSQL: empty tables, filters, tie-break, NULL / <c>null</c> / malformed JSON,
/// limits, string numbers, negative numbers. (Equivalence with the Java SQL on larger data is <see cref="TopRepositoryGoldenTests"/>.)
/// </summary>
[Collection(PlayerAccountDatabaseCollection.Name)]
public class TopRepositoryTests : IClassFixture<TopFixture>, IAsyncLifetime
{
    private readonly TopFixture _fixture;

    public TopRepositoryTests(TopFixture fixture)
    {
        _fixture = fixture;
    }

    // A collection expression targets ReadOnlySpan<T> for Assert.Equal, which cannot live across an await: use an array.
    private static void Same<T>(T[] expected, IEnumerable<T> actual) => Assert.Equal(expected, actual);

    private ITopRepository Repository => new TopRepository(_fixture.Db);

    public Task InitializeAsync() => _fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    private static TopPlayer P(int id, string? name = null) => new(id, name ?? "p" + id);

    /// <summary>A JSON array of strings, the way the game stores item lists and the pet: each element is itself JSON text, with its quotes escaped.</summary>
    private static string Items(params string[] slots) => "[" + string.Join(",", slots.Select(slot => "\"" + slot.Replace("\"", "\\\"") + "\"")) + "]";

    private static string Points(long power) => "[13," + power + ",0,0,0,0,0,0,0,0,0,0,0,0,0,0]";

    private async Task<int[]> PowerIds() => (await Repository.GetTopAsync(TopRanking.Power)).Select(e => e.PlayerId).ToArray();

    [Fact]
    public async Task Every_ranking_on_empty_tables_is_an_empty_list()
    {
        foreach (var ranking in Enum.GetValues<TopRanking>())
        {
            Assert.Empty(await Repository.GetTopAsync(ranking));
        }

        Assert.Empty(await Repository.GetTopNvAsync());
        Assert.Empty(await Repository.GetTopWhisAsync());
        Assert.Empty(await Repository.GetTopVdstAsync());
        Assert.Empty(await Repository.GetTopThoiVangAsync());
    }

    [Fact]
    public async Task Unknown_ranking_is_rejected()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => Repository.GetTopAsync((TopRanking)9999));
    }

    [Fact]
    public async Task Order_is_descending_and_the_entry_carries_the_player_columns()
    {
        await _fixture.LoadAsync(
        [
            P(1, "low").With("data_point", Points(10)),
            (P(2, "high") with { Gender = 2, LastLoginSeconds = 1_700_000_123 }).With("data_point", Points(900)).With("items_body", "[\"[1,2]\"]"),
            P(3, "mid").With("data_point", Points(500)),
        ]);

        var top = await Repository.GetTopAsync(TopRanking.Power);

        Same([2, 3, 1], top.Select(e => e.PlayerId));
        Same([900L, 500L, 10L], top.Select(e => e.Value));
        Assert.Equal("high", top[0].Name);
        Assert.Equal(2, top[0].Gender);
        Assert.Equal("[\"[1,2]\"]", top[0].ItemsBody);
        Assert.Equal(DateTimeKind.Utc, top[0].LastTimeLoginGame!.Value.Kind);
        Assert.Equal(1_700_000_123_000L, top[0].LastTimeLoginMillis);
    }

    [Fact]
    public async Task Banned_accounts_are_left_out_except_in_the_gold_bar_ranking()
    {
        var bar = "[\"[457,10,[]]\"]";
        await _fixture.LoadAsync(
        [
            (P(1) with { Ban = 1 }).With("data_point", Points(999)).With("items_bag", bar),
            P(2).With("data_point", Points(5)).With("items_bag", bar),
        ]);

        Same([2], await PowerIds());
        Same([1, 2], (await Repository.GetTopThoiVangAsync()).Select(e => e.PlayerId)); // TOP_THOI_VANG has no ban filter in the Java SQL
    }

    [Fact]
    public async Task A_player_without_an_account_row_is_not_ranked()
    {
        await _fixture.LoadAsync([P(1).With("data_point", Points(5))]);
        await _fixture.RunAsync("insert into player(id, account_id, name, gender, items_body, items_bag, items_box, data_inventory, data_point, data_task, data_luyentap, pet) "
            + "values (2, 999, 'orphan', 0, '[]', '[]', '[]', '[]', '[13,999]', '[]', '[]', '[]')");

        Same([1], await PowerIds());
    }

    [Fact]
    public async Task Machine_gun_rankings_are_per_gender()
    {
        await _fixture.LoadAsync(
        [
            (P(1) with { Gender = 0 }).With("data_may_dam", "[5,50,500]"),
            (P(2) with { Gender = 1 }).With("data_may_dam", "[6,60,600]"),
            (P(3) with { Gender = 2 }).With("data_may_dam", "[7,70,700]"),
            (P(4) with { Gender = 0 }).With("data_may_dam", "[9,1,1]"),
        ]);

        Same([4, 1], (await Repository.GetTopAsync(TopRanking.PunchingMachineEarthling)).Select(e => e.PlayerId));
        Same([2], (await Repository.GetTopAsync(TopRanking.PunchingMachineNamekian)).Select(e => e.PlayerId));
        Same([3], (await Repository.GetTopAsync(TopRanking.PunchingMachineSaiyan)).Select(e => e.PlayerId));
        Same([700L], (await Repository.GetTopAsync(TopRanking.PunchingMachineSaiyan)).Select(e => e.Value));
    }

    [Fact]
    public async Task Ties_are_broken_by_player_id_ascending_whatever_the_insert_order()
    {
        await _fixture.LoadAsync(
            new[] { 7, 3, 9, 1, 5 }.Select(id => P(id).With("data_point", Points(100))).Append(P(2).With("data_point", Points(101))));

        Same([2, 1, 3, 5, 7, 9], await PowerIds());
    }

    [Fact]
    public async Task Missing_null_and_malformed_json_rank_below_every_number_and_never_fail_the_query()
    {
        await _fixture.LoadAsync(
        [
            P(1).With("data_point", Points(100)),
            P(2).With("data_point", "null"), // JSON null document: $[1] does not exist, SQL NULL
            P(3).With("data_point", "[13,"), // malformed
            P(4).With("data_point", "[]"),
            P(5).With("data_point", Points(0)), // a real zero ranks ABOVE the NULL ones
            P(6).With("data_point", ""),
            P(7).With("data_point", "[13,null]"), // JSON null element: casts to 0, not SQL NULL
            P(8).With("data_point", "{\"1\":5}"),
        ]);

        var top = await Repository.GetTopAsync(TopRanking.Power);

        Same([1, 5, 7, 2, 3, 4, 6, 8], top.Select(e => e.PlayerId));
        Same([100L, 0, 0, 0, 0, 0, 0, 0], top.Select(e => e.Value));
    }

    [Fact]
    public async Task Json_null_document_counts_as_zero_for_index_zero_rankings()
    {
        // MariaDB treats a non-array document as a one-element array for $[0]: 'null' is JSON null (0), 'garbage' is invalid (NULL), '5' is 5.
        await _fixture.LoadAsync(
        [
            P(1).With("data_event_new_year", "garbage"),
            P(2).With("data_event_new_year", "null"),
            P(3).With("data_event_new_year", "5"),
            P(4).With("data_event_new_year", "[0,0,0,0]"),
        ]);

        var top = await Repository.GetTopAsync(TopRanking.OpenLuckyMoney);

        Same([3, 2, 4, 1], top.Select(e => e.PlayerId));
    }

    [Fact]
    public async Task One_malformed_row_does_not_remove_the_other_rows_from_any_ranking()
    {
        var broken = P(99).With("data_point", "][").With("pet", "garbage").With("data_task", "{").With("data_luyentap", "x").With("vodaisinhtu", "x")
            .With("data_inventory", "x").With("data_may_dam", "x").With("data_event_new_year", "x").With("items_bag", "\"[457,").With("items_box", "\"[457,");
        await _fixture.LoadAsync([P(1).With("data_point", Points(5)), broken]);

        foreach (var ranking in Enum.GetValues<TopRanking>())
        {
            await Repository.GetTopAsync(ranking); // must not throw
        }

        Same([1, 99], (await Repository.GetTopAsync(TopRanking.Power)).Select(e => e.PlayerId));
        Same([1, 99], (await Repository.GetTopAsync(TopRanking.PetPower)).Select(e => e.PlayerId));

        Assert.Equal(2, (await Repository.GetTopNvAsync()).Count);
        Assert.Contains((await Repository.GetTopThoiVangAsync()), e => e.PlayerId == 99);
    }

    [Fact]
    public async Task Negative_stored_numbers_wrap_above_every_positive_number_like_cast_unsigned()
    {
        await _fixture.LoadAsync([P(1).With("data_point", Points(1_000_000)), P(2).With("data_point", Points(-5))]);

        var top = await Repository.GetTopAsync(TopRanking.Power);

        Same([2, 1], top.Select(e => e.PlayerId));
        Assert.Equal(-5L, top[0].Value); // the unsigned bit pattern 18446744073709551611 read as a long
    }

    [Fact]
    public async Task String_numbers_count_only_where_the_java_sql_unquoted_them()
    {
        await _fixture.LoadAsync(
        [
            P(1).With("data_point", "[13,\"700\"]").With("data_may_dam", "[\"700\",0,0]"),
            P(2).With("data_point", Points(3)).With("data_may_dam", "[3,0,0]"),
        ]);

        // data_point is cast without JSON_UNQUOTE: the string "700" is 0; data_may_dam is unquoted: "700" is 700
        Same([2, 1], await PowerIds());
        Same([1, 2], (await Repository.GetTopAsync(TopRanking.PunchingMachineEarthling)).Select(e => e.PlayerId));
    }

    [Fact]
    public async Task Pet_rankings_read_the_stats_stored_as_a_string_inside_the_pet_array()
    {
        string Pet(long power, long hp, long ki, long dame) => Items("[1,1,0]", $"[13,{power},0,0,0,0,0,0,0,0,{hp},{ki},{dame}]");
        await _fixture.LoadAsync(
        [
            P(1).With("pet", Pet(10, 40, 50, 70)),
            P(2).With("pet", Pet(20, 30, 60, 60)),
            P(3).With("pet", "[]"), // no pet
        ]);

        Same([2, 1, 3], (await Repository.GetTopAsync(TopRanking.PetPower)).Select(e => e.PlayerId));
        Same([1, 2, 3], (await Repository.GetTopAsync(TopRanking.PetHp)).Select(e => e.PlayerId));
        Same([2, 1, 3], (await Repository.GetTopAsync(TopRanking.PetKi)).Select(e => e.PlayerId));
        Same([1, 2, 3], (await Repository.GetTopAsync(TopRanking.PetDamage)).Select(e => e.PlayerId));
        Same([40L, 30L, 0L], (await Repository.GetTopAsync(TopRanking.PetHp)).Select(e => e.Value));
    }

    [Fact]
    public async Task Account_columns_sort_with_null_last_and_cap_at_one_hundred()
    {
        var players = Enumerable.Range(1, 105).Select(i => P(i) with { Vnd = i }).Append(P(200) with { Vnd = null }).Append(P(201) with { Vnd = 1_000_000_000_000 });
        await _fixture.LoadAsync(players);

        var top = await Repository.GetTopAsync(TopRanking.Vnd);

        Assert.Equal(100, top.Count);
        Assert.Equal(201, top[0].PlayerId);
        Assert.Equal(1_000_000_000_000L, top[0].Value); // bigint balance: no int32 overflow
        Assert.Equal(105, top[1].PlayerId);
        Assert.DoesNotContain(top, e => e.PlayerId == 200); // NULL is last and cut off
    }

    [Fact]
    public async Task Null_account_value_is_zero_and_last_when_there_is_room()
    {
        await _fixture.LoadAsync([P(1) with { Vnd = null }, P(2) with { Vnd = 0 }, P(3) with { Vnd = -4 }]);

        var top = await Repository.GetTopAsync(TopRanking.Vnd);

        Same([2, 3, 1], top.Select(e => e.PlayerId));
        Same([0L, -4L, 0L], top.Select(e => e.Value));
    }

    [Fact]
    public async Task Dead_rankings_keep_their_special_filters_and_limits()
    {
        var players = Enumerable.Range(1, 30).Select(i => P(i) with { Cash = i, Danap = 100_000 + i, PointPvp = -i });
        await _fixture.LoadAsync(players.Append((P(31) with { Cash = null, Danap = 5, Ban = 1 })));

        var nap = await Repository.GetTopAsync(TopRanking.TopUp); // no ban filter, 20 rows, NULL last
        var duaNap = await Repository.GetTopAsync(TopRanking.TopUpRace); // danap >= 100000, 10 rows
        var pvp = await Repository.GetTopAsync(TopRanking.Pvp); // CAST(pointPvp AS UNSIGNED): -1 wraps to the top

        Assert.Equal(20, nap.Count);
        Assert.Equal(30, nap[0].PlayerId);
        Assert.Equal(10, duaNap.Count);
        Assert.Equal(30, duaNap[0].PlayerId);
        Assert.Equal(1, pvp[0].PlayerId);
        Assert.Equal(-1L, pvp[0].Value);
    }

    [Fact]
    public async Task Task_ranking_orders_by_task_then_sub_task_then_progress_then_earlier_login()
    {
        await _fixture.LoadAsync(
        [
            P(1).With("data_task", "[10,2,5,0]"),
            P(2).With("data_task", "[10,3,0,0]"),
            P(3).With("data_task", "[11,0,0,0]"),
            (P(4) with { LastLoginSeconds = 1_600_000_000 }).With("data_task", "[10,2,5,0]"), // same as player 1 but logged in earlier: ahead of it
            P(5).With("data_task", "garbage"),
        ]);

        var top = await Repository.GetTopNvAsync();

        Same([3, 2, 4, 1, 5], top.Select(e => e.PlayerId));
        Same([11L, 10L, 10L, 10L, 0L], top.Select(e => e.Task));
        Same([0L, 3L, 2L, 2L, 0L], top.Select(e => e.SubTask));
        Same([0L, 0L, 5L, 5L, 0L], top.Select(e => e.Progress));
    }

    [Fact]
    public async Task Whis_ranking_keeps_only_positive_top_orders_by_time_ascending_and_stops_at_ten()
    {
        string Luyen(long top, string? time) => "[1,false,-1,10000000,1774835465038," + top + (time is null ? "" : "," + time) + ",0,0,0]";
        var players = new List<TopPlayer>
        {
            P(1).With("data_luyentap", Luyen(0, "5")), // top 0: excluded
            P(2).With("data_luyentap", Luyen(3, "50")),
            P(3).With("data_luyentap", Luyen(3, "20")), // same top, faster: first
            P(4).With("data_luyentap", Luyen(9, "900")),
            P(5).With("data_luyentap", "[1,false,-1,1,1,3]"), // time missing = NULL: smallest in ASC, so ahead of 20
            P(6).With("data_luyentap", "bad"),
            P(7).With("data_luyentap", Luyen(-1, "1")), // negative wraps to a huge unsigned number: first
        };
        players.AddRange(Enumerable.Range(10, 12).Select(i => P(i).With("data_luyentap", Luyen(1, i.ToString()))));
        await _fixture.LoadAsync(players);

        var top = await Repository.GetTopWhisAsync();

        Assert.Equal(10, top.Count);
        Same([7, 4, 5, 3, 2, 10, 11, 12, 13, 14], top.Select(e => e.PlayerId));
        Assert.Equal(0L, top[2].Time);
        Assert.DoesNotContain(top, e => e.PlayerId is 1 or 6);
    }

    [Fact]
    public async Task Vdst_ranking_keeps_only_positive_time_and_stops_at_twenty()
    {
        var players = new List<TopPlayer> { P(1).With("vodaisinhtu", "[false,0,7,0]"), P(2).With("vodaisinhtu", "[false,0,7,9]"), P(3).With("vodaisinhtu", "x") };
        players.AddRange(Enumerable.Range(10, 25).Select(i => P(i).With("vodaisinhtu", "[false,0,0," + i + "]")));
        await _fixture.LoadAsync(players);

        var top = await Repository.GetTopVdstAsync();

        Assert.Equal(20, top.Count);
        Assert.Equal(34, top[0].PlayerId);
        Assert.Equal(34L, top[0].Time);
        Assert.DoesNotContain(top, e => e.PlayerId is 1 or 3);
        Assert.All(top, e => Assert.Equal(0L, e.Field2));
    }

    [Fact]
    public async Task Gold_bar_ranking_sums_bag_and_chest_and_reads_only_the_last_stack()
    {
        string Bag(params long[] quantities) => Items(quantities.Select(q => $"[457,{q},\"[]\",1700000000999]").ToArray());
        await _fixture.LoadAsync(
        [
            P(1).With("items_bag", Bag(5, 9)).With("items_box", Bag(1)), // last stack 9 + chest 1
            P(2).With("items_box", Bag(100)), // chest only
            P(3).With("items_bag", "[\"[1,2,\\\"[]\\\",0]\"]"), // no 457 anywhere: not listed
            P(4).With("items_bag", Bag(10)).With("items_box", "[\"[457,abc,[]]\"]"),
        ]);

        var top = await Repository.GetTopThoiVangAsync();

        Same([2, 1, 4], top.Select(e => e.PlayerId));
        Same([100UL, 10UL, 10UL], top.Select(e => e.Total));
        Same([0UL, 9UL, 10UL], top.Select(e => e.BagQuantity));
        Same([100UL, 1UL, 0UL], top.Select(e => e.BoxQuantity));
        Assert.Equal(top[1].PlayerId, top[1].AccountId);
    }

    [Fact]
    public async Task Gold_bar_ranking_stops_at_twenty()
    {
        string Bag(long q) => $"[\"[457,{q},\\\"[]\\\",0]\"]";
        await _fixture.LoadAsync(Enumerable.Range(1, 25).Select(i => P(i).With("items_bag", Bag(i))));

        var top = await Repository.GetTopThoiVangAsync();

        Assert.Equal(20, top.Count);
        Assert.Equal(25, top[0].PlayerId);
        Assert.Equal(6, top[^1].PlayerId);
    }

    [Fact]
    public async Task Rankings_can_run_in_parallel_without_exhausting_the_pool()
    {
        await _fixture.LoadAsync(Enumerable.Range(1, 40).Select(i => P(i).With("data_point", Points(i))));

        var results = await Task.WhenAll(Enumerable.Range(0, 30).Select(_ => Repository.GetTopAsync(TopRanking.Power)));

        Assert.All(results, r => Assert.Equal(40, r.Count));
    }

    [Fact]
    public async Task Cancelled_token_stops_the_ranking()
    {
        using var cancelled = new CancellationTokenSource();
        await cancelled.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Repository.GetTopAsync(TopRanking.Power, cancelled.Token));
    }
}
