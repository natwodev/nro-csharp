namespace Nro.Game.World.Mobs.ListBigMob;

// Boss Godzilla đánh một mục tiêu bằng hành động 11 hoặc 12.
// Nguồn Java: nro/mob/ListBigMob/Godzila.java (đổi tên lớp thành Godzilla; vẫn gọi RunAttackRound mỗi 3 giây).
public class Godzilla : BigBoss
{
    // Khởi tạo boss từ trạng thái mob của bản đồ. (Java: Godzila)
    public Godzilla(Mob mob)
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
