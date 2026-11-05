using System.Text.Json;
using Nro.Game.World.Maps;

namespace Nro.Content.Loaders;

// Nguồn Java: nro/server/Manager.java (dòng 799–851).
// Parse JSON map có thể là mảng lồng trực tiếp hoặc mảng chuỗi JSON như dump Java. (Java: JSONValue.parse)
internal static class WorldCatalogJson
{
    // Parse các cửa map từ mảng mười phần tử. (Java: waypoints)
    public static List<WayPoint> WayPoints(string json, int mapId)
    {
        using JsonDocument document = ParseArray(NormalizeWayPoints(json, mapId), mapId, "waypoints");
        var wayPoints = new List<WayPoint>();
        int index = 0;
        foreach (JsonElement item in document.RootElement.EnumerateArray())
        {
            JsonElement row = ParseEmbeddedArray(item, mapId, $"waypoints[{index}]");
            RequireLength(row, 10, mapId, $"waypoints[{index}]");
            wayPoints.Add(new WayPoint
            {
                Name = Text(row[0], mapId, $"waypoints[{index}][0]"), MinX = Short(row[1], mapId, $"waypoints[{index}][1]"), MinY = Short(row[2], mapId, $"waypoints[{index}][2]"), MaxX = Short(row[3], mapId, $"waypoints[{index}][3]"), MaxY = Short(row[4], mapId, $"waypoints[{index}][4]"), IsEnter = SByte(row[5], mapId, $"waypoints[{index}][5]") == 1, IsOffline = SByte(row[6], mapId, $"waypoints[{index}][6]") == 1, GoMap = Short(row[7], mapId, $"waypoints[{index}][7]"), GoX = Short(row[8], mapId, $"waypoints[{index}][8]"), GoY = Short(row[9], mapId, $"waypoints[{index}][9]"),
            });
            index++;
        }

        return wayPoints;
    }

    // Parse mảng quái song song như Java. (Java: mobs)
    public static (sbyte[] Temp, sbyte[] Level, int[] Hp, short[] X, short[] Y) Mobs(string json, int mapId)
    {
        using JsonDocument document = ParseArray(json, mapId, "mobs");
        int count = document.RootElement.GetArrayLength();
        var temp = new sbyte[count]; var level = new sbyte[count]; var hp = new int[count]; var x = new short[count]; var y = new short[count];
        int index = 0;
        foreach (JsonElement item in document.RootElement.EnumerateArray())
        {
            JsonElement row = ParseEmbeddedArray(item, mapId, $"mobs[{index}]");
            RequireLength(row, 5, mapId, $"mobs[{index}]");
            temp[index] = SByte(row[0], mapId, $"mobs[{index}][0]"); level[index] = SByte(row[1], mapId, $"mobs[{index}][1]"); hp[index] = Int(row[2], mapId, $"mobs[{index}][2]"); x[index] = Short(row[3], mapId, $"mobs[{index}][3]"); y[index] = Short(row[4], mapId, $"mobs[{index}][4]");
            index++;
        }

        return (temp, level, hp, x, y);
    }

    // Parse mảng NPC song song như Java. (Java: npcs)
    public static (sbyte[] Id, short[] X, short[] Y) Npcs(string json, int mapId)
    {
        using JsonDocument document = ParseArray(json, mapId, "npcs");
        int count = document.RootElement.GetArrayLength();
        var id = new sbyte[count]; var x = new short[count]; var y = new short[count];
        int index = 0;
        foreach (JsonElement item in document.RootElement.EnumerateArray())
        {
            JsonElement row = ParseEmbeddedArray(item, mapId, $"npcs[{index}]");
            RequireLength(row, 3, mapId, $"npcs[{index}]");
            id[index] = SByte(row[0], mapId, $"npcs[{index}][0]"); x[index] = Short(row[1], mapId, $"npcs[{index}][1]"); y[index] = Short(row[2], mapId, $"npcs[{index}][2]");
            index++;
        }

        return (id, x, y);
    }

