using Nro.Core.Util;

namespace Nro.Tests.Unit.Core.Utilities;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/Utils/Functions.java (dòng 1–61).
public sealed class FunctionsTests
{
    // Kiểm tra các mẫu spam Java đại diện, gồm mẫu dấu chấm regex và chuỗi không khớp (Java: isSpam).
    [Theory]
    [InlineData("dkm")]
    [InlineData("ĐKM")]
    [InlineData("xin chào abc.com bạn")]
    [InlineData("cặc")]
    [InlineData("dog")]
    public void Is_spam_matches_the_java_pattern(string text) => Assert.True(Functions.IsSpam(text));

    // Kiểm tra mẫu chỉ khớp tại word boundary và null/rỗng là false như Java (Java: isSpam).
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("academic")]
    [InlineData("hello world")]
    public void Is_spam_rejects_non_matching_text(string? text) => Assert.False(Functions.IsSpam(text));

    // Kiểm tra từng lần sinh rút type rồi rút chữ số/chữ hoa, đúng bộ ký tự Java (Java: generateRandomCharacters).
    [Fact]
    public void Generate_random_characters_preserves_java_draw_order_and_charset()
    {
        var random = new ScriptedRandomProvider().Enqueue(0, 7, 1, 25, 0, 0);

        string text = Functions.GenerateRandomCharacters(3, random);

        Assert.Equal("7Z0", text);
        Assert.Equal([("NextInt", (object?)2), ("NextInt", (object?)10), ("NextInt", (object?)2), ("NextInt", (object?)26), ("NextInt", (object?)2), ("NextInt", (object?)10)], random.Calls);
    }

    // Kiểm tra độ dài và chỉ có số hoặc chữ hoa Java (Java: generateRandomCharacters).
    [Fact]
    public void Generate_random_characters_has_requested_length_and_java_charset()
    {
        string text = Functions.GenerateRandomCharacters(500, new SeededRandomProvider(17));

        Assert.Equal(500, text.Length);
        Assert.All(text, character => Assert.True(character is >= '0' and <= '9' or >= 'A' and <= 'Z'));
    }

    // Kiểm tra trần dương một phía và phép ép số âm Java (Java: maxint).
    [Fact]
    public void Max_int_clamps_only_values_above_int_maximum()
    {
        Assert.Equal(int.MaxValue, Functions.MaxInt((long)int.MaxValue + 1));
        Assert.Equal(17, Functions.MaxInt(17));
        Assert.Equal(-1, Functions.MaxInt(-1));
    }
}
