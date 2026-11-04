using Nro.Data.Db;
using System.Buffers;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nro.Data.Repositories.Event;

// Lưu và tải dữ liệu JSON của các sự kiện mùa trên toàn máy chủ từ bảng event.
// Nguồn Java: jbcd/dao/EventDAO.java (trạng thái static chuyển cho EventSaveState của nơi gọi; SaveAsync cập nhật bốn hàng trong một giao dịch, lỗi cơ sở dữ liệu được truyền lên thay vì bị nuốt).
public sealed class EventRepository : IEventRepository
{
    private const string HungVuongName = "hung_vuong";
    private const string WomensDayName = "international_womens_day";
    private const string LunaNewYearName = "luna_new_year";
    private const string ChristmasName = "christ_mas";

    // Câu lệnh cố định; chỉ tên sự kiện là tham số và citext giữ so sánh không phân biệt hoa thường như collation _ci của MySQL.
    private const string SelectDataSql = "select data from event where name = ? order by id limit 1";
    private const string UpdateDataSql = "update event set data = ? where name = ?";

    private static readonly Action<ILogger, string, Exception?> LoadFailed = LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(1, nameof(LoadFailed)),
        "Could not read the data of event '{EventName}' (the JSON is not what EventDAO expects); the keys read before the failure are kept");

    private static readonly Action<ILogger, string, Exception?> SaveMatchedNothing = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(2, nameof(SaveMatchedNothing)),
        "Could not update event '{EventName}': no such row");

    private readonly NroDatabase _database;
    private readonly ILogger _logger;

    // Tạo kho sự kiện với truy cập cơ sở dữ liệu và bộ ghi nhật ký tùy chọn.
    public EventRepository(NroDatabase database, ILogger<EventRepository>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(database);
        _database = database;
        _logger = logger ?? NullLogger<EventRepository>.Instance;
    }

    // Tải số hạt dưa đã nhận của sự kiện Hùng Vương; trả null khi không có hàng (Java: loadHungVuongEvent).
    public async Task<HungVuongEventData?> LoadHungVuongEventAsync(CancellationToken cancellationToken = default)
    {
        var data = await ReadDataAsync(HungVuongName, cancellationToken);
        if (data is null)
        {
            return null;
        }

        long? receiveMelonSeed = null;
        try
        {
            using var json = ParseObject(data);
            if (json.RootElement.TryGetProperty("ReceiveMelonSeed", out var value))
            {
                receiveMelonSeed = GsonNumber.GetAsLong(value);
            }
        }
        catch (Exception error) when (error is JsonException or FormatException)
        {
            LoadFailed(_logger, HungVuongName, error);
        }

        return new HungVuongEventData(receiveMelonSeed);
    }

    // Tải các chỉ số sự kiện Quốc tế Phụ nữ; lỗi JSON giữ các khóa đã đọc trước lỗi như Java (Java: loadInternationalWomensDayEvent).
    public async Task<InternationalWomensDayEventData?> LoadInternationalWomensDayEventAsync(CancellationToken cancellationToken = default)
    {
        var data = await ReadDataAsync(WomensDayName, cancellationToken);
        if (data is null)
        {
            return null;
        }

        int? dame = null, hp = null, mp = null, point = null, stage = null;
        try
        {
            using var json = ParseObject(data);
            var root = json.RootElement;

            // Giữ thứ tự Java để lỗi chỉ giữ các giá trị đã đọc trước đó.
            if (root.TryGetProperty("damePrecent", out var dameValue))
            {
                dame = GsonNumber.GetAsInt(dameValue);
            }

            if (root.TryGetProperty("hpPrecent", out var hpValue))
            {
                hp = GsonNumber.GetAsInt(hpValue);
            }

            if (root.TryGetProperty("mpPrecent", out var mpValue))
            {
                mp = GsonNumber.GetAsInt(mpValue);
            }

            if (root.TryGetProperty("eventPoint", out var pointValue))
            {
                point = GsonNumber.GetAsInt(pointValue);
            }

            if (root.TryGetProperty("lastExpRewardStage", out var stageValue))
            {
                stage = GsonNumber.GetAsInt(stageValue);
            }
        }
        catch (Exception error) when (error is JsonException or FormatException)
        {
            LoadFailed(_logger, WomensDayName, error);
        }

        return new InternationalWomensDayEventData(dame, hp, mp, point, stage);
    }

    // Tải điểm và mốc thưởng của sự kiện; thiếu hoặc không đọc được một khóa thì trả null (Java: loadEvent).
    public async Task<EventPointData?> LoadEventAsync(string eventName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(eventName);
        var data = await ReadDataAsync(eventName, cancellationToken);
        if (data is null)
        {
            return null;
        }

        try
        {
            using var json = ParseObject(data);
            var root = json.RootElement;

            // Java đọc cả hai khóa trước khi áp dụng; khóa thiếu gây NullPointerException.
            if (!root.TryGetProperty("eventPoint", out var pointValue) || !root.TryGetProperty("lastExpRewardStage", out var stageValue))
            {
                throw new FormatException("A required key is missing.");
            }

            return new EventPointData(GsonNumber.GetAsInt(pointValue), GsonNumber.GetAsInt(stageValue));
        }
        catch (Exception error) when (error is JsonException or FormatException)
        {
            LoadFailed(_logger, eventName, error);
            return null;
        }
    }

    // Tải dữ liệu sự kiện Tết Nguyên Đán (Java: loadLunaNewYearEvent).
    public Task<EventPointData?> LoadLunaNewYearEventAsync(CancellationToken cancellationToken = default) =>
        LoadEventAsync(LunaNewYearName, cancellationToken);

    // Tải dữ liệu sự kiện Giáng sinh (Java: loadChristMasEvent).
    public Task<EventPointData?> LoadChristMasEventAsync(CancellationToken cancellationToken = default) =>
        LoadEventAsync(ChristmasName, cancellationToken);

    // Lưu bốn sự kiện trong một giao dịch và cảnh báo riêng hàng không được cập nhật (Java: save).
    public async Task<EventSaveResult> SaveAsync(EventSaveState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);

        // Văn bản JSON khớp JsonObject.toString của Gson: khóa theo thứ tự chèn, không có khoảng trắng.
        var womensDay = WriteJson(
            ("damePrecent", state.DamePercent),
            ("hpPrecent", state.HpPercent),
            ("mpPrecent", state.MpPercent),
            ("eventPoint", state.WomensDayEventPoint),
            ("lastExpRewardStage", state.WomensDayLastExpRewardStage));
        var lunaNewYear = WriteJson(("eventPoint", state.LunaNewYearEventPoint), ("lastExpRewardStage", state.LunaNewYearLastExpRewardStage));
        var christmas = WriteJson(("eventPoint", state.ChristmasEventPoint), ("lastExpRewardStage", state.ChristmasLastExpRewardStage));
        var hungVuong = WriteJson(("ReceiveMelonSeed", state.ReceiveMelonSeed));

        var result = await _database.InTransactionAsync(
            async transaction =>
            {
                var rows1 = await transaction.ExecuteUpdateAsync(UpdateDataSql, [womensDay, WomensDayName], cancellationToken);
                var rows2 = await transaction.ExecuteUpdateAsync(UpdateDataSql, [lunaNewYear, LunaNewYearName], cancellationToken);
                var rows3 = await transaction.ExecuteUpdateAsync(UpdateDataSql, [christmas, ChristmasName], cancellationToken);
                var rows4 = await transaction.ExecuteUpdateAsync(UpdateDataSql, [hungVuong, HungVuongName], cancellationToken);
                return new EventSaveResult(rows1, rows2, rows3, rows4);
            },
            cancellationToken);

        if (result.WomensDayRows == 0)
        {
            SaveMatchedNothing(_logger, WomensDayName, null);
        }

        if (result.LunaNewYearRows == 0)
        {
            SaveMatchedNothing(_logger, LunaNewYearName, null);
        }

        if (result.ChristmasRows == 0)
        {
            SaveMatchedNothing(_logger, ChristmasName, null);
        }

        if (result.HungVuongRows == 0)
        {
            SaveMatchedNothing(_logger, HungVuongName, null);
        }

        return result;
    }

    private async Task<string?> ReadDataAsync(string eventName, CancellationToken cancellationToken)
    {
        using var rows = await _database.ExecuteQueryAsync(SelectDataSql, [eventName], cancellationToken);
        return rows.Next() ? rows.GetObject("data") as string : null;
    }

    private static JsonDocument ParseObject(string data)
    {
        var document = JsonDocument.Parse(data);
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            document.Dispose();
            throw new FormatException("The event data is not a JSON object.");
        }

        return document;
    }

    private static string WriteJson(params (string Key, long Value)[] properties)
    {
        var buffer = new ArrayBufferWriter<byte>();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartObject();
            foreach (var (key, value) in properties)
            {
                writer.WriteNumber(key, value);
            }

            writer.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(buffer.WrittenSpan);
    }

    // Chuyển giá trị JSON theo getAsInt/getAsLong của Gson; lỗi ném FormatException như ngoại lệ runtime của Java.
    internal static class GsonNumber
    {
        // Trả số nguyên 32 bit theo quy tắc Gson, gồm cả tràn số (Java: JsonElement.getAsInt).
        public static int GetAsInt(JsonElement element) => unchecked((int)Read(element, int.MinValue, int.MaxValue, 32));

        // Trả số nguyên 64 bit theo quy tắc Gson, gồm cả tràn số (Java: JsonElement.getAsLong).
        public static long GetAsLong(JsonElement element) => Read(element, long.MinValue, long.MaxValue, 64);

        private static long Read(JsonElement element, long min, long max, int bits)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Number:
                    return FromNumberText(element.GetRawText(), min, max, bits);
                case JsonValueKind.String:
                    // Integer.parseInt / Long.parseLong của văn bản: chữ số thuần, dấu tùy chọn, không khoảng trắng.
                    return long.TryParse(element.GetString(), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsed) && parsed >= min && parsed <= max
                        ? parsed
                        : throw new FormatException("The text is not an integer.");
                case JsonValueKind.Array when element.GetArrayLength() == 1:
                    return Read(element[0], min, max, bits); // JsonArray.getAsInt của mảng một phần tử
                default:
                    throw new FormatException("The value is not a number.");
            }
        }

        // LazilyParsedNumber.intValue()/longValue(): thử số nguyên chính xác, nếu không lấy bit thấp của BigDecimal đã cắt phần thập phân.
        private static long FromNumberText(string text, long min, long max, int bits)
        {
            if (long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var exact))
            {
                return bits == 64 ? exact : unchecked((int)exact);
            }

            if (!decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                throw new FormatException("The number is out of the supported range.");
            }

            var truncated = new BigInteger(decimal.Truncate(number));
            var mask = (BigInteger.One << bits) - BigInteger.One;
            var low = (ulong)(truncated & mask);
            return bits == 64 ? unchecked((long)low) : unchecked((int)(uint)low);
        }
    }
}
