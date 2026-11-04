using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Templates;
using Nro.Game;
using Nro.Tests.Unit.Helpers;
using Nro.Tests.Unit.Game.Items.Core;

namespace Nro.Tests.Unit.Game.Items.Rewards;

public class RewardServiceTests
{
    private sealed class Rig
    {
        public Rig(ItemCatalog? catalog = null, params object[] script)
        {
            Catalog = catalog ?? ItemTestKit.Catalog();
            Random = new ScriptedRandomProvider().Enqueue(script);
            Time = ItemTestKit.Time(ManualTimeProvider.Vn(2025, 3, 10, 8, 0, 0).ToUnixTimeMilliseconds());
            Host = new RecordingHost();
            ItemService items = ItemTestKit.Service(Catalog, Random, Time, Host);
            Rewards = new RewardService(items, new RandomUtil(Random), ItemTestKit.Clock(Time), Host);
        }

        public ItemCatalog Catalog { get; }

        public ScriptedRandomProvider Random { get; }

        public ManualTimeProvider Time { get; }

        public RecordingHost Host { get; }

        public RewardService Rewards { get; }
    }

    // ----- base options of gear ---------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void A_template_with_a_value_below_ten_has_no_variation_but_still_draws_one_number()
    {
        var rig = new Rig(null, 0);
        var list = new List<ItemOption>();

        rig.Rewards.InitBaseOptionClothes(0, 0, list);

        Assert.Equal("47:2", ItemTestKit.Dump(list));
        Assert.Equal(new object?[] { 1 }, rig.Random.Calls.Select(c => c.Bound).ToArray());
    }

    [Fact]
    public void The_divine_shirt_has_armor_and_a_requirement_each_with_its_own_ten_percent_variation()
    {
        // 800: nextInt(-80, 80) = nextInt(161) draw 160 -> +80 ; 15: nextInt(-1, 1) = nextInt(3) draw 0 -> -1
        var rig = new Rig(null, 160, 0);
        var list = new List<ItemOption>();

        rig.Rewards.InitBaseOptionClothes(555, 0, list);

        Assert.Equal("47:880,21:14", ItemTestKit.Dump(list));
        Assert.Equal(new object?[] { 161, 3 }, rig.Random.Calls.Select(c => c.Bound).ToArray());
    }

    [Fact]
    public void Trousers_have_hp_and_regeneration()
    {
        // 150 -> nextInt(31) draw 0 -> 135 ; 12 -> nextInt(3) draw 2 -> 13
        var rig = new Rig(null, 0, 2);
        var list = new List<ItemOption>();

        rig.Rewards.InitBaseOptionClothes(35, 1, list);

        Assert.Equal("6:135,27:13", ItemTestKit.Dump(list));
    }

    [Fact]
    public void Divine_trousers_use_option_22_with_a_second_value_of_10000()
    {
        // 52 -> variation 5 -> nextInt(11) draw 10 -> 57 ; 10000 -> nextInt(2001) draw 1000 -> 10000 ; 15 -> nextInt(3) draw 1 -> 15
        var rig = new Rig(null, 10, 1000, 1);
        var list = new List<ItemOption>();

        rig.Rewards.InitBaseOptionClothes(556, 1, list);

        Assert.Equal("22:57,27:10000,21:15", ItemTestKit.Dump(list));
    }

    [Fact]
    public void Trousers_with_only_hp_leave_the_regeneration_slot_out()
    {
        var rig = new Rig(null, 3);
        var list = new List<ItemOption>();

        rig.Rewards.InitBaseOptionClothes(6, 1, list);

        Assert.Equal("6:30", ItemTestKit.Dump(list));
    }

    [Theory]
    [InlineData(9999, 0)]
    [InlineData(9999, 1)]
    [InlineData(9999, 2)]
    [InlineData(9999, 3)]
    [InlineData(9999, 4)]
    [InlineData(0, 5)]
    [InlineData(0, -1)]
    [InlineData(0, 99)]
    public void An_unknown_template_or_body_type_adds_nothing_and_draws_nothing(int tempId, int type)
    {
        var rig = new Rig();
        var list = new List<ItemOption>();

        rig.Rewards.InitBaseOptionClothes(tempId, type, list);

        Assert.Empty(list);
        Assert.Empty(rig.Random.Calls);
    }

