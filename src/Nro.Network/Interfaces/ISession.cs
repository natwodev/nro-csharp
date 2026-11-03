using Nro.Core.IO;

namespace Nro.Network.Interfaces;

/// <summary>
/// One client connection, as the game code sees it. Port of the part of <c>network/interfaces/ISession.java</c> the game uses; the plumbing
/// methods of the Java interface (<c>setSendCollect</c>, <c>startSend</c>, <c>sendKey</c>, <c>setKey</c>, <c>doSendMessage</c>...) belong to the
/// implementation (<see cref="Sessions.Session"/>) because the C# version has no separate sender, collector and queue threads.
/// </summary>
public interface ISession
{
    /// <summary>Java <c>getID()</c>: the number the session manager gave this connection.</summary>
    long Id { get; }

    /// <summary>Java <c>getIP()</c>: the client's address (without the port).</summary>
    string IpAddress { get; }

    /// <summary>Java <c>isConnected()</c>: false once the session was disconnected by either side.</summary>
    bool IsConnected { get; }

    /// <summary>
    /// Java <c>sendMessage(Message)</c>: queues <paramref name="message"/> for sending. The payload is captured at this call, so the caller may
    /// <see cref="Message.Cleanup"/> or dispose the message right away. Returns false when the message was not queued (session closed).
    /// </summary>
    bool Send(Message message);

    /// <summary>Java <c>disconnect()</c>: closes the connection. Safe to call more than once and from any thread.</summary>
    void Disconnect();
}
