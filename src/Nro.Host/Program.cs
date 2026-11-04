// Điểm vào host TCP tối thiểu để kiểm tra bắt tay client Unity.
// Nguồn Java: network/Network.java (chỉ phần mở cổng; chưa có đăng nhập hay vòng lặp game).
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Nro.Host.Hosting;
using Nro.Network.Configurations;
using Nro.Network.Protocol;

namespace Nro.Host;

// Khởi động và dừng host TCP tối thiểu từ dòng lệnh.
public static class Program
{
    // Đọc tham số, khởi động GameServer và chờ Ctrl+C để dừng gọn gàng.
    public static async Task<int> Main(string[] args)
    {
        if (!TryReadOptions(args, out GameServerOptions options, out string? error))
        {
            Console.Error.WriteLine(error);
            Console.Error.WriteLine("Cách dùng: Nro.Host [--port <0-65535>] [--bind <địa chỉ IP>]");
            return 2;
        }

        using var loggerFactory = new HostLoggerFactory();
        ILogger logger = loggerFactory.CreateLogger("Nro.Host");
        await using var server = new GameServer(
            options,
            new LoggingMessageHandler(loggerFactory.CreateLogger(nameof(LoggingMessageHandler))),
            new LoggingAcceptHandler(loggerFactory.CreateLogger(nameof(LoggingAcceptHandler))),
            logger);

        try
        {
            server.Start();
        }
        catch (SocketException ex) when (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
        {
            Console.Error.WriteLine($"Không thể mở cổng {options.Port}: cổng này đang được sử dụng.");
            return 1;
        }
        catch (SocketException ex)
        {
            Console.Error.WriteLine($"Không thể mở cổng {options.Port}: lỗi socket {ex.SocketErrorCode}.");
            return 1;
        }

        Console.WriteLine($"Nro.Host đang lắng nghe cổng {server.Port}. Nhấn Ctrl+C để dừng.");

        var stopRequested = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        ConsoleCancelEventHandler onCancelKeyPress = (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            stopRequested.TrySetResult();
        };

        Console.CancelKeyPress += onCancelKeyPress;
        try
        {
            await stopRequested.Task.ConfigureAwait(false);
            await server.StopAsync().ConfigureAwait(false);
            return 0;
        }
        finally
        {
            Console.CancelKeyPress -= onCancelKeyPress;
        }
    }

    private static bool TryReadOptions(string[] args, out GameServerOptions options, out string? error)
    {
        int port = new GameServerOptions().Port;
        IPAddress? bindAddress = null;
        error = null;

        for (int index = 0; index < args.Length; index++)
        {
            string argument = args[index];
            if (argument is not ("--port" or "--bind") || ++index >= args.Length)
            {
                options = null!;
                error = $"Tham số không hợp lệ: {argument}.";
                return false;
            }

            string value = args[index];
            if (argument == "--port")
            {
                if (!int.TryParse(value, out port) || port is < 0 or > ushort.MaxValue)
                {
                    options = null!;
                    error = $"Cổng không hợp lệ: {value}.";
                    return false;
                }
            }
            else if (!IPAddress.TryParse(value, out bindAddress))
            {
                options = null!;
                error = $"Địa chỉ IP không hợp lệ: {value}.";
                return false;
            }
        }

        options = new GameServerOptions
        {
            Port = port,
            BindAddress = bindAddress,
        };
        return true;
    }
}
