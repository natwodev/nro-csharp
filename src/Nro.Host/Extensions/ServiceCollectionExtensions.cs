// Nguồn Java: nro/server/ServerManager.java (dòng 132–218, 520–713).
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nro.Content.StaticData;
using Nro.Data.Db;
using Nro.Host.Configurations;
using Nro.Host.Hosting;
using Nro.Network.Protocol;
using NetworkGameServerOptions = Nro.Network.Configurations.GameServerOptions;
using NetworkSessionOptions = Nro.Network.Sessions.SessionOptions;

namespace Nro.Host.Extensions;

// Đăng ký toàn bộ graph DI vận hành của Nro.Host để entry point và smoke test luôn dùng cùng cấu hình.
public static class ServiceCollectionExtensions
{
    // Thêm graph DI của Host; các handler nhận ILogger không generic qua ILoggerFactory vì container chỉ có ILogger<T> mặc định.
    public static IServiceCollection AddNroHost(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton<IValidateOptions<GameServerOptions>, GameServerOptionsValidator>();
        services.AddSingleton<IValidateOptions<StartupOptions>, StartupOptionsValidator>();
        services.AddSingleton<IValidateOptions<SchedulingOptions>, SchedulingOptionsValidator>();
        services.AddSingleton<IValidateOptions<PersistenceOptions>, PersistenceOptionsValidator>();
        services.AddSingleton<IValidateOptions<AssetsOptions>, AssetsOptionsValidator>();
        services.AddOptions<GameServerOptions>().Bind(configuration.GetSection("GameServer")).ValidateOnStart();
        services.AddOptions<StartupOptions>().Bind(configuration.GetSection("Startup")).ValidateOnStart();
        services.AddOptions<PersistenceOptions>().Bind(configuration.GetSection("Persistence")).ValidateOnStart();
        services.AddOptions<SchedulingOptions>().Bind(configuration.GetSection("Scheduling")).ValidateOnStart();
        services.AddOptions<AssetsOptions>().Bind(configuration.GetSection("Assets")).ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<NroDatabase>(CreateDatabase);
        services.AddSingleton<INroSqlExecutor>(provider => provider.GetRequiredService<NroDatabase>());
        services.AddSingleton<IStaticRowSource, NroSqlStaticRowSource>();
        services.AddSingleton<Nro.Game.Content.IStaticCatalogLoader, HostStaticCatalogLoader>();
        services.AddSingleton<LoggingMessageHandler>(CreateLoggingMessageHandler);
        services.AddSingleton<LoggingAcceptHandler>(CreateLoggingAcceptHandler);
        services.AddSingleton<GameServer>(CreateGameServer);
        services.AddSingleton<IGameServer, GameServerAdapter>();
        services.AddSingleton<IPlayerSaveOnShutdown, LoggingPlayerSaveOnShutdown>();
        services.AddHostedService<GameServerHostedService>();
        return services;
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

    // DI mặc định không đăng ký ILogger không generic; tạo category logger rõ ràng cho hai handler logging.
    private static LoggingMessageHandler CreateLoggingMessageHandler(IServiceProvider services) =>
        new(services.GetRequiredService<ILoggerFactory>().CreateLogger<LoggingMessageHandler>());

    private static LoggingAcceptHandler CreateLoggingAcceptHandler(IServiceProvider services) =>
        new(services.GetRequiredService<ILoggerFactory>().CreateLogger<LoggingAcceptHandler>());

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
