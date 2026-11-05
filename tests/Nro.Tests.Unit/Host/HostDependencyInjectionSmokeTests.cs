using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nro.Host.Extensions;

namespace Nro.Tests.Unit.Host;

// Bảo vệ entry point: graph DI thật phải dựng, resolve hosted service và mở/đóng listener không DB.
public sealed class HostDependencyInjectionSmokeTests
{
    [Fact]
    public async Task AddNroHost_validates_and_starts_without_static_catalogs()
    {
        HostApplicationBuilder builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = Environments.Development,
        });
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["GameServer:Port"] = "0",
            ["GameServer:BindAddress"] = "127.0.0.1",
            ["GameServer:MaxConnectionsPerIp"] = "1",
            ["GameServer:SocketBufferBytes"] = "1024",
            ["GameServer:NoDelay"] = "true",
            ["GameServer:Session:InboundQueueLimit"] = "1",
            ["GameServer:Session:OutboundQueueLimit"] = "1",
            ["GameServer:Session:HandshakeTimeout"] = "00:00:01",
            ["Startup:LoadStaticCatalogs"] = "false",
            ["Startup:ShutdownSaveTimeout"] = "00:00:01",
            ["Persistence:ConnectionName"] = "Nro",
            ["Scheduling:AutoSaveInterval"] = "00:00:01",
            ["Assets:Root"] = string.Empty,
        });
        builder.Services.AddNroHost(builder.Configuration);

        await using (ServiceProvider provider = builder.Services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        }))
        {
            Assert.NotEmpty(provider.GetServices<IHostedService>());
        }

        IHost host = builder.Build();
        await using IAsyncDisposable asyncHost = (IAsyncDisposable)host;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        await host.StartAsync(timeout.Token);
        await host.StopAsync(timeout.Token);
    }
}
