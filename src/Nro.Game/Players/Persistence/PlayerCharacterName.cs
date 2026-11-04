using System.Text.RegularExpressions;

namespace Nro.Game.Players.Persistence;

// Chuẩn hóa và kiểm tra bất biến tên nhân vật trước khi lưu.
// Nguồn Java: jbcd/dao/PlayerDAO.java (lớp phụ trợ cho luồng tạo nhân vật; tên lưu là chữ thường).


public static partial class PlayerCharacterName
{

    // Thông báo Java khi tên nhân vật không hợp lệ.

    public const string ValidationMessage = "Tên nhân vật chỉ đồng ý các ký tự a-z, 0-9 và chiều dài từ 5 đến 10 ký tự";

    // Hạ chữ theo createChar Java rồi kiểm tra trước khi dò trùng tên.

    public static string NormalizeAndValidate(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        string normalized = name.ToLowerInvariant();
        EnsureValidStoredName(normalized);
        return normalized;
    }

    // Từ chối giá trị phá vỡ bất biến tên lưu chữ thường.

    public static void EnsureValidStoredName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!StoredNamePattern().IsMatch(name))
        {
            throw new ArgumentException(ValidationMessage, nameof(name));
        }
    }

    [GeneratedRegex("^[a-z0-9]{5,10}$", RegexOptions.CultureInvariant)]
    private static partial Regex StoredNamePattern();
}
