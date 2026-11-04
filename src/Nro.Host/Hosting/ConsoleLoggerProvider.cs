// Provider tạo logger console cho host TCP tối thiểu.
using Microsoft.Extensions.Logging;

namespace Nro.Host.Hosting;

// Cấp ConsoleLogger độc lập cho từng category log.
public sealed class ConsoleLoggerProvider : ILoggerProvider
{
    // Tạo logger console cho category được yêu cầu.
    public ILogger CreateLogger(string categoryName) => new ConsoleLogger();

    // Provider không giữ tài nguyên cần giải phóng.
    public void Dispose()
    {
    }
}
