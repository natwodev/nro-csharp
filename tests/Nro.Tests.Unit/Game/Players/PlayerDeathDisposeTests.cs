using Nro.Game.Items.Core;
using Nro.Game.Players;
using Nro.Game.Players.Ports;

namespace Nro.Tests.Unit.Game.Players;

// Characterization chết/hủy Player qua cổng, không dùng Service singleton.
// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 2731–3091).
public sealed class PlayerDeathDisposeTests
{
    // Kiểm tra ba ngưỡng template vàng sau khi Java trừ 5% (Java: setDie).
    [Theory]
    [InlineData(9_000_000_000L, 189, 8_540)]
    [InlineData(15_000_000_000L, 188, 14_240)]
    [InlineData(32_000_000_000L, 190, 30_390)]
    public void Set_die_drops_gold_using_the_java_thresholds(long power, int expectedTemplate, int expectedAmount)
    {
        Player player = MakePlayer();
        player.NPoint.Power = power;
        player.Inventory.Gold = 100_000;
        FakeDeathGateway gateway = new();

        player.SetDie(gateway, new FixedRandom(10));

        Assert.Contains($"gold {expectedTemplate}/{expectedAmount}", gateway.Calls);
        Assert.Equal(100_000 - (power / 1_000_000 > 32_000 ? 32_000 : power / 1_000_000) + 10, player.Inventory.Gold);
    }

    // Kiểm tra chết do quái, boss và người chơi chỉ ghi enemy ở nhánh người chơi hợp lệ (Java: setDie).
    [Fact]
    public void Set_die_only_handoffs_enemy_for_an_eligible_player_attacker()
    {
        Player victim = MakePlayer();
        FakeDeathGateway mobGateway = new();
        victim.SetDie(mobGateway, new FixedRandom(10));
        Assert.DoesNotContain("enemy", mobGateway.Calls);

        Player boss = MakePlayer();
        boss.IsBoss = true;
        FakeDeathGateway bossGateway = new();
        victim.SetDie(boss, bossGateway, new FixedRandom(10));
        Assert.DoesNotContain("enemy", bossGateway.Calls);

        FakeDeathGateway playerGateway = new();
        victim.SetDie(MakePlayer(), playerGateway, new FixedRandom(10));
        Assert.Contains("enemy", playerGateway.Calls);
        Assert.Contains("die", playerGateway.Calls);
    }

    // Kiểm tra setDieLV giữ đúng hành vi hiện tại giống setDie và thứ tự handoff war/PVP (Java: setDieLV).
    [Fact]
    public void Set_die_by_level_matches_set_die_and_preserves_gateway_order()
    {
        Player player = MakePlayer();
        player.Inventory.Gold = 100;
        player.NPoint.Power = 20_000_000;
        player.Pvp = new FakePvp();
        player.IsHoldingContestedNamekBall = true;
        FakeDeathGateway gateway = new();

        player.SetDieByLevel(MakePlayer(), gateway, new FixedRandom(10));

        Assert.Equal(["money", "gold 189/9", "remove-effects", "die", "enemy", "pvp", "black-ball", "namek-ball", "contested-ball", "lift"], gateway.Calls);
    }

    // Kiểm tra dispose trả đồ giao dịch trước dọn, an toàn khi lặp lại và không phá component non-nullable (Java: dispose).
    [Fact]
    public void Dispose_returns_trade_items_once_before_marking_player_disposed()
    {
        Player player = MakePlayer();
        Item item = new();
        player.ItemsTradeWVP.Add(item);
        FakeDeathGateway gateway = new();

        player.Dispose(gateway);
        player.Dispose(gateway);

        Assert.True(player.IsDisposed);
        Assert.Empty(player.ItemsTradeWVP);
        Assert.Equal(["trade-item"], gateway.Calls);
        Assert.NotNull(player.Inventory);
        Assert.NotNull(player.NPoint);
    }

    // Kiểm tra dispose sau chết vẫn không ném và giữ idempotent (Java: dispose).
    [Fact]
    public void Dispose_after_death_is_safe()
    {
        Player player = MakePlayer();
        player.SetDie(new FakeDeathGateway(), new FixedRandom(10));

        player.Dispose(new FakeDeathGateway());
        player.Dispose(new FakeDeathGateway());

        Assert.True(player.IsDisposed);
    }

