using Nro.Core.Interfaces;

namespace Nro.Core;

/// <summary>
/// The production <see cref="IRandomProvider"/>: thread-safe, backed by <see cref="Random.Shared"/> (no instance of <see cref="Random"/> is created here).
/// Not reproducible between runs; use <see cref="SeededRandomProvider"/> when a sequence must repeat.
/// </summary>
public sealed class SystemRandomProvider : IRandomProvider
{
    /// <summary>A shared instance (the provider has no state).</summary>
    public static readonly SystemRandomProvider Instance = new();

    /// <inheritdoc />
    public int NextInt() => (int)Random.Shared.NextInt64(int.MinValue, (long)int.MaxValue + 1);

    /// <inheritdoc />
    public int NextInt(int bound)
    {
        RandomBounds.CheckPositive(bound);
        return Random.Shared.Next(bound);
    }

    /// <inheritdoc />
    public long NextLong(long bound)
    {
        RandomBounds.CheckPositive(bound);
        return Random.Shared.NextInt64(bound);
    }

    /// <inheritdoc />
    public double NextDouble(double bound)
    {
        RandomBounds.CheckPositiveFinite(bound);
        return RandomBounds.ScaleDouble(Random.Shared.NextDouble(), bound);
    }

    /// <inheritdoc />
    public float NextFloat(float bound)
    {
        RandomBounds.CheckPositiveFinite(bound);
        return RandomBounds.ScaleFloat(Random.Shared.NextSingle(), bound);
    }
}
