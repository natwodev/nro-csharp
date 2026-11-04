using Nro.Data.Db;
using Nro.Data.Repositories.Players;
using Nro.Game;
using Nro.Game.Players.Persistence;

namespace Nro.Tests.Integration.Db.Players;

/// <summary>Repository-level coverage for lossless player writes on synthetic rows only.</summary>
[Collection(PlayerAccountDatabaseCollection.Name)]
public class PlayerRepositoryTests : IClassFixture<PlayerRepositoryFixture>
{
    private readonly PlayerRepositoryFixture _fixture;

    public PlayerRepositoryTests(PlayerRepositoryFixture fixture) => _fixture = fixture;

    [Fact]
    public void Update_statement_uses_only_whitelisted_changed_columns_in_schema_order()
    {
        string? sql = PlayerRepository.BuildUpdateSql(["data_point", "name", "data_inventory"]);

        Assert.Equal("update player set name=?,data_inventory=?,data_point=? where id=?", sql);
        Assert.DoesNotContain("items_bag", sql, StringComparison.Ordinal);
        Assert.Throws<ArgumentOutOfRangeException>(() => PlayerRepository.BuildUpdateSql(["name; delete from player"]));
        Assert.Null(PlayerRepository.BuildUpdateSql([]));
    }

    [Fact]
    public async Task Create_updates_account_and_inserts_a_full_row_then_case_insensitive_lookup_and_dirty_save_preserve_other_columns()
    {
        await _fixture.ResetAsync();
        await _fixture.AddAccountAsync(10);
        var repository = new PlayerRepository(_fixture.Db);
        PlayerPersistenceRecord created = await repository.CreateAsync(_fixture.NewRecord(10, "saiyan"), new PlayerAccountBalance(10, 999, 123, 456));

        Assert.True(created.Id > 0);
        Assert.Equal(999L, await _fixture.ScalarAsync<long>("select vnd from account where id = $1", 10));
        Assert.Equal(123L, await _fixture.ScalarAsync<long>("select tongnap from account where id = $1", 10));
        Assert.Equal(456L, await _fixture.ScalarAsync<long>("select coin from account where id = $1", 10));
        Assert.Equal(created.Id, (await repository.FindByNameAsync("SAIYAN"))!.Id);
        Assert.Equal(created.Id, (await repository.FindByNameAsync("SaiYan"))!.Id);

        await _fixture.RunAsync("update player set items_bag = '[\"outside\"]' where account_id = 10");
        await repository.SaveAsync(created.With("data_point", "[9223372036854775807,0]"));

        Assert.Equal("[9223372036854775807,0]", await _fixture.ScalarAsync<string>("select data_point from player where id = $1", created.Id));
        Assert.Equal("[\"outside\"]", await _fixture.ScalarAsync<string>("select items_bag from player where id = $1", created.Id));
    }

    [Fact]
    public async Task Create_rolls_back_the_account_change_when_player_insert_fails()
    {
        await _fixture.ResetAsync();
        await _fixture.AddAccountAsync(20);
        var repository = new PlayerRepository(_fixture.Db);
        PlayerPersistenceRecord invalid = _fixture.NewRecord(20, "rollback").With("name", null);

        await Assert.ThrowsAnyAsync<Exception>(() => repository.CreateAsync(invalid, new PlayerAccountBalance(20, 999, 123, 456)));

        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select vnd from account where id = $1", 20));
        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select tongnap from account where id = $1", 20));
        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select coin from account where id = $1", 20));
        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select count(*) from player"));
    }

    [Fact]
    public async Task Create_rejects_an_unknown_account_before_inserting_an_orphan_player()
    {
        await _fixture.ResetAsync();
        var repository = new PlayerRepository(_fixture.Db);

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.CreateAsync(_fixture.NewRecord(404, "missingacct"), new PlayerAccountBalance(404, 999, 123, 456)));

