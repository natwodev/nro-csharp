using Nro.Core;

namespace Nro.Tests.Helpers;

/// <summary><see cref="StringUtil.RandomText(int)"/>: <c>Utils.StringUtil.randomText</c> returns letters and digits only.</summary>
public class StringUtilTests
{
    private static bool IsAlphanumeric(char c) => c is (>= '0' and <= '9') or (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(100)]
    [InlineData(5000)]
    public void RandomText_has_the_requested_length_and_only_letters_and_digits(int length)
    {
        string text = StringUtil.RandomText(length);
        Assert.Equal(length, text.Length);
        Assert.All(text, c => Assert.True(IsAlphanumeric(c), $"unexpected character U+{(int)c:X4}"));
    }

    [Fact]
    public void RandomText_uses_all_62_characters_over_many_draws_and_differs_between_calls()
    {
        string big = StringUtil.RandomText(20_000);
        Assert.Equal(62, big.Distinct().Count());
        Assert.NotEqual(StringUtil.RandomText(32), StringUtil.RandomText(32));
    }

    [Fact]
    public void RandomText_with_a_scripted_source_skips_the_punctuation_between_digits_letters_and_letters()
    {
        // 58..64 (: ; < = > ? @), 91..96 ([ \ ] ^ _ `) are rejected; the others are kept in order
        int[] script = [58, 64, 91, 96, 48, 65, 122, 57, 90, 97, 59, 100];
        var calls = new List<(int Min, int Max)>();
        int next = 0;
        string text = StringUtil.RandomText(6, (min, max) =>
        {
            calls.Add((min, max));
            return script[next++];
        });

        Assert.Equal("0Az9Za", text);
        Assert.Equal(10, next);                         // stops right after the 6th accepted character
        Assert.All(calls, c => Assert.Equal((48, 123), c)); // the source is asked for [48, 122], as Java's random.ints(48, 123)
    }

    [Fact]
    public void RandomText_rejects_a_negative_length_and_a_missing_source()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StringUtil.RandomText(-1));
        Assert.Throws<ArgumentNullException>(() => StringUtil.RandomText(3, null!));
        Assert.Equal(string.Empty, StringUtil.RandomText(0, (_, _) => throw new InvalidOperationException("must not be called")));
    }
}
