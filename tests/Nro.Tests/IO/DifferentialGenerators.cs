using System.Text;

namespace Nro.Tests.IO;

/// <summary>
/// Deterministic pseudo-random data generators, REWRITTEN IDENTICALLY call for call on <see cref="SplitMix64"/> compared with
/// <c>JavaDifferentialReference.java.txt</c> (the Java version, run on a real JDK 17). Changing a single call here (order, count, constants)
/// will throw off the SHA-256 in <c>JavaDifferentialTests</c>; to change it, the constants must be regenerated with the Java version.
/// </summary>
internal static class DifferentialGenerators
{
    private static readonly int[] RegionLo = [0x0000, 0x0080, 0x0800, 0xD800, 0xDC00, 0xE000, 0x1E00, 0x4E00];
    private static readonly int[] RegionHi = [0x007F, 0x07FF, 0xD7FF, 0xDBFF, 0xDFFF, 0xFFFF, 0x1EFF, 0x9FFF];
    private static readonly int[] LongAsciiLengths = [254, 255, 256, 257, 32767, 32768, 32769, 65534, 65535, 65536, 65537];
    private static readonly int[] EdgeUnits = [0, 'a', 0x7F, 0x80, 0x7FF, 0x800, 0xFFFF];

    public static readonly int[] InterestingBytes =
    [
        0x00, 0x01, 0x41, 0x7F, 0x80, 0x81, 0x9F, 0xA0, 0xB0, 0xBF, 0xC0, 0xC1, 0xC2, 0xDF,
        0xE0, 0xE1, 0xEF, 0xF0, 0xF4, 0xF7, 0xF8, 0xFF,
    ];

    public static readonly int[] InterestingInts =
    [
        0, 1, -1, 2, 127, 128, 255, 256, -128, -129, 32767, 32768, 65535, 65536, -32768, -32769,
        0x7FFFFFFF, unchecked((int)0x80000000), 0x7F7F7F7F, 0x01020304, unchecked((int)0xDEADBEEF), 0x12345678,
    ];

    public static readonly long[] InterestingLongs =
    [
        0L, 1L, -1L, 0x7FFFFFFFFFFFFFFFL, unchecked((long)0x8000000000000000UL), 0xFFFFFFFFL, 0x100000000L,
        -4294967296L, 0x0123456789ABCDEFL, 0x7FFFFFFFL, 0x80000000L, 255L, 256L, 65535L, 65536L, -32768L, 32768L, 200L,
    ];

    public static int InterestingInt(SplitMix64 g) =>
        g.Below(4) < 2 ? InterestingInts[g.Below(InterestingInts.Length)] : unchecked((int)g.Next());

    public static long InterestingLong(SplitMix64 g) =>
        g.Below(4) < 2 ? InterestingLongs[g.Below(InterestingLongs.Length)] : unchecked((long)g.Next());

    /// <summary>0..8 arbitrary UTF-16 units (including lone surrogates and NUL).</summary>
    public static string ShortString(SplitMix64 g)
    {
        int len = g.Below(9);
        var sb = new StringBuilder();
        for (int i = 0; i < len; i++)
        {
            sb.Append((char)g.Below(0x10000));
        }

        return sb.ToString();
    }

