using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary>Counting, the "has item X" family, set bonuses and the collection box tiers.</summary>
public class InventoryServiceQueryTests
{
    // ------------------------------------------------------------------ counts and indexes

    [Fact]
    public void GetCountEmpty_counts_slots_without_a_template()
    {
        var k = new InvKit();
        Assert.Equal((sbyte)InvKit.BagSlots, k.Service.GetCountEmptyBag(k.Player));
        Assert.Equal((sbyte)InvKit.BodySlots, k.Service.GetCountEmptyBody(k.Player));

        k.Put(k.Inv.ItemsBag, 0, k.Item(10));
        k.Put(k.Inv.ItemsBody, 0, k.Item(10));
        k.Put(k.Inv.ItemsBody, 5, k.Item(10));
        Assert.Equal((sbyte)(InvKit.BagSlots - 1), k.Service.GetCountEmptyBag(k.Player));
        Assert.Equal((sbyte)(InvKit.BodySlots - 2), k.Service.GetCountEmptyBody(k.Player));
        Assert.Equal(0, k.Service.GetCountEmptyListItem(new List<Item>()));
    }

    [Theory]
    [InlineData(127, 127)]
    [InlineData(128, -128)]
    [InlineData(129, -127)]
    [InlineData(255, -1)]
    [InlineData(256, 0)]
    public void GetCountEmptyListItem_is_a_java_byte_and_wraps_at_128(int empties, int expected)
    {
        var k = new InvKit();
        var list = new List<Item>();
        for (int i = 0; i < empties; i++)
        {
            list.Add(k.Empty());
        }

        Assert.Equal((sbyte)expected, k.Service.GetCountEmptyListItem(list));
    }

    [Fact]
    public void GetIndexBag_finds_by_reference_or_returns_minus_one()
    {
        var k = new InvKit();
        Item item = k.Put(k.Inv.ItemsBag, 7, k.Item(10));

        Assert.Equal(7, k.Service.GetIndexBag(k.Player, item));
        Assert.Equal(-1, k.Service.GetIndexBag(k.Player, k.Item(10)));
    }

