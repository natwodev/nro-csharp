using Nro.Data;
using Npgsql;

namespace Nro.Tests.Db;

/// <summary>
/// Integration tests on a REAL PostgreSQL (the <c>nro_test</c> database, this class's own schema): parameterised queries, NULL, the types,
/// the row count of ExecuteUpdate, the <c>insert ... ()</c> quirk, <c>table.column</c> keys from the OID, code injection, parallel execution, a small pool.
/// </summary>
public class NroDatabaseQueryTests : IClassFixture<PeopleFixture>, IAsyncLifetime
{
    private readonly PeopleFixture _fixture;

    public NroDatabaseQueryTests(PeopleFixture fixture)
    {
        _fixture = fixture;
    }

    private NroDatabase Db => _fixture.Db;

    public Task InitializeAsync() => _fixture.ResetAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Connects_to_the_nro_test_database_and_the_class_own_schema()
    {
        var rs = await Db.ExecuteQueryAsync("select current_database() as db, current_schema() as sch");

        Assert.True(rs.First());
        Assert.Equal(TestDatabase.RequiredDatabase, rs.GetString("db"));
        Assert.Equal(_fixture.Schema, rs.GetString("sch"));
        Assert.Matches("^t_[0-9a-f]{32}$", _fixture.Schema);
    }

    [Fact]
    public async Task Query_with_positional_parameters()
    {
        await Db.ExecuteUpdateAsync("insert into people(id, name, age) values (?, ?, ?)", 1, "An", (short)7);
        await Db.ExecuteUpdateAsync("insert into people(id, name, age) values (?, ?, ?)", 2, "Bình", (short)12);
        await Db.ExecuteUpdateAsync("insert into people(id, name, age) values (?, ?, ?)", 3, "Cường", (short)30);

        var rs = await Db.ExecuteQueryAsync("select id, name from people where age >= ? and name <> ? order by id", (short)10, "Cường");

        Assert.Equal(1, rs.GetRows());
        Assert.True(rs.Next());
        Assert.Equal(2, rs.GetInt("id"));
        Assert.Equal("Bình", rs.GetString("name"));
        Assert.False(rs.Next());
    }

    [Fact]
    public async Task Empty_result_is_a_snapshot_with_no_rows_not_an_error()
    {
        var rs = await Db.ExecuteQueryAsync("select id from people where id = ?", 12345);

        Assert.Equal(0, rs.GetRows());
        Assert.False(rs.Next());
        Assert.False(rs.First());
    }

    [Fact]
    public async Task NULL_values_follow_Java_semantics()
    {
        await Db.ExecuteUpdateAsync("insert into people(id, name, age, level, score, ratio, vip, born, amount, note) values()",
            1, null, null, null, null, null, null, null, null, null);

        var rs = await Db.ExecuteQueryAsync("select * from people where id = ?", 1);
        Assert.True(rs.First());

        foreach (var column in new[] { "name", "age", "level", "score", "ratio", "vip", "born", "amount", "note" })
        {
            Assert.Null(rs.GetObject(column));
            Assert.Equal("null", rs.GetString(column));
            Assert.Null(rs.GetTimestamp(column));
        }

        Assert.Throws<NullReferenceException>(() => rs.GetInt("age"));
        Assert.Throws<NullReferenceException>(() => rs.GetByte("age"));
        Assert.Throws<NullReferenceException>(() => rs.GetShort("age"));
        Assert.Throws<NullReferenceException>(() => rs.GetLong("level"));
        Assert.Throws<NullReferenceException>(() => rs.GetDouble("score"));
        Assert.Throws<NullReferenceException>(() => rs.GetFloat("ratio"));
        Assert.Throws<NullReferenceException>(() => rs.GetBoolean("vip"));
        Assert.Throws<NullReferenceException>(() => rs.GetLong(4));
    }

