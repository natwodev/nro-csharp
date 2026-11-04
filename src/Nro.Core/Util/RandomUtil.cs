namespace Nro.Core.Util;

// Các hàm ngẫu nhiên của game: nextInt, nextLong, nextDouble, nextfloat, sáu bản isTrue, getOne, getChanceFromLuck, Crisnext, GioiHannext, GioiHannextdame, shuffleArray
// Nguồn Java: nửa ngẫu nhiên của Utils/Util.java; biến tĩnh Random rand của Java thay bằng IRandomProvider được truyền vào. Giữ đúng phép tính, thứ tự các lần rút số
// và các lỗi ném ra, nên một nguồn có seed sẽ cho lại đúng những gì Java làm.
// CÁC BẢN isTrue KHÔNG THAY THẾ CHO NHAU ĐƯỢC: Java chọn bản theo kiểu tĩnh của tham số và mỗi bản chạy khác nhau, nên ở đây mỗi bản có tên riêng:
// IsTrueInt (int,int: false khi một trong hai không dương, true khi ratio >= total, không bao giờ ném lỗi), IsTrueLong (long,long), IsTrueDouble (double,double),
// IsTrueFloat (float,long), IsTrueLongAccuracy (long,long,int) và IsTrueFloatAccuracy (float,long,int).
// Khi port lệnh Util.isTrue(a, b) của Java, xem kiểu khai báo của a và b: số nguyên viết trực tiếp chọn bản int, biến long chọn bản long, biểu thức double chọn bản double,
// tham số đầu là float và tham số sau là int/long chọn bản float.
public sealed class RandomUtil
{
    // Nguồn số ngẫu nhiên, thay cho Random rand tĩnh của Java
    private readonly IRandomProvider random;

    // Tạo helper trên một nguồn số ngẫu nhiên (chạy thật dùng SystemRandomProvider.Instance, test dùng nguồn có seed hoặc có kịch bản)
    public RandomUtil(IRandomProvider random)
    {
        ArgumentNullException.ThrowIfNull(random);
        this.random = random;
    }

    // Một số trong khoảng [from, to], gồm cả hai đầu: from + rand.nextInt(to - from + 1); to - from + 1 bị tràn vòng trong int, ra <= 0 thì ném lỗi như Java.
    // Ném ArgumentOutOfRangeException khi to nhỏ hơn from (Java: nextInt(int from, int to))
    public int NextInt(int from, int to) => unchecked(from + random.NextInt(to - from + 1));

    // from + rand.nextFloat(to - from + 1), độ chính xác đơn; đầu trên KHÔNG lấy được như với bản số nguyên (Java: nextfloat(float from, float to))
    public float NextFloat(float from, float to) => from + random.NextFloat((to - from) + 1);

    // Một số trong khoảng [0, max); ném ArgumentOutOfRangeException nếu max không dương (Java: nextInt(int max))
    public int NextInt(int max) => random.NextInt(max);

    // Một số trong khoảng [from, to], gồm cả hai đầu: from + rand.nextLong(to - from + 1) (Java: nextLong(long from, long to))
    public long NextLong(long from, long to) => unchecked(from + random.NextLong(to - from + 1));

    // Một số trong khoảng [0, max) (Java: nextLong(long max))
    public long NextLong(long max) => random.NextLong(max);

    // Một số trong khoảng [0, max) (Java: nextDouble(double max))
    public double NextDouble(double max) => random.NextDouble(max);

    // Rút next = nextInt(1000) rồi trả chỉ số đầu tiên có trọng số cộng dồn vượt quá next; trả percen.Length khi tổng trọng số không vượt quá số rút được
    // (trọng số tính theo phần nghìn, không phải phần trăm) (Java: nextInt(int[] percen))
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

    // isTrue(mum + tyLeMayMan * max / 300, max) bằng bản DOUBLE (IsTrueDouble); nếu isCoBonLa thì max được giảm trước còn max * 100 / 133
    // (Java: getChanceFromLuck)
    public bool GetChanceFromLuck(double tyLeMayMan, double mum, double max, bool isCoBonLa)
    {
        if (isCoBonLa)
        {
            max = (max * 100) / 133;
        }

        return IsTrueDouble(mum + ((tyLeMayMan * max) / 300), max);
    }

