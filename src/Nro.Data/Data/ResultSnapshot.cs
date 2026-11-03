using Nro.Data.Interfaces;

namespace Nro.Data;

/// <summary>
/// Implements <see cref="IResultSnapshot"/>: captures the whole query result in memory (replaces the Java <c>jbcd.ConnectResultSet</c>).
/// This is a line-by-line translation of <c>ConnectResultSet.java</c>; the semantics and the oddities are documented on
/// <see cref="IResultSnapshot"/> and compared against real Java by <c>tests/golden/result-snapshot.json</c>.
/// </summary>
/// <remarks>
/// <para>
/// Deliberate differences from the Java version: (1) a column that is an expression (no table name) has only the <c>column</c> key, while Java also registers a
/// <c>.column</c> key (leading dot) that no source code uses; (2) calling <see cref="Dispose"/> a second time does not throw
/// (Java throws <c>NullPointerException</c>), following the <see cref="IDisposable"/> convention.
/// </para>
/// <para>
/// Instead of every row holding its own <c>HashMap</c> as in Java, this type keeps ONE key-to-column-index table shared by all rows.
/// The two are equivalent because every row has the same key set and the same column wins on a duplicate key; it saves a lot of memory for large results.
/// </para>
/// <para>Not safe for concurrent use from several threads (same as the Java version).</para>
/// </remarks>
public sealed class ResultSnapshot : IResultSnapshot
{
    private readonly Dictionary<string, int> _keyToColumn;
    private object?[][]? _rows;
    private int _index = -1;

    /// <summary>
    /// Builds from existing data. Every row must have exactly as many values as there are columns. The data is copied, so the caller may reuse its arrays.
    /// Unlike the database read path, this does NOT convert value types (they are kept exactly as supplied).
    /// </summary>
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

    /// <summary>Builds and TAKES OWNERSHIP of the row arrays (no copy). Used by the database read path.</summary>
    internal static ResultSnapshot Adopt(IReadOnlyList<SnapshotColumn> columns, object?[][] rows)
        => new(BuildKeys(columns), rows);

    /// <summary>
    /// Key table. Java, for each column j in order: <c>put(columnName.toLowerCase(), v)</c> then
    /// <c>put(tableName.toLowerCase() + "." + columnName.toLowerCase(), v)</c>; on a duplicate key the LATER column wins.
    /// </summary>
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

    // ------------------------------------------------------------------ state and cell access

    /// <summary>Java: <c>if (data == null) throw new Exception("No data available")</c> (data = null after dispose).</summary>
    private object?[][] RequireRows() => _rows ?? throw new InvalidOperationException("No data available");

    /// <summary>Java: <c>if (indexData == -1) throw new Exception("Results need to be prepared in advance")</c>.</summary>
    private void RequirePositioned()
    {
        if (_index == -1)
        {
            throw new InvalidOperationException("Results need to be prepared in advance");
        }
    }

    /// <summary>Java: <c>values[indexData][column - 1]</c> (column indexes start at 1; out of range gives <see cref="IndexOutOfRangeException"/>).</summary>
    private object? Cell(int column)
    {
        var rows = RequireRows();
        RequirePositioned();
        return rows[_index][unchecked(column - 1)];
    }

    /// <summary>
    /// Java: <c>data[indexData].get(column.toLowerCase())</c>. The evaluation order is kept as in Java: the row (may throw
    /// <see cref="IndexOutOfRangeException"/>) first, then <c>column.toLowerCase()</c> (<c>null</c> throws
    /// <see cref="NullReferenceException"/>). A missing key (or a NULL value) gives <c>null</c>, like <c>HashMap.get</c>.
    /// </summary>
    private object? Cell(string column)
    {
        var rows = RequireRows();
        RequirePositioned();
        var row = rows[_index];
        var key = column.ToLowerInvariant();
        return _keyToColumn.TryGetValue(key, out var index) ? row[index] : null;
    }

