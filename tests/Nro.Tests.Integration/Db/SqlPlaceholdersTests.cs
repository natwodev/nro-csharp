using Nro.Data.Db;

namespace Nro.Tests.Integration.Db;

/// <summary>
/// Pure tests (no database needed) for the <c>?</c> to <c>$1..$n</c> translator and the <c>insert ... ()</c> quirk of <c>ConnectDB.executeUpdate</c>.
/// </summary>
public class SqlPlaceholdersTests
{
    /// <summary>(SQL in, argument count, expected SQL out). Over 35 cases: strings, identifiers, comments, E-strings, dollar-quoting, adjacency.</summary>
    public static TheoryData<string, int, string> TranslateCases => new()
    {
        // no parameters / basics
        { "select 1", 0, "select 1" },
        { "", 0, "" },
        { "select * from t where a = ?", 1, "select * from t where a = $1" },
        { "select ?, ?, ?", 3, "select $1, $2, $3" },
        { "update t set a=?,b=? where id=?", 3, "update t set a=$1,b=$2 where id=$3" },
        { "insert into t values(?); select ?", 2, "insert into t values($1); select $2" },

        // a ? inside a string '...'
        { "select '?'", 0, "select '?'" },
        { "select 'it''s ?', ?", 1, "select 'it''s ?', $1" },
        { "select 'a' || ? || 'b?'", 1, "select 'a' || $1 || 'b?'" },
        { "select ''''", 0, "select ''''" },
        { "select 'Ngọc Rồng ?', ?", 1, "select 'Ngọc Rồng ?', $1" },
        { "select '--', ?", 1, "select '--', $1" },
        { "select '/*', ?", 1, "select '/*', $1" },
        { "select 'abc ?", 0, "select 'abc ?" },

        // identifier "..."
        { "select \"col?\" from t where x = ?", 1, "select \"col?\" from t where x = $1" },
        { "select \"a\"\"b?\", ?", 1, "select \"a\"\"b?\", $1" },

        // line and block comments (block comments nest, as in PostgreSQL)
        { "select ? -- comment ? here\n, ?", 2, "select $1 -- comment ? here\n, $2" },
        { "select ? -- x ?\r\n, ?", 2, "select $1 -- x ?\r\n, $2" },
        { "select 1 -- only ?", 0, "select 1 -- only ?" },
        { "select /* ? */ ?", 1, "select /* ? */ $1" },
        { "select /* a /* ? */ ? */ ?", 1, "select /* a /* ? */ ? */ $1" },
        { "select /* ?", 0, "select /* ?" },
        { "select ?/*c*/?", 2, "select $1/*c*/$2" },

        // E-string (escaped with \) and a regular string (\ is an ordinary character)
        { "select E'it\\'s ?', ?", 1, "select E'it\\'s ?', $1" },
        { "select e'\\\\', ?", 1, "select e'\\\\', $1" },
        { "select 'a\\', ?", 1, "select 'a\\', $1" },

        // dollar-quote
        { "select $$ ? $$, ?", 1, "select $$ ? $$, $1" },
        { "select $tag$ ? $$ ? $tag$, ?", 1, "select $tag$ ? $$ ? $tag$, $1" },
        { "select a$b, ?", 1, "select a$b, $1" },
        { "select $1, 5", 0, "select $1, 5" },

        // adjacency and operators
        { "select ?::int, ?", 2, "select $1::int, $2" },
        { "select ?||?", 2, "select $1||$2" },
        { "where a=?and b=?", 2, "where a=$1 and b=$2" },
        { "select 5 - ?, 6 / ?", 2, "select 5 - $1, 6 / $2" },
        { "select 2 * ?", 1, "select 2 * $1" },

        // identifier ending in $: a "$" right after an identifier does not open a dollar-quoted string (a mutation removing '$' from IsIdentifierPart turns this case red)
        { "select x$$ , ?", 1, "select x$$ , $1" },
        { "select x$$$ , ?", 1, "select x$$$ , $1" },
        { "select x$$$ y$$$, ?", 1, "select x$$$ y$$$, $1" },

        // KNOWN LIMITATION: the jsonb operators ?, ?|, ?& are treated as parameters (see the remarks of SqlPlaceholders). This behaviour is pinned: if anyone
        // changes the translator to understand those operators, they must update the case below on purpose; use jsonb_exists/_any/_all instead.
        { "select j ? 'a' from t", 1, "select j $1 'a' from t" },
        { "select j ?| array['a','b'] from t", 1, "select j $1| array['a','b'] from t" },
        { "select j ?& array['a','b'] from t", 1, "select j $1& array['a','b'] from t" },
        { "select j ? ?, 1", 2, "select j $1 $2, 1" },
        { "select jsonb_exists(j, ?) from t", 1, "select jsonb_exists(j, $1) from t" },
        { "select jsonb_exists_any(j, ?::text[]), jsonb_exists_all(j, ?::text[]) from t", 2, "select jsonb_exists_any(j, $1::text[]), jsonb_exists_all(j, $2::text[]) from t" },
    };

