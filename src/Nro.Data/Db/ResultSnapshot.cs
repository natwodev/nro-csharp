using Nro.Data.Db;

namespace Nro.Data.Db;

// Lưu toàn bộ kết quả truy vấn trong bộ nhớ và cung cấp thao tác con trỏ, đọc giá trị tương thích Java.
// Nguồn Java: jbcd/ConnectResultSet.java (bảng khóa dùng chung cho mọi hàng; cột biểu thức không có khóa .column và Dispose lặp lại không lỗi).
public sealed class ResultSnapshot : IResultSnapshot
{
    private readonly Dictionary<string, int> _keyToColumn;
    private object?[][]? _rows;
    private int _index = -1;

    // Tạo ảnh chụp từ dữ liệu có sẵn, sao chép từng hàng và yêu cầu số giá trị đúng bằng số cột.
    public ResultSnapshot(IReadOnlyList<SnapshotColumn> columns, IEnumerable<object?[]> rows)
    {
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(rows);

        _keyToColumn = BuildKeys(columns);
        var copy = new List<object?[]>();
        foreach (var row in rows)
        {
            if (row.Length != columns.Count)
            {
                throw new ArgumentException(
                    $"A row has {row.Length} values but the result has {columns.Count} columns.", nameof(rows));
            }

            copy.Add((object?[])row.Clone());
        }

        _rows = copy.ToArray();
    }

    private ResultSnapshot(Dictionary<string, int> keyToColumn, object?[][] rows)
    {
        _keyToColumn = keyToColumn;
        _rows = rows;
    }

    // Tạo ảnh chụp và nhận quyền sở hữu mảng hàng, không sao chép; dùng khi đọc cơ sở dữ liệu.
    internal static ResultSnapshot Adopt(IReadOnlyList<SnapshotColumn> columns, object?[][] rows)
        => new(BuildKeys(columns), rows);

    // Lập khóa chữ thường cột và bảng.cột theo thứ tự; cột sau ghi đè khi trùng khóa như Java.
    private static Dictionary<string, int> BuildKeys(IReadOnlyList<SnapshotColumn> columns)
    {
        var keys = new Dictionary<string, int>(columns.Count * 2, StringComparer.Ordinal);
        for (var j = 0; j < columns.Count; j++)
        {
            var column = columns[j].Name.ToLowerInvariant();
            keys[column] = j;

            var table = columns[j].Table;
            if (!string.IsNullOrEmpty(table))
            {
                keys[table.ToLowerInvariant() + "." + column] = j;
            }
        }

        return keys;
    }

    // ------------------------------------------------------------------ Trạng thái và truy cập ô

    // Yêu cầu dữ liệu chưa bị hủy (Java: ném lỗi khi data là null).
    private object?[][] RequireRows() => _rows ?? throw new InvalidOperationException("No data available");

    // Yêu cầu con trỏ đã được đặt vào kết quả (Java: indexData khác -1).
    private void RequirePositioned()
    {
        if (_index == -1)
        {
            throw new InvalidOperationException("Results need to be prepared in advance");
        }
    }

    // Lấy ô theo chỉ số cột bắt đầu từ 1; vượt phạm vi ném IndexOutOfRangeException như Java.
    private object? Cell(int column)
    {
        var rows = RequireRows();
        RequirePositioned();
        return rows[_index][unchecked(column - 1)];
    }

    // Lấy ô theo khóa chữ thường, giữ thứ tự đánh giá và null khi thiếu khóa hoặc giá trị SQL NULL như HashMap Java.
    private object? Cell(string column)
    {
        var rows = RequireRows();
        RequirePositioned();
        var row = rows[_index];
        var key = column.ToLowerInvariant();
        return _keyToColumn.TryGetValue(key, out var index) ? row[index] : null;
    }

    // ------------------------------------------------------------------ Di chuyển con trỏ

    // Tăng con trỏ và báo còn nằm trong phạm vi hàng hay không (Java: next).
    public bool Next()
    {
        var rows = RequireRows();
        unchecked
        {
            _index++;
        }

        return _index < rows.Length;
    }

    // Tăng con trỏ và chỉ báo đúng khi vừa tới hàng cuối (Java: first).
    public bool First()
    {
        var rows = RequireRows();
        unchecked
        {
            _index++;
        }

        return _index == rows.Length - 1;
    }

    // Đặt con trỏ về chỉ số yêu cầu sau khi kiểm tra vị trí hiện tại, không kiểm tra đích (Java: gotoResult).
    public bool GotoResult(int index)
    {
        var rows = RequireRows();
        if (_index < 0 || _index >= rows.Length)
        {
            throw new InvalidOperationException("Index out of bound");
        }

        _index = index;
        return true;
    }

