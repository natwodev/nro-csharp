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
        NonNegativeIds(ordered, stepName, "được dùng làm chỉ số Java");
        IReadOnlyList<int> missing = MissingZeroBasedIds(ordered);

        if (missing.Count != 0)
        {
            throw new CatalogGapException(stepName, missing);
        }
    }

    // Liệt kê các id bị khuyết chỉ cho catalog mà Java dùng chính id làm chỉ số List.get.
    public static IReadOnlyList<int> MissingZeroBasedIds(IEnumerable<int> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);
        int[] ordered = ids.OrderBy(id => id).ToArray();
        int maximum = ordered.Length == 0 ? -1 : ordered[^1];
        var actual = new HashSet<int>(ordered);
        var missing = new List<int>();
        for (int expected = 0; expected <= maximum; expected++)
        {
            if (!actual.Contains(expected))
            {
                missing.Add(expected);
            }
        }

        return missing;
    }

    // Kiểm mapId không âm dù map được tra cứu theo id thưa. (Java: mapId)
    public static void NonNegativeIds(IEnumerable<int> ids, string stepName, string usage = "")
    {
        ArgumentNullException.ThrowIfNull(ids);
        int? negative = ids.Where(id => id < 0).Select(id => (int?)id).FirstOrDefault();
        if (negative is not null)
        {
            string context = String.IsNullOrEmpty(usage) ? String.Empty : $" khi {usage}";
            throw new CatalogLoadException(stepName, $"Id phải không âm{context}; nhận {negative.Value}.");
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
