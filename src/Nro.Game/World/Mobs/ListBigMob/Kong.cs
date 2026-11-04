namespace Nro.Game.World.Mobs.ListBigMob;

// Boss Kong đánh một mục tiêu bằng hành động 11 hoặc 12.
// Nguồn Java: nro/mob/ListBigMob/Kong.java (giữ nguyên nhịp gọi RunAttackRound mỗi 3 giây).
public class Kong : BigBoss
{
    // Khởi tạo boss từ trạng thái mob của bản đồ.
    public Kong(Mob mob)
        : base(mob)
    {
    }

    // Thực hiện một đợt đánh mục tiêu đơn khi hết hồi chiêu.
    public override void Attack()
    {
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && Rt.CanDoWithTime(LastBigBossAttackTime, 3000) && RunAttackRound(12, false))
        {
            LastBigBossAttackTime = Rt.Now;
        }
    }
}
