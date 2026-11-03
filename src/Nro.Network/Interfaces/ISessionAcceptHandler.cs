namespace Nro.Network.Interfaces;

/// <summary>Port of <c>network/interfaces/ISessionAcceptHandler.java</c>: told when a session starts and when it has ended.</summary>
public interface ISessionAcceptHandler
{
    /// <summary>Java <c>sessionInit</c>: a new connection was accepted. Called before its first packet is read.</summary>
    void SessionInit(ISession session);

    /// <summary>Java <c>sessionDisconnect</c>: the session ended, whoever closed it. Called exactly once per session.</summary>
    void SessionDisconnect(ISession session);
}
