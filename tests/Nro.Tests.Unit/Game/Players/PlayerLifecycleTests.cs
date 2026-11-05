using Nro.Core.Templates;
using Nro.Game.Items.Core;
using Nro.Game.Players;
using Nro.Game.Players.Ports;
using Nro.Game.Social;
using Nro.Game.World.Maps;
using Nro.Tests.Unit.Game.World;

namespace Nro.Tests.Unit.Game.Players;

// Characterization vòng đời Player qua clock/random/runtime fake; chưa có golden Java vì Player.java không chạy độc lập.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 861–1458).
public sealed class PlayerLifecycleTests
{
    // Kiểm tra chuỗi đầu tick Java và hai lần gọi Mabư 12 giờ theo đúng vị trí (Java: update 1071–1079).
    [Fact]
    public void Update_preserves_component_order_and_calls_mabu_twelve_hour_twice_in_java_order()
    {
        Player player = MakePlayer(19);
        LifecycleServices services = new();

        player.Update(Context(20_001, services));

        // Java Player.java 865–875: các component đầu tick giữ nguyên thứ tự.
        Assert.Equal(["item-time", "npoint", "fusion", "effect-skill", "effect-skin", "satellite", "drop-items"], services.Calls.Take(7));
        // Java Player.java 1073 và 1078: MajinBuu12H.update được gọi đúng hai lần.
        Assert.Equal(2, services.Calls.Count(call => call == "mabu12"));
        // Java Player.java 1072–1074: SuperDivineWater, Mabư, rồi Quá Khứ.
        Assert.True(services.Calls.IndexOf("super-water") < services.Calls.IndexOf("mabu12"));
        // Java Player.java 1073–1074: lần Mabư đầu đứng trước QuaKhu.
        Assert.True(services.Calls.IndexOf("mabu12") < services.Calls.IndexOf("past"));
        // Java Player.java 1078: lần Mabư thứ hai đứng sau SendMabu14H (và sau QuaKhu).
        Assert.True(services.Calls.FindLastIndex(call => call == "mabu12") > services.Calls.IndexOf("past"));
        // Java Player.java 1073/1078: cả hai handoff nhận đúng Player đang update.
        Assert.All(services.MabuTwelveHourPlayers, updated => Assert.Same(player, updated));
    }

    // Kiểm tra hạn dùng quét hơn một giây, giữ OR Java và xóa qua cổng runtime (Java: UpdateOptionItem).
    [Fact]
    public void Update_expires_item_options_after_one_second_using_java_or_condition()
    {
        Player player = MakePlayer(19);
        Item item = new() { Template = new ItemTemplate { Id = 1, Name = "Hết hạn" } };
        item.ItemOptions.Add(new ItemOption(new ItemOptionTemplate(260, "", 0), 1));
        player.Inventory.ItemsBag.Add(item);
        LifecycleServices services = new() { ExpireItems = true };

        player.Update(Context(1_001, services));

        Assert.Contains("load-item", services.Calls);
        Assert.Contains("remove-item", services.Calls);
        Assert.Empty(player.Inventory.ItemsBag);
    }

    // Kiểm tra hai thành viên cùng bang gần nhau bật ba hệ số và đồng bộ point (Java: UpdateOptionClan).
    [Fact]
    public void Update_applies_clan_bonuses_when_two_members_are_nearby()
    {
        Player player = MakePlayer(19);
        Clan clan = new();
        player.Clan = clan;
        player.Zone!.Players.Add(new Player { Clan = clan });
        player.Zone.Players.Add(new Player { Clan = clan });
        LifecycleServices services = new() { SameClan = true, Distance = 300, ClanBonuses = (4, 5, 6) };

        player.Update(Context(20_001, services));

        Assert.Equal(4, player.ClanHpBonusPercent);
        Assert.Equal(5, player.ClanDamageBonusPercent);
        Assert.Equal(6, player.ClanMpBonusPercent);
        Assert.Contains("point", services.Calls);
    }

    // Java chỉ kiểm task/buff bang bên trong if (this.clan != null) (Java: Player.java 1038–1042).
    [Fact]
    public void Update_does_not_check_clan_task_without_a_clan()
    {
        Player player = MakePlayer(19);
        LifecycleServices services = new();

        player.Update(Context(20_001, services));

        // Java Player.java 1038: clan null không gọi ClanService.checkDoneTaskJoinClan.
        Assert.DoesNotContain("clan-task", services.Calls);
    }

