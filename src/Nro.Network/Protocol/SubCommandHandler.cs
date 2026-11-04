using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nro.Core.IO;
using Nro.Network.Sessions;

namespace Nro.Network.Protocol;

// Phân phối gói có byte lệnh con đầu payload đến handler đã đăng ký.
// Nguồn Java: nro/server/Controller.java (tách switch lồng của -29, -28, -30; khác Java: handler -29 phải tự ngắt phiên khi payload login lỗi).
public sealed class SubCommandHandler(ILogger? logger = null) : ICommandHandler
{
    private readonly ConcurrentDictionary<sbyte, ICommandHandler> _subs = new();
    private readonly ILogger _logger = logger ?? NullLogger.Instance;

    // Đăng ký một handler cho lệnh con; mỗi lệnh con chỉ có một handler.
    public SubCommandHandler Register(sbyte subCommand, ICommandHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (!_subs.TryAdd(subCommand, handler))
        {
            throw new InvalidOperationException($"Sub-command {subCommand} already has a handler.");
        }

        return this;
    }

    // Đăng ký delegate làm handler cho một lệnh con.
    public SubCommandHandler Register(sbyte subCommand, Func<ISession, Message, CancellationToken, ValueTask> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return Register(subCommand, new DelegateSubHandler(handler));
    }

    // Đọc byte lệnh con rồi chuyển gói đã qua byte đó đến handler; payload rỗng hoặc handler thiếu bị bỏ im lặng như Java.
    public ValueTask HandleAsync(ISession session, Message message, CancellationToken cancellationToken)
    {
        if (message.Reader().Available < 1)
        {
            _logger.LogDebug("Session {Id}: command {Command} arrived without a sub-command byte", session.Id, message.Command);
            return ValueTask.CompletedTask;
        }

        sbyte sub = message.Reader().ReadByte();
        if (!_subs.TryGetValue(sub, out ICommandHandler? handler))
        {
            _logger.LogDebug("Session {Id}: command {Command} sub-command {Sub} has no handler", session.Id, message.Command, sub);
            return ValueTask.CompletedTask;
        }

        return handler.HandleAsync(session, message, cancellationToken);
    }

    private sealed class DelegateSubHandler(Func<ISession, Message, CancellationToken, ValueTask> action) : ICommandHandler
    {
        public ValueTask HandleAsync(ISession session, Message message, CancellationToken cancellationToken) => action(session, message, cancellationToken);
    }
}
