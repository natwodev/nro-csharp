using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;
using Nro.Core.IO;

namespace Nro.Game.World.Mobs;

// Mô hình một quái của zone: HP, tấn công, chết, hồi sinh và vật phẩm rơi; chỉ tick zone gọi Update và, do gói tin được xếp vào zone theo D1, cũng gọi Injured.
// Nguồn Java: nro/mob/Mob.java (thời gian, ngẫu nhiên, cờ và dịch vụ ngoài đi qua MapRuntime, không dùng gI(), System.currentTimeMillis(), Math.random() hay Mob.gI(); dữ kiện Player chưa có lấy từ MobPlayerView/MobLootPlayerView; bỏ log chẩn đoán [SLOW]; lvMob() đổi thành ComputeLvMob vì C# không cho trường và hàm trùng tên; không chuyển dropItemKichHoat/createItemKichHoat/randomizeOption/powerDropActiveSet vì lời gọi duy nhất đã bị chú thích; danh sách rơi được gắn trước khi phát -12 để tránh race Java; copy constructor cố ý không sao Percent_gold nên bản sao có Percent_gold = 0 như Java).
public class Mob
{
    private const int GoldMax = 100_000_000;

    private long _maxPotential;
    private long _lastTimeAttackPlayer;
    private long _timeAttack = 2000;

    // Tạo mob với runtime, chỉ số, hiệu ứng và vị trí rỗng.
    public Mob(MapRuntime runtime)
    {
        Rt = runtime;
        Point = new MobPoint(this);
        EffectSkill = new MobEffectSkill(this);
        Location = new MobLocation();
        LastRegenTime = runtime.Now;
        LastTimeSendEffect = runtime.Now;
    }

    // Sao chép mã, template, cấp, HP, vị trí và tỷ lệ sát thương/tiềm năng; không sao zone hoặc Percent_gold. (Java: Mob(Mob))
    public Mob(Mob mob)
        : this(mob.Rt)
    {
        Id = mob.Id;
        TempId = mob.TempId;
        Level = mob.Level;
        Point.SetHpFull(mob.Point.GetHpFull());
        Point.Sethp(Point.GetHpFull());
        Location.X = mob.Location.X;
        Location.Y = mob.Location.Y;
        DamagePercent = mob.DamagePercent;
        PotentialPercent = mob.PotentialPercent;
        Type = mob.Type;
        SetPotential();
    }

    // Runtime cung cấp thời gian, ngẫu nhiên và cổng dịch vụ cho mob.
    public MapRuntime Rt { get; }

    // Mã thực thể mob trong zone.
    public int Id { get; set; }

    // Zone hiện chứa mob; có thể null khi chưa gắn bản đồ.
    public Zone? Zone { get; set; }

    // Mã template xác định loại mob.
    public int TempId { get; set; }

    // Tên hiển thị của mob khi có.
    public string? Name { get; set; }

    // Cấp của mob.
    public sbyte Level { get; set; }

    // Danh sách người chơi vừa đánh mob để ưu tiên mục tiêu.
    public SnapshotList<Player> TemporaryEnemies { get; } = new();

    // HP và sát thương của mob.
    public MobPoint Point { get; }

    // Các hiệu ứng kỹ năng đang áp dụng cho mob.
    public MobEffectSkill EffectSkill { get; }

    // Vị trí hiện tại của mob.
    public MobLocation Location { get; }

    // Phần trăm dùng khi tính sát thương từ HP.
    public sbyte DamagePercent { get; set; }

    // Tỷ lệ tiềm năng cơ sở khi player đánh mob.
    public int PotentialPercent { get; set; }

    // Thời điểm mob chết gần nhất.
    public long LastTimeDie { get; set; }

    // Cấp trạng thái mob: 0 thường, lớn hơn 0 là siêu mob hoặc pha boss lớn. (Java: lvMob)
    public int LvMob { get; set; }

    // Trạng thái hoạt ảnh/giao thức hiện tại, mặc định 5.
    public int Status { get; set; } = 5;

    // Loại di chuyển/thuộc tính mob, mặc định 1.
    public int Type { get; set; } = 1;

    // Hệ số vàng riêng; copy constructor giữ 0 theo Java.
    public int Percent_gold { get; set; }

    // Thời điểm hồi máu gần nhất.
    public long LastRegenTime { get; set; }

    // Thời điểm gửi hiệu ứng gần nhất.
    public long LastTimeSendEffect { get; set; }

    // Mã hình ảnh mob khi giao thức cần.
    public int MobImage { get; set; }

    // Cho biết mob do player sở hữu.
    public bool IsPlayerOwnedMob { get; set; }

    private Zone Z => Zone!;

    private int MapIdOf(Player p) => p.Zone!.Map.MapId;

    private void SendAll(Message msg) => Rt.World.SendMessAllPlayerInMap(Z, msg);

    // Quay lại tiềm năng tối đa của mob từ HP và tỷ lệ cấu hình.
    public void SetPotential()
    {
        _maxPotential = Point.GetHpFull() * (PotentialPercent + Rt.NextInt(-2, 2)) / 100;
    }

    // Cho biết mob đã chết theo HP không dương.
    public bool IsDie() => Point.Gethp() <= 0;

    // Ghi nhận thời điểm mob chết và hồi máu.
    public void SetDie()
    {
        LastRegenTime = Rt.Now;
        LastTimeDie = Rt.Now;
    }

    // Thêm player đánh mob vào danh sách ưu tiên nếu chưa có.
    public void AddTemporaryEnemies(Player? pl)
    {
        if (pl != null && !TemporaryEnemies.Contains(pl))
        {
            TemporaryEnemies.Add(pl);
        }
    }

    // Cho biết mob là siêu mob hoặc đang ở pha boss lớn.
    public bool IsSuperMob() => LvMob > 0;

    // Xử lý nhận sát thương.

