// Nguồn Java: nro/server/ServerManager.java (dòng 623–671).
using Microsoft.Extensions.Logging;

namespace Nro.Host.Hosting;

// Cài đặt tạm có log rõ ràng cho đến khi registry người chơi được nối. (Java: ServerManager.close)
public sealed class LoggingPlayerSaveOnShutdown : IPlayerSaveOnShutdown
{
    private readonly ILogger<LoggingPlayerSaveOnShutdown> logger;

    // Nhận logger vận hành của Host. (Java: Logger.log)
    public LoggingPlayerSaveOnShutdown(ILogger<LoggingPlayerSaveOnShutdown> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        this.logger = logger;
    }

    // Ghi rõ việc chưa có registry để shutdown không giả vờ đã lưu người chơi. (Java: ServerManager.close)
    public Task SaveOnlinePlayersAsync(CancellationToken cancellationToken)
    {
        logger.LogWarning("Chưa có registry người chơi; shutdown chưa có người chơi nào để lưu.");
        return Task.CompletedTask;
    }
}
