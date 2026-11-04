using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;
using Nro.Core.IO;

namespace Nro.Game.World.Mobs;

// Lớp nền cho boss bản đồ lớn, gom các vòng tấn công trùng nhau nhưng giữ thứ tự rút ngẫu nhiên của Java.
// Nguồn Java: nro/mob/BigBoss.java (giữ nguyên việc mọi boss đều gọi PianoMove và, khi có sự kiện Hùng Vương, gọi thêm ba lượt thú nên có thể đánh nhiều lần trong một tick; các bộ đếm hồi chiêu vẫn tách riêng).
public class BigBoss : Mob
{
    private long _lastPianoAttackTime;
    private long _lastNineTuskedElephantAttackTime;
    private long _lastNineSpurredRoosterAttackTime;
    private long _lastNineManedHorseAttackTime;

    // Sao chép dữ liệu mob và gắn lại zone cho boss.
    public BigBoss(Mob mob)
        : base(mob)
    {
        Zone = mob.Zone;
    }

    // Mã hành động cuối gửi cho client: 10 di chuyển, 11--20 tấn công, 21 bay, 22--23 chết.
    public int Action { get; set; }

    // Thời điểm boss lớn tấn công gần nhất để áp hồi chiêu.
    public long LastBigBossAttackTime { get; set; }

    // Thời điểm boss lớn chết gần nhất để tính hồi sinh sự kiện.
    public long LastBigBossDeathTime { get; set; }

    // Cập nhật sát thương sự kiện, hồi sinh, hiệu ứng và các vòng tấn công của boss.
    public override void Update()
    {
        if (Zone!.IsGoldenFriezaAlive && Rt.Clock.Is21H() && !IsDie())
        {
            StartDie();
            return;
        }

        if (Rt.Rules.IsMapHungVuongEvent(Zone.Map.MapId))
        {
            switch (TempId)
            {
                case ConstMob.KONG:
                case ConstMob.GODZILLA:
                case ConstMob.NINE_TUSKED_ELEPHANT:
                case ConstMob.NINE_MANED_HORSE:
                case ConstMob.NINE_SPURRED_ROOSTER:
                    Point.Damage = 100_000;
                    break;
            }
        }
        else
        {
            switch (TempId)
            {
                case ConstMob.NINE_TUSKED_ELEPHANT:
                case ConstMob.NINE_MANED_HORSE:
                case ConstMob.NINE_SPURRED_ROOSTER:
                    Point.Damage = 100;
                    break;
            }
        }

        if (IsDie() && IsBigBossHungVuongEvent() && Rt.CanDoWithTime(LastBigBossDeathTime, 1_800_000))
        {
            Respawn();
            SendMobRespawn();
            LastBigBossDeathTime = Rt.Now;
        }

        EffectSkill.Update();
        Attack();
        PianoMove();
        if (Rt.Flags.HungVuong)
        {
            NineSpurredRoosterMove();
            NineTuskedElephantMove();
            NineManedHorseMove();
        }
    }

    // Chạy lượt tấn công riêng của Ngựa chín lông khi hết hồi chiêu.
    public void NineManedHorseMove()
    {
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && Rt.CanDoWithTime(_lastNineManedHorseAttackTime, 3000) && RunAttackRound(12, true))
        {
            _lastNineManedHorseAttackTime = Rt.Now;
        }
    }

    // Chạy lượt tấn công riêng của Voi chín ngà khi hết hồi chiêu.
    public void NineTuskedElephantMove()
    {
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && Rt.CanDoWithTime(_lastNineTuskedElephantAttackTime, 3000) && RunAttackRound(12, true))
        {
            _lastNineTuskedElephantAttackTime = Rt.Now;
        }
    }

    // Chạy lượt tấn công riêng của Gà chín cựa khi hết hồi chiêu.
    public void NineSpurredRoosterMove()
    {
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && Rt.CanDoWithTime(_lastNineSpurredRoosterAttackTime, 3000) && RunAttackRound(12, true))
        {
            _lastNineSpurredRoosterAttackTime = Rt.Now;
        }
    }

    // Chạy lượt tấn công riêng kiểu Piano khi hết hồi chiêu.
    public void PianoMove()
    {
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && Rt.CanDoWithTime(_lastPianoAttackTime, 2000) && RunAttackRound(14, true))
        {
            _lastPianoAttackTime = Rt.Now;
        }
    }

    // Chọn hành động 11..actionMax, chọn mục tiêu trong tầm hoặc chuyển thành đi tới ngẫu nhiên (10), gây sát thương và gửi gói 102; trả false khi zone trống hoặc có lỗi. actionMax là 12 cho thú đơn mục tiêu, 14 Piano, 15 Gấu tướng cướp; dieMeansMove biến mục tiêu chết thành đi tới, còn override Attack không làm vậy.
    protected bool RunAttackRound(int actionMax, bool dieMeansMove)
    {
        SnapshotList<Player> notBosses = Zone!.NotBosses;
        if (notBosses.IsEmpty)
        {
            return false;
        }

        IMapPlayerGateway gateway = Rt.Players;
        List<Player> players = [];
        Action = Rt.NextInt(11, actionMax);
        switch (Action)
        {
            case 11:
                foreach (Player pl in notBosses)
                {
                    if (Distance(pl) < 50)
                    {
                        players.Add(pl);
                        if (dieMeansMove && gateway.IsDie(pl))
                        {
                            Action = 10;
                        }

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
                        if (dieMeansMove && gateway.IsDie(pl))
                        {
                            Action = 10;
                        }

                        break;
                    }
                }

                break;
            case 13:
            case 14:
                foreach (Player pl in notBosses)
                {
                    if (Distance(pl) < 150)
                    {
                        players.Add(pl);
                        if (dieMeansMove && gateway.IsDie(pl))
                        {
                            Action = 10;
                        }
                    }
                }

                break;
            case 15:
                foreach (Player pl in notBosses)
                {
                    if (Distance(pl) < 200)
                    {
                        players.Add(pl);
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
                case 21:
                    foreach (Player player in players)
                    {
                        Location.X = gateway.GetX(player) + Rt.NextInt(-10, 10);
                        Location.Y = gateway.GetY(player);
                    }

                    msg.Writer().WriteShort(Location.X);
                    msg.Writer().WriteShort(Location.Y);
                    break;
                case >= 11 and <= 20:
                    msg.Writer().WriteByte(players.Count);
                    int dir = 0;
                    foreach (Player pl in players)
                    {
                        double damage = gateway.Injured(pl, null, Point.GetDamageAttack(), false, true);
                        msg.Writer().WriteInt(unchecked((int)pl.Id));
                        msg.WriteCris(Rt.CrisGH(damage), Rt.ReadInt);
                        dir = gateway.GetX(pl) < Location.X ? -1 : 1;
                    }

                    msg.Writer().WriteByte(dir);
                    break;
            }

            Rt.World.SendMessAllPlayerInMap(Zone, msg);
            return true;
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(BigBoss), e);
            return false;
        }
    }

    // Tính khoảng cách giữa người chơi và vị trí boss.
    protected int Distance(Player pl) => Util.GetDistance(Rt.Players.GetX(pl), Rt.Players.GetY(pl), Location.X, Location.Y);
}
