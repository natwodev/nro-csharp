using Nro.Core.Logging;

namespace Nro.Core.IO;

// Đọc và ghi nguyên cả file; lỗi vào/ra (thiếu file, không có quyền, đường dẫn sai) không ném ra mà hàm đọc trả null, hàm ghi trả false
// Nguồn Java: Utils/FileIO.java (khác duy nhất: WriteFile trả bool báo thành công thay vì void; tham số null vẫn ném lỗi như NullPointerException của Java)
public static class FileIO
{
    // Đọc toàn bộ byte của file, trả null nếu không đọc được (Java: readFile)
    public static byte[]? ReadFile(string url)
    {
        ArgumentNullException.ThrowIfNull(url);
        try
        {
            return File.ReadAllBytes(url);
        }
        catch (Exception ex) when (IsFileFailure(ex))
        {
            // Java im lặng bỏ qua lỗi này; ở đây vẫn trả null nhưng có ghi cảnh báo để không bị mất dấu (audit A-06 F3)
            Logger.Warning($"FileIO: cannot read '{url}' ({ex.GetType().Name})");
            return null;
        }
    }

    // Đọc file vào một MemoryStream, trả null nếu không đọc được; người gọi tự giải phóng stream (Java: loadFile, trả ByteArrayOutputStream)
    public static MemoryStream? LoadFile(string url)
    {
        byte[]? data = ReadFile(url);
        if (data is null)
        {
            return null;
        }

        var stream = new MemoryStream(data.Length);
        stream.Write(data, 0, data.Length);
        return stream;
    }

    // Ghi đè file bằng mảng byte, thư mục cha phải có sẵn; trả false nếu không ghi được.
    // Khác Java có chủ đích: mảng null bị từ chối trước khi đụng tới file (Java tạo file rỗng rồi mới ném lỗi) (Java: writeFile)
    public static bool WriteFile(string url, byte[] ab)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(ab);
        try
        {
            File.WriteAllBytes(url, ab);
            return true;
        }
        catch (Exception ex) when (IsFileFailure(ex))
        {
            Logger.Warning($"FileIO: cannot write '{url}' ({ex.GetType().Name})");
            return false;
        }
    }

    // Các lỗi mà FileInputStream/FileOutputStream của Java báo là IOException: lỗi vào/ra, bị từ chối quyền, đường dẫn rỗng hoặc có ký tự không hợp lệ
    private static bool IsFileFailure(Exception ex) => ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException;
}
