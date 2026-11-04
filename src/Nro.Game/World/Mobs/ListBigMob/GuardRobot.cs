using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;

namespace Nro.Game.World.Mobs.ListBigMob;

// Robot bảo vệ đánh người chơi trong tầm mỗi giây và báo tử vong cho bản đồ.
// Nguồn Java: nro/mob/ListBigMob/RobotBaoVe.java (đổi tên lớp thành GuardRobot).
public class GuardRobot : BigBoss
{
    // Khởi tạo robot từ trạng thái mob của bản đồ. (Java: RobotBaoVe)
    public GuardRobot(Mob mob)
        : base(mob)
    {
    }

    // Cập nhật boss nền rồi phát thông báo chết riêng của robot.
    public override void Update()
    {
        base.Update();
        if (IsDie())
        {
            Rt.World.SendBigBoss(Zone!, 6, 0, -1, -1);
        }
    }

    // Đánh mục tiêu có thể chọn với hoạt ảnh theo khoảng cách.
    public override void Attack()
    {
        Player? player = GetPlayerCanAttack();
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && Rt.CanDoWithTime(LastBigBossAttackTime, 1000))
        {
            if (player != null)
            {
                double dameMob = Point.GetDamageAttack();
                double dame = Rt.Players.Injured(player, null, dameMob, false, true);
                Action = 0;
                int dis = Distance(player);
                if (dis <= 100)
                {
                    Action = 0;
                }
                else if (dis > 100 && dis <= 200)
                {
                    Action = 1;
                }
                else if (dis > 200)
                {
                    Action = 2;
                }

                Location.X = unchecked((short)Rt.Players.GetX(player));
                Rt.World.SendBigBoss(Zone!, Action, 1, unchecked((int)player.Id), Rt.CrisGH(dame));
            }

            LastBigBossAttackTime = Rt.Now;
        }
    }
}
