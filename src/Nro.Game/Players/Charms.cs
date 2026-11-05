using System.Globalization;
using System.Text.Json;
using Nro.Game.Players.Ports;

namespace Nro.Game.Players;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Charms.java (dòng 5–208).
public class Charms : IDisposable
{
    // Lưu thời điểm hết hạn bùa trí tuệ (Java: tdTriTue).
    public long WisdomCharmExpiry { get; set; }

    // Lưu thời điểm hết hạn bùa mạnh mẽ (Java: tdManhMe).
    public long StrengthCharmExpiry { get; set; }

    // Lưu thời điểm hết hạn bùa da trâu (Java: tdDaTrau).
    public long ToughnessCharmExpiry { get; set; }

    // Lưu thời điểm hết hạn bùa oai hùng (Java: tdOaiHung).
    public long HeroismCharmExpiry { get; set; }

    // Lưu thời điểm hết hạn bùa bất tử (Java: tdBatTu).
    public long ImmortalityCharmExpiry { get; set; }

    // Lưu thời điểm hết hạn bùa dẻo dai (Java: tdDeoDai).
    public long EnduranceCharmExpiry { get; set; }

    // Lưu thời điểm hết hạn bùa thu hút (Java: tdThuHut).
    public long AttractionCharmExpiry { get; set; }

    // Lưu thời điểm hết hạn bùa luyện tập đệ tử (Java: tdDeTu).
    public long PetTrainingCharmExpiry { get; set; }

    // Lưu thời điểm hết hạn bùa luyện tập đệ tử x2 (Java: tdDeTu2).
    public long PetTrainingCharmX2Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa luyện tập đệ tử x3 (Java: tdDeTu3).
    public long PetTrainingCharmX3Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa luyện tập đệ tử x4 (Java: tdDeTu4).
    public long PetTrainingCharmX4Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa luyện tập đệ tử x5 (Java: tdDeTu5).
    public long PetTrainingCharmX5Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa luyện tập đệ tử x7 (Java: tdDeTu7).
    public long PetTrainingCharmX7Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa luyện tập đệ tử x10 (Java: tdDeTu10).
    public long PetTrainingCharmX10Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa luyện tập đệ tử x20 (Java: tdDeTu20).
    public long PetTrainingCharmX20Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa trí tuệ x3 (Java: tdTriTue3).
    public long WisdomCharmX3Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa trí tuệ x4 (Java: tdTriTue4).
    public long WisdomCharmX4Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa trí tuệ x5 (Java: tdTriTue5).
    public long WisdomCharmX5Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa trí tuệ x7 (Java: tdTriTue7).
    public long WisdomCharmX7Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa trí tuệ x10 (Java: tdTriTue10).
    public long WisdomCharmX10Expiry { get; set; }

    // Lưu thời điểm hết hạn bùa trí tuệ x20 (Java: tdTriTue20).
    public long WisdomCharmX20Expiry { get; set; }

    // Lưu lần trừ phút gần nhất của bùa trí tuệ x4 (Java: lastTimeSubMinTriTueX4).
    public long LastWisdomCharmX4MinuteSubtractionTime { get; set; }

    // Lưu thời điểm hết hạn bùa da trâu của bang hội (Java: tdDaTrauClan).
    public long ClanToughnessCharmExpiry { get; set; }

    // Lưu thời điểm hết hạn bùa mạnh mẽ của bang hội (Java: tdManhMeClan).
    public long ClanStrengthCharmExpiry { get; set; }

    // Lưu thời điểm hết hạn bùa trí tuệ của bang hội (Java: tdTriTueClan).
    public long ClanWisdomCharmExpiry { get; set; }

