using Nro.Network.Interfaces;

namespace Nro.Network.Services;

/// <summary>
/// Counts connections per IP address. Port of the <c>CLIENTS</c> map in <c>ServerManager</c> (default limit <c>Manager.MAX_PER_IP = 3</c>).
/// Differences from Java: it is thread-safe (the Java <c>HashMap</c> was shared by the accept thread and every session thread with no locking),
/// and an address is forgotten when its last connection closes (the Java map kept one entry for every address that ever connected).
/// </summary>
public sealed class IpConnectionLimiter : IIpConnectionLimiter
{
    private readonly object _lock = new();
    private readonly Dictionary<string, int> _counts = new(StringComparer.Ordinal);
    private readonly int _maxPerIp;

    public IpConnectionLimiter(int maxPerIp)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxPerIp, 1);
        _maxPerIp = maxPerIp;
    }

    public bool TryAcquire(string ip)
    {
        ArgumentNullException.ThrowIfNull(ip);
        lock (_lock)
        {
            _counts.TryGetValue(ip, out int n);
            if (n >= _maxPerIp)
            {
                return false;
            }

            _counts[ip] = n + 1;
            return true;
        }
    }

    public void Release(string ip)
    {
        ArgumentNullException.ThrowIfNull(ip);
        lock (_lock)
        {
            if (!_counts.TryGetValue(ip, out int n))
            {
                return;
            }

            if (n <= 1)
            {
                _counts.Remove(ip);
            }
            else
            {
                _counts[ip] = n - 1;
            }
        }
    }

    public int CountFor(string ip)
    {
        ArgumentNullException.ThrowIfNull(ip);
        lock (_lock)
        {
            return _counts.GetValueOrDefault(ip);
        }
    }
}
