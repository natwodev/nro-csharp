namespace Nro.Tests.IO;

/// <summary>
/// A hand-written SplitMix64 pseudo-random number generator with a fixed seed that does NOT depend on the runtime's <see cref="Random"/>
/// (so the same seed yields the same sequence on every .NET version and matches the Java version in JavaDifferentialReference.java.txt).
/// </summary>
internal sealed class SplitMix64
{
    private ulong _state;

    public SplitMix64(long seed)
    {
        _state = unchecked((ulong)seed);
    }

    public ulong Next()
    {
        unchecked
        {
            _state += 0x9E3779B97F4A7C15UL;
            ulong z = _state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    /// <summary>An integer in [0, n), n &gt; 0 (same formula as the Java version: (next &gt;&gt;&gt; 33) % n).</summary>
    public int Below(int n) => (int)((Next() >> 33) % (ulong)n);
}