    // Áp quy tắc sát thương theo map/mob, trừ HP rồi xử lý chết hoặc gửi trạng thái còn sống. (Java: injured)
    public virtual void Injured(Player? plAtt, double damage, bool dieWhenHpFull)
    {
        if (IsDie())
        {
            return;
        }

        IMapRules rules = Rt.Rules;
        IMapPlayerGateway gateway = Rt.Players;
        if (plAtt != null && plAtt.Zone != null && plAtt.Zone.Map.MapId == 179 && !plAtt.IsDeTu)
        {
            damage = 0;
        }

        if (damage >= Point.Hp)
        {
            damage = Point.Hp;
        }

        if (!dieWhenHpFull)
        {
            if (Point.Hp == Point.MaxHp && damage >= Point.Hp)
            {
                damage = rules.IsMapRiengTu(Z.Map.MapId) ? Point.Hp - 0 : Point.Hp - 1;
            }

            if (TempId == ConstMob.HIRUDEGARN)
            {
                double maxDamageAtFullHP = 20_000_000;
                double hpRatio = (double)Point.Hp / Point.MaxHp;
                double power = 0.8;
                int calcDamage = JavaNumbers.ToInt(maxDamageAtFullHP * Math.Pow(hpRatio, power));
                int minDamage = JavaNumbers.ToInt(calcDamage * 0.5);
                int maxDamage = Math.Max(calcDamage, 10);
                damage = Rt.NextInt(minDamage, maxDamage);
                if (damage < 10)
                {
                    damage = 10;
                }
            }

            if ((TempId == ConstMob.TRAINING_DUMMY || TempId == ConstMob.HAUNTED_SCARECROW) && damage > Point.MaxHp / 10)
            {
                damage = Point.MaxHp / 10;
            }

            if (plAtt != null && rules.IsMapDiaNguc(MapIdOf(plAtt)))
            {
                damage = Point.MaxHp / 10;
            }

            if (plAtt != null && rules.IsMapNguHanhSon(MapIdOf(plAtt)))
            {
                damage = 80000;
            }

            if (plAtt != null && !rules.IsMapHungVuongEvent(MapIdOf(plAtt))
                && (TempId == ConstMob.NINE_TUSKED_ELEPHANT || TempId == ConstMob.NINE_SPURRED_ROOSTER || TempId == ConstMob.NINE_MANED_HORSE)
                && damage > Point.MaxHp / 10)
            {
                damage = Point.MaxHp / 10;
            }

            if (plAtt != null && rules.IsMapHungVuongEvent(MapIdOf(plAtt)))
            {
                if (TempId == ConstMob.NINE_TUSKED_ELEPHANT && damage > 100_000)
                {
                    damage = 100_000;
                }

                if (TempId == ConstMob.NINE_MANED_HORSE && damage > 75_000)
                {
                    damage = 75_000;
                }

                if (TempId == ConstMob.NINE_SPURRED_ROOSTER && damage > 60_000)
                {
                    damage = 60_000;
                }
            }
        }

        MobPlayerView? attackerView = plAtt != null ? gateway.GetMobPlayerView(plAtt) : null;
        if (plAtt != null && attackerView != null)
        {
            if (TempId == ConstMob.POWER_METER)
            {
                plAtt.TotalDameMayDam = JavaNumbers.ToLong(plAtt.TotalDameMayDam + damage);
                plAtt.LastTimeDameMayDam = Rt.Now;
            }

            int tlDameFly = attackerView.FlyingMobDamagePercent;
            int tlDameMonkey = attackerView.MonkeyMobDamagePercent;
            int tlDameRun = attackerView.RunningMobDamagePercent;
            if (tlDameFly > 0 && IsFlyingMob())
            {
                damage += Rt.CrisGH(damage / 100 * tlDameFly);
            }

            if (tlDameMonkey > 0 && IsMonkeyMob())
            {
                damage += Rt.CrisGH(damage / 100 * tlDameMonkey);
            }

            if (tlDameRun > 0 && IsGroundMob())
            {
                damage += Rt.CrisGH(damage / 100 * tlDameRun);
            }
        }

        if (rules.IsMapKhiGasHuyDiet(Z.Map.MapId))
        {
            bool mob76Die = true;
            foreach (Mob mob in Z.Mobs)
            {
                if (!mob.IsDie() && mob.TempId == ConstMob.DESTRUCTION_MACHINE)
                {
                    mob76Die = false;
                    break;
                }
            }

            if (!mob76Die && attackerView != null && attackerView.SelectedSkillId >= 0)
            {
                int skill = attackerView.SelectedSkillId;
                if (skill == Skill.LIEN_HOAN || skill == Skill.ANTOMIC || skill == Skill.MASENKO || skill == Skill.KAMEJOKO)
                {
                    damage = 1;
                }
            }
        }

        if (!dieWhenHpFull && !IsBigBoss() && !rules.IsMapPhoBan(Z.Map.MapId) && LvMob > 0 && plAtt != null && attackerView != null
            && attackerView.TdOaiHung < Rt.Now)
        {
            damage = (Point.MaxHp <= 20000000 ? Point.MaxHp * 10 : 2000000000) * (10.0 / 100);
            MobAttackPlayer(plAtt);
        }

        if (plAtt != null && plAtt.IsBoss && TempId > 0 && Rt.IsTrue(1, 2) && Rt.CanDoWithTime(_lastTimeAttackPlayer, 2500))
        {
            MobAttackPlayer(plAtt);
            _lastTimeAttackPlayer = Rt.Now;
        }

        Point.Hp = JavaNumbers.ToLong(Point.Hp - damage);
        AddTemporaryEnemies(plAtt);

        if (IsDie())
        {
            Status = 0;
            SetDie();
            TemporaryEnemies.Clear();
            if (plAtt != null)
            {
                SendMobDieAffterAttacked(plAtt, Rt.CrisGH(damage));
                Rt.World.OnMobKilled(plAtt, this);
            }

            if (Id == 13)
            {
                Z.Isbulon1Alive = false;
            }

            if (Id == 14)
            {
                Z.Isbulon2Alive = false;
            }
        }
        else
        {
            SendMobStillAliveAffterAttacked(damage, plAtt != null && attackerView != null && attackerView.IsCritical);
        }

        if (plAtt != null && plAtt.IsNguoiYeu)
        {
            Rt.World.AddSMTN(plAtt, 2, GetPotentialForPlayer(plAtt, damage), true);
        }

        if (plAtt != null)
        {
            if (gateway.IsPl(plAtt) && gateway.HasSatellite(plAtt) && gateway.GetMobPlayerView(plAtt).SatelliteDefend)
            {
                gateway.ClearSatelliteDefend(plAtt);
            }

            long tiemNang = GetPotentialForPlayer(plAtt, damage);
            Rt.World.AddSMTN(plAtt, 2, tiemNang, true);
            Rt.World.TangTnsmLuyenTap(plAtt, tiemNang);

            // Java gọi tangTnsmLuyenTap thêm lần nữa ngay sau lần đầu; giữ nguyên để gấp đôi tiềm năng luyện tập.
            Rt.World.TangTnsmLuyenTap(plAtt, tiemNang);
        }
    }

    // Tính tiềm năng nhận được từ đòn đánh, giảm hoặc tăng 10% cho mỗi chênh lệch cấp. (Java: getTiemNangForPlayer)
    public long GetPotentialForPlayer(Player pl, double damage)
    {
        int levelPlayer = Rt.World.GetCurrLevel(pl);
        int n = levelPlayer - Level;
        long pDameHit;
        if (Point.GetHpFull() >= 100000000)
        {
            pDameHit = Rt.CrisGH(damage) * 500 / Point.GetHpFull();
        }
        else
        {
            pDameHit = Rt.CrisGH(damage) * 100 / Point.GetHpFull();
        }

        long tiemNang = pDameHit * _maxPotential / 100;
        if (n >= 0)
        {
            for (int j = 0; j < n; j++)
            {
                long sub = tiemNang * 10 / 100;
                if (sub <= 0)
                {
                    sub = 1;
                }

                tiemNang -= sub;
            }
        }
        else
        {
            for (int j = 0; j < -n; j++)
            {
                long add = tiemNang * 10 / 100;
                if (add <= 0)
                {
                    add = 1;
                }

                tiemNang += add;
            }
        }

        if (tiemNang <= 0)
        {
            tiemNang = 1;
        }

        if (TempId == ConstMob.POWER_METER)
        {
            tiemNang = 1;
        }

        if (TempId == ConstMob.HIRUDEGARN)
        {
            tiemNang = 1;
        }

        return Rt.CrisGH(Rt.Players.CalculatePowerPotential(pl, tiemNang));
    }

    // Cập nhật theo tick.

