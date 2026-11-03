using System.Collections.Concurrent;
using Nro.Network.Interfaces;
using Nro.Network.Sessions;

namespace Nro.Network.Services;

/// <summary>
/// Thread-safe registry of the connected sessions and the source of session ids. Port of <c>network/session/SessionManager.java</c>
/// (a plain <c>ArrayList</c> that the accept thread and every session thread modified without any locking).
/// </summary>
public sealed class SessionManager : ISessionManager
{
    private readonly ConcurrentDictionary<long, Session> _sessions = new();
    private long _nextId = -1;

    public int Count => _sessions.Count;

    /// <summary>Java <c>Session.ID_INIT++</c>: ids start at 0 and increase by 1 for every connection.</summary>
    public long NextId() => Interlocked.Increment(ref _nextId);

    public void Add(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!_sessions.TryAdd(session.Id, session))
        {
            throw new InvalidOperationException($"A session with id {session.Id} is already registered.");
        }
    }

    public bool Remove(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return _sessions.TryRemove(new KeyValuePair<long, Session>(session.Id, session));
    }

    public bool TryGet(long id, out ISession? session)
    {
        bool found = _sessions.TryGetValue(id, out Session? s);
        session = s;
        return found;
    }

    public IReadOnlyList<ISession> Snapshot() => [.. _sessions.Values.OrderBy(s => s.Id)];
}