    // Đọc mảng JSON và đổi lỗi parser thành lỗi catalog gắn map. (Java: JSONValue.parse)
    private static JsonDocument ParseArray(string json, int mapId, string field)
    {
        try
        {
            JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                document.Dispose();
                throw Error(mapId, $"{field} phải là mảng JSON.");
            }

            return document;
        }
        catch (JsonException exception)
        {
            throw new CatalogLoadException(MapTemplateCatalogLoadStep.StepName, $"Map id {mapId} có {field} JSON không hợp lệ.", exception);
        }
    }

    // Lặp đúng ba replaceAll Java để sửa chuỗi waypoint không phải JSON chuẩn trong dump. (Java: replaceAll)
    private static string NormalizeWayPoints(string json, int mapId) => (json ?? throw Error(mapId, "waypoints không được null."))
        .Replace("[\"[", "[[", StringComparison.Ordinal)
        .Replace("]\"]", "]]", StringComparison.Ordinal)
        .Replace("\",\"", ",", StringComparison.Ordinal);

    // Nhận một mảng trực tiếp hoặc chuỗi chứa mảng JSON của dump. (Java: String.valueOf(dataArray.get(j)))
    private static JsonElement ParseEmbeddedArray(JsonElement item, int mapId, string field)
    {
        if (item.ValueKind == JsonValueKind.Array)
        {
            return item;
        }

        if (item.ValueKind == JsonValueKind.String)
        {
            try
            {
                using JsonDocument nested = JsonDocument.Parse(item.GetString() ?? String.Empty);
                if (nested.RootElement.ValueKind != JsonValueKind.Array)
                {
                    throw Error(mapId, $"{field} phải là mảng JSON.");
                }

                return nested.RootElement.Clone();
            }
            catch (JsonException exception)
            {
                throw new CatalogLoadException(MapTemplateCatalogLoadStep.StepName, $"Map id {mapId} có {field} JSON lồng không hợp lệ.", exception);
            }
        }

        throw Error(mapId, $"{field} phải là mảng hoặc chuỗi JSON.");
    }

    // Kiểm số phần tử tối thiểu như các lệnh get(index) của Java. (Java: JSONArray.get)
    private static void RequireLength(JsonElement row, int length, int mapId, string field)
    {
        if (row.GetArrayLength() < length)
        {
            throw Error(mapId, $"{field} cần ít nhất {length} phần tử.");
        }
    }

    // Đọc chuỗi theo String.valueOf Java. (Java: String.valueOf)
    private static string Text(JsonElement element, int mapId, string field) => element.ValueKind == JsonValueKind.String
        ? element.GetString() ?? String.Empty
        : element.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False ? element.GetRawText() : throw Error(mapId, $"{field} không phải chuỗi.");

    // Đọc số nguyên có thể được dump thành chuỗi. (Java: Integer.parseInt)
    private static int Int(JsonElement element, int mapId, string field)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out int number)) return number;
        if (element.ValueKind == JsonValueKind.String && Int32.TryParse(element.GetString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out number)) return number;
        throw Error(mapId, $"{field} không phải Int32.");
    }

    // Đọc short với kiểm khoảng, không ép tràn. (Java: Short.parseShort)
    private static short Short(JsonElement element, int mapId, string field) => StaticCatalogValidation.Short(Int(element, mapId, field), MapTemplateCatalogLoadStep.StepName, field, mapId);

    // Đọc byte có dấu với kiểm khoảng, không ép tràn. (Java: Byte.parseByte)
    private static sbyte SByte(JsonElement element, int mapId, string field) => StaticCatalogValidation.SByte(Int(element, mapId, field), MapTemplateCatalogLoadStep.StepName, field, mapId);

    // Tạo lỗi có ngữ cảnh map và field. (Java: loadDatabase)
    private static CatalogLoadException Error(int mapId, string message) => new(MapTemplateCatalogLoadStep.StepName, $"Map id {mapId}: {message}");
}
