using Nro.Core.Consts;
using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Game.World.Maps;

namespace Nro.Tests.Unit.Game.Players;

// Đặc tả nhánh Player di chuyển, cổng map và hồi sinh.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 2480–2729).
public sealed class PlayerMovementTests
{
    // Vượt biên map ngọc đen không ở cửa chuyển sẽ reset, trừ HP và gửi HP (Java: fixBlackBallWar).
    [Fact]
    public void Black_ball_boundary_resets_and_reports_hp()
    {
        Player player = NewPlayer();
        var world = new FakeWorld { MapId = 85, IsBlackBallWar = true, X = 23, Y = 50 };

        player.FixBlackBallWar(world);

        Assert.Equal([(23, 100)], world.BlackBallResets);
        Assert.Equal(1, world.BlackBallPenalties);
        Assert.Equal(1, world.InfoHpCount);
    }

    // Cả hai overload move ghi vị trí mới và luôn phát vị trí, kể cả khi tọa độ không đổi (Java: move).
    [Fact]
    public void Move_overloads_update_position_and_broadcast()
    {
        Player player = NewPlayer();
        Player other = NewPlayer();
        var world = new FakeWorld { X = 10, Y = 20 };

        player.Move(other, 30, 40, world);
        player.Move(30, 40, world);

        Assert.Equal((30, 40), (world.X, world.Y));
        Assert.Equal([other, player], world.Moves);
    }

    // Phạt thanh toán giữ ErrorPay của receiver Java, dịch chuyển với random và tịch thu qua cổng (Java: Check_Error_Pay).
    [Fact]
    public void Payment_error_penalty_uses_gateway_and_receiver_counter()
    {
        Player receiver = NewPlayer();
        receiver.ErrorPay = 2;
        Player target = NewPlayer();
        var world = new FakeWorld();

        receiver.CheckErrorPay(target, world, new FixedRandom(345));

        Assert.Equal((target, 21, -1, 345, 300), Assert.Single(world.MapChanges));
        Assert.Equal((target, 2_000_000_000L, 500), Assert.Single(world.CurrencyConfiscations));
        Assert.Equal(1, world.MoneyCount);
        Assert.Contains("2 / 3", Assert.Single(world.BigMessages).Text);
    }

    // Tới đúng năm lỗi map thì gửi cảnh báo và tín hiệu khóa; mọi số khác không làm gì (Java: UpdateBandAcc).
    [Theory]
    [InlineData((sbyte)5, true)]
    [InlineData((sbyte)4, false)]
    public void Banned_account_threshold_is_exact(sbyte errorMap, bool expectedBan)
    {
        Player player = NewPlayer();
        player.ErrorMap = errorMap;
        var world = new FakeWorld();

        player.UpdateBannedAccount(world);

        Assert.Equal(expectedBan ? 1 : 0, world.BanCount);
        Assert.Equal(expectedBan ? 1 : 0, world.Notifications.Count);
    }

    // Sáu cổng map Java phạt đúng khi ngưỡng power/task chưa đạt (Java: checkPlayerInMap).
    [Theory]
    [InlineData(MapGate.NguHanhSon)]
    [InlineData(MapGate.Cereal)]
    [InlineData(MapGate.PrisonPlanet)]
    [InlineData(MapGate.ClanTerritory)]
    [InlineData(MapGate.Future)]
    [InlineData(MapGate.Cold)]
    public void Map_entry_gate_penalizes_when_requirement_is_not_met(MapGate gate)
    {
        Player player = NewPlayerWithZone();
        ConfigureFailedGate(player, gate);
        var world = new FakeWorld { Gate = gate };

        player.CheckPlayerInMap(world);

        Assert.Single(world.HomeReturns);
        Assert.Equal((player, 2_000_000_000L, 0), Assert.Single(world.CurrencyConfiscations));
        Assert.Equal((sbyte)1, player.ErrorMap);
    }

