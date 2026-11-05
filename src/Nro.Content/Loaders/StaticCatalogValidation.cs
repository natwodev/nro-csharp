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
        int expected = 0;
        foreach (int id in ids)
        {
            if (id != expected)
            {
                throw new CatalogLoadException(stepName, $"Id phải liên tục từ 0 theo thứ tự nạp; kỳ vọng {expected} nhưng nhận {id}.");
            }

            expected++;
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
