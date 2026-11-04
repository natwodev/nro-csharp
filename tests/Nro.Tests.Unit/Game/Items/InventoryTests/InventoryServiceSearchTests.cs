using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary>Find, exist, copy, remove, subtract and sort in <see cref="InventoryService"/>.</summary>
public class InventoryServiceSearchTests
{
    [Fact]
    public void FindItem_returns_the_first_real_item_with_that_template_and_null_when_absent()
    {
        var k = new InvKit();
        Item first = k.Put(k.Inv.ItemsBag, 3, k.Item(100));
        k.Put(k.Inv.ItemsBag, 5, k.Item(100));

        Assert.Same(first, k.Service.FindItem(k.Inv.ItemsBag, 100));
        Assert.Null(k.Service.FindItem(k.Inv.ItemsBag, 101));
        Assert.Null(k.Service.FindItem(new List<Item>(), 100));
    }

    [Fact]
    public void FindItem_skips_empty_slots_even_when_their_template_would_match()
    {
        var k = new InvKit();
        Assert.Null(k.Service.FindItem(k.Inv.ItemsBag, 0));
    }

    [Fact]
    public void FindItem_and_FindItemEvent_stop_with_null_at_a_null_element_like_the_java_catch()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, null!);
        k.Put(k.Inv.ItemsBag, 2, k.Item(100));

        Assert.Null(k.Service.FindItem(k.Inv.ItemsBag, 100));
        Assert.Null(k.Service.FindItemEvent(k.Inv.ItemsBag, 3));
    }

    [Fact]
    public void FindItemEvent_matches_the_event_type_of_the_template()
    {
        var k = new InvKit();
        k.Define(200, typeEvent: 4);
        k.Define(201, typeEvent: 6);
        Item second = k.Put(k.Inv.ItemsBag, 2, k.Item(201));
        k.Put(k.Inv.ItemsBag, 1, k.Item(300));

        Assert.Same(second, k.Service.FindItemEvent(k.Inv.ItemsBag, 6));
        Assert.Null(k.Service.FindItemEvent(k.Inv.ItemsBag, 9));
    }

    [Fact]
    public void Find_and_exist_per_place_look_in_the_right_list()
    {
        var k = new InvKit();
        Item body = k.Put(k.Inv.ItemsBody, 1, k.Item(10));
        Item bag = k.Put(k.Inv.ItemsBag, 1, k.Item(11));
        Item box = k.Put(k.Inv.ItemsBox, 1, k.Item(12));
        Item collection = k.Put(k.Inv.ItemsBoxCollection, 1, k.Item(13));
        k.Define(14, typeEvent: 2);
        k.Put(k.Inv.ItemsBody, 2, k.Item(14));
        k.Put(k.Inv.ItemsBag, 2, k.Item(14));
        k.Put(k.Inv.ItemsBox, 2, k.Item(14));
        k.Put(k.Inv.ItemsBoxCollection, 2, k.Item(14));

        Assert.Same(body, k.Service.FindItemBody(k.Player, 10));
        Assert.Same(bag, k.Service.FindItemBag(k.Player, 11));
        Assert.Same(box, k.Service.FindItemBox(k.Player, 12));
        Assert.Same(collection, k.Service.FindItemBoxCollection(k.Player, 13));
        Assert.Null(k.Service.FindItemBag(k.Player, 10));
        Assert.NotNull(k.Service.FindItemBodyEvent(k.Player, 2));
        Assert.NotNull(k.Service.FindItemBagEvent(k.Player, 2));
        Assert.NotNull(k.Service.FindItemBoxEvent(k.Player, 2));
        Assert.NotNull(k.Service.FindItemBoxCollectionEvent(k.Player, 2));

        Assert.True(k.Service.BodyItemExists(k.Player, 10));
        Assert.True(k.Service.BagItemExists(k.Player, 11));
        Assert.True(k.Service.BoxItemExists(k.Player, 12));
        Assert.True(k.Service.CollectionBoxItemExists(k.Player, 13));
        Assert.False(k.Service.BagItemExists(k.Player, 12));
        Assert.True(k.Service.ItemExists(k.Inv.ItemsBag, 11));
        Assert.False(k.Service.ItemExists(k.Inv.ItemsBag, 99));
    }

    [Fact]
    public void FindItem_player_overload_searches_bag_and_box_but_not_body()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBody, 0, k.Item(10));
        k.Put(k.Inv.ItemsBox, 0, k.Item(12));

        Assert.True(k.Service.FindItem(k.Player, 12));
        Assert.False(k.Service.FindItem(k.Player, 10));
        k.Put(k.Inv.ItemsBag, 0, k.Item(10));
        Assert.True(k.Service.FindItem(k.Player, 10));
    }

    [Fact]
    public void Clan_box_lookups_use_the_clan_list_and_need_a_clan()
    {
        var k = new InvKit();
        Assert.Throws<InvalidOperationException>(() => k.Service.FindItemBoxClan(k.Player, 5));

        k.Player.Clan = InvKit.Raw<Clan>();
        k.Host.ClanBox.AddRange(new[] { k.Empty(), k.Item(5), k.Empty() });

        Assert.Same(k.Host.ClanBox[1], k.Service.FindItemBoxClan(k.Player, 5));
        Assert.True(k.Service.ClanBoxItemExists(k.Player, 5));
        Assert.False(k.Service.ClanBoxItemExists(k.Player, 6));
        Assert.Equal(1, k.Service.GetIndexItemBoxClan(k.Player, k.Host.ClanBox[1]));
    }

    [Fact]
    public void FindItemByOption_returns_the_first_bag_item_with_the_option()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(10, 1, (30, 0)));
        Item wanted = k.Put(k.Inv.ItemsBag, 1, k.Item(11, 1, (4, 1), (31, 5)));

        Assert.Same(wanted, k.Service.FindItemByOption(k.Player, 31));
        Assert.Null(k.Service.FindItemByOption(k.Player, 99));
        Assert.Null(k.Service.FindItemByOption(null!, 31));
    }

    [Fact]
    public void HasOption_is_false_for_null_empty_slot_and_missing_option()
    {
        var k = new InvKit();
        Item item = k.Item(10, 1, (30, 0));
        Assert.True(k.Service.HasOption(item, 30));
        Assert.False(k.Service.HasOption(item, 31));
        Assert.False(k.Service.HasOption(k.Empty(), 30));
        Assert.False(k.Service.HasOption(null!, 30));
    }

    [Fact]
    public void RemoveItem_player_empties_the_bag_slot_and_sends_the_bag()
    {
        var k = new InvKit();
        Item item = k.Put(k.Inv.ItemsBag, 4, k.Item(10));

        k.Service.RemoveItem(k.Player, item);

        Assert.False(k.Inv.ItemsBag[4].IsNotNullItem());
        Assert.Single(k.Notifier.Packets);
        Assert.Equal(-36, k.Notifier.Packets[0].Command);
    }

    [Fact]
    public void RemoveItem_player_does_nothing_for_unknown_item_or_nulls()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 4, k.Item(10));

        k.Service.RemoveItem(k.Player, k.Item(10));
        k.Service.RemoveItem(k.Player, null!);
        k.Service.RemoveItem((Player)null!, k.Item(10));

        Assert.True(k.Inv.ItemsBag[4].IsNotNullItem());
        Assert.Empty(k.Notifier.Packets);
    }

    [Fact]
    public void RemoveItem_by_index_swaps_in_a_new_empty_item_and_does_not_dispose_the_old_one()
    {
        var k = new InvKit();
        Item item = k.Put(k.Inv.ItemsBag, 2, k.Item(10));

        k.Service.RemoveItem(k.Inv.ItemsBag, 2);

        Assert.False(k.Inv.ItemsBag[2].IsNotNullItem());
        Assert.NotSame(item, k.Inv.ItemsBag[2]);
        Assert.True(item.IsNotNullItem());
        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.RemoveItem(k.Inv.ItemsBag, 99));
        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.RemoveItem(k.Inv.ItemsBag, -1));
    }

    [Fact]
    public void RemoveItem_by_reference_disposes_the_item_and_matches_only_the_same_object()
    {
        var k = new InvKit();
        Item item = k.Put(k.Inv.ItemsBag, 2, k.Item(10, 1, (30, 0)));
        Item lookalike = k.Item(10, 1, (30, 0));

        k.Service.RemoveItem(k.Inv.ItemsBag, lookalike);
        Assert.True(k.Inv.ItemsBag[2].IsNotNullItem());

        k.Service.RemoveItem(k.Inv.ItemsBag, item);
        Assert.False(k.Inv.ItemsBag[2].IsNotNullItem());
        Assert.False(item.IsNotNullItem());
        Assert.Empty(item.ItemOptions);

        k.Service.RemoveItem(k.Inv.ItemsBag, (Item)null!);
    }

    [Fact]
    public void RemoveItem_by_reference_removes_only_the_first_slot_when_the_same_object_is_in_two_slots()
    {
        var k = new InvKit();
        Item item = k.Item(10);
        k.Inv.ItemsBag[1] = item;
        k.Inv.ItemsBag[3] = item;

        k.Service.RemoveItem(k.Inv.ItemsBag, item);

        Assert.False(k.Inv.ItemsBag[1].IsNotNullItem());
        Assert.Same(item, k.Inv.ItemsBag[3]);
    }

    [Fact]
    public void Remove_per_place_removes_in_the_right_list()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(10));
        k.Put(k.Inv.ItemsBody, 0, k.Item(10));
        k.Put(k.Inv.ItemsBox, 0, k.Item(10));
        k.Put(k.Inv.ItemsBoxCollection, 0, k.Item(10));
        Item bagItem = k.Put(k.Inv.ItemsBag, 1, k.Item(11));

        k.Service.RemoveItemBag(k.Player, 0);
        k.Service.RemoveItemBody(k.Player, 0);
        k.Service.RemoveItemBox(k.Player, 0);
        k.Service.RemoveItemBoxCollection(k.Player, 0);
        k.Service.RemoveItemBag(k.Player, bagItem);

        Assert.Equal(0, k.CountReal(k.Inv.ItemsBag) + k.CountReal(k.Inv.ItemsBody) + k.CountReal(k.Inv.ItemsBox) + k.CountReal(k.Inv.ItemsBoxCollection));
    }

    [Fact]
    public void RemoveItemPetBody_works_on_the_disciple_and_needs_one()
    {
        var k = new InvKit();
        Assert.Throws<InvalidOperationException>(() => k.Service.RemoveItemPetBody(k.Player, 0));

        Player pet = k.NewPlayer(2);
        k.Player.Detu = InvKit.Raw<Detu>();
        k.Player.Detu.Inventory = pet.Inventory;
        k.Put(pet.Inventory.ItemsBody, 3, k.Item(10));

        k.Service.RemoveItemPetBody(k.Player, 3);

        Assert.False(pet.Inventory.ItemsBody[3].IsNotNullItem());
    }

    [Fact]
    public void RemoveItemBoxClan_empties_the_slot_of_the_clan_box()
    {
        var k = new InvKit();
        k.Player.Clan = InvKit.Raw<Clan>();
        k.Host.ClanBox.AddRange(new[] { k.Item(5), k.Item(6) });

        k.Service.RemoveItemBoxClan(k.Player, 1);

        Assert.True(k.Host.ClanBox[0].IsNotNullItem());
        Assert.False(k.Host.ClanBox[1].IsNotNullItem());
    }

    [Fact]
    public void SubQuantityItem_lowers_the_stack_and_removes_the_item_at_zero_or_below()
    {
        var k = new InvKit();
        Item item = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 10));

        k.Service.SubQuantityItem(k.Inv.ItemsBag, item, 3);
        Assert.Equal(7, item.Quantity);
        Assert.True(item.IsNotNullItem());

        k.Service.SubQuantityItem(k.Inv.ItemsBag, item, 7);
        Assert.False(k.Inv.ItemsBag[0].IsNotNullItem());
        Assert.False(item.IsNotNullItem());
    }

    [Fact]
    public void SubQuantityItem_below_zero_removes_and_negative_amount_adds()
    {
        var k = new InvKit();
        Item item = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 5));
        k.Service.SubQuantityItem(k.Inv.ItemsBag, item, -4);
        Assert.Equal(9, item.Quantity);

        k.Service.SubQuantityItem(k.Inv.ItemsBag, item, 100);
        Assert.False(k.Inv.ItemsBag[0].IsNotNullItem());
    }

    [Fact]
    public void SubQuantityItem_ignores_null_and_items_not_in_the_list()
    {
        var k = new InvKit();
        Item inList = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 5));
        Item other = k.Item(10, 5);

        k.Service.SubQuantityItem(k.Inv.ItemsBag, null!, 1);
        k.Service.SubQuantityItem(k.Inv.ItemsBag, other, 1);

        Assert.Equal(5, inList.Quantity);
        Assert.Equal(5, other.Quantity);
    }

    [Fact]
    public void SubQuantityItem_int_overflow_wraps_like_java()
    {
        var k = new InvKit();
        Item item = k.Put(k.Inv.ItemsBag, 0, k.Item(10, int.MinValue + 5));
        k.Service.SubQuantityItem(k.Inv.ItemsBag, item, 10);
        // int.MinValue + 5 - 10 wraps to a large positive number, so the stack is NOT removed.
        Assert.Equal(int.MaxValue - 4, item.Quantity);
        Assert.True(k.Inv.ItemsBag[0].IsNotNullItem());
    }

    [Fact]
    public void SubQuantity_per_place_uses_the_right_list()
    {
        var k = new InvKit();
        Item bag = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 5));
        Item body = k.Put(k.Inv.ItemsBody, 0, k.Item(10, 5));
        Item box = k.Put(k.Inv.ItemsBox, 0, k.Item(10, 5));
        Item collection = k.Put(k.Inv.ItemsBoxCollection, 0, k.Item(10, 5));

        k.Service.SubQuantityItemsBag(k.Player, bag, 1);
        k.Service.SubQuantityItemsBody(k.Player, body, 2);
        k.Service.SubQuantityItemsBox(k.Player, box, 3);
        k.Service.SubQuantityItemsBoxCollection(k.Player, collection, 4);

        Assert.Equal(new[] { 4, 3, 2, 1 }, new[] { bag.Quantity, body.Quantity, box.Quantity, collection.Quantity });

        k.Player.Clan = InvKit.Raw<Clan>();
        Item clanItem = k.Item(10, 9);
        k.Host.ClanBox.Add(clanItem);
        k.Service.SubQuantityItemsBoxClan(k.Player, clanItem, 9);
        Assert.False(k.Host.ClanBox[0].IsNotNullItem());
    }

    [Fact]
    public void SortItems_swaps_the_first_gap_with_the_last_real_item_so_the_order_changes()
    {
        var k = new InvKit();
        var list = new List<Item> { k.Empty(), k.Item(101), k.Empty(), k.Item(102), k.Item(103) };

        k.Service.SortItems(list);

        // [E,101,E,102,103] -> swap(0,4) [103,101,E,102,E] -> swap(2,3) [103,101,102,E,E]
        Assert.Equal(new[] { 103, 101, 102, -1, -1 }, k.Ids(list));
    }

    [Theory]
    [InlineData("")]
    [InlineData("E")]
    [InlineData("A")]
    [InlineData("AB")]
    [InlineData("EE")]
    [InlineData("EA")]
    [InlineData("AEAEAE")]
    [InlineData("EEEAAA")]
    public void SortItems_always_puts_every_real_item_before_every_empty_slot(string layout)
    {
        var k = new InvKit();
        var list = new List<Item>();
        int next = 100;
        foreach (char c in layout)
        {
            list.Add(c == 'E' ? k.Empty() : k.Item(next++));
        }

        var before = k.Ids(list).Where(i => i != -1).OrderBy(i => i).ToList();
        k.Service.SortItems(list);
        var ids = k.Ids(list);

        int firstEmpty = ids.ToList().IndexOf(-1);
        if (firstEmpty >= 0)
        {
            Assert.All(ids.Skip(firstEmpty), i => Assert.Equal(-1, i));
        }

        Assert.Equal(before, ids.Where(i => i != -1).OrderBy(i => i).ToList());
        Assert.Equal(layout.Length, list.Count);
    }

    [Fact]
    public void SortItems_leaves_a_sorted_list_alone_and_handles_a_long_list_without_recursion_trouble()
    {
        var k = new InvKit();
        var sorted = new List<Item> { k.Item(101), k.Item(102), k.Empty() };
        k.Service.SortItems(sorted);
        Assert.Equal(new[] { 101, 102, -1 }, k.Ids(sorted));

        var big = new List<Item>();
        for (int i = 0; i < 4000; i++)
        {
            big.Add(i % 2 == 0 ? k.Empty() : k.Item(100 + (i % 500)));
        }

        k.Service.SortItems(big);
        int firstEmpty = k.Ids(big).ToList().IndexOf(-1);
        Assert.Equal(2000, firstEmpty);
        Assert.All(big.Skip(2000), i => Assert.False(i.IsNotNullItem()));
    }

    [Fact]
    public void CompactItemsWithPositiveQuantity_keeps_positive_quantities_in_order_and_fills_the_tail_with_null()
    {
        var k = new InvKit();
        var list = new List<Item> { k.Item(101, 1), k.Item(102, 0), k.Empty(), k.Item(103, 5), k.Item(104, -1) };

        k.Service.CompactItemsWithPositiveQuantity(list);

        Assert.Equal(5, list.Count);
        Assert.Equal(101, list[0].Template!.Id);
        Assert.Equal(103, list[1].Template!.Id);
        Assert.Null(list[2]);
        Assert.Null(list[3]);
        Assert.Null(list[4]);
    }

    [Fact]
    public void CopyList_is_a_deep_copy()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(10, 4, (30, 0), (31, 6)));

        List<Item> copy = k.Service.CopyItemsBag(k.Player);
        copy[0].ItemOptions[1].Param = 99;
        copy[0].Quantity = 1;

        Assert.Equal(k.Inv.ItemsBag.Count, copy.Count);
        Assert.Equal(6, k.Inv.ItemsBag[0].ItemOptions[1].Param);
        Assert.Equal(4, k.Inv.ItemsBag[0].Quantity);
        Assert.NotSame(k.Inv.ItemsBag[0], copy[0]);
        Assert.NotSame(k.Inv.ItemsBag[1], copy[1]);
    }

    [Fact]
    public void Copy_per_place_copies_the_right_list()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBody, 2, k.Item(10));
        k.Put(k.Inv.ItemsBox, 3, k.Item(11));
        k.Put(k.Inv.ItemsBoxCollection, 4, k.Item(12));
        k.Player.Clan = InvKit.Raw<Clan>();
        k.Host.ClanBox.Add(k.Item(13));

        Assert.Equal(10, k.Service.CopyItemsBody(k.Player)[2].Template!.Id);
        Assert.Equal(11, k.Service.CopyItemsBox(k.Player)[3].Template!.Id);
        Assert.Equal(12, k.Service.CopyItemsBoxCollection(k.Player)[4].Template!.Id);
        Assert.Equal(13, k.Service.CopyItemsBoxClan(k.Player)[0].Template!.Id);
    }

    // ---- throwItem ----

    [Fact]
    public void ThrowItem_from_the_body_removes_it_and_tells_the_player()
    {
        var k = new InvKit();
        k.Define(10, name: "Quan Ao");
        k.Put(k.Inv.ItemsBody, 2, k.Item(10));

        k.Service.ThrowItem(k.Player, 0, 2);

        Assert.False(k.Inv.ItemsBody[2].IsNotNullItem());
        Assert.Contains(k.Notifier.Packets, p => p.Command == -37);
        Assert.Equal("SendNotification:Đã vứt bỏ Quan Ao", k.Notifier.Calls[^1]);
        Assert.Equal(-1, k.Player.TempItemIndex);
        Assert.Null(k.Player.ItemThrow_Drop);
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(99, 127)]
    [InlineData(99, 107)]
    public void ThrowItem_remembers_the_item_for_pants_without_expiry_set_pieces_and_option_107(int type, int optionId)
    {
        var k = new InvKit();
        k.Define(10, (sbyte)type, name: "Special");
        Item item = k.Put(k.Inv.ItemsBody, 2, optionId == 0 ? k.Item(10) : k.Item(10, 1, (optionId, 1)));

        k.Service.ThrowItem(k.Player, 0, 2);

        Assert.Equal(2, k.Player.TempItemIndex);
        Assert.Same(item, k.Player.ItemThrow_Drop);
        Assert.False(k.Inv.ItemsBody[2].IsNotNullItem());
    }

    [Fact]
    public void ThrowItem_type_5_with_expiry_option_93_is_a_plain_throw()
    {
        var k = new InvKit();
        k.Define(10, 5);
        k.Put(k.Inv.ItemsBody, 2, k.Item(10, 1, (93, 3)));

        k.Service.ThrowItem(k.Player, 0, 2);

        Assert.Equal(-1, k.Player.TempItemIndex);
        Assert.Null(k.Player.ItemThrow_Drop);
    }

    [Fact]
    public void ThrowItem_from_the_bag_removes_sorts_and_sends_the_bag()
    {
        var k = new InvKit();
        k.Define(10, name: "Do");
        k.Put(k.Inv.ItemsBag, 0, k.Item(10));
        k.Put(k.Inv.ItemsBag, 1, k.Item(11));
        k.Put(k.Inv.ItemsBag, 2, k.Item(12));

        k.Service.ThrowItem(k.Player, 1, 0);

        // [10,11,12,E...] -> [E,11,12,E...] -> sort swaps the first gap with the last real item: [12,11,E,...]
        Assert.Equal(new[] { 12, 11, -1 }, k.Ids(k.Inv.ItemsBag).Take(3));
        Assert.Contains(k.Notifier.Packets, p => p.Command == -36);
        Assert.Equal("SendNotification:Đã vứt bỏ Do", k.Notifier.Calls[^1]);
    }

    [Theory]
    [InlineData(570)]
    [InlineData(457)]
    public void ThrowItem_refuses_the_wood_chest_and_the_gold_bar(int id)
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(id));

        k.Service.ThrowItem(k.Player, 1, 0);

        Assert.True(k.Inv.ItemsBag[0].IsNotNullItem());
        Assert.Equal(new[] { "SendNotification:Không thể vứt vật phẩm này." }, k.Notifier.Calls);
    }

    [Fact]
    public void ThrowItem_from_the_bag_remembers_set_pieces()
    {
        var k = new InvKit();
        Item item = k.Put(k.Inv.ItemsBag, 3, k.Item(10, 1, (128, 1)));

        k.Service.ThrowItem(k.Player, 1, 3);

        Assert.Same(item, k.Player.ItemThrow_Drop);
        Assert.Equal(3, k.Player.TempItemIndex);
    }

    [Fact]
    public void ThrowItem_unknown_place_does_nothing_and_bad_index_or_empty_slot_throws()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(10));

        k.Service.ThrowItem(k.Player, 2, 0);
        Assert.True(k.Inv.ItemsBag[0].IsNotNullItem());
        Assert.Empty(k.Notifier.Calls);

        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.ThrowItem(k.Player, 1, 500));
        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.ThrowItem(k.Player, 0, -1));
        Assert.Throws<InvalidOperationException>(() => k.Service.ThrowItem(k.Player, 1, 5));
    }
}
