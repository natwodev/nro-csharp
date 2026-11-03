namespace Nro.Network.Services;

/// <summary>Thread-safe counters kept by the <see cref="CommandDispatcher"/> (for diagnostics and tests).</summary>
public sealed class CommandStatistics
{
    private long _handled;
    private long _failed;
    private long _notImplemented;
    private long _unknown;
    private long _droppedNoCharacter;

    /// <summary>Commands whose handler completed.</summary>
    public long Handled => Interlocked.Read(ref _handled);

    /// <summary>Commands whose handler threw (the packet is lost, the connection stays open).</summary>
    public long Failed => Interlocked.Read(ref _failed);

    /// <summary>Commands that exist in the protocol but have no handler registered yet.</summary>
    public long NotImplemented => Interlocked.Read(ref _notImplemented);

    /// <summary>Commands whose number is not in the protocol at all.</summary>
    public long Unknown => Interlocked.Read(ref _unknown);

    /// <summary>Commands dropped because the session has no character yet.</summary>
    public long DroppedNoCharacter => Interlocked.Read(ref _droppedNoCharacter);

    internal void CountHandled() => Interlocked.Increment(ref _handled);

    internal void CountFailed() => Interlocked.Increment(ref _failed);

    internal void CountNotImplemented() => Interlocked.Increment(ref _notImplemented);

    internal void CountUnknown() => Interlocked.Increment(ref _unknown);

    internal void CountDroppedNoCharacter() => Interlocked.Increment(ref _droppedNoCharacter);
}
