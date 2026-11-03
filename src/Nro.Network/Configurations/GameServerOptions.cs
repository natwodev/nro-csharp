using System.Net;
using Nro.Network.Sessions;

namespace Nro.Network.Configurations;

/// <summary>Settings of the TCP listener (<see cref="Services.GameServer"/>).</summary>
public sealed class GameServerOptions
{
    /// <summary>Java <c>ServerManager.PORT</c>. 0 lets the operating system pick a free port (used by the tests).</summary>
    public int Port { get; init; } = 14445;

    /// <summary>The local address to listen on. <c>null</c> means every interface (IPv4 and IPv6 when the machine supports it), like Java's wildcard bind.</summary>
    public IPAddress? BindAddress { get; init; }

    /// <summary>Java <c>Manager.MAX_PER_IP</c>: simultaneous connections allowed from one IP address.</summary>
    public int MaxConnectionsPerIp { get; init; } = 3;

    /// <summary>Java <c>setSendBufferSize/setReceiveBufferSize(0x100000)</c>.</summary>
    public int SocketBufferBytes { get; init; } = 0x100000;

    /// <summary>
    /// Turns Nagle's algorithm off. Java left it on; every packet here is written with a single call, so keeping it on only adds
    /// up to a delayed-ACK interval of latency to small packets.
    /// </summary>
    public bool NoDelay { get; init; } = true;

    /// <summary>Limits applied to each session.</summary>
    public SessionOptions Session { get; init; } = new();
}
