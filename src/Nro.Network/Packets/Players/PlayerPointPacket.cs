namespace Nro.Network.Packets.Players;

// Dữ liệu chỉ số nhân vật sau tính lại. (Java: Service.point)
// Nguồn Java: nro/services/Service.java (dòng 798–831 trong bảng I-02; bản SRC hiện có 425–459).
public sealed record PlayerPointPacket(
    long BaseHp, long BaseMp, long BaseDamage, long HpMax, long MpMax, long Hp, long Mp,
    sbyte Speed, long Damage, int Defense, int CriticalChance, long Potential,
    int BaseDefense, int BaseCriticalChance);
