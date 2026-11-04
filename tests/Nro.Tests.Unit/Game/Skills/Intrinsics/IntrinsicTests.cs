using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;
using Nro.Core.IO;
using Nro.Game;
using Nro.Tests.Unit.Game.Support;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Skills.Intrinsics;

public class IntrinsicTests
{
    private static Intrinsic Sample() => new()
    {
        Id = 4,
        Name = "Tăng p0% đến p1 HP và p2% đến p3 KI",
        ParamFrom1 = 5,
        ParamTo1 = 10,
        ParamFrom2 = 20,
        ParamTo2 = 30,
        Icon = 77,
        Gender = 1,
        Param1 = 8,
        Param2 = 25,
    };

    [Fact]
    public void GetDescription_shows_the_four_range_bounds()
    {
        Assert.Equal("Tăng 5% đến 10 HP và 20% đến 30 KI", Sample().GetDescription());
    }

    [Fact]
    public void GetName_shows_the_rolled_values_and_the_range_of_the_first_parameter()
    {
        Assert.Equal("Tăng 8 HP và 25 KI [5 đến 10]", Sample().GetName());
    }

    [Fact]
    public void GetName_of_intrinsic_zero_has_no_range_suffix()
    {
        var none = new Intrinsic { Id = 0, Name = "Không có Nội Tại" };

        Assert.Equal("Không có Nội Tại", none.GetName());
    }

    [Fact]
    public void Copy_keeps_the_template_data_and_leaves_the_rolled_values_at_zero()
    {
        Intrinsic original = Sample();

        var copy = new Intrinsic(original);

        Assert.Equal(4, copy.Id);
        Assert.Equal(original.Name, copy.Name);
        Assert.Equal(5, copy.ParamFrom1);
        Assert.Equal(30, copy.ParamTo2);
        Assert.Equal(77, copy.Icon);
        Assert.Equal(1, copy.Gender);
        Assert.Equal(0, copy.Param1);
        Assert.Equal(0, copy.Param2);
    }

    [Fact]
    public void Changing_a_copy_never_changes_the_template()
    {
        Intrinsic template = Sample();
        var playerA = new Intrinsic(template);
        var playerB = new Intrinsic(template);

        playerA.Param1 = 9;
        playerA.Name = "khác";

        Assert.Equal(0, playerB.Param1);
        Assert.Equal(8, template.Param1);
        Assert.Equal("Tăng p0% đến p1 HP và p2% đến p3 KI", template.Name);
    }

    [Fact]
    public void Copy_of_null_is_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => new Intrinsic(null!));
    }
}

public class IntrinsicCatalogTests
{
    internal static readonly string[] Columns = ["id", "name", "param_from_1", "param_to_1", "param_from_2", "param_to_2", "icon", "gender"];

    internal static FakeSqlExecutor Db() => new FakeSqlExecutor().Table("from intrinsic", Columns,
        new object?[] { 0, "Không có Nội Tại", 0, 0, 0, 0, 5000, 3 },
        new object?[] { 1, "Đất p0% đến p1 sức đánh", 5, 10, 0, 0, 5001, 0 },
        new object?[] { 2, "Namek p0% đến p1 HP", 20, 30, 0, 0, 5002, 1 },
        new object?[] { 3, "Xayda p0% đến p1 KI", 1, 3, 0, 0, 5003, 2 },
        new object?[] { 4, "Chung p0% đến p1", 2, 4, 6, 8, 5004, 3 },
        new object?[] { 5, "Lạ p0% đến p1", 2, 4, 0, 0, 5005, 9 });

    internal static async Task<IntrinsicCatalog> LoadAsync()
    {
        var catalog = new IntrinsicCatalog();
        await catalog.LoadAsync(Db());
        return catalog;
    }

