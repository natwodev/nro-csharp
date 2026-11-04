using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Templates;
using Nro.Game;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Items.Core;

public class ItemServiceTests
{
    private sealed class Rig
    {
        public Rig(ItemCatalog? catalog = null, params object[] script)
        {
            Catalog = catalog ?? ItemTestKit.Catalog();
            Random = new ScriptedRandomProvider().Enqueue(script);
            Time = ItemTestKit.Time();
            Host = new RecordingHost();
            Service = ItemTestKit.Service(Catalog, Random, Time, Host);
        }

        public ItemCatalog Catalog { get; }

        public ScriptedRandomProvider Random { get; }

        public ManualTimeProvider Time { get; }

        public RecordingHost Host { get; }

        public ItemService Service { get; }
    }

    private static ItemCatalog GearCatalog(IEnumerable<ShopItemEntry>? shop = null) => ItemTestKit.Catalog(
        t =>
        {
            t.Type = t.Id switch
            {
                555 or 557 or 559 or 650 or 652 or 654 or 1048 or 1049 or 1050 => 0,
                556 or 558 or 560 or 651 or 653 or 655 or 1051 or 1052 or 1053 => 1,
                562 or 564 or 566 or 657 or 659 or 661 or 1054 or 1055 or 1056 => 2,
                563 or 565 or 567 or 658 or 660 or 662 or 1057 or 1058 or 1059 => 3,
                561 or 656 or 1060 or 1061 or 1062 => 4,
                _ => (sbyte)99,
            };
        },
        shop);

    private static ItemCatalog WithShop(ItemCatalog bare, params ShopItemEntry[] shop) => new(bare.ItemTemplates, bare.ItemOptionTemplates, shop);

    // ----- construction ---------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Constructor_rejects_null_dependencies()
    {
        ItemCatalog catalog = ItemTestKit.Catalog();
        var random = new RandomUtil(new ScriptedRandomProvider());
        var clock = ItemTestKit.Clock(ItemTestKit.Time());

        Assert.Throws<ArgumentNullException>(() => new ItemService(null!, random, clock));
        Assert.Throws<ArgumentNullException>(() => new ItemService(catalog, null!, clock));
        Assert.Throws<ArgumentNullException>(() => new ItemService(catalog, random, null!));
    }

    [Fact]
    public void Create_option_resolves_the_template_by_position_and_fails_outside_the_table()
    {
        var rig = new Rig();

        ItemOption io = rig.Service.CreateOption(47, 9);

        Assert.Equal(47, io.OptionTemplate.Id);
        Assert.Equal(9, io.Param);
        Assert.Throws<ArgumentOutOfRangeException>(() => rig.Service.CreateOption(-1, 5));
        Assert.Throws<ArgumentOutOfRangeException>(() => rig.Service.CreateOption(ItemTestKit.OptionCount, 5));
    }

    [Fact]
    public void Lookups_delegate_to_the_catalog()
    {
        var rig = new Rig();

        Assert.Same(rig.Catalog.GetTemplate(12), rig.Service.GetTemplate(12));
        Assert.Same(rig.Catalog.GetItemOptionTemplate(12), rig.Service.GetItemOptionTemplate(12));
        Assert.Equal(1, rig.Service.GetItemIdByIcon(1001));
        Assert.Same(rig.Catalog, rig.Service.Catalog);
    }

    // ----- new / copy ----------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Create_new_item_stamps_the_clock_and_builds_the_texts()
    {
        var rig = new Rig();

        Item item = rig.Service.CreateNewItem(20, 3);

        Assert.Equal(20, item.Template!.Id);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(ItemTestKit.T0, item.CreateTime);
        Assert.Equal("Yêu cầu sức mạnh 200 trở lên", item.Content);
        Assert.Equal(string.Empty, item.Info);
        Assert.Empty(item.ItemOptions);
        Assert.Equal(1, rig.Service.CreateNewItem(20).Quantity);
    }

