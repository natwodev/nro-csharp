namespace Nro.Core.Util;

// IRandomProvider tất định dùng cho test và phát lại: cùng seed thì ra cùng dãy số với Random của Java tạo bằng seed đó
// Nguồn Java: java.util.Random, port đúng từng bit (bộ sinh đồng dư tuyến tính 48 bit, thuật toán JDK 17 cho nextInt(bound), nextLong(bound), nextDouble(bound), nextFloat(bound));
// đã đối chiếu với JDK thật trong UtilDifferentialTests.
// An toàn đa luồng: mọi lệnh gọi đều lấy khóa, nên dãy số một luồng nhận được vẫn tất định khi đó là luồng gọi duy nhất.
public sealed class SeededRandomProvider : IRandomProvider
{
    // Hệ số nhân của bộ sinh (Java: Random.multiplier)
    private const long Multiplier = 0x5DEECE66DL;
    // Số cộng của bộ sinh (Java: Random.addend)
    private const long Addend = 0xBL;
    // Mặt nạ giữ 48 bit thấp của trạng thái (Java: Random.mask)
    private const long Mask = (1L << 48) - 1;
    // 2^-53: đổi 53 bit ngẫu nhiên thành số double trong [0, 1) (Java: Random.DOUBLE_UNIT)
    private const double DoubleUnit = 1.0 / (1L << 53);
    // 2^-24: đổi 24 bit ngẫu nhiên thành số float trong [0, 1) (Java: next(24) / ((float)(1 << 24)))
    private const float FloatUnit = 1.0f / (1 << 24);

    // Khóa bảo vệ state khi nhiều luồng gọi cùng lúc
    private readonly object gate = new();
    // Trạng thái 48 bit hiện tại của bộ sinh (Java: Random.seed)
    private long state;

    // Trạng thái ban đầu giống Random của Java tạo bằng seed này (Java: new Random(seed))
    public SeededRandomProvider(long seed)
    {
        state = (seed ^ Multiplier) & Mask;
    }

    // Một số int bất kỳ, phân bố đều, gồm cả số âm (Java: Random.nextInt())
    public int NextInt()
    {
        lock (gate)
        {
            return Next(32);
        }
    }

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không dương (Java: Random.nextInt(bound))
    public int NextInt(int bound)
    {
        RandomBounds.CheckPositive(bound);
        lock (gate)
        {
            int r = Next(31);
            int m = bound - 1;
            if ((bound & m) == 0)
            {
                // bound là lũy thừa của 2: lấy các bit cao
                return (int)((bound * (long)r) >> 31);
            }

            for (int u = r; unchecked((u - (r = u % bound)) + m) < 0; u = Next(31))
            {
                // loại các ứng viên rơi vào nhóm cuối không đủ
            }

            return r;
        }
    }

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không dương (Java 17: Random.nextLong(bound))
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
                // loại các ứng viên làm lệch phân bố
            }

            return r;
        }
    }

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không phải số dương hữu hạn; ghép 26 + 27 bit thành 53 bit (Java 17: Random.nextDouble(bound))
    public double NextDouble(double bound)
    {
        RandomBounds.CheckPositiveFinite(bound);
        lock (gate)
        {
            double unit = ((((long)Next(26)) << 27) + Next(27)) * DoubleUnit;
            return RandomBounds.ScaleDouble(unit, bound);
        }
    }

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không phải số dương hữu hạn; dùng 24 bit (Java 17: Random.nextFloat(bound))
    public float NextFloat(float bound)
    {
        RandomBounds.CheckPositiveFinite(bound);
        lock (gate)
        {
            float unit = Next(24) * FloatUnit;
            return RandomBounds.ScaleFloat(unit, bound);
        }
    }

    // Số long 64 bit ghép từ hai lần Next(32); người gọi phải đang giữ khóa (Java: Random.nextLong())
    private long NextLongUnlocked() => unchecked(((long)Next(32) << 32) + Next(32));

    // Bước bộ sinh một lần rồi trả về bits bit cao của trạng thái; người gọi phải đang giữ khóa (Java: Random.next(int bits))
    private int Next(int bits)
    {
        state = unchecked((state * Multiplier) + Addend) & Mask;
        return unchecked((int)(state >> (48 - bits)));
    }
}