    // Kiểm tra mẫu số không dương trả 0 thay Infinity của Java double division (Java: percentGold).
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Percent_gold_returns_zero_for_non_positive_denominator(int denominator)
    {
        Player player = MakePlayer();
        player.GoldNormar = 5;

        Assert.Equal("0", player.PercentGold(0, denominator, 10));
    }

    // Kiểm tra giá trị hợp lệ giữ hậu tố .0 của String.valueOf(double) Java (Java: percentGold).
    [Fact]
    public void Percent_gold_formats_ceil_as_a_java_double_string()
    {
        Player player = MakePlayer();
        player.GoldNormar = 1;

        Assert.Equal("34.0", player.PercentGold(0, 3, 10));
    }

    // Tạo Player đủ component cho nhánh chết/hủy (Java: Player()).
    private static Player MakePlayer()
    {
        Player player = new() { IsPlayer = true };
        player.NPoint.Hp = 100;
        player.NPoint.HpMax = 100;
        return player;
    }

    // Random fixture cho vàng rơi (Java: Util.nextInt).
    private sealed class FixedRandom(int value) : IPlayerRandom
    {
        // Trả giá trị fixture trong đoạn Java yêu cầu (Java: Util.nextInt).
        public int NextInt(int minInclusive, int maxInclusive) => value;
    }

    // PVP tối thiểu để kích hoạt nhánh PVP (Java: IPVP).
    private sealed class FakePvp : IPVP
    {
    }

    // Gateway ghi thứ tự world/PVP/war/trade để assertion (Java: các singleton service).
    private sealed class FakeDeathGateway : IPlayerDeathGateway
    {
        // Danh sách side effect theo thứ tự thực thi (Java: Service/FriendAndEnemyService/war service).
        public List<string> Calls { get; } = [];

        // Mặc định không ở map chiến tranh (Java: MapService.isMapWar).
        public bool IsWarMap(Player player) => false;

        // Ghi đồng bộ vàng (Java: Service.sendMoney).
        public void SendMoney(Player player) => Calls.Add("money");

        // Ghi gold drop (Java: Service.dropItemMap).
        public void DropGold(Player player, int itemTemplateId, int amount) => Calls.Add($"gold {itemTemplateId}/{amount}");

        // Ghi point packet (Java: Service.point).
        public void SendPoint(Player player) => Calls.Add("point");

        // Ghi gỡ hiệu ứng chết qua adapter EffectSkillService (Java: effectSkill.removeSkillEffectWhenDie).
        public void RemoveSkillEffectsWhenDie(Player player) => Calls.Add("remove-effects");

        // Ghi packet chết (Java: Service.charDie).
        public void SendCharacterDeath(Player player) => Calls.Add("die");

        // Ghi enemy handoff (Java: FriendAndEnemyService.addEnemy).
        public void AddEnemy(Player victim, Player attacker) => Calls.Add("enemy");

        // Mặc định cho xử lý PVP (Java: mapId != 140).
        public bool CanResolvePvpDeath(Player player) => true;

        // Ghi PVP lose (Java: pvp.lose).
        public void ResolvePvpDeath(Player player) => Calls.Add("pvp");

        // Ghi Black Ball handoff (Java: BlackBallWarService.dropBlackBall).
        public void DropBlackBall(Player player) => Calls.Add("black-ball");

        // Ghi Namek ball handoff (Java: NgocRongNamec.dropNamekBall).
        public void DropNamekBall(Player player) => Calls.Add("namek-ball");

        // Ghi contested Namek ball handoff (Java: TranhNgocService.dropBall).
        public void DropContestedNamekBall(Player player) => Calls.Add("contested-ball");

        // Ghi lift update (Java: TranhNgocService.sendUpdateLift).
        public void SendContestedNamekBallLiftUpdate(Player player) => Calls.Add("lift");

        // Ghi trả đồ qua adapter InventoryService (Java: InventoryService.addItemBag).
        public void ReturnTradeItem(Player player, Item item) => Calls.Add("trade-item");

        // Ghi giải phóng PVP (Java: pvp.dispose).
        public void DisposePvp(Player player) => Calls.Add("dispose-pvp");
    }
}
