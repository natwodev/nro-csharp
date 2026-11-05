// Nguồn Java: nro/server/ServerManager.java (dòng 132–218, 623–671).
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Nro.Game.Content;
using Nro.Host.Configurations;
using Nro.Host.Hosting;

namespace Nro.Tests.Unit.Host;

// Kiểm thứ tự bootstrap và shutdown Host mà không mở TCP thật. (Java: ServerManager)
public sealed class GameServerHostedServiceTests
{
    // Lỗi catalog không được phép gọi Start listener. (Java: Manager.loadDatabase)
    [Fact]
    public async Task StartAsync_catalog_failure_does_not_start_listener()
    {
        var events = new List<string>();
        var server = new RecordingServer(events);
        var service = Create(new RecordingCatalogLoader(events, new InvalidOperationException("catalog invalid")), server, new RecordingSave(events));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartAsync(CancellationToken.None));

        Assert.Equal(["load"], events);
    }

    // Listener chỉ mở sau khi catalog nạp thành công. (Java: ServerManager.run)
    [Fact]
    public async Task StartAsync_loads_catalog_before_starting_listener()
    {
        var events = new List<string>();
        var service = Create(new RecordingCatalogLoader(events), new RecordingServer(events), new RecordingSave(events));

        await service.StartAsync(CancellationToken.None);

        Assert.Equal(["load", "start"], events);
    }

    // Tắt LoadStaticCatalogs chỉ dành cho thử nghiệm và không gọi loader. (Java: Manager.loadDatabase)
    [Fact]
    public async Task StartAsync_with_catalog_loading_disabled_skips_loader()
    {
        var events = new List<string>();
        var service = Create(
            new RecordingCatalogLoader(events),
            new RecordingServer(events),
            new RecordingSave(events),
            new StartupOptions { LoadStaticCatalogs = false });

        await service.StartAsync(CancellationToken.None);

        Assert.Equal(["start"], events);
    }

    // Shutdown dừng accept trước, rồi lưu, sau cùng mới đóng listener. (Java: ServerManager.close)
    [Fact]
    public async Task StopAsync_stops_accepting_saves_then_stops_listener()
    {
        var events = new List<string>();
        var service = Create(new RecordingCatalogLoader(events), new RecordingServer(events), new RecordingSave(events));

        await service.StopAsync(CancellationToken.None);

        Assert.Equal(["stop-accepting", "save", "stop"], events);
    }

    // Dựng service với logger null để test chỉ quan sát cổng vòng đời. (Java: ServerManager)
    private static GameServerHostedService Create(
        IStaticCatalogLoader catalogs,
        IGameServer server,
        IPlayerSaveOnShutdown save,
        StartupOptions? startup = null) => new(
            catalogs,
            server,
            save,
            Options.Create(startup ?? new StartupOptions()),
            NullLogger<GameServerHostedService>.Instance);

    // Fake catalog có thể ném lỗi theo kịch bản test. (Java: Manager.loadDatabase)
    private sealed class RecordingCatalogLoader : IStaticCatalogLoader
    {
        private readonly List<string> events;
        private readonly Exception? failure;

        // Tạo fake với danh sách sự kiện dùng chung. (Java: Manager.loadDatabase)
        public RecordingCatalogLoader(List<string> events, Exception? failure = null)
        {
            this.events = events;
            this.failure = failure;
        }

        // Trả snapshot rỗng hợp lệ khi test không cần dữ liệu catalog. (Java: Manager)
        public IStaticCatalogs Current { get; } = new StaticCatalogBuilder().Build();

        // Ghi sự kiện nạp rồi ném lỗi mô phỏng nếu được cấu hình. (Java: Manager.loadDatabase)
        public Task<IStaticCatalogs> LoadAsync(CancellationToken cancellationToken = default)
        {
            events.Add("load");
            return failure is null ? Task.FromResult(Current) : Task.FromException<IStaticCatalogs>(failure);
        }
    }

    // Fake listener chỉ ghi thứ tự lời gọi, không tạo socket. (Java: ServerManager)
    private sealed class RecordingServer : IGameServer
    {
        private readonly List<string> events;

        // Tạo fake với danh sách sự kiện dùng chung. (Java: ServerManager)
        public RecordingServer(List<string> events) => this.events = events;

        // Ghi lúc Host mở listener. (Java: ServerManager.run)
        public void Start() => events.Add("start");

        // Ghi lúc Host khóa accept mới. (Java: ServerManager.close)
        public void StopAccepting() => events.Add("stop-accepting");

        // Ghi lúc Host đóng listener và các phiên. (Java: ServerManager.close)
        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            events.Add("stop");
            return Task.CompletedTask;
        }
    }

    // Fake lưu người chơi chỉ ghi vị trí trong shutdown. (Java: PlayerService.savePlayer)
    private sealed class RecordingSave : IPlayerSaveOnShutdown
    {
        private readonly List<string> events;

        // Tạo fake với danh sách sự kiện dùng chung. (Java: ServerManager.close)
        public RecordingSave(List<string> events) => this.events = events;

        // Ghi tác vụ lưu không chạm DB. (Java: PlayerService.savePlayer)
        public Task SaveOnlinePlayersAsync(CancellationToken cancellationToken)
        {
            events.Add("save");
            return Task.CompletedTask;
        }
    }
}
