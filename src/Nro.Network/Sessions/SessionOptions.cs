namespace Nro.Network.Sessions;

/// <summary>Limits of one <see cref="Session"/>.</summary>
public sealed class SessionOptions
{
    /// <summary>
    /// Java <c>QueueHandler.addMessage</c>: at most this many received packets wait for the handler; further packets are dropped silently
    /// (counted in <see cref="Session.DroppedInboundCount"/>). Default 500, the original value.
    /// </summary>
    public int InboundQueueLimit { get; init; } = 500;

    /// <summary>
    /// Packets waiting to be written. The Java queue is unbounded, so a client that stops reading made the server's memory grow without limit.
    /// Here a session whose queue reaches this size is disconnected (a client that far behind is not reading). Default 10000.
    /// </summary>
    public int OutboundQueueLimit { get; init; } = 10_000;

    /// <summary>
    /// How long a new connection may stay silent before the hello (<c>-27</c>) arrives. Java has no limit, so three idle sockets held all three slots
    /// of an IP for ever. Default 30 seconds.
    /// </summary>
    public TimeSpan HandshakeTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// How long an established connection may send nothing before it is closed; null (the default) means no limit, as in Java. Turn it on only after
    /// checking that the client sends a periodic packet on every screen.
    /// </summary>
    public TimeSpan? IdleTimeout { get; init; }

    /// <summary>
    /// Commands that carry a 3-byte size when the CLIENT sends them. Empty by default (the specification and the Java server read client packets
    /// with a 2-byte size). Kept configurable for the open question whether real clients use 3 bytes for <c>66</c> and <c>-28</c>.
    /// </summary>
    public IReadOnlySet<sbyte> ClientThreeByteCommands { get; init; } = new HashSet<sbyte>();
}
