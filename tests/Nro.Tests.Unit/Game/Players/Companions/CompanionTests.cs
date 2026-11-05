using Nro.Game.Players;
using Nro.Game.Players.Companions;
using Nro.Game.Players.Ports;

namespace Nro.Tests.Unit.Game.Players.Companions;

// Đặc tả thú đi theo và Saibamen qua clock/random/gateway xác định.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/PetFollow.java (dòng 12–114), nro/player/SaiBaMen.java (dòng 14–120).
public sealed class CompanionTests
{
    // Pet giữ ngoại hình ctor và sinh id âm giảm dần (Java: PetFollow dòng 17–25, 28–38).
    [Fact]
    public void Pet_follow_uses_fixed_appearance_and_decreasing_ids()
    {
        int originalId = PetFollow.NextId;
        try
        {
            PetFollow.NextId = -310_720_020;
            var gateway = new FakeCompanionGateway();
            Player owner = AlivePlayer(100, 200);
            var first = new PetFollow(owner, 10, 11, 12, gateway);
            var second = new PetFollow(owner, 20, 21, 22, gateway);

            Assert.Equal(-310_720_020, first.Id);
            Assert.Equal(-310_720_021, second.Id);
            Player viewedAsPlayer = first;
            Assert.Equal((short)10, viewedAsPlayer.GetHead());
            Assert.Equal((short)11, viewedAsPlayer.GetBody());
            Assert.Equal((short)12, viewedAsPlayer.GetLeg());
        }
        finally
        {
            PetFollow.NextId = originalId;
        }
    }

    // Pet idle dùng biên khoảng cách Java và chỉ di chuyển sau mốc strict (Java: moveIdle dòng 56–65).
    [Fact]
    public void Pet_follow_idles_in_java_range_after_strict_interval()
    {
        var gateway = new FakeCompanionGateway();
        Player owner = AlivePlayer(100, 200);
        var pet = new PetFollow(owner, 1, 2, 3, gateway) { IsIdle = true };
        pet.Location.X = 90;
        pet.Location.Y = 200;
        pet.NPoint.Hp = 1;

        pet.Update(Context(1, new FixedRandom(30, 5_000)), gateway);
        pet.Update(Context(5_001, new FixedRandom(50, 8_000)), gateway);
        pet.Update(Context(5_002, new FixedRandom(30, 8_000)), gateway);

        // Java PetFollow.java dòng 62–64: master.location.x + Util.nextInt(dir == -1 ? 30 : -50, dir == -1 ? 50 : 30).
        Assert.Equal([(130, 200), (130, 200)], gateway.Moves);
    }

    // Pet follow đặt cạnh chủ theo khoảng Java 12..12+distance và phát di chuyển (Java: followMaster dòng 92–105).
    [Fact]
    public void Pet_follow_uses_random_side_and_distance()
    {
        var gateway = new FakeCompanionGateway();
        Player owner = AlivePlayer(100, 200);
        var pet = new PetFollow(owner, 1, 2, 3, gateway);

        pet.FollowMaster(new FixedRandom(0, 0), gateway);

        Assert.Equal((112, 200), Assert.Single(gateway.Moves));
    }

    // Pet hồi sinh, rồi join Zone chủ khi gateway xác nhận biểu thức map Java (Java: update dòng 68–87).
    [Fact]
    public void Pet_follow_respawns_and_joins_owner_zone()
    {
        var gateway = new FakeCompanionGateway { MayJoinPetZone = true };
        Player owner = AlivePlayer(100, 200);
        owner.Zone = new Nro.Game.World.Maps.Zone(null!, 0, 1);
        var pet = new PetFollow(owner, 1, 2, 3, gateway);
        pet.NPoint.Hp = 0;
        pet.NPoint.HpMax = 40;
        pet.NPoint.MpMax = 50;

        pet.Update(Context(1, new FixedRandom(-10)), gateway);

        Assert.Equal((40L, 50L), Assert.Single(gateway.Respawns));
        Assert.Same(owner, Assert.Single(gateway.JoinedOwners));
        Assert.Equal((90, 200), (pet.Location.X, pet.Location.Y));
    }

    // Player.Update gọi implementation PetFollow trực tiếp khi Zone đã ghép CompanionGateway (Java: Player.update gọi PetFollow.update).
    [Fact]
    public void Player_lifecycle_dispatches_pet_follow_through_companion_gateway()
    {
        var gateway = new FakeCompanionGateway();
        Player owner = AlivePlayer(100, 200);
        owner.IsPlayer = false;
        var pet = new PetFollow(owner, 1, 2, 3, gateway);
        owner.PetFollow = pet;
        pet.NPoint.Hp = 0;
        pet.NPoint.HpMax = 40;
        pet.NPoint.MpMax = 50;

        owner.Update(Context(1, new FixedRandom(), gateway));

        Assert.Equal((40L, 50L), Assert.Single(gateway.Respawns));
    }