    [Theory]
    [MemberData(nameof(TranslateCases))]
    public void Translates_question_marks_into_positional_parameters(string sql, int argumentCount, string expected)
    {
        Assert.Equal(expected, SqlPlaceholders.Translate(sql, argumentCount));
        Assert.Equal(argumentCount, SqlPlaceholders.Count(sql));
    }

    [Fact]
    public void Translates_100_parameters()
    {
        var sql = "select " + string.Join(",", Enumerable.Repeat("?", 100));
        var expected = "select " + string.Join(",", Enumerable.Range(1, 100).Select(i => "$" + i));

        Assert.Equal(expected, SqlPlaceholders.Translate(sql, 100));
    }

    [Theory]
    [InlineData("select ?, ?", 1, 2, 1)]
    [InlineData("select ?", 2, 1, 2)]
    [InlineData("select ?", 0, 1, 0)]
    [InlineData("select '?'", 1, 0, 1)]
    [InlineData("select 1", 3, 0, 3)]
    [InlineData("select \"?\", /* ? */ 1 -- ?", 1, 0, 1)]
    public void Placeholder_count_different_from_argument_count_throws_a_clear_error(string sql, int argumentCount, int expectedPlaceholders, int expectedArguments)
    {
        var error = Assert.Throws<SqlPlaceholderMismatchException>(() => SqlPlaceholders.Translate(sql, argumentCount));

        Assert.Equal(expectedPlaceholders, error.PlaceholderCount);
        Assert.Equal(expectedArguments, error.ArgumentCount);
        Assert.IsAssignableFrom<ArgumentException>(error);
        Assert.Contains($"({expectedPlaceholders})", error.Message);
        Assert.Contains($"({expectedArguments})", error.Message);
        Assert.DoesNotContain("select", error.Message);   // the message contains no SQL
    }

    [Fact]
    public void Jsonb_question_mark_operator_is_a_known_limitation_so_a_missing_argument_throws_instead_of_silently_sending()
    {
        // `j ? 'a'` written PostgreSQL-style (no parameter) is counted as 1 parameter: 0 arguments is a mismatch, so a clear error, never broken SQL sent.
        var error = Assert.Throws<SqlPlaceholderMismatchException>(() => SqlPlaceholders.Translate("select j ? 'a' from t", 0));

        Assert.Equal(1, error.PlaceholderCount);
        Assert.Equal(0, error.ArgumentCount);
        Assert.Equal(1, SqlPlaceholders.Count("select j ?| array['a'] from t"));
        Assert.Equal(1, SqlPlaceholders.Count("select j ?& array['a'] from t"));
    }