    // Xử lý giờ Golden Frieza, hiệu ứng đặc biệt, hồi sinh theo loại map, hiệu ứng kỹ năng và tấn công. (Java: update)
    public virtual void Update()
    {
        if (Z.IsGoldenFriezaAlive && Rt.Clock.Is21H() && !IsDie())
        {
            StartDie();
            return;
        }

        if (!IsDie() && TempId == ConstMob.DESTRUCTION_MACHINE && Rt.CanDoWithTime(LastTimeSendEffect, 1000))
        {
            SendEffect(55);
            LastTimeSendEffect = Rt.Now;
        }

        if (!IsDie() && TempId == ConstMob.POWER_METER)
        {
            Point.Hp = 2000000000;
        }

        if (IsDie() && !Rt.Flags.MaintenanceRunning && !IsBigBoss())
        {
            switch (Z.Map.Type)
            {
                case ConstMap.MAP_RED_RIBBON_HQ:
                    if (TempId == ConstMob.BULON && Z.IsTUTAlive && Rt.CanDoWithTime(LastTimeDie, 10000))
                    {
                        Respawn();
                        RespawnDungeonMob();
                        if (Id == 13)
                        {
                            Z.Isbulon1Alive = true;
                        }

                        if (Id == 14)
                        {
                            Z.Isbulon2Alive = true;
                        }
                    }

                    break;
                case ConstMap.MAP_TREASURE_UNDER_SEA:
                case ConstMap.MAP_SNAKE_WAY:
                case ConstMap.MAP_DESTRON_GAS:
                case ConstMap.MAP_CLAN_BOSS:
                case ConstMap.MAP_WEST_KARIN:
                    break;
                default:
                    if (Z.IsGoldenFriezaAlive && Rt.Clock.Is21H())
                    {
                        return;
                    }

                    if (Rt.CanDoWithTime(LastRegenTime, 30000) && !IsDie())
                    {
                        LastRegenTime = Rt.Now;
                        long hpMax = Point.MaxHp;
                        if (Point.Hp < hpMax)
                        {
                            HealHp(hpMax / 10);
                        }
                        else
                        {
                            SendMobRespawn();
                        }
                    }

                    if (Rt.CanDoWithTime(LastTimeDie, 5000) && IsDie())
                    {
                        Respawn();
                        SendMobRespawn();
                    }

                    break;
            }
        }

        EffectSkill.Update();
        Attack();
    }

    // Kiểm tra template có phải boss lớn.
    public bool IsBigBoss() =>
        TempId == ConstMob.HIRUDEGARN || TempId == ConstMob.KING_OCTOPUS || TempId == ConstMob.GUARD_ROBOT || TempId == ConstMob.BEAR_THIEF
        || TempId == ConstMob.NINE_TUSKED_ELEPHANT || TempId == ConstMob.NINE_SPURRED_ROOSTER || TempId == ConstMob.NINE_MANED_HORSE || TempId == ConstMob.PIANO
        || TempId == ConstMob.KONG || TempId == ConstMob.GODZILLA;

    // Kiểm tra boss thuộc sự kiện Hùng Vương.
    public bool IsBigBossHungVuongEvent() =>
        TempId == ConstMob.NINE_TUSKED_ELEPHANT || TempId == ConstMob.NINE_SPURRED_ROOSTER || TempId == ConstMob.NINE_MANED_HORSE || TempId == ConstMob.KONG || TempId == ConstMob.GODZILLA;

    // Kiểm tra mob bay theo Type 4.
    public bool IsFlyingMob() => Type == 4;

    // Kiểm tra template thuộc nhóm khỉ.
    public bool IsMonkeyMob() =>
        TempId == ConstMob.IRON_ARMORED_MONKEY || TempId == ConstMob.BLACK_FURRED_MONKEY || TempId == ConstMob.RED_FURRED_MONKEY || TempId == ConstMob.YELLOW_FURRED_MONKEY
        || TempId == ConstMob.BLUE_FURRED_MONKEY;

    // Kiểm tra template thuộc nhóm heo.
    public bool IsPigMob() =>
        TempId == ConstMob.GREEN_SKINNED_PIG || TempId == ConstMob.FOREST_PIG || TempId == ConstMob.FOREST_PIG_MOTHER || TempId == ConstMob.GREEN_PIG_MOTHER
        || TempId == ConstMob.SAIYAN_PIG || TempId == ConstMob.SAIYAN_PIG_MOTHER;

    // Kiểm tra mob đi đất theo Type 1.
    public bool IsGroundMob() => Type == 1;

    // Chọn mục tiêu như Java và đánh khi mob hợp lệ, hết hồi chiêu. (Java: attack)
    public virtual void Attack()
    {
        Player? player = GetPlayerCanAttack();
        if (!IsDie() && !EffectSkill.IsHaveEffectSkill() && TempId != ConstMob.TRAINING_DUMMY && TempId != ConstMob.POWER_METER
            && TempId != ConstMob.HAUNTED_SCARECROW && TempId != ConstMob.DESTRUCTION_MACHINE && !IsBigBoss()
            && (LvMob < 1 || Rt.Rules.IsMapPhoBan(Z.Map.MapId))
            && Rt.CanDoWithTime(_lastTimeAttackPlayer, _timeAttack))
        {
            if (player != null)
            {
                MobAttackPlayer(player);
            }

            _lastTimeAttackPlayer = Rt.Now;
        }
    }

    // Ưu tiên player đã đánh mob trong 300, nếu không chọn player hợp lệ gần nhất trong 100; boss lớn không giới hạn khoảng cách. (Java: getPlayerCanAttack)
    public Player? GetPlayerCanAttack()
    {
        Player? plAttack = GetFirstPlayerCanAttack();
        if (plAttack != null)
        {
            return plAttack;
        }

        int distance = 100;
        try
        {
            SnapshotList<Player> players = Z.NotBosses;
            if (players.IsEmpty)
            {
                return null;
            }

            foreach (Player pl in players)
            {
                if (pl == null)
                {
                    continue;
                }

                MobPlayerView view = Rt.Players.GetMobPlayerView(pl);
                bool canAttack = !view.IsDie && !pl.IsBoss && !pl.IsPetFollow && !pl.IsDuongTang && !view.SatelliteDefend && !view.IsInvisible
                    && (TempId > 18 || (TempId > 9 && Type == 4) || IsBigBoss());
                if (canAttack)
                {
                    int dis = Util.GetDistance(view.X, view.Y, Location.X, Location.Y);
                    if (dis <= distance || IsBigBoss())
                    {
                        plAttack = pl;
                        distance = dis;
                    }
                }
            }

            _timeAttack = 2000;
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }

        return plAttack;
    }

    private Player? GetFirstPlayerCanAttack()
    {
        Player? plAtt = null;
        try
        {
            int dis = 300;
            foreach (Player plAttt in Z.Humanoids)
            {
                if (plAttt == null)
                {
                    continue;
                }

                MobPlayerView view = Rt.Players.GetMobPlayerView(plAttt);
                if (view.IsDie || plAttt.IsBoss || view.SatelliteDefend || view.IsInvisible || !TemporaryEnemies.Contains(plAttt))
                {
                    continue;
                }

                int d = Util.GetDistance(view.X, view.Y, Location.X, Location.Y);
                if (d <= dis)
                {
                    dis = d;
                    plAtt = plAttt;
                }
            }

            _timeAttack = 1000;
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }

        return plAtt;
    }

    private void MobAttackPlayer(Player player)
    {
        MobPlayerView view = Rt.Players.GetMobPlayerView(player);
        double dameMob = Rt.CrisGH(Point.GetDamageAttack());
        if (dameMob > 2_000_000_000)
        {
            dameMob = 2_000_000_000;
        }

        if (view.TdDaTrau > Rt.Now)
        {
            dameMob /= 2;
        }

        if (view.ClanBuaDaTrau > Rt.Now)
        {
            int clanLevel = view.ClanLevel;
            int bonusPercent = clanLevel * 20;
            if (bonusPercent > 200)
            {
                bonusPercent = 200;
            }

            dameMob -= dameMob * bonusPercent / 100;
        }

        if (player.IsDeTu && view.MasterTdDeTu > Rt.Now)
        {
            dameMob /= 2;
        }

        if (LvMob > 0 && !Rt.Rules.IsMapPhoBan(Z.Map.MapId))
        {
            dameMob = view.HpMax * (10.0 / 100);
        }

        if (view.SatelliteDefend)
        {
            dameMob -= dameMob / 5;
        }

        if (view.IsCommenson)
        {
            dameMob = JavaNumbers.Round(dameMob * 0.1);
        }

        if (LvMob > 0 && view.TdOaiHung > Rt.Now)
        {
            dameMob = 0;
        }

        if (Rt.Rules.IsMapNguHanhSon(MapIdOf(player)))
        {
            dameMob = view.HpMax / 10;
        }

        double dame = Rt.Players.Injured(player, null, Rt.CrisGH(dameMob), false, true);
        SendMobAttackMe(player, dame);
        SendMobAttackPlayer(player);
        ReflectDamage(player, dame);
    }

