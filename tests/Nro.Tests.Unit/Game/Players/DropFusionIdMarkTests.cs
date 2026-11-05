using Nro.Core.Consts;
using Nro.Game.Players.Gifts;
using Nro.Game.Players.Ports;
using Nro.Game.Players.Relations;
using Nro.Game.World.Maps;
using Nro.Tests.Unit.Game.World;

namespace Nro.Tests.Unit.Game.Players;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/{DropItem,Fusion,IDMark}.java (dòng 1–141).
public sealed class DropFusionIdMarkTests
{
    // Kiểm tra DropItem bỏ qua từng điều kiện bảo vệ Java (Java: update).
    [Theory]
    [InlineData(false, 1, false, false)]
    [InlineData(true, 0, false, false)]
    [InlineData(true, 1, true, false)]
    [InlineData(true, 1, false, true)]
    public void Drop_item_does_not_drop_when_a_java_guard_fails(bool playerFlag, int emptyBag, bool hasBagItem, bool hasMapItem)
    {
        (Player player, Zone zone) = CreateMap52Player(playerFlag);
        var gateway = new DropGateway { EmptyBag = emptyBag, BagContainsItem = hasBagItem, HasMapItem = hasMapItem };

        new DropItem(player).Update(gateway, new FixedRandom(100));

        Assert.Null(gateway.Drop);
        Assert.NotNull(zone);
    }

    // Kiểm tra DropItem tạo vật phẩm 726 riêng với hai option 30/93 đúng thứ tự (Java: update).
    [Fact]
    public void Drop_item_creates_the_personal_726_item_with_java_options()
    {
        (Player player, Zone _) = CreateMap52Player(true);
        var gateway = new DropGateway { EmptyBag = 1 };

        new DropItem(player).Update(gateway, new FixedRandom(100));

        DropRecord result = Assert.IsType<DropRecord>(gateway.Drop);
        Assert.Equal((726, 1, 100, 100), (result.ItemTemplateId, result.Quantity, result.X, result.Y));
        Assert.Equal([(30, 0), (93, 1)], result.Options.Select(option => (option.Id, option.Param)).ToArray());
        Assert.Same(player, result.Player);
    }

    // Kiểm tra Fusion tách đúng ngưỡng 600000 ms qua IPlayerClock và chỉ với Lưỡng long nhất thể (Java: update).
    [Fact]
    public void Fusion_unfuses_at_the_java_time_threshold()
    {
        Player player = new();
        var gateway = new FusionGateway();
        player.Fusion.TypeFusion = ConstPlayer.FUSION_DANCE;
        player.Fusion.LastTimeFusion = 10;

        player.Fusion.Update(new FixedClock(600_009), gateway);
        Assert.Equal(0, gateway.UnfuseCalls);
        player.Fusion.Update(new FixedClock(600_010), gateway);

        Assert.Equal(1, gateway.UnfuseCalls);
        Assert.Same(player, gateway.Player);
        player.Fusion.Dispose();
        Assert.Null(player.Fusion.Player);
    }

    // Kiểm tra giá trị mặc định, accessor và dispose của IDMark (Java: IDMark, dispose).
    [Fact]
    public void Id_mark_defaults_accessors_and_dispose_match_java()
    {
        var marker = new IDMark { MenuIndex = ConstNpc.BASE_MENU, NpcId = 12 };
        var shop = new TestShop();
        marker.OpenShop = shop;
        marker.ShopTagName = "tag";
        marker.TradePlayer = new Player();

        Assert.Equal(-9999, marker.KillCharacterId);
        Assert.Equal(-1, marker.TradePlayerId);
        Assert.Equal(-1, marker.TemporaryBlackBallHoldId);
        Assert.Equal(-1, marker.TemporaryNamekBallHoldId);
        Assert.Equal((sbyte)-1, marker.NamekWarState);
        marker.SetNamekWarState(3);
        Assert.Equal((sbyte)3, marker.GetNamekWarState());
        Assert.Equal((sbyte)3, marker.GetIsNamekWarState());
        Assert.True(marker.IsBaseMenu());
        Assert.Equal((sbyte)12, marker.NpcId);

        marker.Dispose();

        Assert.True(shop.Disposed);
        Assert.Null(marker.OpenShop);
        Assert.Null(marker.ShopTagName);
        Assert.Null(marker.TradePlayer);
    }

