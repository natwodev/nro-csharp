namespace Nro.Core.Util;

// Nguồn số ngẫu nhiên dùng cho RandomUtil; test cắm nguồn có kịch bản hoặc có seed, code chạy thật dùng SystemRandomProvider (an toàn đa luồng)
// Nguồn Java: thay cho biến tĩnh java.util.Random rand của Utils/Util.java; không lớp nào được tự tạo System.Random riêng.
// Năm hàm đúng là năm lệnh Util.java gọi trên Random; tham số sai xử lý như Java: bound không dương (với số thực: không hữu hạn)
// thì ném ArgumentOutOfRangeException ở chỗ Java ném IllegalArgumentException.
// Lớp cài đặt phải gọi được từ nhiều luồng cùng lúc (Random của Java cũng vậy).
public interface IRandomProvider
{
    // Một số int bất kỳ, phân bố đều, gồm cả số âm (Java: Random.nextInt())
    int NextInt();

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không dương (Java: Random.nextInt(bound))
    int NextInt(int bound);

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không dương (Java 17: Random.nextLong(bound))
    long NextLong(long bound);

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không phải số dương hữu hạn (Java 17: Random.nextDouble(bound))
    double NextDouble(double bound);

    // Một số trong khoảng [0, bound), ném lỗi nếu bound không phải số dương hữu hạn (Java 17: Random.nextFloat(bound))
    float NextFloat(float bound);
}