    // Cộng thời hạn bùa từ vật phẩm, lấy thời điểm hiện tại qua cổng đồng hồ (Java: addTimeCharms).
    public void AddTimeCharms(int itemId, int minutes, IPlayerClock clock)
    {
        if (itemId == 1950)
        {
            ResetAllCharms();
            return;
        }

        ArgumentNullException.ThrowIfNull(clock);
        long now = clock.NowMilliseconds;
        switch (itemId)
        {
            case 213: WisdomCharmExpiry = Extend(WisdomCharmExpiry, minutes, now); break;
            case 214: StrengthCharmExpiry = Extend(StrengthCharmExpiry, minutes, now); break;
            case 215: ToughnessCharmExpiry = Extend(ToughnessCharmExpiry, minutes, now); break;
            case 216: HeroismCharmExpiry = Extend(HeroismCharmExpiry, minutes, now); break;
            case 217: ImmortalityCharmExpiry = Extend(ImmortalityCharmExpiry, minutes, now); break;
            case 218: EnduranceCharmExpiry = Extend(EnduranceCharmExpiry, minutes, now); break;
            case 219: AttractionCharmExpiry = Extend(AttractionCharmExpiry, minutes, now); break;
            case 522: PetTrainingCharmExpiry = Extend(PetTrainingCharmExpiry, minutes, now); break;
            case 671: WisdomCharmX3Expiry = Extend(WisdomCharmX3Expiry, minutes, now); break;
            case 672: WisdomCharmX4Expiry = Extend(WisdomCharmX4Expiry, minutes, now); break;
            case 1734: PetTrainingCharmX2Expiry = Extend(PetTrainingCharmX2Expiry, minutes, now); break;
            case 1735: PetTrainingCharmX3Expiry = Extend(PetTrainingCharmX3Expiry, minutes, now); break;
            case 1736: PetTrainingCharmX4Expiry = Extend(PetTrainingCharmX4Expiry, minutes, now); break;
            case 1737: PetTrainingCharmX5Expiry = Extend(PetTrainingCharmX5Expiry, minutes, now); break;
            case 1738: PetTrainingCharmX7Expiry = Extend(PetTrainingCharmX7Expiry, minutes, now); break;
            case 1739: PetTrainingCharmX10Expiry = Extend(PetTrainingCharmX10Expiry, minutes, now); break;
            case 1740: PetTrainingCharmX20Expiry = Extend(PetTrainingCharmX20Expiry, minutes, now); break;
            case 1741: WisdomCharmX5Expiry = Extend(WisdomCharmX5Expiry, minutes, now); break;
            case 1742: WisdomCharmX7Expiry = Extend(WisdomCharmX7Expiry, minutes, now); break;
            case 1743: WisdomCharmX10Expiry = Extend(WisdomCharmX10Expiry, minutes, now); break;
            case 1744: WisdomCharmX20Expiry = Extend(WisdomCharmX20Expiry, minutes, now); break;
            case 797: ClanWisdomCharmExpiry = Extend(ClanWisdomCharmExpiry, minutes, now); break;
            case 798: ClanStrengthCharmExpiry = Extend(ClanStrengthCharmExpiry, minutes, now); break;
            case 799: ClanToughnessCharmExpiry = Extend(ClanToughnessCharmExpiry, minutes, now); break;
        }
    }

    // Đặt lại mọi thời điểm hết hạn bùa về không (Java: resetAllCharms).
    public void ResetAllCharms()
    {
        WisdomCharmExpiry = StrengthCharmExpiry = ToughnessCharmExpiry = HeroismCharmExpiry = ImmortalityCharmExpiry = EnduranceCharmExpiry = AttractionCharmExpiry = 0;
        PetTrainingCharmExpiry = PetTrainingCharmX2Expiry = PetTrainingCharmX3Expiry = PetTrainingCharmX4Expiry = PetTrainingCharmX5Expiry = PetTrainingCharmX7Expiry = PetTrainingCharmX10Expiry = PetTrainingCharmX20Expiry = 0;
        WisdomCharmX3Expiry = WisdomCharmX4Expiry = WisdomCharmX5Expiry = WisdomCharmX7Expiry = WisdomCharmX10Expiry = WisdomCharmX20Expiry = 0;
        LastWisdomCharmX4MinuteSubtractionTime = 0;
        ClanToughnessCharmExpiry = ClanStrengthCharmExpiry = ClanWisdomCharmExpiry = 0;
    }

