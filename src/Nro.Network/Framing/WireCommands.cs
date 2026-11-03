using Nro.Network.Consts;

namespace Nro.Network.Framing;

/// <summary>
/// Protocol constants shared by the frame encoder, the frame reader and the handshake
/// (see <c>docs/PROTOCOL.md</c> section 1 and <c>network/io/MessageSendCollect.java</c>).
/// </summary>
public static class WireCommands
{
    /// <summary>
    /// <c>-27</c>: the first packet of every connection. The client sends it with no payload to ask for the session key,
    /// and the server answers with a packet of the same command carrying the key (not encrypted).
    /// </summary>
    public const sbyte GetSessionId = Cmd_message.GET_SESSION_ID;

    /// <summary>
    /// The eight commands whose server-to-client packets carry a 3-byte size field instead of a 2-byte one
    /// (<c>MessageSendCollect.doSendMessage</c> lines 71-74). They exist because their payloads can exceed 64 KiB
    /// (<c>-28</c> carries the whole item-template update).
    /// </summary>
    public static readonly IReadOnlySet<sbyte> LargeSizeCommands = new HashSet<sbyte> { -32, -66, -74, 11, -67, -87, 66, -28 };

    /// <summary>The largest payload a 2-byte size field can describe.</summary>
    public const int MaxTwoByteSize = 0xFFFF;

    /// <summary>The largest payload a 3-byte size field can describe.</summary>
    public const int MaxThreeByteSize = 0xFFFFFF;

    /// <summary>True when server-to-client packets of <paramref name="command"/> use the 3-byte size field.</summary>
    public static bool IsLargeSize(sbyte command) => LargeSizeCommands.Contains(command);
}