    [Fact]
    public async Task Rows_go_to_the_list_of_their_race_and_gender_3_or_unknown_goes_to_all_three()
    {
        IntrinsicCatalog catalog = await LoadAsync();

        Assert.Equal([0, 1, 4, 5], catalog.GetIntrinsics(ConstPlayer.EARTHLING).Select(i => i.Id));
        Assert.Equal([0, 2, 4, 5], catalog.GetIntrinsics(ConstPlayer.NAMEKIAN).Select(i => i.Id));
        Assert.Equal([0, 3, 4, 5], catalog.GetIntrinsics(ConstPlayer.SAIYAN).Select(i => i.Id));
        Assert.Equal([0, 1, 2, 3, 4, 5], catalog.Intrinsics.Select(i => i.Id));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(-1)]
    [InlineData(100)]
    public async Task An_unknown_gender_gets_the_saiyan_list(int gender)
    {
        IntrinsicCatalog catalog = await LoadAsync();

        Assert.Equal([0, 3, 4, 5], catalog.GetIntrinsics((sbyte)gender).Select(i => i.Id));
    }

    [Fact]
    public async Task The_shared_entry_is_the_same_object_in_the_three_lists()
    {
        IntrinsicCatalog catalog = await LoadAsync();

        Assert.Same(catalog.GetIntrinsics(0)[0], catalog.GetIntrinsics(1)[0]);
        Assert.Same(catalog.GetIntrinsics(1)[0], catalog.GetIntrinsics(2)[0]);
    }

    [Fact]
    public async Task The_query_is_ordered_by_id()
    {
        FakeSqlExecutor db = Db();

        await new IntrinsicCatalog().LoadAsync(db);

        Assert.Contains("order by id", db.QueryLog.Single());
    }

    [Fact]
    public async Task All_columns_are_read()
    {
        IntrinsicCatalog catalog = await LoadAsync();

        Intrinsic it = catalog.Intrinsics[4];

        Assert.Equal("Chung p0% đến p1", it.Name);
        Assert.Equal(2, it.ParamFrom1);
        Assert.Equal(4, it.ParamTo1);
        Assert.Equal(6, it.ParamFrom2);
        Assert.Equal(8, it.ParamTo2);
        Assert.Equal(5004, it.Icon);
        Assert.Equal(3, it.Gender);
    }

    [Fact]
    public async Task GetIntrinsicById_returns_a_copy_so_the_catalog_cannot_be_changed_through_it()
    {
        IntrinsicCatalog catalog = await LoadAsync();

        Intrinsic first = catalog.GetIntrinsicById(1)!;
        first.Param1 = 99;
        first.Name = "đã đổi";
        Intrinsic second = catalog.GetIntrinsicById(1)!;

        Assert.NotSame(first, second);
        Assert.Equal(0, second.Param1);
        Assert.Equal("Đất p0% đến p1 sức đánh", second.Name);
        Assert.Null(catalog.GetIntrinsicById(77));
    }

    [Fact]
    public async Task An_id_above_127_wraps_like_the_java_getByte()
    {
        var db = new FakeSqlExecutor().Table("from intrinsic", Columns, new object?[] { 200, "x", 0, 0, 0, 0, 1, 0 });
        var catalog = new IntrinsicCatalog();

        await catalog.LoadAsync(db);

        Assert.Equal(-56, catalog.Intrinsics[0].Id);
    }

    [Fact]
    public async Task A_failing_load_changes_nothing()
    {
        IntrinsicCatalog catalog = await LoadAsync();
        FakeSqlExecutor db = Db();
        db.FailQueryContaining = ("intrinsic", new InvalidOperationException("db down"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => catalog.LoadAsync(db));

        Assert.Equal(6, catalog.Intrinsics.Count);
    }

    [Fact]
    public async Task IntrinsicPlayer_starts_with_a_copy_of_intrinsic_zero()
    {
        IntrinsicCatalog catalog = await LoadAsync();

        var first = new IntrinsicPlayer(catalog);
        var second = new IntrinsicPlayer(catalog);

        Assert.Equal(0, first.Intrinsic.Id);
        Assert.Equal("Không có Nội Tại", first.Intrinsic.Name);
        Assert.Equal(0, first.CountOpen);
        Assert.NotSame(first.Intrinsic, second.Intrinsic);
        Assert.NotSame(catalog.Intrinsics[0], first.Intrinsic);
    }

    [Fact]
    public void IntrinsicPlayer_without_a_catalog_or_without_an_entry_zero_starts_empty_but_never_null()
    {
        var empty = new IntrinsicPlayer();
        var noZero = new IntrinsicPlayer(new IntrinsicCatalog());

        Assert.NotNull(empty.Intrinsic);
        Assert.Equal(0, empty.Intrinsic.Id);
        Assert.NotNull(noZero.Intrinsic);
        empty.Dispose();
        Assert.NotNull(empty.Intrinsic);
        Assert.Throws<ArgumentNullException>(() => new IntrinsicPlayer(null!));
    }
}

public class IntrinsicServiceTests
{
    private static async Task<(IntrinsicService Service, FakeIntrinsicHost Host, ScriptedRandomProvider Random, Player Player)> MakeAsync(sbyte gender = 0)
    {
        IntrinsicCatalog catalog = await IntrinsicCatalogTests.LoadAsync();
        var host = new FakeIntrinsicHost();
        var random = new ScriptedRandomProvider();
        var service = new IntrinsicService(catalog, host, new RandomUtil(random));
        var player = new Player { Gender = gender };
        return (service, host, random, player);
    }

