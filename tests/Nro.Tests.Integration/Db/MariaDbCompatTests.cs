using Nro.Data.Db;

namespace Nro.Tests.Integration.Db;

/// <summary>
/// Pins <see cref="MariaDbCompat"/> to what <c>mariadb:10.4.34</c> answered when the same expressions were run by hand (the cases below are the measured results;
/// the end-to-end equivalence on whole rankings is <see cref="TopRepositoryGoldenTests"/>).
/// </summary>
public class MariaDbCompatTests
{
    [Theory]
    [InlineData("+5", 5UL)]
    [InlineData(" 5", 5UL)]
    [InlineData("\t5", 5UL)]
    [InlineData("\n5", 5UL)]
    [InlineData("- 5", 0UL)]
    [InlineData("-", 0UL)]
    [InlineData("+", 0UL)]
    [InlineData("", 0UL)]
    [InlineData("0005", 5UL)]
    [InlineData("1,5", 1UL)]
    [InlineData("-99999999999999999999", 9223372036854775808UL)]
    [InlineData("-18446744073709551615", 9223372036854775808UL)]
    [InlineData("-9223372036854775808", 9223372036854775808UL)]
    [InlineData("-9223372036854775809", 9223372036854775808UL)]
    [InlineData("9223372036854775807", 9223372036854775807UL)]
    [InlineData("18446744073709551615", 18446744073709551615UL)]
    [InlineData("18446744073709551616", 18446744073709551615UL)]
    [InlineData("12345678901234567890123", 18446744073709551615UL)]
    [InlineData("٣", 0UL)]
    [InlineData("１２", 0UL)]
    [InlineData("5 6", 5UL)]
    [InlineData("5x", 5UL)]
    [InlineData("x5", 0UL)]
    [InlineData(".5", 0UL)]
    [InlineData("5.", 5UL)]
    [InlineData("-.5", 0UL)]
    [InlineData("1e5", 1UL)]
    [InlineData("0x10", 0UL)]
    [InlineData("  -7", 18446744073709551609UL)]
    [InlineData("--5", 0UL)]
    [InlineData("+-5", 0UL)]
    [InlineData("-0", 0UL)]
    [InlineData("-1", 18446744073709551615UL)]
    [InlineData("00000000000000000000000000000000012", 12UL)]
    [InlineData("5\0", 5UL)]
    public void CastToUnsigned_matches_the_measured_mariadb_results(string text, ulong expected)
    {
        Assert.Equal(expected, MariaDbCompat.CastToUnsigned(text));
    }