    // Sáu cổng map Java bỏ qua người chơi đạt đủ ngưỡng power/task (Java: checkPlayerInMap).
    [Theory]
    [InlineData(MapGate.NguHanhSon)]
    [InlineData(MapGate.Cereal)]
    [InlineData(MapGate.PrisonPlanet)]
    [InlineData(MapGate.ClanTerritory)]
    [InlineData(MapGate.Future)]
    [InlineData(MapGate.Cold)]
    public void Map_entry_gate_allows_when_requirement_is_met(MapGate gate)
    {
        Player player = NewPlayerWithZone();
        ConfigurePassingGate(player, gate);
        var world = new FakeWorld { Gate = gate, TaskId = PassingTaskId(gate) };

        player.CheckPlayerInMap(world);

        Assert.Empty(world.HomeReturns);
        Assert.Empty(world.CurrencyConfiscations);
        Assert.Equal((sbyte)0, player.ErrorMap);
    }

    // Ngoài vùng map thường bị phạt, còn trong vùng thì giữ nguyên (Java: checkPlayerLocation).
    [Theory]
    [InlineData(101, 50, true)]
    [InlineData(100, 100, false)]
    public void Player_location_penalty_applies_only_outside_bounds(int x, int y, bool expectedPenalty)
    {
        Player player = NewPlayerWithZone();
        var world = new FakeWorld { X = x, Y = y, MapWidth = 100, MapHeight = 100 };

        player.CheckPlayerLocation(world);

        Assert.Equal(expectedPenalty ? 1 : 0, world.HomeReturns.Count);
        Assert.Equal(expectedPenalty ? (sbyte)1 : (sbyte)0, player.ErrorLocation);
    }

    // Snapshot bang chỉ được phát khi có ClanMember, không ghi trực tiếp placeholder (Java: setClanMember).
    [Fact]
    public void Clan_member_snapshot_is_sent_only_for_existing_member()
    {
        Player player = NewPlayer();
        player.NPoint.Power = 123;
        var world = new FakeWorld();

        player.SetClanMember(world);
        Assert.Empty(world.ClanSnapshots);

        player.ClanMember = new Nro.Game.Social.ClanMember();
        player.SetClanMember(world);
        Assert.Equal((player, 123L), Assert.Single(world.ClanSnapshots));
    }

    // Mỗi vai trò gia đình chỉ đánh dấu đúng cờ và thời điểm tương ứng (Java: setJustRevivaled).
    [Theory]
    [InlineData(FamilyRole.Pet)]
    [InlineData(FamilyRole.Father)]
    [InlineData(FamilyRole.Mother)]
    [InlineData(FamilyRole.Lover)]
    [InlineData(FamilyRole.ChildOne)]
    [InlineData(FamilyRole.ChildTwo)]
    [InlineData(FamilyRole.ChildThree)]
    public void Revival_marks_the_matching_family_role(FamilyRole role)
    {
        Player player = NewPlayer();
        switch (role)
        {
            case FamilyRole.Pet: player.IsPet = true; break;
            case FamilyRole.Father: player.IsFather = true; break;
            case FamilyRole.Mother: player.IsMother = true; break;
            case FamilyRole.Lover: player.IsLover = true; break;
            case FamilyRole.ChildOne: player.IsChildOne = true; break;
            case FamilyRole.ChildTwo: player.IsChildTwo = true; break;
            case FamilyRole.ChildThree: player.IsChildThree = true; break;
        }

        player.SetJustRevivaled(new FixedClock(456));

        bool revived = role switch
        {
            FamilyRole.Pet => player.JustRevived,
            FamilyRole.Father => player.FatherJustRevived,
            FamilyRole.Mother => player.MotherJustRevived,
            FamilyRole.Lover => player.LoverJustRevived,
            FamilyRole.ChildOne => player.ChildOneJustRevived,
            FamilyRole.ChildTwo => player.ChildTwoJustRevived,
            _ => player.ChildThreeJustRevived,
        };
        long revivedAt = role switch
        {
            FamilyRole.Pet => player.LastTimeRevived,
            FamilyRole.Father => player.LastFatherRevivedTime,
            FamilyRole.Mother => player.LastMotherRevivedTime,
            FamilyRole.Lover => player.LastLoverRevivedTime,
            FamilyRole.ChildOne => player.LastChildOneRevivedTime,
            FamilyRole.ChildTwo => player.LastChildTwoRevivedTime,
            _ => player.LastChildThreeRevivedTime,
        };
        Assert.True(revived);
        Assert.Equal(456, revivedAt);
    }