    [Fact]
    public async Task Types_smallint_int_bigint_text_timestamp_boolean_double()
    {
        await Db.ExecuteUpdateAsync("insert into people(id, name, age, level, score, ratio, vip, born, amount, note) values()",
            1, "Ngọc Rồng", (short)7, 9_000_000_000L, 2.5d, 1.5f, true, new DateTime(2024, 5, 6, 7, 8, 9, 123), 12.50m, null);

        var rs = await Db.ExecuteQueryAsync("select * from people where id = ?", 1);
        Assert.True(rs.First());

        // integer: GetInt by name needs an int
        Assert.Equal(1, rs.GetInt("id"));
        Assert.IsType<int>(rs.GetObject("id"));

        // text, including Vietnamese diacritics
        Assert.Equal("Ngọc Rồng", rs.GetString("name"));

        // smallint: like JDBC, returns int (Integer) so getByte/getShort/getInt by name all work like the Java code
        Assert.IsType<int>(rs.GetObject("age"));
        Assert.Equal((sbyte)7, rs.GetByte("age"));
        Assert.Equal((short)7, rs.GetShort("age"));
        Assert.Equal(7, rs.GetInt("age"));

        // bigint
        Assert.IsType<long>(rs.GetObject("level"));
        Assert.Equal(9_000_000_000L, rs.GetLong("level"));
        Assert.Throws<InvalidCastException>(() => rs.GetInt("level"));

        // double precision and real: not swapped
        Assert.Equal(2.5d, rs.GetDouble("score"));
        Assert.Equal(1.5f, rs.GetFloat("ratio"));
        Assert.Throws<InvalidCastException>(() => rs.GetDouble("ratio"));
        Assert.Throws<InvalidCastException>(() => rs.GetFloat("score"));

        // boolean
        Assert.True(rs.GetBoolean("vip"));
        Assert.Equal("true", rs.GetString("vip"));

        // timestamp (without time zone): DateTime Kind=Unspecified, keeps the wall-clock time that was written
        var born = rs.GetTimestamp("born");
        Assert.Equal(new DateTime(2024, 5, 6, 7, 8, 9, 123), born);
        Assert.Equal(DateTimeKind.Unspecified, born!.Value.Kind);
        Assert.Equal("2024-05-06 07:08:09.123", rs.GetString("born"));

        // numeric
        Assert.Equal(12.50m, rs.GetObject("amount"));
        Assert.Equal("12.50", rs.GetString("amount"));

        // NULL
        Assert.Null(rs.GetObject("note"));
        Assert.Equal("null", rs.GetString("note"));

        // by index: GetInt(index) needs a long (an asymmetry of the Java version), GetLong(index) is correct for bigint
        Assert.Throws<InvalidCastException>(() => rs.GetInt(1));
        Assert.Equal(9_000_000_000L, rs.GetLong(4));
        Assert.Equal("Ngọc Rồng", rs.GetString(2));
        Assert.Equal(1.5f, rs.GetFloat(6));
    }

    [Fact]
    public async Task COUNT_is_bigint_so_GetLong_by_index_and_GetInt_by_index_both_work()
    {
        await Db.ExecuteUpdateAsync("insert into people(id) values (?)", 1);
        await Db.ExecuteUpdateAsync("insert into people(id) values (?)", 2);

        var rs = await Db.ExecuteQueryAsync("select count(*) from people");

        Assert.True(rs.First());
        Assert.Equal(2L, rs.GetLong(1));    // like Java ServerManager.getNumPlayer
        Assert.Equal(2, rs.GetInt(1));      // (int)(long)
        Assert.Equal(2L, rs.GetLong("count"));
    }

    [Fact]
    public async Task ExecuteUpdate_returns_the_exact_number_of_affected_rows()
    {
        Assert.Equal(1, await Db.ExecuteUpdateAsync("insert into people(id, name, age) values (?, ?, ?)", 1, "a", (short)1));
        Assert.Equal(1, await Db.ExecuteUpdateAsync("insert into people(id, name, age) values (?, ?, ?)", 2, "b", (short)1));
        Assert.Equal(1, await Db.ExecuteUpdateAsync("insert into people(id, name, age) values (?, ?, ?)", 3, "c", (short)2));

        Assert.Equal(2, await Db.ExecuteUpdateAsync("update people set name = ? where age = ?", "z", (short)1));
        Assert.Equal(0, await Db.ExecuteUpdateAsync("update people set name = ? where id = ?", "z", 999));
        Assert.Equal(1, await Db.ExecuteUpdateAsync("delete from people where id = ?", 3));
        Assert.Equal(2, await Db.ExecuteUpdateAsync("delete from people"));
        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select count(*) from people"));
    }