    [Theory]
    // json, index, unquote, expected (null = SQL NULL)
    [InlineData("[0,5,7]", 1, false, 5UL)]
    [InlineData("[0,-5,7]", 1, false, 18446744073709551611UL)]
    [InlineData("[0,1.5,7]", 1, false, 1UL)]
    [InlineData("[0,1.9e2,7]", 1, false, 1UL)]
    [InlineData("[0,\"12\",7]", 1, false, 0UL)]
    [InlineData("[0,\"12\",7]", 1, true, 12UL)]
    [InlineData("[0,\"abc\",7]", 1, true, 0UL)]
    [InlineData("[0,\"\",7]", 1, true, 0UL)]
    [InlineData("[0,true,7]", 1, true, 0UL)]
    [InlineData("[0,false,7]", 1, true, 0UL)]
    [InlineData("[0,null,7]", 1, true, 0UL)]
    [InlineData("[0,[1,2],7]", 1, true, 0UL)]
    [InlineData("[0,{\"a\":1},7]", 1, true, 0UL)]
    [InlineData("[0,18446744073709551615,7]", 1, false, 18446744073709551615UL)]
    [InlineData("[0,18446744073709551616,7]", 1, false, 18446744073709551615UL)]
    [InlineData("[0,99999999999999999999999,7]", 1, false, 18446744073709551615UL)]
    [InlineData("[0,9223372036854775808,7]", 1, false, 9223372036854775808UL)]
    [InlineData("[0]", 1, false, null)]
    [InlineData("[]", 1, false, null)]
    [InlineData("{}", 1, false, null)]
    [InlineData("null", 1, false, null)]
    [InlineData("", 1, false, null)]
    [InlineData("   ", 1, false, null)]
    [InlineData("[0,5,", 1, false, null)]
    [InlineData("not json", 1, false, null)]
    [InlineData("[0,5,7] trailing", 1, false, null)]
    [InlineData("  [0, 8 ,7]  ", 1, false, 8UL)]
    [InlineData("[0,+5,7]", 1, false, null)]
    [InlineData("[0,05,7]", 1, false, null)]
    [InlineData("[0,1e2,7]", 1, false, 1UL)]
    [InlineData("[0,\"1e2\",7]", 1, false, 0UL)]
    [InlineData("[0,\"1e2\",7]", 1, true, 1UL)]
    [InlineData("[0,\"  12\",7]", 1, true, 12UL)]
    [InlineData("[0,\"12abc\",7]", 1, true, 12UL)]
    [InlineData("[0,-0,7]", 1, false, 0UL)]
    [InlineData("[0,0.5,7]", 1, false, 0UL)]
    [InlineData("[0,-0.5,7]", 1, false, 0UL)]
    [InlineData("[0,\"-3\",7]", 1, false, 0UL)]
    [InlineData("[0,\"-3\",7]", 1, true, 18446744073709551613UL)]
    [InlineData("{\"1\":5}", 1, false, null)]
    [InlineData("[0,12345678901234567890,7]", 1, false, 12345678901234567890UL)]
    [InlineData("[0,\"0x1F\",7]", 1, true, 0UL)]
    [InlineData("[0,1E+2,7]", 1, false, 1UL)]
    [InlineData("\"str\"", 1, false, null)]
    [InlineData("5", 1, false, null)]
    [InlineData("[0,9.5e18,7]", 1, false, 9UL)]
    [InlineData("[0,2.5e19,7]", 1, false, 2UL)]
    [InlineData("[0,-2.5e19,7]", 1, false, 18446744073709551614UL)]
    [InlineData("[0,1.0,7]", 1, false, 1UL)]
    // a document that is not an array counts as a one-element array for $[0]
    [InlineData("5", 0, false, 5UL)]
    [InlineData("\"12\"", 0, true, 12UL)]
    [InlineData("\"12\"", 0, false, 0UL)]
    [InlineData("{\"a\":1}", 0, true, 0UL)]
    [InlineData("null", 0, false, 0UL)]
    [InlineData("true", 0, true, 0UL)]
    // invalid per MariaDB's json_valid
    [InlineData("[1,]", 0, false, null)]
    [InlineData("[1 2]", 0, false, null)]
    [InlineData("['a']", 0, false, null)]
    [InlineData("[NaN]", 0, false, null)]
    [InlineData("[01]", 0, false, null)]
    [InlineData("[.5]", 0, false, null)]
    public void JsonIndexToUnsigned_matches_the_measured_mariadb_results(string json, int index, bool unquote, ulong? expected)
    {
        Assert.Equal(expected, MariaDbCompat.JsonIndexToUnsigned(json, index, unquote));
    }

    [Fact]
    public void JsonIndexToUnsigned_of_a_null_reference_is_sql_null()
    {
        Assert.Null(MariaDbCompat.JsonIndexToUnsigned(null, 0, unquote: true));
    }

    [Fact]
    public void Unquote_decodes_json_escapes_before_casting()
    {
        // ["\u0031\u0032"] : the two escaped digits are the text 12
        Assert.Equal(12UL, MariaDbCompat.JsonIndexToUnsigned("[\"\\u0031\\u0032\"]", 0, unquote: true));
        Assert.Equal(0UL, MariaDbCompat.JsonIndexToUnsigned("[\"\\u0031\\u0032\"]", 0, unquote: false));
    }

