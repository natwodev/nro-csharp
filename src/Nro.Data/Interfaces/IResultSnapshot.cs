namespace Nro.Data.Interfaces;

/// <summary>
/// An in-memory snapshot of a query result (replaces the Java <c>jbcd.CrisResultSet</c>).
/// </summary>
/// <remarks>
/// <para>
/// Preserves EXACTLY the semantics of the Java <c>ConnectResultSet</c>, including its oddities (each one has a comparison case against real Java
/// in <c>tests/golden/result-snapshot.json</c>):
/// </para>
/// <list type="bullet">
/// <item><b>Column indexes start at 1.</b> Name keys are case-insensitive; each column has a <c>column</c> key, and a
/// <c>table.column</c> key if it has a table name. When two columns share a name (join), the <c>column</c> key points to the LAST one.</item>
/// <item>The row cursor starts before the first row (-1). <see cref="Next"/> always advances the index (it is not clamped at the end).</item>
/// <item><see cref="First"/> does NOT jump to the first row: it advances the index by 1 and returns <c>true</c> when the index equals the LAST row.
/// So it returns <c>true</c> on the very first call only when the result has exactly one row (the way the Java code uses it to read a single row).</item>
/// <item><see cref="GotoResult"/> checks the CURRENT index, not the target <c>index</c>.</item>
/// <item>Value getters use hard casts with no conversion: <c>GetInt(string)</c> needs a value of exactly type <c>int</c>, but
/// <c>GetInt(int)</c> needs a <c>long</c> and truncates it to <c>int</c>; <c>GetByte</c>/<c>GetShort</c> need an <c>int</c> and truncate it;
/// <c>GetFloat</c> needs a <c>float</c>, <c>GetDouble</c> a <c>double</c>, <c>GetLong</c> a <c>long</c>.
/// A wrong type throws <see cref="InvalidCastException"/> (corresponding to <c>ClassCastException</c>).</item>
/// <item>NULL values: the getters that return numeric types throw <see cref="NullReferenceException"/> (corresponding to <c>NullPointerException</c>);
/// <c>GetObject</c> and <c>GetTimestamp</c> return <c>null</c>; <c>GetString</c> returns the string <c>"null"</c>.</item>
/// <item>A column that does not exist: by NAME it yields the same as a NULL value; by INDEX it throws <see cref="IndexOutOfRangeException"/>
/// (corresponding to <c>ArrayIndexOutOfBoundsException</c>), which is also the error when the row index is outside the result.</item>
/// <item>State errors (<see cref="InvalidOperationException"/>, messages identical to Java): "No data available" (after
/// <see cref="IDisposable.Dispose"/>), "Results need to be prepared in advance" (reading before the cursor is positioned),
/// "Index out of bound" (<see cref="GotoResult"/>).</item>
/// </list>
/// <para>
/// The return type for Java <c>byte</c> is <see cref="sbyte"/>; <c>java.sql.Timestamp</c> is <see cref="DateTime"/>.
/// </para>
/// </remarks>
public interface IResultSnapshot : IDisposable
{
    /// <summary>Java: <c>(byte)(int)value</c>. The value must be an <c>int</c>; it is truncated (wrapped) to <see cref="sbyte"/>.</summary>
    sbyte GetByte(int column);

    /// <inheritdoc cref="GetByte(int)"/>
    sbyte GetByte(string column);

    /// <summary>Java: <c>(int)(long)value</c>. The value must be a <c>long</c> (e.g. <c>COUNT(*)</c>); it is truncated to <see cref="int"/>.</summary>
    int GetInt(int column);

    /// <summary>Java: <c>(int)value</c>. The value must be exactly an <c>int</c> (not a <c>long</c>).</summary>
    int GetInt(string column);

    /// <summary>Java: <c>(short)(int)value</c>. The value must be an <c>int</c>; it is truncated to <see cref="short"/>.</summary>
    short GetShort(int column);

    /// <inheritdoc cref="GetShort(int)"/>
    short GetShort(string column);

    /// <summary>Java: <c>(float)value</c>. The value must be a <c>float</c> (not a <c>double</c>).</summary>
    float GetFloat(int column);

    /// <inheritdoc cref="GetFloat(int)"/>
    float GetFloat(string column);

    /// <summary>Java: <c>(double)value</c>. The value must be a <c>double</c> (not a <c>float</c> or <c>decimal</c>).</summary>
    double GetDouble(int column);

    /// <inheritdoc cref="GetDouble(int)"/>
    double GetDouble(string column);

    /// <summary>Java: <c>(long)value</c>. The value must be a <c>long</c> (not an <c>int</c>).</summary>
    long GetLong(int column);

    /// <inheritdoc cref="GetLong(int)"/>
    long GetLong(string column);

    /// <summary>
    /// Java: <c>String.valueOf(value)</c>. NULL gives the string <c>"null"</c>. Numbers, booleans (<c>"true"</c>/<c>"false"</c>) and
    /// times are formatted like Java <c>toString()</c>.
    /// </summary>
    string GetString(int column);

    /// <inheritdoc cref="GetString(int)"/>
    string GetString(string column);

    /// <summary>
    /// Java: if the value is an <c>int</c> then <c>== 1</c>; otherwise cast to <c>bool</c> (the value must be a <c>bool</c>).
    /// This makes it work for both 0/1 columns and boolean columns.
    /// </summary>
    bool GetBoolean(int column);

    /// <inheritdoc cref="GetBoolean(int)"/>
    bool GetBoolean(string column);

    /// <summary>The raw captured value (<c>null</c> if NULL).</summary>
    object? GetObject(int column);

    /// <inheritdoc cref="GetObject(int)"/>
    object? GetObject(string column);

    /// <summary>Java: <c>(Timestamp)value</c>. NULL gives <c>null</c>; a value that is not a <see cref="DateTime"/> throws <see cref="InvalidCastException"/>.</summary>
    DateTime? GetTimestamp(int column);

    /// <inheritdoc cref="GetTimestamp(int)"/>
    DateTime? GetTimestamp(string column);

    /// <summary>Advances the row index and returns <c>true</c> if it is still inside the result. Not clamped at the end.</summary>
    bool Next();

    /// <summary>Advances the row index and returns <c>true</c> only when the index equals the last row (see the note on the type).</summary>
    bool First();

    /// <summary>Sets the row index to <paramref name="index"/>; it only checks the current index. Always returns <c>true</c>.</summary>
    bool GotoResult(int index);

    /// <summary>Goes to the first row; an empty result throws "No data available". Always returns <c>true</c>.</summary>
    bool GotoFirst();

    /// <summary>Goes before the first row (index -1). Checks nothing, even after Dispose.</summary>
    void GotoBeforeFirst();

    /// <summary>Goes to the last row (an empty result gives index -1). Always returns <c>true</c>.</summary>
    bool GotoLast();

    /// <summary>Number of rows.</summary>
    int GetRows();
}
