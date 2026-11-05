// Nguồn Java: nro/server/ServerManager.java (dòng 520–582, 623–671).
using Nro.Network.Protocol;

namespace Nro.Host.Hosting;

// Chuyển GameServer mạng hiện có thành cổng vòng đời của Host. (Java: ServerManager)
public sealed class GameServerAdapter : IGameServer, IAsyncDisposable
{
    private readonly GameServer server;

    // Nhận listener TCP thật được cấu hình qua DI. (Java: ServerManager)
    public GameServerAdapter(GameServer server)
    {
        ArgumentNullException.ThrowIfNull(server);
        this.server = server;
    }

    // Mở listener TCP thật. (Java: ServerManager.run)
    public void Start() => server.Start();

    // Ngừng accept mà chưa ngắt phiên đang chạy. (Java: ServerManager.close)
    public void StopAccepting() => server.StopAccepting();

    // Đóng listener và các phiên còn lại. (Java: ServerManager.close)
    public Task StopAsync(CancellationToken cancellationToken = default) => server.StopAsync(cancellationToken);

    // Giải phóng token hủy của listener khi DI dừng. (Java: ServerManager.close)
    public ValueTask DisposeAsync() => server.DisposeAsync();
}
