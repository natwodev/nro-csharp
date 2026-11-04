using Nro.Core.Templates;
using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.Core;

public class ItemTests
{
    private static ItemCatalog Catalog(Action<ItemTemplate>? configure = null, IEnumerable<ShopItemEntry>? shop = null) => ItemTestKit.Catalog(configure, shop);

    private static Item Make(short id, ItemCatalog? catalog = null, params (int, int)[] options)
        => ItemTestKit.MakeItem(catalog ?? Catalog(), id, 1234, options);

    // ----- construction, empty slot, dispose -----------------------------------------------------------------------------------------------------

    [Fact]
    public void A_new_item_has_no_template_no_options_and_the_given_creation_time()
    {
        var item = new Item(Catalog(), 987654321L);

        Assert.False(item.IsNotNullItem());
        Assert.Empty(item.ItemOptions);
        Assert.Equal(987654321L, item.CreateTime);
        Assert.Equal(0, item.Quantity);
        Assert.Equal(0, item.QuantityGD);
    }

    [Fact]
    public void An_item_made_from_a_template_id_loads_the_template()
    {
        var item = new Item(Catalog(), 5, 42);

        Assert.True(item.IsNotNullItem());
        Assert.Equal(42, item.Template!.Id);
        Assert.Equal("Item42", item.Name());
    }

    [Fact]
    public void An_item_made_from_a_template_id_needs_a_catalog()
    {
        Assert.Throws<InvalidOperationException>(() => new Item(null, 5, 42));
    }

    [Fact]
    public void Reading_the_template_of_an_empty_slot_throws_where_java_threw_null_pointer()
    {
        var empty = new Item(Catalog());

        Assert.Throws<InvalidOperationException>(() => empty.Name());
        Assert.Throws<InvalidOperationException>(() => empty.IsDTS());
        Assert.Throws<InvalidOperationException>(() => empty.GetContent());
    }

    [Fact]
    public void Dispose_turns_the_item_into_an_empty_slot_with_an_empty_option_list()
    {
        Item item = Make(5, null, (47, 10), (21, 5));
        item.Info = "i";
        item.Content = "c";

        item.Dispose();

        Assert.False(item.IsNotNullItem());
        Assert.Null(item.Info);
        Assert.Null(item.Content);
        Assert.Empty(item.ItemOptions);
    }

    [Fact]
    public void Info_is_the_option_texts_with_no_separator_and_content_names_the_power_requirement()
    {
        Item item = Make(7, null, (47, 10), (21, 5));

        Assert.Equal("Opt47 10Opt21 5", item.GetInfo());
        Assert.Equal("Yêu cầu sức mạnh 70 trở lên", item.GetContent());
    }

    // ----- template predicates -------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(649, false)]
    [InlineData(650, true)]
    [InlineData(662, true)]
    [InlineData(663, false)]
    public void Destruction_gear_is_template_650_to_662(short id, bool expected) => Assert.Equal(expected, Make(id).IsDHD1());

    [Theory]
    [InlineData(554, false)]
    [InlineData(555, true)]
    [InlineData(567, true)]
    [InlineData(568, false)]
    public void Divine_gear_is_template_555_to_567(short id, bool expected) => Assert.Equal(expected, Make(id).IsDTLNew());

    [Theory]
    [InlineData(1065, false)]
    [InlineData(1066, true)]
    [InlineData(1070, true)]
    [InlineData(1071, false)]
    public void Angel_pieces_are_1066_to_1070(short id, bool expected) => Assert.Equal(expected, Make(id).IsManhThienSu());

    [Theory]
    [InlineData(1078, false)]
    [InlineData(1079, true)]
    [InlineData(1083, true)]
    [InlineData(1084, false)]
    public void Lucky_stones_are_1079_to_1083(short id, bool expected) => Assert.Equal(expected, Make(id).IsDaMayMan());

    [Theory]
    [InlineData(1073, false)]
    [InlineData(1074, true)]
    [InlineData(1078, true)]
    [InlineData(1079, false)]
    public void Angel_upgrade_stones_are_1074_to_1078(short id, bool expected) => Assert.Equal(expected, Make(id).IsDaNangCapTS());