        Assert.Equal("Expected to update exactly one account row before creating a player; updated 0.", error.Message);
        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select count(*) from player"));
    }

    [Fact]
    public async Task Json_text_keeps_null_and_long_lexemes_and_rejects_empty_or_malformed_required_arrays()
    {
        await _fixture.ResetAsync();
        await _fixture.AddAccountAsync(30);
        var repository = new PlayerRepository(_fixture.Db);
        string longText = new('x', 9_000);
        PlayerPersistenceRecord created = await repository.CreateAsync(
            _fixture.NewRecord(30, "jsondata").With("data_card", longText),
            new PlayerAccountBalance(30, 0, 0, 0));

        PlayerPersistenceRecord loaded = (await repository.FindByIdAsync(created.Id))!;
        Assert.Null(loaded["notify"]);
        Assert.Equal(longText, loaded["data_card"]);

        await _fixture.RunAsync("update player set data_inventory = '' where account_id = 30");
        await Assert.ThrowsAsync<FormatException>(() => repository.FindByIdAsync(created.Id));
        await _fixture.RunAsync("update player set data_inventory = '[1' where account_id = 30");
        await Assert.ThrowsAsync<FormatException>(() => repository.FindByIdAsync(created.Id));
    }

    [Fact]
    public async Task Full_114_column_row_preserves_each_deferred_column_type_and_value_after_hydrated_mutation()
    {
        await _fixture.ResetAsync();
        await _fixture.AddAccountAsync(40);
        var repository = new PlayerRepository(_fixture.Db);
        var mapper = new PlayerPersistenceMapper();
        PlayerPersistenceRecord original = await repository.CreateAsync(
            _fixture.NewCompositeRecord(40, "deferreddb"),
            new PlayerAccountBalance(40, 0, 0, 0));

        Player player = mapper.Load(original, 1_700_000_000_000L);
        MutateHydratedColumns(player);
        await repository.SaveAsync(mapper.Save(player));

        PlayerPersistenceRecord reloaded = (await repository.FindByIdAsync(original.Id))!;
        IReadOnlySet<string> deferredColumns = mapper.GetDeferredData(mapper.Load(reloaded, 1_700_000_000_000L)).DeferredColumns;
        IReadOnlyDictionary<string, object?> expectedHydrated = ExpectedHydratedValuesAfterMutation(original.Id);
        Assert.Equal(114, original.Values.Count);
        Assert.Equal(96, deferredColumns.Count);
        Assert.Equal(18, expectedHydrated.Count);
        Assert.Equal(114, expectedHydrated.Count + deferredColumns.Count);
        foreach ((string column, object? expected) in expectedHydrated)
        {
            AssertSameTypeAndValue(column, expected, reloaded[column]);
        }

        foreach (string column in deferredColumns)
        {
            AssertSameTypeAndValue(column, original[column], reloaded[column]);
        }
    }

    private static IReadOnlyDictionary<string, object?> ExpectedHydratedValuesAfterMutation(int playerId) =>
        new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["id"] = playerId,
            ["account_id"] = 40,
            ["name"] = "changedone",
            ["head"] = 777,
            ["gender"] = 2,
            ["have_tennis_space_ship"] = true,
            ["last_time_drop_tail"] = 2_000_000_001L,
            ["last_time_khi_su_kien"] = 2_000_000_002L,
            ["isbienhinh"] = 91,
            ["notify"] = "mutated notify",
            ["lasttimepkcommeson"] = 2_000_000_003L,
            ["dragon_christmas"] = 2_000_000_004L,
            ["dragon_halloween"] = 2_000_000_005L,
            ["lastimelogin"] = DateTime.SpecifyKind(DateTime.UnixEpoch.AddSeconds(1), DateTimeKind.Unspecified),
            ["first_time_login"] = DateTime.UnixEpoch.AddSeconds(2),
            ["last_time_login_game"] = DateTime.UnixEpoch.AddSeconds(3),
            ["data_inventory"] = "[8000000000,71,72,73]",
            ["data_point"] = "[1,8000000001,8000000002,74,75,8000000003,8000000004,8000000005,76,77,0,8000000006,8000000007,8000000008,8000000009,8000000010]",
        };

    private static void MutateHydratedColumns(Player player)
    {
        player.Name = "changedone";
        player.Head = 777;
        player.Gender = 2;
        player.HaveTennisSpaceShip = true;
        player.LastTimeDropTail = 2_000_000_001;
        player.LastEventGasTime = 2_000_000_002;
        player.TransformationState = 91;
        player.Notify = "mutated notify";
        player.LastPkCommesonTime = 2_000_000_003;
        player.LastTimeShenronAppeared_Christmas = 2_000_000_004;
        player.LastTimeShenronAppeared_Halloween = 2_000_000_005;
        player.Lastimelogin = DateTime.SpecifyKind(DateTime.UnixEpoch.AddSeconds(1), DateTimeKind.Unspecified);
        player.FirstTimeLogin = DateTime.UnixEpoch.AddSeconds(2);
        player.LastTimeLoginGame = DateTime.UnixEpoch.AddSeconds(3);
        player.Inventory.Gold = 8_000_000_000;
        player.Inventory.Gem = 71;
        player.Inventory.Ruby = 72;
        player.Inventory.VipExperience = 73;
        player.NPoint.LimitPower = 1;
        player.NPoint.Power = 8_000_000_001;
        player.NPoint.Potential = 8_000_000_002;
        player.NPoint.Stamina = 74;
        player.NPoint.MaxStamina = 75;
        player.NPoint.BaseHp = 8_000_000_003;
        player.NPoint.BaseMp = 8_000_000_004;
        player.NPoint.BaseDamage = 8_000_000_005;
        player.NPoint.BaseDefense = 76;
        player.NPoint.BaseCriticalChance = 77;
        player.NPoint.Hp = 8_000_000_006;
        player.NPoint.Mp = 8_000_000_007;
        player.NPoint.Damage = 8_000_000_008;
        player.NPoint.HpMax = 8_000_000_009;
        player.NPoint.MpMax = 8_000_000_010;
    }

    private static void AssertSameTypeAndValue(string column, object? expected, object? actual)
    {
        if (expected is null)
        {
            Assert.Null(actual);
            return;
        }

        Assert.NotNull(actual);
        object actualValue = actual!;
        Assert.Equal(expected.GetType(), actualValue.GetType());
        Assert.Equal(expected, actualValue);
        if (expected is DateTime expectedDateTime)
        {
            Assert.Equal(expectedDateTime.Kind, ((DateTime)actualValue).Kind);
        }
    }
}