    // false khi typeRatio hoặc ratio không dương, true (không rút số) khi ratio >= typeRatio, còn lại là nextInt(typeRatio) < ratio.
    // Bản duy nhất không bao giờ ném lỗi (Java: isTrue(int ratio, int typeRatio))
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

    // Trả n1 khi rand.nextInt() % 2 == 0 (rút được số chẵn), ngược lại trả n2 (Java: getOne(n1, n2))
    public int GetOne(int n1, int n2) => random.NextInt() % 2 == 0 ? n1 : n2;

    // nextLong(total) < ratio. Luôn rút số: total không dương thì ném ArgumentOutOfRangeException, còn ratio từ total trở lên cho true chỉ vì mọi số rút được đều nhỏ hơn nó
    // (Java: isTrue(long ratioPercentage, long totalPercentage))
    public bool IsTrueLong(long ratioPercentage, long totalPercentage) => NextLong(totalPercentage) < ratioPercentage;

    // nextDouble(total) < ratio. Luôn rút số; ném ArgumentOutOfRangeException khi total không phải số dương hữu hạn
    // (Java: isTrue(double ratioPercentage, double totalPercentage))
    public bool IsTrueDouble(double ratioPercentage, double totalPercentage) => NextDouble(totalPercentage) < ratioPercentage;

    // ratio nhỏ hơn 1 được coi là phân số nên cả hai số được nhân 100 trước (0.5f, 100 thành 50, 10000); sau đó ratio bị cắt về (long) rồi chuyển cho IsTrueLong
    // (Java: isTrue(float ratioPercentage, long totalPercentage))
    public bool IsTrueFloat(float ratioPercentage, long totalPercentage)
    {
        if (ratioPercentage < 1)
        {
            ratioPercentage *= 100;
            totalPercentage = unchecked(totalPercentage * 100);
        }

        return IsTrueLong(JavaNumbers.ToLong(ratioPercentage), totalPercentage);
    }

    // nextLong(total * accuracy) < ratio && nextInt(accuracy) == 0. Lần rút thứ hai chỉ xảy ra khi phép thử đầu đạt (đoản mạch), và xác suất là
    // ratio / (total * accuracy) / accuracy, tức accuracy bị áp HAI lần (Java: isTrue(long ratioPercentage, long totalPercentage, int accuracy))
    public bool IsTrueLongAccuracy(long ratioPercentage, long totalPercentage, int accuracy) =>
        NextLong(unchecked(totalPercentage * accuracy)) < ratioPercentage && NextInt(accuracy) == 0;

    // Nhân 100 cho phân số như IsTrueFloat, rồi gọi IsTrueLongAccuracy (Java: isTrue(float ratioPercentage, long totalPercentage, int accuracy))
    public bool IsTrueFloatAccuracy(float ratioPercentage, long totalPercentage, int accuracy)
    {
        if (ratioPercentage < 1)
        {
            ratioPercentage *= 100;
            totalPercentage = unchecked(totalPercentage * 100);
        }

        return IsTrueLongAccuracy(JavaNumbers.ToLong(ratioPercentage), totalPercentage, accuracy);
    }

    // Trả (long)(from + rand.nextInt((int)(to - from + 1))) (Java: Crisnext(double from, double to))
    public long Crisnext(double from, double to) => JavaNumbers.ToLong(from + random.NextInt(JavaNumbers.ToInt((to - from) + 1)));

    // Trả (long)rand.nextInt((int)max) (Java: Crisnext(double max))
    public long Crisnext(double max) => random.NextInt(JavaNumbers.ToInt(max));

    // Giống hệt Crisnext(double, double) (Java: GioiHannext(double from, double to))
    public long GioiHannext(double from, double to) => JavaNumbers.ToLong(from + random.NextInt(JavaNumbers.ToInt((to - from) + 1)));

    // Trả from + rand.nextInt((int)(to - from + 1)) dạng double (không ép sang long) (Java: GioiHannextdame(double from, double to))
    public double GioiHannextdame(double from, double to) => from + random.NextInt(JavaNumbers.ToInt((to - from) + 1));

    // Xáo trộn mảng tại chỗ theo Fisher-Yates từ cuối về: j = nextInt(i + 1) với i = length - 1 .. 1 (Java: shuffleArray(int[]))
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
