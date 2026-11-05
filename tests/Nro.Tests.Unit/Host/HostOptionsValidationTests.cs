// Nguồn Java: nro/server/ServerManager.java (dòng 520–620).
using Microsoft.Extensions.Options;
using Nro.Host.Configurations;

namespace Nro.Tests.Unit.Host;

// Kiểm cấu hình Host bị từ chối trước khi listener khởi động. (Java: ServerManager)
public sealed class HostOptionsValidationTests
{
    // Cổng ngoài khoảng TCP phải bị từ chối rõ nghĩa. (Java: ServerManager.PORT)
    [Fact]
    public void GameServerOptions_rejects_invalid_port()
    {
        ValidateOptionsResult result = new GameServerOptionsValidator().Validate(null, new GameServerOptions { Port = 65536 });

        Assert.True(result.Failed);
        Assert.Contains("GameServer:Port", result.FailureMessage ?? string.Empty);
    }

    // Chu kỳ tự lưu không dương phải bị từ chối để không tạo scheduler nóng. (Java: ServerManager.autoSave)
    [Fact]
    public void SchedulingOptions_rejects_non_positive_auto_save_interval()
    {
        ValidateOptionsResult result = new SchedulingOptionsValidator().Validate(null, new SchedulingOptions { AutoSaveInterval = TimeSpan.Zero });

        Assert.True(result.Failed);
        Assert.Contains("Scheduling:AutoSaveInterval", result.FailureMessage ?? string.Empty);
    }
}