    [Fact]
    public void Base_options_append_to_the_existing_list()
    {
        var rig = new Rig(null, 0);
        var list = new List<ItemOption> { ItemTestKit.Opt(rig.Catalog, 30, 1) };

        rig.Rewards.InitBaseOptionClothes(0, 0, list);

        Assert.Equal("30:1,47:2", ItemTestKit.Dump(list));
    }

    [Fact]
    public void Init_stat_of_an_item_reads_the_template_type_and_id()
    {
        ItemCatalog catalog = ItemTestKit.Catalog(t =>
        {
            if (t.Id == 0)
            {
                t.Type = 0;
            }
        });
        var rig = new Rig(catalog, 0);
        Item item = ItemTestKit.MakeItem(catalog, 0);

        rig.Rewards.InitChiSoItem(item);

        Assert.Equal("47:2", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void Every_template_and_body_type_stays_inside_the_ten_percent_band()
    {
        var provider = new SeededRandomProvider(20251103);
        ItemCatalog catalog = ItemTestKit.Catalog();
        var items = new ItemService(catalog, new RandomUtil(provider), ItemTestKit.Clock(ItemTestKit.Time()));
        var rewards = new RewardService(items, new RandomUtil(provider), ItemTestKit.Clock(ItemTestKit.Time()));
        int tested = 0;

        for (int type = 0; type <= 4; type++)
        {
            for (int id = 0; id < ItemTestKit.TemplateCount; id++)
            {
                int[][] slots = BaseClothesOptions.GetOptionParams(id, type);
                var list = new List<ItemOption>();

                rewards.InitBaseOptionClothes(id, type, list);

                int[][] filled = slots.Where(s => s[0] != -1 && s[1] != -1).ToArray();
                Assert.Equal(filled.Length, list.Count);
                for (int i = 0; i < filled.Length; i++)
                {
                    int value = filled[i][1];
                    int spread = value * 10 / 100;
                    Assert.Equal(filled[i][0], list[i].OptionTemplate.Id);
                    Assert.InRange(list[i].Param, value - spread, value + spread);
                }

                tested += filled.Length;
            }
        }

        Assert.True(tested > 150, "the table must produce options: " + tested);
    }

    [Fact]
    public void The_table_has_the_right_number_of_slots_and_a_fresh_array_each_time()
    {
        int[][] a = BaseClothesOptions.GetOptionParams(555, 0);
        int[][] b = BaseClothesOptions.GetOptionParams(555, 0);

        a[0][1] = 1;

        Assert.Equal(BaseClothesOptions.SlotCount, a.Length);
        Assert.Equal(800, b[0][1]);
        Assert.Equal(new[] { 21, 15 }, b[2]);
    }

    // ----- star options -----------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void A_star_is_added_when_the_chance_roll_succeeds()
    {
        // one entry: nextInt(0,0) draw 0 ; isTrue(1, 2): nextInt(2) draw 0 < 1
        var rig = new Rig(null, 0, 0);
        var options = new List<ItemOption>();

        rig.Rewards.InitStarOption(options, [new RewardService.RatioStar(3, 1, 2)]);

        Assert.Equal("107:3", ItemTestKit.Dump(options));
    }

    [Fact]
    public void A_star_is_not_added_when_the_chance_roll_fails()
    {
        var rig = new Rig(null, 0, 1);
        var options = new List<ItemOption>();

        rig.Rewards.InitStarOption(options, [new RewardService.RatioStar(3, 1, 2)]);

        Assert.Empty(options);
    }

    [Fact]
    public void The_entry_is_chosen_first_and_a_certain_chance_draws_nothing_more()
    {
        var rig = new Rig(null, 1);
        Item item = ItemTestKit.MakeItem(rig.Catalog, 5);

        rig.Rewards.InitStarOption(item, [new RewardService.RatioStar(1, 0, 100), new RewardService.RatioStar(7, 100, 100)]);

        Assert.Equal("107:7", ItemTestKit.Dump(item.ItemOptions));
        Assert.Single(rig.Random.Calls);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-3, 100)]
    [InlineData(5, 0)]
    [InlineData(5, -1)]
    public void A_non_positive_chance_never_adds_a_star_and_draws_nothing_for_it(int ratio, int typeRatio)
    {
        var rig = new Rig(null, 0);
        var options = new List<ItemOption>();

        rig.Rewards.InitStarOption(options, [new RewardService.RatioStar(2, ratio, typeRatio)]);

        Assert.Empty(options);
        Assert.Single(rig.Random.Calls);
    }

    [Fact]
    public void An_empty_chance_table_throws()
    {
        var rig = new Rig(null, 0);

        Assert.ThrowsAny<Exception>(() => rig.Rewards.InitStarOption(new List<ItemOption>(), []));
    }

    [Fact]
    public void The_chance_record_keeps_its_three_values()
    {
        var ratio = new RewardService.RatioStar(4, 25, 1000);

        Assert.Equal(4, ratio.NumStar);
        Assert.Equal(25, ratio.Ratio);
        Assert.Equal(1000, ratio.TypeRatio);
    }

    // ----- socket stones -------------------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(441, 95, 5)]
    [InlineData(442, 96, 5)]
    [InlineData(443, 97, 5)]
    [InlineData(444, 98, 3)]
    [InlineData(445, 99, 3)]
    [InlineData(446, 100, 5)]
    [InlineData(447, 101, 5)]
    [InlineData(220, 71, 0)]
    [InlineData(221, 70, 0)]
    [InlineData(222, 69, 0)]
    [InlineData(223, 68, 0)]
    [InlineData(224, 67, 0)]
    [InlineData(225, 73, 0)]
    public void A_socket_stone_gets_its_option(short id, int optionId, int param)
    {
        var rig = new Rig();
        Item stone = ItemTestKit.MakeItem(rig.Catalog, id);

        rig.Rewards.InitBaseOptionSaoPhaLe(stone);

        Assert.Equal(optionId + ":" + param, ItemTestKit.Dump(stone.ItemOptions));
    }

    [Theory]
    [InlineData(440)]
    [InlineData(448)]
    [InlineData(1)]
    [InlineData(226)]
    public void An_unknown_stone_throws_where_java_asked_the_table_for_position_minus_one(short id)
    {
        var rig = new Rig();
        Item stone = ItemTestKit.MakeItem(rig.Catalog, id, 0, (47, 1));

        Assert.Throws<ArgumentOutOfRangeException>(() => rig.Rewards.InitBaseOptionSaoPhaLe(stone));

        Assert.Equal("47:1", ItemTestKit.Dump(stone.ItemOptions));
    }

    // ----- activation options -------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(0, 0, "129:1,141:1,30:7")]
    [InlineData(0, 1, "127:1,140:1,30:7")]
    [InlineData(0, 2, "128:1,139:1,30:7")]
    [InlineData(1, 0, "131:1,143:1,30:7")]
    [InlineData(1, 1, "132:1,144:1,30:7")]
    [InlineData(1, 2, "130:1,142:1,30:7")]
    [InlineData(2, 0, "135:1,138:1,30:7")]
    [InlineData(2, 1, "133:1,136:1,30:7")]
    [InlineData(2, 2, "134:1,137:1,30:7")]
    public void Activation_options_by_race_and_pick(int gender, int draw, string expected)
    {
        var rig = new Rig(null, draw);
        var list = new List<ItemOption>();

        rig.Rewards.InitActivationOption(gender, 3, list);

        Assert.Equal(expected, ItemTestKit.Dump(list));
        Assert.Equal(3, (int)rig.Random.Calls.Single().Bound!);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(99)]
    public void Activation_options_are_only_for_body_types_up_to_4(int type)
    {
        var rig = new Rig();
        var list = new List<ItemOption>();

        rig.Rewards.InitActivationOption(0, type, list);

        Assert.Empty(list);
        Assert.Empty(rig.Random.Calls);
    }

    [Fact]
    public void An_unknown_race_throws_before_any_number_is_drawn()
    {
        var rig = new Rig(null, 0);

        Assert.Throws<IndexOutOfRangeException>(() => rig.Rewards.InitActivationOption(3, 0, new List<ItemOption>()));
        Assert.Equal(1, rig.Random.Remaining);
    }

    // ----- reward packs --------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void The_rewards_reject_null_arguments()
    {
        var rig = new Rig();

        Assert.Throws<ArgumentNullException>(() => rig.Rewards.RewardBeNa(null!));
        Assert.Throws<ArgumentNullException>(() => rig.Rewards.RewardTuanLoc(null!));
        Assert.Throws<ArgumentNullException>(() => rig.Rewards.RewardFirstTimeLoginPerDay(null!));
        Assert.Throws<ArgumentNullException>(() => rig.Rewards.InitChiSoItem(null!));
        Assert.Throws<ArgumentNullException>(() => rig.Rewards.InitBaseOptionClothes(0, 0, null!));
    }

    [Fact]
    public void Constructor_rejects_null_dependencies()
    {
        ItemCatalog catalog = ItemTestKit.Catalog();
        var random = new RandomUtil(new ScriptedRandomProvider());
        var clock = ItemTestKit.Clock(ItemTestKit.Time());
        var items = new ItemService(catalog, random, clock);

        Assert.Throws<ArgumentNullException>(() => new RewardService(null!, random, clock));
        Assert.Throws<ArgumentNullException>(() => new RewardService(items, null!, clock));
        Assert.Throws<ArgumentNullException>(() => new RewardService(items, random, null!));
    }

    [Fact]
    public void Be_na_gives_one_to_five_of_item_758_and_marks_the_player()
    {
        var rig = new Rig(null, 4);
        var player = new Player { CanReward_PeNa = true };

        rig.Rewards.RewardBeNa(player);

        Item given = rig.Host.BagAdds.Single();
        Assert.Equal(758, given.Template!.Id);
        Assert.Equal(5, given.Quantity);
        Assert.Equal("86:0,93:30", ItemTestKit.Dump(given.ItemOptions));
        Assert.False(player.CanReward_PeNa);
        Assert.True(player.HaveReward_PeNa);
        Assert.Equal("Bạn vừa nhận được 5 Item758", rig.Host.Notifications.Single());
        Assert.Equal(new[] { "add", "bag", "notify" }, rig.Host.Calls);
        Assert.Equal(5, (int)rig.Random.Calls.Single().Bound!);
    }

    [Fact]
    public void Pi_long_gives_one_to_five_of_item_1187()
    {
        var rig = new Rig(null, 0);
        var player = new Player { CanReward_PiLong = true };

        rig.Rewards.RewardPiLong(player);

        Assert.Equal(1187, rig.Host.BagAdds.Single().Template!.Id);
        Assert.Equal(1, rig.Host.BagAdds.Single().Quantity);
        Assert.False(player.CanReward_PiLong);
        Assert.True(player.HaveReward_PiLong);
    }

    [Fact]
    public void Meo_den_gives_one_to_three_of_item_1187()
    {
        var rig = new Rig(null, 2);
        var player = new Player { CanReward_MeoDen = true };

        rig.Rewards.RewardMeoDen(player);

        Assert.Equal(3, rig.Host.BagAdds.Single().Quantity);
        Assert.Equal(1187, rig.Host.BagAdds.Single().Template!.Id);
        Assert.Equal(3, (int)rig.Random.Calls.Single().Bound!);
        Assert.False(player.CanReward_MeoDen);
        Assert.True(player.HaveReward_MeoDen);
    }

    [Fact]
    public void Lan_con_gives_one_to_three_of_item_758()
    {
        var rig = new Rig(null, 1);
        var player = new Player { CanReward = true };

        rig.Rewards.RewardLancon(player);

        Assert.Equal(758, rig.Host.BagAdds.Single().Template!.Id);
        Assert.Equal(2, rig.Host.BagAdds.Single().Quantity);
        Assert.False(player.CanReward);
        Assert.True(player.HaveReward);
    }

    [Fact]
    public void Packs_with_no_free_slot_leave_the_player_and_the_random_source_untouched()
    {
        var rig = new Rig();
        rig.Host.EmptyBagSlots = 0;
        var player = new Player { CanReward_PeNa = true, CanReward_PiLong = true, CanReward_MeoDen = true, CanReward = true, CanReward_TuanLoc = true };

        rig.Rewards.RewardBeNa(player);
        rig.Rewards.RewardPiLong(player);
        rig.Rewards.RewardMeoDen(player);
        rig.Rewards.RewardLancon(player);
        rig.Rewards.RewardTuanLoc(player);

        Assert.Equal(5, rig.Host.Notifications.Count);
        Assert.All(rig.Host.Notifications, n => Assert.Equal("Cần 1 ô hành trang trống", n));
        Assert.True(player.CanReward_PeNa && player.CanReward_PiLong && player.CanReward_MeoDen && player.CanReward && player.CanReward_TuanLoc);
        Assert.False(player.HaveReward_PeNa || player.HaveReward_PiLong || player.HaveReward_MeoDen || player.HaveReward || player.HaveReward_TuanLoc);
        Assert.Empty(rig.Host.BagAdds);
        Assert.Empty(rig.Random.Calls);
    }

    // ----- Tuan Loc -------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Tuan_loc_first_item_gets_three_random_stats_the_hidden_option_and_a_seven_to_thirty_day_expiry()
    {
        // pick index 0 (1654); stats nextInt(15,17) draws 0,1,2 -> 15,16,17; isTrue(99,100) draw 0 -> yes ; expiry index nextInt(0,2) draw 1 -> 15
        var rig = new Rig(null, 0, 0, 1, 2, 0, 1);
        var player = new Player { CanReward_TuanLoc = true };

        rig.Rewards.RewardTuanLoc(player);

        Item given = rig.Host.BagAdds.Single();
        Assert.Equal(1654, given.Template!.Id);
        Assert.Equal("50:15,77:16,103:17,106:0,93:15", ItemTestKit.Dump(given.ItemOptions));
        Assert.False(player.CanReward_TuanLoc);
        Assert.True(player.HaveReward_TuanLoc);
        Assert.Equal("Bạn vừa nhận được Item1654", rig.Host.Notifications.Single());
    }

    [Fact]
    public void Tuan_loc_expiry_is_skipped_on_the_one_percent_miss()
    {
        // index 1 (1748): 103 nextInt(15,25) draw 0 -> 15 ; 81 nextInt(5,15) draw 10 -> 15 ; 236 nextInt(20,25) draw 5 -> 25 ; isTrue(99,100) draw 99 -> no
        var rig = new Rig(null, 1, 0, 10, 5, 99);
        var player = new Player();

        rig.Rewards.RewardTuanLoc(player);

        Assert.Equal("103:15,81:15,236:25", ItemTestKit.Dump(rig.Host.BagAdds.Single().ItemOptions));
    }

    [Fact]
    public void Tuan_loc_third_and_fourth_items()
    {
        var rig = new Rig(null, 2, 0, 0, 0, 0, 0);

        rig.Rewards.RewardTuanLoc(new Player());

        // 1829: 77 nextInt(15,20) draw 0 -> 15 ; 80 nextInt(10,25) draw 0 -> 10 ; 236 nextInt(20,25) draw 0 -> 20 ; isTrue draw 0 yes ; expiry index 0 -> 7
        Assert.Equal("77:15,80:10,236:20,93:7", ItemTestKit.Dump(rig.Host.BagAdds.Single().ItemOptions));

        var rig2 = new Rig(null, 3, 0, 0, 0, 2);
        rig2.Rewards.RewardTuanLoc(new Player());

        // 1841: 77 fixed 23 ; 80 nextInt(10,20) draw 0 -> 10 ; 94 nextInt(10,20) draw 0 -> 10 ; isTrue draw 0 yes ; expiry index 2 -> 30
        Assert.Equal("77:23,80:10,94:10,93:30", ItemTestKit.Dump(rig2.Host.BagAdds.Single().ItemOptions));
    }

    [Fact]
    public void Tuan_loc_fifth_item_has_four_stats()
    {
        var rig = new Rig(null, 4, 0, 1, 2, 3, 0, 0);

        rig.Rewards.RewardTuanLoc(new Player());

        // 1467: 77 nextInt(10,20) draw 0 -> 10 ; 50 draw 1 -> 11 ; 103 draw 2 -> 12 ; 101 nextInt(20,50) draw 3 -> 23 ; isTrue draw 0 ; expiry 0 -> 7
        Assert.Equal("77:10,50:11,103:12,101:23,93:7", ItemTestKit.Dump(rig.Host.BagAdds.Single().ItemOptions));
    }

    [Theory]
    [InlineData(5, 822)]
    [InlineData(6, 823)]
    public void Tuan_loc_pair_822_823_share_a_recipe(int index, int templateId)
    {
        // 50, 77, 103 nextInt(15,20) draws 5 -> 20 ; 106 ; isTrue draw 0 yes ; expiry 0 -> 7
        var rig = new Rig(null, index, 5, 5, 5, 0, 0);

        rig.Rewards.RewardTuanLoc(new Player());

        Item given = rig.Host.BagAdds.Single();
        Assert.Equal(templateId, given.Template!.Id);
        Assert.Equal("50:20,77:20,103:20,106:0,93:7", ItemTestKit.Dump(given.ItemOptions));
    }

    [Theory]
    [InlineData(7, 388)]
    [InlineData(8, 391)]
    [InlineData(9, 394)]
    public void Tuan_loc_balls_have_fixed_stats_and_draw_nothing_after_the_pick(int index, int templateId)
    {
        var rig = new Rig(null, index);

        rig.Rewards.RewardTuanLoc(new Player());

        Item given = rig.Host.BagAdds.Single();
        Assert.Equal(templateId, given.Template!.Id);
        Assert.Equal("77:30,106:0,80:50,50:20", ItemTestKit.Dump(given.ItemOptions));
        Assert.Single(rig.Random.Calls);
    }

    [Theory]
    [InlineData(10, 925)]
    [InlineData(11, 926)]
    [InlineData(12, 927)]
    public void Tuan_loc_last_three_items_are_locked_with_a_35_day_expiry(int index, int templateId)
    {
        var rig = new Rig(null, index);

        rig.Rewards.RewardTuanLoc(new Player());

        Item given = rig.Host.BagAdds.Single();
        Assert.Equal(templateId, given.Template!.Id);
        Assert.Equal("87:0,30:0,93:35", ItemTestKit.Dump(given.ItemOptions));
    }

    // ----- first login of the day ------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void The_first_login_of_a_new_day_gives_the_gift_and_stamps_the_time()
    {
        var rig = new Rig();
        var player = new Player { FirstTimeLogin = ManualTimeProvider.Vn(2025, 3, 9, 23, 0, 0).UtcDateTime };

        rig.Rewards.RewardFirstTimeLoginPerDay(player);

        Item given = rig.Host.BagAdds.Single();
        Assert.Equal(649, given.Template!.Id);
        Assert.Equal(1, given.Quantity);
        Assert.Equal("74:0,30:0", ItemTestKit.Dump(given.ItemOptions));
        Assert.Equal("Quà đăng nhập hàng ngày: \nBạn nhận được Item649 số lượng : 1", rig.Host.AdminNotifications.Single());
        Assert.Equal(ManualTimeProvider.Vn(2025, 3, 10, 8, 0, 0).UtcDateTime, player.FirstTimeLogin);
        Assert.Equal(new[] { "add", "bag", "notifyAdmin" }, rig.Host.Calls);
    }

    [Fact]
    public void A_second_login_on_the_same_day_gives_nothing()
    {
        var rig = new Rig();
        DateTime early = ManualTimeProvider.Vn(2025, 3, 10, 0, 30, 0).UtcDateTime;
        var player = new Player { FirstTimeLogin = early };

        rig.Rewards.RewardFirstTimeLoginPerDay(player);

        Assert.Empty(rig.Host.Calls);
        Assert.Equal(early, player.FirstTimeLogin);
    }

    [Fact]
    public void The_gift_ignores_a_full_bag_check_because_java_never_made_one()
    {
        var rig = new Rig();
        rig.Host.EmptyBagSlots = 0;
        var player = new Player { FirstTimeLogin = ManualTimeProvider.Vn(2025, 3, 1).UtcDateTime };

        rig.Rewards.RewardFirstTimeLoginPerDay(player);

        Assert.Single(rig.Host.BagAdds);
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Unspecified)]
    public void The_stored_login_time_is_read_as_utc_unless_it_says_otherwise(DateTimeKind kind)
    {
        var rig = new Rig();

        // 2025-03-09 16:30 UTC is 23:30 of the 9th in Vietnam: a new day compared with the 10th
        var player = new Player { FirstTimeLogin = DateTime.SpecifyKind(new DateTime(2025, 3, 9, 16, 30, 0), kind) };

        rig.Rewards.RewardFirstTimeLoginPerDay(player);

        Assert.Single(rig.Host.BagAdds);
    }

    [Fact]
    public void A_missing_login_time_throws_where_java_threw_null_pointer()
    {
        var rig = new Rig();

        Assert.Throws<InvalidOperationException>(() => rig.Rewards.RewardFirstTimeLoginPerDay(new Player { FirstTimeLogin = null }));
        Assert.Empty(rig.Host.Calls);
    }
}