    // Java kiểm task bang khi Player có đúng Clan hợp lệ (Java: Player.java 1038–1042).
    [Fact]
    public void Update_checks_clan_task_when_player_has_a_clan()
    {
        Player player = MakePlayer(19);
        player.Clan = new Clan();
        LifecycleServices services = new();

        player.Update(Context(20_001, services));

        // Java Player.java 1039: clan khác null gọi checkDoneTaskJoinClan.
        Assert.Contains("clan-task", services.Calls);
    }

    // Kiểm tra Chibi hồi KI mỗi hơn một giây và Mabư giữ trừ HP, cả hai qua clock injected (Java: SendPetChiBi, SendMabu14H).
    [Fact]
    public void Update_handles_chibi_regen_and_mabu_hold()
    {
        Player player = MakePlayer(19);
        player.EffectSkill.IsChibi = true;
        player.TypeChibi = 1;
        player.LastTimeUpdateChibi = 0;
        player.NPoint.Mp = 1;
        player.NPoint.MpMax = 100;
        player.EffectSkill.IsMabuHold = true;
        player.NPoint.Hp = 100;
        player.NPoint.HpMax = 100;
        LifecycleServices services = new();

        player.Update(Context(1_001, services));

        Assert.Equal(11, player.NPoint.Mp);
        Assert.Equal(100, player.NPoint.Hp); // 100 / 500 cắt về 0 đúng Java integer division.
        Assert.Contains("info-mp", services.Calls);
        Assert.Contains("info-hp", services.Calls);
    }

    // Java phát packet Chibi dù chỉ số đã đầy; không thêm tối ưu im lặng (Java: Player.java 1348–1368).
    [Theory]
    [InlineData((sbyte)1, "info-mp")]
    [InlineData((sbyte)3, "info-hp")]
    public void Update_chibi_sends_info_packet_when_resource_is_already_full(sbyte typeChibi, string expectedCall)
    {
        Player player = MakePlayer(19);
        player.EffectSkill.IsChibi = true;
        player.TypeChibi = typeChibi;
        player.LastTimeUpdateChibi = 0;
        player.NPoint.Hp = player.NPoint.HpMax = 100;
        player.NPoint.Mp = player.NPoint.MpMax = 100;
        LifecycleServices services = new();

        player.Update(Context(1_001, services));

        // Java Player.java 1357/1366: gửi info-mp/info-hp ngoài điều kiện chỉ số nhỏ hơn max.
        Assert.Contains(expectedCall, services.Calls);
    }