    // Bom luôn handoff chết, nhưng gọi Sai Ba Men chỉ sau hơn 5 giây (Java: setBom).
    [Fact]
    public void Bomb_death_and_sai_ba_men_keep_strict_java_interval()
    {
        Player player = NewPlayer();
        var world = new FakeWorld();
        var combat = new FakeCombat();

        player.SetBom(null, combat, world, new FixedClock(5_000));
        player.SetBom(null, combat, world, new FixedClock(5_001));
        player.SetBom(null, combat, world, new FixedClock(10_001));

        Assert.Equal(3, combat.DeathCount);
        Assert.Equal(1, world.SaiBaMenCount);
    }

    // Không có session phải trả false thay vì NullPointerException Java (Java: isQuanTriVien).
    [Fact]
    public void Moderator_check_is_null_safe()
    {
        Assert.False(NewPlayer().IsQuanTriVien());
    }

    private static Player NewPlayer() => new() { IsPlayer = true };

    private static Player NewPlayerWithZone()
    {
        Player player = NewPlayer();
        player.Zone = new Zone(null!, 0, 1);
        return player;
    }

    private static void ConfigureFailedGate(Player player, MapGate gate)
    {
        player.NPoint.Power = gate switch
        {
            MapGate.NguHanhSon => 39_999_999_999,
            MapGate.Cereal => 29_999_999_999,
            MapGate.PrisonPlanet => 59_999_999_999,
            MapGate.ClanTerritory => 39_999_999_999,
            _ => 0,
        };
        player.PlayerTask = new Nro.Game.Progress.Tasks.TaskPlayer();
    }

    private static void ConfigurePassingGate(Player player, MapGate gate)
    {
        player.NPoint.Power = gate switch
        {
            MapGate.NguHanhSon or MapGate.ClanTerritory => 40_000_000_000,
            MapGate.Cereal => 30_000_000_000,
            MapGate.PrisonPlanet => 60_000_000_000,
            _ => 0,
        };
        player.PlayerTask = new Nro.Game.Progress.Tasks.TaskPlayer();
    }

    private static int PassingTaskId(MapGate gate) => gate switch
    {
        MapGate.Future => ConstTask.TASK_22_0,
        MapGate.Cold => ConstTask.TASK_25_0,
        _ => 0,
    };

    public enum MapGate { NguHanhSon, Cereal, PrisonPlanet, ClanTerritory, Future, Cold }

    public enum FamilyRole { Pet, Father, Mother, Lover, ChildOne, ChildTwo, ChildThree }

    private sealed class FixedClock(long now) : IPlayerClock
    {
        public long NowMilliseconds => now;
    }

    private sealed class FixedRandom(int result) : IPlayerRandom
    {
        public int NextInt(int minInclusive, int maxInclusive)
        {
            Assert.Equal(200, minInclusive);
            Assert.Equal(600, maxInclusive);
            return result;
        }
    }

    private sealed class FakeWorld : IPlayerWorldGateway
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int MapId { get; set; } = 1;
        public int MapWidth { get; set; } = 1_000;
        public int MapHeight { get; set; } = 500;
        public bool IsBlackBallWar { get; set; }
        public MapGate? Gate { get; set; }
        public int TaskId { get; set; }
        public int BlackBallPenalties { get; private set; }
        public int InfoHpCount { get; private set; }
        public int MoneyCount { get; private set; }
        public int BanCount { get; private set; }
        public int SaiBaMenCount { get; private set; }
        public List<(int X, int Y)> BlackBallResets { get; } = [];
        public List<Player> Moves { get; } = [];
        public List<(Player Player, int MapId, int ZoneId, int X, int Y)> MapChanges { get; } = [];
        public List<(Player Player, int MapId, int X, int Y)> HomeReturns { get; } = [];
        public List<(Player Player, long Gold, int Ruby)> CurrencyConfiscations { get; } = [];
        public List<(int MessageId, string Text)> BigMessages { get; } = [];
        public List<string> Notifications { get; } = [];
        public List<(Player Player, long Power)> ClanSnapshots { get; } = [];