    // ------------------------------------------------------------------ cursor movement

    /// <inheritdoc/>
    public bool Next()
    {
        var rows = RequireRows();
        unchecked
        {
            _index++;
        }

        return _index < rows.Length;
    }

    /// <inheritdoc/>
    public bool First()
    {
        var rows = RequireRows();
        unchecked
        {
            _index++;
        }

        return _index == rows.Length - 1;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public bool GotoFirst()
    {
        if (_rows is null || _rows.Length == 0)
        {
            throw new InvalidOperationException("No data available");
        }

        _index = 0;
        return true;
    }

    /// <inheritdoc/>
    public void GotoBeforeFirst() => _index = -1;

    /// <inheritdoc/>
    public bool GotoLast()
    {
        _index = RequireRows().Length - 1;
        return true;
    }

    /// <inheritdoc/>
    public int GetRows() => RequireRows().Length;

    // ------------------------------------------------------------------ reading values (hard casts as in Java)

    /// <inheritdoc/>
    public sbyte GetByte(int column) => unchecked((sbyte)(int)Cell(column)!);

    /// <inheritdoc/>
    public sbyte GetByte(string column) => unchecked((sbyte)(int)Cell(column)!);

    /// <inheritdoc/>
    public int GetInt(int column) => unchecked((int)(long)Cell(column)!);

    /// <inheritdoc/>
    public int GetInt(string column) => (int)Cell(column)!;

    /// <inheritdoc/>
    public short GetShort(int column) => unchecked((short)(int)Cell(column)!);

    /// <inheritdoc/>
    public short GetShort(string column) => unchecked((short)(int)Cell(column)!);

    /// <inheritdoc/>
    public float GetFloat(int column) => (float)Cell(column)!;

    /// <inheritdoc/>
    public float GetFloat(string column) => (float)Cell(column)!;

    /// <inheritdoc/>
    public double GetDouble(int column) => (double)Cell(column)!;

    /// <inheritdoc/>
    public double GetDouble(string column) => (double)Cell(column)!;

    /// <inheritdoc/>
    public long GetLong(int column) => (long)Cell(column)!;

    /// <inheritdoc/>
    public long GetLong(string column) => (long)Cell(column)!;

    /// <inheritdoc/>
    public string GetString(int column) => JavaStringFormat.ValueOf(Cell(column));

    /// <inheritdoc/>
    public string GetString(string column) => JavaStringFormat.ValueOf(Cell(column));

    /// <inheritdoc/>
    public object? GetObject(int column) => Cell(column);

    /// <inheritdoc/>
    public object? GetObject(string column) => Cell(column);

    /// <inheritdoc/>
    public bool GetBoolean(int column) => ToBoolean(Cell(column));

    /// <inheritdoc/>
    public bool GetBoolean(string column) => ToBoolean(Cell(column));

    /// <summary>
    /// Java: <c>try { return (int)v == 1; } catch (Exception e) { return (boolean)v; }</c>. Written directly as a type check
    /// that gives the same result without using exceptions as control flow: an <c>int</c> is compared with 1; anything else is cast to <c>bool</c>
    /// (NULL gives <see cref="NullReferenceException"/>, another type gives <see cref="InvalidCastException"/>, like the two casts in Java).
    /// </summary>
    private static bool ToBoolean(object? value) => value is int number ? number == 1 : (bool)value!;

    /// <inheritdoc/>
    public DateTime? GetTimestamp(int column) => (DateTime?)Cell(column);

    /// <inheritdoc/>
    public DateTime? GetTimestamp(string column) => (DateTime?)Cell(column);

    // ------------------------------------------------------------------ disposal

    /// <summary>
    /// Java <c>dispose()</c> sets <c>data = null; values = null</c>; every later call (except <see cref="GotoBeforeFirst"/>)
    /// throws "No data available". Calling it repeatedly is fine (unlike Java, see the note on the type).
    /// </summary>
    public void Dispose() => _rows = null;
}