    // Tạo người chơi thật ở map 52 đủ chiều rộng cho khoảng random Java (Java: DropItem.update).
    private static (Player Player, Zone Zone) CreateMap52Player(bool isPlayer)
    {
        var game = new FakeGame();
        var map = new Map(game.Runtime, 52, "test", 0, 0, 0, 0, 0, [new int[20]], [], 0, 7, [], [], 0);
        var zone = new Zone(map, 0, 7);
        var player = new Player { IsPlayer = isPlayer, Zone = zone };
        return (player, zone);
    }

    // Ghi nhận lệnh thả qua cổng thay cho Service Java chưa được port (Java: IDropItemGateway).
    private sealed class DropGateway : IDropItemGateway
    {
        // Số ô trống trả về cho nhánh Java (Java: getCountEmptyBag).
        public int EmptyBag { get; set; }

        // Cờ vật phẩm đã có trong túi (Java: isExistItemBag).
        public bool BagContainsItem { get; set; }

        // Cờ vật phẩm riêng đã ở map (Java: findItemMapByPlayer).
        public bool HasMapItem { get; set; }

        // Lệnh rơi đã nhận, null khi nhánh không rơi (Java: dropItemMapForMe).
        public DropRecord? Drop { get; private set; }

        // Trả số ô trống túi đã cấu hình (Java: getCountEmptyBag).
        public int GetEmptyBagCount(Player player) => EmptyBag;

        // Trả cờ vật phẩm túi đã cấu hình (Java: isExistItemBag).
        public bool HasBagItem(Player player, int itemTemplateId) => BagContainsItem;

        // Trả cờ vật phẩm map đã cấu hình (Java: findItemMapByPlayer).
        public bool HasItemMapForPlayer(Player player, int itemTemplateId) => HasMapItem;

        // Ghi nhận tham số vật phẩm rơi (Java: dropItemMapForMe).
        public void DropItemMapForPlayer(Player player, Zone zone, int itemTemplateId, int quantity, int x, int y, IReadOnlyList<ItemMapOption> options) => Drop = new(player, itemTemplateId, quantity, x, y, options);
    }

    // Bản ghi lệnh rơi dùng kiểm thử (Java: ItemMap).
    private sealed record DropRecord(Player Player, int ItemTemplateId, int Quantity, int X, int Y, IReadOnlyList<ItemMapOption> Options);

    // Đồng hồ cố định để xác định ngưỡng Java (Java: System.currentTimeMillis).
    private sealed class FixedClock(long nowMilliseconds) : IPlayerClock
    {
        // Trả mili giây epoch kiểm thử (Java: System.currentTimeMillis).
        public long NowMilliseconds { get; } = nowMilliseconds;
    }

    // Random trả cận dưới hợp lệ Java (Java: Util.nextInt).
    private sealed class FixedRandom(int value) : IPlayerRandom
    {
        // Trả giá trị trong khoảng yêu cầu (Java: Util.nextInt).
        public int NextInt(int minInclusive, int maxInclusive) => value;
    }

    // Shop tối thiểu để kiểm tra dispose qua cổng (Java: Shop.dispose).
    private sealed class TestShop : IPlayerShop
    {
        // Cờ đã dispose của shop (Java: dispose).
        public bool Disposed { get; private set; }

        // Đánh dấu shop đã dispose (Java: dispose).
        public void Dispose() => Disposed = true;
    }

    // Cổng tách hợp thể ghi nhận lệnh gọi (Java: Detu.unFusion).
    private sealed class FusionGateway : IFusionGateway
    {
        // Số lần tách hợp thể (Java: unFusion).
        public int UnfuseCalls { get; private set; }

        // Người chơi được tách hợp thể (Java: unFusion).
        public Player? Player { get; private set; }

        // Ghi nhận lệnh tách hợp thể (Java: unFusion).
        public void UnfusePet(Player player)
        {
            UnfuseCalls++;
            Player = player;
        }
    }
}
