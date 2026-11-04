using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.IO;

namespace Nro.Game.World.Mobs.ListBigMob;

// Vua bạch tuộc chỉ đánh người chơi có tọa độ X từ 441 đến 949.
// Nguồn Java: nro/mob/ListBigMob/VuaBachTuoc.java (đổi tên lớp thành KingOctopus; giữ nguyên việc khoảng cách từ 500 vẫn dùng hành động 3).
public class KingOctopus : BigBoss
{
    // Khởi tạo boss từ trạng thái mob của bản đồ. (Java: VuaBachTuoc)
    public KingOctopus(Mob mob)
        : base(mob)
    {
    }

    // Cập nhật boss nền rồi phát thông báo chết riêng của vua bạch tuộc.
    public override void Update()
    {
        base.Update();
        if (IsDie())
        {
            Rt.World.SendBigBoss(Zone!, 7, 0, -1, -1);
        }
    }

    // Đánh mục tiêu hợp lệ trong hành lang tọa độ của boss.
    public override void Attack()
    {
        Player? player = GetPlayerCanAttack();
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && Rt.CanDoWithTime(LastBigBossAttackTime, 1000))
        {
            int px = player != null ? Rt.Players.GetX(player) : 0;
            if (player != null && px > 440 && px < 950)
            {
                double mobDamage = Point.GetDamageAttack();
                double damage = Rt.Players.Injured(player, null, mobDamage, false, true);
                Action = 3;
                int dis = Distance(player);
                if (dis <= 100)
                {
                    Action = 3;
                }
                else if (dis > 100 && dis <= 200)
                {
                    Action = 4;
                }
                else if (dis < 500)
                {
                    Action = 5;
                    Location.X = unchecked((short)px);
                }

                try
                {
                    Message msg = new(102);
                    msg.Writer().WriteByte(Action);
                    if (Action == 3 || Action == 4)
                    {
                        msg.Writer().WriteByte(1);
                        msg.Writer().WriteInt(unchecked((int)player.Id));
                        msg.WriteCris(Rt.CrisGH(damage), Rt.ReadInt);
                    }
                    else
                    {
                        msg.Writer().WriteShort(Location.X);
                    }

                    Rt.World.SendMessAllPlayerInMap(Zone!, msg);
                }
                catch (Exception e)
                {
                    Logger.LogException(typeof(KingOctopus), e);
                }
            }

            LastBigBossAttackTime = Rt.Now;
        }
    }
}
