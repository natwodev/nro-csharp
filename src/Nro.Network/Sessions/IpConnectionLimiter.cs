namespace Nro.Network.Sessions;

// Đếm và giới hạn số kết nối theo địa chỉ IP.
// Nguồn Java: nro/server/ServerManager.java (khác Java HashMap không khóa: khóa truy cập và quên IP khi kết nối cuối đóng).
public sealed class IpConnectionLimiter : IIpConnectionLimiter
{
    private readonly object _lock = new();
    private readonly Dictionary<string, int> _counts = new(StringComparer.Ordinal);
    private readonly int _maxPerIp;

    // Tạo bộ giới hạn với số kết nối tối đa cho mỗi IP.
    public IpConnectionLimiter(int maxPerIp)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxPerIp, 1);
        _maxPerIp = maxPerIp;
    }

    // Thử chiếm một slot của IP, trả false khi đã đạt giới hạn.
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

    // Trả một slot của IP và xóa IP khi hết kết nối.
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

    // Trả số slot IP hiện đang chiếm.
    public int CountFor(string ip)
    {
        ArgumentNullException.ThrowIfNull(ip);
        lock (_lock)
        {
            return _counts.GetValueOrDefault(ip);
        }
    }
}
