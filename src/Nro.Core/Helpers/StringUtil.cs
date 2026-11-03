using System.Text;

namespace Nro.Core;

/// <summary>Port of <c>Utils/StringUtil.java</c>.</summary>
public static class StringUtil
{
    private const int LeftLimit = 48;   // '0'
    private const int RightLimit = 122; // 'z'

    /// <summary>
    /// Java <c>randomText(int length)</c>: a random text of <paramref name="length"/> ASCII letters and digits (<c>[0-9A-Za-z]</c>).
    /// Java drew <c>random.ints(48, 123)</c> (uniform in [48, 122]) and dropped the characters between '9' and 'A' and between 'Z' and 'a', so each accepted
    /// character is drawn the same way here (rejection sampling; the distribution over the 62 characters is uniform). Uses <see cref="Random.Shared"/>.
    /// </summary>
    public static string RandomText(int length) => RandomText(length, Random.Shared.Next);

    /// <summary>
    /// Same as <see cref="RandomText(int)"/> with an injectable random source so a test is deterministic: <paramref name="nextInt"/>(minInclusive, maxExclusive)
    /// returns a uniform integer in that range (the shape of <c>Random.Next(int, int)</c>; adapt it from the project's random provider).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Negative length (Java: <c>Stream.limit</c> throws <c>IllegalArgumentException</c>).</exception>
    public static string RandomText(int length, Func<int, int, int> nextInt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        ArgumentNullException.ThrowIfNull(nextInt);

        var sb = new StringBuilder(length);
        while (sb.Length < length)
        {
            int i = nextInt(LeftLimit, RightLimit + 1);
            if ((i <= 57 || i >= 65) && (i <= 90 || i >= 97))
            {
                sb.Append((char)i);
            }
        }

        return sb.ToString();
    }
}
