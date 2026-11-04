using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Game;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary>AddItemList, AddItemBag and the special items of <see cref="InventoryService"/>.</summary>
public class InventoryServiceAddTests
{
    // ------------------------------------------------------------------ AddItemList: placing

    [Fact]
    public void AddItemList_puts_a_copy_in_the_first_empty_slot_and_zeroes_the_source()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(10));
        Item add = k.Item(11, 1, (4, 8));

        bool ok = k.Service.AddItemList(k.Inv.ItemsBag, add);

        Assert.True(ok);
        Assert.Equal(0, add.Quantity);
        Assert.Equal(11, k.Inv.ItemsBag[1].Template!.Id);
        Assert.NotSame(add, k.Inv.ItemsBag[1]);
        Assert.NotSame(add.ItemOptions[0], k.Inv.ItemsBag[1].ItemOptions[0]);
        Assert.Equal(8, k.Inv.ItemsBag[1].ItemOptions[0].Param);
        Assert.Equal(1, k.Inv.ItemsBag[1].Quantity);
    }

    [Fact]
    public void AddItemList_gives_an_item_without_options_the_empty_option_73()
    {
        var k = new InvKit();
        Item add = k.Item(11);

        k.Service.AddItemList(k.Inv.ItemsBag, add);

        Assert.Single(add.ItemOptions);
        Assert.Equal(73, add.ItemOptions[0].OptionTemplate.Id);
        Assert.Equal(0, add.ItemOptions[0].Param);
        Assert.Equal(73, k.Inv.ItemsBag[0].ItemOptions[0].OptionTemplate.Id);
    }

    [Fact]
    public void AddItemList_on_a_full_list_returns_false_and_leaves_the_item_alone()
    {
        var k = new InvKit();
        for (int i = 0; i < k.Inv.ItemsBag.Count; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(10 + i));
        }

        Item add = k.Item(50, 3);
        bool ok = k.Service.AddItemList(k.Inv.ItemsBag, add);

        Assert.False(ok);
        Assert.Equal(3, add.Quantity);
        Assert.Equal(InvKit.BagSlots, k.CountReal(k.Inv.ItemsBag));
    }

    [Fact]
    public void AddItemList_with_zero_quantity_adds_nothing_and_returns_false()
    {
        var k = new InvKit();
        Item add = k.Item(50, 0);

        Assert.False(k.Service.AddItemList(k.Inv.ItemsBag, add));
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBag));
    }

    [Fact]
    public void AddItemList_into_an_empty_list_object_returns_false()
    {
        var k = new InvKit();
        Assert.False(k.Service.AddItemList(new List<Item>(), k.Item(50)));
    }

    [Fact]
    public void AddItemList_does_not_stack_items_that_are_not_stackable()
    {
        var k = new InvKit();
        k.Define(10, upToUp: false);
        k.Put(k.Inv.ItemsBag, 0, k.Item(10, 5));

        k.Service.AddItemList(k.Inv.ItemsBag, k.Item(10, 5));

        Assert.Equal(5, k.Inv.ItemsBag[0].Quantity);
        Assert.Equal(5, k.Inv.ItemsBag[1].Quantity);
    }

    // ------------------------------------------------------------------ AddItemList: stacking

    [Fact]
    public void AddItemList_stacks_into_an_existing_stack()
    {
        var k = new InvKit();
        k.Define(10, upToUp: true);
        k.Put(k.Inv.ItemsBag, 2, k.Item(10, 100));
        Item add = k.Item(10, 50);

        bool ok = k.Service.AddItemList(k.Inv.ItemsBag, add);

        Assert.True(ok);
        Assert.Equal(150, k.Inv.ItemsBag[2].Quantity);
        Assert.Equal(0, add.Quantity);
        Assert.Equal(1, k.CountReal(k.Inv.ItemsBag));
    }

    [Theory]
    [InlineData(9990, 9, 9999, 0)]
    [InlineData(9990, 10, 9999, 1)]
    [InlineData(9990, 20, 9999, 11)]
    [InlineData(1, 9998, 9999, 0)]
    [InlineData(1, 9999, 9999, 1)]
    public void AddItemList_fills_a_stack_to_9999_and_puts_the_rest_in_an_empty_slot(int existing, int adding, int expectedStack, int expectedRest)
    {
        var k = new InvKit();
        k.Define(10, upToUp: true);
        k.Put(k.Inv.ItemsBag, 0, k.Item(10, existing));

        bool ok = k.Service.AddItemList(k.Inv.ItemsBag, k.Item(10, adding));

        Assert.True(ok);
        Assert.Equal(expectedStack, k.Inv.ItemsBag[0].Quantity);
        if (expectedRest > 0)
        {
            Assert.Equal(expectedRest, k.Inv.ItemsBag[1].Quantity);
        }
        else
        {
            Assert.False(k.Inv.ItemsBag[1].IsNotNullItem());
        }
    }

    [Fact]
    public void AddItemList_stack_that_is_already_full_is_skipped()
    {
        var k = new InvKit();
        k.Define(10, upToUp: true);
        k.Put(k.Inv.ItemsBag, 0, k.Item(10, 9999));
        k.Put(k.Inv.ItemsBag, 1, k.Item(10, 5));

        k.Service.AddItemList(k.Inv.ItemsBag, k.Item(10, 3));

        Assert.Equal(9999, k.Inv.ItemsBag[0].Quantity);
        Assert.Equal(8, k.Inv.ItemsBag[1].Quantity);
    }

    [Fact]
    public void AddItemList_stack_over_9999_but_below_100_million_takes_nothing_more()
    {
        var k = new InvKit();
        k.Define(10, upToUp: true);
        k.Put(k.Inv.ItemsBag, 0, k.Item(10, 50_000));

        k.Service.AddItemList(k.Inv.ItemsBag, k.Item(10, 3));

        Assert.Equal(50_000, k.Inv.ItemsBag[0].Quantity);
        Assert.Equal(3, k.Inv.ItemsBag[1].Quantity);
    }

    [Fact]
    public void AddItemList_partial_stacking_with_no_empty_slot_returns_false_but_the_stack_has_grown()
    {
        var k = new InvKit();
        k.Define(10, upToUp: true);
        for (int i = 0; i < k.Inv.ItemsBag.Count; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(100 + i));
        }

        k.Put(k.Inv.ItemsBag, 0, k.Item(10, 9990));
        Item add = k.Item(10, 20);

        bool ok = k.Service.AddItemList(k.Inv.ItemsBag, add);

        Assert.False(ok);
        Assert.Equal(9999, k.Inv.ItemsBag[0].Quantity);
        Assert.Equal(11, add.Quantity);
    }

    [Fact]
    public void AddItemList_item_1705_has_no_cap_but_skips_stacks_of_100_million_or_more()
    {
        var k = new InvKit();
        k.Define(1705, upToUp: true);
        k.Put(k.Inv.ItemsBag, 0, k.Item(1705, 99_999_999));
        k.Service.AddItemList(k.Inv.ItemsBag, k.Item(1705, 1_000_000));
        Assert.Equal(100_999_999, k.Inv.ItemsBag[0].Quantity);
        Assert.Equal(1, k.CountReal(k.Inv.ItemsBag));

        k.Service.AddItemList(k.Inv.ItemsBag, k.Item(1705, 7));
        Assert.Equal(100_999_999, k.Inv.ItemsBag[0].Quantity);
        Assert.Equal(7, k.Inv.ItemsBag[1].Quantity);
    }

    [Fact]
    public void AddItemList_item_1705_quantity_wraps_in_32_bits()
    {
        var k = new InvKit();
        k.Define(1705, upToUp: true);
        k.Put(k.Inv.ItemsBag, 0, k.Item(1705, 99_999_999));

        k.Service.AddItemList(k.Inv.ItemsBag, k.Item(1705, int.MaxValue));

        Assert.Equal(unchecked(99_999_999 + int.MaxValue), k.Inv.ItemsBag[0].Quantity);
        Assert.True(k.Inv.ItemsBag[0].Quantity < 0);
    }

    // ------------------------------------------------------------------ AddItemList: incremental options

    [Theory]
    [InlineData(1)]
    [InlineData(31)]
    public void AddItemList_incremental_item_adds_its_parameter_to_the_existing_item(int optionId)
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 4, k.Item(10, 1, (optionId, 100)));
        Item add = k.Item(10, 1, (optionId, 25));

        bool ok = k.Service.AddItemList(k.Inv.ItemsBag, add);

        Assert.True(ok);
        Assert.Equal(125, k.Inv.ItemsBag[4].ItemOptions[0].Param);
        Assert.Equal(0, add.Quantity);
        Assert.Equal(1, k.CountReal(k.Inv.ItemsBag));
    }

    [Fact]
    public void AddItemList_incremental_item_vanishes_when_the_existing_one_lacks_the_option()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 4, k.Item(10, 1, (9, 100)));
        Item add = k.Item(10, 1, (31, 25));

        bool ok = k.Service.AddItemList(k.Inv.ItemsBag, add);

        Assert.True(ok);
        Assert.Equal(0, add.Quantity);
        Assert.Equal(100, k.Inv.ItemsBag[4].ItemOptions[0].Param);
        Assert.Equal(1, k.CountReal(k.Inv.ItemsBag));
    }

    [Fact]
    public void AddItemList_incremental_item_with_no_match_goes_to_an_empty_slot()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(11, 1, (31, 100)));

        k.Service.AddItemList(k.Inv.ItemsBag, k.Item(10, 1, (31, 25)));

        Assert.Equal(25, k.Inv.ItemsBag[1].ItemOptions[0].Param);
    }

    // ------------------------------------------------------------------ AddItemList: gold bars

    [Fact]
    public void AddItemList_gold_bar_stacks_up_to_100000_and_the_rest_goes_to_a_new_slot()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(457, 99_990));

        bool ok = k.Service.AddItemList(k.Inv.ItemsBag, k.Item(457, 20), k.Player);

        Assert.True(ok);
        Assert.Equal(100_000, k.Inv.ItemsBag[0].Quantity);
        Assert.Equal(10, k.Inv.ItemsBag[1].Quantity);
    }

    [Fact]
    public void AddItemList_locked_and_unlocked_gold_bars_do_not_mix()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(457, 5, (30, 0)));

        k.Service.AddItemList(k.Inv.ItemsBag, k.Item(457, 7), k.Player);
        k.Service.AddItemList(k.Inv.ItemsBag, k.Item(457, 3, (30, 0)), k.Player);

        Assert.Equal(8, k.Inv.ItemsBag[0].Quantity);
        Assert.Equal(7, k.Inv.ItemsBag[1].Quantity);
        Assert.False(k.Service.HasOption(k.Inv.ItemsBag[1], 30));
    }

    [Fact]
    public void AddItemList_a_big_pile_of_gold_bars_is_cut_into_slots_of_100000()
    {
        var k = new InvKit();
        Item add = k.Item(457, 250_000);

        bool ok = k.Service.AddItemList(k.Inv.ItemsBag, add, k.Player);

        Assert.True(ok);
        Assert.Equal(new[] { 100_000, 100_000, 50_000 }, k.Inv.ItemsBag.Take(3).Select(i => i.Quantity));
        Assert.Equal(0, add.Quantity);
        Assert.Equal(3, k.CountReal(k.Inv.ItemsBag));
    }

    [Fact]
    public void AddItemList_gold_bars_that_do_not_fit_tell_the_player_and_return_false()
    {
        var k = new InvKit();
        for (int i = 0; i < k.Inv.ItemsBag.Count - 1; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(100 + i));
        }

        Item add = k.Item(457, 150_000);
        bool ok = k.Service.AddItemList(k.Inv.ItemsBag, add, k.Player);

        Assert.False(ok);
        Assert.Equal(50_000, add.Quantity);
        Assert.Equal(100_000, k.Inv.ItemsBag[^1].Quantity);
        Assert.Equal(new[] { "SendNotification:Hành trang đã đầy, không thể nhận thêm Thỏi vàng" }, k.Notifier.Calls);
    }

    [Fact]
    public void AddItemList_gold_bars_without_a_player_to_tell_stay_silent()
    {
        var k = new InvKit();
        for (int i = 0; i < k.Inv.ItemsBag.Count; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(100 + i));
        }

        Assert.False(k.Service.AddItemList(k.Inv.ItemsBag, k.Item(457, 5)));
        Assert.Empty(k.Notifier.Calls);
    }

    [Fact]
    public void AddItemList_gold_bar_of_zero_quantity_is_accepted_only_when_a_matching_stack_exists()
    {
        var k = new InvKit();
        Assert.False(k.Service.AddItemList(k.Inv.ItemsBag, k.Item(457, 0), k.Player));

        k.Put(k.Inv.ItemsBag, 0, k.Item(457, 5));
        Assert.True(k.Service.AddItemList(k.Inv.ItemsBag, k.Item(457, 0), k.Player));
    }

    [Fact]
    public void AddItemBox_AddItemBoxCollection_and_AddItemBoxClan_use_their_own_list()
    {
        var k = new InvKit();
        k.Player.Clan = InvKit.Raw<Clan>();
        k.Host.ClanBox.AddRange(new[] { k.Empty(), k.Empty() });

        Assert.True(k.Service.AddItemBox(k.Player, k.Item(10)));
        Assert.True(k.Service.AddItemBoxCollection(k.Player, k.Item(11)));
        Assert.True(k.Service.AddItemBoxClan(k.Player, k.Item(12)));

        Assert.Equal(10, k.Inv.ItemsBox[0].Template!.Id);
        Assert.Equal(11, k.Inv.ItemsBoxCollection[0].Template!.Id);
        Assert.Equal(12, k.Host.ClanBox[0].Template!.Id);
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBag));
    }

    [Fact]
    public void CheckListsEqual_compares_length_and_each_option_id_and_parameter()
    {
        var k = new InvKit();
        var a = new List<ItemOption> { k.Opt(1, 5), k.Opt(2, 6) };
        Assert.True(InventoryService.CheckListsEqual(a, new List<ItemOption> { k.Opt(1, 5), k.Opt(2, 6) }));
        Assert.False(InventoryService.CheckListsEqual(a, new List<ItemOption> { k.Opt(1, 5) }));
        Assert.False(InventoryService.CheckListsEqual(a, new List<ItemOption> { k.Opt(1, 5), k.Opt(2, 7) }));
        Assert.False(InventoryService.CheckListsEqual(a, new List<ItemOption> { k.Opt(2, 6), k.Opt(1, 5) }));
        Assert.True(InventoryService.CheckListsEqual([], []));
    }

    // ------------------------------------------------------------------ AddItemBag

    [Fact]
    public void AddItemBag_black_ball_is_handed_to_the_black_ball_war()
    {
        var k = new InvKit();
        k.Host.BlackBalls.Add(14);
        k.Host.PickResult = false;

        Assert.False(k.Service.AddItemBag(k.Player, k.Item(14)));
        k.Host.PickResult = true;
        Assert.True(k.Service.AddItemBag(k.Player, k.Item(14)));

        Assert.Equal(new[] { "PickBlackBall", "PickBlackBall" }, k.Host.Calls);
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBag));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void AddItemBag_namek_ball_and_namek_stone_are_handed_to_the_namek_war(bool ball, bool stone)
    {
        var k = new InvKit();
        if (ball)
        {
            k.Host.NamecBalls.Add(353);
        }

        if (stone)
        {
            k.Host.NamecStones.Add(353);
        }

        Assert.True(k.Service.AddItemBag(k.Player, k.Item(353)));
        Assert.Equal(new[] { "PickNamekBall" }, k.Host.Calls);
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBag));
    }

    [Fact]
    public void AddItemBag_plain_item_goes_to_the_bag()
    {
        var k = new InvKit();
        Assert.True(k.Service.AddItemBag(k.Player, k.Item(77, 2)));
        Assert.Equal(77, k.Inv.ItemsBag[0].Template!.Id);
        Assert.Equal(2, k.Inv.ItemsBag[0].Quantity);
    }

    [Fact]
    public void AddItemBag_full_bag_returns_false()
    {
        var k = new InvKit();
        for (int i = 0; i < k.Inv.ItemsBag.Count; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(100 + i));
        }

        Assert.False(k.Service.AddItemBag(k.Player, k.Item(77)));
    }

    [Theory]
    [InlineData(0L, 1_000, true, 1_000L)]
    [InlineData(1_999_999_999_000L, 1_000, true, 2_000_000_000_000L)]
    [InlineData(1_999_999_999_000L, 1_001, false, 1_999_999_999_000L)]
    [InlineData(2_000_000_000_000L, 1, false, 2_000_000_000_000L)]
    [InlineData(2_000_000_000_000L, 0, true, 2_000_000_000_000L)]
    [InlineData(5_000_000_000L, int.MaxValue, true, 5_000_000_000L + int.MaxValue)]
    public void AddItemBag_gold_item_is_added_to_gold_only_up_to_the_limit(long gold, int quantity, bool expected, long expectedGold)
    {
        var k = new InvKit();
        k.Define(190, 9);
        k.Inv.Gold = gold;

        bool ok = k.Service.AddItemBag(k.Player, k.Item(190, quantity));

        Assert.Equal(expected, ok);
        Assert.Equal(expectedGold, k.Inv.Gold);
        Assert.Equal(expected ? "SendMoney" : "SendNotification:Vàng sau khi nhặt quá giới hạn cho phép", k.Notifier.Calls.Single());
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBag));
    }

    [Theory]
    [InlineData(10, 0, 5, 5)]
    [InlineData(10, 2_000_000_000, 147_483_647, int.MaxValue)]
    [InlineData(10, 2_000_000_000, 147_483_648, int.MaxValue)]
    [InlineData(10, 2_000_000_000, int.MaxValue, int.MaxValue)]
    [InlineData(10, int.MaxValue, 1, int.MaxValue)]
    [InlineData(10, 10, -30, -20)]
    [InlineData(34, 7, 3, 10)]
    [InlineData(34, 2_000_000_000, int.MaxValue, int.MaxValue)]
    public void AddItemBag_gem_and_ruby_items_are_added_in_64_bits_and_clamped_to_int_max(int type, int start, int quantity, int expected)
    {
        var k = new InvKit();
        k.Define(191 + type, (sbyte)type);
        if (type == 10)
        {
            k.Inv.Gem = start;
        }
        else
        {
            k.Inv.Ruby = start;
        }

        bool ok = k.Service.AddItemBag(k.Player, k.Item(191 + type, quantity));

        Assert.True(ok);
        Assert.Equal(expected, type == 10 ? k.Inv.Gem : k.Inv.Ruby);
        Assert.Equal(new[] { "SendMoney" }, k.Notifier.Calls);
    }

    [Fact]
    public void AddItemBag_gem_below_int_min_wraps_through_the_long_cast()
    {
        var k = new InvKit();
        k.Define(201, 10);
        k.Inv.Gem = int.MinValue;

        k.Service.AddItemBag(k.Player, k.Item(201, -1));

        // (long) int.MinValue + (long) -1 = -2147483649, cast back to int wraps to int.MaxValue.
        Assert.Equal(int.MaxValue, k.Inv.Gem);
    }

    [Theory]
    [InlineData(1627)]
    [InlineData(517)]
    public void AddItemBag_bag_expansion_items_add_one_slot(int id)
    {
        var k = new InvKit();
        int before = k.Inv.ItemsBag.Count;

        Assert.True(k.Service.AddItemBag(k.Player, k.Item(id)));

        Assert.Equal(before + 1, k.Inv.ItemsBag.Count);
        Assert.False(k.Inv.ItemsBag[^1].IsNotNullItem());
        Assert.Equal(new[] { "SendNotificationOk:Hành trang của bạn đã được mở rộng thêm 1 ô" }, k.Notifier.Calls);
    }

    [Fact]
    public void AddItemBag_box_expansion_item_adds_one_box_slot()
    {
        var k = new InvKit();
        int before = k.Inv.ItemsBox.Count;

        Assert.True(k.Service.AddItemBag(k.Player, k.Item(518)));

        Assert.Equal(before + 1, k.Inv.ItemsBox.Count);
        Assert.Equal(new[] { "SendNotificationOk:Rương đồ của bạn đã được mở rộng thêm 1 ô" }, k.Notifier.Calls);
    }

    [Fact]
    public void AddItemBag_item_988_never_succeeds_because_the_gold_limit_is_already_the_maximum()
    {
        var k = new InvKit();
        Assert.False(k.Service.AddItemBag(k.Player, k.Item(988)));
        Assert.Equal(new[] { "SendNotificationOk:Giới hạn vàng của bạn đã đạt tối đa" }, k.Notifier.Calls);
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBag));
    }

    [Theory]
    [InlineData(718)]
    [InlineData(1788)]
    public void AddItemBag_gift_ticket_without_one_in_the_bag_is_stored_with_option_31(int id)
    {
        var k = new InvKit();

        Assert.True(k.Service.AddItemBag(k.Player, k.Item(id, 5)));

        Assert.Equal(id, k.Inv.ItemsBag[0].Template!.Id);
        Assert.Equal(5, k.Inv.ItemsBag[0].Quantity);
        Assert.True(k.Service.HasOption(k.Inv.ItemsBag[0], 31));
    }

    [Theory]
    [InlineData(718, 100, 50, 150)]
    [InlineData(1788, 29_000, 2_000, 30_000)]
    [InlineData(718, 30_000, 1, 30_000)]
    [InlineData(718, 0, 0, 0)]
    public void AddItemBag_gift_ticket_is_folded_into_the_ticket_in_the_bag_with_a_cap_of_30000(int id, int existingParam, int quantity, int expectedParam)
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 3, k.Item(id, 1, (31, existingParam)));
        Item add = k.Item(id, quantity);

        Assert.True(k.Service.AddItemBag(k.Player, add));

        Assert.Equal(expectedParam, k.Inv.ItemsBag[3].ItemOptions[0].Param);
        Assert.Equal(0, add.Quantity);
        Assert.Equal(1, k.CountReal(k.Inv.ItemsBag));
    }

    [Fact]
    public void AddItemBag_gift_ticket_parameter_wraps_before_the_cap_like_java()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(718, 1, (31, 29_999)));

        k.Service.AddItemBag(k.Player, k.Item(718, int.MaxValue));

        Assert.Equal(unchecked(29_999 + int.MaxValue), k.Inv.ItemsBag[0].ItemOptions[0].Param);
        Assert.True(k.Inv.ItemsBag[0].ItemOptions[0].Param < 0);
    }

    [Fact]
    public void AddItemBag_ticket_in_the_bag_without_option_31_is_not_a_target()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(718, 1, (4, 1)));

        k.Service.AddItemBag(k.Player, k.Item(718, 5));

        // not folded; the incremental rule of AddItemList then adds the parameter 0 to a ticket that has no option 31: the item vanishes
        Assert.Equal(1, k.CountReal(k.Inv.ItemsBag));
        Assert.Equal(4, k.Inv.ItemsBag[0].ItemOptions[0].OptionTemplate.Id);
    }

    // ------------------------------------------------------------------ special items

    [Theory]
    [InlineData("BUA_1H", 60)]
    [InlineData("BUA_DETU", 60)]
    [InlineData("BUA_8H", 480)]
    [InlineData("BUA_1M", 43_200)]
    [InlineData("OTHER", 0)]
    [InlineData(null, 0)]
    public void AddItemBag_charm_adds_minutes_by_the_open_shop_tag(string? tag, int minutes)
    {
        var k = new InvKit();
        k.Define(213, 13);
        k.Host.ShopTag = tag;

        Assert.True(k.Service.AddItemBag(k.Player, k.Item(213)));

        Assert.Equal(new[] { "AddTimeCharms:213:" + minutes }, k.Host.Calls);
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBag));
    }

    [Fact]
    public void AddItemBag_charm_with_a_failing_shop_lookup_gets_zero_minutes()
    {
        var k = new InvKit();
        k.Define(213, 13);
        k.Host.ShopTagThrows = true;

        Assert.True(k.Service.AddItemBag(k.Player, k.Item(213)));

        Assert.Equal(new[] { "AddTimeCharms:213:0" }, k.Host.Calls);
    }

    [Fact]
    public void AddItemBag_bill_egg_is_created_once()
    {
        var k = new InvKit();
        Assert.True(k.Service.AddItemBag(k.Player, k.Item(2500)));
        Assert.Equal(new[] { "CreateBillEgg" }, k.Host.Calls);

        k.Player.BillEgg = InvKit.Raw<BillEgg>();
        Assert.True(k.Service.AddItemBag(k.Player, k.Item(2500)));
        Assert.Single(k.Host.Calls);
    }

    [Fact]
    public void AddItemBag_tennis_space_ship_sets_the_flag()
    {
        var k = new InvKit();
        Assert.True(k.Service.AddItemBag(k.Player, k.Item(453)));
        Assert.True(k.Player.HaveTennisSpaceShip);
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBag));
    }

    [Theory]
    [InlineData(74, "SetFullHpMp")]
    [InlineData(191, "SetFullHp")]
    [InlineData(192, "SetFullMp")]
    public void AddItemBag_food_refills_hp_and_mp(int id, string call)
    {
        var k = new InvKit();
        Assert.True(k.Service.AddItemBag(k.Player, k.Item(id)));
        Assert.Equal(new[] { call }, k.Host.Calls);
        Assert.Equal(new[] { "SendInfoHpMp" }, k.Notifier.Calls);
    }

    [Theory]
    [InlineData(516)]
    [InlineData(670)]
    public void AddItemBag_chocolate_refills_and_gives_500k_to_1m_potential(int id)
    {
        var script = new ScriptedRandomProvider().Enqueue(123_456);
        var k = new InvKit(script);

        Assert.True(k.Service.AddItemBag(k.Player, k.Item(id)));

        Assert.Equal(new[] { "SetFullHpMp" }, k.Host.Calls);
        Assert.Equal(new[] { "AddPotential:2:623456", "SendInfoHpMp" }, k.Notifier.Calls);
        Assert.Equal(new[] { ("NextInt", (object?)500_001) }, script.Calls.Select(c => (c.Method, c.Bound)));
    }

    [Theory]
    [InlineData(1784, 7)]
    [InlineData(1798, 15)]
    public void AddItemBag_set_search_item_extends_the_deadline_and_tells_the_player(int id, int days)
    {
        var k = new InvKit();
        k.Host.SkhResult = (1_700_000_000_000L, 30);

        Assert.True(k.Service.AddItemBag(k.Player, k.Item(id)));

        Assert.Equal(new[] { "AddActivationSetSearchDays:" + days }, k.Host.Calls);
        string expected = "SendNotification:Thời gian tìm set kích hoạt đến " + TimeUtil.GetDeadline(1_700_000_000_000L, 30);
        Assert.Equal(new[] { expected }, k.Notifier.Calls);
    }

    [Fact]
    public void AddItemBag_set_search_item_without_a_session_says_nothing()
    {
        var k = new InvKit();
        k.Host.SkhResult = null;

        Assert.True(k.Service.AddItemBag(k.Player, k.Item(1784)));

        Assert.Equal(new[] { "AddActivationSetSearchDays:7" }, k.Host.Calls);
        Assert.Empty(k.Notifier.Calls);
    }

    [Theory]
    [InlineData(1706, 220, 71, 0, 99)]
    [InlineData(1707, 221, 70, 0, 99)]
    [InlineData(1708, 222, 69, 0, 99)]
    [InlineData(1709, 223, 68, 0, 99)]
    [InlineData(1710, 224, 67, 0, 99)]
    [InlineData(1703, 595, 2, 512, 30)]
    [InlineData(1704, 1796, 2, 1024, 30)]
    public void AddItemBag_opening_items_put_a_stack_of_the_reward_in_the_bag(int id, int rewardId, int optionId, int optionParam, int quantity)
    {
        var k = new InvKit();

        Assert.True(k.Service.AddItemBag(k.Player, k.Item(id)));

        Item reward = k.Inv.ItemsBag[0];
        Assert.Equal(rewardId, reward.Template!.Id);
        Assert.Equal(quantity, reward.Quantity);
        Assert.Equal(optionId, reward.ItemOptions[0].OptionTemplate.Id);
        Assert.Equal(optionParam, reward.ItemOptions[0].Param);
        Assert.Equal(1, k.CountReal(k.Inv.ItemsBag));
        Assert.Contains(k.Notifier.Packets, p => p.Command == -36);
    }

    [Fact]
    public void AddItemBag_opening_item_with_a_full_bag_still_reports_success_and_the_reward_is_lost()
    {
        var k = new InvKit();
        for (int i = 0; i < k.Inv.ItemsBag.Count; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(100 + i));
        }

        Assert.True(k.Service.AddItemBag(k.Player, k.Item(1706)));
        Assert.DoesNotContain(k.Inv.ItemsBag, i => i.Template!.Id == 220);
    }
}
