// Handler chỉ ghi metadata gói nhận được, không xử lý nghiệp vụ hay gửi phản hồi.
// Nguồn Java: network/interfaces/IMessageHandler.java (khác Java: host thử nghiệm không có Controller).
using System.Reflection;
using Microsoft.Extensions.Logging;
using Nro.Core.IO;
using Nro.Network.Configurations;
using Nro.Network.Protocol;
using Nro.Network.Sessions;

namespace Nro.Host.Hosting;

// Ghi số phiên, mã lệnh, tên hằng số nếu có và độ dài payload của từng gói được phân phối.
public sealed class LoggingMessageHandler : IMessageHandler
{
    private static readonly IReadOnlyDictionary<sbyte, string> CommandNames = CreateCommandNames();
    private readonly ILogger _logger;

    // Tạo handler ghi metadata gói bằng logger được cung cấp.
    public LoggingMessageHandler(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    // Ghi metadata gói mà không đọc payload, không trả lời và không làm ngắt cmd lạ.
    public ValueTask OnMessageAsync(ISession session, Message message, CancellationToken cancellationToken)
    {
        int payloadLength = message.Reader().Available;
        string commandName = CommandNames.TryGetValue(message.Command, out string? name) ? $" ({name})" : string.Empty;
        _logger.LogInformation(
            "Phiên {SessionId}: nhận cmd {Command}{CommandName}, {PayloadLength} byte dữ liệu",
            session.Id,
            message.Command,
            commandName,
            payloadLength);
        return ValueTask.CompletedTask;
    }

    private static IReadOnlyDictionary<sbyte, string> CreateCommandNames() =>
        typeof(Cmd_message)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(sbyte))
            .Select(field => (Name: field.Name, Value: (sbyte)field.GetRawConstantValue()!))
            .GroupBy(item => item.Value)
            .ToDictionary(group => group.Key, group => group.First().Name);
}