    /// <summary>Test string for writeUTF: empty, ASCII, any UTF-16 unit, surrogate pairs, region boundaries, NUL, just under/over 65535 bytes, etc.</summary>
    public static string GenString(SplitMix64 g)
    {
        int kind = g.Below(12);
        var sb = new StringBuilder();
        switch (kind)
        {
            case 0:
                break;
            case 1:
            {
                int len = 1 + g.Below(48);
                for (int i = 0; i < len; i++)
                {
                    sb.Append((char)(0x20 + g.Below(0x5F)));
                }

                break;
            }

            case 2:
            {
                int len = 1 + g.Below(64);
                for (int i = 0; i < len; i++)
                {
                    sb.Append((char)g.Below(0x10000));
                }

                break;
            }

            case 3:
            {
                int pairs = 1 + g.Below(24);
                for (int i = 0; i < pairs; i++)
                {
                    sb.Append((char)(0xD800 + g.Below(0x400)));
                    sb.Append((char)(0xDC00 + g.Below(0x400)));
                }

                break;
            }

            case 4:
            {
                int len = 1 + g.Below(32);
                for (int i = 0; i < len; i++)
                {
                    sb.Append((char)EdgeUnits[g.Below(7)]);
                }

                break;
            }

            case 5:
            {
                int len = 1 + g.Below(64);
                for (int i = 0; i < len; i++)
                {
                    int r = g.Below(8);
                    sb.Append((char)(RegionLo[r] + g.Below(RegionHi[r] - RegionLo[r] + 1)));
                }

                break;
            }

            case 6:
                GenNearLimit(g, sb);
                break;
            case 7:
            {
                int len = LongAsciiLengths[g.Below(11)];
                char c = (char)('a' + g.Below(26));
                for (int i = 0; i < len; i++)
                {
                    sb.Append(c);
                }

                break;
            }

            case 8:
            {
                int len = 1 + g.Below(40);
                for (int i = 0; i < len; i++)
                {
                    if (g.Below(2) == 0)
                    {
                        sb.Append((char)(0xD800 + g.Below(0x800)));
                    }
                    else
                    {
                        sb.Append((char)('a' + g.Below(26)));
                    }
                }

                break;
            }

            default:
            {
                int len = 1 + g.Below(100);
                for (int i = 0; i < len; i++)
                {
                    int r = g.Below(6);
                    char c;
                    switch (r)
                    {
                        case 0: c = (char)(0x20 + g.Below(0x5F)); break;
                        case 1: c = (char)(0x80 + g.Below(0x780)); break;
                        case 2: c = (char)(0x800 + g.Below(0xD800 - 0x800)); break;
                        case 3: c = (char)(0xD800 + g.Below(0x400)); break;
                        case 4: c = (char)(0xDC00 + g.Below(0x400)); break;
                        default: c = (char)0; break;
                    }

                    sb.Append(c);
                }

                break;
            }
        }

        return sb.ToString();
    }

    /// <summary>A short string (0..24 units) used to build valid data that is then corrupted to exercise readUTF.</summary>
    public static string GenSmallString(SplitMix64 g)
    {
        int len = g.Below(25);
        var sb = new StringBuilder();
        for (int i = 0; i < len; i++)
        {
            int r = g.Below(7);
            char c;
            switch (r)
            {
                case 0: c = (char)(0x20 + g.Below(0x5F)); break;
                case 1: c = (char)(0x80 + g.Below(0x780)); break;
                case 2: c = (char)(0x800 + g.Below(0xD800 - 0x800)); break;
                case 3: c = (char)(0xD800 + g.Below(0x400)); break;
                case 4: c = (char)(0xDC00 + g.Below(0x400)); break;
                case 5: c = (char)(0xE000 + g.Below(0x2000)); break;
                default: c = (char)0; break;
            }

            sb.Append(c);
        }

        return sb.ToString();
    }

    /// <summary>A string whose encoded length is 65529..65541 bytes (around the 65535 boundary: just fits, then exceeds).</summary>
    private static void GenNearLimit(SplitMix64 g, StringBuilder sb)
    {
        long target = 65529 + g.Below(13);
        var block = new char[16];
        var blen = new int[16];
        for (int k = 0; k < 16; k++)
        {
            int r = g.Below(4);
            char c;
            int size;
            if (r == 0)
            {
                c = (char)(0x20 + g.Below(0x5F));
                size = 1;
            }
            else if (r == 1)
            {
                c = (char)(0x80 + g.Below(0x780));
                size = 2;
            }
            else if (r == 2)
            {
                c = (char)(0x800 + g.Below(0xD800 - 0x800));
                size = 3;
            }
            else
            {
                c = (char)0;
                size = 2;
            }

            block[k] = c;
            blen[k] = size;
        }

        long len = 0;
        int idx = 0;
        while (true)
        {
            int sz = blen[idx & 15];
            if (len + sz > target)
            {
                break;
            }

            sb.Append(block[idx & 15]);
            len += sz;
            idx++;
        }

        long rem = target - len;
        while (rem > 0)
        {
            if (rem >= 2)
            {
                sb.Append((char)(0x80 + g.Below(0x780)));
                rem -= 2;
            }
            else
            {
                sb.Append((char)(0x20 + g.Below(0x5F)));
                rem -= 1;
            }
        }
    }
}
