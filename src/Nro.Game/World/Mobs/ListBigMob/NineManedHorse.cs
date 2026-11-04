namespace Nro.Game.World.Mobs.ListBigMob;

// Boss Ngựa chín lông đánh một mục tiêu bằng hành động 11 hoặc 12.
// Nguồn Java: nro/mob/ListBigMob/NguaChinLmao.java (đổi tên lớp thành NineManedHorse; vẫn gọi RunAttackRound mỗi 3 giây).
public class NineManedHorse : BigBoss
{
    // Khởi tạo boss từ trạng thái mob của bản đồ. (Java: NguaChinLmao)
    public NineManedHorse(Mob mob)
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
