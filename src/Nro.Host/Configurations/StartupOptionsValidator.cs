// Nguồn Java: nro/server/ServerManager.java (dòng 623–671).
using Microsoft.Extensions.Options;

namespace Nro.Host.Configurations;

// Kiểm deadline shutdown trước khi HostedService được khởi động. (Java: ServerManager.close)
public sealed class StartupOptionsValidator : IValidateOptions<StartupOptions>
{
    // Từ chối deadline lưu không dương để shutdown không bỏ qua cổng lưu. (Java: ServerManager.close)
    public ValidateOptionsResult Validate(string? name, StartupOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.ShutdownSaveTimeout > TimeSpan.Zero
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("Startup:ShutdownSaveTimeout phải lớn hơn 0.");
    }
}