    // Đặt con trỏ vào hàng đầu, hoặc ném lỗi nếu không có dữ liệu (Java: gotoFirst).
    public bool GotoFirst()
    {
        if (_rows is null || _rows.Length == 0)
        {
            throw new InvalidOperationException("No data available");
        }

        _index = 0;
        return true;
    }

    // Đặt con trỏ trước hàng đầu (Java: gotoBeforeFirst).
    public void GotoBeforeFirst() => _index = -1;

    // Đặt con trỏ vào hàng cuối (Java: gotoLast).
    public bool GotoLast()
    {
        _index = RequireRows().Length - 1;
        return true;
    }

    // Trả số hàng trong ảnh chụp (Java: getRows).
    public int GetRows() => RequireRows().Length;

    // ------------------------------------------------------------------ Đọc giá trị bằng ép kiểu cứng như Java

    // Đọc byte có dấu theo chỉ số cột (Java: getByte).
    public sbyte GetByte(int column) => unchecked((sbyte)(int)Cell(column)!);

    // Đọc byte có dấu theo tên cột (Java: getByte).
    public sbyte GetByte(string column) => unchecked((sbyte)(int)Cell(column)!);

    // Đọc số nguyên theo chỉ số cột (Java: getInt).
    public int GetInt(int column) => unchecked((int)(long)Cell(column)!);

    // Đọc số nguyên theo tên cột (Java: getInt).
    public int GetInt(string column) => (int)Cell(column)!;

    // Đọc số nguyên ngắn theo chỉ số cột (Java: getShort).
    public short GetShort(int column) => unchecked((short)(int)Cell(column)!);

    // Đọc số nguyên ngắn theo tên cột (Java: getShort).
    public short GetShort(string column) => unchecked((short)(int)Cell(column)!);

    // Đọc số thực đơn chính xác theo chỉ số cột (Java: getFloat).
    public float GetFloat(int column) => (float)Cell(column)!;

    // Đọc số thực đơn chính xác theo tên cột (Java: getFloat).
    public float GetFloat(string column) => (float)Cell(column)!;

    // Đọc số thực kép chính xác theo chỉ số cột (Java: getDouble).
    public double GetDouble(int column) => (double)Cell(column)!;

    // Đọc số thực kép chính xác theo tên cột (Java: getDouble).
    public double GetDouble(string column) => (double)Cell(column)!;

    // Đọc số nguyên dài theo chỉ số cột (Java: getLong).
    public long GetLong(int column) => (long)Cell(column)!;

    // Đọc số nguyên dài theo tên cột (Java: getLong).
    public long GetLong(string column) => (long)Cell(column)!;

    // Đọc chuỗi theo chỉ số cột, tái tạo Java String.valueOf (Java: getString).
    public string GetString(int column) => JavaStringFormat.ValueOf(Cell(column));

    // Đọc chuỗi theo tên cột, tái tạo Java String.valueOf (Java: getString).
    public string GetString(string column) => JavaStringFormat.ValueOf(Cell(column));

    // Đọc đối tượng theo chỉ số cột (Java: getObject).
    public object? GetObject(int column) => Cell(column);

    // Đọc đối tượng theo tên cột (Java: getObject).
    public object? GetObject(string column) => Cell(column);

    // Đọc luận lý theo chỉ số cột; int bằng 1 là true (Java: getBoolean).
    public bool GetBoolean(int column) => ToBoolean(Cell(column));

    // Đọc luận lý theo tên cột; int bằng 1 là true (Java: getBoolean).
    public bool GetBoolean(string column) => ToBoolean(Cell(column));

    // So sánh int với 1, còn lại ép bool để giữ kết quả và loại ngoại lệ Java.
    private static bool ToBoolean(object? value) => value is int number ? number == 1 : (bool)value!;

    // Đọc thời điểm theo chỉ số cột (Java: getTimestamp).
    public DateTime? GetTimestamp(int column) => (DateTime?)Cell(column);

    // Đọc thời điểm theo tên cột (Java: getTimestamp).
    public DateTime? GetTimestamp(string column) => (DateTime?)Cell(column);

    // ------------------------------------------------------------------ Hủy tài nguyên

    // Giải phóng dữ liệu; gọi lặp lại an toàn, còn các lời gọi sau (trừ GotoBeforeFirst) ném lỗi như Java.
    public void Dispose() => _rows = null;
}
