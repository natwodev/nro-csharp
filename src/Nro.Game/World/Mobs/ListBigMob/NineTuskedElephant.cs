namespace Nro.Game.World.Mobs.ListBigMob;

// Boss Voi chín ngà đánh một mục tiêu bằng hành động 11 hoặc 12.
// Nguồn Java: nro/mob/ListBigMob/VoiChinNga.java (đổi tên lớp thành NineTuskedElephant; vẫn gọi RunAttackRound mỗi 3 giây).
public class NineTuskedElephant : BigBoss
{
    // Khởi tạo boss từ trạng thái mob của bản đồ. (Java: VoiChinNga)
    public NineTuskedElephant(Mob mob)
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