    // Saibamen clone đúng nhóm chỉ số, đặt full HP/KI và giữ ngoại hình 642–644 (Java: ctor, getHead/getBody/getLeg).
    [Fact]
    public void Sai_ba_men_clones_stats_and_fixed_appearance()
    {
        var gateway = new FakeCompanionGateway();
        Player owner = AlivePlayer(100, 200);
        owner.NPoint.BaseHp = 100;
        owner.NPoint.BaseMp = 200;
        owner.NPoint.BaseDamage = 300;
        owner.NPoint.Hp = 31;
        owner.NPoint.Mp = 32;
        owner.NPoint.Damage = 33;
        owner.NPoint.HpMax = 400;
        owner.NPoint.MpMax = 500;
        owner.NPoint.Power = 600;
        owner.NPoint.Potential = 700;
        owner.NPoint.Stamina = 80;
        owner.NPoint.MaxStamina = 90;
        var saibamen = new SaiBaMen(owner, new FixedRandom(1_700_000_123), gateway);

        Assert.Equal(1_700_000_123, saibamen.Id);
        Assert.Equal("Saibamen", saibamen.Name);
        Assert.Equal((sbyte)2, saibamen.Gender);
        Assert.Equal(100, saibamen.NPoint.BaseHp);
        Assert.Equal(200, saibamen.NPoint.BaseMp);
        Assert.Equal(300, saibamen.NPoint.BaseDamage);
        Assert.Equal(600, saibamen.NPoint.Power);
        Assert.Equal(700, saibamen.NPoint.Potential);
        Assert.Equal(80, saibamen.NPoint.Stamina);
        Assert.Equal(90, saibamen.NPoint.MaxStamina);
        Assert.Equal(saibamen.NPoint.HpMax, saibamen.NPoint.Hp);
        Assert.Equal(saibamen.NPoint.MpMax, saibamen.NPoint.Mp);
        Player viewedAsPlayer = saibamen;
        Assert.Equal((short)642, viewedAsPlayer.GetHead());
        Assert.Equal((short)643, viewedAsPlayer.GetBody());
        Assert.Equal((short)644, viewedAsPlayer.GetLeg());
    }

    // Saibamen nổ sau hơn 2,5 giây, không nổ ở đúng 2,5 giây và AOE hoàn toàn qua gateway (Java: setBom dòng 74–112).
    [Fact]
    public void Sai_ba_men_self_destructs_after_strict_java_delay()
    {
        var gateway = new FakeCompanionGateway();
        Player owner = AlivePlayer(100, 200);
        owner.Zone = new Nro.Game.World.Maps.Zone(null!, 0, 1);
        var saibamen = new SaiBaMen(owner, new FixedRandom(1_700_000_000), gateway);
        saibamen.NPoint.HpMax = 999;

        saibamen.JoinMap(new FixedClock(0), gateway);
        saibamen.Update(Context(2_500, new FixedRandom()), gateway);
        saibamen.Update(Context(2_501, new FixedRandom()), gateway);

        Assert.Equal([(104, (short)2_000)], gateway.SelfDestructEffects);
        Assert.Equal([(saibamen, 999d)], gateway.Explosions);
        Assert.False(saibamen.PlayerSkill.PrepareTuSat);
    }

    // Dispose companion thoát map và gỡ liên kết Owner Java; Saibamen còn gỡ owner.Saibamen (Java: dispose).
    [Fact]
    public void Companion_dispose_exits_map_and_unlinks_owner()
    {
        var gateway = new FakeCompanionGateway();
        Player owner = AlivePlayer(100, 200);
        var pet = new PetFollow(owner, 1, 2, 3, gateway);
        var saibamen = new SaiBaMen(owner, new FixedRandom(1_700_000_000), gateway);
        owner.Saibamen = saibamen;

        pet.Dispose(gateway);
        saibamen.Dispose(gateway);

        Assert.Null(pet.Owner);
        Assert.Null(saibamen.Owner);
        Assert.Null(owner.Saibamen);
        Assert.Equal([pet, saibamen], gateway.Exited);
    }

    // Player.Dispose nhận diện companion đa hình để không bỏ qua exit map và unlink (Java: Player.dispose gọi companion.dispose).
    [Fact]
    public void Player_dispose_uses_companion_lifecycle()
    {
        var gateway = new FakeCompanionGateway();
        Player owner = AlivePlayer(100, 200);
        var pet = new PetFollow(owner, 1, 2, 3, gateway);
        var saibamen = new SaiBaMen(owner, new FixedRandom(1_700_000_000), gateway);
        owner.PetFollow = pet;
        owner.Saibamen = saibamen;

        owner.Dispose();

        Assert.True(owner.IsDisposed);
        Assert.True(pet.IsDisposed);
        Assert.True(saibamen.IsDisposed);
        Assert.Equal([pet, saibamen], gateway.Exited);
    }

