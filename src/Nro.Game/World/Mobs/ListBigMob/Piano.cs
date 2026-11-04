namespace Nro.Game.World.Mobs.ListBigMob;

// Boss Piano đánh vùng bằng hành động 11--14; hành động 13 và 14 trúng mọi mục tiêu cách dưới 150.
// Nguồn Java: nro/mob/ListBigMob/Piano.java (giữ nguyên nhịp gọi RunAttackRound mỗi 3 giây).
public class Piano : BigBoss
{
    // Khởi tạo boss từ trạng thái mob của bản đồ.
    public Piano(Mob mob)
        : base(mob)
    {
    }

    // Thực hiện một đợt đánh diện rộng khi hết hồi chiêu.
    public override void Attack()
    {
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && Rt.CanDoWithTime(LastBigBossAttackTime, 3000) && RunAttackRound(14, false))
        {
            LastBigBossAttackTime = Rt.Now;
        }
    }
}
