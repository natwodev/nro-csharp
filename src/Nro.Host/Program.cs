// Nguồn Java: nro/server/ServerManager.java (dòng 132–218, 520–713).
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nro.Core.Logging;
using Nro.Host.Extensions;
using Nro.Host.Hosting;

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

        builder.Services.AddNroHost(builder.Configuration);

        IHost host = builder.Build();
        await using IAsyncDisposable asyncHost = (IAsyncDisposable)host;
        Logger.Configure(host.Services.GetRequiredService<ILoggerFactory>());
        await host.RunAsync().ConfigureAwait(false);
    }

}