    [Fact]
    public async Task ExecuteUpdate_with_DDL_returns_0_like_JDBC_not_the_minus_1_of_Npgsql()
    {
        Assert.Equal(0, await Db.ExecuteUpdateAsync("create table ddl_probe(i integer)"));
        Assert.Equal(0, await Db.ExecuteUpdateAsync("drop table ddl_probe"));
    }

    [Fact]
    public async Task Insert_quirk_empty_values_parentheses_are_rewritten_into_a_parameter_list()
    {
        Assert.Equal(1, await Db.ExecuteUpdateAsync("insert into pets(id, owner_id, name) values()", 10, 1, "Mimi"));
        Assert.Equal(1, await Db.ExecuteUpdateAsync("insert into pets values()", 11, 1, "Kitty"));   // all columns in table order

        Assert.Equal(2L, await _fixture.ScalarAsync<long>("select count(*) from pets"));
        Assert.Equal("Mimi", await _fixture.ScalarAsync<string>("select name from pets where id = $1", 10));
        Assert.Equal("Kitty", await _fixture.ScalarAsync<string>("select name from pets where id = $1", 11));
    }

    [Fact]
    public async Task Insert_quirk_does_not_apply_when_the_conditions_are_not_met_so_a_missing_placeholder_is_an_error()
    {
        // an upper-case INSERT is not rewritten (case-sensitive comparison): 0 placeholders but 3 arguments
        var upper = await Assert.ThrowsAsync<SqlPlaceholderMismatchException>(
            () => Db.ExecuteUpdateAsync("INSERT INTO pets(id, owner_id, name) VALUES()", 10, 1, "Mimi"));
        Assert.Equal(0, upper.PlaceholderCount);
        Assert.Equal(3, upper.ArgumentCount);

        // trailing semicolon: does not end with "()"
        await Assert.ThrowsAsync<SqlPlaceholderMismatchException>(
            () => Db.ExecuteUpdateAsync("insert into pets(id, owner_id, name) values();", 10, 1, "Mimi"));

        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select count(*) from pets"));
    }

    [Fact]
    public async Task ExecuteQuery_has_no_insert_quirk_same_as_the_Java_version()
    {
        // Java only rewrites in executeUpdate(String, Object...). With executeQuery, "values()" stays as is, so 0 placeholders against 3 arguments.
        await Assert.ThrowsAsync<SqlPlaceholderMismatchException>(
            () => Db.ExecuteQueryAsync("insert into pets(id, owner_id, name) values() returning id", 10, 1, "Mimi"));

        Assert.Equal(0L, await _fixture.ScalarAsync<long>("select count(*) from pets"));
    }

    [Fact]
    public async Task ExecuteQuery_with_a_statement_that_returns_no_result_set_throws_like_JDBC_executeQuery()
    {
        await Db.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 1, "a");

        await Assert.ThrowsAsync<InvalidOperationException>(() => Db.ExecuteQueryAsync("update people set name = ? where id = ?", "b", 1));