    [Fact]
    public void Invalid_arguments()
    {
        Assert.Throws<ArgumentNullException>(() => SqlPlaceholders.Translate(null!, 0));
        Assert.Throws<ArgumentNullException>(() => SqlPlaceholders.Count(null!));
        Assert.Throws<ArgumentNullException>(() => SqlPlaceholders.ExpandInsertValues(null!, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SqlPlaceholders.Translate("select 1", -1));
    }

    // ------------------------------------------------------------------ insert ... () quirk

    [Theory]
    [InlineData("insert into t values()", 3, "insert into t values(?,?,?)")]
    [InlineData("insert into t() values()", 2, "insert into t(?,?) values(?,?)")]
    [InlineData("insert into t values ()", 1, "insert into t values (?)")]
    [InlineData("insert ignore into t()", 5, "insert ignore into t(?,?,?,?,?)")]
    [InlineData("insertion into x()", 2, "insertion into x(?,?)")]
    [InlineData("insert()", 2, "insert(?,?)")]
    [InlineData("insert into t () values ()", 1, "insert into t (?) values (?)")]
    public void Insert_quirk_replaces_every_empty_parentheses_pair_with_a_placeholder_list(string sql, int argumentCount, string expected)
    {
        Assert.Equal(expected, SqlPlaceholders.ExpandInsertValues(sql, argumentCount));
    }

    [Theory]
    [InlineData("INSERT INTO t VALUES()")]                  // case-sensitive: only lower-case "insert"
    [InlineData("Insert into t values()")]
    [InlineData(" insert into t values()")]                // leading whitespace: indexOf != 0
    [InlineData("insert into t values() ")]                // trailing whitespace: does not end with "()"
    [InlineData("insert into t values();")]                // trailing semicolon
    [InlineData("insert into t values(?)")]                // does not end with "()"
    [InlineData("insert into t(a, b) values(?, now())")]   // ends with "())"
    [InlineData("update t set a=()")]                       // does not start with insert
    [InlineData("select ()")]
    public void Insert_quirk_does_not_apply_when_the_conditions_are_not_met(string sql)
    {
        Assert.Equal(sql, SqlPlaceholders.ExpandInsertValues(sql, 3));
    }

    [Fact]
    public void Insert_quirk_does_not_apply_without_arguments_like_the_Java_no_argument_overload()
    {
        Assert.Equal("insert into t values()", SqlPlaceholders.ExpandInsertValues("insert into t values()", 0));
    }

    [Fact]
    public void Insert_quirk_combined_with_the_translator_gives_the_right_parameter_count()
    {
        var expanded = SqlPlaceholders.ExpandInsertValues("insert into pets(id, owner_id, name) values()", 3);

        Assert.Equal("insert into pets(id, owner_id, name) values($1,$2,$3)", SqlPlaceholders.Translate(expanded, 3));
    }

    // ------------------------------------------------------------------ comparison with the real Java ConnectDB

    [Fact]
    public void Insert_quirk_matches_real_Java_ConnectDB_in_every_golden_case()
    {
        var golden = SnapshotGolden.Load();
        Assert.True(golden.ExecuteCases.Count >= 20, $"Only {golden.ExecuteCases.Count} ConnectDB cases");

        var problems = new List<string>();
        foreach (var c in golden.ExecuteCases)
        {
            // The SQL that the REAL Java passes to prepareStatement; for failed cases it comes from the log line (Java logs the SQL after rewriting).
            var javaSql = c.Failed ? LoggedSql(c.Log) : c.Prepared.Single();

            string csharpSql = c.Api switch
            {
                "executeUpdate.varargs" => SqlPlaceholders.ExpandInsertValues(c.Sql, c.Args.Length),
                "executeUpdate.plain" => SqlPlaceholders.ExpandInsertValues(c.Sql, 0),
                "executeQuery.varargs" or "executeQuery.plain" => c.Sql,   // the query path has NO quirk (ExecuteQueryAsync does not call this function)
                _ => throw new InvalidOperationException("unknown api: " + c.Api),
            };

            if (c.Id == "u08_empty_varargs_array")
            {
                // Deliberate difference: Java produces broken SQL "(" for an explicitly empty array; C# keeps the SQL unchanged when there are no arguments.
                if (javaSql != "insert into t values(" || csharpSql != "insert into t values()")
                {
                    problems.Add($"{c.Id}: the declared divergence no longer holds (Java '{javaSql}', C# '{csharpSql}')");
                }

                continue;
            }

            if (csharpSql != javaSql)
            {
                problems.Add($"{c.Id}: Java '{javaSql}', C# '{csharpSql}'");
            }

            // When the placeholder count matches the argument count the translator must work; on a mismatch Java/JDBC also fails at setObject/execute and C# throws a clear error.
            var placeholders = SqlPlaceholders.Count(csharpSql);
            if (placeholders == c.Args.Length)
            {
                var translated = SqlPlaceholders.Translate(csharpSql, c.Args.Length);
                if (SqlPlaceholders.Count(translated) != 0)
                {
                    problems.Add($"{c.Id}: the translated SQL '{translated}' still contains a ?");
                }
            }
            else if (!Throws<SqlPlaceholderMismatchException>(() => SqlPlaceholders.Translate(csharpSql, c.Args.Length)))
            {
                problems.Add($"{c.Id}: the SQL has {placeholders} ? but {c.Args.Length} arguments, yet SqlPlaceholderMismatchException was not thrown");
            }
        }

        Assert.Empty(problems);
    }

    [Fact]
    public void Java_error_log_lines_in_golden_carry_the_known_message_prefix()
    {
        // The Vietnamese prefix below is DATA: it is the exact text recorded in tests/golden/result-snapshot.json from the real Java server.
        var failing = SnapshotGolden.Load().ExecuteCases.Where(c => c.Failed).ToList();

        Assert.NotEmpty(failing);
        foreach (var c in failing)
        {
            Assert.Contains(c.Log, line => line.StartsWith("RED|Có lỗi xảy ra khi thực thi câu lệnh: ", StringComparison.Ordinal));
        }
    }

    private static bool Throws<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
            return false;
        }
        catch (TException)
        {
            return true;
        }
    }

    private static string LoggedSql(string[] log)
    {
        const string prefix = "RED|Có lỗi xảy ra khi thực thi câu lệnh: ";
        var line = log.Single(l => l.StartsWith(prefix, StringComparison.Ordinal));
        return line[prefix.Length..].TrimEnd('\n');
    }
}
