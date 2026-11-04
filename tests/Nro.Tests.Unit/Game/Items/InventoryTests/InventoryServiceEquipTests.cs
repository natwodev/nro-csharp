using Nro.Game;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary>Wearing and taking off equipment (<c>itemBagToBody</c>, <c>itemBodyToBag</c> and the disciple versions).</summary>
public class InventoryServiceEquipTests
{
    private static readonly string[] PlainRefresh = ["SendCostume:1", "SendFlagBag", "Point", "SendCostume:1", "SendFlagBag"];

    private static InvKit WithArmor(out Item armor, int index = 2, sbyte type = 0, sbyte gender = 0, int strRequire = 0)
    {
        var k = new InvKit();
        k.Define(10, type, gender: gender, strRequire: strRequire, name: "Ao");
        armor = k.Put(k.Inv.ItemsBag, index, k.Item(10));
        return k;
    }

    [Fact]
    public void ItemBagToBody_wears_the_item_and_refreshes_everything_in_java_order()
    {
        InvKit k = WithArmor(out Item armor);

        k.Service.ItemBagToBody(k.Player, 2);

        Assert.Same(armor, k.Inv.ItemsBody[0]);
        Assert.False(k.Inv.ItemsBag[2].IsNotNullItem());
        Assert.Equal(PlainRefresh, k.Notifier.Calls);
        Assert.Equal(new sbyte[] { -36, -37 }, k.Notifier.Packets.Select(p => p.Command));
    }

