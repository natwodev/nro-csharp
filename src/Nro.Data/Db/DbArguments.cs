using Nro.Data.Db;
using Npgsql;
using NpgsqlTypes;

namespace Nro.Data.Db;

// Chuyển đổi giá trị tham số và giá trị đọc từ Npgsql giữa kiểu dữ liệu trò chơi và trình điều khiển PostgreSQL.
// Nguồn Java: jbcd/ConnectDB.java (tham số chuỗi được gửi không gán kiểu để PostgreSQL tự suy kiểu như MySQL, giữ so sánh citext không phân biệt hoa thường).
internal static class DbArguments
{
    // Chuyển từng đối số thành tham số vị trí; null thành DBNull, chuỗi là unknown để suy kiểu theo ngữ cảnh, còn kiểu khác giữ nguyên.
    public static NpgsqlParameter[] ToParameters(object?[] args)
    {
        var parameters = new NpgsqlParameter[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            parameters[i] = args[i] switch
            {
                null => new NpgsqlParameter { Value = DBNull.Value },
                string text => new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Unknown, Value = text },

                // params object?[] nhận cả CancellationToken đặt sai vị trí nên báo lỗi sớm, rõ ràng.
                CancellationToken => throw new ArgumentException(
                    $"SQL parameter #{i + 1} is a CancellationToken: pass the token through the (sql, args, cancellationToken) overload.",
                    nameof(args)),
                var value => new NpgsqlParameter { Value = value },
            };
        }

        return parameters;
    }

    // Chuẩn hóa DBNull thành null và short thành int để giữ cách ép kiểu JDBC của Java; kiểu khác không đổi.
    public static object? FromDriverValue(object value) => value switch
    {
        DBNull => null,
        short number => (int)number,
        _ => value,
    };
}
