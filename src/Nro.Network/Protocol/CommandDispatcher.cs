using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nro.Core.IO;
using Nro.Network.Configurations;
using Nro.Network.Ports;
using Nro.Network.Sessions;

namespace Nro.Network.Protocol;

// Phân phối từng gói đến handler đăng ký theo mã lệnh và áp dụng các quy tắc chung của giao thức.
// Nguồn Java: nro/server/Controller.java (tách switch lớn; lệnh lạ/chưa làm bị bỏ và đếm, lỗi handler chỉ mất một gói, hủy phiên được ném tiếp).
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

    // Tạo bộ phân phối với cổng kiểm tra nhân vật, cấu hình và logger tùy chọn.
    public CommandDispatcher(ICharacterGate gate, CommandDispatcherOptions? options = null, ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(gate);
        _gate = gate;
        _options = options ?? new CommandDispatcherOptions();
        _logger = logger ?? NullLogger.Instance;
    }

    // Các bộ đếm xử lý gói hiện có.
    public CommandStatistics Statistics { get; } = new();

    // Đăng ký handler cho lệnh; mặc định yêu cầu nhân vật trừ lệnh trong NoCharacterRequired, và mỗi lệnh chỉ có một handler.
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

    // Đăng ký delegate làm handler cho lệnh với quy tắc nhân vật như overload còn lại.
    public CommandDispatcher Register(sbyte command, Func<ISession, Message, CancellationToken, ValueTask> handler, bool? requiresCharacter = null)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return Register(command, new DelegateHandler(handler), requiresCharacter);
    }

    // Xử lý một gói: bỏ lệnh lạ/chưa cài hoặc thiếu nhân vật, gọi handler và giới hạn log lỗi mỗi phiên.
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