    [Theory]
    [InlineData(1070, false)]
    [InlineData(1071, true)]
    [InlineData(1073, true)]
    [InlineData(1074, false)]
    public void Recipes_are_1071_to_1073(short id, bool expected) => Assert.Equal(expected, Make(id).IsCongThuc());

    [Theory]
    [InlineData(1083, false)]
    [InlineData(1084, true)]
    [InlineData(1086, true)]
    [InlineData(1087, false)]
    public void Vip_recipes_are_1084_to_1086(short id, bool expected) => Assert.Equal(expected, Make(id).IsCongThucVip());

    [Theory]
    [InlineData(13, true, false, false)]
    [InlineData(14, false, true, false)]
    [InlineData(15, false, false, true)]
    [InlineData(12, false, false, false)]
    public void Divine_destruction_and_angel_tier_follow_the_template_level(sbyte level, bool dtl, bool dhd, bool dts)
    {
        Item item = Make(5, Catalog(t => t.Level = level));

        Assert.Equal(dtl, item.IsDTL());
        Assert.Equal(dhd, item.IsDHD());
        Assert.Equal(dts, item.IsDTS());
    }

    [Theory]
    [InlineData(0, "Áo")]
    [InlineData(1, "Quần")]
    [InlineData(2, "Găng")]
    [InlineData(3, "Giày")]
    [InlineData(4, "Rada")]
    [InlineData(5, "")]
    [InlineData(-1, "")]
    public void Type_name_is_vietnamese_for_the_five_body_parts(sbyte type, string expected)
        => Assert.Equal(expected, Make(5, Catalog(t => t.Type = type)).TypeName());

    [Theory]
    [InlineData(0, "Trái Đất")]
    [InlineData(1, "Namếc")]
    [InlineData(2, "Xay da")]
    [InlineData(3, "Xay da")]
    public void Gender_name_maps_0_1_and_anything_else(sbyte gender, string expected)
        => Assert.Equal(expected, Make(5, Catalog(t => t.Gender = gender)).GetGenderName());

    [Theory]
    [InlineData(1066, 0)]
    [InlineData(1067, 1)]
    [InlineData(1070, 2)]
    [InlineData(1068, 3)]
    [InlineData(1069, 4)]
    [InlineData(1071, -1)]
    [InlineData(1, -1)]
    public void Piece_type_follows_the_odd_java_order(short id, sbyte expected) => Assert.Equal(expected, Make(id).TypeManh());

    [Theory]
    [InlineData(1044, true)]
    [InlineData(1211, true)]
    [InlineData(1212, true)]
    [InlineData(1213, false)]
    public void Skill_book_ids(short id, bool expected) => Assert.Equal(expected, Make(id).IsSachTuyetKy());

    [Theory]
    [InlineData(1277, false)]
    [InlineData(1278, true)]
    [InlineData(1280, true)]
    [InlineData(1281, false)]
    public void Second_skill_book_ids(short id, bool expected) => Assert.Equal(expected, Make(id).IsSachTuyetKy2());

    [Theory]
    [InlineData(0, 223, true)]
    [InlineData(1, 222, true)]
    [InlineData(2, 224, true)]
    [InlineData(3, 221, true)]
    [InlineData(4, 220, true)]
    [InlineData(0, 222, false)]
    [InlineData(1, 223, false)]
    [InlineData(4, 224, false)]
    [InlineData(5, 220, false)]
    public void Upgrade_stone_matches_its_body_part(sbyte type, short stone, bool expected)
    {
        ItemCatalog catalog = Catalog(t => t.Type = t.Id == 5 ? type : (sbyte)99);

        Assert.Equal(expected, Make(5, catalog).CanNangCapWithNDC(Make(stone, catalog)));
    }

    [Theory]
    [InlineData(0, 100, false)]
    [InlineData(30, 100, true)]
    [InlineData(1, 14, true)]
    [InlineData(1, 20, true)]
    [InlineData(1, 13, false)]
    [InlineData(1, 21, false)]
    public void Socket_stones_are_type_30_or_template_14_to_20(sbyte type, short id, bool expected)
        => Assert.Equal(expected, Make(id, Catalog(t => t.Type = type)).IsDaPhaLeEpSao());