    [Fact]
    public void A_lone_surrogate_escape_is_invalid_json_in_mariadb_and_never_throws_here()
    {
        // MariaDB: json_valid('["\ud800"]') is 0, so the answer is NULL; whatever System.Text.Json decides, there must be no exception.
        var result = MariaDbCompat.JsonIndexToUnsigned("[\"\\ud800\"]", 0, unquote: true);
        Assert.True(result is null or 0UL);
    }

    [Fact]
    public void Very_deep_nesting_is_invalid_json_not_a_crash()
    {
        var deep = new string('[', 5000) + new string(']', 5000);
        Assert.Null(MariaDbCompat.JsonIndexToUnsigned(deep, 1, unquote: false));
    }

    [Theory]
    [InlineData("[\"x\",\"[1,2,3]\"]", 1, 2UL)]
    [InlineData("[\"x\",[1,2,3]]", 1, 2UL)] // pet[1] is a real array: JSON_UNQUOTE leaves it alone
    [InlineData("[\"x\",5]", 1, null)] // a number has no element 1
    [InlineData("[\"x\",\"not json\"]", 1, null)]
    [InlineData("[\"x\"]", 1, null)]
    [InlineData("garbage", 1, null)]
    [InlineData("[]", 1, null)]
    [InlineData("[\"x\",\"[1,\\\"7\\\",3]\"]", 1, 7UL)] // string elements are unquoted
    public void NestedJsonIndexToUnsigned_reads_the_pet_shape(string pet, int innerIndex, ulong? expected)
    {
        Assert.Equal(expected, MariaDbCompat.NestedJsonIndexToUnsigned(pet, 1, innerIndex));
    }

    [Theory]
    [InlineData("a[457,5]b[457,7]c", "[457,", -1, "7]c")]
    [InlineData("abc", "[457,", -1, "abc")]
    [InlineData("x,y,z", ",", 2, "x,y")]
    [InlineData("x", ",", 2, "x")]
    [InlineData("", ",", 2, "")]
    [InlineData("ab]cd]", "]", 1, "ab")]
    [InlineData("[457,9]", "[457,", -1, "9]")]
    [InlineData("a[457,5]b[457,7]c", "[457,", 1, "a")]
    [InlineData("]x", "]", 1, "")]
    [InlineData("a,b,c,d", ",", -2, "c,d")]
    [InlineData("a,b,c,d", ",", 3, "a,b,c")]
    [InlineData("a,b", ",", 0, "")]
    public void SubstringIndex_matches_mysql(string text, string delimiter, int count, string expected)
    {
        Assert.Equal(expected, MariaDbCompat.SubstringIndex(text, delimiter, count));
    }

    [Theory]
    [InlineData("[\"[1,2,[]]\",\"[457,40,[]]\"]", 40UL)]
    [InlineData("[\"[457,5,[]]\",\"[457,9,[]]\"]", 9UL)] // only the LAST stack is read
    [InlineData("[\"[1,2,[]]\"]", 0UL)]
    [InlineData("[]", 0UL)]
    [InlineData("", 0UL)]
    [InlineData("[\"[457,abc]\"]", 0UL)]
    [InlineData("[\"[457,3.9,[]]\"]", 3UL)]
    [InlineData("[\"[457, 12,[]]\"]", 12UL)]
    [InlineData("[\"[457,-3,[]]\"]", 18446744073709551613UL)]
    [InlineData("[\"[457,99999999999999999999,[]]\"]", 18446744073709551615UL)]
    [InlineData("[\"[457,160,\\\"[\\\\\\\"[48,1]\\\\\\\"]\\\",1700000000999]\"]", 160UL)] // the real shape: nested escaped options
    public void GoldBarQuantity_follows_the_java_substring_chain(string itemList, ulong expected)
    {
        Assert.Equal(expected, MariaDbCompat.GoldBarQuantity(itemList));
    }
}
