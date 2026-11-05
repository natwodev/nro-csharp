// Nguồn Java: nro/server/ServerManager.java (dòng 520–582).
using System.Net;
using Microsoft.Extensions.Options;

namespace Nro.Host.Configurations;

// Kiểm các giới hạn listener trước khi HostedService được khởi động. (Java: ServerManager)
public sealed class GameServerOptionsValidator : IValidateOptions<GameServerOptions>
{
    // Kiểm các giới hạn IP, buffer, địa chỉ và thời hạn phiên. (Java: ServerManager)
    public ValidateOptionsResult Validate(string? name, GameServerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var failures = new List<string>();
        if (options.Port is < 0 or > ushort.MaxValue)
        {
            failures.Add("GameServer:Port phải nằm trong khoảng 0 đến 65535.");
        }

        if (!string.IsNullOrWhiteSpace(options.BindAddress) && !IPAddress.TryParse(options.BindAddress, out _))
        {
            failures.Add("GameServer:BindAddress phải là một địa chỉ IP hợp lệ hoặc để trống.");
        }

        if (options.MaxConnectionsPerIp <= 0)
        {
            failures.Add("GameServer:MaxConnectionsPerIp phải lớn hơn 0.");
        }

        if (options.SocketBufferBytes <= 0)
        {
            failures.Add("GameServer:SocketBufferBytes phải lớn hơn 0.");
        }

        if (options.Session.InboundQueueLimit <= 0 || options.Session.OutboundQueueLimit <= 0)
        {
            failures.Add("GameServer:Session queue limits phải lớn hơn 0.");
        }

        if (options.Session.HandshakeTimeout <= TimeSpan.Zero
            || (options.Session.IdleTimeout is TimeSpan idleTimeout && idleTimeout <= TimeSpan.Zero))
        {
            failures.Add("GameServer:Session timeouts phải lớn hơn 0 khi được cấu hình.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
