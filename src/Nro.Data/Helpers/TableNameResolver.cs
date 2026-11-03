using System.Collections.Concurrent;
using Npgsql;
using NpgsqlTypes;

namespace Nro.Data;

/// <summary>
/// Looks up the source table name of a column (to build the <c>table.column</c> key of <see cref="ResultSnapshot"/>) from the table OID PostgreSQL sends
/// with the column description (<c>NpgsqlDbColumn.TableOID</c>, 0 if the column is an expression).
/// </summary>
/// <remarks>
/// <para>
/// The result is the same as <c>NpgsqlDbColumn.BaseTableName</c> (both are <c>pg_class.relname</c>) but WITHOUT using
/// <c>CommandBehavior.KeyInfo</c>. Measured on Npgsql 10.0.3: <c>BaseTableName</c> is only available when reading with <c>KeyInfo</c>, and then
/// <c>GetColumnSchema()</c> opens a SECOND connection from the pool on every call (with <c>Maximum Pool Size=1</c> it reports "connection pool
/// has been exhausted"). In the game server, many concurrent queries that each hold a connection and then ask for a second one
/// could exhaust the pool and hang the whole system. Here the OID only needs the column description (no extra connection), and the table name is looked up with
/// a small query on the SAME connection already in use, after the result has been fully read, and cached by OID (each table is looked up once).
/// </para>
/// <para>
/// A table OID is stable for the table's whole life; renaming a table while the server is running leaves the old name in the cache until a
/// restart (this does not happen in normal operation).
/// </para>
/// </remarks>
internal sealed class TableNameResolver
{
    private const string LookupSql = "select oid, relname from pg_catalog.pg_class where oid = any($1)";

    /// <summary>Stops the cache from growing forever when temporary tables are created/dropped continuously.</summary>
    private const int MaxEntries = 4096;

    private readonly ConcurrentDictionary<uint, string> _names = new();

    /// <summary>Returns the table name of each column in order (<c>null</c> for an expression column or an OID no longer in the catalog).</summary>
    public async Task<string?[]> ResolveAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction? transaction,
        uint[] tableOids,
        CancellationToken cancellationToken)
    {
        var missing = tableOids.Where(oid => oid != 0 && !_names.ContainsKey(oid)).Distinct().ToArray();
        if (missing.Length > 0)
        {
            if (_names.Count >= MaxEntries)
            {
                _names.Clear();
            }

            await using var command = new NpgsqlCommand(LookupSql, connection, transaction);
            command.Parameters.Add(new NpgsqlParameter
            {
                NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Oid,
                Value = missing,
            });

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                _names[reader.GetFieldValue<uint>(0)] = reader.GetString(1);
            }
        }

        var result = new string?[tableOids.Length];
        for (var i = 0; i < tableOids.Length; i++)
        {
            result[i] = tableOids[i] != 0 && _names.TryGetValue(tableOids[i], out var name) ? name : null;
        }

        return result;
    }
}
