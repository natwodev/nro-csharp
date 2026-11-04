using System.Collections.ObjectModel;

namespace Nro.Data.Models.Players;

// Biểu diễn đầy đủ, không mất dữ liệu của một dòng player trong cơ sở dữ liệu.
// Nguồn Java: jbcd/dao/PlayerDAO.java (các cột JSON giữ dạng văn bản để PostgreSQL jsonb hoặc System.Text.Json không đổi khoảng trắng và cách ghi số).
public sealed class PlayerPersistenceRecord
{
    private readonly IReadOnlyDictionary<string, object?> _values;
    private readonly IReadOnlySet<string> _changedColumns;

    // Tạo bản ghi từ đủ các cột vật lý của player; mọi cột trừ id được đánh dấu đã đổi.
    public PlayerPersistenceRecord(IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var copy = new Dictionary<string, object?>(values, StringComparer.Ordinal);
        foreach (string column in PlayerPersistenceColumns.All)
        {
            if (!copy.ContainsKey(column))
            {
                throw new ArgumentException($"The player row is missing column '{column}'.", nameof(values));
            }
        }

        _values = new ReadOnlyDictionary<string, object?>(copy);
        _changedColumns = new HashSet<string>(PlayerPersistenceColumns.All.Where(static column => column != "id"), StringComparer.Ordinal);
    }

    private PlayerPersistenceRecord(IReadOnlyDictionary<string, object?> values, IReadOnlySet<string> changedColumns)
    {
        _values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(values, StringComparer.Ordinal));
        _changedColumns = new HashSet<string>(changedColumns, StringComparer.Ordinal);
    }

    // Lấy tập giá trị cột theo tên.
    public IReadOnlyDictionary<string, object?> Values => _values;

    // Lấy các cột vật lý đã thay đổi kể từ lúc đọc từ PostgreSQL.
    public IReadOnlySet<string> ChangedColumns => _changedColumns;

    // Lấy id người chơi, yêu cầu giá trị Int32.
    public int Id => _values["id"] is int value ? value : throw new InvalidOperationException("Player id is not an Int32.");

    // Lấy tên người chơi.
    public string Name => (string)_values["name"]!;

    // Lấy giá trị cột theo tên, không có cột trả null.
    public object? this[string column] => _values.TryGetValue(column, out object? value) ? value : null;

    // Trả bản ghi mới với một cột được thay đổi; không cho phép cột ngoài danh sách player.
    public PlayerPersistenceRecord With(string column, object? value)
    {
        if (!PlayerPersistenceColumns.All.Contains(column, StringComparer.Ordinal))
        {
            throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown player column.");
        }

        var copy = new Dictionary<string, object?>(_values, StringComparer.Ordinal) { [column] = value };
        var changed = new HashSet<string>(_changedColumns, StringComparer.Ordinal);
        if (column != "id")
        {
            changed.Add(column);
        }

        return new PlayerPersistenceRecord(copy, changed);
    }

    // Tạo bản ghi vừa đọc từ cơ sở dữ liệu, chưa có cột nào thay đổi.
    internal static PlayerPersistenceRecord FromDatabase(IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return new PlayerPersistenceRecord(values, new HashSet<string>(StringComparer.Ordinal));
    }
}
