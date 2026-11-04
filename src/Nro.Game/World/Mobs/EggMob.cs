using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.IO;

namespace Nro.Game.World.Mobs;

// Mob Đệ trứng do kỹ năng Trứng triệu hồi, được giữ tại Player.EggMob thay vì danh sách mob của zone.
// Nguồn Java: nro/mob/DeTrung.java (đổi tên lớp thành EggMob; các bảng SkillUtil cần thiết được chép cục bộ vì SkillUtil chưa chuyển; synchronized Java dùng quy tắc luồng zone D1 và catch rỗng được thay bằng ghi log).
public sealed class EggMob : Mob
{
    private static readonly int[] SummonedMobTemplates = [8, 11, 32, 25, 43, 49, 50];
    private static readonly long[] SummonedMobHpPercents = [30, 40, 50, 60, 70, 80, 90];

    private readonly long _lastTimeSpawn;
    private readonly int _timeSurvive;
    private readonly bool _fullPikkoroSet;
    private Player? _player;

    // Tạo Đệ trứng theo cấp kỹ năng và chỉ số của chủ nhân. (Java: DeTrung)
    public EggMob(Player player, MapRuntime runtime)
        : base(runtime)
    {
        _player = player;
        Id = unchecked((int)player.Id);
        EggMobSource source = runtime.Players.GetEggMobSource(player);
        int level = source.SkillLevel;
        TempId = SummonedMobTemplates[level - 1];
        Point.MaxHp = SummonedMobHp(runtime.CrisGH(source.HpMax), level);
        Point.Damage = SummonedMobHp(runtime.CrisGH(source.DamageAttack), level);
        _fullPikkoroSet = source.PikkoroDaimao == 5;
        if (_fullPikkoroSet)
        {
            Point.Damage *= 2;
        }

        Point.Hp = Point.MaxHp;
        Zone = player.Zone;
        _lastTimeSpawn = runtime.Now;

        // Công thức Java SkillUtil.getTimeSurviveMobMe(level): getTimeMonkey(level) * 2 = (level + 5) * 10000 * 2.
        _timeSurvive = (level + 5) * 10000 * 2;
        Spawn();
    }

    private static long SummonedMobHp(long hpMaxPlayer, int level) => hpMaxPlayer * SummonedMobHpPercents[level - 1] / 100;

    // Hủy Đệ trứng khi hết thời gian tồn tại, trừ khi đủ bộ Pikkoro.
    public override void Update()
    {
        if (_player != null && Rt.CanDoWithTime(_lastTimeSpawn, _timeSurvive) && !_fullPikkoroSet)
        {
            SummonedMobDie();
            Dispose();
        }
    }

    // Tấn công người chơi hoặc mob và cộng tiềm năng từ mob cho chủ nhân.
    public void Attack(Player? pl, Mob? mob, bool miss)
    {
        if (_player == null)
        {
            return;
        }

        try
        {
            if (pl != null)
            {
                long dame = !miss ? Point.Damage : 0;
                MobPlayerView before = Rt.Players.GetMobPlayerView(pl);
                if ((before.Hp > dame && before.Hp > before.HpMax * 0.05) || _fullPikkoroSet)
                {
                    double dameHit = Rt.Players.Injured(pl, _player, dame, true, true);
                    Message msg = new(-95);
                    msg.Writer().WriteByte(2);
                    msg.Writer().WriteInt(Id);
                    msg.Writer().WriteInt(unchecked((int)pl.Id));
                    msg.WriteCris(Rt.CrisGH(dameHit), Rt.ReadInt);
                    msg.WriteCris(Rt.CrisGH(Rt.Players.GetMobPlayerView(pl).Hp), Rt.ReadInt);
                    Rt.World.SendMessAllPlayerInMap(_player, msg);
                }
            }

            if (mob != null && mob.Point.Gethp() > Point.Damage)
            {
                long tnsm = mob.GetPotentialForPlayer(_player, Point.Damage);
                Message msg = new(-95);
                msg.Writer().WriteByte(3);
                msg.Writer().WriteInt(Id);
                msg.Writer().WriteInt(mob.Id);
                mob.Point.Sethp(mob.Point.Gethp() - Point.Damage);
                msg.WriteCris(Rt.CrisGH(mob.Point.Gethp()), Rt.ReadInt);
                msg.WriteCris(Rt.CrisGH(Point.Damage), Rt.ReadInt);
                Rt.World.SendMessAllPlayerInMap(_player, msg);
                Rt.World.AddSMTN(_player, 2, tnsm, true);
            }
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(EggMob), e);
        }
    }

    // Nhận tối đa 5% HP tối đa mỗi đòn; chết thì báo hủy và tách khỏi chủ.
    public override void Injured(Player? plAtt, double damage, bool dieWhenHpFull)
    {
        try
        {
            if (damage > Point.MaxHp / 20)
            {
                damage = Point.MaxHp / 20;
            }

            Point.Hp = JavaNumbers.ToLong(Point.Hp - damage);
            Message msg = new(-95);
            msg.Writer().WriteByte(5);
            msg.Writer().WriteInt(unchecked((int)plAtt!.Id));
            msg.Writer().WriteByte(Rt.Players.GetMobPlayerView(plAtt).SelectedSkillId);
            msg.Writer().WriteInt(Id);
            msg.WriteCris(Rt.CrisGH(damage), Rt.ReadInt);
            msg.WriteCris(Rt.CrisGH(Point.Hp), Rt.ReadInt);
            Rt.World.SendMessAllPlayerInMap(_player!, msg);
            if (IsDie())
            {
                SummonedMobDie();
                Dispose();
            }
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(EggMob), e);
        }
    }

    // Gửi gói báo Đệ trứng xuất hiện trong zone.
    public void Spawn()
    {
        try
        {
            Message msg = new(-95);
            msg.Writer().WriteByte(0);
            msg.Writer().WriteInt(unchecked((int)_player!.Id));
            msg.Writer().WriteShort(TempId);
            msg.WriteCris(Rt.CrisGH(Point.Hp), Rt.ReadInt);
            Rt.World.SendMessAllPlayerInMap(Zone!, msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(EggMob), e);
        }
    }

    // Gỡ khỏi zone cũ rồi chuyển sang zone mới khi zone hợp lệ.
    public void GoToMap(Zone? zone)
    {
        if (zone != null)
        {
            RemoveMobInMap();
            Zone = zone;
        }
    }

    private void RemoveMobInMap()
    {
        SendOwnerPacket(7);
    }

    // Gửi gói báo Đệ trứng đã chết cho zone.
    public void SummonedMobDie()
    {
        SendOwnerPacket(6);
    }

    private void SendOwnerPacket(int type)
    {
        try
        {
            Message msg = new(-95);
            msg.Writer().WriteByte(type);
            msg.Writer().WriteInt(unchecked((int)_player!.Id));
            Rt.World.SendMessAllPlayerInMap(Zone!, msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(EggMob), e);
        }
    }

    // Tách Đệ trứng khỏi chủ sở hữu. (Java: dispose)
    public void Dispose()
    {
        if (_player != null)
        {
            _player.EggMob = null;
        }

        _player = null;
    }
}
