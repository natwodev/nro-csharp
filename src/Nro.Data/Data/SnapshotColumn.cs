namespace Nro.Data;

/// <summary>
/// Metadata of one column of a <see cref="ResultSnapshot"/>: the column name and the source table name.
/// </summary>
/// <param name="Name">The column name as reported by the driver (the column label if the query uses an alias). The lookup key is the lower-case form.</param>
/// <param name="Table">
/// Source table name; <c>null</c> or empty if the column is an expression. With a table name, the column also gets a <c>table.column</c> key.
/// </param>
public readonly record struct SnapshotColumn(string Name, string? Table = null);
