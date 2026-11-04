using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Items.Core;

// Theo dõi hạn mức rơi bốn vật phẩm sự kiện theo ngày lịch Việt Nam.
// Thực hiện thành viên; giữ nguyên điều kiện, thứ tự và số học của Java.
public class ItemEvent
{
    // Thực hiện thành viên; giữ nguyên điều kiện, thứ tự và số học của Java.
    public ItemEvent(Player player, VietnamClock? clock = null)
    {
        Player = player;
        Clock = clock ?? new VietnamClock();
    }

    // Java player. (Java: player).
    public Player Player { get; }

    // Thực hiện clock; giữ nguyên điều kiện, thứ tự và số học của Java.
    public VietnamClock Clock { get; set; }

    // Java lastTVGSTime. (Java: lastTVGSTime).
    public long LastTVGSTime { get; set; }

    // Java remainingTVGSCount. (Java: remainingTVGSCount).
    public int RemainingTVGSCount { get; set; }

    // Java lastKeoGiangSinhTime. (Java: lastKeoGiangSinhTime).
    public long LastKeoGiangSinhTime { get; set; }

    // Java remainingKeoGiangSinhCount. (Java: remainingKeoGiangSinhCount).
    public int RemainingKeoGiangSinhCount { get; set; }

    // Java lastHHTime. (Java: lastHHTime).
    public long LastHHTime { get; set; }

    // Java remainingHHCount. (Java: remainingHHCount).
    public int RemainingHHCount { get; set; }

    // Java lastBNTime. (Java: lastBNTime).
    public long LastBNTime { get; set; }

    // Java remainingBNCount. (Java: remainingBNCount).
    public int RemainingBNCount { get; set; }

    // Java canDropTatVoGiangSinh(maxCount). (Java: canDropTatVoGiangSinh(maxCount)).
    public bool CanDropTatVoGiangSinh(int maxCount)
    {
        long now = Clock.CurrentTimeMillis();
        if (Util.IsAfterDay(now, LastTVGSTime))
        {
            RemainingTVGSCount = maxCount;
            LastTVGSTime = Clock.CurrentTimeMillis();
            return true;
        }

        if (RemainingTVGSCount > 0)
        {
            RemainingTVGSCount--;
            return true;
        }

        return false;
    }

    // Java canDropKeoGiangSinh(maxCount). (Java: canDropKeoGiangSinh(maxCount)).
    public bool CanDropKeoGiangSinh(int maxCount)
    {
        long now = Clock.CurrentTimeMillis();
        if (Util.IsAfterDay(now, LastKeoGiangSinhTime))
        {
            RemainingKeoGiangSinhCount = maxCount;
            LastKeoGiangSinhTime = Clock.CurrentTimeMillis();
            return true;
        }

        if (RemainingKeoGiangSinhCount > 0)
        {
            RemainingKeoGiangSinhCount--;
            return true;
        }

        return false;
    }

    // Java canDropHoaHong(maxCount). (Java: canDropHoaHong(maxCount)).
    public bool CanDropHoaHong(int maxCount)
    {
        long now = Clock.CurrentTimeMillis();
        if (Util.IsAfterDay(now, LastHHTime))
        {
            RemainingHHCount = maxCount;
            LastHHTime = Clock.CurrentTimeMillis();
            return true;
        }

        if (RemainingHHCount > 0)
        {
            RemainingHHCount--;
            return true;
        }

        return false;
    }

    // Java canDropBinhNuoc(maxCount). (Java: canDropBinhNuoc(maxCount)).
    public bool CanDropBinhNuoc(int maxCount)
    {
        long now = Clock.CurrentTimeMillis();
        if (Util.IsAfterDay(now, LastBNTime))
        {
            RemainingBNCount = maxCount;
            LastBNTime = Clock.CurrentTimeMillis();
            return true;
        }

        if (RemainingBNCount > 0)
        {
            RemainingBNCount--;
            return true;
        }

        return false;
    }
}