    [Fact]
    public async Task Open_without_enough_power_only_says_so()
    {
        (IntrinsicService service, FakeIntrinsicHost host, ScriptedRandomProvider random, Player player) = await MakeAsync();
        host.Power = 9_999_999_999L;
        host.Gold = long.MaxValue;

        service.Open(player);

        Assert.Equal(["Yêu cầu sức mạnh tối thiểu 10 tỷ"], host.Notifications);
        Assert.Equal(long.MaxValue, host.Gold);
        Assert.Empty(random.Calls);
        Assert.Equal(0, player.PlayerIntrinsic.CountOpen);
    }

    [Fact]
    public async Task Open_with_exactly_ten_billion_power_is_allowed()
    {
        (IntrinsicService service, FakeIntrinsicHost host, ScriptedRandomProvider random, Player player) = await MakeAsync();
        host.Power = 10_000_000_000L;
        host.Gold = 10_000_000;
        random.Enqueue(0, 0, 0);

        service.Open(player);

        Assert.Equal(0, host.Gold);
        Assert.Equal(1, player.PlayerIntrinsic.CountOpen);
    }

    [Fact]
    public async Task Open_without_enough_gold_tells_how_much_is_missing()
    {
        (IntrinsicService service, FakeIntrinsicHost host, ScriptedRandomProvider random, Player player) = await MakeAsync();
        host.Gold = 4_000_000;

        service.Open(player);

        Assert.Equal(["Bạn không đủ vàng, còn thiếu 6 Triệu vàng nữa"], host.Notifications);
        Assert.Equal(4_000_000, host.Gold);
        Assert.Equal(0, host.MoneyInfoSent);
        Assert.Empty(random.Calls);
        Assert.Equal(0, player.PlayerIntrinsic.CountOpen);
    }

    [Fact]
    public async Task Open_pays_rolls_in_the_java_draw_order_and_raises_the_counter()
    {
        (IntrinsicService service, FakeIntrinsicHost host, ScriptedRandomProvider random, Player player) = await MakeAsync();
        host.Gold = 25_000_000;
        // Earth list: [none, 1, 4, 5] -> index draw in [0,3); then param 1 of entry 4 in 2..4 -> [0,3); then param 2 in 6..8 -> [0,3).
        random.Enqueue(1, 2, 1);

        service.Open(player);

        Assert.Equal(15_000_000, host.Gold);
        Assert.Equal(1, host.MoneyInfoSent);
        Assert.Equal([("NextInt", (object?)3), ("NextInt", 3), ("NextInt", 3)], random.Calls);
        Intrinsic rolled = player.PlayerIntrinsic.Intrinsic;
        Assert.Equal(4, rolled.Id);
        Assert.Equal(4, rolled.Param1);
        Assert.Equal(7, rolled.Param2);
        Assert.Equal(1, player.PlayerIntrinsic.CountOpen);
        Assert.Equal(["Bạn nhận được Nội tại:\nChung 4"], host.Notifications);
        (int command, byte[] data) = Assert.Single(host.Messages);
        Assert.Equal(112, command);
        var info = new Message((sbyte)112, data);
        Assert.Equal(0, info.ReadByte());
        Assert.Equal(5004, info.ReadShort());
        Assert.Equal("Chung 4 [2 đến 4]", info.ReadUTF());
    }

