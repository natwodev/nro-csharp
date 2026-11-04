// Factory logger tối giản chỉ cấp logger console cho host thử nghiệm.
using Microsoft.Extensions.Logging;

namespace Nro.Host.Hosting;

// Cấp logger console mà không cần Generic Host hoặc package mới.
public sealed class HostLoggerFactory : ILoggerFactory
{
    private readonly ConsoleLoggerProvider _provider = new();

    // Thêm provider không được hỗ trợ vì host tối thiểu chỉ dùng console.
    public void AddProvider(ILoggerProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        throw new NotSupportedException("Host tối thiểu chỉ hỗ trợ ConsoleLoggerProvider.");
    }

    // Tạo logger console cho category được yêu cầu.
    public ILogger CreateLogger(string categoryName) => _provider.CreateLogger(categoryName);

    // Giải phóng provider console.
    public void Dispose() => _provider.Dispose();
}