    [Fact]
    public void ItemBagToBody_swaps_with_the_item_already_worn()
    {
        InvKit k = WithArmor(out Item armor);
        Item old = k.Put(k.Inv.ItemsBody, 0, k.Item(11, 1, (21, 0)));
        k.Define(11, 0);

        k.Service.ItemBagToBody(k.Player, 2);

        Assert.Same(armor, k.Inv.ItemsBody[0]);
        // the displaced item goes to the bag slot, and the bag is then sorted by sendItemBag
        Assert.Contains(old, k.Inv.ItemsBag);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 2)]
    [InlineData(3, 3)]
    [InlineData(4, 4)]
    [InlineData(5, 5)]
    [InlineData(32, 6)]
    [InlineData(21, 7)]
    [InlineData(11, 8)]
    [InlineData(23, 9)]
    [InlineData(24, 9)]
    [InlineData(35, 10)]
    [InlineData(39, 11)]
    public void ItemBagToBody_puts_each_type_in_its_body_slot(int type, int slot)
    {
        InvKit k = WithArmor(out Item armor, type: (sbyte)type);

        k.Service.ItemBagToBody(k.Player, 2);

        Assert.Same(armor, k.Inv.ItemsBody[slot]);
    }

    [Fact]
    public void ItemBagToBody_refuses_a_type_that_cannot_be_worn()
    {
        InvKit k = WithArmor(out Item armor, type: 50);

        k.Service.ItemBagToBody(k.Player, 2);

        Assert.Same(armor, k.Inv.ItemsBag[0]);
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBody));
        Assert.Equal("SendNotificationOk:Trang bị không phù hợp!", k.Notifier.Calls[0]);
        Assert.Equal(1, k.Notifier.NoticeTargets.Single());
    }

    [Theory]
    [InlineData(1, 0, false)]
    [InlineData(2, 0, false)]
    [InlineData(0, 0, true)]
    [InlineData(3, 0, true)]
    [InlineData(1, 1, true)]
    [InlineData(0, 2, false)]
    public void ItemBagToBody_checks_the_gender_and_accepts_3_for_everybody(int itemGender, int playerGender, bool worn)
    {
        InvKit k = WithArmor(out Item armor, gender: (sbyte)itemGender);
        k.Player.Gender = (sbyte)playerGender;

        k.Service.ItemBagToBody(k.Player, 2);

        Assert.Equal(worn, ReferenceEquals(k.Inv.ItemsBody[0], armor));
        Assert.Equal(!worn, k.Notifier.Calls.Contains("SendNotificationOk:Trang bị không phù hợp!"));
    }

    [Theory]
    [InlineData(1000L, 1000, true)]
    [InlineData(999L, 1000, false)]
    [InlineData(0L, 1, false)]
    [InlineData(0L, 0, true)]
    public void ItemBagToBody_needs_the_strength_of_the_template(long power, int strRequire, bool worn)
    {
        InvKit k = WithArmor(out Item armor, strRequire: strRequire);
        k.Host.PowerValue = power;

        k.Service.ItemBagToBody(k.Player, 2);

        Assert.Equal(worn, ReferenceEquals(k.Inv.ItemsBody[0], armor));
        Assert.Equal(!worn, k.Notifier.Calls.Contains("SendNotificationOk:Sức mạnh không đủ yêu cầu!"));
    }

    [Theory]
    [InlineData(2_000_000_000L, 2, true)]
    [InlineData(1_999_999_999L, 2, false)]
    [InlineData(80_000_000_000L, 80, true)]
    [InlineData(3_000_000_000L, 1, true)]
    public void ItemBagToBody_option_21_replaces_the_requirement_with_billions(long power, int param, bool worn)
    {
        var k = new InvKit();
        k.Define(10, 0, strRequire: 5);
        Item armor = k.Put(k.Inv.ItemsBag, 2, k.Item(10, 1, (21, param)));
        k.Host.PowerValue = power;

        k.Service.ItemBagToBody(k.Player, 2);

        Assert.Equal(worn, ReferenceEquals(k.Inv.ItemsBody[0], armor));
    }

    [Fact]
    public void ItemBagToBody_negative_index_and_empty_slot()
    {
        var k = new InvKit();

        k.Service.ItemBagToBody(k.Player, -1);
        Assert.Equal(new[] { "SendNotification:Không thể thực hiện" }, k.Notifier.Calls);

        k.Notifier.Calls.Clear();
        k.Service.ItemBagToBody(k.Player, 3);
        Assert.Empty(k.Notifier.Calls);
        Assert.Empty(k.Notifier.Packets);
    }

    [Fact]
    public void ItemBagToBody_index_past_the_bag_throws()
    {
        var k = new InvKit();
        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.ItemBagToBody(k.Player, 500));
    }

    [Theory]
    [InlineData(70)]
    [InlineData(75)]
    public void ItemBagToBody_types_70_to_75_pass_the_first_check_and_then_have_no_slot(int type)
    {
        InvKit k = WithArmor(out Item armor, type: (sbyte)type);

        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.ItemBagToBody(k.Player, 2));

        Assert.Same(armor, k.Inv.ItemsBag[2]);
    }

    [Fact]
    public void ItemBagToBody_chan_menh_items_change_the_title_and_send_the_effect()
    {
        InvKit k = WithArmor(out _, type: 39);
        k.Define(2005, 39);
        k.Inv.ItemsBag[2] = k.Item(2005);

        k.Service.ItemBagToBody(k.Player, 2);

        Assert.Equal("RemoveTitle", k.Notifier.Calls[0]);
        Assert.Equal("SendTrueSonOfHeaven:2005", k.Notifier.Calls[1]);
        // slot 11 now holds a type 39 item, so sendItemBody sends the effect again
        Assert.Equal(2, k.Notifier.Calls.Count(c => c == "SendTrueSonOfHeaven:2005"));
    }

    [Fact]
    public void ItemBagToBody_type_39_outside_2002_2010_has_no_title_change()
    {
        InvKit k = WithArmor(out _, type: 39);
        k.Define(2001, 39);
        k.Inv.ItemsBag[2] = k.Item(2001);

        k.Service.ItemBagToBody(k.Player, 2);

        Assert.DoesNotContain("RemoveTitle", k.Notifier.Calls);
    }

    [Fact]
    public void ItemBagToBody_replacing_the_item_in_slot_8_removes_its_effect()
    {
        var k = new InvKit();
        k.Define(10, 11, name: "Flag");
        k.Define(11, 11, part: 321);
        k.Put(k.Inv.ItemsBody, 8, k.Item(11));
        k.Put(k.Inv.ItemsBag, 0, k.Item(10));

        k.Service.ItemBagToBody(k.Player, 0);

        Assert.Equal("RemovePlayerEffect:321", k.Notifier.Calls[0]);
    }

    [Fact]
    public void ItemBagToBody_item_with_option_210_rolls_new_random_options()
    {
        var k = new InvKit();
        k.Define(10, 0);
        Item armor = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 1, (210, 3), (4, 9)));

        k.Service.ItemBagToBody(k.Player, 0);

        Assert.DoesNotContain(armor.ItemOptions, o => o.OptionTemplate.Id == 210);
        Assert.Equal(4, armor.ItemOptions.Count);
        Assert.Equal(4, armor.ItemOptions[0].OptionTemplate.Id);
        int[] pool = [50, 77, 103, 14, 5, 94, 97, 108, 95, 96, 101, 204, 80, 81, 45, 46, 197];
        Assert.All(armor.ItemOptions.Skip(1), o => Assert.Contains(o.OptionTemplate.Id, pool));
        Assert.Equal(3, armor.ItemOptions.Skip(1).Select(o => o.OptionTemplate.Id).Distinct().Count());
    }

    [Fact]
    public void ItemBagToBody_option_210_roll_draws_the_shuffle_first_and_then_one_parameter_per_option()
    {
        // identity shuffle: nextInt(i) = i - 1 for i = 17 .. 2; then the parameters of the first two ids of the pool (50 and 77, both 5..10 -> bound 6)
        var script = new ScriptedRandomProvider();
        for (int i = 17; i >= 2; i--)
        {
            script.Enqueue(i - 1);
        }

        script.Enqueue(3, 5);
        var k = new InvKit(script);
        k.Define(10, 0);
        Item armor = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 1, (210, 2)));

        k.Service.ItemBagToBody(k.Player, 0);

        Assert.Equal(0, script.Remaining);
        Assert.Equal(new[] { (50, 8), (77, 10) }, armor.ItemOptions.Select(o => (o.OptionTemplate.Id, o.Param)));
        Assert.Equal(Enumerable.Range(2, 16).Reverse().Select(b => (object?)b).Concat(new object?[] { 6, 6 }), script.Calls.Select(c => c.Bound));
    }

    [Fact]
    public void ItemBagToBody_option_210_with_more_rolls_than_ids_repeats_ids_at_random()
    {
        var k = new InvKit();
        k.Define(10, 0);
        Item armor = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 1, (210, 20)));

        k.Service.ItemBagToBody(k.Player, 0);

        Assert.Equal(20, armor.ItemOptions.Count);
        Assert.Equal(17, armor.ItemOptions.Take(17).Select(o => o.OptionTemplate.Id).Distinct().Count());
    }

    [Theory]
    [InlineData(89, 3, true, 7)]
    [InlineData(0, 0, true, 1)]
    [InlineData(89, 5, true, 30)]
    [InlineData(90, 0, false, 0)]
    [InlineData(99, 0, false, 0)]
    public void ItemBagToBody_option_231_becomes_an_expiry_with_ninety_percent(int firstDraw, int secondDraw, bool expiry, int days)
    {
        var script = new ScriptedRandomProvider().Enqueue(firstDraw);
        if (expiry)
        {
            script.Enqueue(secondDraw);
        }

        var k = new InvKit(script);
        k.Define(10, 0);
        Item armor = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 1, (231, 1), (4, 9)));

        k.Service.ItemBagToBody(k.Player, 0);

        Assert.DoesNotContain(armor.ItemOptions, o => o.OptionTemplate.Id == 231);
        if (expiry)
        {
            ItemOption last = armor.ItemOptions.Last();
            Assert.Equal(93, last.OptionTemplate.Id);
            Assert.Equal(days, last.Param);
        }
        else
        {
            Assert.DoesNotContain(armor.ItemOptions, o => o.OptionTemplate.Id == 93);
        }

        Assert.Equal(0, script.Remaining);
    }

    [Fact]
    public void ItemBagToBody_a_disciple_sends_the_refusals_to_its_master()
    {
        InvKit k = WithArmor(out _, gender: 1);
        Player master = k.NewPlayer(9);
        k.Player.IsDeTu = true;
        k.Host.PetMaster = master;

        k.Service.ItemBagToBody(k.Player, 2);

        Assert.Equal(9, k.Notifier.NoticeTargets.Single());
    }

    [Fact]
    public void ItemBagToBody_cloak_for_players_cannot_be_worn_by_a_disciple()
    {
        var k = new InvKit();
        k.Define(545, 5, name: "Cai trang");
        k.Put(k.Inv.ItemsBag, 0, k.Item(545));
        k.Player.IsDeTu = true;
        k.Host.PetMaster = k.NewPlayer(9);

        k.Service.ItemBagToBody(k.Player, 0);

        Assert.Equal("SendNotification:Không thể cho đệ tử mặc Cải Trang này", k.Notifier.Calls[0]);
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBody));
    }

    [Fact]
    public void ItemBagToBody_pet_only_items_are_refused_for_a_player()
    {
        var k = new InvKit();
        k.Define(547, 5);
        k.Put(k.Inv.ItemsBag, 0, k.Item(547));

        k.Service.ItemBagToBody(k.Player, 0);

        Assert.Equal("SendNotification:Chỉ sử dụng được cho đệ tử", k.Notifier.Calls[0]);
        Assert.Equal(0, k.CountReal(k.Inv.ItemsBody));
    }

    [Theory]
    [InlineData(0, false, "Chỉ khi hợp thể mới sử dụng được")]
    [InlineData(4, true, null)]
    public void ItemBagToBody_option_38_needs_a_fusion(int fusionType, bool worn, string? message)
    {
        var k = new InvKit();
        k.Define(10, 0);
        Item item = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 1, (38, 0)));
        k.Host.TypeFusion = (sbyte)fusionType;

        k.Service.ItemBagToBody(k.Player, 0);

        Assert.Equal(worn, ReferenceEquals(k.Inv.ItemsBody[0], item));
        if (message != null)
        {
            Assert.Equal("SendNotification:" + message, k.Notifier.Calls[0]);
        }
    }

    [Fact]
    public void ItemBagToBody_option_38_message_for_a_disciple_differs()
    {
        var k = new InvKit();
        k.Define(10, 0);
        k.Put(k.Inv.ItemsBag, 0, k.Item(10, 1, (38, 0)));
        k.Player.IsDeTu = true;
        k.Host.PetMaster = k.NewPlayer(9);

        k.Service.ItemBagToBody(k.Player, 0);

        Assert.Equal("SendNotification:Không thể cho đệ tử sử dụng", k.Notifier.Calls[0]);
    }

    [Fact]
    public void ItemBagToBody_updates_the_set_bonus_when_the_fifth_piece_is_worn()
    {
        var k = new InvKit();
        for (int slot = 0; slot < 4; slot++)
        {
            k.Define(20 + slot, (sbyte)slot);
            k.Put(k.Inv.ItemsBody, slot, k.Item(20 + slot, 1, (127, 1), (139, 0)));
        }

        k.Define(24, 4);
        Item fifth = k.Put(k.Inv.ItemsBag, 0, k.Item(24, 1, (127, 1), (139, 0)));

        k.Service.ItemBagToBody(k.Player, 0);

        Assert.Same(fifth, k.Inv.ItemsBody[4]);
        Assert.All(k.Inv.ItemsBody.Take(5), i => Assert.Equal(1, i.GetOptionParam(139)));
    }

    // ------------------------------------------------------------------ body to bag

    [Fact]
    public void ItemBodyToBag_takes_the_item_off_and_refreshes()
    {
        var k = new InvKit();
        k.Define(10, 0);
        Item item = k.Put(k.Inv.ItemsBody, 0, k.Item(10));

        k.Service.ItemBodyToBag(k.Player, 0);

        Assert.False(k.Inv.ItemsBody[0].IsNotNullItem());
        Assert.Same(item, k.Inv.ItemsBag[0]);
        Assert.Equal(new[] { "SendCostume:1", "SendFlagBag", "Point", "SendCostume:1", "SendFlagBag" }, k.Notifier.Calls);
        Assert.Equal(new sbyte[] { -36, -37 }, k.Notifier.Packets.Select(p => p.Command));
    }

    [Fact]
    public void ItemBodyToBag_needs_a_free_bag_slot()
    {
        var k = new InvKit();
        for (int i = 0; i < k.Inv.ItemsBag.Count; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(100 + i));
        }

        Item item = k.Put(k.Inv.ItemsBody, 0, k.Item(10));
        k.Service.ItemBodyToBag(k.Player, 0);

        Assert.Same(item, k.Inv.ItemsBody[0]);
        Assert.Equal(new[] { "SendNotification:Hành trang của bạn không đủ chỗ trống" }, k.Notifier.Calls);
    }

    [Fact]
    public void ItemBodyToBag_empty_slot_does_nothing_and_bad_index_throws()
    {
        var k = new InvKit();
        k.Service.ItemBodyToBag(k.Player, 3);
        Assert.Empty(k.Notifier.Calls);
        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.ItemBodyToBag(k.Player, 40));
    }

    [Fact]
    public void ItemBodyToBag_taking_off_the_pet_slot_sends_the_following_pet_away()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBody, 7, k.Item(10));
        k.Player.PetFollow = k.NewFollow(55);

        k.Service.ItemBodyToBag(k.Player, 7);

        Assert.Null(k.Player.PetFollow);
        Assert.Equal(new[] { "ExitMap:55", "DisposePlayer:55" }, k.Host.Calls);
    }

    [Fact]
    public void ItemBodyToBag_a_disciple_keeps_its_follower()
    {
        var k = new InvKit();
        k.Player.IsDeTu = true;
        k.Put(k.Inv.ItemsBody, 7, k.Item(10));
        k.Player.PetFollow = k.NewFollow(55);

        k.Service.ItemBodyToBag(k.Player, 7);

        Assert.NotNull(k.Player.PetFollow);
        Assert.Empty(k.Host.Calls);
    }

    [Fact]
    public void ItemBodyToBag_type_39_removes_the_title_and_slot_8_removes_the_effect()
    {
        var k = new InvKit();
        k.Define(10, 39);
        k.Define(11, 11, part: 77);
        k.Put(k.Inv.ItemsBody, 11, k.Item(10));
        k.Put(k.Inv.ItemsBody, 8, k.Item(11));

        k.Service.ItemBodyToBag(k.Player, 11);
        Assert.Equal("RemoveTitle", k.Notifier.Calls[0]);

        k.Notifier.Calls.Clear();
        k.Service.ItemBodyToBag(k.Player, 8);
        Assert.Contains("RemovePlayerEffect:77", k.Notifier.Calls);
    }

    [Fact]
    public void ItemBodyToBag_resets_the_set_bonus_on_the_pieces_left_and_in_the_bag()
    {
        var k = new InvKit();
        for (int slot = 0; slot < 5; slot++)
        {
            k.Define(20 + slot, (sbyte)slot);
            k.Put(k.Inv.ItemsBody, slot, k.Item(20 + slot, 1, (127, 1), (139, 1)));
        }

        Item spare = k.Put(k.Inv.ItemsBag, 0, k.Item(30, 1, (139, 1)));

        k.Service.ItemBodyToBag(k.Player, 4);

        Assert.All(k.Inv.ItemsBody.Take(4), i => Assert.Equal(0, i.GetOptionParam(139)));
        Assert.Equal(0, spare.GetOptionParam(139));
    }

    // ------------------------------------------------------------------ disciple

    [Fact]
    public void ItemBagToPetBody_needs_a_disciple_with_1_500_000_power()
    {
        var k = new InvKit();
        k.Define(10, 0);
        Item item = k.Put(k.Inv.ItemsBag, 0, k.Item(10));

        k.Service.ItemBagToPetBody(k.Player, 0);
        Assert.Equal(new[] { "SendNotification:Đệ tử phải đạt 1tr5 sức mạnh mới có thể mặc" }, k.Notifier.Calls);

        Detu pet = k.NewDetu(2);
        k.Player.Detu = pet;
        k.Host.PetMaster = k.Player;
        k.Host.PowerOf[pet] = 1_499_999;
        k.Notifier.Calls.Clear();
        k.Service.ItemBagToPetBody(k.Player, 0);
        Assert.Equal(new[] { "SendNotification:Đệ tử phải đạt 1tr5 sức mạnh mới có thể mặc" }, k.Notifier.Calls);
        Assert.Same(item, k.Inv.ItemsBag[0]);
    }

    [Fact]
    public void ItemBagToPetBody_dresses_the_disciple()
    {
        var k = new InvKit();
        k.Define(10, 0);
        Item item = k.Put(k.Inv.ItemsBag, 0, k.Item(10));
        Detu pet = k.NewDetu(2);
        k.Player.Detu = pet;
        k.Host.PetMaster = k.Player;
        k.Host.PowerOf[pet] = 1_500_000;
        k.Player.TypeTabPet = 3;

        k.Service.ItemBagToPetBody(k.Player, 0);

        Assert.Same(item, pet.Inventory.ItemsBody[0]);
        Assert.False(k.Inv.ItemsBag[0].IsNotNullItem());
        Assert.Equal(0, k.Player.TypeTabPet);
        Assert.Contains("ShowInfoPet", k.Notifier.Calls);
        Assert.Contains("SendCostume:2", k.Notifier.Calls);
        Assert.Contains("SendCostume:1", k.Notifier.Calls);
    }

    [Fact]
    public void ItemBagToPetBody_unwearable_item_stays_and_no_info_is_shown()
    {
        var k = new InvKit();
        k.Define(10, 50);
        Item item = k.Put(k.Inv.ItemsBag, 0, k.Item(10));
        Detu pet = k.NewDetu(2);
        k.Player.Detu = pet;
        k.Host.PetMaster = k.Player;
        k.Host.PowerOf[pet] = 2_000_000;

        k.Service.ItemBagToPetBody(k.Player, 0);

        Assert.Same(item, k.Inv.ItemsBag[0]);
        Assert.DoesNotContain("ShowInfoPet", k.Notifier.Calls);
        Assert.Contains("SendNotificationOk:Trang bị không phù hợp!", k.Notifier.Calls);
    }

    [Fact]
    public void ItemBagToPetBody_failure_is_answered_with_a_message()
    {
        var k = new InvKit();
        Detu pet = k.NewDetu(2);
        k.Player.Detu = pet;
        k.Host.PetMaster = k.Player;
        k.Host.PowerOf[pet] = 2_000_000;

        k.Service.ItemBagToPetBody(k.Player, 500);

        Assert.Equal(new[] { "SendNotification:Không thể thực hiện" }, k.Notifier.Calls);
    }

    [Fact]
    public void ItemPetBodyToBag_moves_the_disciples_item_to_the_bag()
    {
        var k = new InvKit();
        Detu pet = k.NewDetu(2);
        k.Player.Detu = pet;
        k.Host.PetMaster = k.Player;
        k.Define(10, 0);
        Item item = k.Put(pet.Inventory.ItemsBody, 1, k.Item(10));

        k.Service.ItemPetBodyToBag(k.Player, 1);

        Assert.False(pet.Inventory.ItemsBody[1].IsNotNullItem());
        Assert.Same(item, k.Inv.ItemsBag[0]);
        Assert.Contains("SendCostume:2", k.Notifier.Calls);
        Assert.Contains("ShowInfoPet", k.Notifier.Calls);
    }

    [Fact]
    public void ItemPetBodyToBag_other_tabs_do_not_show_the_pet_info_and_a_full_bag_stops_it()
    {
        var k = new InvKit();
        Detu pet = k.NewDetu(2);
        k.Player.Detu = pet;
        k.Host.PetMaster = k.Player;
        k.Player.TypeTabPet = 1;
        k.Put(pet.Inventory.ItemsBody, 1, k.Item(10));

        k.Service.ItemPetBodyToBag(k.Player, 1);
        Assert.DoesNotContain("ShowInfoPet", k.Notifier.Calls);

        k.Notifier.Calls.Clear();
        for (int i = 0; i < k.Inv.ItemsBag.Count; i++)
        {
            k.Put(k.Inv.ItemsBag, i, k.Item(100 + i));
        }

        k.Put(pet.Inventory.ItemsBody, 2, k.Item(10));
        k.Service.ItemPetBodyToBag(k.Player, 2);
        Assert.Equal(new[] { "SendNotification:Hành trang của bạn không đủ chỗ trống" }, k.Notifier.Calls);
        Assert.True(pet.Inventory.ItemsBody[2].IsNotNullItem());
    }

    [Fact]
    public void ItemPetBodyToBag_without_a_disciple_throws()
    {
        var k = new InvKit();
        Assert.Throws<InvalidOperationException>(() => k.Service.ItemPetBodyToBag(k.Player, 0));
    }
}