    [Fact]
    public async Task Open_uses_the_price_of_the_counter_and_the_counter_stops_at_the_last_price()
    {
        (IntrinsicService service, FakeIntrinsicHost host, ScriptedRandomProvider random, Player player) = await MakeAsync();
        player.PlayerIntrinsic.CountOpen = 7;
        host.Gold = 1_300_000_000L;
        random.Enqueue(0, 0, 0);

        service.Open(player);

        Assert.Equal(20_000_000L, host.Gold); // price 1280 * 1,000,000
        Assert.Equal(7, player.PlayerIntrinsic.CountOpen);
    }

    [Fact]
    public async Task Open_with_a_counter_outside_the_price_table_fails_like_java()
    {
        (IntrinsicService service, _, _, Player player) = await MakeAsync();
        player.PlayerIntrinsic.CountOpen = 8;

        Assert.Throws<IndexOutOfRangeException>(() => service.Open(player));
    }

    [Fact]
    public async Task OpenVip_charges_200_million_and_resets_the_counter()
    {
        (IntrinsicService service, FakeIntrinsicHost host, ScriptedRandomProvider random, Player player) = await MakeAsync(1);
        player.PlayerIntrinsic.CountOpen = 5;
        host.Gold = 250_000_000;
        // Namek list: [none, 2, 4, 5]
        random.Enqueue(0, 0, 0);

        service.OpenVip(player);

        Assert.Equal(50_000_000, host.Gold);
        Assert.Equal(0, player.PlayerIntrinsic.CountOpen);
        Assert.Equal(2, player.PlayerIntrinsic.Intrinsic.Id);
        Assert.Equal(1, host.MoneyInfoSent);
    }

    [Fact]
    public async Task OpenVip_without_enough_gold_or_power_only_says_so()
    {
        (IntrinsicService service, FakeIntrinsicHost host, _, Player player) = await MakeAsync();
        host.Gold = 150_000_000;
        service.OpenVip(player);
        Assert.Equal(["Bạn không đủ vàng, còn thiếu 50 Triệu vàng nữa"], host.Notifications);

        host.Notifications.Clear();
        host.Power = 1;
        service.OpenVip(player);
        Assert.Equal(["Yêu cầu sức mạnh tối thiểu 10 tỷ"], host.Notifications);
        Assert.Equal(150_000_000, host.Gold);
    }

    [Fact]
    public async Task DoiNoiTai_rolls_without_charging()
    {
        (IntrinsicService service, FakeIntrinsicHost host, ScriptedRandomProvider random, Player player) = await MakeAsync(2);
        host.Gold = 5;
        player.PlayerIntrinsic.CountOpen = 3;
        // Saiyan list: [none, 3, 4, 5]; draw index 0 -> position 1 (Xayda), param1 in 1..3 -> [0,3)
        random.Enqueue(0, 2, 0);

        service.DoiNoiTai(player);

        Assert.Equal(5, host.Gold);
        Assert.Equal(3, player.PlayerIntrinsic.CountOpen);
        Assert.Equal(3, player.PlayerIntrinsic.Intrinsic.Id);
        Assert.Equal(3, player.PlayerIntrinsic.Intrinsic.Param1);
        Assert.Equal(0, host.MoneyInfoSent);
    }

    [Fact]
    public async Task A_roll_from_a_race_list_with_only_the_none_entry_is_an_error()
    {
        var catalog = new IntrinsicCatalog();
        await catalog.LoadAsync(new FakeSqlExecutor().Table("from intrinsic", IntrinsicCatalogTests.Columns, new object?[] { 0, "Không có", 0, 0, 0, 0, 1, 3 }));
        var service = new IntrinsicService(catalog, new FakeIntrinsicHost(), new RandomUtil(new ScriptedRandomProvider()));

        Assert.Throws<ArgumentOutOfRangeException>(() => service.DoiNoiTai(new Player()));
    }

    [Fact]
    public async Task Two_players_who_roll_the_same_entry_get_independent_copies()
    {
        (IntrinsicService service, FakeIntrinsicHost host, ScriptedRandomProvider random, Player playerA) = await MakeAsync();
        var playerB = new Player();
        host.Gold = 100_000_000;
        random.Enqueue(0, 0, 0, 0, 1, 0);

        service.DoiNoiTai(playerA);
        service.DoiNoiTai(playerB);

        Assert.NotSame(playerA.PlayerIntrinsic.Intrinsic, playerB.PlayerIntrinsic.Intrinsic);
        Assert.Equal(5, playerA.PlayerIntrinsic.Intrinsic.Param1);
        Assert.Equal(6, playerB.PlayerIntrinsic.Intrinsic.Param1);
    }

