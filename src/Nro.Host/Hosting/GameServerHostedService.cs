// Nguồn Java: nro/server/ServerManager.java (dòng 132–218, 520–713).
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nro.Content.Loaders;
using Nro.Game.Content;
using Nro.Host.Configurations;

namespace Nro.Host.Hosting;

// Điều phối nạp catalog, mở listener và shutdown theo thứ tự an toàn. (Java: ServerManager)
public sealed class GameServerHostedService : IHostedService
{
    private readonly IStaticCatalogLoader catalogs;
    private readonly IGameServer server;
    private readonly IPlayerSaveOnShutdown playerSave;
    private readonly StartupOptions startup;
    private readonly ILogger<GameServerHostedService> logger;

    // Nhận các cổng vòng đời từ DI. (Java: ServerManager)
    public GameServerHostedService(
        IStaticCatalogLoader catalogs,
        IGameServer server,
        IPlayerSaveOnShutdown playerSave,
        IOptions<StartupOptions> startup,
        ILogger<GameServerHostedService> logger)
    {
        ArgumentNullException.ThrowIfNull(catalogs);
        ArgumentNullException.ThrowIfNull(server);
        ArgumentNullException.ThrowIfNull(playerSave);
        ArgumentNullException.ThrowIfNull(startup);
        ArgumentNullException.ThrowIfNull(logger);
        this.catalogs = catalogs;
        this.server = server;
        this.playerSave = playerSave;
        this.startup = startup.Value;
        this.logger = logger;
    }

    // Nạp catalog thành công rồi mới mở listener; lỗi nạp làm Host fail-fast. (Java: Manager.loadDatabase)
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (startup.LoadStaticCatalogs)
        {
            try
            {
                await catalogs.LoadAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (CatalogLoadAggregateException exception)
            {
                foreach (CatalogLoadException error in exception.Errors)
                {
                    // Không truyền exception gốc cho logger: provider/driver có thể chứa chuỗi kết nối hoặc mật khẩu.
                    logger.LogError("Static catalog lỗi ở bước {CatalogStep}: {CatalogMessage}", error.StepName, error.Message);
                }

                foreach (CatalogLoadSkippedStep skipped in exception.SkippedSteps)
                {
                    logger.LogError("Static catalog: {CatalogSkip}", skipped.Message);
                }

                // Aggregate vẫn giữ ngoại lệ gốc cho caller trực tiếp, nhưng Host không bọc nó để logger nền không in thông tin kết nối nhạy cảm.
                throw new InvalidOperationException("Khởi động thất bại: không thể nạp static catalog trước khi mở cổng.");
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError("Không thể nạp static catalog; GameServer sẽ không mở cổng.");
                throw new InvalidOperationException("Khởi động thất bại: không thể nạp static catalog trước khi mở cổng.");
            }
        }
        else
        {
            logger.LogWarning("Startup:LoadStaticCatalogs=false chỉ dành cho chạy thử không DB; không dùng cấu hình này cho máy chủ công khai.");
        }

        server.Start();
        logger.LogInformation("GameServer đã mở listener sau khi hoàn tất bootstrap.");
    }

    // Ngừng accept, lưu trong deadline rồi mới đóng phiên và listener. (Java: ServerManager.close)
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        server.StopAccepting();
        using var saveDeadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        saveDeadline.CancelAfter(startup.ShutdownSaveTimeout);
        try
        {
            await playerSave.SaveOnlinePlayersAsync(saveDeadline.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (saveDeadline.IsCancellationRequested)
        {
            logger.LogWarning("Hết thời hạn {Timeout} khi lưu người chơi lúc shutdown.", startup.ShutdownSaveTimeout);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Lưu người chơi khi shutdown thất bại; vẫn tiếp tục đóng listener.");
        }
        finally
        {
            await server.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
