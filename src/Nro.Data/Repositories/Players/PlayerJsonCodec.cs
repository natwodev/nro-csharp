using Nro.Data.Db;
using System.Text.Json;

namespace Nro.Data.Repositories.Players;

// Kiểm tra JSON mảng của người chơi mà không tuần tự hóa lại, giữ nguyên văn bản đã đọc để chu trình tải/lưu tương thích từng byte.
// Nguồn Java: jbcd/dao/PlayerDAO.java (các cột JSON văn bản được bảo toàn nguyên dạng; C# chỉ dùng bộ phân tích nghiêm ngặt để phát hiện dữ liệu không phải mảng).
public static class PlayerJsonCodec
{
    // Kiểm tra JSON không rỗng là mảng rồi trả lại đúng chuỗi gốc; JSON lỗi ném FormatException.
    public static string ReadArray(string? json, string column)
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new FormatException($"Player JSON column '{column}' is null or empty.");
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new FormatException($"Player JSON column '{column}' must be an array.");
            }
        }
        catch (JsonException exception)
        {
            throw new FormatException($"Player JSON column '{column}' is malformed.", exception);
        }

        return json;
    }
}
