namespace Nro.Network.Interfaces;

/// <summary>The sessions currently connected. Port of the lookup side of <c>network/session/SessionManager.java</c>; thread-safe.</summary>
public interface ISessionManager
{
    /// <summary>Java <c>getNumSession()</c>.</summary>
    int Count { get; }

    /// <summary>
    /// Java <c>findByID</c>. The Java version throws when the id is unknown (and stops early once it passes the id, which assumes ordered ids);
    /// this one simply reports whether the session exists.
    /// </summary>
    bool TryGet(long id, out ISession? session);

    /// <summary>Java <c>getSessions()</c>: a copy, so iterating while sessions come and go is safe.</summary>
    IReadOnlyList<ISession> Snapshot();
}
