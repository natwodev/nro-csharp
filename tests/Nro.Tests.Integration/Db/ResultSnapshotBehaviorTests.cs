using Nro.Data.Db;

namespace Nro.Tests.Integration.Db;

/// <summary>
/// Behaviours of <see cref="ResultSnapshot"/> stated by name (apart from the golden comparison suite): easy to read, and they show immediately what broke.
/// </summary>
public class ResultSnapshotBehaviorTests
{
    private static ResultSnapshot Make(IReadOnlyList<SnapshotColumn> columns, params object?[][] rows) => new(columns, rows);

    private static SnapshotColumn Col(string name, string? table = null) => new(name, table);

    [Fact]
    public void Name_keys_are_lower_case_and_include_the_table_column_key()
    {
        using var rs = Make([Col("ID", "Account"), Col("UserName", "Account")], [7, "boss"]);
        rs.Next();

        Assert.Equal(7, rs.GetInt("id"));
        Assert.Equal(7, rs.GetInt("Id"));
        Assert.Equal(7, rs.GetInt("account.id"));
        Assert.Equal(7, rs.GetInt("ACCOUNT.ID"));
        Assert.Equal("boss", rs.GetString("username"));
        Assert.Equal("boss", rs.GetString("account.username"));
    }

    [Fact]
    public void Two_columns_with_the_same_name_make_the_column_key_point_to_the_last_while_table_column_keys_stay_distinct()
    {
        using var rs = Make([Col("id", "account"), Col("id", "player")], [1, 10]);
        rs.Next();

        Assert.Equal(10, rs.GetInt("id"));
        Assert.Equal(1, rs.GetInt("account.id"));
        Assert.Equal(10, rs.GetInt("player.id"));
        Assert.Equal(1, rs.GetObject(1));    // by index both columns are still visible
        Assert.Equal(10, rs.GetObject(2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Expression_column_without_a_table_name_has_only_the_column_key(string? table)
    {
        using var rs = Make([Col("cnt", table)], [5L]);
        rs.Next();

        Assert.Equal(5L, rs.GetLong("cnt"));
        Assert.Null(rs.GetObject(".cnt"));          // Java has this key; C# deliberately does not
        Assert.Null(rs.GetObject("account.cnt"));
    }

    [Fact]
    public void Column_indexes_start_at_1()
    {
        using var rs = Make([Col("a"), Col("b")], ["x", "y"]);
        rs.Next();

        Assert.Equal("x", rs.GetString(1));
        Assert.Equal("y", rs.GetString(2));
        Assert.Throws<IndexOutOfRangeException>(() => rs.GetString(0));
        Assert.Throws<IndexOutOfRangeException>(() => rs.GetString(3));
    }

    [Fact]
    public void Data_is_copied_so_modifying_the_source_array_has_no_effect()
    {
        var source = new object?[] { 1, "a" };
        using var rs = Make([Col("id"), Col("name")], source);
        source[0] = 999;
        rs.Next();

        Assert.Equal(1, rs.GetInt("id"));
    }

    [Fact]
    public void Row_with_a_value_count_different_from_the_column_count_throws()
    {
        Assert.Throws<ArgumentException>(() => Make([Col("a"), Col("b")], [1]));
    }

    [Fact]
    public void Dispose_repeatedly_is_fine_and_afterwards_every_method_reports_no_data()
    {
        var rs = Make([Col("id")], [1]);
        rs.Next();
        rs.Dispose();
        rs.Dispose();   // Java throws NullPointerException the second time; C# is idempotent (per IDisposable)

        Assert.Equal("No data available", Assert.Throws<InvalidOperationException>(() => rs.Next()).Message);
        Assert.Equal("No data available", Assert.Throws<InvalidOperationException>(() => rs.GetInt("id")).Message);
        Assert.Equal("No data available", Assert.Throws<InvalidOperationException>(() => rs.GetRows()).Message);
        rs.GotoBeforeFirst();   // Java: the only method that does not check, still works after dispose
    }

    [Fact]
    public void Reading_before_positioning_the_cursor_reports_not_prepared()
    {
        using var rs = Make([Col("id")], [1]);

        Assert.Equal("Results need to be prepared in advance", Assert.Throws<InvalidOperationException>(() => rs.GetInt("id")).Message);
        Assert.Equal("Results need to be prepared in advance", Assert.Throws<InvalidOperationException>(() => rs.GetObject(1)).Message);
    }

    [Fact]
    public void First_does_not_go_to_the_first_row_but_advances_the_index_and_is_true_only_at_the_last_row()
    {
        using var one = Make([Col("id")], [1]);
        Assert.True(one.First());      // one row: the first call reaches the last row immediately
        Assert.False(one.First());     // the second call goes past the end

        using var three = Make([Col("id")], [1], [2], [3]);
        Assert.False(three.First());   // reaches row 0 but it is not the last row yet
        Assert.Equal(1, three.GetInt("id"));   // the cursor IS at row 0 even though false was returned
        Assert.False(three.First());
        Assert.True(three.First());    // true only when the last row is reached
        Assert.Equal(3, three.GetInt("id"));
    }

    [Fact]
    public void Next_is_not_clamped_at_the_end_and_goto_resets()
    {
        using var rs = Make([Col("id")], [1]);

        Assert.True(rs.Next());
        Assert.False(rs.Next());
        Assert.False(rs.Next());
        Assert.Throws<IndexOutOfRangeException>(() => rs.GetInt("id"));   // index 2, outside the result
        rs.GotoBeforeFirst();
        Assert.True(rs.Next());
        Assert.Equal(1, rs.GetInt("id"));
    }

    [Fact]
    public void GotoResult_checks_only_the_current_index_not_the_target()
    {
        using var rs = Make([Col("id")], [1], [2]);

        Assert.Equal("Index out of bound", Assert.Throws<InvalidOperationException>(() => rs.GotoResult(0)).Message);   // cursor not positioned yet
        rs.GotoFirst();
        Assert.True(rs.GotoResult(99));   // the target index 99 is not blocked
        Assert.Throws<IndexOutOfRangeException>(() => rs.GetInt("id"));
        Assert.Equal("Index out of bound", Assert.Throws<InvalidOperationException>(() => rs.GotoResult(0)).Message);   // now the current index 99 is invalid
    }

    [Fact]
    public void Empty_result_gotoFirst_throws_gotoLast_is_still_true_but_not_prepared()
    {
        using var rs = Make([Col("id")]);

        Assert.Equal(0, rs.GetRows());
        Assert.False(rs.Next());
        Assert.Equal("No data available", Assert.Throws<InvalidOperationException>(() => rs.GotoFirst()).Message);
        Assert.True(rs.GotoLast());
        Assert.Equal("Results need to be prepared in advance", Assert.Throws<InvalidOperationException>(() => rs.GetInt("id")).Message);
    }

    [Fact]
    public void GetInt_by_name_needs_int_while_by_index_needs_long_and_truncates()
    {
        using var rs = Make([Col("i"), Col("l")], [5, 4294967297L]);
        rs.Next();

        Assert.Equal(5, rs.GetInt("i"));
        Assert.Throws<InvalidCastException>(() => rs.GetInt("l"));
        Assert.Throws<InvalidCastException>(() => rs.GetInt(1));
        Assert.Equal(1, rs.GetInt(2));   // (int)(long)4294967297L = 1
    }

    [Fact]
    public void GetByte_and_GetShort_need_int_and_truncate_wrap()
    {
        using var rs = Make([Col("v")], [200], [128], [-129], [40000], [65536]);

        rs.Next();
        Assert.Equal((sbyte)-56, rs.GetByte("v"));
        Assert.Equal((short)200, rs.GetShort("v"));
        rs.Next();
        Assert.Equal(sbyte.MinValue, rs.GetByte(1));
        rs.Next();
        Assert.Equal(sbyte.MaxValue, rs.GetByte("v"));
        rs.Next();
        Assert.Equal((short)-25536, rs.GetShort("v"));
        rs.Next();
        Assert.Equal((short)0, rs.GetShort(1));
    }

    [Fact]
    public void GetBoolean_accepts_int_equal_to_1_and_bool_but_throws_for_other_types()
    {
        using var rs = Make([Col("v")], [1], [0], [2], [true], [false], [1L], [null]);

        rs.Next();
        Assert.True(rs.GetBoolean("v"));
        rs.Next();
        Assert.False(rs.GetBoolean("v"));
        rs.Next();
        Assert.False(rs.GetBoolean("v"));          // Integer 2 != 1
        rs.Next();
        Assert.True(rs.GetBoolean(1));
        rs.Next();
        Assert.False(rs.GetBoolean(1));
        rs.Next();
        Assert.Throws<InvalidCastException>(() => rs.GetBoolean("v"));   // Long
        rs.Next();
        Assert.Throws<NullReferenceException>(() => rs.GetBoolean("v"));
    }

    [Fact]
    public void GetString_of_NULL_is_the_string_null_while_GetObject_and_GetTimestamp_are_null()
    {
        using var rs = Make([Col("v")], [new object?[] { null }]);
        rs.Next();

        Assert.Equal("null", rs.GetString("v"));
        Assert.Equal("null", rs.GetString(1));
        Assert.Null(rs.GetObject("v"));
        Assert.Null(rs.GetTimestamp(1));
        Assert.Throws<NullReferenceException>(() => rs.GetInt("v"));
        Assert.Throws<NullReferenceException>(() => rs.GetLong(1));
        Assert.Throws<NullReferenceException>(() => rs.GetDouble("v"));
    }

    [Fact]
    public void Missing_column_by_name_behaves_like_NULL_while_by_index_is_an_index_error()
    {
        using var rs = Make([Col("id")], [1]);
        rs.Next();

        Assert.Null(rs.GetObject("nope"));
        Assert.Equal("null", rs.GetString("nope"));
        Assert.Null(rs.GetTimestamp("nope"));
        Assert.Throws<NullReferenceException>(() => rs.GetInt("nope"));
        Assert.Throws<IndexOutOfRangeException>(() => rs.GetObject(2));
        Assert.Throws<NullReferenceException>(() => rs.GetObject(null!));   // Java: column.toLowerCase() with a null column throws NPE
    }

    // ------------------------------------------------------------------ Java-style String.valueOf

    [Theory]
    [MemberData(nameof(JavaStringCases))]
    public void GetString_formats_like_Java_String_valueOf(object? value, string expected)
    {
        using var rs = Make([Col("v")], [new[] { value }]);
        rs.Next();

        Assert.Equal(expected, rs.GetString("v"));
        Assert.Equal(expected, rs.GetString(1));
    }

    public static TheoryData<object?, string> JavaStringCases => new()
    {
        { true, "true" },
        { false, "false" },
        { 5, "5" },
        { -1, "-1" },
        { long.MaxValue, "9223372036854775807" },
        { (short)-7, "-7" },
        { (sbyte)-128, "-128" },
        { "abc", "abc" },
        { string.Empty, string.Empty },
        { 1.5f, "1.5" },
        { 100.0f, "100.0" },
        { 1.0E7f, "1.0E7" },
        { 0.001f, "0.001" },
        { 1.0E-5f, "1.0E-5" },
        { float.NaN, "NaN" },
        { float.PositiveInfinity, "Infinity" },
        { float.NegativeInfinity, "-Infinity" },
        { -0.0f, "-0.0" },
        { 0.0d, "0.0" },
        { 100.0d, "100.0" },
        { 9999999.0d, "9999999.0" },
        { 1.0E7d, "1.0E7" },
        { 1.2345678901234568E17d, "1.2345678901234568E17" },
        { 0.001d, "0.001" },
        { 9.999E-4d, "9.999E-4" },
        { 0.1d + 0.2d, "0.30000000000000004" },
        { double.MaxValue, "1.7976931348623157E308" },
        { 12.50m, "12.50" },
        { 100m, "100" },
        { -0.5m, "-0.5" },
        { 0.000001m, "0.000001" },
        { 0.0000001m, "1E-7" },
        { 0.00000012345m, "1.2345E-7" },
        { new DateTime(2024, 5, 6, 7, 8, 9), "2024-05-06 07:08:09.0" },
        { new DateTime(2024, 5, 6, 7, 8, 9).AddTicks(5_000_000), "2024-05-06 07:08:09.5" },
        { new DateTime(2024, 5, 6, 7, 8, 9).AddTicks(1_234_567), "2024-05-06 07:08:09.1234567" },
        { new DateOnly(2024, 5, 6), "2024-05-06" },
    };
}
