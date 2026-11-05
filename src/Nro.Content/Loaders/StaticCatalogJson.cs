using System.Text.Json;
using Nro.Core.Templates;

namespace Nro.Content.Loaders;

// Parse JSON định dạng dữ liệu tĩnh với thông báo lỗi gắn bước và hàng. Nguồn Java: nro/server/Manager.java (dòng 351–380, 590–600).
internal static class StaticCatalogJson
{
    // Parse mảng chi tiết part dạng [[iconId, dx, dy], ...]. (Java: part.data)
    public static List<PartDetail> PartDetails(string json, string stepName, int rowId)
    {
        JsonElement root = RootArray(json, stepName, rowId, "data");
        var result = new List<PartDetail>(root.GetArrayLength());
        int index = 0;
        foreach (JsonElement detail in root.EnumerateArray())
        {
            JsonElement row = EmbeddedArray(detail, stepName, rowId, $"data[{index}]");
            if (row.GetArrayLength() < 3)
            {
                throw new CatalogLoadException(stepName, $"Hàng id {rowId} có data[{index}] cần ít nhất ba số [iconId, dx, dy].");
            }

            // Java lấy [0..2] và bỏ qua dữ liệu dư trong JSONArray.
            int iconId = Int(row[0], stepName, rowId, $"data[{index}][0]");
            int dx = Int(row[1], stepName, rowId, $"data[{index}][1]");
            int dy = Int(row[2], stepName, rowId, $"data[{index}][2]");
            result.Add(new PartDetail(
                StaticCatalogValidation.Short(iconId, stepName, "iconId", rowId),
                StaticCatalogValidation.SByte(dx, stepName, "dx", rowId),
                StaticCatalogValidation.SByte(dy, stepName, "dy", rowId)));
            index++;
        }

        return result;
    }

    // Parse mảng frame đầu dạng [int, ...]. (Java: array_head_2_frames.data)
    public static List<int> Frames(string json, string stepName, int rowId)
    {
        JsonElement root = RootArray(json, stepName, rowId, "data");
        var result = new List<int>(root.GetArrayLength());
        int index = 0;
        foreach (JsonElement value in root.EnumerateArray())
        {
            result.Add(Int(value, stepName, rowId, $"data[{index}]"));
            index++;
        }

        return result;
    }

    private static JsonElement RootArray(string json, string stepName, int rowId, string field)
    {
        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field} phải là mảng JSON.");
            }

            return document.RootElement.Clone();
        }
        catch (JsonException exception)
        {
            throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field} JSON không hợp lệ.", exception);
        }
    }

    private static JsonElement EmbeddedArray(JsonElement value, string stepName, int rowId, string field)
    {
        if (value.ValueKind == JsonValueKind.Array)
        {
            return value;
        }

        if (value.ValueKind == JsonValueKind.String)
        {
            try
            {
                using JsonDocument nested = JsonDocument.Parse(value.GetString() ?? String.Empty);
                if (nested.RootElement.ValueKind == JsonValueKind.Array)
                {
                    return nested.RootElement.Clone();
                }
            }
            catch (JsonException exception)
            {
                throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field} chuỗi JSON không hợp lệ.", exception);
            }
        }

        throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field} phải là mảng JSON hoặc chuỗi JSON.");
    }

    private static int Int(JsonElement value, string stepName, int rowId, string field)
    {
        string? text = value.ValueKind == JsonValueKind.String ? value.GetString() : value.ValueKind == JsonValueKind.Number ? value.GetRawText() : null;
        return Int32.TryParse(text, System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out int result)
            ? result
            : throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field} phải là Int32 JSON.");
    }
}
