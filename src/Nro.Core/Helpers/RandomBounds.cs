namespace Nro.Core;

/// <summary>Argument checks and the rounding correction shared by the <see cref="Interfaces.IRandomProvider"/> implementations (Java 17 <c>RandomSupport</c>).</summary>
internal static class RandomBounds
{
    internal static void CheckPositive(long bound)
    {
        if (bound <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bound), bound, "bound must be positive");
        }
    }

    internal static void CheckPositiveFinite(double bound)
    {
        if (!(bound > 0.0 && bound < double.PositiveInfinity))
        {
            throw new ArgumentOutOfRangeException(nameof(bound), bound, "bound must be finite and positive");
        }
    }

    /// <summary>Java <c>RandomSupport.boundedNextDouble</c>: <c>r * bound</c>, corrected to the largest double below <paramref name="bound"/> when rounding reached it.</summary>
    internal static double ScaleDouble(double unit, double bound)
    {
        double r = unit * bound;
        return r >= bound ? Math.BitDecrement(bound) : r;
    }

    /// <summary>Java <c>RandomSupport.boundedNextFloat</c>: the same with single precision.</summary>
    internal static float ScaleFloat(float unit, float bound)
    {
        float r = unit * bound;
        return r >= bound ? MathF.BitDecrement(bound) : r;
    }
}
