// Logger console tối giản, chỉ ghi mức Information, Warning và Error.
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Nro.Host.Hosting;

// Ghi thông điệp vận hành ngắn gọn ra console mà không in exception hoặc dữ liệu gói.
public sealed class ConsoleLogger : ILogger
{
    private static readonly object WriteLock = new();

    // Scope không được host tối thiểu này sử dụng.
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    // Cho phép các mức log cần cho theo dõi host.
    public bool IsEnabled(LogLevel logLevel) => logLevel is LogLevel.Information or LogLevel.Warning or LogLevel.Error or LogLevel.Critical;

    // Định dạng và ghi một thông điệp log an toàn ra console.
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        ArgumentNullException.ThrowIfNull(formatter);
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string content = formatter(state, null);
        string timestamp = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
        lock (WriteLock)
        {
            Console.WriteLine($"{timestamp} [{logLevel}] {content}");
        }
    }
}
