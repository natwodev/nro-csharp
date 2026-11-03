namespace Nro.Network.Framing;

/// <summary>
/// A payload does not fit the packet's size field (above 65535 bytes for an ordinary command, above 16777215 for the eight large ones).
/// The Java original silently truncates the size and produces a corrupt frame (golden quirks <c>non-extended-size-overflow-65536-*</c>
/// and <c>size-overflow-16777216</c>); this port refuses to encode instead.
/// </summary>
public sealed class FrameTooLargeException(sbyte command, int size, int maximum)
    : InvalidOperationException($"Payload of {size} bytes is too large for command {command}: the size field holds at most {maximum}.")
{
    public sbyte Command { get; } = command;

    public int Size { get; } = size;

    public int Maximum { get; } = maximum;
}
