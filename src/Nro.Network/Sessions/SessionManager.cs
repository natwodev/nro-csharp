using System.Collections.Concurrent;

namespace Nro.Network.Sessions;

// Quản lý các phiên đang kết nối và cấp mã phiên.
// Nguồn Java: network/session/SessionManager.java (khác Java ArrayList không khóa: dùng ConcurrentDictionary an toàn luồng).
public sealed class SessionManager : ISessionManager
{
    private readonly ConcurrentDictionary<long, Session> _sessions = new();
    private long _nextId = -1;

    // Số phiên đang được đăng ký.
    public int Count => _sessions.Count;

    // Cấp mã từ 0, tăng một cho từng kết nối (Java: Session.ID_INIT++).
    public long NextId() => Interlocked.Increment(ref _nextId);

    // Đăng ký một phiên; không cho phép trùng mã.
    public void Add(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (!_sessions.TryAdd(session.Id, session))
        {
            throw new InvalidOperationException($"A session with id {session.Id} is already registered.");
        }
    }

    // Gỡ đúng phiên theo mã và tham chiếu.
    public bool Remove(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return _sessions.TryRemove(new KeyValuePair<long, Session>(session.Id, session));
    }

    // Tìm phiên theo mã.
    public bool TryGet(long id, out ISession? session)
    {
        bool found = _sessions.TryGetValue(id, out Session? s);
        session = s;
        return found;
    }

    // Trả ảnh chụp các phiên theo thứ tự mã.
    public IReadOnlyList<ISession> Snapshot() => [.. _sessions.Values.OrderBy(s => s.Id)];
}