    [Fact]
    public void GetIndexBag_cannot_see_slots_past_127_like_the_java_byte_loop()
    {
        var k = new InvKit();
        k.Fill(k.Inv.ItemsBag, 140);
        Item far = k.Put(k.Inv.ItemsBag, 130, k.Item(10));

        // the loop variable wraps from 127 to -128, which is a negative index
        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.GetIndexBag(k.Player, far));
        Item near = k.Put(k.Inv.ItemsBag, 100, k.Item(11));
        Assert.Equal(100, k.Service.GetIndexBag(k.Player, near));
    }

    [Fact]
    public void GetIndexItem_per_place_uses_reference_equality()
    {
        var k = new InvKit();
        Item a = k.Put(k.Inv.ItemsBag, 1, k.Item(10));
        Item b = k.Put(k.Inv.ItemsBody, 2, k.Item(10));
        Item c = k.Put(k.Inv.ItemsBox, 3, k.Item(10));
        Item d = k.Put(k.Inv.ItemsBoxCollection, 4, k.Item(10));

        Assert.Equal(1, k.Service.GetIndexItemBag(k.Player, a));
        Assert.Equal(2, k.Service.GetIndexItemBody(k.Player, b));
        Assert.Equal(3, k.Service.GetIndexItemBox(k.Player, c));
        Assert.Equal(4, k.Service.GetIndexItemBoxCollection(k.Player, d));
        Assert.Equal(-1, k.Service.GetIndexItemBag(k.Player, k.Item(10)));
        Assert.Equal(-1, k.Service.GetIndexItem(k.Player, k.Inv.ItemsBag, b));
    }

    [Fact]
    public void GetBagItemOptionParameter_returns_the_first_matching_option_of_a_bag_item_or_zero()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(10, 1, (4, 5)));
        k.Put(k.Inv.ItemsBag, 1, k.Item(10, 1, (31, 77)));
        k.Put(k.Inv.ItemsBag, 2, k.Item(11, 1, (31, 99)));

        Assert.Equal(77, k.Service.GetBagItemOptionParameter(k.Player, 31, 10));
        Assert.Equal(99, k.Service.GetBagItemOptionParameter(k.Player, 31, 11));
        Assert.Equal(0, k.Service.GetBagItemOptionParameter(k.Player, 32, 10));
        Assert.Equal(0, k.Service.GetBagItemOptionParameter(k.Player, 31, 12));
    }

    [Fact]
    public void SubtractBagItemOptionParameter_lowers_the_option_and_removes_the_item_when_it_reaches_zero()
    {
        var k = new InvKit();
        Item item = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 1, (31, 10)));

        k.Service.SubtractBagItemOptionParameter(k.Player, 10, 31, 4);
        Assert.Equal(6, item.ItemOptions[0].Param);
        Assert.True(item.IsNotNullItem());

        k.Service.SubtractBagItemOptionParameter(k.Player, 10, 31, 6);
        Assert.False(k.Inv.ItemsBag[0].IsNotNullItem());
        Assert.False(item.IsNotNullItem());
    }

    [Fact]
    public void SubtractBagItemOptionParameter_only_looks_at_the_first_item_with_that_template()
    {
        var k = new InvKit();
        Item first = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 1, (4, 5)));
        Item second = k.Put(k.Inv.ItemsBag, 1, k.Item(10, 1, (31, 10)));

        k.Service.SubtractBagItemOptionParameter(k.Player, 10, 31, 4);

        Assert.Equal(5, first.ItemOptions[0].Param);
        Assert.Equal(10, second.ItemOptions[0].Param);
    }

    [Fact]
    public void SubtractBagItemOptionParameter_goes_below_zero_and_removes()
    {
        var k = new InvKit();
        Item item = k.Put(k.Inv.ItemsBag, 0, k.Item(10, 1, (31, 3)));

        k.Service.SubtractBagItemOptionParameter(k.Player, 10, 31, 100);

        Assert.False(k.Inv.ItemsBag[0].IsNotNullItem());
        Assert.False(item.IsNotNullItem());
    }

    [Fact]
    public void SubtractBagItemOptionParameter_reaching_an_empty_slot_before_the_match_throws_like_the_java_npe()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 3, k.Item(10, 1, (31, 3)));

        Assert.Throws<InvalidOperationException>(() => k.Service.SubtractBagItemOptionParameter(k.Player, 10, 31, 1));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(22, true)]
    [InlineData(23, true)]
    [InlineData(14, true)]
    [InlineData(27, true)]
    [InlineData(28, true)]
    [InlineData(47, true)]
    [InlineData(6, false)]
    [InlineData(7, false)]
    [InlineData(-1, false)]
    public void CanUpgradeOption_lists_the_seven_upgradable_options(int id, bool expected)
    {
        Assert.Equal(expected, new InvKit().Service.CanUpgradeOption(id));
    }

    // ------------------------------------------------------------------ the "has item" family

    [Theory]
    [InlineData("bag", 570, false)]
    [InlineData("box", 570, false)]
    [InlineData("body", 570, true)]
    [InlineData("none", 570, true)]
    public void HasNoWoodChest_is_true_only_when_the_chest_is_absent(string where, int id, bool expected)
    {
        var k = new InvKit();
        switch (where)
        {
            case "bag": k.Put(k.Inv.ItemsBag, 0, k.Item(id)); break;
            case "box": k.Put(k.Inv.ItemsBox, 0, k.Item(id)); break;
            case "body": k.Put(k.Inv.ItemsBody, 0, k.Item(id)); break;
        }

        Assert.Equal(expected, k.Service.HasNoWoodChest(k.Player));
    }

    public static IEnumerable<object[]> Finders()
    {
        // name, template id, places where the finder looks: B = bag, X = box, Y = body
        yield return new object[] { "HasEnergyAbsorbingBottle", 1911, "B" };
        yield return new object[] { "HasApricotBranch", 1185, "BX" };
        yield return new object[] { "HasStandardDiscountCoupon", 459, "B" };
        yield return new object[] { "HasCerberusPet", 1654, "Y" };
        yield return new object[] { "HasVipDiscountCoupon", 721, "B" };
        yield return new object[] { "HasLevelTwoPorataEarrings", 921, "BX" };
        yield return new object[] { "HasKameElderCostume", 710, "BXY" };
        yield return new object[] { "HasDraburaFrostCostume", 1205, "Y" };
        yield return new object[] { "HasTimeSpaceDistortionRing", 992, "BX" };
        yield return new object[] { "HasHitCostume", 2077, "BX" };
        yield return new object[] { "HasChristmasStocking", 649, "B" };
        yield return new object[] { "HasGoldenHeadband", 543, "B" };
        yield return new object[] { "HasSnowflake", 745, "Y" };
        yield return new object[] { "HasMagicBottle", 1259, "B" };
        yield return new object[] { "HasMysticOrbBasket", 1348, "B" };
        yield return new object[] { "HasWoodenSword", 1304, "B" };
    }

    private static bool Call(InvKit k, string name) => name switch
    {
        "HasEnergyAbsorbingBottle" => k.Service.HasEnergyAbsorbingBottle(k.Player),
        "HasApricotBranch" => k.Service.HasApricotBranch(k.Player),
        "HasStandardDiscountCoupon" => k.Service.HasStandardDiscountCoupon(k.Player),
        "HasCerberusPet" => k.Service.HasCerberusPet(k.Player),
        "HasVipDiscountCoupon" => k.Service.HasVipDiscountCoupon(k.Player),
        "HasLevelTwoPorataEarrings" => k.Service.HasLevelTwoPorataEarrings(k.Player),
        "HasKameElderCostume" => k.Service.HasKameElderCostume(k.Player),
        "HasDraburaFrostCostume" => k.Service.HasDraburaFrostCostume(k.Player),
        "HasTimeSpaceDistortionRing" => k.Service.HasTimeSpaceDistortionRing(k.Player),
        "HasHitCostume" => k.Service.HasHitCostume(k.Player),
        "HasChristmasStocking" => k.Service.HasChristmasStocking(k.Player),
        "HasGoldenHeadband" => k.Service.HasGoldenHeadband(k.Player),
        "HasSnowflake" => k.Service.HasSnowflake(k.Player),
        "HasMagicBottle" => k.Service.HasMagicBottle(k.Player),
        "HasMysticOrbBasket" => k.Service.HasMysticOrbBasket(k.Player),
        "HasWoodenSword" => k.Service.HasWoodenSword(k.Player),
        _ => throw new ArgumentException(name),
    };

    [Theory]
    [MemberData(nameof(Finders))]
    public void Has_item_finders_look_in_exactly_their_places(string name, int id, string places)
    {
        foreach ((char place, Func<InvKit, List<Item>> list) in new (char, Func<InvKit, List<Item>>)[]
        {
            ('B', x => x.Inv.ItemsBag),
            ('X', x => x.Inv.ItemsBox),
            ('Y', x => x.Inv.ItemsBody),
        })
        {
            var k = new InvKit();
            Assert.False(Call(k, name), name + " on an empty inventory");

            k.Put(list(k), 2, k.Item(id));
            Assert.Equal(places.Contains(place), Call(k, name));

            k.Put(list(k), 2, k.Item(id + 1));
            Assert.False(Call(k, name));
        }
    }

    [Theory]
    [InlineData("HasEnergyAbsorbingBottle")]
    [InlineData("HasApricotBranch")]
    [InlineData("HasTimeSpaceDistortionRing")]
    [InlineData("HasHitCostume")]
    public void Some_finders_only_answer_for_real_players(string name)
    {
        var k = new InvKit();
        int id = name switch { "HasEnergyAbsorbingBottle" => 1911, "HasApricotBranch" => 1185, "HasTimeSpaceDistortionRing" => 992, _ => 2077 };
        k.Put(k.Inv.ItemsBag, 0, k.Item(id));
        k.Host.Pl = false;

        Assert.False(Call(k, name));
        k.Host.Pl = true;
        Assert.True(Call(k, name));
    }

    [Fact]
    public void HasMagicBottle_with_a_null_player_is_false()
    {
        Assert.False(new InvKit().Service.HasMagicBottle(null!));
    }

    [Fact]
    public void HasAllBabyDragons_needs_all_seven_dragon_items_in_the_bag()
    {
        var k = new InvKit();
        for (int id = 1872; id <= 1877; id++)
        {
            k.Put(k.Inv.ItemsBag, id - 1872, k.Item(id));
        }

        Assert.False(k.Service.HasAllBabyDragons(k.Player));
        k.Put(k.Inv.ItemsBag, 6, k.Item(1878));
        Assert.True(k.Service.HasAllBabyDragons(k.Player));

        k.Put(k.Inv.ItemsBag, 6, k.Item(1872));
        Assert.False(k.Service.HasAllBabyDragons(k.Player));

        var box = new InvKit();
        for (int id = 1872; id <= 1878; id++)
        {
            box.Put(box.Inv.ItemsBox, id - 1872, box.Item(id));
        }

        Assert.False(box.Service.HasAllBabyDragons(box.Player));
    }

    [Fact]
    public void HasNamekianEquipment_looks_at_the_isGender_flag_of_worn_items()
    {
        var k = new InvKit();
        Assert.False(k.Service.HasNamekianEquipment(k.Player));
        k.Put(k.Inv.ItemsBody, 0, k.Item(10));
        Assert.False(k.Service.HasNamekianEquipment(k.Player));
        k.Catalog.GetTemplate(10).IsGender = 1;
        Assert.True(k.Service.HasNamekianEquipment(k.Player));
        k.Catalog.GetTemplate(10).IsGender = 2;
        Assert.False(k.Service.HasNamekianEquipment(k.Player));
    }

    [Theory]
    [InlineData(554, false)]
    [InlineData(555, true)]
    [InlineData(567, true)]
    [InlineData(568, false)]
    public void HasDivineItem_covers_555_to_567(int id, bool expected)
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBody, 0, k.Item(id));
        Assert.Equal(expected, k.Service.HasDivineItem(k.Player));
    }

    [Theory]
    [InlineData(760, false)]
    [InlineData(761, true)]
    [InlineData(763, true)]
    [InlineData(764, false)]
    public void HasFaceMaskAvatar_covers_761_to_763(int id, bool expected)
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBody, 0, k.Item(id));
        Assert.Equal(expected, k.Service.HasFaceMaskAvatar(k.Player));
    }

    [Theory]
    [InlineData(385, false)]
    [InlineData(386, true)]
    [InlineData(394, true)]
    [InlineData(395, false)]
    public void HasChristmasHat_covers_386_to_394(int id, bool expected)
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBody, 0, k.Item(id));
        Assert.Equal(expected, k.Service.HasChristmasHat(k.Player));
    }

    [Fact]
    public void HasFullDivineSet_needs_level_13_in_the_first_five_slots()
    {
        var k = new InvKit();
        for (int slot = 0; slot < 5; slot++)
        {
            k.Define(30 + slot, (sbyte)slot, level: 13);
            k.Put(k.Inv.ItemsBody, slot, k.Item(30 + slot));
        }

        Assert.True(k.Service.HasFullDivineSet(k.Player));

        k.Catalog.GetTemplate(32).Level = 12;
        Assert.False(k.Service.HasFullDivineSet(k.Player));

        k.Catalog.GetTemplate(32).Level = 13;
        k.Inv.ItemsBody[4] = k.Empty();
        Assert.False(k.Service.HasFullDivineSet(k.Player));
    }

    [Theory]
    [InlineData(662, 99, false)]
    [InlineData(663, 99, true)]
    [InlineData(667, 99, true)]
    [InlineData(668, 99, false)]
    [InlineData(665, 98, false)]
    [InlineData(665, 100, true)]
    public void HasNinetyNineBillFoodItems_needs_99_of_one_food(int id, int quantity, bool expected)
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 0, k.Item(id, quantity));
        Assert.Equal(expected, k.Service.HasNinetyNineBillFoodItems(k.Player));
    }

    [Fact]
    public void CanOpenBeerusShop_needs_the_full_set_and_the_food()
    {
        var k = new InvKit();
        for (int slot = 0; slot < 5; slot++)
        {
            k.Define(30 + slot, (sbyte)slot, level: 13);
            k.Put(k.Inv.ItemsBody, slot, k.Item(30 + slot));
        }

        Assert.False(k.Service.CanOpenBeerusShop(k.Player));
        k.Put(k.Inv.ItemsBag, 0, k.Item(664, 99));
        Assert.True(k.Service.CanOpenBeerusShop(k.Player));
        k.Inv.ItemsBody[0] = k.Empty();
        Assert.False(k.Service.CanOpenBeerusShop(k.Player));
    }

    // ------------------------------------------------------------------ lantern check

    private static InvKit WithClanOnline(int members, out List<Player> players, bool sameZone = true)
    {
        var k = new InvKit();
        Clan clan = InvKit.Raw<Clan>();
        Zone zone = InvKit.Raw<Zone>();
        k.Player.Clan = clan;
        k.Player.Zone = zone;
        k.Host.Online[1] = k.Player;
        k.Host.MemberIds.Add(1);
        players = [k.Player];
        for (int i = 2; i <= members; i++)
        {
            Player p = k.NewPlayer(i);
            p.Zone = sameZone ? zone : InvKit.Raw<Zone>();
            k.Host.Online[i] = p;
            k.Host.MemberIds.Add(i);
            players.Add(p);
        }

        foreach (Player p in players)
        {
            k.Put(p.Inventory.ItemsBody, 3, k.Item(467));
        }

        return k;
    }

    [Fact]
    public void AreAllMembersInSameZoneAndHaveLantern_true_when_everybody_is_in_the_zone_with_a_lantern()
    {
        InvKit k = WithClanOnline(3, out _);
        Assert.True(k.Service.AreAllMembersInSameZoneAndHaveLantern(k.Player));
    }

    [Theory]
    [InlineData(467, true)]
    [InlineData(471, true)]
    [InlineData(800, true)]
    [InlineData(804, true)]
    [InlineData(1047, true)]
    [InlineData(1303, true)]
    [InlineData(1675, true)]
    [InlineData(472, false)]
    [InlineData(805, false)]
    public void AreAllMembersInSameZoneAndHaveLantern_accepts_the_thirteen_lantern_ids(int id, bool expected)
    {
        InvKit k = WithClanOnline(2, out List<Player> players);
        players[1].Inventory.ItemsBody[3] = k.Item(id);
        Assert.Equal(expected, k.Service.AreAllMembersInSameZoneAndHaveLantern(k.Player));
    }

    [Fact]
    public void AreAllMembersInSameZoneAndHaveLantern_false_for_an_offline_member_a_different_zone_or_a_missing_lantern()
    {
        InvKit offline = WithClanOnline(2, out _);
        offline.Host.MemberIds.Add(99);
        Assert.False(offline.Service.AreAllMembersInSameZoneAndHaveLantern(offline.Player));

        InvKit elsewhere = WithClanOnline(2, out _, sameZone: false);
        Assert.False(elsewhere.Service.AreAllMembersInSameZoneAndHaveLantern(elsewhere.Player));

        InvKit none = WithClanOnline(2, out List<Player> players);
        players[1].Inventory.ItemsBody[3] = none.Empty();
        Assert.False(none.Service.AreAllMembersInSameZoneAndHaveLantern(none.Player));
    }

    [Fact]
    public void AreAllMembersInSameZoneAndHaveLantern_false_without_a_clan_a_zone_or_members()
    {
        var k = new InvKit();
        Assert.False(k.Service.AreAllMembersInSameZoneAndHaveLantern(k.Player));
        Assert.False(k.Service.AreAllMembersInSameZoneAndHaveLantern(null!));

        k.Player.Clan = InvKit.Raw<Clan>();
        Assert.False(k.Service.AreAllMembersInSameZoneAndHaveLantern(k.Player));

        k.Player.Zone = InvKit.Raw<Zone>();
        Assert.False(k.Service.AreAllMembersInSameZoneAndHaveLantern(k.Player));
    }

    // ------------------------------------------------------------------ set bonuses

    private static void Pieces(InvKit k, int count, int marker, int bonus, int bonusParam = 0)
    {
        for (int slot = 0; slot < count; slot++)
        {
            k.Put(k.Inv.ItemsBody, slot, k.Item(40 + slot, 1, (marker, 1), (bonus, bonusParam)));
        }
    }

    [Theory]
    [InlineData(5, 1)]
    [InlineData(4, 0)]
    [InlineData(3, 0)]
    [InlineData(1, 0)]
    public void UpdateSet_turns_the_bonus_on_at_exactly_five_pieces(int pieces, int expected)
    {
        var k = new InvKit();
        Pieces(k, pieces, 127, 139, bonusParam: 7);

        k.Service.UpdateSet(k.Player, 127, 139);

        Assert.All(k.Inv.ItemsBody.Take(pieces), i => Assert.Equal(expected, i.GetOptionParam(139)));
    }

    [Fact]
    public void UpdateSet_six_marked_pieces_switches_the_bonus_off_because_the_test_is_equals_five()
    {
        var k = new InvKit();
        Pieces(k, 6, 127, 139, bonusParam: 1);

        k.Service.UpdateSet(k.Player, 127, 139);

        Assert.All(k.Inv.ItemsBody.Take(6), i => Assert.Equal(0, i.GetOptionParam(139)));
    }

    [Fact]
    public void UpdateSet_never_adds_a_missing_bonus_option_and_ignores_empty_slots()
    {
        var k = new InvKit();
        for (int slot = 0; slot < 5; slot++)
        {
            k.Put(k.Inv.ItemsBody, slot, k.Item(40 + slot, 1, (127, 1)));
        }

        k.Service.UpdateSet(k.Player, 127, 139);

        Assert.All(k.Inv.ItemsBody.Take(5), i => Assert.DoesNotContain(i.ItemOptions, o => o.OptionTemplate.Id == 139));
    }

    [Fact]
    public void UpdateSet_counts_an_item_once_even_if_it_has_the_marker_twice()
    {
        var k = new InvKit();
        for (int slot = 0; slot < 5; slot++)
        {
            k.Put(k.Inv.ItemsBody, slot, k.Item(40 + slot, 1, (127, 1), (127, 2), (139, 0)));
        }

        k.Service.UpdateSet(k.Player, 127, 139);

        // five items count as five (not ten), so the bonus is switched on
        Assert.All(k.Inv.ItemsBody.Take(5), i => Assert.Equal(1, i.GetOptionParam(139)));
    }

    [Fact]
    public void ResetSet_zeroes_the_bonus_below_five_pieces_on_the_body_and_always_on_the_bags_and_box()
    {
        var k = new InvKit();
        Pieces(k, 4, 127, 139, bonusParam: 1);
        Player other = k.NewPlayer(2);
        Item inBag = k.Put(k.Inv.ItemsBag, 0, k.Item(60, 1, (139, 1)));
        Item inOtherBag = k.Put(other.Inventory.ItemsBag, 0, k.Item(61, 1, (139, 1)));
        Item inOtherBox = k.Put(other.Inventory.ItemsBox, 0, k.Item(62, 1, (139, 1)));
        Item inOtherBody = k.Put(other.Inventory.ItemsBody, 0, k.Item(63, 1, (139, 1)));
        Item inOwnBox = k.Put(k.Inv.ItemsBox, 0, k.Item(64, 1, (139, 1)));

        k.Service.ResetSet(k.Player, other, other, 127, 139);

        Assert.All(k.Inv.ItemsBody.Take(4), i => Assert.Equal(0, i.GetOptionParam(139)));
        Assert.Equal(0, inBag.GetOptionParam(139));
        Assert.Equal(0, inOtherBag.GetOptionParam(139));
        Assert.Equal(0, inOtherBox.GetOptionParam(139));
        Assert.Equal(1, inOtherBody.GetOptionParam(139));
        Assert.Equal(1, inOwnBox.GetOptionParam(139));
    }

    [Fact]
    public void ResetSet_keeps_the_bonus_on_the_body_when_five_pieces_remain_but_still_clears_the_bag()
    {
        var k = new InvKit();
        Pieces(k, 5, 127, 139, bonusParam: 1);
        Item inBag = k.Put(k.Inv.ItemsBag, 0, k.Item(60, 1, (139, 1)));

        k.Service.ResetSet(k.Player, null, null, 127, 139);

        Assert.All(k.Inv.ItemsBody.Take(5), i => Assert.Equal(1, i.GetOptionParam(139)));
        Assert.Equal(0, inBag.GetOptionParam(139));
    }

    [Theory]
    [InlineData(1, 0, 0, 0)]
    [InlineData(2, 1, 0, 0)]
    [InlineData(3, 1, 0, 0)]
    [InlineData(4, 1, 1, 0)]
    [InlineData(5, 1, 1, 1)]
    public void UpdateSetNew_turns_on_the_bonuses_at_2_4_and_5_pieces(int pieces, int b1, int b2, int b3)
    {
        var k = new InvKit();
        for (int slot = 0; slot < pieces; slot++)
        {
            k.Put(k.Inv.ItemsBody, slot, k.Item(40 + slot, 1, (241 + (slot % 4), 1), (242, 0), (243, 0), (244, 0)));
        }

        k.Service.UpdateSetNew(k.Player, 241, 244, [242, 243, 244]);

        Assert.All(k.Inv.ItemsBody.Take(pieces), i =>
        {
            Assert.Equal(b1, i.GetOptionParam(242));
            Assert.Equal(b2, i.GetOptionParam(243));
            Assert.Equal(b3, i.GetOptionParam(244));
        });
    }

    [Fact]
    public void ResetSetNew_zeroes_only_the_bonuses_whose_threshold_is_lost_on_the_body_and_all_in_bags_and_box()
    {
        var k = new InvKit();
        for (int slot = 0; slot < 3; slot++)
        {
            k.Put(k.Inv.ItemsBody, slot, k.Item(40 + slot, 1, (241, 1), (242, 1), (243, 1), (244, 1)));
        }

        Item bag = k.Put(k.Inv.ItemsBag, 0, k.Item(60, 1, (242, 1), (243, 1), (244, 1)));
        Player other = k.NewPlayer(2);
        Item box = k.Put(other.Inventory.ItemsBox, 0, k.Item(61, 1, (242, 1)));

        k.Service.ResetSetNew(k.Player, null, other, 241, 244, [242, 243, 244]);

        Assert.All(k.Inv.ItemsBody.Take(3), i =>
        {
            Assert.Equal(1, i.GetOptionParam(242));
            Assert.Equal(0, i.GetOptionParam(243));
            Assert.Equal(0, i.GetOptionParam(244));
        });
        Assert.Equal(new[] { 0, 0, 0 }, new[] { bag.GetOptionParam(242), bag.GetOptionParam(243), bag.GetOptionParam(244) });
        Assert.Equal(0, box.GetOptionParam(242));
    }

    [Fact]
    public void ResetSetNew_with_one_piece_zeroes_all_three()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBody, 0, k.Item(40, 1, (241, 1), (242, 1), (243, 1), (244, 1)));

        k.Service.ResetSetNew(k.Player, null, null, 241, 244, [242, 243, 244]);

        Assert.Equal(new[] { 0, 0, 0 }, new[] { k.Inv.ItemsBody[0].GetOptionParam(242), k.Inv.ItemsBody[0].GetOptionParam(243), k.Inv.ItemsBody[0].GetOptionParam(244) });
    }

    // ------------------------------------------------------------------ collection box tiers

    private static InvKit WithCollection(int count)
    {
        var k = new InvKit();
        k.Fill(k.Inv.ItemsBoxCollection, 130);
        for (int i = 0; i < count; i++)
        {
            k.Put(k.Inv.ItemsBoxCollection, i, k.Item(100 + i));
        }

        return k;
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(9, 0)]
    [InlineData(10, 1)]
    [InlineData(19, 1)]
    [InlineData(20, 2)]
    [InlineData(29, 2)]
    [InlineData(30, 4)]
    [InlineData(39, 4)]
    [InlineData(40, 5)]
    [InlineData(59, 5)]
    [InlineData(60, 10)]
    [InlineData(79, 10)]
    [InlineData(80, 15)]
    [InlineData(99, 15)]
    [InlineData(100, 20)]
    [InlineData(119, 20)]
    [InlineData(120, 30)]
    [InlineData(130, 30)]
    public void GetCollectionBoxHealthBonusPercent_tiers(int count, int expected)
    {
        InvKit k = WithCollection(count);
        Assert.Equal(expected, k.Service.GetCollectionBoxHealthBonusPercent(k.Player));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 0)]
    [InlineData(5, 1)]
    [InlineData(9, 1)]
    [InlineData(10, 2)]
    [InlineData(19, 2)]
    [InlineData(20, 4)]
    [InlineData(34, 4)]
    [InlineData(35, 7)]
    [InlineData(49, 7)]
    [InlineData(50, 12)]
    [InlineData(59, 12)]
    [InlineData(60, 18)]
    [InlineData(79, 18)]
    [InlineData(80, 24)]
    [InlineData(99, 24)]
    [InlineData(100, 30)]
    [InlineData(119, 30)]
    [InlineData(120, 35)]
    public void GetCollectionBoxManaBonusPercent_tiers(int count, int expected)
    {
        InvKit k = WithCollection(count);
        Assert.Equal(expected, k.Service.GetCollectionBoxManaBonusPercent(k.Player));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(14, 0)]
    [InlineData(15, 1)]
    [InlineData(29, 1)]
    [InlineData(30, 2)]
    [InlineData(49, 2)]
    [InlineData(50, 5)]
    [InlineData(69, 5)]
    [InlineData(70, 7)]
    [InlineData(89, 7)]
    [InlineData(90, 15)]
    [InlineData(99, 15)]
    [InlineData(100, 18)]
    [InlineData(119, 18)]
    [InlineData(120, 25)]
    public void GetCollectionBoxDamageBonusPercent_tiers(int count, int expected)
    {
        InvKit k = WithCollection(count);
        Assert.Equal(expected, k.Service.GetCollectionBoxDamageBonusPercent(k.Player));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(19, 0)]
    [InlineData(20, 1)]
    [InlineData(39, 1)]
    [InlineData(40, 2)]
    [InlineData(69, 2)]
    [InlineData(70, 3)]
    [InlineData(99, 3)]
    [InlineData(100, 4)]
    [InlineData(119, 4)]
    [InlineData(120, 5)]
    public void GetCollectionBoxCriticalChanceBonusPercent_tiers(int count, int expected)
    {
        InvKit k = WithCollection(count);
        Assert.Equal(expected, k.Service.GetCollectionBoxCriticalChanceBonusPercent(k.Player));
    }

    [Fact]
    public void Collection_tiers_ignore_empty_slots_and_a_null_player_gives_zero()
    {
        InvKit k = WithCollection(10);
        k.Inv.ItemsBoxCollection[3] = k.Empty();
        Assert.Equal(0, k.Service.GetCollectionBoxHealthBonusPercent(k.Player));
        Assert.Equal(0, k.Service.GetCollectionBoxHealthBonusPercent(null!));
        Assert.Equal(0, k.Service.GetCollectionBoxManaBonusPercent(null!));
        Assert.Equal(0, k.Service.GetCollectionBoxDamageBonusPercent(null!));
        Assert.Equal(0, k.Service.GetCollectionBoxCriticalChanceBonusPercent(null!));
    }

    [Fact]
    public void CountItemsInBoxCollection_tells_the_player_the_number_of_items()
    {
        InvKit k = WithCollection(7);
        k.Service.CountItemsInBoxCollection(k.Player);
        Assert.Equal(new[] { "SendNotification:Trong rương sưu tầm hiện có 7 vật phẩm" }, k.Notifier.Calls);
        Assert.Throws<ArgumentNullException>(() => k.Service.CountItemsInBoxCollection(null!));
    }
}
