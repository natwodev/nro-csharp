using System.Text;

namespace Nro.Core.Util;

// Tiện ích chuỗi: sinh chuỗi ngẫu nhiên gồm chữ và số ASCII
// Nguồn Java: Utils/StringUtil.java
public static class StringUtil
{
    // Mã ký tự nhỏ nhất được rút (Java: leftLimit)
    private const int LeftLimit = 48;   // chữ số '0'
    // Mã ký tự lớn nhất được rút (Java: rightLimit)
    private const int RightLimit = 122; // chữ 'z'

    // Chuỗi ngẫu nhiên dài length ký tự gồm chữ và số ASCII ([0-9A-Za-z]), dùng Random.Shared (Java: randomText(int length))
    // Java rút random.ints(48, 123) (đều trong [48, 122]) rồi bỏ các ký tự giữa '9' và 'A', giữa 'Z' và 'a'; ở đây mỗi ký tự được nhận cũng rút đúng
    // như vậy (lấy mẫu loại bỏ - rejection sampling; phân bố trên 62 ký tự là đều)
    public static string RandomText(int length) => RandomText(length, Random.Shared.Next);

    // Như RandomText(int) nhưng nhận nguồn ngẫu nhiên từ ngoài để test cho kết quả cố định: nextInt(minInclusive, maxExclusive) trả số nguyên đều
    // trong khoảng đó (cùng dạng Random.Next(int, int); chuyển từ nguồn ngẫu nhiên của dự án sang)
    // Ném ArgumentOutOfRangeException nếu length âm (Java: Stream.limit ném IllegalArgumentException)
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
