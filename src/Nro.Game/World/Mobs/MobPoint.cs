namespace Nro.Game.World.Mobs;

// Lưu tọa độ mob với hai trục mà mob thực sự dùng.
// Nguồn Java: lớp Location của player (MobLocation được thêm vì mob chỉ cần x và y).
public sealed class MobLocation
{
    // Tọa độ ngang của mob.
    public int X { get; set; }

    // Tọa độ dọc của mob.
    public int Y { get; set; }
}

// Lưu HP và sát thương của mob.
// Nguồn Java: nro/mob/MobPoint.java.
public sealed class MobPoint
{
    // Gắn điểm chỉ số vào mob sở hữu.
    public MobPoint(Mob mob)
    {
        Mob = mob;
    }

    // Mob sở hữu chỉ số này.
    public Mob Mob { get; }

    // HP hiện tại.
    public long Hp { get; set; }

    // HP tối đa.
    public long MaxHp { get; set; }

    // Sát thương cố định; 0 nghĩa là suy ra từ HP tối đa.
    public long Damage { get; set; }

    // Trả HP tối đa như HP đầy.
    public long GetHpFull() => MaxHp;

    // Đặt HP tối đa.
    public void SetHpFull(long hp) => MaxHp = hp;

    // Trả HP hiện tại. (Java: gethp)
    public long Gethp() => Hp;

    // Đặt HP theo Java sethp(hp): kiểm tra HP HIỆN TẠI, không kiểm tra đối số; vì vậy đối số âm vẫn được lưu, chỉ mob vốn đã âm mới bị ép về 0.
    public void Sethp(long hp)
    {
        if (Hp < 0)
        {
            Hp = 0;
        }
        else
        {
            Hp = hp;
        }
    }

    // Tính sát thương: Damage ±1% nếu đã đặt, nếu không thì maxHp * nextInt(pDame - 1, pDame + 1) / 100 + nextInt(-level * 10, level * 10) bằng số học long, nhân trước rồi chia. (Java: getDameAttack)
    public long GetDamageAttack()
    {
        MapRuntime rt = Mob.Rt;
        return Damage != 0
            ? Damage + rt.Random.Crisnext(-(Damage / 100), Damage / 100)
            : (GetHpFull() * rt.NextInt(Mob.DamagePercent - 1, Mob.DamagePercent + 1) / 100) + rt.NextInt(-(Mob.Level * 10), Mob.Level * 10);
    }
}