        public void GetPosition(Player player, out int x, out int y) => (x, y) = (X, Y);
        public void SetPosition(Player player, int x, int y) => (X, Y) = (x, y);
        public void SendPlayerMove(Player player) => Moves.Add(player);
        public bool TryGetMapState(Player player, out int mapId, out int mapWidth, out int mapHeight)
        {
            (mapId, mapWidth, mapHeight) = (MapId, MapWidth, MapHeight);
            return true;
        }
        public int GetYPhysicInTop(Player player, int x, int y) => y;
        public bool IsBlackBallWarMap(int mapId) => IsBlackBallWar;
        public bool HasWaypointPlayerIn(Player player) => false;
        public void ResetBlackBallPosition(Player player, int x, int y) => BlackBallResets.Add((x, y));
        public void ApplyBlackBallBoundaryPenalty(Player player) => BlackBallPenalties++;
        public void SendInfoHp(Player player) => InfoHpCount++;
        public void ChangeMap(Player player, int mapId, int zoneId, int x, int y) => MapChanges.Add((player, mapId, zoneId, x, y));
        public void ReturnPlayerHome(Player player, int mapId, int x, int y) => HomeReturns.Add((player, mapId, x, y));
        public bool IsNguHanhSonMap(int mapId) => Gate == MapGate.NguHanhSon;
        public bool IsCerealMap(int mapId) => Gate == MapGate.Cereal;
        public bool IsPrisonPlanetMap(int mapId) => Gate == MapGate.PrisonPlanet;
        public bool IsClanTerritoryMap(int mapId) => Gate == MapGate.ClanTerritory;
        public bool IsFutureMap(int mapId) => Gate == MapGate.Future;
        public bool IsColdMap(int mapId) => Gate == MapGate.Cold;
        public bool IsDestronGasMap(int mapId) => false;
        public int GetTaskId(Player player) => Gate switch
        {
            MapGate.Future => TaskId == 0 ? ConstTask.TASK_22_0 - 1 : TaskId,
            MapGate.Cold => TaskId == 0 ? ConstTask.TASK_25_0 - 1 : TaskId,
            _ => TaskId,
        };
        public void ConfiscateCurrency(Player player, long gold, int ruby) => CurrencyConfiscations.Add((player, gold, ruby));
        public void SendMoney(Player player) => MoneyCount++;
        public void SendInfoHpMpMoney(Player player) { }
        public void SendNotification(Player player, string text) => Notifications.Add(text);
        public void SendBigMessage(Player player, int messageId, string text) => BigMessages.Add((messageId, text));
        public void BanAccount(Player player) => BanCount++;
        public void UpdateClanMemberSnapshot(Player player, long power, short head, short body, short leg) => ClanSnapshots.Add((player, power));
        public void CallSaiBaMen(Player player) => SaiBaMenCount++;
    }

    private sealed class FakeCombat : IPlayerCombatGateway
    {
        public int DeathCount { get; private set; }
        public bool ReadInt => true;
        public bool IsHalloweenEvent => false;
        public void SetHalloween(Player player, int outfitId, int durationMilliseconds) { }
        public bool IsMabuTwelveHourMap(Player player) => false;
        public void RestoreHp(Player player, long hp) { }
        public bool IsBlastSkill(Player player) => false;
        public void BreakShield(Player player) { }
        public void SetShieldReflectDamage(Player player, long damage) { }
        public bool IsImmortalCharmActive(Player player, long nowMilliseconds) => false;
        public bool IsMabuHoldMap(Player player) => false;
        public bool AreSameClan(Player first, Player second) => false;
        public void SendPrivateChat(Player sender, Player receiver, string text) { }
        public bool IsDungeonMap(Player player) => false;
        public void SendNotification(Player player, string text) { }
        public void RecordDamageTakenOnMap51(Player player, double damage) { }
        public bool HandleMartialArtCongressDeath(Player player) => false;
        public bool HasFriezaLeaderCostume(Player player) => false;
        public bool HasFriezaMinionCostume(Player player) => false;
        public int GetBrolyLevel(Player player) => 0;
        public bool CanDropMonkeyTail(Player victim, Player attacker) => false;
        public void DropCombatItem(Player victim, int itemTemplateId, int quantity, Player owner) { }
        public void MarkTailDropTimeDirty(Player player) { }
        public bool CanDropDeathItems(Player player) => false;
        public void TriggerBombDeath(Player victim, Player? attacker) { }
        public void TriggerPlayerDeath(Player victim, Player? attacker) => DeathCount++;
    }
}