    [Theory]
    [InlineData(410, false)]
    [InlineData(411, true)]
    [InlineData(447, true)]
    [InlineData(448, false)]
    public void First_class_socket_stones_are_411_to_447(short id, bool expected) => Assert.Equal(expected, Make(id).IsDaPhaLeC1());

    [Theory]
    [InlineData(1415, false)]
    [InlineData(1416, true)]
    [InlineData(1422, true)]
    [InlineData(1423, false)]
    [InlineData(964, true)]
    [InlineData(965, true)]
    [InlineData(966, false)]
    public void Second_class_socket_stones(short id, bool expected) => Assert.Equal(expected, Make(id).IsDaPhaLeC2());

    [Theory]
    [InlineData(1425, false)]
    [InlineData(1426, true)]
    [InlineData(1434, true)]
    [InlineData(1435, false)]
    [InlineData(1416, true)]
    [InlineData(964, true)]
    public void New_socket_stones_add_1426_to_1434(short id, bool expected) => Assert.Equal(expected, Make(id).IsDaPhaLeMoi());

    [Theory]
    [InlineData(440, false)]
    [InlineData(441, true)]
    [InlineData(447, true)]
    [InlineData(448, false)]
    public void Old_socket_stones_are_441_to_447(short id, bool expected) => Assert.Equal(expected, Make(id).IsDaPhaLeCu());

    [Fact]
    public void Socket_predicates_on_an_empty_slot_are_false_except_the_java_precedence_cases_that_throw()
    {
        var empty = new Item(Catalog());

        Assert.False(empty.IsDaPhaLeEpSao());
        Assert.False(empty.IsDaPhaLeC1());
        Assert.False(empty.IsDaPhaLeCu());
        Assert.Throws<InvalidOperationException>(() => empty.IsDaPhaLeC2());
        Assert.Throws<InvalidOperationException>(() => empty.IsDaPhaLeMoi());
        Assert.Throws<InvalidOperationException>(() => empty.IsTypeBody());
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(6, false)]
    [InlineData(-1, false)]
    [InlineData(11, true)]
    [InlineData(23, true)]
    [InlineData(32, true)]
    [InlineData(35, true)]
    [InlineData(21, false)]
    public void Body_types_are_0_to_5_and_11_23_32_35(sbyte type, bool expected)
        => Assert.Equal(expected, Make(5, Catalog(t => t.Type = type)).IsTypeBody());

    [Theory]
    [InlineData(0, true)]
    [InlineData(4, true)]
    [InlineData(5, false)]
    [InlineData(32, true)]
    [InlineData(31, false)]
    public void Only_gear_types_can_be_socketed(sbyte type, bool expected)
        => Assert.Equal(expected, Make(5, Catalog(t => t.Type = type)).CanPhaLeHoa());

    [Theory]
    [InlineData(21, true)]
    [InlineData(11, true)]
    [InlineData(23, true)]
    [InlineData(22, false)]
    public void Cosmetic_types(sbyte type, bool expected) => Assert.Equal(expected, Make(5, Catalog(t => t.Type = type)).IsTrangBiPSH());

    [Theory]
    [InlineData(27, true)]
    [InlineData(26, false)]
    public void Trade_locked_type(sbyte type, bool expected) => Assert.Equal(expected, Make(5, Catalog(t => t.Type = type)).IsTrangBiKhoaGd());