    [Fact]
    public void Create_new_item_with_an_unknown_template_throws()
    {
        var rig = new Rig();

        Assert.Throws<ArgumentOutOfRangeException>(() => rig.Service.CreateNewItem(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => rig.Service.CreateNewItem(ItemTestKit.TemplateCount));
    }

    [Fact]
    public void Create_new_item_follows_the_fake_clock()
    {
        var rig = new Rig();
        rig.Time.Advance(TimeSpan.FromMinutes(5));

        Assert.Equal(ItemTestKit.T0 + 300_000, rig.Service.CreateNewItem(1).CreateTime);
    }

    [Fact]
    public void Create_locked_item_adds_option_30_after_the_info_text_is_built()
    {
        var rig = new Rig();

        Item item = rig.Service.CreateNewItemLock(7, 2);

        Assert.Equal("30:1", ItemTestKit.Dump(item.ItemOptions));
        Assert.Equal(string.Empty, item.Info);
        Assert.Equal(2, item.Quantity);
        Assert.Equal("30:1", ItemTestKit.Dump(rig.Service.CreateNewItemLock(7).ItemOptions));
    }

    [Fact]
    public void Copy_item_shares_no_option_object_and_keeps_the_creation_time()
    {
        var rig = new Rig();
        Item original = ItemTestKit.MakeItem(rig.Catalog, 5, 777, (47, 10), (21, 5));
        original.Quantity = 4;
        original.QuantityGD = 2;
        original.Info = "i";
        original.Content = "c";

        Item copy = rig.Service.CopyItem(original);
        copy.ItemOptions[0].Param = 1;
        copy.ItemOptions.RemoveAt(1);

        Assert.Equal("47:10,21:5", ItemTestKit.Dump(original.ItemOptions));
        Assert.Equal(777, copy.CreateTime);
        Assert.Equal(4, copy.Quantity);
        Assert.Equal(0, copy.QuantityGD);
        Assert.Equal("i", copy.Info);
        Assert.Equal("c", copy.Content);
        Assert.NotSame(original.ItemOptions, copy.ItemOptions);
    }

    [Fact]
    public void Copy_of_a_copy_still_resolves_option_ids_through_the_catalog()
    {
        var rig = new Rig();
        Item copy = rig.Service.CopyItem(ItemTestKit.MakeItem(rig.Catalog, 5, 0, (47, 10)));

        copy.AddOptionParam(21, 5);

        Assert.Equal("47:10,21:5", ItemTestKit.Dump(copy.ItemOptions));
    }

    [Fact]
    public void Copy_item_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => new Rig().Service.CopyItem(null!));
    }

    [Fact]
    public void Item_from_item_map_takes_over_the_option_list()
    {
        var rig = new Rig();
        var options = new List<ItemOption> { ItemTestKit.Opt(rig.Catalog, 47, 5) };

        Item item = rig.Service.CreateItemFromItemMap(9, 2, options);

        Assert.Same(options, item.ItemOptions);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(9, item.Template!.Id);
    }

    [Fact]
    public void Item_without_options_variants_start_empty()
    {
        var rig = new Rig();

        Assert.Empty(rig.Service.CreateActivationSetItem(5, 2).ItemOptions);
        Assert.Empty(rig.Service.CreateItemDoHuyDiet(5, 2).ItemOptions);
        Assert.False(rig.Service.CreateItemNull().IsNotNullItem());
        Assert.Equal(2, rig.Service.CreateActivationSetItem(5, 2).Quantity);
    }

    // ----- shop item --------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Item_from_the_shop_copies_the_options_and_draws_one_expiry_number_per_option()
    {
        ItemCatalog bare = ItemTestKit.Catalog();
        ShopItemEntry entry = new(bare.GetTemplate(5), [ItemTestKit.Opt(bare, 47, 100), ItemTestKit.Opt(bare, 21, 5)]);
        var rig = new Rig(WithShop(bare, entry), 0, 0);

        Item item = rig.Service.CreateItemFromItemShop(entry);
        item.ItemOptions[0].Param = 1;

        Assert.Equal("47:1,21:5", ItemTestKit.Dump(item.ItemOptions));
        Assert.Equal(100, entry.Options[0].Param);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(string.Empty, item.Info);
        Assert.Equal("Yêu cầu sức mạnh 50 trở lên", item.Content);
        Assert.Equal(new object?[] { 6, 6 }, rig.Random.Calls.Select(c => c.Bound).ToArray());
        Assert.Equal(0, rig.Random.Remaining);
    }

    [Fact]
    public void Shop_option_231_becomes_the_expiry_option_93_with_a_random_day_count()
    {
        ItemCatalog bare = ItemTestKit.Catalog();
        ShopItemEntry entry = new(bare.GetTemplate(5), [ItemTestKit.Opt(bare, 231, 1)]);

        // Draws: day index 2 -> 5 days; nextInt(86,90) -> 86; nextInt(96,100) -> 100; isTrue(86,100) draw 10 -> true
        var rig = new Rig(WithShop(bare, entry), 2, 0, 4, 10);

        Item item = rig.Service.CreateItemFromItemShop(entry.Template, entry.Options);

        Assert.Equal("93:5", ItemTestKit.Dump(item.ItemOptions));
        Assert.Equal(new object?[] { 6, 5, 5, 100 }, rig.Random.Calls.Select(c => c.Bound).ToArray());
    }

    [Fact]
    public void Shop_option_231_is_just_dropped_when_the_chance_fails()
    {
        ItemCatalog bare = ItemTestKit.Catalog();
        var rig = new Rig(bare, 5, 0, 0, 95);

        Item item = rig.Service.CreateItemFromItemShop(bare.GetTemplate(5), [ItemTestKit.Opt(bare, 231, 1)]);

        Assert.Empty(item.ItemOptions);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 3)]
    [InlineData(2, 5)]
    [InlineData(3, 7)]
    [InlineData(4, 14)]
    [InlineData(5, 30)]
    public void Expiry_day_count_comes_from_the_six_value_table(int draw, int days)
    {
        ItemCatalog bare = ItemTestKit.Catalog();
        var rig = new Rig(bare, draw, 0, 0, 0);

        Item item = rig.Service.CreateItemFromItemShop(bare.GetTemplate(5), [ItemTestKit.Opt(bare, 231, 1)]);

        Assert.Equal("93:" + days, ItemTestKit.Dump(item.ItemOptions));
    }

    [Theory]
    [InlineData(76)]
    [InlineData(164)]
    [InlineData(249)]
    public void Shop_options_76_164_249_are_removed(int optionId)
    {
        ItemCatalog bare = ItemTestKit.Catalog();
        var rig = new Rig(bare, 0, 0);

        Item item = rig.Service.CreateItemFromItemShop(bare.GetTemplate(5), [ItemTestKit.Opt(bare, optionId, 1), ItemTestKit.Opt(bare, 47, 2)]);

        Assert.Equal("47:2", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void Shop_option_210_expands_into_that_many_random_bonus_options()
    {
        ItemCatalog bare = ItemTestKit.Catalog();

        // shuffle draws nextInt(i + 1) for i = 16 .. 1; answering i keeps the order. Then 3 parameter draws (range 5..10) and one expiry-day draw
        var script = new List<object>();
        for (int i = 16; i >= 1; i--)
        {
            script.Add(i);
        }

        script.AddRange([0, 3, 5, 0]);
        var rig = new Rig(bare, script.ToArray());

        Item item = rig.Service.CreateItemFromItemShop(bare.GetTemplate(5), [ItemTestKit.Opt(bare, 210, 3)]);

        Assert.Equal("50:5,77:8,103:10", ItemTestKit.Dump(item.ItemOptions));
        Assert.Equal(0, rig.Random.Remaining);
    }

    [Fact]
    public void Shop_option_210_uses_the_parameter_range_of_each_kind()
    {
        ItemCatalog bare = ItemTestKit.Catalog();
        var script = new List<object>();
        for (int i = 16; i >= 1; i--)
        {
            script.Add(i);
        }

        // order after the identity shuffle: 50 77 103 14 5 94 97 108 95 96 101 204 80 81 45 46 197
        // take 5 (index 4, range 10..19), 14 (index 3, range 7..17) with count 5: ids 50 77 103 14 5
        script.AddRange([0, 0, 0, 0, 0, 0]);
        var rig = new Rig(bare, script.ToArray());

        Item item = rig.Service.CreateItemFromItemShop(bare.GetTemplate(5), [ItemTestKit.Opt(bare, 210, 5)]);

        Assert.Equal("50:5,77:5,103:5,14:7,5:10", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void Shop_option_210_with_more_than_17_picks_repeats_random_ids()
    {
        ItemCatalog bare = ItemTestKit.Catalog();
        var script = new List<object>();
        for (int i = 16; i >= 1; i--)
        {
            script.Add(i);
        }

        // 17 parameters for the 17 ids (all take 0 -> minimum of their range), then the 18th: random id index 0 (50) and its parameter
        for (int i = 0; i < 17; i++)
        {
            script.Add(0);
        }

        script.AddRange([0, 0, 0]);
        var rig = new Rig(bare, script.ToArray());

        Item item = rig.Service.CreateItemFromItemShop(bare.GetTemplate(5), [ItemTestKit.Opt(bare, 210, 18)]);

        Assert.Equal(18, item.ItemOptions.Count);
        Assert.Equal("50:5", ItemTestKit.Dump(item.ItemOptions.TakeLast(1)));
    }

    [Fact]
    public void Shop_overload_with_null_arguments_throws()
    {
        var rig = new Rig();

        Assert.Throws<ArgumentNullException>(() => rig.Service.CreateItemFromItemShop((ShopItemEntry)null!));
        Assert.Throws<ArgumentNullException>(() => rig.Service.CreateItemFromItemShop(null!, []));
        Assert.Throws<ArgumentNullException>(() => rig.Service.CreateItemFromItemShop(rig.Catalog.GetTemplate(1), null!));
    }

    // ----- gear with fixed ranges (otpts / otptl / otphd) -----------------------------------------------------------------------------------------

    [Theory]
    [InlineData(555, 500, "21:80,47:2500")]
    [InlineData(556, 0, "21:80,22:150")]
    [InlineData(562, 2000, "21:80,0:20000")]
    [InlineData(563, 49, "21:80,23:199")]
    [InlineData(561, 5, "21:80,14:25")]
    public void Otpts_rolls_the_stat_of_the_body_part(short id, int draw, string expected)
    {
        var rig = new Rig(GearCatalog(), draw);

        Item item = rig.Service.Otpts(id);

        Assert.Equal(expected, ItemTestKit.Dump(item.ItemOptions));
        Assert.Equal(id, item.Template!.Id);
        Assert.Equal(item.GetInfo(), item.Info);
    }

    [Theory]
    [InlineData(555, 400, "21:19,47:1200")]
    [InlineData(556, 4, "21:19,22:28")]
    [InlineData(562, 2300, "21:19,0:7800")]
    [InlineData(563, 6, "21:19,23:29")]
    [InlineData(561, 13, "21:19,14:14")]
    public void Otptl_rolls_the_divine_stat(short id, int draw, string expected)
    {
        var rig = new Rig(GearCatalog(), draw);

        Assert.Equal(expected, ItemTestKit.Dump(rig.Service.Otptl(id).ItemOptions));
    }

    [Theory]
    [InlineData(650, 900, "21:80,47:2100")]
    [InlineData(651, 20, "21:80,22:80")]
    [InlineData(657, 2500, "21:80,0:11000")]
    [InlineData(658, 23, "21:80,23:82")]
    [InlineData(656, 13, "21:80,14:18")]
    public void Otphd_rolls_the_destruction_stat(short id, int draw, string expected)
    {
        var rig = new Rig(GearCatalog(), draw);

        Assert.Equal(expected, ItemTestKit.Dump(rig.Service.Otphd(id).ItemOptions));
    }

    [Fact]
    public void Gear_roll_of_a_template_of_another_type_has_no_options_and_draws_nothing()
    {
        var rig = new Rig();

        Assert.Empty(rig.Service.Otpts(5).ItemOptions);
        Assert.Empty(rig.Service.Otptl(5, 3).ItemOptions);
        Assert.Empty(rig.Service.Otphd(5, 3).ItemOptions);
        Assert.Empty(rig.Random.Calls);
    }

    [Fact]
    public void Gear_roll_sets_quantity_texts_and_time()
    {
        var rig = new Rig(GearCatalog(), 0);

        Item item = rig.Service.Otpts(555, 4);

        Assert.Equal(4, item.Quantity);
        Assert.Equal(ItemTestKit.T0, item.CreateTime);
        Assert.Equal("Opt21 80Opt47 2000", item.Info);
        Assert.Equal("Yêu cầu sức mạnh 5550 trở lên", item.Content);
    }

    // ----- divine drops ---------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Boss_divine_drop_ring_branch()
    {
        // isTrue(10,100) draw 5 -> ring; tiLe nextInt(100,115) draw 10 -> 110; isTrue(30,100) draw 50 -> no lock; strength nextInt(15,17) draw 2 -> 17
        var rig = new Rig(GearCatalog(), 5, 10, 50, 2);

        RolledDrop drop = rig.Service.RandDoTLBoss();

        Assert.Equal(561, drop.TemplateId);
        Assert.Equal("14:15,21:17", ItemTestKit.Dump(drop.Options));
        Assert.Equal(new object?[] { 100, 16, 100, 3 }, rig.Random.Calls.Select(c => c.Bound).ToArray());
    }

    [Fact]
    public void Boss_divine_drop_gloves_branch_with_rare_bonus_and_lock_option()
    {
        // 10: draw 50 false; 25: draw 20 true; gang[nextInt(3)] draw 2 -> 566; tiLe draw 15 -> 115; lock isTrue draw 10 true; options[nextInt(2)] draw 1 -> 87; strength draw 0 -> 15
        var rig = new Rig(GearCatalog(), 50, 20, 2, 15, 10, 1, 0);

        RolledDrop drop = rig.Service.RandDoTLBoss();

        Assert.Equal(566, drop.TemplateId);
        Assert.Equal("0:5175,207:15,87:0,21:15", ItemTestKit.Dump(drop.Options));
    }

    [Fact]
    public void Boss_divine_drop_boots_branch_has_no_rare_bonus_at_100_percent()
    {
        var rig = new Rig(GearCatalog(), 99, 99, 99, 99, 2, 0, 99, 1);

        RolledDrop drop = rig.Service.RandDoTLBoss();

        Assert.Equal(567, drop.TemplateId);
        Assert.Equal("23:46,28:6900,21:16", ItemTestKit.Dump(drop.Options));
    }

    [Fact]
    public void Boss_divine_drop_trousers_branch()
    {
        var rig = new Rig(GearCatalog(), 99, 99, 10, 0, 5, 50, 0);

        RolledDrop drop = rig.Service.RandDoTLBoss();

        Assert.Equal(556, drop.TemplateId);
        Assert.Equal("22:54,27:2730,207:5,21:15", ItemTestKit.Dump(drop.Options));
    }

    [Fact]
    public void Boss_divine_drop_shirt_branch()
    {
        var rig = new Rig(GearCatalog(), 99, 99, 99, 10, 0, 0, 99, 0);

        RolledDrop drop = rig.Service.RandDoTLBoss();

        Assert.Equal(555, drop.TemplateId);
        Assert.Equal("47:800,21:15", ItemTestKit.Dump(drop.Options));
    }

    [Theory]
    [InlineData(1, 557, 850)]
    [InlineData(2, 559, 900)]
    public void Boss_divine_shirts_of_the_other_races_use_their_own_base(int pick, int templateId, int baseValue)
    {
        var rig = new Rig(GearCatalog(), 99, 99, 99, 10, pick, 0, 99, 0);

        RolledDrop drop = rig.Service.RandDoTLBoss();

        Assert.Equal(templateId, drop.TemplateId);
        Assert.Equal("47:" + baseValue + ",21:15", ItemTestKit.Dump(drop.Options));
    }

    [Fact]
    public void Divine_drop_ring_branch()
    {
        // 5 -> ring (type 4); tiLe draw 5 -> 105; nextInt(14,17) draw 3 -> 17; 17 * 105 / 100 = 17; lock isTrue(90,100) draw 95 -> no; strength nextInt(15,20) draw 5 -> 20
        var rig = new Rig(GearCatalog(), 5, 5, 3, 95, 5);

        RolledDrop drop = rig.Service.RandDoTL();

        Assert.Equal(561, drop.TemplateId);
        Assert.Equal("14:17,21:20", ItemTestKit.Dump(drop.Options));
    }

    [Fact]
    public void Divine_drop_shirt_branch_with_a_lock_option()
    {
        // 99 99 99 then 10 (shirt); ao[draw 2] = 559; tiLe draw 15 -> 115; base nextInt(800,900) draw 100 -> 900; 900 * 115 / 100 = 1035; lock draw 10 -> options[draw 3] = 36; strength draw 0 -> 15
        var rig = new Rig(GearCatalog(), 99, 99, 99, 10, 2, 15, 100, 10, 3, 0);

        RolledDrop drop = rig.Service.RandDoTL();

        Assert.Equal(559, drop.TemplateId);
        Assert.Equal("47:1035,36:0,21:15", ItemTestKit.Dump(drop.Options));
    }

    [Fact]
    public void Divine_drop_trousers_branch()
    {
        var rig = new Rig(GearCatalog(), 99, 99, 10, 1, 0, 0, 95, 0);

        RolledDrop drop = rig.Service.RandDoTL();

        Assert.Equal(558, drop.TemplateId);
        Assert.Equal("22:46,27:5750,21:15", ItemTestKit.Dump(drop.Options));
    }

    [Fact]
    public void Divine_drop_boots_branch()
    {
        var rig = new Rig(GearCatalog(), 99, 99, 99, 99, 0, 0, 3000, 95, 1);

        RolledDrop drop = rig.Service.RandDoTL();

        Assert.Equal(563, drop.TemplateId);
        Assert.Equal("23:49,28:6125,21:16", ItemTestKit.Dump(drop.Options));
    }

    [Fact]
    public void Divine_drop_gloves_branch()
    {
        var rig = new Rig(GearCatalog(), 99, 10, 1, 0, 200, 95, 0);

        RolledDrop drop = rig.Service.RandDoTL();

        Assert.Equal(564, drop.TemplateId);
        Assert.Equal("0:4500,21:15", ItemTestKit.Dump(drop.Options));
    }

    [Fact]
    public void Default_divine_options_for_a_shirt_with_a_lock_and_the_top_requirement()
    {
        // tiLe draw 15 -> 115; 800 * 115 / 100 = 920; lock isTrue(30,100) draw 10 -> nextInt(86,87) draw 1 -> 87; strength nextInt(15,40) draw 25 -> 40
        var rig = new Rig(GearCatalog(), 15, 10, 1, 25);

        Assert.Equal("47:920,87:0,21:40", ItemTestKit.Dump(rig.Service.GetDefaultOptionTL(555)));
    }

    [Theory]
    [InlineData(557, "47:850")]
    [InlineData(559, "47:900")]
    [InlineData(556, "22:52,27:2600")]
    [InlineData(558, "22:50,27:2500")]
    [InlineData(560, "22:48,27:2400")]
    [InlineData(562, "0:4400")]
    [InlineData(564, "0:4300")]
    [InlineData(566, "0:4500")]
    [InlineData(563, "23:48,28:2400")]
    [InlineData(565, "23:50,28:2500")]
    [InlineData(567, "23:46,28:6900")]
    [InlineData(561, "14:14")]
    public void Default_divine_options_at_100_percent_for_every_piece(int itemId, string stat)
    {
        // tiLe draw 0 -> 100; no lock (draw 99); strength draw 0 -> 15
        var rig = new Rig(GearCatalog(), 0, 99, 0);

        Assert.Equal(stat + ",21:15", ItemTestKit.Dump(rig.Service.GetDefaultOptionTL(itemId)));
    }

    [Fact]
    public void Default_divine_options_for_an_unknown_piece_are_only_the_requirement()
    {
        var rig = new Rig(GearCatalog(), 3, 99, 4);

        Assert.Equal("21:19", ItemTestKit.Dump(rig.Service.GetDefaultOptionTL(1)));
    }

    // ----- set bonus ---------------------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(0, 0, 99, 128)]
    [InlineData(0, 0, 10, 214)]
    [InlineData(0, 24, 10, 214)]
    [InlineData(0, 25, -1, 129)]
    [InlineData(0, 59, -1, 129)]
    [InlineData(0, 60, -1, 127)]
    [InlineData(0, 99, -1, 127)]
    [InlineData(1, 0, -1, 131)]
    [InlineData(1, 24, -1, 131)]
    [InlineData(1, 25, -1, 132)]
    [InlineData(1, 60, -1, 130)]
    [InlineData(2, 0, -1, 133)]
    [InlineData(2, 30, -1, 135)]
    [InlineData(2, 70, -1, 134)]
    [InlineData(3, 0, -1, 133)]
    [InlineData(3, 70, -1, 134)]
    public void Random_set_bonus_id_by_race_and_roll(int gender, int draw, int secondDraw, int expected)
    {
        object[] script = secondDraw >= 0 ? new object[] { draw, secondDraw } : new object[] { draw };
        var rig = new Rig(null, script);

        Assert.Equal(expected, rig.Service.RandomSKHId((sbyte)gender));
        Assert.Equal(0, rig.Random.Remaining);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(-1)]
    [InlineData(100)]
    public void Random_set_bonus_id_of_an_unknown_race_throws(int gender)
    {
        var rig = new Rig(null, 0, 0);

        Assert.Throws<IndexOutOfRangeException>(() => rig.Service.RandomSKHId((sbyte)gender));
    }

    [Theory]
    [InlineData(127, 139)]
    [InlineData(128, 140)]
    [InlineData(129, 141)]
    [InlineData(130, 143)]
    [InlineData(131, 254)]
    [InlineData(132, 144)]
    [InlineData(133, 136)]
    [InlineData(134, 137)]
    [InlineData(135, 138)]
    [InlineData(250, 253)]
    [InlineData(251, 254)]
    [InlineData(255, 256)]
    [InlineData(252, 0)]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    public void Set_bonus_option_tables(int setId, int bonusId)
    {
        var service = new Rig().Service;

        Assert.Equal(bonusId, service.OptionIdSKH(setId));
        Assert.Equal(bonusId, service.OptionIdDHD(setId));
    }

    [Fact]
    public void Set_item_gets_the_shop_options_the_set_option_its_bonus_and_the_lock()
    {
        ItemCatalog bare = ItemTestKit.Catalog();
        ShopItemEntry entry = new(bare.GetTemplate(5), [ItemTestKit.Opt(bare, 47, 10)]);
        var rig = new Rig(WithShop(bare, entry));

        Item item = rig.Service.ItemSKH(5, 127);
        item.ItemOptions[0].Param = 999;

        Assert.Equal("47:999,127:1,139:1,30:1", ItemTestKit.Dump(item.ItemOptions));
        Assert.Equal(10, entry.Options[0].Param);
        Assert.Equal(1, item.Quantity);
    }

    [Fact]
    public void Set_item_of_a_template_the_shop_does_not_sell_has_no_base_options()
    {
        var rig = new Rig();

        Assert.Equal("131:1,254:1,30:1", ItemTestKit.Dump(rig.Service.ItemSKH(5, 131).ItemOptions));
    }

    [Fact]
    public void Destruction_set_item_has_the_set_bonus_and_lock_only()
    {
        var rig = new Rig();

        Assert.Equal("133:1,136:1,30:1", ItemTestKit.Dump(rig.Service.ItemDHD(650, 133).ItemOptions));
        Assert.Equal("250:1,253:1,30:1", ItemTestKit.Dump(rig.Service.ItemDHD(650, 250).ItemOptions));
    }

    // ----- destruction gear with race bonus ---------------------------------------------------------------------------------------------------------

    [Fact]
    public void Destruction_shirt_is_raised_ten_percent_for_xayda()
    {
        var rig = new Rig(GearCatalog(), 1000);

        Item item = rig.Service.RandomCS_DHD(650, 2);

        Assert.Equal("47:3080,21:80,30:1", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void Destruction_shirt_is_not_raised_for_other_races()
    {
        var rig = new Rig(GearCatalog(), 1000);

        Assert.Equal("47:2800,21:80,30:1", ItemTestKit.Dump(rig.Service.RandomCS_DHD(654, 0).ItemOptions));
    }

    [Theory]
    [InlineData(651, 0, 15, "22:110,21:80,30:1")]
    [InlineData(651, 1, 15, "22:100,21:80,30:1")]
    [InlineData(657, 2, 149, "0:9513,21:80,30:1")]
    [InlineData(657, 0, 149, "0:8649,21:80,30:1")]
    [InlineData(658, 1, 10, "23:99,21:80,30:1")]
    [InlineData(658, 2, 10, "23:90,21:80,30:1")]
    [InlineData(656, 1, 2, "14:19,21:80,30:1")]
    [InlineData(656, 0, 0, "14:17,21:80,30:1")]
    public void Destruction_pieces_by_body_part_and_race(int itemId, int gender, int draw, string expected)
    {
        var rig = new Rig(GearCatalog(), draw);

        Assert.Equal(expected, ItemTestKit.Dump(rig.Service.RandomCS_DHD(itemId, gender).ItemOptions));
    }

    [Fact]
    public void Destruction_roll_of_an_unrelated_template_has_only_the_requirement_and_lock()
    {
        var rig = new Rig();

        Assert.Equal("21:80,30:1", ItemTestKit.Dump(rig.Service.RandomCS_DHD(700, 1).ItemOptions));
        Assert.Empty(rig.Random.Calls);
    }

    // ----- angel gear --------------------------------------------------------------------------------------------------------------------------------

    [Fact]
    public void Angel_shirt_for_a_non_matching_race()
    {
        // armor nextInt(1201) draw 1200 -> 4000, then three 80 percent draws (99 -> always the else branch, but they are drawn); then fixed options
        var rig = new Rig(GearCatalog(), 1200, 10, 10, 10);

        Item item = rig.Service.DoThienSu(1048, 0);

        Assert.Equal("47:4000,21:120,30:1", ItemTestKit.Dump(item.ItemOptions));
        Assert.Equal(0, rig.Random.Remaining);
    }

    [Fact]
    public void Angel_shirt_for_xayda_is_raised_ten_percent()
    {
        var rig = new Rig(GearCatalog(), 1200, 10, 10, 10);

        Assert.Equal("47:4400,21:120,30:1", ItemTestKit.Dump(rig.Service.DoThienSu(1050, 2).ItemOptions));
    }

    [Theory]
    [InlineData(10, 10, "22:130")]
    [InlineData(99, 20, "22:150")]
    public void Angel_trousers_normal_or_high_range(int chanceDraw, int valueDraw, string expectedStat)
    {
        // order of draws: trousers chance, (trousers value), gloves chance, boots chance
        var rig = new Rig(GearCatalog(), chanceDraw, valueDraw, 99, 99);

        Item item = rig.Service.DoThienSu(1051, 1);

        Assert.Equal(expectedStat + ",21:120,30:1", ItemTestKit.Dump(item.ItemOptions));
        Assert.Equal(0, rig.Random.Remaining);
    }

    [Fact]
    public void Angel_trousers_are_raised_for_earthling()
    {
        var rig = new Rig(GearCatalog(), 10, 10, 10, 10);

        Assert.Equal("22:143,21:120,30:1", ItemTestKit.Dump(rig.Service.DoThienSu(1052, 0).ItemOptions));
    }

    [Theory]
    [InlineData(10, 650, "0:10000")]
    [InlineData(99, 1000, "0:11000")]
    public void Angel_gloves_normal_or_high_range(int chanceDraw, int valueDraw, string expectedStat)
    {
        // trousers chance (no value), gloves chance, gloves value, boots chance
        var rig = new Rig(GearCatalog(), 10, chanceDraw, valueDraw, 10);

        Assert.Equal(expectedStat + ",21:120,30:1", ItemTestKit.Dump(rig.Service.DoThienSu(1054, 1).ItemOptions));
    }

    [Theory]
    [InlineData(10, 20, "23:110")]
    [InlineData(99, 0, "23:110")]
    [InlineData(10, 0, "23:90")]
    public void Angel_boots_normal_or_high_range(int chanceDraw, int valueDraw, string expectedStat)
    {
        var rig = new Rig(GearCatalog(), 10, 10, chanceDraw, valueDraw);

        Assert.Equal(expectedStat + ",21:120,30:1", ItemTestKit.Dump(rig.Service.DoThienSu(1057, 0).ItemOptions));
    }

    [Fact]
    public void Angel_ring_has_critical_between_18_and_20_and_is_raised_for_namek()
    {
        var rig = new Rig(GearCatalog(), 10, 10, 10, 2);

        Assert.Equal("14:22,21:120,30:1", ItemTestKit.Dump(rig.Service.DoThienSu(1060, 1).ItemOptions));
    }

    [Fact]
    public void Angel_item_has_the_requirement_and_lock_added_by_sum()
    {
        var rig = new Rig(GearCatalog(), 10, 10, 10);

        Item item = rig.Service.DoThienSu(700, 0);

        Assert.Equal("21:120,30:1", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void Angel_item_from_the_combine_window()
    {
        var rig = new Rig(GearCatalog());
        rig.Host.CombineItems.AddRange([ItemTestKit.MakeItem(rig.Catalog, 555), ItemTestKit.MakeItem(rig.Catalog, 650), ItemTestKit.MakeItem(rig.Catalog, 651)]);
        rig.Random.Enqueue(1200, 10, 10, 10);
        Player player = ItemTestKit.NewPlayer();

        rig.Service.OpenDTS(player);

        Assert.Single(rig.Host.BagAdds);
        Assert.Equal(1048, rig.Host.BagAdds[0].Template!.Id);
        Assert.Equal(3, rig.Host.BagSubs.Count);
        Assert.All(rig.Host.BagSubs, s => Assert.Equal(1, s.Quantity));
        Assert.Equal("Item555", rig.Host.BagSubs[0].Item!.Name());
        Assert.Equal(new[] { "add", "sub", "sub", "sub", "bag", "notify" }, rig.Host.Calls);
        Assert.Equal("Bạn đã nhận được Item1048", rig.Host.Notifications.Single());
    }

    [Fact]
    public void Open_angel_picks_the_piece_of_the_players_race_and_the_divine_piece_body_part()
    {
        var rig = new Rig(GearCatalog());
        rig.Host.CombineItems.AddRange([ItemTestKit.MakeItem(rig.Catalog, 562), ItemTestKit.MakeItem(rig.Catalog, 650), ItemTestKit.MakeItem(rig.Catalog, 651)]);
        rig.Random.Enqueue(10, 10, 10, 10);
        Player player = ItemTestKit.NewPlayer();
        player.Gender = 2;

        rig.Service.OpenDTS(player);

        // gender 2 row, body part 2 (gloves) -> 1056
        Assert.Equal(1056, rig.Host.BagAdds.Single().Template!.Id);
    }

    [Fact]
    public void Open_angel_reports_each_missing_ingredient()
    {
        var rig = new Rig(GearCatalog());
        Player player = ItemTestKit.NewPlayer();

        rig.Host.CombineItems.AddRange([ItemTestKit.MakeItem(rig.Catalog, 650), ItemTestKit.MakeItem(rig.Catalog, 651), ItemTestKit.MakeItem(rig.Catalog, 5)]);
        rig.Service.OpenDTS(player);
        rig.Host.CombineItems.Clear();
        rig.Host.CombineItems.AddRange([ItemTestKit.MakeItem(rig.Catalog, 555), ItemTestKit.MakeItem(rig.Catalog, 650), ItemTestKit.MakeItem(rig.Catalog, 5)]);
        rig.Service.OpenDTS(player);
        rig.Host.CombineItems.Clear();
        rig.Host.CombineItems.AddRange([ItemTestKit.MakeItem(rig.Catalog, 555), ItemTestKit.MakeItem(rig.Catalog, 650), ItemTestKit.MakeItem(rig.Catalog, 651), ItemTestKit.MakeItem(rig.Catalog, 652)]);
        rig.Service.OpenDTS(player);

        Assert.Equal(new[] { "Thiếu đồ thần linh", "Thiếu đồ hủy diệt", "Thiếu đồ" }, rig.Host.Notifications);
        Assert.Empty(rig.Host.BagAdds);
    }

    [Fact]
    public void Open_angel_needs_a_free_bag_slot_and_keeps_the_ingredients()
    {
        var rig = new Rig(GearCatalog());
        rig.Host.EmptyBagSlots = 0;
        rig.Host.CombineItems.AddRange([ItemTestKit.MakeItem(rig.Catalog, 555), ItemTestKit.MakeItem(rig.Catalog, 650), ItemTestKit.MakeItem(rig.Catalog, 651)]);

        rig.Service.OpenDTS(ItemTestKit.NewPlayer());

        Assert.Equal("Bạn phải có ít nhất 1 ô trống hành trang", rig.Host.Notifications.Single());
        Assert.Empty(rig.Host.BagAdds);
        Assert.Empty(rig.Host.BagSubs);
        Assert.Empty(rig.Random.Calls);
    }

    [Fact]
    public void Open_angel_rejects_a_null_player()
    {
        Assert.Throws<ArgumentNullException>(() => new Rig().Service.OpenDTS(null!));
    }

    // ----- shop lookups and the angel item from the shop -----------------------------------------------------------------------------------------

    [Fact]
    public void Option_list_by_level_gender_type_skips_entries_until_the_list_is_filled_and_copies()
    {
        ItemCatalog bare = ItemTestKit.Catalog(t =>
        {
            if (t.Id is 5 or 6 or 7)
            {
                t.Level = 14;
                t.Gender = 1;
                t.Type = 2;
            }
        });
        var emptyEntry = new ShopItemEntry(bare.GetTemplate(5), []);
        var fullEntry = new ShopItemEntry(bare.GetTemplate(6), [ItemTestKit.Opt(bare, 0, 55)]);
        var laterEntry = new ShopItemEntry(bare.GetTemplate(7), [ItemTestKit.Opt(bare, 0, 77)]);
        var rig = new Rig(WithShop(bare, emptyEntry, fullEntry, laterEntry));

        List<ItemOption> list = rig.Service.GetListOptionItemShop(14, 1, 2);
        list[0].Param = 1;

        Assert.Single(list);
        Assert.Equal(55, fullEntry.Options[0].Param);
        Assert.Empty(rig.Service.GetListOptionItemShop(14, 0, 2));
        Assert.Empty(rig.Service.GetListOptionItemShop(13, 1, 2));
    }

    [Fact]
    public void Option_list_by_template_id_returns_copies_of_the_first_matching_entry()
    {
        ItemCatalog bare = ItemTestKit.Catalog();
        var first = new ShopItemEntry(bare.GetTemplate(9), [ItemTestKit.Opt(bare, 47, 5), ItemTestKit.Opt(bare, 21, 3)]);
        var second = new ShopItemEntry(bare.GetTemplate(9), [ItemTestKit.Opt(bare, 6, 99)]);
        var rig = new Rig(WithShop(bare, first, second));

        List<ItemOption> list = rig.Service.GetListOptionItemShop((short)9);
        list[0].Param = 1000;

        Assert.Equal("47:1000,21:3", ItemTestKit.Dump(list));
        Assert.Equal(5, first.Options[0].Param);
        Assert.Empty(rig.Service.GetListOptionItemShop((short)10));
    }

    [Fact]
    public void Angel_item_from_the_shop_raises_upgradable_options_by_20_percent_and_sets_the_requirement()
    {
        ItemCatalog bare = ItemTestKit.Catalog(t =>
        {
            if (t.Id == 5)
            {
                t.Level = 14;
                t.Gender = 1;
                t.Type = 2;
            }
        });
        var entry = new ShopItemEntry(bare.GetTemplate(5), [ItemTestKit.Opt(bare, 0, 100), ItemTestKit.Opt(bare, 21, 80), ItemTestKit.Opt(bare, 6, 7)]);
        var rig = new Rig(WithShop(bare, entry));

        Item item = rig.Service.GetAngelItem(1, 2);

        // template 1048 + 2 * 3 + 1 = 1055; 100 * 1.2 = 120; 7 * 1.2 = 8.4 -> 8; option 21 is not upgradable
        Assert.Equal(1055, item.Template!.Id);
        Assert.Equal("0:120,6:8,21:70,30:0", ItemTestKit.Dump(item.ItemOptions));
    }

    [Theory]
    [InlineData(0, 62)]
    [InlineData(1, 66)]
    [InlineData(2, 70)]
    [InlineData(3, 64)]
    [InlineData(4, 68)]
    [InlineData(9, 68)]
    public void Angel_item_requirement_by_body_part(int type, int requirement)
    {
        var rig = new Rig();

        Item item = rig.Service.GetAngelItem(0, type);

        Assert.Equal("21:" + requirement + ",30:0", ItemTestKit.Dump(item.ItemOptions));
    }

    [Fact]
    public void Angel_ring_looks_up_the_shop_with_gender_3()
    {
        ItemCatalog bare = ItemTestKit.Catalog(t =>
        {
            if (t.Id == 5)
            {
                t.Level = 14;
                t.Gender = 3;
                t.Type = 4;
            }
        });
        var entry = new ShopItemEntry(bare.GetTemplate(5), [ItemTestKit.Opt(bare, 14, 10)]);
        var rig = new Rig(WithShop(bare, entry));

        Item item = rig.Service.GetAngelItem(2, 4);

        Assert.Equal(1048 + 12 + 2, item.Template!.Id);
        Assert.Equal("14:12,21:68,30:0", ItemTestKit.Dump(item.ItemOptions));
    }

    // ----- activation gear picks ------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(0, 0, 1, 33)]
    [InlineData(0, 0, 0, 0)]
    [InlineData(0, 0, 2, 3)]
    [InlineData(1, 1, 0, 7)]
    [InlineData(2, 3, 1, 55)]
    public void Basic_activation_template_comes_from_the_group_candidates_of_the_race(int gender, int group, int candidate, int expected)
    {
        // the draw order is the group, then the candidate among the group pairs that suit the race (race 3 pairs suit every race)
        var rig = new Rig(null, group, candidate);

        int id = rig.Service.RandomActivationSetTemplate(gender);

        Assert.Equal(expected, id);
        Assert.Equal(2, rig.Random.Calls.Count);
    }

    [Fact]
    public void Basic_activation_ring_group_is_open_to_every_race()
    {
        Assert.Equal(59, new Rig(null, 4, 3).Service.RandomActivationSetTemplate(1));
        Assert.Equal(12, new Rig(null, 4, 0).Service.RandomActivationSetTemplate(2));
    }

    [Fact]
    public void Basic_activation_with_no_candidate_for_the_race_returns_zero_after_one_draw()
    {
        var rig = new Rig(null, 0);

        Assert.Equal(0, rig.Service.RandomActivationSetTemplate(5));
        Assert.Single(rig.Random.Calls);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(0, 11, 233)]
    [InlineData(1, 0, 1)]
    public void Vip_activation_template_comes_from_the_first_group(int gender, int candidate, int expected)
    {
        var rig = new Rig(null, 0, candidate);

        Assert.Equal(expected, rig.Service.RandomActivationSetTemplate_Vip(gender));
    }

    [Fact]
    public void Vip_activation_ring_group_has_twelve_ids()
    {
        var rig = new Rig(null, 4, 11);

        Assert.Equal(281, rig.Service.RandomActivationSetTemplate_Vip(0));
    }

    [Theory]
    [InlineData(0, 0, 128, 140)]
    [InlineData(0, 4, 250, 253)]
    [InlineData(0, 7, 267, 268)]
    [InlineData(1, 0, 130, 143)]
    [InlineData(1, 4, 251, 142)]
    [InlineData(2, 0, 134, 137)]
    [InlineData(2, 4, 252, 255)]
    [InlineData(7, 3, 233, 234)]
    [InlineData(-1, 1, 135, 138)]
    public void Activation_option_pair_by_race(int gender, int draw, int first, int second)
    {
        var rig = new Rig(null, draw);

        Assert.Equal(new[] { first, second }, rig.Service.RandomActivationSetOptions(gender));
        Assert.Equal(8, (int)rig.Random.Calls.Single().Bound!);
    }

    [Theory]
    [InlineData(0, 245, 248)]
    [InlineData(1, 237, 240)]
    [InlineData(2, 241, 244)]
    [InlineData(3, 269, 272)]
    [InlineData(-5, 269, 272)]
    public void New_activation_options_are_four_consecutive_ids_and_use_no_random(int gender, int first, int last)
    {
        var rig = new Rig();

        int[] options = rig.Service.RandomActivationSetOptionsNew(gender);

        Assert.Equal(new[] { first, first + 1, first + 2, last }, options);
        Assert.Empty(rig.Random.Calls);
    }

    // ----- training armor -------------------------------------------------------------------------------------------------------------------------

    [Theory]
    [InlineData(529, 10)]
    [InlineData(534, 10)]
    [InlineData(530, 20)]
    [InlineData(535, 20)]
    [InlineData(531, 30)]
    [InlineData(536, 30)]
    [InlineData(1797, 40)]
    [InlineData(532, 0)]
    [InlineData(1, 0)]
    public void Training_armor_bonus(short id, int percent)
    {
        var service = new Rig().Service;
        Item item = ItemTestKit.MakeItem(ItemTestKit.Catalog(), id);

        Assert.Equal(percent, service.GetPercentTrainArmor(item));
        Assert.Equal(percent != 0, service.IsTrainArmor(item));
    }

    [Fact]
    public void Training_armor_of_nothing_is_zero_and_not_activation()
    {
        var service = new Rig().Service;

        Assert.Equal(0, service.GetPercentTrainArmor(null));
        Assert.False(service.IsTrainArmor(null));
        Assert.False(service.IsItemActivation(new Item()));
    }

    // ----- expiry -------------------------------------------------------------------------------------------------------------------------------------

    private static readonly long Day = 86_400_000L;
    private static readonly long Hour = 3_600_000L;
    private static readonly long Minute = 60_000L;

    private static (Rig Rig, Item Item) TimedItem(long ageMs, params (int, int)[] options)
    {
        var rig = new Rig();
        Item item = ItemTestKit.MakeItem(rig.Catalog, 5, ItemTestKit.T0 - ageMs, options);
        return (rig, item);
    }

    [Fact]
    public void Expiry_check_of_null_or_an_item_without_countdown_is_false_and_changes_nothing()
    {
        (Rig rig, Item item) = TimedItem(10 * Day, (47, 10));

        Assert.False(rig.Service.IsOutOfDateTime(null));
        Assert.False(rig.Service.IsOutOfDateTime(item));
        Assert.Equal(ItemTestKit.T0 - (10 * Day), item.CreateTime);
        Assert.Equal(10, item.GetOptionParam(47));
    }

    [Fact]
    public void Days_option_counts_down_by_whole_days_and_moves_the_start_to_now()
    {
        (Rig rig, Item item) = TimedItem((2 * Day) + (5 * Hour), (93, 5));

        Assert.False(rig.Service.IsOutOfDateTime(item));

        Assert.Equal(3, item.GetOptionParam(93));
        Assert.Equal(ItemTestKit.T0, item.CreateTime);
    }

    [Fact]
    public void Days_option_that_reaches_zero_expires_and_keeps_the_old_start()
    {
        (Rig rig, Item item) = TimedItem(3 * Day, (93, 3));

        Assert.True(rig.Service.IsOutOfDateTime(item));

        Assert.Equal(0, item.GetOptionParam(93));
        Assert.Equal(ItemTestKit.T0 - (3 * Day), item.CreateTime);
    }

    [Fact]
    public void Days_option_that_goes_negative_expires()
    {
        (Rig rig, Item item) = TimedItem(10 * Day, (93, 3));

        Assert.True(rig.Service.IsOutOfDateTime(item));
        Assert.Equal(-7, item.GetOptionParam(93));
    }

    [Fact]
    public void Less_than_a_whole_day_changes_nothing()
    {
        (Rig rig, Item item) = TimedItem(Day - 1, (93, 1));

        Assert.False(rig.Service.IsOutOfDateTime(item));
        Assert.Equal(1, item.GetOptionParam(93));
        Assert.Equal(ItemTestKit.T0 - Day + 1, item.CreateTime);
    }

    [Fact]
    public void Exactly_one_day_with_one_day_left_expires()
    {
        (Rig rig, Item item) = TimedItem(Day, (93, 1));

        Assert.True(rig.Service.IsOutOfDateTime(item));
    }

    [Theory]
    [InlineData(5, 10, 5)]
    [InlineData(23, 30, 7)]
    [InlineData(24, 30, 30)]
    [InlineData(25, 30, 29)]
    [InlineData(49, 30, 29)]
    public void Hours_option_uses_the_hour_of_day_difference_which_wraps_every_24_hours(int ageHours, int start, int expectedParam)
    {
        (Rig rig, Item item) = TimedItem(ageHours * Hour, (260, start));

        rig.Service.IsOutOfDateTime(item);

        Assert.Equal(expectedParam, item.GetOptionParam(260));
    }

    [Theory]
    [InlineData(5, 10, 5)]
    [InlineData(59, 100, 41)]
    [InlineData(60, 100, 100)]
    [InlineData(90, 100, 70)]
    public void Minutes_option_uses_the_minute_of_hour_difference_which_wraps_every_hour(int ageMinutes, int start, int expectedParam)
    {
        (Rig rig, Item item) = TimedItem(ageMinutes * Minute, (261, start));

        rig.Service.IsOutOfDateTime(item);

        Assert.Equal(expectedParam, item.GetOptionParam(261));
    }

    [Fact]
    public void Minutes_option_reaching_zero_expires()
    {
        (Rig rig, Item item) = TimedItem(5 * Minute, (261, 5));

        Assert.True(rig.Service.IsOutOfDateTime(item));
    }

    [Fact]
    public void A_start_time_in_the_future_counts_as_elapsed_time_because_java_takes_the_absolute_difference()
    {
        (Rig rig, Item item) = TimedItem(-2 * Day, (93, 5));

        Assert.False(rig.Service.IsOutOfDateTime(item));
        Assert.Equal(3, item.GetOptionParam(93));
    }

    [Fact]
    public void An_option_that_moves_the_start_changes_what_the_next_option_sees()
    {
        (Rig rig, Item item) = TimedItem((2 * Day) + (3 * Hour), (93, 5), (260, 10));

        rig.Service.IsOutOfDateTime(item);

        Assert.Equal(3, item.GetOptionParam(93));
        Assert.Equal(10, item.GetOptionParam(260));
    }

    [Fact]
    public void One_expired_option_makes_the_item_expired_even_if_a_later_one_is_fine()
    {
        (Rig rig, Item item) = TimedItem(2 * Day, (93, 1), (260, 100));

        Assert.True(rig.Service.IsOutOfDateTime(item));
    }

    [Fact]
    public void Expiry_follows_the_fake_clock()
    {
        (Rig rig, Item item) = TimedItem(0, (93, 2));

        Assert.False(rig.Service.IsOutOfDateTime(item));
        rig.Time.Advance(TimeSpan.FromDays(2));
        Assert.True(rig.Service.IsOutOfDateTime(item));
    }

    [Fact]
    public void Loading_an_item_brings_it_up_to_date_clamps_at_zero_and_resends_the_bag()
    {
        (Rig rig, Item item) = TimedItem(5 * Day, (93, 3), (21, 5));
        Player player = ItemTestKit.NewPlayer();

        rig.Service.LoadItemTimeStatus(player, item);

        Assert.Equal(0, item.GetOptionParam(93));
        Assert.Equal(ItemTestKit.T0, item.CreateTime);
        Assert.Equal(new[] { "bag" }, rig.Host.Calls);
    }

    [Fact]
    public void Loading_keeps_a_positive_remainder_and_does_not_subtract_twice()
    {
        (Rig rig, Item item) = TimedItem(2 * Day, (93, 10));

        rig.Service.LoadItemTimeStatus(ItemTestKit.NewPlayer(), item);

        Assert.Equal(8, item.GetOptionParam(93));
    }

    [Fact]
    public void Loading_an_item_with_hour_and_minute_options_handles_each()
    {
        (Rig rig, Item item) = TimedItem((3 * Hour) + (10 * Minute), (260, 5), (261, 100));

        rig.Service.LoadItemTimeStatus(ItemTestKit.NewPlayer(), item);

        Assert.Equal(2, item.GetOptionParam(260));
        Assert.Equal(100, item.GetOptionParam(261));
    }

    [Fact]
    public void Loading_null_does_nothing_at_all()
    {
        var rig = new Rig();

        rig.Service.LoadItemTimeStatus(ItemTestKit.NewPlayer(), null);

        Assert.Empty(rig.Host.Calls);
    }

    [Fact]
    public void Loading_stops_updating_the_real_item_after_the_first_expiry_because_java_swapped_its_local_variable()
    {
        // measured on the real Java code: the check after option 93 expires option 261 (-2) and swaps the local item for an empty one
        (Rig rig, Item item) = TimedItem(5 * Minute, (93, 1), (261, 3));

        rig.Service.LoadItemTimeStatus(ItemTestKit.NewPlayer(), item);

        Assert.Equal("93:1,261:-2", ItemTestKit.Dump(item.ItemOptions));
        Assert.Equal(ItemTestKit.T0 - (5 * Minute), item.CreateTime);
        Assert.Equal(new[] { "bag" }, rig.Host.Calls);
    }

    [Fact]
    public void Loading_an_expired_item_does_not_replace_it_as_java_only_changed_a_local_variable()
    {
        (Rig rig, Item item) = TimedItem(5 * Day, (93, 3));

        rig.Service.LoadItemTimeStatus(ItemTestKit.NewPlayer(), item);

        Assert.True(item.IsNotNullItem());
    }

    // ----- reward packs ---------------------------------------------------------------------------------------------------------------------------

    private static ItemCatalog PackCatalog() => GearCatalog();

    [Fact]
    public void There_are_eighteen_pack_definitions_with_unique_names_and_the_two_pack_items()
    {
        IReadOnlyList<SetRewardDefinition> all = SetRewardDefinition.All;

        Assert.Equal(18, all.Count);
        Assert.Equal(18, all.Select(d => d.JavaName).Distinct().Count());
        Assert.Equal(9, all.Count(d => d.Kind == SetRewardKind.ThanLinh && d.PackItemId == 1703));
        Assert.Equal(9, all.Count(d => d.Kind == SetRewardKind.HuyDiet && d.PackItemId == 1704));
        Assert.All(all, d => Assert.Equal(5, d.CandidateIds.Length));
        Assert.All(all, d => Assert.Equal(d.CandidateIds.Last(), d.Kind == SetRewardKind.ThanLinh ? 561 : 656));
    }

    [Fact]
    public void Pack_gives_the_rolled_piece_with_the_set_options_and_uses_up_one_pack()
    {
        var rig = new Rig(PackCatalog(), 2, 0);
        Item pack = ItemTestKit.MakeItem(rig.Catalog, 1703);
        rig.Host.Bag[1703] = pack;

        rig.Service.ClaimSetReward(ItemTestKit.NewPlayer(), "settlkaio");

        Item given = rig.Host.BagAdds.Single();
        Assert.Equal(562, given.Template!.Id);
        Assert.Equal("21:19,0:5500,127:0,139:0,30:0", ItemTestKit.Dump(given.ItemOptions));
        Assert.Equal("Bạn đã nhận được 1 món Thần Linh ", rig.Host.Notifications.Single());
        Assert.Same(pack, rig.Host.BagSubs.Single().Item);
        Assert.Equal(new[] { "add", "bag", "notify", "sub", "bag" }, rig.Host.Calls);
    }

    [Fact]
    public void Pack_with_a_full_bag_still_uses_the_random_numbers_but_gives_and_removes_nothing()
    {
        var rig = new Rig(PackCatalog(), 2, 0);
        rig.Host.EmptyBagSlots = 1;
        rig.Host.Bag[1703] = ItemTestKit.MakeItem(rig.Catalog, 1703);

        rig.Service.ClaimSetReward(ItemTestKit.NewPlayer(), "settlkaio");

        Assert.Equal(0, rig.Random.Remaining);
        Assert.Empty(rig.Host.BagAdds);
        Assert.Empty(rig.Host.BagSubs);
        Assert.Equal("Bạn phải có ít nhất 1 ô trống hành trang", rig.Host.Notifications.Single());
    }

    [Theory]
    [InlineData("settloctieu", 4, false)]
    [InlineData("settloctieu", 5, true)]
    [InlineData("sethdpiko", 4, false)]
    [InlineData("sethdpiko", 5, true)]
    [InlineData("settlkaio", 1, false)]
    [InlineData("settlkaio", 2, true)]
    [InlineData("sethdnappa", 2, true)]
    public void Pack_needs_more_empty_slots_than_its_threshold(string name, int empty, bool given)
    {
        var rig = new Rig(PackCatalog(), 0, 0);
        rig.Host.EmptyBagSlots = empty;

        rig.Service.ClaimSetReward(ItemTestKit.NewPlayer(), name);

        Assert.Equal(given, rig.Host.BagAdds.Count == 1);
    }

    [Fact]
    public void Pack_without_the_pack_item_in_the_bag_still_gives_the_piece_and_passes_null_to_the_removal()
    {
        var rig = new Rig(PackCatalog(), 0, 0);

        rig.Service.ClaimSetReward(ItemTestKit.NewPlayer(), "sethdkaio");

        Assert.Single(rig.Host.BagAdds);
        Assert.Null(rig.Host.BagSubs.Single().Item);
    }

    [Fact]
    public void Destruction_pack_gives_a_destruction_piece_with_its_text()
    {
        var rig = new Rig(PackCatalog(), 4, 0);

        rig.Service.ClaimSetReward(ItemTestKit.NewPlayer(), "sethdgenki");

        Item given = rig.Host.BagAdds.Single();
        Assert.Equal(656, given.Template!.Id);
        Assert.Equal("21:80,14:5,128:0,140:0,30:0", ItemTestKit.Dump(given.ItemOptions));
        Assert.Equal("Bạn đã nhận được món Hủy Diệt ", rig.Host.Notifications.Single());
    }

    [Fact]
    public void Pack_with_an_unknown_name_is_rejected_before_anything_happens()
    {
        var rig = new Rig();

        Assert.Throws<ArgumentException>(() => rig.Service.ClaimSetReward(ItemTestKit.NewPlayer(), "settlnothing"));
        Assert.Empty(rig.Host.Calls);
        Assert.Empty(rig.Random.Calls);
    }

    [Fact]
    public void Every_pack_row_can_be_claimed_and_matches_its_candidates()
    {
        foreach (SetRewardDefinition definition in SetRewardDefinition.All)
        {
            var rig = new Rig(PackCatalog(), 1, 0);

            rig.Service.ClaimSetReward(ItemTestKit.NewPlayer(), definition);

            Item given = rig.Host.BagAdds.Single();
            Assert.Equal(definition.CandidateIds[1], given.Template!.Id);
            Assert.Contains(definition.SetOptionId + ":0", ItemTestKit.Dump(given.ItemOptions));
            Assert.Contains(definition.BonusOptionId + ":0", ItemTestKit.Dump(given.ItemOptions));
        }
    }
}
