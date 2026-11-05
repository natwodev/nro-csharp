// Nguồn Java: nro/server/ServerManager.java (dòng 623–671).
namespace Nro.Host.Hosting;

// Cổng lưu người chơi trong shutdown, tách Host khỏi persistence người chơi chưa port. (Java: ServerManager.close)
public interface IPlayerSaveOnShutdown
{
    // Lưu snapshot người chơi online trước khi Host ngắt các phiên. (Java: PlayerService.savePlayer)
    Task SaveOnlinePlayersAsync(CancellationToken cancellationToken);
}
