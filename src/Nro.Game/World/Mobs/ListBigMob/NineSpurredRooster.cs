namespace Nro.Game.World.Mobs.ListBigMob;

// Boss Gà chín cựa đánh một mục tiêu bằng hành động 11 hoặc 12.
// Nguồn Java: nro/mob/ListBigMob/GaChinCua.java (đổi tên lớp thành NineSpurredRooster; vẫn gọi RunAttackRound mỗi 3 giây).
public class NineSpurredRooster : BigBoss
{
    // Khởi tạo boss từ trạng thái mob của bản đồ. (Java: GaChinCua)
    public NineSpurredRooster(Mob mob)
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
