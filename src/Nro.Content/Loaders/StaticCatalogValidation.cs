namespace Nro.Content.Loaders;

// Kiểm tra kiểu số và id trước khi dữ liệu trở thành snapshot. Nguồn Java: nro/server/Manager.java (dòng 351–436, 560–600, 727–741).
internal static class StaticCatalogValidation
{
    // Ép int sang short sau khi kiểm khoảng rõ nghĩa. (Java: getShort)
    public static short Short(int value, string stepName, string field, int rowId)
    {
        if (value is < short.MinValue or > short.MaxValue)
        {
            throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field}={value} vượt khoảng Int16.");
        }

        return (short)value;
    }

    // Ép int sang byte có dấu sau khi kiểm khoảng rõ nghĩa. (Java: getByte)
    public static sbyte SByte(int value, string stepName, string field, int rowId)
    {
        if (value is < sbyte.MinValue or > sbyte.MaxValue)
        {
            throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field}={value} vượt khoảng Int8 có dấu.");
        }

        return (sbyte)value;
    }

    // Ép PowerRequire long sang int theo Java sau khi kiểm không mất dữ liệu. (Java: getInt)
    public static int Int32(long value, string stepName, string field, int rowId)
    {
        if (value is < int.MinValue or > int.MaxValue)
        {
            throw new CatalogLoadException(stepName, $"Hàng id {rowId} có {field}={value} vượt khoảng Int32 mà Java đọc.");
        }

        return (int)value;
    }

    // Kiểm id duy nhất, liên tục từ 0 khi consumer tra theo vị trí danh sách. (Java: List.get)
    public static void ContinuousZeroBasedIds(IEnumerable<int> ids, string stepName)
    {
        ArgumentNullException.ThrowIfNull(ids);
        int[] ordered = ids.OrderBy(id => id).ToArray();
        if (ordered.Length != 0 && ordered[0] < 0)
        {
            throw new CatalogLoadException(stepName, $"Id phải không âm khi được dùng làm chỉ số Java; nhận {ordered[0]}.");
        }

        var missing = new List<int>();
        int maximum = ordered.Length == 0 ? -1 : ordered[^1];
        var actual = new HashSet<int>(ordered);
        for (int expected = 0; expected <= maximum; expected++)
        {
            if (!actual.Contains(expected))
            {
                missing.Add(expected);
            }
        }

        if (missing.Count != 0)
        {
            throw new CatalogGapException(stepName, missing);
        }
    }

    // Kiểm id liên tục với điểm bắt đầu do dữ liệu quyết định khi contract chỉ dùng vị trí ORDER BY. (Java: List.get)
    public static void ContinuousIds(IEnumerable<int> ids, string stepName)
    {
        using IEnumerator<int> enumerator = ids.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return;
        }

        int expected = enumerator.Current + 1;
        while (enumerator.MoveNext())
        {
            if (enumerator.Current != expected)
            {
                throw new CatalogLoadException(stepName, $"Id phải liên tục theo thứ tự nạp; kỳ vọng {expected} nhưng nhận {enumerator.Current}.");
            }

            expected++;
        }
    }

    // Thêm id vào tập và báo trùng với bước nguồn rõ ràng. (Java: ArrayList.add)
    public static void Unique(HashSet<int> ids, int id, string stepName)
    {
        if (!ids.Add(id))
        {
            throw new CatalogLoadException(stepName, $"Id bị trùng: {id}.");
        }
    }
}
