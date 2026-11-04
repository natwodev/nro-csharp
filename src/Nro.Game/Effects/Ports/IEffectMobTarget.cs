namespace Nro.Game.Effects.Ports;

// Cung cấp phần dữ liệu và thao tác của quái mà hiệu ứng người chơi cần, để hiệu ứng biên dịch và kiểm thử khi Mob chưa hoàn thành.
// Nguồn Java: nro/mob/Mob.java, MobPoint.java và MobEffectSkill.java (cổng phụ trợ; Mob thật hoặc adapter sẽ triển khai ở Wave 3).
public interface IEffectMobTarget
{
    // Id quái, được ghi cho client dưới dạng một byte (Java: mob.id).
    int Id { get; }

    // HP hiện tại của quái (Java: mob.point.gethp).
    long Hp { get; }

    // HP đầy của quái (Java: mob.point.getHpFull).
    long HpFull { get; }

    // Tọa độ X của quái (Java: mob.location.x).
    int X { get; }

    // Tọa độ Y của quái (Java: mob.location.y).
    int Y { get; }

    // Làm quái bị thương bởi người tấn công, với cờ chết khi HP đầy (Java: mob.injured).
    void Injured(Player? attacker, double damage, bool dieWhenHpFull);

    // Gỡ trạng thái trói của quái (Java: mob.effectSkill.removeAnTroi) (Java: removeAnTroi).
    void RemoveBind();

    // Đặt trạng thái bị nhốt bình cùng người tấn công, thời điểm bắt đầu và thời lượng (Java: mob.effectSkill.setBinh) (Java: setBinh).
    void SetBottle(Player attacker, long bottleStartTime, int bottleDuration);

    // Đặt trạng thái sôcôla với mốc thời gian và thời lượng (Java: mob.effectSkill.setSocola).
    void SetChocolate(long lastChocolateTime, int chocolateDuration);
}