    // Ghi đúng mảng JSON 21 phần tử theo thứ tự Java, chủ ý không ghi bốn trường runtime còn lại (Java: toJSONArray).
    public string ToJsonArray() => $"[{string.Join(',', PersistedValues().Select(value => value.ToString(CultureInfo.InvariantCulture)))}]";

    // Đọc mảng JSON Java 21 phần tử; null hoặc mảng rỗng không thay đổi trạng thái (Java: loadFromJSONArray).
    public void LoadFromJsonArray(string? json)
    {
        if (json is null)
        {
            return;
        }

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement array = document.RootElement;
        if (array.ValueKind != JsonValueKind.Array)
        {
            throw new FormatException("Charms JSON must be an array.");
        }

        JsonElement[] values = array.EnumerateArray().ToArray();
        if (values.Length == 0)
        {
            return;
        }

        WisdomCharmExpiry = ReadLong(values[0]); StrengthCharmExpiry = ReadLong(values[1]); ToughnessCharmExpiry = ReadLong(values[2]); HeroismCharmExpiry = ReadLong(values[3]);
        ImmortalityCharmExpiry = ReadLong(values[4]); EnduranceCharmExpiry = ReadLong(values[5]); AttractionCharmExpiry = ReadLong(values[6]); PetTrainingCharmExpiry = ReadLong(values[7]);
        WisdomCharmX3Expiry = ReadLong(values[8]); WisdomCharmX4Expiry = ReadLong(values[9]); PetTrainingCharmX2Expiry = ReadLong(values[10]); PetTrainingCharmX3Expiry = ReadLong(values[11]);
        PetTrainingCharmX4Expiry = ReadLong(values[12]); PetTrainingCharmX5Expiry = ReadLong(values[13]); PetTrainingCharmX7Expiry = ReadLong(values[14]); PetTrainingCharmX10Expiry = ReadLong(values[15]);
        PetTrainingCharmX20Expiry = ReadLong(values[16]); WisdomCharmX5Expiry = ReadLong(values[17]); WisdomCharmX7Expiry = ReadLong(values[18]); WisdomCharmX10Expiry = ReadLong(values[19]);
        WisdomCharmX20Expiry = ReadLong(values[20]);
    }

    // Hoàn tất vòng đời rỗng đúng Java; Charms không sở hữu tài nguyên để giải phóng (Java: dispose).
    public void Dispose()
    {
    }

    // Kéo dài một mốc hết hạn theo số phút với ngữ nghĩa tràn số long của Java (Java: addTimeCharms).
    private static long Extend(long expiry, int minutes, long now) => unchecked(Math.Max(expiry, now) + unchecked((long)unchecked(minutes * 60) * 1000L));

    // Lấy đúng 21 giá trị bền vững theo chỉ số mảng JSON Java (Java: toJSONArray).
    private IEnumerable<long> PersistedValues() =>
    [
        WisdomCharmExpiry, StrengthCharmExpiry, ToughnessCharmExpiry, HeroismCharmExpiry, ImmortalityCharmExpiry, EnduranceCharmExpiry, AttractionCharmExpiry,
        PetTrainingCharmExpiry, WisdomCharmX3Expiry, WisdomCharmX4Expiry, PetTrainingCharmX2Expiry, PetTrainingCharmX3Expiry, PetTrainingCharmX4Expiry,
        PetTrainingCharmX5Expiry, PetTrainingCharmX7Expiry, PetTrainingCharmX10Expiry, PetTrainingCharmX20Expiry, WisdomCharmX5Expiry, WisdomCharmX7Expiry,
        WisdomCharmX10Expiry, WisdomCharmX20Expiry,
    ];

    // Chuyển từng số JSON qua chuỗi để khớp Long.parseLong(String.valueOf(...)) của Java (Java: loadFromJSONArray).
    private static long ReadLong(JsonElement value) => long.Parse(value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText(), CultureInfo.InvariantCulture);
}
