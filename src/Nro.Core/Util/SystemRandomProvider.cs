namespace Nro.Core.Util;

// IRandomProvider dùng khi chạy thật: an toàn đa luồng, dựa trên Random.Shared (không tạo đối tượng Random nào ở đây)
// Nguồn Java: thay cho biến tĩnh rand = new Random() của Utils/Util.java. Kết quả không lặp lại giữa các lần chạy; cần dãy số lặp lại thì dùng SeededRandomProvider.
public sealed class SystemRandomProvider : IRandomProvider
{
    // Thực thể dùng chung (lớp này không có trạng thái)
    public static readonly SystemRandomProvider Instance = new();

    // Một số int bất kỳ, phân bố đều, gồm cả số âm (Java: Random.nextInt())
    public int NextInt() => (int)Random.Shared.NextInt64(int.MinValue, (long)int.MaxValue + 1);

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không dương (Java: Random.nextInt(bound))
    public int NextInt(int bound)
    {
        RandomBounds.CheckPositive(bound);
        return Random.Shared.Next(bound);
    }

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không dương (Java 17: Random.nextLong(bound))
    public long NextLong(long bound)
    {
        RandomBounds.CheckPositive(bound);
        return Random.Shared.NextInt64(bound);
    }

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không phải số dương hữu hạn (Java 17: Random.nextDouble(bound))
    public double NextDouble(double bound)
    {
        RandomBounds.CheckPositiveFinite(bound);
        return RandomBounds.ScaleDouble(Random.Shared.NextDouble(), bound);
    }

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không phải số dương hữu hạn (Java 17: Random.nextFloat(bound))
    public float NextFloat(float bound)
    {
        RandomBounds.CheckPositiveFinite(bound);
        return RandomBounds.ScaleFloat(Random.Shared.NextSingle(), bound);
    }
}
