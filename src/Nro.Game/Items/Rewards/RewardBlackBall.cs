using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.Items.Rewards;

// Quản lý thưởng chỉ số khi người chơi giữ ngọc rồng 1..7 sao trong Đại hội võ thuật; mỗi sao có hiệu lực 22 giờ và nhận lại tối đa mỗi giờ.
// Nguồn Java: models/Reward/RewardBlackBall.java (đồng hồ và lịch chiến được tiêm; Time8h tính khi đọc thay cho static time8h bị mọi người chơi ghi đè; gọi Service qua IItemHost; giữ lỗi Java luôn báo còn "0 phút").
public class RewardBlackBall
{
    // Chỉ số thưởng thứ nhất của ngọc một sao.
    public const int R1S_1 = 20;

    // Chỉ số thưởng thứ hai của ngọc một sao.
    public const int R1S_2 = 21;

    // Chỉ số thưởng thứ nhất của ngọc hai sao.
    public const int R2S_1 = 35;

    // Chỉ số thưởng thứ hai của ngọc hai sao.
    public const int R2S_2 = 20;

    // Chỉ số thưởng thứ nhất của ngọc ba sao.
    public const int R3S_1 = 35;

    // Chỉ số thưởng thứ hai của ngọc ba sao.
    public const int R3S_2 = 10;

    // Chỉ số thưởng thứ nhất của ngọc bốn sao.
    public const int R4S_1 = 10;

    // Chỉ số thưởng thứ hai của ngọc bốn sao.
    public const int R4S_2 = 35;

    // Chỉ số thưởng thứ nhất của ngọc năm sao.
    public const int R5S_1 = 35;

    // Chỉ số thưởng thứ hai của ngọc năm sao.
    public const int R5S_2 = 20;

    // Chỉ số thưởng thứ ba của ngọc năm sao.
    public const int R5S_3 = 20;

    // Chỉ số thưởng thứ nhất của ngọc sáu sao.
    public const int R6S_1 = 40;

    // Chỉ số thưởng thứ hai của ngọc sáu sao.
    public const int R6S_2 = 20;

    // Chỉ số thưởng thứ nhất của ngọc bảy sao.
    public const int R7S_1 = 14;

    // Chỉ số thưởng thứ hai của ngọc bảy sao.
    public const int R7S_2 = 15;

    // Khoảng chờ một giờ giữa hai lần nhận cùng sao.
    public const int TIME_WAIT = 3600000;

    // Thời lượng thưởng riêng tư: 22 giờ.
    private const int TIME_REWARD = 79200000;

    // Lịch Đại hội võ thuật mặc định: mở 20:00, đóng 21:00; giờ nhặt không dùng tại đây.
    public static readonly BlackBallWarSchedule DefaultSchedule = new(new TimeOnly(20, 0, 0), new TimeOnly(21, 0, 0), new TimeOnly(20, 0, 0));

    private static readonly VietnamClock DefaultClock = new();

    private readonly BlackBallWarSchedule schedule;

    // Gắn thưởng cho chủ sở hữu; dùng đồng hồ hệ thống, NullItemHost và lịch mặc định nếu không truyền.
    public RewardBlackBall(Player player, VietnamClock? clock = null, IItemHost? host = null, BlackBallWarSchedule? schedule = null)
    {
        Player = player;
        Clock = clock ?? DefaultClock;
        Host = host ?? NullItemHost.Instance;
        this.schedule = schedule ?? DefaultSchedule;
    }

    // Người chơi sở hữu phần thưởng.
    public Player Player { get; }

    // Nguồn thời gian để kiểm tra hiệu lực và chờ nhận.
    public VietnamClock Clock { get; set; }

    // Cổng gọi thông báo và đồng bộ chỉ số trò chơi.
    public IItemHost Host { get; set; }

    // Thời điểm epoch hết thưởng 22 giờ theo từng sao (chỉ số 0 là một sao); 0 nếu chưa từng nhận.
    public long[] TimeOutOfDateReward { get; set; } = new long[7];

    // Số lần thưởng được nhận hoặc lĩnh theo từng sao.
    public int[] QuantilyBlackBall { get; set; } = new int[7];

    // Thời điểm lĩnh thưởng gần nhất theo từng sao.
    public long[] LastTimeGetReward { get; set; } = new long[7];

    // Giờ mở chiến hôm nay theo ngày Việt Nam được coi là UTC, tức muộn hơn giờ mở thực bảy giờ; tính động nên không cũ và không chia sẻ mutable static.
    public long Time8h => Clock.GetStartTimeBlackBallWar(schedule);

    // Ghi nhận vừa thắng ngọc sao; nếu thưởng còn quá Time8h thì tăng đếm, luôn gia hạn 22 giờ và gửi lại chỉ số; star ngoài 1..7 sẽ lỗi chỉ số mảng.
    public void Reward(sbyte star)
    {
        if (TimeOutOfDateReward[star - 1] > Time8h)
        {
            QuantilyBlackBall[star - 1]++;
        }

        TimeOutOfDateReward[star - 1] = Clock.CurrentTimeMillis() + TIME_REWARD;
        Host.SendPoint(Player);
    }

    // Lĩnh ngọc còn hiệu lực ở vị trí select tính từ 0 trong danh sách sao đang có thưởng.
    public void GetRewardSelect(sbyte select)
    {
        int index = 0;
        for (int i = 0; i < TimeOutOfDateReward.Length; i++)
        {
            if (TimeOutOfDateReward[i] > Clock.CurrentTimeMillis())
            {
                index++;
                if (index == select + 1)
                {
                    GetReward(i + 1);
                    break;
                }
            }
        }
    }

    private void GetReward(int star)
    {
        if (TimeOutOfDateReward[star - 1] > Clock.CurrentTimeMillis()
            && Clock.CurrentTimeMillis() - LastTimeGetReward[star - 1] > TIME_WAIT)
        {
            LastTimeGetReward[star - 1] = Clock.CurrentTimeMillis();
            QuantilyBlackBall[star - 1]++;
            switch (star)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                    Host.SendNotification(Player, "Phần thưởng chỉ số " + star + " sao tự động nhận");
                    break;
            }

            Host.SendPoint(Player);
        }
        else
        {
            Host.SendNotification(
                Player,
                "Chưa thể nhận phần quà ngay lúc này, vui lòng đợi "
                + TimeUtil.DiffDate(LastTimeGetReward[star - 1], LastTimeGetReward[star - 1] + TIME_WAIT, TimeUtil.MINUTE) + " phút nữa");
        }
    }

    // Không có tài nguyên để giải phóng; giữ tham chiếu người chơi theo quy ước Player shell.
    public void Dispose()
    {
    }
}