        // INSERT ... RETURNING has a result set, so it works with ExecuteQuery
        var rs = await Db.ExecuteQueryAsync("insert into pets(id, owner_id, name) values (?, ?, ?) returning id", 5, 1, "Rex");
        Assert.True(rs.First());
        Assert.Equal(5, rs.GetInt("id"));
    }

    [Fact]
    public async Task Table_column_key_comes_from_the_OID_the_real_table_name_not_the_alias()
    {
        await Db.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 1, "An");
        await Db.ExecuteUpdateAsync("insert into pets(id, owner_id, name) values (?, ?, ?)", 10, 1, "Mimi");

        var rs = await Db.ExecuteQueryAsync("select p.id, p.name, t.id, t.name, count(*) over () as cnt, 1 + 1 as two from people p join pets t on t.owner_id = p.id");

        Assert.True(rs.First());
        Assert.Equal(1, rs.GetInt("people.id"));       // the REAL table name (MySQL reports the alias "p")
        Assert.Equal(10, rs.GetInt("pets.id"));
        Assert.Equal(10, rs.GetInt("id"));              // duplicate key: the LAST column wins, like HashMap.put
        Assert.Equal("An", rs.GetString("people.name"));
        Assert.Equal("Mimi", rs.GetString("pets.name"));
        Assert.Equal("Mimi", rs.GetString("name"));
        Assert.Equal(1L, rs.GetLong("cnt"));            // expression: only the "cnt" key exists
        Assert.Equal(2, rs.GetInt("two"));
        Assert.Null(rs.GetObject(".cnt"));
        Assert.Null(rs.GetObject("p.id"));
        Assert.Equal(1L, rs.GetLong(5));
        Assert.Equal(1, rs.GetObject(1));
        Assert.Equal(10, rs.GetObject(3));
    }

    [Fact]
    public async Task Table_column_key_of_a_view_is_the_view_name_and_of_a_subquery_is_the_base_table()
    {
        await Db.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 1, "An");

        var view = await Db.ExecuteQueryAsync("select * from v_people");
        Assert.True(view.First());
        Assert.Equal(1, view.GetInt("v_people.id"));
        Assert.Equal(1, view.GetInt("id"));

        var sub = await Db.ExecuteQueryAsync("select * from (select id, name from people) as sub");
        Assert.True(sub.First());
        Assert.Equal(1, sub.GetInt("people.id"));
        Assert.Equal("An", sub.GetString("people.name"));
        Assert.Null(sub.GetObject("sub.id"));
    }

    [Fact]
    public async Task Aliased_column_uses_the_column_label_as_the_key()
    {
        await Db.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 1, "An");

        var rs = await Db.ExecuteQueryAsync("select id as person_id, name from people");

        Assert.True(rs.First());
        Assert.Equal(1, rs.GetInt("person_id"));
        Assert.Equal(1, rs.GetInt("people.person_id"));
        Assert.Null(rs.GetObject("id"));   // the key is the column label, not the original column name
    }

    [Fact]
    public async Task Table_name_is_cached_by_OID_and_needs_no_second_connection_when_the_pool_has_only_1_connection()
    {
        await Db.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 1, "An");
        await Db.ExecuteUpdateAsync("insert into pets(id, owner_id, name) values (?, ?, ?)", 10, 1, "Mimi");

        // Maximum Pool Size=1: if the table name were fetched with CommandBehavior.KeyInfo (Npgsql opens a second connection), the line below would report "pool exhausted".
        await using var single = _fixture.CreateDatabase(configureConnection: c =>
        {
            c.MaxPoolSize = 1;
            c.Timeout = 5;
        });

        var rs = await single.ExecuteQueryAsync("select people.id, pets.id from people join pets on pets.owner_id = people.id");
        Assert.True(rs.First());
        Assert.Equal(1, rs.GetInt("people.id"));
        Assert.Equal(10, rs.GetInt("pets.id"));

        // later calls use the OID cache, and many concurrent queries queue up on the single connection
        var counts = await Task.WhenAll(Enumerable.Range(0, 12).Select(async _ =>
        {
            var r = await single.ExecuteQueryAsync("select people.id from people");
            return r.GetRows();
        }));
        Assert.All(counts, n => Assert.Equal(1, n));

        // inside a transaction it also uses only the transaction's own connection
        var inTransaction = await single.InTransactionAsync(async tx =>
        {
            var r = await tx.ExecuteQueryAsync("select pets.id, people.id from pets join people on pets.owner_id = people.id");
            return r.First() ? r.GetInt("people.id") : -1;
        });
        Assert.Equal(1, inTransaction);
    }

    [Fact]
    public async Task Npgsql_BaseTableName_needs_KeyInfo_and_KeyInfo_needs_a_second_connection_so_the_OID_is_used()
    {
        // Verifies TableNameResolver's assumption about Npgsql 10.0.3 (if Npgsql changes behaviour, this test goes red so the design gets reviewed).
        await using var raw = _fixture.CreateRawDataSource(c =>
        {
            c.MaxPoolSize = 1;
            c.Timeout = 2;
        });

        // (1) Without KeyInfo: TableOID is available immediately (no extra connection) but BaseTableName is empty.
        await using (var connection = await raw.OpenConnectionAsync())
        {
            await using var command = new NpgsqlCommand("select id, name, 1 as one from people", connection);
            await using var reader = await command.ExecuteReaderAsync();
            var columns = reader.GetColumnSchema();
            Assert.NotEqual(0u, columns[0].TableOID);
            Assert.Equal(columns[0].TableOID, columns[1].TableOID);
            Assert.Equal(0u, columns[2].TableOID);       // expression
            Assert.Null(columns[0].BaseTableName);
        }

        // (2) With KeyInfo, BaseTableName is available, but Npgsql opens an extra connection from the pool: with a 1-connection pool it is exhausted.
        await using (var connection = await raw.OpenConnectionAsync())
        {
            await using var command = new NpgsqlCommand("select id, name from people", connection);
            await using var reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.KeyInfo);
            var error = Assert.ThrowsAny<NpgsqlException>(() => reader.GetColumnSchema());
            Assert.Contains("pool", error.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Runs_20_updates_and_20_queries_in_parallel()
    {
        await Task.WhenAll(Enumerable.Range(1, 20).Select(
            i => Db.ExecuteUpdateAsync("insert into people(id, name, age) values()", i, $"p{i}", (short)i)));

        Assert.Equal(20L, await _fixture.ScalarAsync<long>("select count(*) from people"));

        var names = await Task.WhenAll(Enumerable.Range(1, 20).Select(async i =>
        {
            var rs = await Db.ExecuteQueryAsync("select people.id, name from people where id = ?", i);
            return rs.First() ? $"{rs.GetInt("people.id")}:{rs.GetString("name")}" : "(no row)";
        }));

        Assert.Equal(Enumerable.Range(1, 20).Select(i => $"{i}:p{i}"), names);
    }

    [Fact]
    public async Task Thirty_parallel_statements_on_a_pool_of_only_3_connections_do_not_get_stuck()
    {
        await Db.ExecuteUpdateAsync("insert into people(id, name) values (?, ?)", 1, "An");

        await using var small = _fixture.CreateDatabase(configureConnection: c =>
        {
            c.MaxPoolSize = 3;
            c.Timeout = 20;
        });

        var results = await Task.WhenAll(Enumerable.Range(0, 30).Select(async _ =>
        {
            var rs = await small.ExecuteQueryAsync("select people.id, name from people");
            return rs.First() && rs.GetInt("people.id") == 1;
        }));

        Assert.All(results, ok => Assert.True(ok));
    }

    [Fact]
    public async Task String_containing_SQL_code_is_only_stored_as_data()
    {
        await Db.ExecuteUpdateAsync("create table x(i integer)");
        const string evil = "'; DROP TABLE x; --";

        await Db.ExecuteUpdateAsync("insert into notes(body) values(?)", evil);

        Assert.Equal(1L, await _fixture.ScalarAsync<long>("select count(*) from pg_tables where schemaname = current_schema() and tablename = 'x'"));
        Assert.Equal(evil, await _fixture.ScalarAsync<string>("select body from notes"));

        var found = await Db.ExecuteQueryAsync("select body from notes where body = ?", evil);
        Assert.True(found.First());
        Assert.Equal(evil, found.GetString("body"));

        // even when the string is in the query's condition
        var none = await Db.ExecuteQueryAsync("select * from notes where body = ?", "x' or '1'='1");
        Assert.Equal(0, none.GetRows());
    }

    [Fact]
    public async Task Raw_connection_is_usable_and_returns_to_the_pool_when_disposed()
    {
        await using (var connection = await Db.OpenConnectionAsync())
        {
            await using var command = new NpgsqlCommand("select 1", connection);
            Assert.Equal(1, (int)(await command.ExecuteScalarAsync())!);
        }

        // if the connection were not returned to the pool, a 1-connection pool would be exhausted on the second open
        await using var single = _fixture.CreateDatabase(configureConnection: c =>
        {
            c.MaxPoolSize = 1;
            c.Timeout = 3;
        });
        for (var i = 0; i < 3; i++)
        {
            await using var connection = await single.OpenConnectionAsync();
            await using var command = new NpgsqlCommand("select 1", connection);
            Assert.Equal(1, (int)(await command.ExecuteScalarAsync())!);
        }
    }

    [Fact]
    public async Task CancellationToken_passed_by_mistake_into_params_is_blocked_with_a_clear_message()
    {
        var error = await Assert.ThrowsAsync<ArgumentException>(() => Db.ExecuteQueryAsync("select ?", (object)CancellationToken.None));

        Assert.Contains("CancellationToken", error.Message);

        // correct overloads: (sql, token) and (sql, args, token)
        using var source = new CancellationTokenSource();
        var rs = await Db.ExecuteQueryAsync("select 1 as one", source.Token);
        Assert.True(rs.First());
        Assert.Equal(1, rs.GetInt("one"));
        var rs2 = await Db.ExecuteQueryAsync("select ?::int as two", [2], source.Token);
        Assert.True(rs2.First());
        Assert.Equal(2, rs2.GetInt("two"));
    }

    [Fact]
    public async Task String_parameters_are_untyped_so_citext_compares_case_insensitively_like_MySQL_ci_collation()
    {
        await Db.ExecuteUpdateAsync("insert into accounts(id, username) values (?, ?)", 1, "Admin");

        // Java/MySQL: select ... where username = ? matches "admin" with "Admin" (collation _ci). With citext this must be preserved.
        var rs = await Db.ExecuteQueryAsync("select id from accounts where username = ?", "admin");
        Assert.True(rs.First());
        Assert.Equal(1, rs.GetInt("id"));

        var upper = await Db.ExecuteQueryAsync("select id from accounts where username = ?", "ADMIN");
        Assert.Equal(1, upper.GetRows());

        // Npgsql's default behaviour, which this class deliberately avoids: a string sent as text makes citext = text compare case-sensitively.
        await using var connection = await Db.OpenConnectionAsync();
        await using var typed = new NpgsqlCommand("select count(*) from accounts where username = $1", connection);
        typed.Parameters.Add(new NpgsqlParameter { Value = "admin" });
        Assert.Equal(0L, (long)(await typed.ExecuteScalarAsync())!);
    }

    [Fact]
    public async Task String_parameters_are_coerced_to_the_column_type_like_MySQL_number_boolean_timestamp_jsonb()
    {
        await Db.ExecuteUpdateAsync(
            "insert into accounts(id, username, active, level, born, profile) values (?, ?, ?, ?, ?, ?)",
            "7", "ngoc", "true", "12", "2024-05-06 07:08:09", "{\"gold\": 5}");

        var rs = await Db.ExecuteQueryAsync("select * from accounts where id = ? and active = ? and level = ?", "7", "true", "12");

        Assert.True(rs.First());
        Assert.Equal(7, rs.GetInt("id"));
        Assert.True(rs.GetBoolean("active"));
        Assert.Equal(12, rs.GetInt("level"));
        Assert.Equal(new DateTime(2024, 5, 6, 7, 8, 9), rs.GetTimestamp("born"));
        Assert.Equal("{\"gold\": 5}", rs.GetString("profile"));   // jsonb normalises whitespace: PostgreSQL prints {"gold": 5}
    }

    [Fact]
    public async Task String_that_cannot_be_coerced_into_a_numeric_column_is_still_a_clear_error_not_swallowed()
    {
        var error = await Assert.ThrowsAsync<PostgresException>(() => Db.ExecuteUpdateAsync("insert into accounts(id) values (?)", "abc"));

        Assert.Equal(PostgresErrorCodes.InvalidTextRepresentation, error.SqlState);
    }

    [Fact]
    public async Task String_parameters_are_unknown_so_statements_that_let_PostgreSQL_infer_the_type_must_cast_explicitly()
    {
        // KNOWN LIMITATION of sending strings as "unknown" (see README.md and DbArguments.ToParameters): when there is no type context
        // (comparison with NULL, passing to pg_typeof, to_jsonb, jsonb_build_object ...) PostgreSQL cannot infer the parameter's type.
        // DAOs must cast explicitly: ?::text, ?::jsonb ... Both directions are pinned so anyone changing DbArguments has to update the documentation on purpose.
        foreach (var sql in new[] { "select ? is null as r", "select pg_typeof(?) as r", "select to_jsonb(?) as r", "select jsonb_build_object('a', ?) as r" })
        {
            var error = await Assert.ThrowsAsync<PostgresException>(() => Db.ExecuteQueryAsync(sql, "abc"));
            // the message and SQLSTATE differ by function: "could not determine data type of parameter $1" (42P18) or
            // "could not determine polymorphic type because input has type unknown" (42804)
            Assert.Contains(error.SqlState, new[] { PostgresErrorCodes.IndeterminateDatatype, PostgresErrorCodes.DatatypeMismatch });
            Assert.StartsWith("could not determine", error.MessageText, StringComparison.Ordinal);
        }

        var isNull = await Db.ExecuteQueryAsync("select ?::text is null as r", "abc");
        Assert.True(isNull.First());
        Assert.False(isNull.GetBoolean("r"));

        var typeOf = await Db.ExecuteQueryAsync("select pg_typeof(?::text)::text as r", "abc");
        Assert.True(typeOf.First());
        Assert.Equal("text", typeOf.GetString("r"));

        var json = await Db.ExecuteQueryAsync("select to_jsonb(?::text)::text as a, jsonb_build_object('a', ?::text)::text as b", "abc", "abc");
        Assert.True(json.First());
        Assert.Equal("\"abc\"", json.GetString("a"));
        Assert.Equal("{\"a\": \"abc\"}", json.GetString("b"));

        // With a type context no cast is needed: comparison with a column, jsonb_exists(jsonb, text), coalesce with a column...
        await Db.ExecuteUpdateAsync("insert into accounts(id, username, profile) values (?, ?, ?::jsonb)", 1, "ngoc", "{\"gold\": 5}");
        var withContext = await Db.ExecuteQueryAsync("select id from accounts where jsonb_exists(profile, ?) and username = ?", "gold", "NGOC");
        Assert.True(withContext.First());
        Assert.Equal(1, withContext.GetInt("id"));
    }

    [Fact]
    public async Task Real_number_and_boolean_parameters_keep_their_strict_type()
    {
        // C# number/boolean types are still sent with the right type: they are not treated as strings
        await Assert.ThrowsAsync<PostgresException>(() => Db.ExecuteUpdateAsync("insert into accounts(id, active) values (?, ?)", 1, 1));
        Assert.Equal(1, await Db.ExecuteUpdateAsync("insert into accounts(id, active) values (?, ?)", 2, true));
    }

    [Fact]
    public async Task Login_sample_like_GodGK_login_reads_correctly_with_the_Java_getters()
    {
        // The same calls that GodGK.login (Java) uses, on the PostgreSQL columns the data model plans: serial, citext, boolean, smallint, bigint, timestamp.
        var logged = new DateTime(2024, 5, 6, 7, 8, 9);
        await Db.ExecuteUpdateAsync(
            "insert into account(username, password, is_jail, active, vnd, bd_player, last_time_logout, gender, vethang_expire, Vip_Point) values()",
            "Admin", "pw", false, true, 100, 1.5d, logged, (short)2, 1_700_000_000_000L, 7);

        var rs = await Db.ExecuteQueryAsync("select * from account where username = ? and password = ?", "admin", "pw");

        Assert.True(rs.First());                                   // exactly one row: first() is true like the real login code
        Assert.Equal(1, rs.GetInt("account.id"));                  // serial = integer
        Assert.False(rs.GetBoolean("is_jail"));
        Assert.True(rs.GetBoolean("active"));
        Assert.Equal(100, rs.GetInt("vnd"));
        Assert.Equal(1.5d, rs.GetDouble("account.bd_player"));
        Assert.Equal(logged, rs.GetTimestamp("last_time_logout"));
        Assert.Equal((sbyte)2, rs.GetByte("gender"));              // smallint becomes int so getByte/getShort work like MySQL TINYINT
        Assert.Equal((short)2, rs.GetShort("gender"));
        Assert.Equal(1_700_000_000_000L, rs.GetLong("vethang_expire"));
        Assert.Equal(7, rs.GetInt("Vip_Point"));                   // Java's mixed-case column names can still be looked up
        Assert.Equal("null", rs.GetString("nickname"));            // NULL becomes "null" like String.valueOf

        var wrong = await Db.ExecuteQueryAsync("select * from account where username = ? and password = ?", "admin", "sai-mat-khau");
        Assert.False(wrong.First());                               // no row at all: first() is false
    }

    [Fact]
    public async Task Timestamp_without_time_zone_written_and_read_back_keeps_the_wall_clock_time()
    {
        var moment = new DateTime(2024, 12, 31, 23, 59, 59, 999, DateTimeKind.Unspecified);
        await Db.ExecuteUpdateAsync("insert into people(id, born) values (?, ?)", 1, moment);

        var rs = await Db.ExecuteQueryAsync("select born from people where id = ?", 1);

        Assert.True(rs.First());
        Assert.Equal(moment, rs.GetTimestamp("born"));
        Assert.Equal(moment, rs.GetTimestamp(1));
    }
}