    // Java chỉ thêm Whis khi list > 2 rồi list >= 5, nên mọi rank ở size 0–4 đều im lặng (Java: Player.java 1401–1418).
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 1)]
    [InlineData(4, 2)]
    public void Update_whis_does_not_add_effect_before_five_participants(int participantCount, int rank)
    {
        Player player = MakePlayer(19);
        LifecycleServices services = new() { WhisParticipantCount = participantCount, WhisRank = rank };

        player.Update(Context(20_001, services));

        // Java Player.java 1401–1402: size < 5 không được gọi addEffectChar.
        Assert.DoesNotContain(services.Calls, call => call.StartsWith("whis ", StringComparison.Ordinal));
    }

    // Java map rank 1–3 sang effect 58/57/56 khi list >= 5 (Java: Player.java 1402–1413).
    [Theory]
    [InlineData(1, 58)]
    [InlineData(2, 57)]
    [InlineData(3, 56)]
    public void Update_whis_adds_the_java_rank_effect_at_five_participants(int rank, int effectId)
    {
        Player player = MakePlayer(19);
        LifecycleServices services = new() { WhisParticipantCount = 5, WhisRank = rank };

        player.Update(Context(20_001, services));

        // Java Player.java 1405–1412: hạng 1/2/3 thêm effect 58/57/56.
        Assert.Contains($"whis {effectId}", services.Calls);
    }

    // Bosses rỗng phải return trước khi thông báo tuần lộc (Java: Player.java 1183–1186).
    [Fact]
    public void Update_santa_event_is_silent_when_zone_bosses_is_empty()
    {
        Player player = MakePlayer(19);
        player.CanReward_TuanLoc = true;
        LifecycleServices services = new();

        player.Update(Context(20_001, services));

        // Java Player.java 1184–1185: bosses.isEmpty() return, không sendThongBao.
        Assert.DoesNotContain("santa-notification", services.Calls);
    }

    // Có boss khác nhưng không có Ông Già Noel thì Java vẫn thông báo khi đủ điều kiện (Java: Player.java 1200–1203).
    [Fact]
    public void Update_santa_event_notifies_when_zone_has_non_santa_boss()
    {
        Player player = MakePlayer(19);
        player.CanReward_TuanLoc = true;
        player.Zone!.Bosses.Add(new Player());
        LifecycleServices services = new();

        player.Update(Context(20_001, services));

        // Java Player.java 1201–1203: boss list không rỗng nhưng không có Santa thì sendThongBao.
        Assert.Contains("santa-notification", services.Calls);
    }

    // Handoff PetFollow phải nhận chính Player hiện tại, không chỉ ghi tên method (Java: Player.java 909–910).
    [Fact]
    public void Update_synchronizes_pet_follow_with_the_current_player()
    {
        Player player = MakePlayer(19);
        LifecycleServices services = new();

        player.Update(Context(20_001, services));

        // Java Player.java 909–910: PetFollow.update nhận Player đang tick.
        Assert.Equal([player], services.SynchronizedPetFollowPlayers);
    }

    // Kiểm tra SnakeWay chuyển map sau năm giây theo random injected (Java: UpdatePhoBan).
    [Fact]
    public void Update_moves_completed_snake_way_to_map_144()
    {
        Player player = MakePlayer(47);
        player.JoinCDRD = true;
        player.HasTalkedToKarin = true;
        LifecycleServices services = new() { SnakeWayCompleted = true };

        player.Update(Context(5_001, services));

        Assert.Contains("snake-way 200/312", services.Calls);
        Assert.Equal(5_001, player.TimeChangeMap144);
    }

    // Kiểm tra Player đã Dispose không tạo một side effect nào (Java: beforeDispose/Dispose khác C# có chủ đích).
    [Fact]
    public void Update_does_nothing_after_dispose()
    {
        Player player = MakePlayer(19);
        player.Dispose(new NoopDeathGateway());
        LifecycleServices services = new();

        player.Update(Context(20_001, services));

        Assert.Empty(services.Calls);
    }

    // Kiểm tra guard Zone null tránh dereference zone.map Java khi đây là player thật (Java: update 876).
    [Fact]
    public void Update_guards_a_real_player_with_null_zone()
    {
        Player player = new() { IsPlayer = true };
        LifecycleServices services = new();

        player.Update(Context(20_001, services));

        Assert.Equal(0, services.Errors);
        // Java Player.java dòng 864/880: player thật có Zone null không gọi itemTime.update() hay nhánh zone.
        Assert.Empty(services.Calls);
    }

    // Tạo player thật trong Zone tối thiểu chỉ phục vụ các nhánh lifecycle (Java: player.zone.map).
    private static Player MakePlayer(int mapId)
    {
        var game = new FakeGame();
        var map = new Map(game.Runtime, mapId, "test", 0, 0, 0, 0, 0, [], [], 0, 7, [], [], 0);
        var player = new Player { IsPlayer = true, Zone = new Zone(map, 0, 7) };
        player.Zone.Players.Add(player);
        player.NPoint.Hp = 100;
        player.NPoint.HpMax = 100;
        return player;
    }

    // Tạo context xác định cho một update (Java: System.currentTimeMillis, Util.nextInt).
    private static PlayerRuntimeContext Context(long now, LifecycleServices services) => new()
    {
        Clock = new FixedClock(now),
        Random = new LowestRandom(),
        Services = services,
    };

    // Đồng hồ cố định cho từng case (Java: System.currentTimeMillis).
    private sealed class FixedClock(long now) : IPlayerClock
    {
        // Trả mốc test (Java: System.currentTimeMillis).
        public long NowMilliseconds => now;
    }

    // Random trả cận dưới để xác nhận được các nhánh xác suất Java (Java: Util.isTrue, Util.nextInt).
    private sealed class LowestRandom : IPlayerRandom
    {
        // Trả cận dưới hợp lệ (Java: Util.nextInt).
        public int NextInt(int minInclusive, int maxInclusive) => minInclusive;
    }

    // Runtime fake ghi nhận side effect theo thứ tự Java; các method không override dùng no-op base adapter.
    private sealed class LifecycleServices : PlayerRuntimeServices
    {
        // Nhật ký thứ tự handoff (Java: Service/MapService/ClanService/event singleton).
        public List<string> Calls { get; } = [];

        // Điều khiển item hết hạn (Java: ItemService.isOutOfDateTime).
        public bool ExpireItems { get; set; }

        // Điều khiển so sánh bang (Java: clan.equals).
        public bool SameClan { get; set; }

        // Khoảng cách fixture (Java: Util.getDistance).
        public int Distance { get; set; } = int.MaxValue;

        // Ba hệ số clan fixture (Java: nPoint.tl*Clan).
        public (int Hp, int Damage, int Mp) ClanBonuses { get; set; }

        // Kích thước list Whis fixture (Java: TopKillWhisManager.getList).
        public int WhisParticipantCount { get; set; }

        // Hạng Whis fixture (Java: getPlayerWhis).
        public int WhisRank { get; set; }

        // Các Player truyền cho Mabư 12 giờ (Java: MajinBuu12H.update).
        public List<Player> MabuTwelveHourPlayers { get; } = [];

        // Các Player truyền cho handoff PetFollow (Java: PetFollow.update).
        public List<Player> SynchronizedPetFollowPlayers { get; } = [];

        // Điều khiển SnakeWay hoàn tất (Java: allMobsDead).
        public bool SnakeWayCompleted { get; set; }

        // Số lỗi Player báo ra cổng (Java: Logger.logException).
        public int Errors { get; private set; }

        // Không phải map nhà để đi toàn bộ nhánh update Java (Java: MapService.isHome).
        public override bool IsHomeMap(int mapId) => false;

        // Ghi itemTime trước component khác (Java: itemTime.update).
        public override void UpdateItemTime(Player player) => Calls.Add("item-time");

        // Ghi nPoint update (Java: nPoint.update).
        public override void UpdateNPoint(Player player) => Calls.Add("npoint");

        // Ghi fusion update (Java: fusion.update).
        public override void UpdateFusion(Player player) => Calls.Add("fusion");

        // Ghi effect skill update (Java: effectSkill.update).
        public override void UpdateEffectSkill(Player player) => Calls.Add("effect-skill");

        // Ghi effect skin update (Java: effectSkin.update).
        public override void UpdateEffectSkin(Player player) => Calls.Add("effect-skin");

        // Ghi satellite update (Java: satellite.update).
        public override void UpdateSatellite(Player player) => Calls.Add("satellite");

        // Ghi drop item update (Java: dropItem.update).
        public override void UpdateDropItems(Player player) => Calls.Add("drop-items");

        // Ghi nạp trạng thái hạn dùng (Java: ItemService.loadItemTimeStatus).
        public override void LoadItemTimeStatus(Player player, Item item) => Calls.Add("load-item");

        // Trả fixture hết hạn (Java: ItemService.isOutOfDateTime).
        public override bool IsOutOfDateTime(Item item) => ExpireItems;

        // Xóa item fixture khỏi túi (Java: InventoryService.removeItem).
        public override void RemoveExpiredItem(Player player, Item item)
        {
            Calls.Add("remove-item");
            player.Inventory.ItemsBag.Remove(item);
        }

        // Java Player.java 1157: plclan.clan != null && plclan.clan.equals(this.clan), không true cho mọi cặp.
        public override bool AreSameClan(Player first, Player second) => SameClan && first.Clan is not null && ReferenceEquals(first.Clan, second.Clan);

        // Trả khoảng cách fixture (Java: Util.getDistance).
        public override int GetDistance(Player first, Player second) => Distance;

        // Trả hệ số clan fixture (Java: nPoint.tl*Clan).
        public override void GetClanBonuses(Player player, out int hp, out int damage, out int mp) => (hp, damage, mp) = ClanBonuses;

        // Ghi point (Java: Service.point).
        public override void SendPoint(Player player) => Calls.Add("point");

        // Ghi Siêu Thần Thủy trước Mabư (Java: SuperDivineWater.update).
        public override void UpdateSuperDivineWater(Player player) => Calls.Add("super-water");

        // Ghi Mabư 12 giờ kèm Player để kiểm contract handoff (Java: MajinBuu12H.update).
        public override void UpdateMabuTwelveHour(Player player)
        {
            Calls.Add("mabu12");
            MabuTwelveHourPlayers.Add(player);
        }

        // Ghi Quá Khứ sau Mabư (Java: QuaKhu.update).
        public override void UpdatePastEvent(Player player) => Calls.Add("past");

        // Trả dungeon SnakeWay hoàn tất fixture (Java: allMobsDead).
        public override bool IsSnakeWayCompleted(Player player) => SnakeWayCompleted;

        // Ghi chuyển map SnakeWay (Java: changeMapYardrat).
        public override void MoveToSnakeWayExit(Player player, int x, int y) => Calls.Add($"snake-way {x}/{y}");

        // Trả kích thước list Whis fixture (Java: TopKillWhisManager.getList).
        public override int GetWhisParticipantCount() => WhisParticipantCount;

        // Trả hạng Whis fixture (Java: getPlayerWhis).
        public override int GetWhisRank(Player player) => WhisRank;

        // Ghi effect Whis để kiểm id Java (Java: Service.addEffectChar).
        public override void AddWhisEffect(Player player, int effectId) => Calls.Add($"whis {effectId}");

        // Ghi thông báo Santa (Java: Service.sendThongBao).
        public override void SendBossEventNotification(Player player, string message) => Calls.Add("santa-notification");

        // Ghi check task bang (Java: ClanService.checkDoneTaskJoinClan).
        public override void CheckClanTask(Player player) => Calls.Add("clan-task");

        // Ghi handoff cuối PetFollow cùng Player (Java: PetFollow.update).
        public override void SynchronizePetFollow(Player player) => SynchronizedPetFollowPlayers.Add(player);

        // Ghi HP packet (Java: PlayerService.sendInfoHp).
        public override void SendInfoHp(Player player) => Calls.Add("info-hp");

        // Ghi MP packet (Java: PlayerService.sendInfoMp).
        public override void SendInfoMp(Player player) => Calls.Add("info-mp");

        // Không map cấm để nhánh Chibi hồi phục (Java: MapService predicates).
        public override bool IsWarMap(int mapId) => false;

        // Không map cấm để nhánh Chibi hồi phục (Java: MapService predicates).
        public override bool IsBlackBallWarMap(int mapId) => false;

        // Không map cấm để nhánh Chibi hồi phục (Java: MapService predicates).
        public override bool IsOfflineMap(int mapId) => false;

        // Tăng số lỗi để guard Zone null có thể kiểm (Java: Logger.logException).
        public override void ReportUpdateFailure(Player player, Exception exception) => Errors++;
    }

    // Cổng chết rỗng để Dispose không cần dùng external service (Java: dispose).
    private sealed class NoopDeathGateway : IPlayerDeathGateway
    {
        // Không ở map chiến tranh (Java: MapService.isMapWar).
        public bool IsWarMap(Player player) => false;
        // No-op packet vàng (Java: Service.sendMoney).
        public void SendMoney(Player player) { }
        // No-op drop vàng (Java: Service.dropItemMap).
        public void DropGold(Player player, int itemTemplateId, int amount) { }
        // No-op point (Java: Service.point).
        public void SendPoint(Player player) { }
        // No-op remove effect (Java: EffectSkillService).
        public void RemoveSkillEffectsWhenDie(Player player) { }
        // No-op death packet (Java: Service.charDie).
        public void SendCharacterDeath(Player player) { }
        // No-op social enemy (Java: FriendAndEnemyService).
        public void AddEnemy(Player victim, Player attacker) { }
        // Cho phép PVP mặc định (Java: mapId != 140).
        public bool CanResolvePvpDeath(Player player) => true;
        // No-op PVP (Java: pvp.lose).
        public void ResolvePvpDeath(Player player) { }
        // No-op Black Ball (Java: dropBlackBall).
        public void DropBlackBall(Player player) { }
        // No-op Namek ball (Java: dropNamekBall).
        public void DropNamekBall(Player player) { }
        // No-op contested ball (Java: TranhNgoc.dropBall).
        public void DropContestedNamekBall(Player player) { }
        // No-op contested lift (Java: sendUpdateLift).
        public void SendContestedNamekBallLiftUpdate(Player player) { }
        // No-op trade return (Java: InventoryService.addItemBag).
        public void ReturnTradeItem(Player player, Item item) { }
        // No-op PVP dispose (Java: pvp.dispose).
        public void DisposePvp(Player player) { }
    }
}
