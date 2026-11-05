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
            if (detail.ValueKind != JsonValueKind.Array || detail.GetArrayLength() != 3)
            {
                throw new CatalogLoadException(stepName, $"Hàng id {rowId} có data[{index}] phải là mảng ba số [iconId, dx, dy].");
            }

            int iconId = Int(detail[0], stepName, rowId, $"data[{index}][0]");
            int dx = Int(detail[1], stepName, rowId, $"data[{index}][1]");
            int dy = Int(detail[2], stepName, rowId, $"data[{index}][2]");
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

    private static int Int(JsonElement value, string stepName, int rowId, string field) => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out int result)
        ? result
        : throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field} phải là Int32 JSON.");
}
