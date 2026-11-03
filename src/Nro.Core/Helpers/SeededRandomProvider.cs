using Nro.Core.Interfaces;

namespace Nro.Core;

/// <summary>
/// A deterministic <see cref="IRandomProvider"/> for tests and replays: a bit-exact port of the generator of <c>java.util.Random</c> (48-bit linear congruential generator,
/// JDK 17 algorithms for <c>nextInt(bound)</c>, <c>nextLong(bound)</c>, <c>nextDouble(bound)</c>, <c>nextFloat(bound)</c>). The same seed therefore yields the same numbers
/// as the Java <c>Random</c> built with the same seed (checked against the real JDK in <c>UtilDifferentialTests</c>).
/// Thread-safe: every call takes a lock, so the sequence a single thread sees is still deterministic when it is the only caller.
/// </summary>
public sealed class SeededRandomProvider : IRandomProvider
{
    private const long Multiplier = 0x5DEECE66DL;
    private const long Addend = 0xBL;
    private const long Mask = (1L << 48) - 1;
    private const double DoubleUnit = 1.0 / (1L << 53);
    private const float FloatUnit = 1.0f / (1 << 24);

    private readonly object gate = new();
    private long state;

    /// <summary>The same starting state as a Java <c>Random</c> constructed with <paramref name="seed"/>.</summary>
    public SeededRandomProvider(long seed)
    {
        state = (seed ^ Multiplier) & Mask;
    }

    /// <inheritdoc />
    public int NextInt()
    {
        lock (gate)
        {
            return Next(32);
        }
    }

    /// <inheritdoc />
    public int NextInt(int bound)
    {
        RandomBounds.CheckPositive(bound);
        lock (gate)
        {
            int r = Next(31);
            int m = bound - 1;
            if ((bound & m) == 0)
            {
                // a power of two: take the high-order bits
                return (int)((bound * (long)r) >> 31);
            }

            for (int u = r; unchecked((u - (r = u % bound)) + m) < 0; u = Next(31))
            {
                // reject candidates from the incomplete last bucket
            }

            return r;
        }
    }

    /// <inheritdoc />
    public long NextLong(long bound)
    {
        RandomBounds.CheckPositive(bound);
        lock (gate)
        {
            long m = bound - 1;
            long r = NextLongUnlocked();
            if ((bound & m) == 0L)
            {
                return r & m;
            }

            for (long u = (long)((ulong)r >> 1); unchecked((u + m) - (r = u % bound)) < 0L; u = (long)((ulong)NextLongUnlocked() >> 1))
            {
                // reject over-represented candidates
            }

            return r;
        }
    }

    /// <inheritdoc />
    public double NextDouble(double bound)
    {
        RandomBounds.CheckPositiveFinite(bound);
        lock (gate)
        {
            double unit = ((((long)Next(26)) << 27) + Next(27)) * DoubleUnit;
            return RandomBounds.ScaleDouble(unit, bound);
        }
    }

    /// <inheritdoc />
    public float NextFloat(float bound)
    {
        RandomBounds.CheckPositiveFinite(bound);
        lock (gate)
        {
            float unit = Next(24) * FloatUnit;
            return RandomBounds.ScaleFloat(unit, bound);
        }
    }

    private long NextLongUnlocked() => unchecked(((long)Next(32) << 32) + Next(32));

    private int Next(int bits)
    {
        state = unchecked((state * Multiplier) + Addend) & Mask;
        return unchecked((int)(state >> (48 - bits)));
    }
}