    [Fact]
    public async Task SendInfoIntrinsic_sends_command_112_with_icon_and_name()
    {
        (IntrinsicService service, FakeIntrinsicHost host, _, Player player) = await MakeAsync();
        player.PlayerIntrinsic.Intrinsic = new Intrinsic { Id = 0, Name = "Không có Nội Tại", Icon = 5000 };

        service.SendInfoIntrinsic(player);

        (int command, byte[] data) = Assert.Single(host.Messages);
        Assert.Equal(112, command);
        var info = new Message((sbyte)112, data);
        Assert.Equal(0, info.ReadByte());
        Assert.Equal(5000, info.ReadShort());
        Assert.Equal("Không có Nội Tại", info.ReadUTF());
    }

    [Fact]
    public async Task ShowAllIntrinsic_lists_every_entry_but_the_first_with_its_description()
    {
        (IntrinsicService service, FakeIntrinsicHost host, _, Player player) = await MakeAsync();

        service.ShowAllIntrinsic(player);

        (int command, byte[] data) = Assert.Single(host.Messages);
        Assert.Equal(112, command);
        var list = new Message((sbyte)112, data);
        Assert.Equal(1, list.ReadByte());
        Assert.Equal(1, list.ReadByte());
        Assert.Equal("Nội tại", list.ReadUTF());
        Assert.Equal(3, list.ReadByte());
        Assert.Equal(5001, list.ReadShort());
        Assert.Equal("Đất 5% đến 10 sức đánh", list.ReadUTF());
        Assert.Equal(5004, list.ReadShort());
        Assert.Equal("Chung 2% đến 4", list.ReadUTF());
        Assert.Equal(5005, list.ReadShort());
        Assert.Equal("Lạ 2% đến 4", list.ReadUTF());
    }

    [Fact]
    public async Task The_menus_use_the_java_texts_and_ids()
    {
        (IntrinsicService service, FakeIntrinsicHost host, _, Player player) = await MakeAsync();
        player.PlayerIntrinsic.CountOpen = 2;

        service.ShowMenu(player);
        service.ShowConfirmOpen(player);
        service.ShowConfirmOpenVip(player);

        Assert.Equal(3, host.Menus.Count);
        Assert.Equal(ConstNpc.INTRINSIC, host.Menus[0].MenuId);
        Assert.Equal(-1, host.Menus[0].Avatar);
        Assert.Equal("Nội tại là một kỹ năng bị động hỗ trợ đặc biệt\nBạn có muốn mở hoặc thay đổi nội tại không?", host.Menus[0].Text);
        Assert.Equal(["Xem\ntất cả\nNội Tại", "Mở\nNội Tại", "Mở VIP", "Từ chối"], host.Menus[0].Options);
        Assert.Equal(ConstNpc.CONFIRM_OPEN_INTRINSIC, host.Menus[1].MenuId);
        Assert.Equal("Bạn muốn đổi Nội Tại khác\nvới giá là 40 Tr vàng ?", host.Menus[1].Text);
        Assert.Equal(["Mở\nNội Tại", "Từ chối"], host.Menus[1].Options);
        Assert.Equal(ConstNpc.CONFIRM_OPEN_INTRINSIC_VIP, host.Menus[2].MenuId);
        Assert.Equal("Bạn có muốn mở Nội Tại VIP\nvới giá là 200 triệu vàng\nvà tái lập giá vàng quay lại ban đầu không?", host.Menus[2].Text);
        Assert.Equal(["Mở\nNội VIP", "Từ chối"], host.Menus[2].Options);
    }

    [Fact]
    public async Task Catalog_delegates_are_exposed_by_the_service()
    {
        (IntrinsicService service, _, _, _) = await MakeAsync();

        Assert.Equal(4, service.GetIntrinsics(0).Count);
        Assert.Equal(2, service.GetIntrinsicById(2)!.Id);
        Assert.Null(service.GetIntrinsicById(99));
    }
}
