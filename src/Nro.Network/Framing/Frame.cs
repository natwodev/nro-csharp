using Nro.Core.IO;

namespace Nro.Network.Framing;

/// <summary>A decoded packet: the command and its payload (already decrypted).</summary>
public readonly record struct Frame(sbyte Command, byte[] Payload)
{
    /// <summary>A read-mode <see cref="Message"/> over the payload (what <c>Controller.onMessage</c> consumes). The payload array is not copied.</summary>
    public Message ToMessage() => new(Command, Payload);
}