    // Dựng Player sống tối thiểu cho companion (Java: Player ctor, nPoint).
    private static Player AlivePlayer(int x, int y)
    {
        var player = new Player { IsPlayer = true };
        player.Location.X = x;
        player.Location.Y = y;
        player.NPoint.Hp = 1;
        player.NPoint.HpMax = 1;
        return player;
    }

    // Dựng ngữ cảnh tick không dùng thời gian tường hay random toàn cục (Java: update).
    private static PlayerRuntimeContext Context(long now, IPlayerRandom random, IPlayerCompanionGateway? gateway = null) => new()
    {
        Clock = new FixedClock(now),
        Random = random,
        Services = new NoOpPlayerRuntimeServices(),
        CompanionGateway = gateway,
    };

    // Đồng hồ cố định để đối chiếu mốc strict Java (Java: System.currentTimeMillis).
    private sealed class FixedClock(long now) : IPlayerClock
    {
        // Trả epoch mili giây fixture (Java: System.currentTimeMillis).
        public long NowMilliseconds => now;
    }

    // Adapter trống cho ngữ cảnh companion; PlayerRuntimeServices là abstract base adapter.
    private sealed class NoOpPlayerRuntimeServices : PlayerRuntimeServices
    {
    }

    // Nguồn random theo kịch bản, trả cận dưới khi dữ liệu fixture đã hết (Java: Util.nextInt, Math.random).
    private sealed class FixedRandom(params int[] values) : IPlayerRandom
    {
        // Trả giá trị fixture và xác nhận nằm trong biên Java yêu cầu (Java: Util.nextInt).
        public int NextInt(int minInclusive, int maxInclusive)
        {
            int value = values.Length == 0 ? minInclusive : values[Math.Min(callCount++, values.Length - 1)];
            Assert.InRange(value, minInclusive, maxInclusive);
            return value;
        }

        // Chỉ số giá trị fixture kế tiếp (Java: Util.nextInt).
        private int callCount;
    }

    // Gateway ghi side effect map/combat để kiểm tra companion không gọi singleton Java.
    private sealed class FakeCompanionGateway : IPlayerCompanionGateway
    {
        // Cờ điều khiển join thú (Java: điều kiện loại map PetFollow.update).
        public bool MayJoinPetZone { get; set; }

        // Các owner đã được companion join (Java: goToMap/load_Me_To_Another).
        public List<Player> JoinedOwners { get; } = [];

        // Các lần companion thoát map (Java: exitMap).
        public List<Player> Exited { get; } = [];

        // Các lần di chuyển phát map (Java: playerMove).
        public List<(int X, int Y)> Moves { get; } = [];

        // Các lần hồi sinh (Java: hsChar).
        public List<(long Hp, long Mp)> Respawns { get; } = [];

        // Các hiệu ứng tự hủy đã phát (Java: Message -45).
        public List<(short Effect, short Duration)> SelfDestructEffects { get; } = [];

        // Các AOE đã handoff (Java: setDie + injured).
        public List<(SaiBaMen SaiBaMen, double Damage)> Explosions { get; } = [];

        // Trả fixture điều kiện join thú (Java: PetFollow.update).
        public bool CanJoinPetFollowMasterZone(Player master) => MayJoinPetZone;

        // Ghi nhận join map (Java: ChangeMapService.goToMap, Zone.load_Me_To_Another).
        public void JoinMasterZone(Player companion, Player master) => JoinedOwners.Add(master);

        // Ghi nhận exit map (Java: ChangeMapService.exitMap).
        public void ExitMap(Player companion) => Exited.Add(companion);

        // Ghi nhận di chuyển map (Java: PlayerService.playerMove).
        public void Move(Player companion, int x, int y) => Moves.Add((x, y));

        // Ghi nhận hồi sinh (Java: Service.hsChar).
        public void Respawn(Player companion, long hp, long mp) => Respawns.Add((hp, mp));

        // Ghi nhận gói hoạt ảnh tự hủy (Java: Message -45).
        public void BroadcastSaiBaMenSelfDestruct(SaiBaMen saibamen, short effectId, short durationMilliseconds) => SelfDestructEffects.Add((effectId, durationMilliseconds));

        // Ghi nhận AOE; Zone/Combat thật sẽ xử lý chết, injured và packet tại một boundary (Java: setDie, injured).
        public void ExplodeSaiBaMen(SaiBaMen saibamen, double damage) => Explosions.Add((saibamen, damage));
    }
}
