namespace Nro.Game.World.Mobs.ListBigMob;

// Boss Gấu tướng cướp đánh vùng bằng các hành động 11--15; hành động 15 trúng mọi mục tiêu cách dưới 200.
// Nguồn Java: nro/mob/ListBigMob/GauTuongCuop.java (đổi tên lớp thành BearThief; vẫn gọi RunAttackRound mỗi 3 giây).
public class BearThief : BigBoss
{
    // Khởi tạo boss từ trạng thái mob của bản đồ. (Java: GauTuongCuop)
    public BearThief(Mob mob)
        : base(mob)
    {
    }

    // Thực hiện một đợt đánh diện rộng khi hết hồi chiêu.
    public override void Attack()
    {
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && Rt.CanDoWithTime(LastBigBossAttackTime, 3000) && RunAttackRound(15, false))
        {
            LastBigBossAttackTime = Rt.Now;
        }
    }
}
