using Nro.Core.Interfaces;

namespace Nro.Core;

/// <summary>
/// Port of the random half of <c>Utils/Util.java</c> (<c>nextInt</c>, <c>nextLong</c>, <c>nextDouble</c>, <c>nextfloat</c>, the five <c>isTrue</c> overloads, <c>getOne</c>,
/// <c>getChanceFromLuck</c>, <c>Crisnext</c>, <c>GioiHannext</c>, <c>GioiHannextdame</c>, <c>shuffleArray</c>). The static <c>Random rand</c> of the Java class becomes the injected
/// <see cref="IRandomProvider"/>; the arithmetic, the order of the random calls and the exceptions are kept exactly, so a seeded provider replays what Java would do.
/// <para>
/// THE <c>isTrue</c> OVERLOADS ARE NOT INTERCHANGEABLE. Java picks one from the static types of the arguments, and they behave differently, so each has its own name here:
/// <see cref="IsTrueInt"/> (<c>int,int</c>: false when either is not positive, true when ratio &gt;= total, never throws),
/// <see cref="IsTrueLong"/> (<c>long,long</c>), <see cref="IsTrueDouble"/> (<c>double,double</c>), <see cref="IsTrueFloat"/> (<c>float,long</c>),
/// <see cref="IsTrueLongAccuracy"/> (<c>long,long,int</c>) and <see cref="IsTrueFloatAccuracy"/> (<c>float,long,int</c>).
/// When porting a Java call <c>Util.isTrue(a, b)</c>, look at the declared types of <c>a</c> and <c>b</c> (an int literal selects the <c>int</c> version, a <c>long</c> variable the
/// <c>long</c> version, a <c>double</c> expression the <c>double</c> version, a <c>float</c> first argument with an int/long second argument the <c>float</c> version).
/// </para>
/// </summary>
public sealed class RandomUtil
{
    private readonly IRandomProvider random;

    /// <summary>Creates the helper over a random source (<see cref="SystemRandomProvider.Instance"/> in production, a seeded or scripted provider in tests).</summary>
    public RandomUtil(IRandomProvider random)
    {
        ArgumentNullException.ThrowIfNull(random);
        this.random = random;
    }

    /// <summary>Java <c>nextInt(int from, int to)</c>: <c>from + rand.nextInt(to - from + 1)</c>, both ends inclusive. <c>to - from + 1</c> wraps in <c>int</c>; a result &lt;= 0 throws like Java.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="to"/> is less than <paramref name="from"/>.</exception>
    public int NextInt(int from, int to) => unchecked(from + random.NextInt(to - from + 1));

    /// <summary>Java <c>nextfloat(float from, float to)</c>: <c>from + rand.nextFloat(to - from + 1)</c> (single precision, the upper end is NOT reachable the way it is for ints).</summary>
    public float NextFloat(float from, float to) => from + random.NextFloat((to - from) + 1);

    /// <summary>Java <c>nextInt(int max)</c>: a value in <c>[0, max)</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="max"/> is not positive.</exception>
    public int NextInt(int max) => random.NextInt(max);

    /// <summary>Java <c>nextLong(long from, long to)</c>: <c>from + rand.nextLong(to - from + 1)</c>, both ends inclusive.</summary>
    public long NextLong(long from, long to) => unchecked(from + random.NextLong(to - from + 1));

    /// <summary>Java <c>nextLong(long max)</c>: a value in <c>[0, max)</c>.</summary>
    public long NextLong(long max) => random.NextLong(max);

    /// <summary>Java <c>nextDouble(double max)</c>: a value in <c>[0, max)</c>.</summary>
    public double NextDouble(double max) => random.NextDouble(max);

    /// <summary>
    /// Java <c>nextInt(int[] percen)</c>: draws <c>next = nextInt(1000)</c>, then returns the first index whose cumulative weight exceeds it; returns
    /// <c>percen.Length</c> when the weights add up to less than the draw (the weights are per mille, not percent).
    /// </summary>
    public int NextInt(int[] percen)
    {
        ArgumentNullException.ThrowIfNull(percen);
        int next = NextInt(1000);
        int i;
        for (i = 0; i < percen.Length; i++)
        {
            if (next < percen[i])
            {
                return i;
            }

            next -= percen[i];
        }

        return i;
    }

    /// <summary>
    /// Java <c>getChanceFromLuck</c>: <c>isTrue(mum + tyLeMayMan * max / 300, max)</c> with the DOUBLE overload (<see cref="IsTrueDouble"/>), where <c>max</c> is first reduced to
    /// <c>max * 100 / 133</c> when <paramref name="isCoBonLa"/> is set.
    /// </summary>
    public bool GetChanceFromLuck(double tyLeMayMan, double mum, double max, bool isCoBonLa)
    {
        if (isCoBonLa)
        {
            max = (max * 100) / 133;
        }

        return IsTrueDouble(mum + ((tyLeMayMan * max) / 300), max);
    }

