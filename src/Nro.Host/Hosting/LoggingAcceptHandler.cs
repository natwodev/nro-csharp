// Handler chỉ ghi vòng đời mở và ngắt của phiên TCP.
// Nguồn Java: network/interfaces/ISessionAcceptHandler.java.
using Microsoft.Extensions.Logging;
using Nro.Network.Sessions;

namespace Nro.Host.Hosting;

// Ghi địa chỉ client và mã phiên khi kết nối thay đổi trạng thái.
public sealed class LoggingAcceptHandler : ISessionAcceptHandler
{
    private readonly ILogger _logger;

    // Tạo handler ghi vòng đời phiên bằng logger được cung cấp.
    public LoggingAcceptHandler(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    // Ghi phiên mới mở với địa chỉ IP:cổng xa của client.
    public void SessionInit(ISession session) =>
        _logger.LogInformation("Phiên mở: {SessionId} từ {RemoteEndPoint}", session.Id, session.RemoteEndPoint);

    // Ghi phiên ngắt; lý do không có trong hợp đồng ISession nên không suy diễn.
    public void SessionDisconnect(ISession session) =>
        _logger.LogInformation("Phiên ngắt: {SessionId} từ {RemoteEndPoint}", session.Id, session.RemoteEndPoint);
}
