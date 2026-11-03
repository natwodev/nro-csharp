using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nro.Core.IO;
using Nro.Network.Configurations;
using Nro.Network.Interfaces;

namespace Nro.Network.Services;

/// <summary>
/// Routes each packet to the handler registered for its command. The replacement for the giant <c>switch</c> of the Java <c>Controller.onMessage</c>:
/// the network layer owns the rules every command shares, the game layer registers the handlers.
/// <list type="bullet">
/// <item><description><b>Unknown or not-yet-implemented commands never break the connection</b>: the packet is dropped and counted. A number that is not in the protocol
/// at all (<see cref="CommandRules.KnownCommands"/>) is logged as a warning, a protocol command that has no handler yet only at debug level.</description></item>
/// <item><description><b>Character gate</b>: a command that needs a character (every command except <see cref="CommandRules.NoCharacterRequired"/>, unless chosen otherwise
/// at registration) is silently dropped while <see cref="ICharacterGate.HasCharacter"/> is false, like <c>player != null</c> in Java.</description></item>
/// <item><description><b>One bad packet costs one packet</b>: an exception in a handler (including a truncated payload) is caught, counted and logged, and the next packet is
/// handled normally. Each session may log at most <see cref="CommandDispatcherOptions.MaxLoggedErrorsPerSession"/> errors.</description></item>
/// <item><description>A command that runs longer than <see cref="CommandDispatcherOptions.SlowCommandThreshold"/> is logged as slow.</description></item>
/// </list>
/// Cancellation (the session is closing) is not an error: it propagates.
/// </summary>
public sealed class CommandDispatcher : IMessageHandler
{
    private readonly record struct Route(ICommandHandler Handler, bool RequiresCharacter);

    private sealed class ErrorBudget
    {
        public int Logged;
        public int LoggedUnknown;
    }

    private sealed class DelegateHandler(Func<ISession, Message, CancellationToken, ValueTask> action) : ICommandHandler
    {
        public ValueTask HandleAsync(ISession session, Message message, CancellationToken cancellationToken) => action(session, message, cancellationToken);
    }

    private readonly ConcurrentDictionary<sbyte, Route> _routes = new();
    private readonly ConditionalWeakTable<ISession, ErrorBudget> _budgets = new();
    private readonly ICharacterGate _gate;
    private readonly CommandDispatcherOptions _options;
    private readonly ILogger _logger;

    public CommandDispatcher(ICharacterGate gate, CommandDispatcherOptions? options = null, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(gate);
        _gate = gate;
        _options = options ?? new CommandDispatcherOptions();
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>Counters of what happened to the packets so far.</summary>
    public CommandStatistics Statistics { get; } = new();

    /// <summary>
    /// Registers the handler of <paramref name="command"/>. <paramref name="requiresCharacter"/> defaults to the rule of the protocol:
    /// true unless the command is in <see cref="CommandRules.NoCharacterRequired"/>. A command can have only one handler.
    /// </summary>
    public CommandDispatcher Register(sbyte command, ICommandHandler handler, bool? requiresCharacter = null)
    {
        ArgumentNullException.ThrowIfNull(handler);
        bool requires = requiresCharacter ?? !CommandRules.NoCharacterRequired.Contains(command);
        if (!_routes.TryAdd(command, new Route(handler, requires)))
        {
            throw new InvalidOperationException($"Command {command} already has a handler.");
        }

        return this;
    }

    /// <summary>Registers a delegate as the handler of <paramref name="command"/> (see the other overload).</summary>
    public CommandDispatcher Register(sbyte command, Func<ISession, Message, CancellationToken, ValueTask> handler, bool? requiresCharacter = null)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return Register(command, new DelegateHandler(handler), requiresCharacter);
    }

    public async ValueTask OnMessageAsync(ISession session, Message message, CancellationToken cancellationToken)
    {
        sbyte command = message.Command;
        if (!_routes.TryGetValue(command, out Route route))
        {
            if (CommandRules.KnownCommands.Contains(command))
            {
                Statistics.CountNotImplemented();
                _logger.LogDebug("Session {Id}: command {Command} has no handler yet", session.Id, command);
            }
            else
            {
                Statistics.CountUnknown();
                ErrorBudget unknownBudget = _budgets.GetOrCreateValue(session);
                if (Interlocked.Increment(ref unknownBudget.LoggedUnknown) <= _options.MaxLoggedErrorsPerSession)
                {
                    _logger.LogWarning("Session {Id} ({Ip}): command {Command} is not part of the protocol; packet dropped", session.Id, session.IpAddress, command);
                }
            }

            return;
        }

        if (route.RequiresCharacter && !_gate.HasCharacter(session))
        {
            Statistics.CountDroppedNoCharacter();
            return;
        }

        long start = Stopwatch.GetTimestamp();
        try
        {
            await route.Handler.HandleAsync(session, message, cancellationToken).ConfigureAwait(false);
            Statistics.CountHandled();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            Statistics.CountFailed();
            ErrorBudget budget = _budgets.GetOrCreateValue(session);
            if (Interlocked.Increment(ref budget.Logged) <= _options.MaxLoggedErrorsPerSession)
            {
                _logger.LogError(ex, "Session {Id} ({Ip}): handler of command {Command} failed; packet dropped", session.Id, session.IpAddress, command);
            }
        }
        finally
        {
            TimeSpan took = Stopwatch.GetElapsedTime(start);
            if (took > _options.SlowCommandThreshold)
            {
                _logger.LogWarning("Session {Id}: command {Command} took {Milliseconds} ms", session.Id, command, (long)took.TotalMilliseconds);
            }
        }
    }
}
