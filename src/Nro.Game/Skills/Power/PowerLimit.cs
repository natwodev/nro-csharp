namespace Nro.Game.Skills.Power;

// Lưu một mốc giới hạn sức mạnh, HP, KI, sát thương, phòng thủ và chí mạng khi mở sức mạnh.
// Nguồn Java: nro/power/PowerLimit.java (Lombok được thay bằng constructor và thuộc tính C#).
public class PowerLimit
{
    public int Id { get; set; }

    public long Power { get; set; }

    public long Hp { get; set; }

    public long Mp { get; set; }

    public long Damage { get; set; }

    public int Defense { get; set; }

    public int Critical { get; set; }

    public PowerLimit(int id, long power, long hp, long mp, long damage, int defense, int critical)
    {
        Id = id;
        Power = power;
        Hp = hp;
        Mp = mp;
        Damage = damage;
        Defense = defense;
        Critical = critical;
    }
}
    // Mã mốc và các giới hạn chỉ số tương ứng.
    // Tạo một mốc giới hạn với đủ các chỉ số.