    /// <summary>
    /// Java <c>isTrue(int ratio, int typeRatio)</c>: false when <paramref name="typeRatio"/> or <paramref name="ratio"/> is not positive, true (without drawing) when
    /// <c>ratio &gt;= typeRatio</c>, otherwise <c>nextInt(typeRatio) &lt; ratio</c>. The only overload that never throws.
    /// </summary>
    public bool IsTrueInt(int ratio, int typeRatio)
    {
        if (typeRatio <= 0 || ratio <= 0)
        {
            return false;
        }

        if (ratio >= typeRatio)
        {
            return true;
        }

        return NextInt(typeRatio) < ratio;
    }

    /// <summary>Java <c>getOne(n1, n2)</c>: <paramref name="n1"/> when <c>rand.nextInt() % 2 == 0</c> (an even draw), otherwise <paramref name="n2"/>.</summary>
    public int GetOne(int n1, int n2) => random.NextInt() % 2 == 0 ? n1 : n2;

    /// <summary>
    /// Java <c>isTrue(long ratioPercentage, long totalPercentage)</c>: <c>nextLong(total) &lt; ratio</c>. Always draws: a non-positive total throws, and a ratio at or above the
    /// total is true only because every draw is below it.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="totalPercentage"/> is not positive.</exception>
    public bool IsTrueLong(long ratioPercentage, long totalPercentage) => NextLong(totalPercentage) < ratioPercentage;

    /// <summary>
    /// Java <c>isTrue(double ratioPercentage, double totalPercentage)</c>: <c>nextDouble(total) &lt; ratio</c>. Always draws; throws when the total is not a positive finite number.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="totalPercentage"/> is not positive and finite.</exception>
    public bool IsTrueDouble(double ratioPercentage, double totalPercentage) => NextDouble(totalPercentage) < ratioPercentage;

    /// <summary>
    /// Java <c>isTrue(float ratioPercentage, long totalPercentage)</c>: a ratio below 1 is a fraction and both numbers are multiplied by 100 first
    /// (<c>0.5f, 100</c> becomes <c>50, 10000</c>); then the <c>(long)</c> truncated ratio goes to <see cref="IsTrueLong"/>.
    /// </summary>
    public bool IsTrueFloat(float ratioPercentage, long totalPercentage)
    {
        if (ratioPercentage < 1)
        {
            ratioPercentage *= 100;
            totalPercentage = unchecked(totalPercentage * 100);
        }

        return IsTrueLong(JavaNumbers.ToLong(ratioPercentage), totalPercentage);
    }

    /// <summary>
    /// Java <c>isTrue(long ratioPercentage, long totalPercentage, int accuracy)</c>: <c>nextLong(total * accuracy) &lt; ratio &amp;&amp; nextInt(accuracy) == 0</c>. The second draw
    /// happens only when the first test passed (short circuit), and the odds are ratio / (total * accuracy) / accuracy, i.e. the accuracy is applied TWICE.
    /// </summary>
    public bool IsTrueLongAccuracy(long ratioPercentage, long totalPercentage, int accuracy) =>
        NextLong(unchecked(totalPercentage * accuracy)) < ratioPercentage && NextInt(accuracy) == 0;

    /// <summary>Java <c>isTrue(float ratioPercentage, long totalPercentage, int accuracy)</c>: the fraction scaling of <see cref="IsTrueFloat"/>, then <see cref="IsTrueLongAccuracy"/>.</summary>
    public bool IsTrueFloatAccuracy(float ratioPercentage, long totalPercentage, int accuracy)
    {
        if (ratioPercentage < 1)
        {
            ratioPercentage *= 100;
            totalPercentage = unchecked(totalPercentage * 100);
        }

        return IsTrueLongAccuracy(JavaNumbers.ToLong(ratioPercentage), totalPercentage, accuracy);
    }

    /// <summary>Java <c>Crisnext(double from, double to)</c>: <c>(long)(from + rand.nextInt((int)(to - from + 1)))</c>.</summary>
    public long Crisnext(double from, double to) => JavaNumbers.ToLong(from + random.NextInt(JavaNumbers.ToInt((to - from) + 1)));

    /// <summary>Java <c>Crisnext(double max)</c>: <c>(long)rand.nextInt((int)max)</c>.</summary>
    public long Crisnext(double max) => random.NextInt(JavaNumbers.ToInt(max));

    /// <summary>Java <c>GioiHannext(double from, double to)</c>: identical to <see cref="Crisnext(double, double)"/>.</summary>
    public long GioiHannext(double from, double to) => JavaNumbers.ToLong(from + random.NextInt(JavaNumbers.ToInt((to - from) + 1)));

    /// <summary>Java <c>GioiHannextdame(double from, double to)</c>: <c>from + rand.nextInt((int)(to - from + 1))</c> as a double (no cast to long).</summary>
    public double GioiHannextdame(double from, double to) => from + random.NextInt(JavaNumbers.ToInt((to - from) + 1));

    /// <summary>Java <c>shuffleArray(int[])</c>: Fisher-Yates from the end, <c>j = nextInt(i + 1)</c> for <c>i = length - 1 .. 1</c>, in place.</summary>
    public void ShuffleArray(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = random.NextInt(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
