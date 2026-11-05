// Nguồn Java: nro/server/Manager.java (dòng 1145–1194).
using Microsoft.Extensions.Options;

namespace Nro.Host.Configurations;

// Kiểm gốc asset trước khi handler J-06 dùng đường dẫn tệp. (Java: DataGame)
public sealed class AssetsOptionsValidator : IValidateOptions<AssetsOptions>
{
    // Chỉ chấp nhận đường dẫn tuyệt đối khi đã cấu hình; để trống khi asset handler chưa bật. (Java: DataGame)
    public ValidateOptionsResult Validate(string? name, AssetsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return string.IsNullOrWhiteSpace(options.Root) || Path.IsPathFullyQualified(options.Root)
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("Assets:Root phải là đường dẫn tuyệt đối hoặc để trống khi handler asset chưa bật.");
    }
}
