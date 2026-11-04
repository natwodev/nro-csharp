using Nro.Data.Db;
using System.Collections.Concurrent;
using Npgsql;
using NpgsqlTypes;

namespace Nro.Data.Db;

// Tra tên bảng nguồn của cột từ OID PostgreSQL để dựng khóa table.column cho ResultSnapshot.
// Nguồn Java: jbcd/ConnectDB.java (khác Java: tra pg_class trên chính kết nối sau khi đọc xong và đệm theo OID, không dùng CommandBehavior.KeyInfo vì có thể mở kết nối thứ hai làm cạn pool; đổi tên bảng chỉ cập nhật sau khởi động lại).
internal sealed class TableNameResolver
{
    private const string LookupSql = "select oid, relname from pg_catalog.pg_class where oid = any($1)";

    // Giới hạn bộ đệm khi bảng tạm liên tục được tạo và xóa.
    private const int MaxEntries = 4096;

    private readonly ConcurrentDictionary<uint, string> _names = new();

    // Trả tên bảng theo thứ tự cột; null cho biểu thức hoặc OID không còn trong catalog.
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
