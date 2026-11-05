// Nguồn Java: nro/server/ServerManager.java (dòng 607–620).
using Microsoft.Extensions.Options;

namespace Nro.Host.Configurations;

// Kiểm chu kỳ scheduler trước khi HostedService được khởi động. (Java: ServerManager.autoSave)
public sealed class SchedulingOptionsValidator : IValidateOptions<SchedulingOptions>
{
    // Từ chối chu kỳ không dương để không tạo vòng lặp nền nóng. (Java: ServerManager.autoSave)
    public ValidateOptionsResult Validate(string? name, SchedulingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.AutoSaveInterval > TimeSpan.Zero
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("Scheduling:AutoSaveInterval phải lớn hơn 0.");
    }
}
