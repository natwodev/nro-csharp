using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nro.Core.IO;
using Nro.Network.Interfaces;

namespace Nro.Network.Services;

/// <summary>
/// The handler of a command whose payload starts with a sub-command byte (the nested <c>switch</c> of <c>-29</c>, <c>-28</c>, <c>-30</c> and others, see
/// section 3.3 of <c>docs/PROTOCOL.md</c>). It reads that first byte and passes the packet, now positioned after it, to the handler registered
/// for that sub-command. An empty payload or a sub-command without a handler is dropped quietly (Java read the byte, fell through the
/// <c>switch</c> and did nothing, or threw on an empty payload, which was logged).
/// Known difference: for <c>-29</c> (login) Java catches <c>IOException</c> (empty or truncated payload, bad UTF) and DISCONNECTS the session
/// (<c>Controller.messageNotLogin</c>); here a handler failure only drops the packet, so the <c>-29</c> handler of the game layer must call
/// <see cref="ISession.Disconnect"/> itself on a malformed login packet. <c>-28</c> and <c>-30</c> only log in Java, as here.
/// </summary>
public sealed class SubCommandHandler(ILogger? logger = null) : ICommandHandler
{
    private readonly ConcurrentDictionary<sbyte, ICommandHandler> _subs = new();
    private readonly ILogger _logger = logger ?? NullLogger.Instance;

    /// <summary>Registers the handler of one sub-command. A sub-command can have only one handler.</summary>
    public SubCommandHandler Register(sbyte subCommand, ICommandHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (!_subs.TryAdd(subCommand, handler))
        {
            throw new InvalidOperationException($"Sub-command {subCommand} already has a handler.");
        }

        return this;
    }

    /// <summary>Registers a delegate as the handler of one sub-command.</summary>
    public SubCommandHandler Register(sbyte subCommand, Func<ISession, Message, CancellationToken, ValueTask> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return Register(subCommand, new DelegateSubHandler(handler));
    }

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