    [Theory]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(-3, true)]
    [InlineData(6, false)]
    [InlineData(32, true)]
    [InlineData(21, true)]
    [InlineData(23, true)]
    [InlineData(11, true)]
    [InlineData(72, true)]
    [InlineData(73, false)]
    public void Hac_hoa_gear_types(sbyte type, bool expected) => Assert.Equal(expected, Make(5, Catalog(t => t.Type = type)).IsTrangBiHacHoa());

    [Theory]
    [InlineData(5, true)]
    [InlineData(11, true)]
    [InlineData(21, true)]
    [InlineData(23, true)]
    [InlineData(6, false)]
    public void Collection_types(sbyte type, bool expected) => Assert.Equal(expected, Make(5, Catalog(t => t.Type = type)).IsItemCollection());

    [Theory]
    [InlineData(547, true)]
    [InlineData(548, true)]
    [InlineData(932, true)]
    [InlineData(1302, true)]
    [InlineData(1371, true)]
    [InlineData(1380, true)]
    [InlineData(1381, true)]
    [InlineData(549, false)]
    [InlineData(1382, false)]
    public void Pet_templates(short id, bool expected) => Assert.Equal(expected, Make(id).IsItemTemplateToPet());

    [Theory]
    [InlineData(543, false)]
    [InlineData(544, true)]
    [InlineData(546, true)]
    [InlineData(547, false)]
    public void Player_templates(short id, bool expected) => Assert.Equal(expected, Make(id).IsItemTemplateToPlayer());

    [Theory]
    [InlineData(14, 1, true)]
    [InlineData(15, 1, true)]
    [InlineData(6, 1, true)]
    [InlineData(1, 1, false)]
    [InlineData(1, 14, true)]
    [InlineData(1, 20, true)]
    [InlineData(1, 21, false)]
    public void Consignable_by_type_or_by_stone_template(sbyte type, short id, bool expected)
        => Assert.Equal(expected, Make(id, Catalog(t => t.Type = type)).IsDoKyGui());

    [Theory]
    [InlineData(86, true)]
    [InlineData(87, true)]
    [InlineData(88, false)]
    public void Consignable_when_it_has_option_86_or_87(int optionId, bool expected)
        => Assert.Equal(expected, Make(100, null, (optionId, 1)).IsDoKyGui());

    [Fact]
    public void Item_type_14_is_an_upgrade_stone()
    {
        Assert.True(Make(5, Catalog(t => t.Type = 14)).IsDaNangCap());
        Assert.False(Make(5, Catalog(t => t.Type = 15)).IsDaNangCap());
    }

    // ----- options ------------------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(50, 36, true)]
    [InlineData(50, 35, false)]
    [InlineData(77, 100, true)]
    [InlineData(103, 36, true)]
    [InlineData(5, 36, true)]
    [InlineData(6, 100, false)]
    [InlineData(50, -100, false)]
    public void An_item_is_buggy_when_a_cheat_prone_option_exceeds_35(int optionId, int param, bool expected)
        => Assert.Equal(expected, Make(5, null, (optionId, param)).IsBUg());

    [Fact]
    public void Buggy_check_looks_at_every_option()
    {
        Assert.True(Make(5, null, (47, 1), (50, 10), (77, 36)).IsBUg());
        Assert.False(Make(5, null).IsBUg());
    }

    [Theory]
    [InlineData(126, false)]
    [InlineData(127, true)]
    [InlineData(135, true)]
    [InlineData(136, false)]
    public void Set_bonus_options_are_127_to_135(int optionId, bool expected)
    {
        Item item = Make(5, null, (optionId, 1));

        Assert.Equal(expected, item.IsSKH());
        Assert.Equal(expected, item.HaveSetKichHoat());
    }

    [Fact]
    public void Have_set_on_an_empty_slot_is_false()
    {
        var empty = new Item(Catalog());
        empty.ItemOptions.Add(ItemTestKit.Opt(Catalog(), 127, 1));

        Assert.False(empty.HaveSetKichHoat());
        Assert.False(empty.HaveOption(127));
    }

    [Fact]
    public void Have_option_and_option_param_find_the_first_match()
    {
        Item item = Make(5, null, (47, 10), (47, 20), (21, 5));

        Assert.True(item.IsHaveOption(21));
        Assert.True(item.HaveOption(47));
        Assert.False(item.IsHaveOption(0));
        Assert.Equal(10, item.GetOptionParam(47));
        Assert.Equal(0, item.GetOptionParam(0));
    }

    [Fact]
    public void Add_option_param_sums_into_the_first_match_or_appends()
    {
        Item item = Make(5, null, (47, 10));

        item.AddOptionParam(47, 5);
        item.AddOptionParam(21, 80);

        Assert.Equal("47:15,21:80", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void Add_option_param_wraps_in_32_bits_like_java()
    {
        Item item = Make(5, null, (47, int.MaxValue));

        item.AddOptionParam(47, 1);

        Assert.Equal(int.MinValue, item.GetOptionParam(47));
    }

    [Fact]
    public void Add_option_param_with_an_unknown_option_id_throws_and_leaves_the_item_unchanged()
    {
        Item item = Make(5, null, (47, 10));

        Assert.Throws<ArgumentOutOfRangeException>(() => item.AddOptionParam(ItemTestKit.OptionCount, 1));

        Assert.Equal("47:10", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void An_item_without_a_catalog_cannot_add_options_by_id()
    {
        var item = new Item();

        Assert.Throws<InvalidOperationException>(() => item.AddOptionParam(47, 1));
        Assert.Throws<InvalidOperationException>(() => item.SetOptionParam(47, 1));
    }

    [Fact]
    public void Set_option_param_replaces_or_appends()
    {
        Item item = Make(5, null, (47, 10));

        item.SetOptionParam(47, 99);
        item.SetOptionParam(30, 1);

        Assert.Equal("47:99,30:1", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void The_misnamed_setter_changes_only_an_existing_option()
    {
        Item item = Make(5, null, (47, 10));

        item.GetOptionParam(47, 3);
        item.GetOptionParam(30, 1);

        Assert.Equal("47:3", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void Sub_option_param_only_touches_an_existing_option()
    {
        Item item = Make(5, null, (47, 10));

        item.SubOptionParam(47, 4);
        item.SubOptionParam(30, 4);

        Assert.Equal("47:6", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void Sub_and_remove_if_zero_tests_the_argument_not_the_result_as_java_does()
    {
        Item item = Make(5, null, (47, 10), (21, 5));

        item.SubOptionParamAndRemoveIfZero(47, 10);

        Assert.Equal("47:0,21:5", ItemTestKit.Dump(item.ItemOptions));

        item.SubOptionParamAndRemoveIfZero(47, 0);

        Assert.Equal("21:5", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void Sub_and_remove_removes_when_the_amount_is_negative()
    {
        Item item = Make(5, null, (47, 10));

        item.SubOptionParamAndRemoveIfZero(47, -3);

        Assert.Empty(item.ItemOptions);
    }

    [Fact]
    public void Remove_option_removes_only_the_first_match()
    {
        Item item = Make(5, null, (47, 1), (21, 2), (47, 3));

        item.RemoveOption(47);
        item.RemoveOption(99);

        Assert.Equal("21:2,47:3", ItemTestKit.Dump(item.ItemOptions));
    }

    [Theory]
    [InlineData(20, 77, 5)]
    [InlineData(19, 103, 5)]
    [InlineData(18, 80, 5)]
    [InlineData(17, 81, 5)]
    [InlineData(16, 50, 3)]
    [InlineData(15, 94, 2)]
    [InlineData(14, 108, 2)]
    [InlineData(441, 95, 5)]
    [InlineData(447, 101, 5)]
    [InlineData(1416, 95, 5)]
    [InlineData(1422, 101, 5)]
    [InlineData(1426, 95, 5)]
    [InlineData(1432, 101, 5)]
    [InlineData(1433, 153, 5)]
    [InlineData(1434, 160, 5)]
    public void A_socket_stone_gives_its_option(short stone, int optionId, int param)
    {
        ItemOption io = Make(stone, null, (1, 1)).GetOptionDaPhaLe();

        Assert.Equal(optionId, io.OptionTemplate.Id);
        Assert.Equal(param, io.Param);
    }

    [Fact]
    public void An_unknown_stone_returns_its_first_own_option_as_the_same_object_and_throws_without_options()
    {
        Item odd = Make(300, null, (47, 8), (21, 9));

        Assert.Same(odd.ItemOptions[0], odd.GetOptionDaPhaLe());
        Assert.Throws<ArgumentOutOfRangeException>(() => Make(300).GetOptionDaPhaLe());
    }

    [Fact]
    public void Socket_preview_adds_the_stone_to_the_first_matching_option_and_hides_internal_options()
    {
        Item gear = Make(5, null, (47, 100), (72, 2), (73, 1), (102, 1), (107, 1), (21, 5));
        Item stone = Make(16, null);

        // stone 16 = option 50 with 3: no matching option, so its line is appended
        string preview = gear.GetOptionInfo(stone);

        Assert.Equal("Opt47 100\nOpt21 5\nOpt50 3", preview);
        Assert.Equal(100, gear.GetOptionParam(47));
    }

    [Fact]
    public void Socket_preview_merges_into_an_existing_option_once()
    {
        Item gear = Make(5, null, (50, 7), (50, 8));
        Item stone = Make(16, null);

        Assert.Equal("Opt50 10\nOpt50 8", gear.GetOptionInfo(stone));
        Assert.Equal(7, gear.GetOptionParam(50));
    }

    [Fact]
    public void Strengthen_preview_hides_218_and_appends_the_stone_line()
    {
        Item gear = Make(5, null, (47, 100), (218, 4), (72, 1));

        Assert.Equal("Opt47 100\nOpt50 3", gear.GetOptionInfoCuongHoa(Make(16, null)));
    }

    [Fact]
    public void Option_info_hides_the_internal_options()
    {
        Item gear = Make(5, null, (47, 100), (72, 1), (73, 1), (102, 1), (107, 1), (218, 1), (21, 5));

        Assert.Equal("Opt47 100\nOpt21 5", gear.GetOptionInfo());
        Assert.Equal(string.Empty, Make(5).GetOptionInfo());
    }

    [Fact]
    public void Upgrade_info_lists_upgradable_options_option_21_and_a_parameter_of_exactly_30()
    {
        Item gear = Make(5, null, (47, 100), (21, 5), (50, 30), (218, 30), (30, 1));

        Assert.Equal("Opt47 100\nOpt21 5\nOpt50 30", gear.GetOptionInfoUpgrade());
    }

    [Fact]
    public void Upgrade_final_preview_applies_one_step_to_a_copy_only()
    {
        Item gear = Make(5, null, (47, 100), (50, 30), (21, 5));

        // 100 + max(100 / 10, 1) = 110
        Assert.Equal("Opt47 110\nOpt50 30", gear.GetOptionInfoUpgradeFinal());
        Assert.Equal(100, gear.GetOptionParam(47));
    }

    // ----- expiry options and clones -------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(-1, false)]
    public void Time_limited_gear_has_option_93_with_a_parameter_of_zero_or_more(int param, bool expected)
        => Assert.Equal(expected, Make(5, null, (93, param)).IsTrangBiHSD());

    [Fact]
    public void Time_limited_check_is_false_without_option_93()
    {
        Assert.False(Make(5, null, (260, 5)).IsTrangBiHSD());
    }

    [Fact]
    public void Clone_copies_every_option_so_the_clone_shares_nothing_mutable()
    {
        Item original = Make(5, null, (47, 10), (21, 5));
        original.Info = "info";
        original.Content = "content";
        original.Quantity = 7;
        original.QuantityGD = 3;

        Item clone = original.CloneItem();
        clone.ItemOptions[0].Param = 999;
        clone.ItemOptions.RemoveAt(1);
        clone.Quantity = 1;

        Assert.Equal("47:10,21:5", ItemTestKit.Dump(original.ItemOptions));
        Assert.Equal(7, original.Quantity);
        Assert.NotSame(original.ItemOptions[0], clone.ItemOptions[0]);
    }

    [Fact]
    public void Clone_copies_texts_time_and_template_but_not_the_trade_quantity()
    {
        Item original = Make(5, null, (47, 10));
        original.Info = "info";
        original.Content = "content";
        original.QuantityGD = 3;

        Item clone = original.CloneItem();

        Assert.Same(original.Template, clone.Template);
        Assert.Equal("info", clone.Info);
        Assert.Equal("content", clone.Content);
        Assert.Equal(1234, clone.CreateTime);
        Assert.Equal(0, clone.QuantityGD);
        Assert.Same(original.Catalog, clone.Catalog);
    }

    [Fact]
    public void Clone_of_an_empty_slot_is_an_empty_slot()
    {
        Assert.False(new Item(Catalog()).CloneItem().IsNotNullItem());
    }

    // ----- percent of shop value ----------------------------------------------------------------------------------------------------------------

    private static (ItemCatalog Catalog, Item Gear) Shirt(int shopValue, int shirtValue, int stars)
    {
        ItemCatalog bare = Catalog(t => t.Type = t.Id == 5 ? (sbyte)0 : (sbyte)99);
        var shop = new[] { new ShopItemEntry(bare.GetTemplate(5), [ItemTestKit.Opt(bare, 47, shopValue)]) };
        var catalog = new ItemCatalog(bare.ItemTemplates, bare.ItemOptionTemplates, shop);
        return (catalog, ItemTestKit.MakeItem(catalog, 5, 0, (47, shirtValue), (72, stars)));
    }

    [Theory]
    [InlineData(100, 100, 0, 100)]
    [InlineData(100, 50, 0, 50)]
    [InlineData(100, 200, 0, 200)]
    [InlineData(200, 111, 1, 50)]
    [InlineData(1000, 1, 0, 0)]
    public void Percent_option_is_the_unscaled_value_over_the_shop_value(int shopValue, int shirtValue, int stars, int expected)
    {
        (_, Item gear) = Shirt(shopValue, shirtValue, stars);

        // star scaling is undone first: reversePoint(111, 1) = 111 - 111 / 11 = 101; (101 * 100) / 200 = 50
        Assert.Equal(expected, gear.GetPercentOption());
    }

    [Fact]
    public void Percent_option_of_an_item_that_is_not_gear_is_zero_even_with_no_shop()
    {
        Assert.Equal(0, Make(5).GetPercentOption());
    }

    [Fact]
    public void Percent_option_with_a_zero_shop_value_throws_divide_by_zero_like_java()
    {
        (_, Item gear) = Shirt(0, 10, 0);

        Assert.Throws<DivideByZeroException>(() => gear.GetPercentOption());
    }

    [Fact]
    public void Percent_option_without_a_shop_entry_divides_by_minus_one()
    {
        ItemCatalog catalog = Catalog(t => t.Type = 0);
        Item gear = ItemTestKit.MakeItem(catalog, 5, 0, (47, 10));

        Assert.Equal(-1000, gear.GetPercentOption());
    }

    [Fact]
    public void Percent_option_for_each_body_part_reads_its_own_option()
    {
        (sbyte Type, int Option)[] parts = [(0, 47), (1, 6), (2, 0), (3, 7), (4, 14)];
        foreach ((sbyte type, int optionId) in parts)
        {
            ItemCatalog bare = Catalog(t => t.Type = t.Id == 5 ? type : (sbyte)99);
            var shop = new[] { new ShopItemEntry(bare.GetTemplate(5), [ItemTestKit.Opt(bare, optionId, 50)]) };
            var catalog = new ItemCatalog(bare.ItemTemplates, bare.ItemOptionTemplates, shop);
            Item gear = ItemTestKit.MakeItem(catalog, 5, 0, (optionId, 25));

            Assert.Equal(50, gear.GetPercentOption());
        }
    }

    [Fact]
    public void Transfer_preview_scales_upgradable_options_by_the_other_items_percent()
    {
        ItemCatalog bare = Catalog(t => t.Type = t.Id == 5 || t.Id == 6 ? (sbyte)0 : (sbyte)99);
        var shop = new[] { new ShopItemEntry(bare.GetTemplate(6), [ItemTestKit.Opt(bare, 47, 100)]) };
        var catalog = new ItemCatalog(bare.ItemTemplates, bare.ItemOptionTemplates, shop);
        Item target = ItemTestKit.MakeItem(catalog, 5, 0, (47, 200), (21, 5));
        Item source = ItemTestKit.MakeItem(catalog, 6, 0, (47, 50), (30, 1));

        // percent = 50; 200 * 50 / 100 = 100; pointUp(100, 2) = 110 then 121
        string preview = target.GetOptionInfoChuyenHoa(source, 2);

        Assert.Equal("Opt47 121\nOpt21 5\nOpt30 1", preview);
        Assert.Equal(200, target.GetOptionParam(47));
    }
}
