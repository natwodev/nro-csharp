using Nro.Core.IO;
using Nro.Core.Logging;

namespace Nro.Game.World.Mobs.ListBigMob;

// Boss VegetaM tiến 10 px mỗi lượt về mục tiêu bằng gói 103 và tấn công ở tầm 50 hoặc 100.
// Nguồn Java: nro/mob/ListBigMob/VegetaM.java (không bản đồ Java nào tạo lớp này trong Map.initMob; giữ lại để đầy đủ).
public class VegetaM : BigBoss
{
    // Khởi tạo boss từ trạng thái mob của bản đồ.
    public VegetaM(Mob mob)
        : base(mob)
    {
    }

    // Di chuyển tới hoặc tấn công mục tiêu theo hành động đã chọn.
    public override void Attack()
    {
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && Rt.CanDoWithTime(LastBigBossAttackTime, 3000))
        {
            SnapshotList<Player> notBosses = Zone!.NotBosses;
            if (notBosses.IsEmpty)
            {
                return;
            }

            List<Player> players = [];
            Action = Rt.NextInt(11, 12);
            switch (Action)
            {
                case 11:
                    foreach (Player pl in notBosses)
                    {
                        if (Distance(pl) < 50)
                        {
                            players.Add(pl);
                            break;
                        }
                    }

                    break;
                case 12:
                    foreach (Player pl in notBosses)
                    {
                        if (Distance(pl) < 100)
                        {
                            players.Add(pl);
                            break;
                        }
                    }

                    break;
            }

            if (players.Count == 0)
            {
                int index = Rt.NextInt(0, notBosses.Count - 1);
                players.Add(notBosses[index]);
                Action = 10;
            }

            try
            {
                Message msg = new(102);
                msg.Writer().WriteByte(Action);
                msg.Writer().WriteByte(Id);
                switch (Action)
                {
                    case 10:
                        foreach (Player player in players)
                        {
                            MoveToward(player);
                        }

                        msg.Writer().WriteShort(Location.X);
                        msg.Writer().WriteShort(Location.Y);
                        Message moveMsg = new(103);
                        moveMsg.Writer().WriteByte(Id);
                        moveMsg.Writer().WriteShort(Location.X);
                        moveMsg.Writer().WriteShort(Location.Y);
                        moveMsg.Writer().WriteByte(Status);
                        Rt.World.SendMessAllPlayerInMap(Zone, moveMsg);

                        break;
                    case 11:
                    case 12:
                        msg.Writer().WriteByte(players.Count);
                        int dir = 0;
                        foreach (Player pl in players)
                        {
                            double damage = Rt.Players.Injured(pl, null, Point.GetDamageAttack(), false, true);
                            msg.Writer().WriteInt(unchecked((int)pl.Id));
                            msg.WriteCris(Rt.CrisGH(damage), Rt.ReadInt);
                            dir = Rt.Players.GetX(pl) < Location.X ? -1 : 1;
                        }

                        msg.Writer().WriteByte(dir);
                        Status = 3;
                        break;
                }

                Rt.World.SendMessAllPlayerInMap(Zone, msg);
                LastBigBossAttackTime = Rt.Now;
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(VegetaM), e);
            }
        }
    }

    private void MoveToward(Player player)
    {
        IMapPlayerGateway gateway = Rt.Players;
        int dx = gateway.GetX(player) - Location.X;
        int dy = gateway.GetY(player) - Location.Y;
        int step = 10;
        bool moved = false;
        if (Math.Abs(dx) > step)
        {
            Location.X += dx > 0 ? step : -step;
            moved = true;
        }
        else
        {
            Location.X = gateway.GetX(player);
        }

        if (Math.Abs(dy) > step)
        {
            Location.Y += dy > 0 ? step : -step;
            moved = true;
        }
        else
        {
            Location.Y = gateway.GetY(player);
        }

        Status = moved ? 1 : 0;
    }
}