public sealed class PlayerRepositoryFixture : TestDatabase
{
    private static readonly HashSet<string> IntegerColumns =
    ["account_id", "head", "gender", "clan_id", "point_pvp", "whis_sparring", "isbienhinh", "rank", "event_point", "event_point_boss", "event_point_nhs", "event_point_quai", "diem_huy_diet", "diem_chien_truong_namek", "diem_su_kien_tet", "diem_su_kien_giangsinh", "diem_su_kien_halloween", "diem_su_kien_8_3", "diem_su_kien_trungthu", "diem_su_kien_hung_vuong", "diem_su_kien_black_friday", "diem_su_kien_20_10", "hp_point_fusion", "mp_point_fusion", "point_fusion_damage"];
    private static readonly HashSet<string> BigintColumns = ["last_time_drop_tail", "last_time_khi_su_kien", "barracks_state", "lasttimepkcommeson", "dragon_christmas", "dragon_halloween", "last_time_update_task"];
    private static readonly HashSet<string> TimestampColumns = ["create_time", "lastimelogin", "first_time_login", "last_time_login_game"];

    protected override async Task SeedAsync()
    {
        await RunAsync("create table account(id integer primary key, vnd bigint not null default 0, tongnap bigint not null default 0, coin bigint not null default 0)");
        string columns = string.Join(",", PlayerPersistenceColumns.All.Select(ColumnDefinition));
        await RunAsync("create table player (" + columns + ")"); // lint-ok: L08 test schema uses fixed whitelist
        await RunAsync("create unique index ux_player_account_id on player(account_id)");
    }

    public Task ResetAsync() => RunAsync("delete from player; delete from account");

    public async Task AddAccountAsync(int id)
    {
        _ = await Db.ExecuteUpdateAsync("insert into account(id) values (?)", id);
    }

    public PlayerPersistenceRecord NewRecord(int accountId, string name)
    {
        var values = PlayerPersistenceColumns.All.ToDictionary(column => column, DefaultValue, StringComparer.Ordinal);
        values["id"] = 0;
        values["account_id"] = accountId;
        values["name"] = name;
        values["gender"] = 1;
        values["head"] = 102;
        return new PlayerPersistenceRecord(values);
    }

    public PlayerPersistenceRecord NewCompositeRecord(int accountId, string name)
    {
        PlayerPersistenceRecord record = NewRecord(accountId, name);
        var values = new Dictionary<string, object?>(record.Values, StringComparer.Ordinal);
        foreach (string column in PlayerPersistenceColumns.All)
        {
            values[column] = CompositeValue(column, values[column]);
        }

        values["id"] = 0;
        values["account_id"] = accountId;
        values["name"] = name;
        values["head"] = 102;
        values["gender"] = 1;
        values["have_tennis_space_ship"] = false;
        values["data_inventory"] = "[7000000000,11,12,13]";
        values["data_point"] = "[0,7000000001,7000000002,14,15,7000000003,7000000004,7000000005,16,17,0,7000000006,7000000007,7000000008,7000000009,7000000010]";
        values["notify"] = null;
        return new PlayerPersistenceRecord(values);
    }

    private static string ColumnDefinition(string column) => column switch
    {
        "id" => "id integer generated by default as identity primary key",
        "name" => "name citext not null",
        "have_tennis_space_ship" => "have_tennis_space_ship boolean not null",
        "notify" => "notify text null",
        _ when IntegerColumns.Contains(column) => column + " integer not null",
        _ when BigintColumns.Contains(column) => column + " bigint not null",
        _ when TimestampColumns.Contains(column) => column + (column == "lastimelogin" ? " timestamp not null" : " timestamptz not null"),
        _ => column + " text not null",
    };

    private static object? DefaultValue(string column) => column switch
    {
        "id" => 0,
        "name" => "player",
        "have_tennis_space_ship" => false,
        "notify" => null,
        _ when IntegerColumns.Contains(column) => 0,
        _ when BigintColumns.Contains(column) => 0L,
        "lastimelogin" => DateTime.SpecifyKind(DateTime.UnixEpoch, DateTimeKind.Unspecified),
        _ when TimestampColumns.Contains(column) => DateTime.UnixEpoch,
        _ => "[]",
    };

    private static object? CompositeValue(string column, object? defaultValue) => defaultValue switch
    {
        int => 100_000 + Array.IndexOf(PlayerPersistenceColumns.All, column),
        long => 9_000_000_000L + Array.IndexOf(PlayerPersistenceColumns.All, column),
        DateTime dateTime => dateTime.AddSeconds(Array.IndexOf(PlayerPersistenceColumns.All, column)),
        string when column is not "name" => "[\"" + column + "\",9223372036854775807]",
        _ => defaultValue,
    };
}
