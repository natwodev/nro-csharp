using Nro.Core.IO;

namespace Nro.Network.Interfaces;

/// <summary>
/// Receives the packets of a session, one at a time and in the order they arrived. Port of <c>network/interfaces/IMessageHandler.java</c>
/// (the game's <c>Controller.onMessage</c> implements it).
/// </summary>
public interface IMessageHandler
{
    /// <summary>
    /// Handles one packet. A packet is never delivered while the previous one is still being handled. An exception is logged and the next
    /// packet is still delivered (in Java an exception silently ended the whole delivery loop).
    /// </summary>
    ValueTask OnMessageAsync(ISession session, Message message, CancellationToken cancellationToken);
}
