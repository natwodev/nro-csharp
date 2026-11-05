// Nguồn Java: nro/server/ServerManager.java (dòng 132–218, 520–713).
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nro.Content.StaticData;
using Nro.Core.Logging;
using Nro.Data.Db;
using Nro.Host.Configurations;
using Nro.Host.Hosting;
using Nro.Network.Protocol;
using NetworkGameServerOptions = Nro.Network.Configurations.GameServerOptions;
using NetworkSessionOptions = Nro.Network.Sessions.SessionOptions;

namespace Nro.Host;

// Điểm vào Generic Host, cấu hình DI trước khi dịch vụ nền chạy. (Java: ServerManager)
public static class Program
{
    // Tạo và chạy Host; ngoại lệ bootstrap không bị nuốt để tiến trình kết thúc mã khác 0. (Java: ServerManager.run)
    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);

        HostApplicationBuilder builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        builder.Configuration.AddCommandLine(args, new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["--port"] = "GameServer:Port",
            ["--bind"] = "GameServer:BindAddress",
        });
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddSingleton<IValidateOptions<GameServerOptions>, GameServerOptionsValidator>();
        builder.Services.AddSingleton<IValidateOptions<StartupOptions>, StartupOptionsValidator>();
        builder.Services.AddSingleton<IValidateOptions<SchedulingOptions>, SchedulingOptionsValidator>();
        builder.Services.AddSingleton<IValidateOptions<PersistenceOptions>, PersistenceOptionsValidator>();
        builder.Services.AddSingleton<IValidateOptions<AssetsOptions>, AssetsOptionsValidator>();
        builder.Services.AddOptions<GameServerOptions>().Bind(builder.Configuration.GetSection("GameServer")).ValidateOnStart();
        builder.Services.AddOptions<StartupOptions>().Bind(builder.Configuration.GetSection("Startup")).ValidateOnStart();
        builder.Services.AddOptions<PersistenceOptions>().Bind(builder.Configuration.GetSection("Persistence")).ValidateOnStart();
        builder.Services.AddOptions<SchedulingOptions>().Bind(builder.Configuration.GetSection("Scheduling")).ValidateOnStart();
        builder.Services.AddOptions<AssetsOptions>().Bind(builder.Configuration.GetSection("Assets")).ValidateOnStart();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<NroDatabase>(CreateDatabase);
        builder.Services.AddSingleton<INroSqlExecutor>(services => services.GetRequiredService<NroDatabase>());
        builder.Services.AddSingleton<IStaticRowSource, NroSqlStaticRowSource>();
        builder.Services.AddSingleton<Nro.Game.Content.IStaticCatalogLoader, HostStaticCatalogLoader>();
        builder.Services.AddSingleton<LoggingMessageHandler>();
        builder.Services.AddSingleton<LoggingAcceptHandler>();
        builder.Services.AddSingleton<GameServer>(CreateGameServer);
        builder.Services.AddSingleton<IGameServer, GameServerAdapter>();
        builder.Services.AddSingleton<IPlayerSaveOnShutdown, LoggingPlayerSaveOnShutdown>();
        builder.Services.AddHostedService<GameServerHostedService>();

        using IHost host = builder.Build();
        Logger.Configure(host.Services.GetRequiredService<ILoggerFactory>());
        await host.RunAsync().ConfigureAwait(false);
    }

    // Dựng NroDatabase từ tên ConnectionStrings đã được validator xác nhận, không log bí mật. (Java: Manager.loadProperties)
    private static NroDatabase CreateDatabase(IServiceProvider services)
    {
        IConfiguration configuration = services.GetRequiredService<IConfiguration>();
        PersistenceOptions persistence = services.GetRequiredService<IOptions<PersistenceOptions>>().Value;
        string connectionString = configuration.GetConnectionString(persistence.ConnectionName)
            ?? throw new InvalidOperationException($"Thiếu ConnectionStrings:{persistence.ConnectionName}.");
        return new NroDatabase(new NroDatabaseOptions { ConnectionString = connectionString }, services.GetRequiredService<ILogger<NroDatabase>>());
    }

    // Ánh xạ Options Host sang listener Network hiện hữu, giữ handler logging đến J-03A. (Java: ServerManager.run)
    private static GameServer CreateGameServer(IServiceProvider services)
    {
        GameServerOptions options = services.GetRequiredService<IOptions<GameServerOptions>>().Value;
        IPAddress? bindAddress = string.IsNullOrWhiteSpace(options.BindAddress) ? null : IPAddress.Parse(options.BindAddress);
        var networkOptions = new NetworkGameServerOptions
        {
            Port = options.Port,
            BindAddress = bindAddress,
            MaxConnectionsPerIp = options.MaxConnectionsPerIp,
            SocketBufferBytes = options.SocketBufferBytes,
            NoDelay = options.NoDelay,
            Session = new NetworkSessionOptions
            {
                InboundQueueLimit = options.Session.InboundQueueLimit,
                OutboundQueueLimit = options.Session.OutboundQueueLimit,
                HandshakeTimeout = options.Session.HandshakeTimeout,
                IdleTimeout = options.Session.IdleTimeout,
            },
        };
        return new GameServer(
            networkOptions,
            services.GetRequiredService<LoggingMessageHandler>(),
            services.GetRequiredService<LoggingAcceptHandler>(),
            services.GetRequiredService<ILogger<GameServer>>());
    }
}
