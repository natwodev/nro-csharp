namespace Nro.Core.Util;

// Kiểm tra tham số bound và hiệu chỉnh làm tròn, dùng chung cho các lớp cài đặt IRandomProvider
// Nguồn Java: java.util.random.RandomSupport của Java 17 (checkBound, boundedNextDouble, boundedNextFloat); Java ném IllegalArgumentException, ở đây ném ArgumentOutOfRangeException
internal static class RandomBounds
{
    // Ném ArgumentOutOfRangeException nếu bound không dương (Java: RandomSupport.checkBound)
    internal static void CheckPositive(long bound)
    {
        if (bound <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bound), bound, "bound must be positive");
        }
    }

    // Ném ArgumentOutOfRangeException nếu bound không phải số dương hữu hạn (NaN và vô cực đều bị từ chối) (Java: RandomSupport.checkBound)
    internal static void CheckPositiveFinite(double bound)
    {
        if (!(bound > 0.0 && bound < double.PositiveInfinity))
        {
            throw new ArgumentOutOfRangeException(nameof(bound), bound, "bound must be finite and positive");
        }
    }

    // Trả unit * bound; nếu làm tròn chạm đúng bound thì lùi về số double lớn nhất nhỏ hơn bound, để kết quả luôn trong [0, bound) (Java: RandomSupport.boundedNextDouble)
    internal static double ScaleDouble(double unit, double bound)
    {
        double r = unit * bound;
        return r >= bound ? Math.BitDecrement(bound) : r;
    }

    // Giống ScaleDouble nhưng với độ chính xác đơn (float) (Java: RandomSupport.boundedNextFloat)
    internal static float ScaleFloat(float unit, float bound)
    {
        float r = unit * bound;
        return r >= bound ? MathF.BitDecrement(bound) : r;
    }
}
