using Nro.Core;

namespace Nro.Tests.Helpers;

/// <summary>
/// Behaviour of the random half of <c>Util.java</c> (<see cref="RandomUtil"/>) with a scripted random source: which draw is made with which bound, in which order, and
/// what the result is. The expected values are worked out by hand from the Java source of each function.
/// </summary>
public class RandomUtilTests
{
    private static (RandomUtil Util, ScriptedRandomProvider Random) Create(params object[] scripted)
    {
        var random = new ScriptedRandomProvider().Enqueue(scripted);
        return (new RandomUtil(random), random);
    }

    // ---- nextInt / nextLong / nextDouble / nextfloat ----

    [Fact]
    public void NextInt_from_to_is_inclusive_and_draws_with_bound_to_minus_from_plus_one()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(0, 5, 3);
        Assert.Equal(10, util.NextInt(10, 15));
        Assert.Equal(-2, util.NextInt(-7, -2)); // draws 5 of [0,6)
        Assert.Equal(13, util.NextInt(10, 13)); // draws 3 of [0,4)
        Assert.Equal(new object?[] { 6, 6, 4 }, random.Calls.Select(c => c.Bound).ToArray());
    }

    [Fact]
    public void NextInt_from_to_with_to_below_from_throws_like_java()
    {
        (RandomUtil util, _) = Create();
        Assert.Throws<ArgumentOutOfRangeException>(() => util.NextInt(5, 4)); // bound 0
        Assert.Throws<ArgumentOutOfRangeException>(() => util.NextInt(5, 3)); // bound -1
        Assert.Throws<ArgumentOutOfRangeException>(() => util.NextInt(int.MinValue, int.MaxValue)); // to - from + 1 wraps to 0
    }

    [Fact]
    public void NextInt_max_and_NextLong_max_pass_the_bound_through()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(4, 99L);
        Assert.Equal(4, util.NextInt(5));
        Assert.Equal(99L, util.NextLong(100L));
        Assert.Equal(new object?[] { 5, 100L }, random.Calls.Select(c => c.Bound).ToArray());
        Assert.Throws<ArgumentOutOfRangeException>(() => util.NextInt(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => util.NextLong(-1L));
    }

    [Fact]
    public void NextLong_from_to_adds_from_to_the_draw()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(2L);
        Assert.Equal(10_000_000_002L, util.NextLong(10_000_000_000L, 10_000_000_009L));
        Assert.Equal(10L, random.Calls[0].Bound);
    }

    [Fact]
    public void NextDouble_and_NextFloat_use_the_bound_given_or_to_minus_from_plus_one()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(2.5, 0.25f);
        Assert.Equal(2.5, util.NextDouble(10.0));
        Assert.Equal(1.25f, util.NextFloat(1.0f, 3.5f)); // 1 + draw in [0, 3.5)
        Assert.Equal(new object?[] { 10.0, 3.5f }, random.Calls.Select(c => c.Bound).ToArray());
    }

    [Fact]
    public void NextInt_percentages_returns_the_first_bucket_whose_cumulative_weight_exceeds_the_draw_of_1000()
    {
        int[] weights = [100, 200, 300];
        foreach ((int draw, int expected) in new[] { (0, 0), (99, 0), (100, 1), (299, 1), (300, 2), (599, 2), (600, 3), (999, 3) })
        {
            (RandomUtil util, ScriptedRandomProvider random) = Create(draw);
            Assert.Equal(expected, util.NextInt(weights));
            Assert.Equal(1000, random.Calls[0].Bound);
        }

        (RandomUtil empty, _) = Create(500);
        Assert.Equal(0, empty.NextInt([]));
    }

    // ---- isTrue: one test per overload ----

    [Fact]
    public void IsTrueInt_is_false_for_non_positive_arguments_and_true_for_ratio_at_or_above_total_without_drawing()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create();
        Assert.False(util.IsTrueInt(0, 100));
        Assert.False(util.IsTrueInt(-5, 100));
        Assert.False(util.IsTrueInt(50, 0));
        Assert.False(util.IsTrueInt(50, -100));
        Assert.True(util.IsTrueInt(100, 100));
        Assert.True(util.IsTrueInt(150, 100));
        Assert.True(util.IsTrueInt(int.MaxValue, 1));
        Assert.Empty(random.Calls);
    }

    [Fact]
    public void IsTrueInt_draws_below_the_total_and_compares_strictly()
    {
        foreach ((int draw, bool expected) in new[] { (0, true), (59, true), (60, false), (99, false) })
        {
            (RandomUtil util, ScriptedRandomProvider random) = Create(draw);
            Assert.Equal(expected, util.IsTrueInt(60, 100));
            Assert.Equal(100, random.Calls.Single().Bound);
        }
    }

    [Fact]
    public void IsTrueLong_always_draws_and_throws_for_a_non_positive_total()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(99L, 0L, 49L, 50L);
        Assert.True(util.IsTrueLong(100, 100)); // the int version would not draw here
        Assert.True(util.IsTrueLong(1, 100)); // draw 0 < 1
        Assert.True(util.IsTrueLong(50, 100)); // draw 49 < 50
        Assert.False(util.IsTrueLong(50, 100)); // draw 50 is not below 50
        Assert.Equal(4, random.Calls.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => util.IsTrueLong(10, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => util.IsTrueLong(10, -1));
    }

    [Fact]
    public void IsTrueLong_with_non_positive_ratio_still_draws_unlike_the_int_version()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(0L);
        Assert.False(util.IsTrueLong(0, 100)); // draw 0 < 0 is false
        Assert.Single(random.Calls);
    }

    [Fact]
    public void IsTrueDouble_draws_a_double_below_the_total()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(19.99, 20.0, 0.0);
        Assert.True(util.IsTrueDouble(20.0, 100.0));
        Assert.False(util.IsTrueDouble(20.0, 100.0));
        Assert.False(util.IsTrueDouble(0.0, 100.0));
        Assert.All(random.Calls, call => Assert.Equal(100.0, call.Bound));
        Assert.Throws<ArgumentOutOfRangeException>(() => util.IsTrueDouble(1.0, 0.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => util.IsTrueDouble(1.0, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => util.IsTrueDouble(1.0, double.PositiveInfinity));
    }

    [Fact]
    public void IsTrueFloat_scales_a_fraction_by_100_and_truncates_the_ratio()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(49L, 50L, 0L, 99L);
        Assert.True(util.IsTrueFloat(0.5f, 100)); // 0.5 -> 50 of 10000
        Assert.False(util.IsTrueFloat(0.5f, 100));
        Assert.True(util.IsTrueFloat(1.0f, 100)); // not a fraction: 1 of 100, draw 0 < 1
        Assert.False(util.IsTrueFloat(0.999f, 1)); // 99.9 -> (long) 99 of 100: draw 99 is not below 99
        Assert.Equal(new object?[] { 10000L, 10000L, 100L, 100L }, random.Calls.Select(c => c.Bound).ToArray());
    }

    [Fact]
    public void IsTrueLongAccuracy_draws_the_second_number_only_when_the_first_test_passed()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(5L, 0, 5L, 3, 20L);
        Assert.True(util.IsTrueLongAccuracy(10, 100, 4)); // nextLong(400)=5 < 10, nextInt(4)=0
        Assert.False(util.IsTrueLongAccuracy(10, 100, 4)); // nextLong(400)=5 < 10, nextInt(4)=3 is not 0
        Assert.False(util.IsTrueLongAccuracy(10, 100, 4)); // nextLong(400)=20 is not below 10: no second draw
        Assert.Equal(new object?[] { 400L, 4, 400L, 4, 400L }, random.Calls.Select(c => c.Bound).ToArray());
        Assert.Equal(0, random.Remaining);
    }

    [Fact]
    public void IsTrueFloatAccuracy_scales_a_fraction_then_applies_the_accuracy_twice()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(49L, 0);
        Assert.True(util.IsTrueFloatAccuracy(0.5f, 100, 2)); // 50 of (10000 * 2), then nextInt(2) == 0
        Assert.Equal(new object?[] { 20000L, 2 }, random.Calls.Select(c => c.Bound).ToArray());
    }

    [Fact]
    public void GetOne_returns_the_first_number_for_an_even_draw_including_negative_even_and_zero()
    {
        (RandomUtil util, _) = Create(4, 3, -3, -4, 0, int.MinValue, int.MaxValue);
        Assert.Equal(1, util.GetOne(1, 2)); // 4
        Assert.Equal(2, util.GetOne(1, 2)); // 3
        Assert.Equal(2, util.GetOne(1, 2)); // -3 % 2 == -1 in Java and in C#
        Assert.Equal(1, util.GetOne(1, 2)); // -4
        Assert.Equal(1, util.GetOne(1, 2)); // 0
        Assert.Equal(1, util.GetOne(1, 2)); // int.MinValue is even
        Assert.Equal(2, util.GetOne(1, 2)); // int.MaxValue is odd
    }

    [Fact]
    public void GetChanceFromLuck_uses_the_double_overload_with_the_luck_bonus()
    {
        // ratio = 10 + 30 * 100 / 300 = 20 of 100
        (RandomUtil util, ScriptedRandomProvider random) = Create(19.5, 20.0);
        Assert.True(util.GetChanceFromLuck(30, 10, 100, false));
        Assert.False(util.GetChanceFromLuck(30, 10, 100, false));
        Assert.All(random.Calls, call => Assert.Equal(100.0, call.Bound));
    }

    [Fact]
    public void GetChanceFromLuck_with_the_four_leaf_clover_reduces_max_to_a_133rd_first()
    {
        double max = 100.0 * 100 / 133;
        double ratio = 10 + (30 * max / 300);
        (RandomUtil util, ScriptedRandomProvider random) = Create(Math.BitDecrement(ratio), ratio);
        Assert.True(util.GetChanceFromLuck(30, 10, 100, true));
        Assert.False(util.GetChanceFromLuck(30, 10, 100, true));
        Assert.All(random.Calls, call => Assert.Equal(max, call.Bound));
    }

    // ---- Crisnext / GioiHannext ----

    [Fact]
    public void Crisnext_and_GioiHannext_add_a_draw_below_to_minus_from_plus_one_and_cut_to_long()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(3, 3, 0, 6);
        Assert.Equal(8L, util.Crisnext(5.0, 9.0)); // 5 + 3 of [0,5)
        Assert.Equal(8L, util.GioiHannext(5.0, 9.0));
        Assert.Equal(0L, util.Crisnext(7.9)); // (int) 7.9 = 7: draw of [0,7)
        Assert.Equal(6L, util.Crisnext(7.0));
        Assert.Equal(new object?[] { 5, 5, 7, 7 }, random.Calls.Select(c => c.Bound).ToArray());
    }

    [Fact]
    public void Crisnext_cuts_fractional_bounds_toward_zero_and_adds_the_draw_to_the_double()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(1);
        Assert.Equal(2L, util.Crisnext(1.5, 3.4)); // (int)(3.4 - 1.5 + 1) = 2: draw of [0,2); (long)(1.5 + 1) = 2
        Assert.Equal(2, random.Calls[0].Bound);
    }

    [Fact]
    public void GioiHannextdame_returns_a_double_without_cutting_it()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(1);
        Assert.Equal(2.5, util.GioiHannextdame(1.5, 3.4));
        Assert.Equal(2, random.Calls[0].Bound);
    }

    // ---- shuffleArray ----

    [Fact]
    public void ShuffleArray_is_a_fisher_yates_from_the_end_with_bounds_n_down_to_2()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(0, 0, 0);
        int[] array = [1, 2, 3, 4];
        util.ShuffleArray(array);
        Assert.Equal(new[] { 2, 3, 4, 1 }, array);
        Assert.Equal(new object?[] { 4, 3, 2 }, random.Calls.Select(c => c.Bound).ToArray());
    }

    [Fact]
    public void ShuffleArray_with_identity_draws_leaves_the_array_unchanged_and_small_arrays_do_not_draw()
    {
        (RandomUtil util, ScriptedRandomProvider random) = Create(3, 2, 1);
        int[] array = [1, 2, 3, 4];
        util.ShuffleArray(array);
        Assert.Equal(new[] { 1, 2, 3, 4 }, array);

        util.ShuffleArray([]);
        util.ShuffleArray([7]);
        Assert.Equal(3, random.Calls.Count);
    }

    [Fact]
    public void ShuffleArray_with_a_real_provider_keeps_the_same_elements()
    {
        var util = new RandomUtil(new SeededRandomProvider(7));
        int[] array = Enumerable.Range(0, 50).ToArray();
        util.ShuffleArray(array);
        Assert.Equal(Enumerable.Range(0, 50), array.Order());
        Assert.NotEqual(Enumerable.Range(0, 50), array);
    }

    [Fact]
    public void A_null_provider_or_array_is_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => new RandomUtil(null!));
        (RandomUtil util, _) = Create();
        Assert.Throws<ArgumentNullException>(() => util.ShuffleArray(null!));
        Assert.Throws<ArgumentNullException>(() => util.NextInt((int[])null!));
    }
}
