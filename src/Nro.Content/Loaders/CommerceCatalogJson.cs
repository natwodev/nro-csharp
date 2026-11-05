using System.Globalization;
using System.Text.Json;
using Nro.Game.Cards;
using Nro.Game.Items.Core;

namespace Nro.Content.Loaders;

// Parse JSON huy hiệu và radar với lỗi gắn hàng nguồn. Nguồn Java: nro/server/Manager.java (dòng 790–809, 961–991).
internal static class CommerceCatalogJson
{
    // Đọc option huy hiệu và resolve mẫu option. (Java: BagesTemplate.options)
    public static IReadOnlyList<ItemOption> BadgeOptions(string json, int rowId, Func<int, ItemOption> create)
    {
        ArgumentNullException.ThrowIfNull(create);
        // JSONValue.parse của Java trả null cho chuỗi rỗng/null; Java chỉ bỏ qua options trong trường hợp đó.
        if (String.IsNullOrWhiteSpace(json) || String.Equals(json, "null", StringComparison.OrdinalIgnoreCase))
        {
            return Array.Empty<ItemOption>();
        }

        using JsonDocument document = ParseArray(json, BadgesCatalogLoadStep.StepName, rowId, "options");
        var result = new List<ItemOption>(document.RootElement.GetArrayLength());
        int index = 0;
        foreach (JsonElement element in document.RootElement.EnumerateArray())
        {
            RequireObject(element, BadgesCatalogLoadStep.StepName, rowId, $"options[{index}]");
            ItemOption option = create(Int(element, "id", BadgesCatalogLoadStep.StepName, rowId, $"options[{index}].id"));
            option.Param = Int(element, "param", BadgesCatalogLoadStep.StepName, rowId, $"options[{index}].param");
            result.Add(option);
            index++;
        }

        return result.AsReadOnly();
    }

    // Đọc phần thân radar; Java lấy object hợp lệ cuối cùng của mảng. (Java: RadarCard.Head/Body/Leg/Bag)
    public static (short Head, short Body, short Leg, short Bag) RadarBody(string json, int rowId)
    {
        using JsonDocument document = ParseArray(json, RadarCatalogLoadStep.StepName, rowId, "body");
        short head = 0;
        short body = 0;
        short leg = 0;
        short bag = 0;
        int index = 0;
        foreach (JsonElement element in document.RootElement.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                index++;
                continue;
            }

            RequireObject(element, RadarCatalogLoadStep.StepName, rowId, $"body[{index}]");
            head = StaticCatalogValidation.Short(Int(element, "head", RadarCatalogLoadStep.StepName, rowId, $"body[{index}].head"), RadarCatalogLoadStep.StepName, "head", rowId);
            body = StaticCatalogValidation.Short(Int(element, "body", RadarCatalogLoadStep.StepName, rowId, $"body[{index}].body"), RadarCatalogLoadStep.StepName, "body", rowId);
            leg = StaticCatalogValidation.Short(Int(element, "leg", RadarCatalogLoadStep.StepName, rowId, $"body[{index}].leg"), RadarCatalogLoadStep.StepName, "leg", rowId);
            bag = StaticCatalogValidation.Short(Int(element, "bag", RadarCatalogLoadStep.StepName, rowId, $"body[{index}].bag"), RadarCatalogLoadStep.StepName, "bag", rowId);
            index++;
        }

        return (head, body, leg, bag);
    }

    // Đọc option radar theo đúng thứ tự JSON. (Java: RadarCard.Options)
    public static IReadOnlyList<RadarOption> RadarOptions(string json, int rowId)
    {
        using JsonDocument document = ParseArray(json, RadarCatalogLoadStep.StepName, rowId, "options");
        var result = new List<RadarOption>(document.RootElement.GetArrayLength());
        int index = 0;
        foreach (JsonElement element in document.RootElement.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                index++;
                continue;
            }

            RequireObject(element, RadarCatalogLoadStep.StepName, rowId, $"options[{index}]");
            int id = Int(element, "id", RadarCatalogLoadStep.StepName, rowId, $"options[{index}].id");
            int param = StaticCatalogValidation.Short(Int(element, "param", RadarCatalogLoadStep.StepName, rowId, $"options[{index}].param"), RadarCatalogLoadStep.StepName, "option.param", rowId);
            sbyte active = StaticCatalogValidation.SByte(Int(element, "activeCard", RadarCatalogLoadStep.StepName, rowId, $"options[{index}].activeCard"), RadarCatalogLoadStep.StepName, "option.activeCard", rowId);
            result.Add(new RadarOption(id, param, active));
            index++;
        }

        return result.AsReadOnly();
    }

    private static JsonDocument ParseArray(string json, string stepName, int rowId, string field)
    {
        try
        {
            JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                document.Dispose();
                throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field} phải là mảng JSON.");
            }

            return document;
        }
        catch (JsonException exception)
        {
            throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field} JSON không hợp lệ.", exception);
        }
    }

    private static void RequireObject(JsonElement value, string stepName, int rowId, string field)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field} phải là object JSON.");
        }
    }

    private static int Int(JsonElement objectValue, string property, string stepName, int rowId, string field)
    {
        if (!objectValue.TryGetProperty(property, out JsonElement value))
        {
            throw new CatalogLoadException(stepName, $"Hàng id {rowId} thiếu {field}.");
        }

        string? text = value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText();
        return Int32.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out int result)
            ? result
            : throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field} phải là Int32 JSON.");
    }
}
