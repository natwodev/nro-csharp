using Nro.Core.Consts;
using Nro.Game.Players.Ports;

namespace Nro.Game.Players;

// Nhắc quà hằng ngày và gửi HP/MP qua cổng client, không dùng Service singleton.
// Nguồn Java: nro/player/Player.java (dòng 2093–2141).
public partial class Player
{
    // Mã text nhận bùa miễn phí trên client (Java: ItemTime.TEXT_NHAN_BUA_MIEN_PHI).
    private const sbyte FreeCharmTextId = 5;

    // Mã text điểm danh hằng ngày trên client (Java: ItemTime.TEXT_DIEM_DANH_HANG_NGAY).
    private const sbyte DailyCheckInTextId = 6;

    // Gửi nhắc quà hằng ngày khi qua các khoảng thời gian Java; thời gian do caller cấp để không đọc đồng hồ trực tiếp (Java: sendTextTimeDaiLyGift).
    public void SendDailyGiftReminders(long nowMs, IPlayerClientNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(notifier);

        if (CanDoWithTime(nowMs, LastTimeSendTextTime, 300_000))
        {
            if (notifier.CanReceiveDailyGift(this, ConstDailyGift.FREE_CHARM))
            {
                notifier.SendTimedText(this, FreeCharmTextId, "Nhận ngẫu nhiên bùa 1h mỗi ngày tại Bà Hạt Mít ở vách núi", 30);
            }

            LastTimeSendTextTime = nowMs;
        }

        if (CanDoWithTime(nowMs, LastTimeSendTextTime_2, 60_000))
        {
            if (notifier.CanReceiveDailyGift(this, ConstDailyGift.DAILY_CHECK_IN))
            {
                string? message = Gender switch
                {
                    ConstPlayer.EARTHLING => "Điểm danh mỗi ngày tại Ông Gôhan ở Nhà Gôhan",
                    ConstPlayer.NAMEKIAN => "Điểm danh mỗi ngày tại Ông Moori ở Nhà Moori",
                    ConstPlayer.SAIYAN => "Điểm danh mỗi ngày tại Ông Paragus ở Nhà Broly",
                    _ => null,
                };
                if (message is not null)
                {
                    notifier.SendTimedText(this, DailyCheckInTextId, message, 30);
                }
            }

            LastTimeSendTextTime_2 = nowMs;
        }
    }

    // Gửi HP và MP hiện tại cho người chơi thật; adapter chịu trách nhiệm tạo/dọn hai Message Java (Java: sendInfoHPMP).
    public void SendInfoHpMp(IPlayerClientNotifier notifier)
    {
        ArgumentNullException.ThrowIfNull(notifier);
        if (!IsPl())
        {
            return;
        }

        try
        {
            notifier.SendCurrentHp(this, NPoint.Hp);
            notifier.SendCurrentMp(this, NPoint.Mp);
        }
        catch (Exception exception)
        {
            notifier.ReportNotificationFailure(this, exception);
        }
    }
}
