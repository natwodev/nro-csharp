using Nro.Core.IO;

namespace Nro.Network.Interfaces;

/// <summary>
/// Handles one command of the protocol (one <c>case</c> of the Java <c>Controller.onMessage</c> switch). The packet is positioned at the start of its
/// payload; read what the command defines with <see cref="Message.Reader"/>.
/// </summary>
public interface ICommandHandler
{
    ValueTask HandleAsync(ISession session, Message message, CancellationToken cancellationToken);
}
