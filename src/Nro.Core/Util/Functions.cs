using System.Text;
using System.Text.RegularExpressions;

namespace Nro.Core.Util;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/Utils/Functions.java (dòng 1–61).
public static class Functions
{
    // Mẫu lọc chat, giữ nguyên từng mục và thứ tự từ Java (Java: REGEX).
    private const string SpamRegex = "\\b("
        + "dkm|đkm|đbrr|địt|đĩ|đỹ|cm|cmm|lồn|buồi|cc|ôm cl|mẹ mày|cặc|đụ|"
        + "fuck|damn|clmm|dcmm|cl|tml|đ*t|c*c|dit|d*t|c.a.c|l.o.n|c.ặ.c|l.ồ.n|"
        + "b.u.ồ.i|bu*i|đặc cầu|đốn lầu|bú cu|buscu|đm|cc|đb|db|lol|"
        + "nhu lon|nhu cac|vc|vl|vãi|đéo|đờ mờ|đờ cờ mờ|clgt|dell|mẹ|cứt|"
        + "shit|idiot|khốn|xiên chết|cụ|giao phối|thiểu năng|ngáo|chó|dog|"
        + "đcmm|vcl|vkl|đ!t|d!t|đụyt|dyt|ngu|óc|"
        + ".com|.net|.online|.vn|.pw|.pro|.org|.info|.ml|.ga|.gq|.cf|.fun|.xyz|.io|.club"
        + ")\\b";

    // Biên dịch mẫu không phân biệt hoa thường như Java (Java: pattern).
    private static readonly Regex SpamPattern = new(SpamRegex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    // Kiểm tra chuỗi chat theo mẫu spam Java; null hoặc rỗng là false (Java: isSpam).
    public static bool IsSpam(string? text) => !string.IsNullOrEmpty(text) && SpamPattern.IsMatch(text.ToLowerInvariant());

    // Ép long về int với trần một phía; số âm tràn giữ phép ép Java (Java: maxint).
    public static int MaxInt(long value) => unchecked((int)(value > int.MaxValue ? int.MaxValue : value));

    // Sinh ký tự số hoặc chữ in hoa qua nguồn ngẫu nhiên được tiêm, đúng thứ tự hai lần rút Java (Java: generateRandomCharacters).
    public static string GenerateRandomCharacters(int quantity, IRandomProvider random)
    {
        ArgumentNullException.ThrowIfNull(random);
        var output = new StringBuilder();
        for (int i = 0; i < quantity; i++)
        {
            int type = random.NextInt(2);
            char generated = type == 0 ? (char)(random.NextInt(10) + '0') : (char)(random.NextInt(26) + 'A');
            output.Append(generated);
        }

        return output.ToString();
    }

    // Chờ bất đồng bộ có hủy; không dùng Thread.Sleep trong tick (Java: sleep).
    public static Task SleepAsync(long milliseconds, CancellationToken cancellationToken = default)
    {
        if (milliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(milliseconds));
        }

        return Task.Delay(TimeSpan.FromMilliseconds(milliseconds), cancellationToken);
    }

    // Chờ một mili giây bất đồng bộ có hủy; không gọi Thread.Sleep (Java: sleep1Milis).
    public static Task SleepOneMillisecondAsync(CancellationToken cancellationToken = default) => SleepAsync(1, cancellationToken);
}
