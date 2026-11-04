using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Effects.Helpers;

// Cung cấp phép kiểm thời gian dùng khắp hiệu ứng; C# nhận VietnamClock có thể giả lập để kiểm thử thay vì Java tự đọc System.currentTimeMillis, và đặt ở Nro.Game vì phần Util phụ thuộc đồng hồ chưa ở Nro.Core.
// Nguồn Java: Utils/Util.java (Util.canDoWithTime).
public static class EffectTime
{
    // Trả đúng khi now - lastTime > miniTimeTarget theo long; so sánh nghiêm ngặt nên đúng tại mốc còn false, sau 1 ms mới true, thời điểm tương lai false đến khi đồng hồ bắt kịp, 0 luôn true và phép trừ tràn như Java (Java: canDoWithTime).
    public static bool CanDoWithTime(this VietnamClock clock, long lastTime, long miniTimeTarget)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return unchecked(clock.CurrentTimeMillis() - lastTime) > miniTimeTarget;
    }
}
