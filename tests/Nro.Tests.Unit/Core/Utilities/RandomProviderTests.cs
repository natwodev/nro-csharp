using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Util;

namespace Nro.Tests.Unit.Core.Utilities;

/// <summary>The two real <see cref="IRandomProvider"/> implementations: argument checks like Java, ranges, thread safety and reproducibility.</summary>
public class RandomProviderTests
{
    public static TheoryData<string> Providers => new() { "system", "seeded" };

    private static IRandomProvider Create(string kind) => kind == "system" ? SystemRandomProvider.Instance : new SeededRandomProvider(12345);

    [Theory]
    [MemberData(nameof(Providers))]
    public void A_bound_that_is_not_positive_throws_like_java_IllegalArgumentException(string kind)
    {
        IRandomProvider random = Create(kind);
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(-5));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(int.MinValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextLong(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextLong(long.MinValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextDouble(0.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextDouble(-1.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextDouble(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextDouble(double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextFloat(0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextFloat(float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => random.NextFloat(float.PositiveInfinity));
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public void Draws_stay_inside_the_bound_and_a_bound_of_one_always_gives_zero(string kind)
    {
        IRandomProvider random = Create(kind);
        for (int i = 0; i < 20_000; i++)
        {
            Assert.InRange(random.NextInt(7), 0, 6);
            Assert.InRange(random.NextInt(1_000_000_007), 0, 1_000_000_006);
            Assert.InRange(random.NextLong(5L), 0L, 4L);
            Assert.InRange(random.NextLong(long.MaxValue), 0L, long.MaxValue - 1);
            Assert.InRange(random.NextDouble(2.5), 0.0, Math.BitDecrement(2.5));
            Assert.InRange(random.NextFloat(2.5f), 0f, MathF.BitDecrement(2.5f));
            Assert.Equal(0, random.NextInt(1));
            Assert.Equal(0L, random.NextLong(1L));
        }
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public void Every_value_of_a_small_bound_shows_up_and_both_signs_of_NextInt_do(string kind)
    {
        IRandomProvider random = Create(kind);
        var seen = new HashSet<int>();
        bool negative = false;
        bool positive = false;
        for (int i = 0; i < 5_000; i++)
        {
            seen.Add(random.NextInt(6));
            int any = random.NextInt();
            negative |= any < 0;
            positive |= any > 0;
        }

        Assert.Equal(6, seen.Count);
        Assert.True(negative && positive);
    }

    [Theory]
    [MemberData(nameof(Providers))]
    public void Concurrent_use_never_leaves_the_range_or_throws(string kind)
    {
        IRandomProvider random = Create(kind);
        int outOfRange = 0;
        Parallel.For(0, 8, _ =>
        {
            for (int i = 0; i < 20_000; i++)
            {
                if (random.NextInt(100) is < 0 or >= 100 || random.NextLong(100L) is < 0 or >= 100)
                {
                    Interlocked.Increment(ref outOfRange);
                }
            }
        });
        Assert.Equal(0, outOfRange);
    }

    [Fact]
    public void Seeded_provider_repeats_for_the_same_seed_and_differs_for_another_seed()
    {
        var a = new SeededRandomProvider(99);
        var b = new SeededRandomProvider(99);
        var c = new SeededRandomProvider(100);
        int[] first = Enumerable.Range(0, 20).Select(_ => a.NextInt(1000)).ToArray();
        Assert.Equal(first, Enumerable.Range(0, 20).Select(_ => b.NextInt(1000)).ToArray());
        Assert.NotEqual(first, Enumerable.Range(0, 20).Select(_ => c.NextInt(1000)).ToArray());
    }

    [Fact]
    public void Seeded_provider_gives_the_values_of_java_util_Random_for_seed_42()
    {
        // java (JDK 17), seed 42: nextInt() gives -1170105035, 234785527; nextInt(100) from a fresh generator gives 30, 63, 48, 84, 70
        var raw = new SeededRandomProvider(42);
        Assert.Equal(-1170105035, raw.NextInt());
        Assert.Equal(234785527, raw.NextInt());
        var bounded = new SeededRandomProvider(42);
        Assert.Equal(new[] { 30, 63, 48, 84, 70 }, Enumerable.Range(0, 5).Select(_ => bounded.NextInt(100)).ToArray());
    }

    [Fact]
    public void Real_sources_cannot_be_null_and_the_system_instance_is_shared()
    {
        Assert.Same(SystemRandomProvider.Instance, SystemRandomProvider.Instance);
        Assert.Throws<ArgumentNullException>(() => new RandomUtil(null!));
    }
}
