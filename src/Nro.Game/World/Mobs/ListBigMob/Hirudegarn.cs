using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.IO;

namespace Nro.Game.World.Mobs.ListBigMob;

// Boss Hirudegarn có bốn trạng thái mạng: 0--2 chuyển pha sau 5 giây, pha 3 biến mất 10 phút; mỗi đòn chỉ mất 1% HP hiện tại, tối thiểu 1.
// Nguồn Java: nro/mob/ListBigMob/Hirudegarn.java (giữ nguyên 10 đống vàng khi chuyển pha và vị trí chờ (-1000, -1000) sau pha cuối).
public class Hirudegarn : BigBoss
{
    private int _errors;

    // Khởi tạo boss từ trạng thái mob của bản đồ.
    public Hirudegarn(Mob mob)
        : base(mob)
    {
    }

    // Giới hạn sát thương mỗi đòn bằng 1% HP hiện tại, không dùng sát thương truyền vào.
    public override void Injured(Player? plAtt, double damage, bool dieWhenHpFull)
    {
        damage = Point.Hp / 100 > 0 ? Point.Hp / 100 : 1;
        base.Injured(plAtt, damage, false);
    }

    // Điều khiển hồi sinh, chuyển pha và thả vàng của bốn trạng thái.
    public override void Update()
    {
        base.Update();
        if (IsDie() && Rt.Now - LastTimeDie > 600000 && LvMob == 3)
        {
            LvMob = 0;
            Action = 0;
            Location.X = Rt.NextInt(100, 900);
            Location.Y = 360;
            Point.Hp = Point.GetHpFull();
            Rt.World.SendBigBoss2(Zone!, Action, this);
            SendHpPacket();
        }
        else if (IsDie() && Rt.Now - LastTimeDie > 5000 && LvMob <= 2)
        {
            switch (LvMob)
            {
                case 0:
                    LvMob = 1;
                    Action = 6;
                    Point.Hp = Point.GetHpFull();
                    break;
                case 1:
                    LvMob = 2;
                    Action = 5;
                    Point.Hp = Point.GetHpFull();
                    break;
                case 2:
                    LvMob = 3;
                    Action = 9;
                    break;
            }

            int trai = 0;
            int phai = 1;
            int next = 0;
            for (int i = 0; i < 10; i++)
            {
                int x = next == 0 ? -15 * trai : 15 * phai;
                if (next == 0)
                {
                    trai++;
                }
                else
                {
                    phai++;
                }

                next = next == 0 ? 1 : 0;
                if (trai > 10)
                {
                    trai = 0;
                }

                if (phai > 10)
                {
                    phai = 1;
                }

                Rt.World.DropItemMap(Zone!, new ItemMap(Zone!, 190, 30000, Location.X + x, Zone!.Map.YPhysicInTop(Location.X, Location.Y - 24), -1));
            }

            Rt.World.SendBigBoss2(Zone!, Action, this);
            if (LvMob <= 2)
            {
                SendHpPacket();
            }
            else
            {
                Location.X = -1000;
                Location.Y = -1000;
            }
        }
    }

    private void SendHpPacket()
    {
        try
        {
            Message msg = new(-9);
            msg.Writer().WriteByte(Id);
            msg.WriteCris(Rt.CrisGH(Point.Gethp()), Rt.ReadInt);
            msg.Writer().WriteInt(1);
            Rt.World.SendMessAllPlayerInMap(Zone!, msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    // Chọn hành động và gửi gói tấn công phù hợp với pha hiện tại.
    public override void Attack()
    {
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && Rt.CanDoWithTime(LastBigBossAttackTime, 3000))
        {
            try
            {
                int[] idAction = [1, 2, 3, 7];
                if (LvMob >= 2)
                {
                    idAction = [1, 2];
                }

                Action = Action == 7 ? 0 : idAction[Rt.NextInt(0, idAction.Length - 1)];
                if (Zone!.Players.IsEmpty)
                {
                    return;
                }

                int index = Rt.NextInt(0, Zone.Players.Count - 1);
                Player player = Zone.Players[index];
                if (player == null || Rt.Players.IsDie(player))
                {
                    return;
                }

                if (Action == 1)
                {
                    Location.X = unchecked((short)Rt.Players.GetX(player));
                    Rt.World.SendBigBoss2(Zone, 8, this);
                }

                Message msg = new(101);
                msg.Writer().WriteByte(Action);
                if (Action >= 0 && Action <= 4)
                {
                    switch (Action)
                    {
                        case 1:
                            msg.Writer().WriteByte(1);
                            double dame = Rt.Players.Injured(player, null, Point.GetDamageAttack(), false, true);
                            msg.Writer().WriteInt(unchecked((int)player.Id));
                            msg.WriteCris(Rt.CrisGH(dame), Rt.ReadInt);
                            break;
                        case 3:
                            Location.X = unchecked((short)Rt.Players.GetX(player));
                            msg.Writer().WriteShort(Location.X);
                            msg.Writer().WriteShort(Location.Y);
                            break;
                        default:
                            Player[] all = Zone.Players.ToArray();
                            msg.Writer().WriteByte(all.Length);
                            foreach (Player pl in all)
                            {
                                double d = Rt.Players.Injured(pl, null, Point.GetDamageAttack(), false, true);
                                msg.Writer().WriteInt(unchecked((int)pl.Id));
                                msg.WriteCris(Rt.CrisGH(d), Rt.ReadInt);
                            }

                            break;
                    }
                }
                else if (Action == 6 || Action == 8)
                {
                    Location.X = unchecked((short)Rt.Players.GetX(player));
                    msg.Writer().WriteShort(Location.X);
                    msg.Writer().WriteShort(Location.Y);
                }

                Rt.World.SendMessAllPlayerInMap(Zone, msg);
                LastBigBossAttackTime = Rt.Now;
            }
            catch (Exception e)
            {
                if (_errors < 5)
                {
                    _errors++;
                    Logger.LogException(typeof(Mob), e);
                }
            }
        }
    }
}
