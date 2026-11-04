using System.Globalization;
using System.Text.Json;

namespace Nro.Tests.Integration.Db.Top;

/// <summary>
/// One SYNTHETIC player (with its account) for the ranking tests. <see cref="Texts"/> holds the JSON/text columns of <c>player</c> by column name;
/// a column that is not set gets a neutral value from <see cref="TopPlayer.DefaultText"/>.
/// </summary>
public sealed record TopPlayer(
    int Id,
    string Name,
    int Gender = 0,
    int IsBanned = 0,
    long? Vnd = 0,
    long? Coin = 0,
    long ToppedUpAmount = 0,
    int? Cash = 0,
    int NamekWar = 0,
    int? PointPvp = 0,
    long LastLoginSeconds = 1_700_000_000,
    IReadOnlyDictionary<string, string>? Texts = null)
{
    public static readonly string[] TextColumns =
    [
        "items_body", "items_bag", "items_box", "data_inventory", "data_point", "data_task", "data_luyentap", "pet", "vodaisinhtu",
        "data_may_dam", "data_event_new_year", "data_event_christ_mas", "data_event_vulan", "data_event_halloween", "data_event_8_3",
        "data_event_trung_thu", "data_event_hung_vuong", "data_event_black_friday",
    ];

    public int AccountId => Id;

    public DateTime LastLogin => DateTime.SpecifyKind(DateTimeOffset.FromUnixTimeSeconds(LastLoginSeconds).UtcDateTime, DateTimeKind.Utc);

    public string Text(string column) => Texts is not null && Texts.TryGetValue(column, out var value) ? value : DefaultText(column);

    public TopPlayer With(string column, string value)
    {
        var texts = new Dictionary<string, string>(Texts ?? new Dictionary<string, string>()) { [column] = value };
        return this with { Texts = texts };
    }

    public static string DefaultText(string column) => column == "items_body" ? "[\"[-1,0]\",\"[-1,0]\"]" : "[]";

    public static TopPlayer FromJson(JsonElement json)
    {
        var account = json.GetProperty("account");
        var texts = TextColumns.ToDictionary(c => c, c => json.GetProperty(c).GetString()!);
        return new TopPlayer(
            json.GetProperty("id").GetInt32(),
            json.GetProperty("name").GetString()!,
            json.GetProperty("gender").GetInt32(),
            account.GetProperty("ban").GetInt32(),
            OptionalLong(account, "vnd"),
            OptionalLong(account, "coin"),
            account.GetProperty("danap").GetInt64(),
            (int?)OptionalLong(account, "cash"),
            json.GetProperty("diem_chien_truong_namek").GetInt32(),
            (int?)OptionalLong(json, "point_pvp"),
            json.GetProperty("lastTimeLoginGame").GetInt64(),
            texts);
    }

    private static long? OptionalLong(JsonElement owner, string name)
        => owner.GetProperty(name) is { ValueKind: not JsonValueKind.Null } value ? value.GetInt64() : null;
}

/// <summary>Real PostgreSQL schema (nro_test, own schema) with the columns of <c>player</c>/<c>account</c>/<c>event</c> that the repositories read; types copied from <c>postgres/schema/schema.sql</c>.</summary>
public sealed class TopFixture : TestDatabase
{
    private const string InsertAccountSql = "insert into account(id, username, is_admin, cash, is_banned, vnd, coin, topped_up_amount) values (?, ?, 0, ?, ?, ?, ?, ?)";

    private const string InsertPlayerSql =
        "insert into player(id, account_id, name, gender, last_time_login_game, diem_chien_truong_namek, point_pvp, "
        + "items_body, items_bag, items_box, data_inventory, data_point, data_task, data_luyentap, pet, vodaisinhtu, data_may_dam, "
        + "data_event_new_year, data_event_christ_mas, data_event_vulan, data_event_halloween, data_event_8_3, data_event_trung_thu, "
        + "data_event_hung_vuong, data_event_black_friday) "
        + "values (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";

    private string? _loadedDataset;

    protected override async Task SeedAsync()
    {
        await RunAsync(
            "create table account(id integer not null primary key, username citext not null, is_admin smallint not null default 0, cash integer default 0, "
            + "is_banned smallint not null default 0, vnd bigint default 0, coin bigint default 0, topped_up_amount bigint not null default 0)");
        await RunAsync(
            "create table player(id integer not null primary key, account_id integer, name citext not null, gender integer not null, "
            + "items_body text not null, items_bag text not null, items_box text not null, data_inventory text not null, data_point text not null, "
            + "data_task text not null, data_luyentap text not null, pet text not null, vodaisinhtu varchar(255) not null default '[]', "
            + "data_may_dam varchar(255) not null default '[0,0,0]', data_event_new_year varchar(100) not null default '[0,0,0,0]', "
            + "data_event_christ_mas varchar(100) not null default '[0,0,0,0]', data_event_vulan varchar(100) not null default '[0,0,0]', "
            + "data_event_halloween varchar(100) not null default '[0,0]', data_event_8_3 varchar(100) not null default '[0,0]', "
            + "data_event_trung_thu varchar(100) not null default '[0,0]', data_event_hung_vuong varchar(100) not null default '[0,0,0,0,0,0]', "
            + "data_event_black_friday varchar(100) not null default '[0,0]', diem_chien_truong_namek integer not null default 0, "
            + "point_pvp integer default 0, last_time_login_game timestamptz not null default now())");
        await RunAsync("create unique index ux_player_account_id on player (account_id)");
        await RunAsync("create table event(id integer generated by default as identity primary key, name citext not null, data text not null)");
    }

    /// <summary>Empties the ranking tables.</summary>
    public Task ResetAsync()
    {
        _loadedDataset = null;
        return RunAsync("truncate table player, account");
    }

    public async Task LoadAsync(IEnumerable<TopPlayer> players)
    {
        foreach (var p in players)
        {
            await Db.ExecuteUpdateAsync(InsertAccountSql, p.AccountId, "u" + p.Name, p.Cash, p.IsBanned, p.Vnd, p.Coin, p.ToppedUpAmount);
            var args = new List<object?> { p.Id, p.AccountId, p.Name, p.Gender, p.LastLogin, p.NamekWar, p.PointPvp };
            args.AddRange(TopPlayer.TextColumns.Select(c => (object?)p.Text(c)));
            await Db.ExecuteUpdateAsync(InsertPlayerSql, args.ToArray());
        }
    }

    /// <summary>Loads a golden dataset once and keeps it while consecutive tests use the same one (xUnit runs the tests of a class one after another).</summary>
    public async Task EnsureDatasetAsync(string name, IReadOnlyList<TopPlayer> players)
    {
        if (_loadedDataset == name)
        {
            return;
        }

        await RunAsync("truncate table player, account");
        await LoadAsync(players);
        _loadedDataset = name;
    }

    public static string FormatTimestamp(DateTime? time)
        => time is { } value ? value.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) : "NULL";
}
