namespace Nro.Core.Interfaces;

/// <summary>
/// The source of random numbers behind <see cref="RandomUtil"/> (the port of the random half of <c>Utils/Util.java</c>). It replaces the single static
/// <c>java.util.Random rand</c> of the Java <c>Util</c> class, so a test can plug in a scripted or seeded source and the production code can use the
/// thread-safe <see cref="SystemRandomProvider"/>. No class may create its own <c>System.Random</c>.
/// <para>
/// The five members are exactly the calls <c>Util.java</c> makes on its <c>Random</c>: <c>nextInt()</c>, <c>nextInt(int)</c>, <c>nextLong(long)</c>,
/// <c>nextDouble(double)</c> and <c>nextFloat(float)</c>. Argument errors behave like Java: a bound that is not positive (and for the floating point versions not finite)
/// throws <see cref="ArgumentOutOfRangeException"/> where Java throws <c>IllegalArgumentException</c>.
/// </para>
/// <para>Implementations must be safe to call from several threads at once (the Java <c>Random</c> is).</para>
/// </summary>
public interface IRandomProvider
{
    /// <summary>Java <c>Random.nextInt()</c>: any <see cref="int"/>, uniformly (negative values included).</summary>
    int NextInt();

    /// <summary>Java <c>Random.nextInt(bound)</c>: a value in <c>[0, bound)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bound"/> is not positive.</exception>
    int NextInt(int bound);

    /// <summary>Java 17 <c>Random.nextLong(bound)</c>: a value in <c>[0, bound)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bound"/> is not positive.</exception>
    long NextLong(long bound);

    /// <summary>Java 17 <c>Random.nextDouble(bound)</c>: a value in <c>[0, bound)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bound"/> is not a positive finite number.</exception>
    double NextDouble(double bound);

    /// <summary>Java 17 <c>Random.nextFloat(bound)</c>: a value in <c>[0, bound)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bound"/> is not a positive finite number.</exception>
    float NextFloat(float bound);
}
