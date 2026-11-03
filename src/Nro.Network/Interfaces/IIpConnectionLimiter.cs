namespace Nro.Network.Interfaces;

/// <summary>Limits how many connections one IP address may hold at the same time (Java <c>ServerManager.canConnectWithIp</c> and <c>disconnect</c>).</summary>
public interface IIpConnectionLimiter
{
    /// <summary>Takes one connection slot for <paramref name="ip"/>. Returns false when the IP already holds the maximum.</summary>
    bool TryAcquire(string ip);

    /// <summary>Gives a slot back. Releasing an IP that holds none does nothing (the counter never goes negative).</summary>
    void Release(string ip);

    /// <summary>How many connections <paramref name="ip"/> holds now.</summary>
    int CountFor(string ip);
}