    private void SendMobAttackMe(Player player, double dame)
    {
        if (!player.IsDeTu && !player.IsBo && !player.IsMe && !player.IsPetFollow && !player.IsDuongTang && !player.IsBot && !player.IsBot_Event
            && !player.IsBot_New && !player.IsBot_Valentine && !player.IsNguoiYeu && !player.IsConOne && !player.IsConTwo && !player.IsConThree)
        {
            try
            {
                Message msg = new(-11);
                msg.Writer().WriteByte(Id);
                msg.WriteCris(Rt.CrisGH(dame), Rt.ReadInt);
                Rt.Players.SendMessage(player, msg);
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Mob), e);
            }
        }
    }

    private void SendMobAttackPlayer(Player player)
    {
        try
        {
            Message msg = new(-10);
            msg.Writer().WriteByte(Id);
            msg.Writer().WriteInt(unchecked((int)player.Id));
            msg.WriteCris(Rt.CrisGH(Rt.Players.GetMobPlayerView(player).Hp), Rt.ReadInt);
            Rt.World.SendMessAnotherNotMeInMap(player, msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    // Hồi sinh và hiệu ứng.

    // Khôi phục trạng thái thường, HP đầy và tiềm năng của mob.
    public void Respawn()
    {
        Status = 5;
        Point.Hp = Point.MaxHp;
        SetPotential();
    }

    // Khi zone chưa có siêu mob, quay 10% để mob này thành siêu mob và hồi HP; nếu đã có thì trả 0 không quay. (Java: lvMob; đổi tên thành ComputeLvMob)
    public int ComputeLvMob()
    {
        foreach (Mob mobMap in Z.Mobs)
        {
            if (mobMap.LvMob > 0)
            {
                return 0;
            }
        }

        LvMob = TempId > 18 && !IsBigBoss() ? (Rt.IsTrue(10, 100) ? 1 : 0) : 0;
        Point.Hp = LvMob > 0 ? (Point.MaxHp <= 20000000 ? Point.MaxHp * 10 : 2000000000) : Point.MaxHp;
        return LvMob;
    }

    // Gửi gói hồi sinh và HP tối đa cho client.
    public void SendMobRespawn()
    {
        try
        {
            Message msg = new(-13);
            msg.Writer().WriteByte(Id);
            msg.Writer().WriteByte(TempId);
            msg.Writer().WriteByte(ComputeLvMob());
            msg.WriteCris(Rt.CrisGH(Point.Hp), Rt.ReadInt);
            SendAll(msg);
            SendMobMaxHp(Point.Hp);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    // Gửi gói cập nhật HP tối đa.
    public void SendMobMaxHp(long maxHp)
    {
        try
        {
            Message msg = new(87);
            msg.Writer().WriteByte(Id);
            msg.WriteCris(Rt.CrisGH(maxHp), Rt.ReadInt);
            SendAll(msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    // Hồi HP, hoặc gây tổn thương khi hp âm, rồi phát trạng thái. (Java: hoi_hp)
    public void HealHp(long hp)
    {
        try
        {
            Point.Sethp(Point.Gethp() + hp);
            long hpShown = hp > 0 ? 1 : Math.Abs(hp);
            Message msg = new(-9);
            msg.Writer().WriteByte(Id);
            msg.WriteCris(Rt.CrisGH(Point.Gethp()), Rt.ReadInt);
            msg.WriteCris(Rt.CrisGH(hpShown), Rt.ReadInt);
            msg.Writer().WriteBoolean(false);
            msg.Writer().WriteByte(-1);
            SendAll(msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    // Phát hiệu ứng gắn với mob.
    public void SendEffect(int effect)
    {
        try
        {
            Message msg = new(-9);
            msg.Writer().WriteByte(Id);
            msg.WriteCris(Rt.CrisGH(Point.Gethp()), Rt.ReadInt);
            msg.WriteCris(Rt.CrisGH(Point.Gethp()), Rt.ReadInt);
            msg.Writer().WriteBoolean(false);
            msg.Writer().WriteByte(effect);
            SendAll(msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    private void SendMobDieAffterAttacked(Player plKill, double dameHit)
    {
        try
        {
            Message msg = new(-12);
            msg.Writer().WriteByte(Id);
            msg.WriteCris(Rt.CrisGH(dameHit), Rt.ReadInt);
            msg.Writer().WriteBoolean(Rt.Players.GetMobPlayerView(plKill).IsCritical);
            List<ItemMap>? items = null;
            if (Rt.Players.IsPl(plKill))
            {
                items = MobReward(plKill, DropItemTask(plKill), msg);
            }

            SendAll(msg);
            if (items != null)
            {
                PullItems(plKill, items);
            }
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    // Gửi chết và vật phẩm rơi khi mob do player sở hữu đánh thú cưng.
    public void SendMobDieAfterPlayerOwnedMobAttackedPet(Player plKill, double damageHit)
    {
        Status = 0;
        try
        {
            Message msg = new(-12);
            msg.Writer().WriteByte(Id);
            msg.WriteCris(Rt.CrisGH(damageHit), Rt.ReadInt);
            msg.Writer().WriteBoolean(false);
            List<ItemMap> items = MobReward(plKill, DropItemTask(plKill), msg);
            Rt.World.SendMessAllPlayerInMap(Z, msg);
            PullItems(plKill, items);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }

        LastTimeDie = Rt.Now;
    }

    private void SendMobStillAliveAffterAttacked(double dameHit, bool crit)
    {
        try
        {
            Message msg = new(-9);
            msg.Writer().WriteByte(Id);
            msg.WriteCris(Rt.CrisGH(Point.Gethp()), Rt.ReadInt);
            msg.WriteCris(Rt.CrisGH(dameHit), Rt.ReadInt);
            msg.Writer().WriteBoolean(crit);
            msg.Writer().WriteInt(-1);
            SendAll(msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    // Hồi sinh mob phó bản với HP đầy, tiềm năng mới và gói -13.
    public void RespawnDungeonMob()
    {
        Point.Hp = Point.MaxHp;
        SetPotential();
        SendRespawn13();
    }

    // Hồi sinh mob Tây Karin với HP đầy, tiềm năng cố định 1 và gói -13.
    public void RespawnWestKarinMob()
    {
        Point.Hp = Point.MaxHp;
        _maxPotential = 1;
        SendRespawn13();
    }

    private void SendRespawn13()
    {
        try
        {
            Message msg = new(-13);
            msg.Writer().WriteByte(Id);
            msg.Writer().WriteByte(TempId);
            msg.Writer().WriteByte(LvMob);
            msg.WriteCris(Rt.CrisGH(Point.Hp), Rt.ReadInt);
            SendAll(msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    // Gửi kiểu siêu mob qua gói -75.
    public void SendSuperMob(int type) => SendIdByte(-75, type);

    // Gửi cờ vô hiệu qua gói 81.
    public void SendDisable(bool value) => SendIdBool(81, value);

    // Gửi cờ hoàn tất di chuyển qua gói 82.
    public void SendDoneMove(bool value) => SendIdBool(82, value);

    // Gửi cờ hiệu ứng lửa qua gói 85.
    public void SendFire(bool value) => SendIdBool(85, value);

    // Gửi cờ hiệu ứng băng qua gói 86.
    public void SendIce(bool value) => SendIdBool(86, value);

    // Gửi cờ hiệu ứng gió qua gói 87.
    public void SendWind(bool value) => SendIdBool(87, value);

    private void SendIdByte(int cmd, int value)
    {
        try
        {
            Message msg = new(cmd);
            msg.Writer().WriteByte(Id);
            msg.Writer().WriteByte(value);
            SendAll(msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    private void SendIdBool(int cmd, bool value)
    {
        try
        {
            Message msg = new(cmd);
            msg.Writer().WriteByte(Id);
            msg.Writer().WriteBoolean(value);
            SendAll(msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    private void ReflectDamage(Player plTarget, double dame)
    {
        int percentPST = Rt.Players.GetMobPlayerView(plTarget).ReflectDamagePercent;
        if (percentPST != 0)
        {
            double damePST = Rt.CrisGH(dame * percentPST / 100L);
            try
            {
                Message msg = new(-9);
                msg.Writer().WriteByte(Id);
                if (damePST >= Point.Hp)
                {
                    damePST = Point.Hp - 1;
                }

                long hpMob = Rt.CrisGH(Point.Hp);
                Injured(null, damePST, true);
                damePST = hpMob - Point.Hp;
                msg.WriteCris(Rt.CrisGH(Point.Hp), Rt.ReadInt);
                msg.WriteCris(Rt.CrisGH(damePST), Rt.ReadInt);
                msg.Writer().WriteBoolean(false);
                msg.Writer().WriteByte(36);
                SendAll(msg);
            }
            catch (Exception e)
            {
                Logger.LogException(typeof(Mob), e);
            }
        }
    }

    // Giết mob ngay bằng HP -1 rồi thông báo zone. (Java: startDie)
    public void StartDie()
    {
        try
        {
            SetDie();
            Point.Hp = -1;
            Status = 0;
            Message msg = new(-12);
            msg.Writer().WriteByte(Id);
            SendAll(msg);
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }
    }

    // Vật phẩm rơi.

    private ItemMap CreateItemMap(Player player, int itemId, int x, int y, params ItemMapOption[] options)
    {
        ItemMap itemMap = new(Z, itemId, 1, x + Rt.NextInt(-10, 10), y, player.Id);
        itemMap.Options.AddRange(options);
        return itemMap;
    }

    private static ItemMapOption Opt(int id, int param) => new(id, param);

    private List<ItemMap> GetItemEventMobReward(Player player, int x, int yEnd)
    {
        List<ItemMap> list = [];
        if (player.IsBoss || TempId == 0)
        {
            return list;
        }

        MobLootPlayerView lv = Rt.Loot.GetLootView(player);
        double tyLeMayMan = lv.LuckPercent;
        bool coBonLa = lv.UseCoBonLa;
        int mapId = MapIdOf(player);
        IMapRules rules = Rt.Rules;
        IGameFlags flags = Rt.Flags;
        IMobLootHost loot = Rt.Loot;
        bool Luck(double mum, double max) => Rt.Random.GetChanceFromLuck(tyLeMayMan, mum, max, coBonLa);
        int Next(int a, int b) => Rt.NextInt(a, b);

        // Sự kiện Tết.
        if (flags.LunarNewYear && !rules.IsMapNotDropItems(mapId))
        {
            if (IsPigMob() && Luck(1, 20))
            {
                list.Add(CreateItemMap(player, 748, x, yEnd, Opt(86, 0), Opt(93, 30)));
            }
            else
            {
                if (Luck(1, 12))
                {
                    list.Add(CreateItemMap(player, Next(749, 750), x, yEnd, Opt(86, 0), Opt(93, 30)));
                }

                if (Luck(1, 8))
                {
                    list.Add(CreateItemMap(player, Next(1177, 1181), x, yEnd, Opt(86, 0), Opt(93, 30)));
                }
            }

            if (Luck(1, 8))
            {
                list.Add(CreateItemMap(player, Next(1473, 1474), x, yEnd, Opt(86, 0), Opt(93, 30)));
            }

            if (Luck(1, 20))
            {
                list.Add(CreateItemMap(player, 1475, x, yEnd, Opt(86, 0), Opt(93, 30)));
            }
        }

        // Sự kiện Noel.
        if (flags.Christmas && !rules.IsMapNotDropItems(mapId))
        {
            if (loot.HasChristmasHat(player))
            {
                if (Luck(1, 100) && loot.CanDropKeoGiangSinh(player, 99))
                {
                    list.Add(CreateItemMap(player, 533, x, yEnd, Opt(30, 0)));
                }

                if (Luck(1, 80) && loot.CanDropTatVoGiangSinh(player, 99))
                {
                    list.Add(CreateItemMap(player, 649, x, yEnd, Opt(30, 0), Opt(93, 30)));
                }
            }

            if (rules.IsMapChristMasEvent(mapId))
            {
                if (Luck(1, 20))
                {
                    list.Add(CreateItemMap(player, Next(1459, 1461), x, yEnd, Opt(86, 0), Opt(93, 30)));
                }

                if (Luck(1, 50))
                {
                    list.Add(CreateItemMap(player, 1839, x, yEnd, Opt(86, 0), Opt(93, 30)));
                }

                if (Luck(1, 10))
                {
                    list.Add(CreateItemMap(player, 1445, x, yEnd, Opt(86, 0), Opt(93, 30)));
                }
            }

            if (Luck(1, 199))
            {
                list.Add(CreateItemMap(player, Next(925, 927), x, yEnd, Opt(87, 0), Opt(30, 0), Opt(93, 35)));
            }

            if (Luck(1, 99))
            {
                list.Add(CreateItemMap(player, Next(928, 931), x, yEnd, Opt(87, 0), Opt(30, 0), Opt(93, 35)));
            }
        }

        // Sự kiện Vu Lan.
        if (flags.VuLanFestival && rules.IsMapDiaNguc(mapId) && !rules.IsMapNotDropItems(mapId))
        {
            bool useMayDo = lv.UseMayDoLinhHon;
            if (Luck(1, useMayDo ? 5 : 10))
            {
                list.Add(CreateItemMap(player, 1258, x, yEnd, Opt(30, 0)));
            }

            if (Luck(1, 5))
            {
                list.Add(CreateItemMap(player, 1032, x, yEnd, Opt(30, 0)));
            }

            if (Luck(1, 7))
            {
                list.Add(CreateItemMap(player, 1035, x, yEnd, Opt(30, 0)));
            }
        }

        // Sự kiện Halloween.
        if (flags.Halloween && !rules.IsMapNotDropItems(mapId))
        {
            double hp = Point.Hp;
            double hpFactor = Math.Floor(hp / 1000.0);
            double max = 30.0 - (hpFactor * 0.02);
            max = Math.Max(10.0, Math.Min(30.0, max));
            if (lv.UseMayDoNgocBi && Luck(1, max))
            {
                list.Add(CreateItemMap(player, Next(705, 708), x, yEnd, Opt(87, 0), Opt(30, 0), Opt(93, 35)));
            }

            if (Luck(1, max))
            {
                list.Add(CreateItemMap(player, 901, x, yEnd, Opt(86, 0), Opt(93, 35)));
            }
        }

        // Sự kiện 8/3.
        if (flags.InternationalWomansDay && !rules.IsMapNotDropItems(mapId))
        {
            if (Luck(1, 20))
            {
                list.Add(CreateItemMap(player, 1526, x, yEnd, Opt(30, 0), Opt(93, 30)));
            }

            if (!rules.IsMapUpSKH(mapId) && Luck(1, 20))
            {
                list.Add(CreateItemMap(player, 1505, x, yEnd, Opt(86, 0), Opt(93, 35), Opt(30, 0)));
            }

            if (!loot.HasNamekianEquipment(player) && Luck(1, 20))
            {
                list.Add(CreateItemMap(player, 1507, x, yEnd, Opt(93, 35), Opt(86, 0), Opt(30, 0)));
            }

            if (rules.IsMapSauLang(mapId) && Luck(1, 30))
            {
                list.Add(CreateItemMap(player, 1508, x, yEnd, Opt(93, 35), Opt(86, 0), Opt(30, 0)));
            }
        }

        // Sự kiện Trung Thu.
        if (flags.TrungThu && !rules.IsMapNotDropItems(mapId))
        {
            if (Luck(1, 50))
            {
                list.Add(CreateItemMap(player, 1311, x, yEnd, Opt(30, 0), Opt(93, 35)));
            }

            if (!rules.IsMapMobBangHoiPart1(mapId))
            {
                if (Luck(1, 10))
                {
                    list.Add(CreateItemMap(player, Next(888, 889), x, yEnd, Opt(86, 0), Opt(30, 0), Opt(93, 35)));
                }

                if (Luck(1, 20))
                {
                    list.Add(CreateItemMap(player, 886, x, yEnd, Opt(30, 0), Opt(93, 35)));
                }

                if (Luck(1, 25))
                {
                    list.Add(CreateItemMap(player, 887, x, yEnd, Opt(30, 0), Opt(93, 35)));
                }
            }

            if (rules.IsMapMobBangHoiPart1(mapId) && Luck(1, 30))
            {
                list.Add(CreateItemMap(player, 1312, x, yEnd, Opt(86, 0), Opt(30, 0), Opt(93, 35)));
            }
        }

        // Sự kiện Hùng Vương.
        if (flags.HungVuong && !rules.IsMapNotDropItems(mapId))
        {
            short[] foods = [1548, 1547, 1545];
            if (rules.IsMapKongvsGodzila(mapId))
            {
                if (TempId == ConstMob.KONG)
                {
                    list.Add(CreateItemMap(
                        player,
                        1551,
                        x,
                        yEnd,
                        Opt(50, Next(15, 18)),
                        Opt(77, Next(15, 18)),
                        Opt(103, Next(15, 18)),
                        Opt(94, Next(10, 15)),
                        Rt.IsTrue(90, 100) ? Opt(93, Next(3, 7)) : Opt(73, 0)));
                    if (Rt.IsTrue(70, 100))
                    {
                        list.Add(CreateItemMap(player, 1173, x, yEnd, Opt(30, 0)));
                    }

                    for (int j = 0; j < 10; j++)
                    {
                        list.Add(CreateItemMap(player, foods[Next(0, foods.Length - 1)], x + Next(-50, 50), yEnd, Opt(87, 0), Opt(93, 30)));
                    }
                }

                if (TempId == ConstMob.GODZILLA)
                {
                    list.Add(CreateItemMap(
                        player,
                        1550,
                        x,
                        yEnd,
                        Opt(50, Next(15, 17)),
                        Opt(77, Next(15, 17)),
                        Opt(103, Next(15, 17)),
                        Opt(14, Next(11, 15)),
                        Rt.IsTrue(90, 100) ? Opt(93, Next(3, 7)) : Opt(73, 0)));
                    if (Rt.IsTrue(70, 100))
                    {
                        list.Add(CreateItemMap(player, 1173, x, yEnd, Opt(30, 0)));
                    }

                    for (int j = 0; j < 10; j++)
                    {
                        list.Add(CreateItemMap(player, foods[Next(0, foods.Length - 1)], x + Next(-50, 50), yEnd, Opt(87, 0), Opt(93, 30)));
                    }
                }

                if (Luck(1, 10))
                {
                    list.Add(CreateItemMap(player, 1546, x, yEnd, Opt(86, 0), Opt(93, 30)));
                }
            }

            if (TempId == ConstMob.NINE_TUSKED_ELEPHANT)
            {
                AddHungVuongBossLoot(list, player, 1220, x, yEnd);
            }

            if (TempId == ConstMob.NINE_SPURRED_ROOSTER)
            {
                AddHungVuongBossLoot(list, player, 1221, x, yEnd);
            }

            if (TempId == ConstMob.NINE_MANED_HORSE)
            {
                AddHungVuongBossLoot(list, player, 1222, x, yEnd);
            }

            if (IsPigMob() && Luck(1, 20))
            {
                list.Add(CreateItemMap(player, 1549, x + Next(-15, 15), yEnd, Opt(87, 0), Opt(30, 0), Opt(93, 30)));
            }
        }

        // Sự kiện Valentine.
        if (flags.ValentineDay && !rules.IsMapNotDropItems(mapId) && Luck(1, 18))
        {
            list.Add(CreateItemMap(player, 709, x + Next(-15, 15), yEnd, Opt(86, 0), Opt(93, 30)));
        }

        // Sự kiện 20/10.
        if (flags.Day2010 && !rules.IsMapNotDropItems(mapId))
        {
            if (Luck(1, 15))
            {
                list.Add(CreateItemMap(player, 1093, x + Next(-15, 15), yEnd, Opt(93, 30)));
            }

            if (Luck(1, 15))
            {
                list.Add(CreateItemMap(player, 1095, x + Next(-15, 15), yEnd, Opt(93, 30)));
            }
        }

        return list;
    }

    // Java đọc lại nextInt ở điều kiện lặp nên số lượt lặp cũng ngẫu nhiên; giữ nguyên.
    private void AddHungVuongBossLoot(List<ItemMap> list, Player player, int specialItemId, int x, int yEnd)
    {
        for (int j = 0; j < Rt.NextInt(1, 2); j++)
        {
            list.Add(CreateItemMap(player, specialItemId, x + Rt.NextInt(-50, 50), yEnd, Opt(87, 0), Opt(93, 30)));
        }

        for (int j = 0; j < Rt.NextInt(1, 10); j++)
        {
            list.Add(CreateItemMap(player, 861, x + Rt.NextInt(-50, 50), yEnd));
        }
    }

    private void PullItems(Player player, List<ItemMap> items)
    {
        MobPlayerView view = Rt.Players.GetMobPlayerView(player);
        if (view.ThuHutOwner != null && view.TdThuHut > Rt.Now)
        {
            foreach (ItemMap item in items)
            {
                Rt.World.PullItemToPlayer(view.ThuHutOwner, item.ItemMapId);
            }
        }
    }

    private List<ItemMap> MobReward(Player player, ItemMap? itemTask, Message msg)
    {
        List<ItemMap> itemReward = [];
        try
        {
            itemReward = GetItemMobReward(player, Location.X + Rt.NextInt(-10, 10), Z.Map.YPhysicInTop(Location.X, Location.Y));
            if (itemTask != null)
            {
                itemReward.Add(itemTask);
            }

            JavaDataOutput w = msg.Writer();
            w.WriteByte(itemReward.Count);
            foreach (ItemMap itemMap in itemReward)
            {
                w.WriteShort(itemMap.ItemMapId);
                w.WriteShort(itemMap.ItemTemplate!.Id);
                w.WriteShort(itemMap.X);
                w.WriteShort(itemMap.Y);
                w.WriteInt(unchecked((int)itemMap.PlayerId));
            }
        }
        catch (Exception e)
        {
            Logger.LogException(typeof(Mob), e);
        }

        return itemReward;
    }

    private int GetGoldDrop(MobLootPlayerView lv, Mob mob)
    {
        long playerPower = lv.Power;
        long mobHp = mob.Point.MaxHp;
        double baseGold = (mobHp / 200.0) * Rt.NextInt(20, 30);
        double smFactor = 1 + Math.Pow(playerPower / ((mobHp * 1000.0) + 1), 0.5);
        double totalGold = baseGold / smFactor;
        totalGold *= Math.Log10(mobHp + 1);
        if (totalGold > 300_000)
        {
            totalGold = 300_000;
        }

        totalGold *= 0.5 + (Rt.Random.NextDouble(1.0) * 0.2);
        return Math.Max(1, JavaNumbers.ToInt(totalGold));
    }

    private void DropGold(Player player, MobLootPlayerView lv, Mob mob, List<ItemMap> list, Zone zone, int x, int y)
    {
        double? attribute = Rt.World.GetActiveGoldAttributeValue();
        int goldAmount = GetGoldDrop(lv, mob);
        if (goldAmount <= 0)
        {
            return;
        }

        goldAmount += Percent_gold - 1;
        double playerBonus = 1.0 + (lv.GoldPercent / 100.0);
        double totalGold = goldAmount * playerBonus;
        if (attribute != null)
        {
            totalGold *= attribute.Value / 100.0;
        }

        int finalGold = JavaNumbers.ToInt(totalGold);
        if (finalGold > 300_000)
        {
            finalGold = 300_000;
        }

        if (finalGold <= 0)
        {
            finalGold = 1;
        }

        ItemMap itemMap = new(zone, 190, finalGold, x + Rt.NextInt(-10, 10), y, player.Id);
        list.Add(itemMap);
    }

    // Quay toàn bộ phần thưởng khi giết mob: sự kiện, vàng, đồ rác, thỏi, thưởng bộ và map đặc biệt; vật phẩm Java thả ngay vẫn được thông báo ngay, các vật phẩm khác được trả về. (Java: getItemMobReward)
    public List<ItemMap> GetItemMobReward(Player player, int x, int yEnd)
    {
        List<ItemMap> list = [];
        IMapRules rules = Rt.Rules;
        IMobLootHost loot = Rt.Loot;
        IMapWorld world = Rt.World;

        world.UpdateBadgesTaskCount(player, ConstTaskBadges.SCRAP_GOD, 1);
        if (player.IsBoss)
        {
            return list;
        }

        list.AddRange(GetItemEventMobReward(player, x, yEnd));

        MobLootPlayerView lv = loot.GetLootView(player);
        double tyLeMayMan = lv.LuckPercent;
        bool coBonLa = lv.UseCoBonLa;
        int mapid = MapIdOf(player);
        bool Luck(double mum, double max) => Rt.Random.GetChanceFromLuck(tyLeMayMan, mum, max, coBonLa);
        int Next(int a, int b) => Rt.NextInt(a, b);

        // Điểm Ngũ Hành Sơn.
        if (rules.IsMapNguHanhSon(mapid) && Luck(1, 5))
        {
            loot.AddEventPointNHS(player, 1);
            world.SendNotification(player, "Bạn nhận được 1 điểm Ngũ Hành Sơn");
        }

        // Mảnh bông tai.
        if (rules.IsMapBang(mapid) && Luck(1, 10))
        {
            ItemMap it = new(Z, 934, 1, x + Next(-10, 10), yEnd, player.Id);
            world.DropItemMap(Z, it);
        }

        // Bình nước.
        if (mapid == 5 || mapid == 13 || mapid == 29 || mapid == 30 || mapid == 33 || mapid == 34)
        {
            Player pl = loot.GetBinhNuocOwner(player);
            if (((mapid == 5 || mapid == 13) ? Luck(1, 5) : Luck(1, 4)) && loot.CanDropBinhNuoc(pl, 100))
            {
                list.Add(new ItemMap(Z, 456, 1, x, yEnd, pl.Id));
            }
        }

        // Vàng thường.
        if (!rules.IsMapBanDoKhoBau(mapid) && !rules.IsMapLang(mapid) && Rt.IsTrue(2, 10))
        {
            DropGold(player, lv, this, list, Z, x, yEnd);
        }

        // Vàng bản đồ kho báu.
        if (rules.IsMapBanDoKhoBau(mapid))
        {
            DropGoldForTreasureUnderSea(player, lv, yEnd);
        }

        // Danh hiệu Thiên Tử.
        if (rules.IsMapHanhTinhNgucTu(mapid))
        {
            if (player.LastTimeDanhHieu_ThienTu > 0 && player.IsUseDanhHieu_ThienTu)
            {
                if (Luck(1, 10))
                {
                    list.Add(new ItemMap(Z, 2012, 1, x + Next(-10, 10), yEnd, player.Id));
                }
            }
            else if (Luck(1, 15))
            {
                list.Add(new ItemMap(Z, 2012, 1, x + Next(-10, 10), yEnd, player.Id));
            }
        }

        // Viên nang.
        if (lv.UseMayDo && rules.IsMapTuongLai(mapid) && Rt.IsTrue(1, 30))
        {
            list.Add(new ItemMap(Z, 380, 1, x, yEnd, player.Id));
        }

        // Đá siêu cấp.
        if (lv.UseMayDoSieuHoa && rules.IsMapCold(mapid) && Rt.IsTrue(1, 70))
        {
            list.Add(new ItemMap(Z, 1979, 1, x, yEnd, player.Id));
        }

        // Vật phẩm rác.
        if (Rt.IsTrue(1, 7) && !rules.IsMapNotDropItems(mapid))
        {
            ItemMap it = new(Z, 225, 1, x + Next(-10, 10), yEnd, player.Id);
            it.AddOptionParam(74, 0);
            list.Add(it);
        }

        if (Rt.IsTrue(1, 25))
        {
            list.Add(new ItemMap(Z, 20, 1, x + Next(-10, 10), yEnd, player.Id));
        }

        // Thỏi vàng.
        if (lv.IsActive)
        {
            int tile = 200;
            if (lv.VeThangActive)
            {
                tile = 50;
            }
            else if (lv.VeTuanActive)
            {
                tile = 150;
            }

            if (Rt.IsTrue(1, tile))
            {
                list.Add(new ItemMap(Z, 457, 1, x + Next(-10, 10), yEnd, player.Id));
            }
        }

        if (Rt.IsTrue(1, 30))
        {
            list.Add(new ItemMap(Z, 19, 1, x + Next(-10, 10), yEnd, player.Id));
        }

        if (lv.IsSPLCostume && Rt.IsTrue(1, 10))
        {
            int rand = Next(0, 6);
            ItemMap it = new(Z, rand + 441, 1, x + Next(-10, 10), yEnd, player.Id);
            it.AddOptionParam(95 + rand, (rand == 3 || rand == 4) ? 3 : 5);
            list.Add(it);
        }

        // Vàng thường, lượt quay thứ hai như Java.
        if (!rules.IsMapBanDoKhoBau(mapid) && !rules.IsMapLang(mapid) && Rt.IsTrue(2, 10))
        {
            DropGold(player, lv, this, list, Z, x, yEnd);
        }

        // Thức ăn khi mặc đủ bộ Thần Linh.
        if (rules.IsMapCold(mapid) && Rt.IsTrue(1, 30) && loot.CountBodySetPieces(player, BodySetKind.Dtl) == 5)
        {
            int[] foods = MobDrop.FoodItemTemplateIds;
            int randomFood = foods[Next(0, foods.Length - 1)];
            world.DropItemMap(Z, new ItemMap(Z, randomFood, 1, x + Next(-15, 15), yEnd, player.Id));
        }

        // Thứ tự ưu tiên Java: Lang || UpSKH || Cold || (TuongLai && isTrue(1, 10)).
        if (rules.IsMapLang(mapid) || rules.IsMapUpSKH(mapid) || rules.IsMapCold(mapid) || (rules.IsMapTuongLai(mapid) && Rt.IsTrue(1, 10)))
        {
            if (Rt.IsTrue(1, 10))
            {
                int[] stones = MobDrop.StoneItemTemplateIds;
                int randomStone = stones[Next(0, stones.Length - 1)];
                world.DropItemMap(Z, new ItemMap(Z, randomStone, 1, x + Next(-15, 15), yEnd, player.Id));
            }
        }

        // Thưởng bộ Hủy Diệt.
        if (mapid >= 161 && mapid <= 163 && Rt.IsTrue(1, 100) && loot.CountBodySetPieces(player, BodySetKind.Dhd) == 5)
        {
            int randomItem = Next(1066, 1070);
            world.DropItemMap(Z, new ItemMap(Z, randomItem, 1, x + Next(-15, 15), yEnd, player.Id));
        }

        // Tăng trưởng bình hút năng lượng.
        if (rules.IsMapBinhHutNangLuong(mapid) && loot.HasEnergyAbsorbingBottle(player))
        {
            IReadOnlyList<int> parameters = loot.GetBinhHutNangLuongParams(player);
            for (int k = 0; k < parameters.Count; k++)
            {
                int param = parameters[k];
                if (param >= 3000)
                {
                    continue;
                }

                int chance;
                if (param < 1000)
                {
                    chance = 3;
                }
                else if (param < 2000)
                {
                    chance = 6;
                }
                else if (param < 2900)
                {
                    chance = 9;
                }
                else
                {
                    chance = 15;
                }

                if (Rt.IsTrue(1, chance))
                {
                    loot.IncreaseBinhHutNangLuong(player, k);
                }
            }
        }

        if (rules.IsMapPotara(mapid) && Luck(1, 5))
        {
            ItemMap it = new(Z, 1927, 1, x + Next(-10, 10), yEnd, player.Id);
            it.AddOptionParam(31, 1);
            it.AddOptionParam(30, 0);
            world.DropItemMap(Z, it);
        }

        return list;
    }

    private void DropGoldForTreasureUnderSea(Player pl, MobLootPlayerView lv, int locationY)
    {
        double? attribute = Rt.World.GetActiveGoldAttributeValue();
        if (!lv.HasTreasureMapClan)
        {
            return;
        }

        long baseGold = Point.MaxHp / 40;
        List<long> goldChunks = [];
        for (int j = 0; j < 8; j++)
        {
            double randFactor = 0.9 + (Rt.Random.NextDouble(1.0) * 0.2);
            long goldAmount = JavaNumbers.ToLong(baseGold * randFactor);
            double playerBonus = (100.0 + lv.GoldPercent) / 100.0;
            long totalGold = JavaNumbers.ToLong(goldAmount * playerBonus);
            if (attribute != null)
            {
                totalGold = JavaNumbers.ToLong(totalGold * attribute.Value / 100.0);
            }

            if (totalGold <= 0)
            {
                totalGold = 1;
            }

            while (totalGold > 0)
            {
                long chunk = Math.Min(GoldMax, totalGold);
                goldChunks.Add(chunk);
                totalGold -= chunk;
            }
        }

        int[] baseOffsets = new int[8];
        for (int j = 0; j < 8 && j < goldChunks.Count; j++)
        {
            baseOffsets[j] = (j - 5) * 15;
            int goldDrop = (int)Math.Min(GoldMax, goldChunks[j]);
            DropGoldItem(baseOffsets[j], locationY, goldDrop);
        }

        int extraCount = goldChunks.Count - 8;
        if (extraCount > 0)
        {
            int leftCount = extraCount / 2;
            int rightCount = extraCount - leftCount;
            int leftStart = baseOffsets[0] - 15;
            for (int j = 0; j < leftCount; j++)
            {
                int idx = 8 + j;
                int goldDrop = (int)Math.Min(GoldMax, goldChunks[idx]);
                DropGoldItem(leftStart - (j * 15), locationY, goldDrop);
            }

            int rightStart = baseOffsets[7] + 15;
            for (int j = 0; j < rightCount; j++)
            {
                int idx = 8 + leftCount + j;
                int goldDrop = (int)Math.Min(GoldMax, goldChunks[idx]);
                DropGoldItem(rightStart + (j * 15), locationY, goldDrop);
            }
        }
    }

    private void DropGoldItem(int offsetX, int offsetY, int goldAmount)
    {
        ItemMap item = new(Z, 190, goldAmount, Location.X + offsetX, offsetY, -1);
        Rt.World.DropItemMap(Z, item);
    }

    // Chọn vật phẩm nhiệm vụ có thể rơi; switch Java không break giữa sáu mob mẹ nên giết mob N còn chạy các khối sau, mỗi khối chỉ tác động nhiệm vụ của nó và vật phẩm tạo trước vẫn nằm đất khi khối sau thay biến trả về; giữ nguyên fall-through này. (Java: dropItemTask)
    private ItemMap? DropItemTask(Player player)
    {
        ItemMap? itemMap = null;
        int taskId = Rt.World.GetTaskId(player);
        if (TempId == ConstMob.DINOSAUR || TempId == ConstMob.WILD_BOAR || TempId == ConstMob.EARTH_DEMON)
        {
            if (taskId == ConstTask.TASK_2_0)
            {
                itemMap = new ItemMap(Z, 73, 1, Location.X, Location.Y, player.Id);
            }

            return itemMap;
        }

        int start = TempId switch
        {
            ConstMob.LIZARD_MOTHER => 0,
            ConstMob.FLYING_DRAGON_MOTHER => 1,
            ConstMob.FLYING_DEMON_MOTHER => 2,
            ConstMob.HERMIT_CRAB => 3,
            ConstMob.SAIYAN_PIG_MOTHER => 4,
            ConstMob.SNAIL => 5,
            _ => -1,
        };
        if (start < 0)
        {
            return null;
        }

        (int Task, int Den, int ItemId, string Text)[] blocks =
        [
            (ConstTask.TASK_8_1, 5, 20, "Con thằn lằn mẹ này không giữ ngọc, hãy tìm con thằn lằn mẹ khác"),
            (ConstTask.TASK_8_1, 5, 20, "Con phi long mẹ này không giữ ngọc, hãy tìm con phi long mẹ khác"),
            (ConstTask.TASK_8_1, 5, 20, "Con quỷ bay mẹ này không giữ ngọc, hãy tìm con quỷ bay mẹ khác"),
            (ConstTask.TASK_14_1, 4, 85, "Con ốc mượn hồn này không giữ truyện tranh, hãy thử tìm con ốc mượn hồn khác"),
            (ConstTask.TASK_14_1, 4, 85, "Con heo xayda mẹ này không giữ truyện tranh, hãy thử tìm con heo xayda mẹ khác"),
            (ConstTask.TASK_14_1, 4, 85, "Con ốc xên này không giữ truyện tranh, hãy thử tìm con ốc xên khác"),
        ];
        for (int b = start; b < blocks.Length; b++)
        {
            (int task, int den, int itemId, string text) = blocks[b];
            if (Rt.World.GetTaskId(player) == task)
            {
                if (Rt.IsTrue(1, den))
                {
                    itemMap = new ItemMap(Z, itemId, 1, Location.X, Location.Y, player.Id);
                }
                else
                {
                    Rt.World.SendNotification(player, text);
                }
            }
        }

        return itemMap;
    }
}
