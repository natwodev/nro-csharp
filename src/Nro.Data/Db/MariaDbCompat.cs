using Nro.Data.Db;
using System.Text.Json;

namespace Nro.Data.Db;

// Tái hiện trong C# các hàm MariaDB/MySQL mà TopDAO dùng xếp hạng, để PostgreSQL không phải phân tích JSON chứa trong cột văn bản.
// Nguồn Java: jbcd/dao/TopDAO.java (các hàm MariaDB mà TopDAO.java dùng, tái hiện trong C#; giữ CAST unsigned, json-extract/json-unquote và substring-index; JSON [1.] hoặc [-] không chuẩn bị System.Text.Json coi là NULL, khác MariaDB 10.4).
public static class MariaDbCompat
{
    private const ulong TwoToTheSixtyThree = 1UL << 63;

    // Ép chuỗi sang unsigned theo MariaDB, không cảnh báo hay ném lỗi.
    public static ulong CastToUnsigned(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var position = 0;
        while (position < text.Length && IsSpace(text[position]))
        {
            position++;
        }

        var negative = false;
        if (position < text.Length && (text[position] == '+' || text[position] == '-'))
        {
            negative = text[position] == '-';
            position++;
        }

        ulong magnitude = 0;
        var saturated = false;
        while (position < text.Length && text[position] is >= '0' and <= '9')
        {
            if (!saturated)
            {
                var digit = (ulong)(text[position] - '0');
                if (magnitude > (ulong.MaxValue - digit) / 10)
                {
                    magnitude = ulong.MaxValue;
                    saturated = true;
                }
                else
                {
                    magnitude = (magnitude * 10) + digit;
                }
            }

            position++;
        }

        if (!negative)
        {
            return magnitude;
        }

        // Giá trị âm được ép sang signed 64-bit, chặn ở cực tiểu rồi diễn giải lại thành unsigned.
        var clamped = magnitude > TwoToTheSixtyThree ? TwoToTheSixtyThree : magnitude;
        return unchecked(0UL - clamped);
    }

    // Lấy phần tử JSON theo chỉ số, tùy chọn bỏ dấu nháy rồi ép unsigned; trả null tại nơi MariaDB trả SQL NULL.
    public static ulong? JsonIndexToUnsigned(string? json, int index, bool unquote)
    {
        using var document = ParseOrNull(json);
        return document is null ? null : ElementToUnsigned(document.RootElement, index, unquote);
    }

    // Lấy phần tử JSON lồng nhau kiểu player.pet, trong đó mảng ngoài được lưu thành chuỗi JSON.
    public static ulong? NestedJsonIndexToUnsigned(string? json, int outerIndex, int innerIndex)
    {
        using var document = ParseOrNull(json);
        if (document is null)
        {
            return null;
        }

        if (!TryGetElement(document.RootElement, outerIndex, out var outer))
        {
            return null;
        }

        // json-unquote trả nội dung chuỗi JSON; kiểu khác giữ nguyên văn bản JSON.
        var inner = outer.ValueKind == JsonValueKind.String ? SafeGetString(outer) : outer.GetRawText();
        return JsonIndexToUnsigned(inner, innerIndex, unquote: true);
    }

    // Tái hiện substring-index của MySQL bằng so sánh nhị phân; dấu phân cách của game không có chữ cái.
    public static string SubstringIndex(string text, string delimiter, int count)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentException.ThrowIfNullOrEmpty(delimiter);
        if (count == 0)
        {
            return string.Empty;
        }

        if (count > 0)
        {
            var from = 0;
            for (var seen = 0; seen < count; seen++)
            {
                var found = text.IndexOf(delimiter, from, StringComparison.Ordinal);
                if (found < 0)
                {
                    return text;
                }

                if (seen == count - 1)
                {
                    return text[..found];
                }

                from = found + delimiter.Length;
            }

            return text;
        }

        var end = text.Length;
        for (var seen = 0; seen < -count; seen++)
        {
            var found = end == 0 ? -1 : text.LastIndexOf(delimiter, end - 1, StringComparison.Ordinal);
            if (found < 0)
            {
                return text;
            }

            if (seen == -count - 1)
            {
                return text[(found + delimiter.Length)..];
            }

            end = found;
        }

        return text;
    }

    // Lấy số lượng vật phẩm 457 (thỏi vàng) từ items_bag/items_box theo chuỗi hàm SQL gốc của TopDAO.
    public static ulong GoldBarQuantity(string itemList)
    {
        ArgumentNullException.ThrowIfNull(itemList);
        const string Marker = "[457,";
        var afterLast = SubstringIndex(itemList, Marker, -1);
        var untilBracket = SubstringIndex(afterLast, "]", 1);
        var rebuilt = Marker + untilBracket;
        var firstTwoFields = SubstringIndex(rebuilt, ",", 2);
        var withoutBracket = SubstringIndex(firstTwoFields, "]", 1);
        return CastToUnsigned(withoutBracket.Replace(Marker, string.Empty, StringComparison.Ordinal));
    }

    private static ulong? ElementToUnsigned(JsonElement root, int index, bool unquote)
    {
        if (!TryGetElement(root, index, out var element))
        {
            return null;
        }

        return element.ValueKind switch
        {
            JsonValueKind.Number => CastToUnsigned(element.GetRawText()),
            JsonValueKind.String => unquote ? CastToUnsigned(SafeGetString(element)) : 0UL,
            _ => 0UL, // true, false, null, mảng, đối tượng: văn bản bắt đầu bằng chữ cái, '[' hoặc '{'.
        };
    }

    private static bool TryGetElement(JsonElement root, int index, out JsonElement element)
    {
        if (root.ValueKind == JsonValueKind.Array)
        {
            if (index >= 0 && index < root.GetArrayLength())
            {
                element = root[index];
                return true;
            }
        }
        else if (index == 0)
        {
            element = root; // MariaDB coi tài liệu không phải mảng là mảng một phần tử tại $[0].
            return true;
        }

        element = default;
        return false;
    }

    private static JsonDocument? ParseOrNull(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            return null; // MariaDB: JSON không hợp lệ trả SQL NULL, không ném lỗi.
        }
    }

    private static string SafeGetString(JsonElement element)
    {
        try
        {
            return element.GetString() ?? string.Empty;
        }
        catch (InvalidOperationException)
        {
            return string.Empty; // Escape không giải mã được (surrogate đơn lẻ) không có chữ số nên ép thành 0.
        }
    }

    private static bool IsSpace(char c) => c is ' ' or '\t' or '\n' or '\r' or '\v' or '\f';
}
