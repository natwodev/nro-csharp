using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.World.Maps;

// Điều phối tick D1: một vòng lặp sở hữu các zone của map hoặc một zone và là luồng duy nhất thay đổi chúng.
// Nguồn Java: nro/map/Map.java, Map.run() (thay while(true) và Functions.sleep(max(1000 - elapsed, 10)) bằng hủy qua token và TimeProvider tiêm được).
// Mỗi lượt duyệt zone theo chỉ số, chạy lệnh hàng đợi trước Zone.Update; lỗi một zone được log nhưng các zone sau vẫn chạy, còn lỗi thực thể đã được Zone.Update chứa.
public sealed class MapTickLoop
{
    // Chu kỳ Java: một lượt mỗi giây.
    public static readonly TimeSpan DefaultPeriod = TimeSpan.FromMilliseconds(1000);

    // Cận dưới thời gian nghỉ Java: 10 mili giây.
    public static readonly TimeSpan DefaultMinimumDelay = TimeSpan.FromMilliseconds(10);

    private readonly MapRuntime _runtime;
    private readonly Func<IReadOnlyList<Zone>> _zones;
    private readonly string _name;
    private long _turns;

    // Tạo vòng lặp cho tất cả zone của map theo cấu trúc Java.
    public MapTickLoop(Map map, TimeSpan? period = null, TimeSpan? minimumDelay = null)
        : this(map.Runtime, () => map.Zones, "map " + map.MapId, period, minimumDelay)
    {
    }

    // Tạo vòng lặp chuyên dụng cho một zone.
    public MapTickLoop(Zone zone, TimeSpan? period = null, TimeSpan? minimumDelay = null)
        : this(zone.Runtime, () => [zone], "zone " + zone.ZoneId + " of map " + zone.Map.MapId, period, minimumDelay)
    {
    }

    private MapTickLoop(MapRuntime runtime, Func<IReadOnlyList<Zone>> zones, string name, TimeSpan? period, TimeSpan? minimumDelay)
    {
        _runtime = runtime;
        _zones = zones;
        _name = name;
        Period = period ?? DefaultPeriod;
        MinimumDelay = minimumDelay ?? DefaultMinimumDelay;
        if (Period <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(period), "The period must be positive.");
        }

        if (MinimumDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(minimumDelay), "The minimum delay cannot be negative.");
        }
    }

    // Chu kỳ tick được áp dụng.
    public TimeSpan Period { get; }

    // Độ trễ nhỏ nhất giữa hai lượt.
    public TimeSpan MinimumDelay { get; }

    // Số lượt đã hoàn thành.
    public long Turns => Interlocked.Read(ref _turns);

    // Chạy ngay một lượt theo thứ tự zone và trả số zone lỗi đã log; không gọi đồng thời hai luồng cho cùng zone vì zone sẽ từ chối tick trùng.
    public int TickOnce()
    {
        int failed = 0;
        foreach (Zone zone in _zones())
        {
            try
            {
                zone.Tick();
            }
            catch (Exception e)
            {
                failed++;
                Logger.LogException(typeof(MapTickLoop), e, "Lỗi update zone");
            }
        }

        Interlocked.Increment(ref _turns);
        return failed;
    }

    // Chạy đến khi hủy, nghỉ max(period - elapsed, minimumDelay) giữa các lượt và kết thúc bình thường khi token bị hủy.
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            long start = _runtime.Now;
            try
            {
                TickOnce();
            }
            catch (Exception e)
            {
                // TickOnce đã chứa lỗi zone; khối này chỉ bảo vệ chính vòng lặp (Java: "Lỗi update map").
                Logger.LogException(typeof(MapTickLoop), e, "Lỗi update " + _name);
            }

            long elapsedMs = _runtime.Now - start;
            TimeSpan delay = Period - TimeSpan.FromMilliseconds(elapsedMs);
            if (delay < MinimumDelay)
            {
                delay = MinimumDelay;
            }

            try
            {
                await Task.Delay(delay, _runtime.Time, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
