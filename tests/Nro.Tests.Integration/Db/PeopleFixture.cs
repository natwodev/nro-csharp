namespace Nro.Tests.Integration.Db;

/// <summary>Fixture that pre-builds a few small tables in the test class's own schema (every type to be tried lives in the <c>people</c> table).</summary>
public sealed class PeopleFixture : TestDatabase
{
    protected override async Task SeedAsync()
    {
        await RunAsync(
            "create table people(id integer primary key, name text, age smallint, level bigint, score double precision, "
            + "ratio real, vip boolean, born timestamp, amount numeric(10,2), note text)");
        await RunAsync("create table pets(id integer primary key, owner_id integer, name text)");
        await RunAsync("create table notes(id serial primary key, body text)");
        await RunAsync("create table uniq(id integer primary key, code text unique)");
        await RunAsync("create view v_people as select id, name from people");
        await RunAsync(
            "create table account(id serial primary key, username citext, password text, is_jail boolean, active boolean, vnd integer, "
            + "player_bd_multiplier double precision, last_time_logout timestamp, gender smallint, monthly_pass_expires_at bigint, nickname text, Vip_Point integer)");
        await RunAsync("create table accounts(id integer primary key, username citext, active boolean, level integer, born timestamp, profile jsonb)");
    }

    /// <summary>Clears the data of every table so each test starts from a clean state (tables and schema are kept).</summary>
    public Task ResetAsync() => RunAsync("truncate table people, pets, notes, uniq, accounts, account restart identity");
}
