using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Consts;
using Nro.Core.Templates;
using Nro.Game;
using Nro.Tests.Shared.Game.Support;
using Nro.Tests.Unit.Game.Support;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Progress.Tasks;

public class TaskMainTests
{
    private static TaskMain Template()
    {
        var task = new TaskMain { Id = 3, Index = 2, Name = "Nhiệm vụ", Detail = "Chi tiết", LastTime = 99 };
        task.SubTasks.Add(new SubTaskMain { Name = "Bước 1", MaxCount = 5, Notify = "n1", NpcId = 7, MapId = 21, Count = 4 });
        task.SubTasks.Add(new SubTaskMain { Name = "Bước 2", MaxCount = -1, Notify = "n2", NpcId = 8, MapId = 22 });
        return task;
    }

    [Fact]
    public void A_new_task_has_an_empty_step_list()
    {
        var task = new TaskMain();

        Assert.Empty(task.SubTasks);
        Assert.Equal("", task.Name);
    }

    [Fact]
    public void Copy_is_deep_and_every_step_starts_at_zero_progress()
    {
        TaskMain template = Template();

        var copy = new TaskMain(template);

        Assert.Equal(3, copy.Id);
        Assert.Equal(2, copy.Index);
        Assert.Equal("Nhiệm vụ", copy.Name);
        Assert.Equal("Chi tiết", copy.Detail);
        Assert.Equal(2, copy.SubTasks.Count);
        Assert.NotSame(template.SubTasks, copy.SubTasks);
        Assert.NotSame(template.SubTasks[0], copy.SubTasks[0]);
        Assert.Equal(0, copy.SubTasks[0].Count);
        Assert.Equal(4, template.SubTasks[0].Count);
        Assert.Equal("Bước 1", copy.SubTasks[0].Name);
        Assert.Equal(5, copy.SubTasks[0].MaxCount);
        Assert.Equal("n1", copy.SubTasks[0].Notify);
        Assert.Equal(7, copy.SubTasks[0].NpcId);
        Assert.Equal(21, copy.SubTasks[0].MapId);
    }

    [Fact]
    public void Copy_does_not_carry_the_last_time_a_java_quirk()
    {
        Assert.Equal(0, new TaskMain(Template()).LastTime);
    }

    [Fact]
    public void Two_copies_of_the_same_template_share_no_state()
    {
        TaskMain template = Template();
        var playerA = new TaskMain(template);
        var playerB = new TaskMain(template);

        playerA.SubTasks[0].Count = 3;
        playerA.SubTasks[1].Name = "đổi tên";
        playerA.Name = "khác";
        playerA.SubTasks.RemoveAt(1);

        Assert.Equal(0, playerB.SubTasks[0].Count);
        Assert.Equal("Bước 2", playerB.SubTasks[1].Name);
        Assert.Equal("Nhiệm vụ", playerB.Name);
        Assert.Equal(4, template.SubTasks[0].Count);
        Assert.Equal(2, template.SubTasks.Count);
        Assert.Equal("Bước 2", template.SubTasks[1].Name);
    }

    [Fact]
    public void Step_copy_copies_the_description_only()
    {
        var step = new SubTaskMain { Name = "a", MaxCount = 2, Notify = "b", NpcId = 1, MapId = 2, Count = 9 };

        var copy = new SubTaskMain(step);

        Assert.Equal(0, copy.Count);
        Assert.Equal("a", copy.Name);
        Assert.Equal(2, copy.MaxCount);
        Assert.Equal("b", copy.Notify);
        Assert.Equal(1, copy.NpcId);
        Assert.Equal(2, copy.MapId);
    }

    [Fact]
    public void Copies_of_null_are_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => new TaskMain(null!));
        Assert.Throws<ArgumentNullException>(() => new SubTaskMain(null!));
    }
}

public class SideAndEventTaskTests
{
    private static int[][] Ranges() => [[1, 2], [3, 4], [5, 6], [7, 8], [9, 10]];

    [Fact]
    public void A_new_task_starts_with_the_full_daily_allowance()
    {
        Assert.Equal(ConstTask.MAX_SIDE_TASK, new SideTask().LeftTask);
        Assert.Equal(ConstTask.MAX_EVENT_TASK, new EventTask().LeftTask);
    }

    [Fact]
    public void Reset_clears_template_progress_level_and_notifications_but_not_the_amount_or_allowance()
    {
        var side = new SideTask
        {
            Template = new SideTaskTemplate(1, "x", Ranges()),
            Count = 5,
            MaxCount = 9,
            Level = 3,
            LeftTask = 4,
            ReceivedTime = 77,
            Notify0 = true,
            Notify10 = true,
            Notify20 = true,
            Notify30 = true,
            Notify40 = true,
            Notify50 = true,
            Notify60 = true,
            Notify70 = true,
            Notify80 = true,
            Notify90 = true,
        };

        side.Reset();

        Assert.Null(side.Template);
        Assert.Equal(0, side.Count);
        Assert.Equal(0, side.Level);
        Assert.False(side.Notify0 || side.Notify10 || side.Notify20 || side.Notify30 || side.Notify40 || side.Notify50 || side.Notify60 || side.Notify70 || side.Notify80 || side.Notify90);
        Assert.Equal(9, side.MaxCount);
        Assert.Equal(4, side.LeftTask);
        Assert.Equal(77, side.ReceivedTime);
    }

    [Fact]
    public void Event_task_reset_does_the_same()
    {
        var evt = new EventTask { Template = new EventTaskTemplate(1, "x", Ranges()), Count = 5, MaxCount = 9, Level = 3, Notify90 = true, LeftTask = 2 };

        evt.Reset();

        Assert.Null(evt.Template);
        Assert.Equal(0, evt.Count);
        Assert.Equal(0, evt.Level);
        Assert.False(evt.Notify90);
        Assert.Equal(9, evt.MaxCount);
        Assert.Equal(2, evt.LeftTask);
    }

    [Fact]
    public void Renew_gives_the_allowance_back_after_midnight_in_vietnam()
    {
        var time = new ManualTimeProvider(ManualTimeProvider.Vn(2026, 11, 3, 0, 0, 1));
        var clock = new VietnamClock(time);
        var side = new SideTask { LeftTask = 2, ReceivedTime = ManualTimeProvider.Vn(2026, 11, 2, 23, 59, 0).ToUnixTimeMilliseconds() };

        side.Renew(clock);

        Assert.Equal(ConstTask.MAX_SIDE_TASK, side.LeftTask);
        Assert.Equal(time.GetUtcNow().ToUnixTimeMilliseconds(), side.ReceivedTime);
    }

    [Fact]
    public void Renew_does_nothing_on_the_same_vietnam_day_even_if_the_utc_day_changed()
    {
        // 06:00 Vietnam on the 3rd is 23:00 UTC on the 2nd; 05:00 Vietnam on the 3rd is also the 2nd in UTC.
        var time = new ManualTimeProvider(ManualTimeProvider.Vn(2026, 11, 3, 6, 0, 0));
        long received = ManualTimeProvider.Vn(2026, 11, 3, 5, 0, 0).ToUnixTimeMilliseconds();
        var evt = new EventTask { LeftTask = 2, ReceivedTime = received };

        evt.Renew(new VietnamClock(time));

        Assert.Equal(2, evt.LeftTask);
        Assert.Equal(received, evt.ReceivedTime);
    }

    [Fact]
    public void Renew_on_a_brand_new_task_stamps_the_time_because_the_received_time_is_zero()
    {
        var time = new ManualTimeProvider(ManualTimeProvider.Vn(2026, 11, 3, 10, 0, 0));
        var side = new SideTask();

        side.Renew(new VietnamClock(time));

        Assert.Equal(ConstTask.MAX_SIDE_TASK, side.LeftTask);
        Assert.Equal(time.GetUtcNow().ToUnixTimeMilliseconds(), side.ReceivedTime);
    }

    [Fact]
    public void GetName_replaces_the_amount_or_says_there_is_no_task()
    {
        Assert.Equal("Hạ 15 quái", new SideTask { Template = new SideTaskTemplate(1, "Hạ %1 quái", Ranges()), MaxCount = 15 }.GetName());
        Assert.Equal("Hiện tại không có nhiệm vụ nào", new SideTask().GetName());
        Assert.Equal("Thu 3 và 3", new EventTask { Template = new EventTaskTemplate(1, "Thu %1 và %1", Ranges()), MaxCount = 3 }.GetName());
        Assert.Equal("Hiện tại không có nhiệm vụ nào", new EventTask().GetName());
    }

    [Theory]
    [InlineData(0, "dễ")]
    [InlineData(1, "bình thường")]
    [InlineData(2, "khó")]
    [InlineData(3, "rất khó")]
    [InlineData(4, "địa ngục")]
    [InlineData(5, "Anwin")]
    [InlineData(-1, "Anwin")]
    public void GetLevel_names_the_difficulty(int level, string expected)
    {
        Assert.Equal(expected, new SideTask { Level = level }.GetLevel());
        Assert.Equal(expected, new EventTask { Level = level }.GetLevel());
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 3, 33)]
    [InlineData(2, 3, 66)]
    [InlineData(9, 10, 90)]
    [InlineData(10, 10, 100)]
    [InlineData(11, 10, 100)]
    public void GetPercentProcess_truncates_and_caps_at_100(int count, int max, int expected)
    {
        Assert.Equal(expected, new SideTask { Count = count, MaxCount = max }.GetPercentProcess());
        Assert.Equal(expected, new EventTask { Count = count, MaxCount = max }.GetPercentProcess());
        Assert.Equal(count >= max, new SideTask { Count = count, MaxCount = max }.IsDone());
    }

    [Fact]
    public void GetPercentProcess_does_not_overflow_because_it_computes_in_64_bits()
    {
        Assert.Equal(93, new SideTask { Count = 2_000_000_000, MaxCount = int.MaxValue }.GetPercentProcess());
    }

    [Fact]
    public void GetPercentProcess_with_a_zero_amount_and_negative_progress_divides_by_zero_like_java()
    {
        Assert.Throws<DivideByZeroException>(() => new SideTask { Count = -1, MaxCount = 0 }.GetPercentProcess());
        Assert.Equal(100, new SideTask { Count = 0, MaxCount = 0 }.GetPercentProcess());
    }

    [Fact]
    public void Templates_are_immutable_copies_of_the_ranges()
    {
        int[][] ranges = Ranges();
        var template = new SideTaskTemplate(4, "x", ranges);

        ranges[0][0] = 999;
        ranges[1] = [0, 0];

        Assert.Equal(1, template.Count[0][0]);
        Assert.Equal(3, template.Count[1][0]);
        Assert.Equal(5, template.Count.Count);
        Assert.False(template.Count is int[][]);
        Assert.False(template.Count[0] is int[]);
        Assert.Equal(10, template.Count[4][1]);
    }

    [Fact]
    public void Templates_reject_a_wrong_number_of_levels_or_numbers()
    {
        Assert.Throws<ArgumentException>(() => new SideTaskTemplate(1, "x", [[1, 2]]));
        Assert.Throws<ArgumentException>(() => new EventTaskTemplate(1, "x", [[1, 2], [1, 2], [1, 2], [1, 2], [1, 2, 3]]));
        Assert.Throws<ArgumentException>(() => new EventTaskTemplate(1, "x", [[1, 2], [1, 2], [1, 2], [1, 2], null!]));
        Assert.Throws<ArgumentNullException>(() => new SideTaskTemplate(1, "x", null!));
    }

    [Fact]
    public void Two_players_with_the_same_template_have_independent_progress()
    {
        var template = new SideTaskTemplate(1, "Hạ %1 quái", Ranges());
        var playerA = new SideTask { Template = template, MaxCount = 10 };
        var playerB = new SideTask { Template = template, MaxCount = 20 };

        playerA.Count = 7;
        playerA.Notify50 = true;
        playerA.Reset();

        Assert.Equal(0, playerB.Count);
        Assert.False(playerB.Notify50);
        Assert.Same(template, playerB.Template);
        Assert.Equal("Hạ 20 quái", playerB.GetName());
    }
}

public class KolTaskTests
{
    private static async Task<TaskCatalog> LoadedCatalogAsync(FakeKolItems items, int rows = 7)
    {
        var kolRows = Enumerable.Range(0, rows).Select(i => new object?[] { i, "Thu thập %1 vật phẩm " + i, (i + 1) * 10 + "" }).ToArray();
        var db = new FakeSqlExecutor()
            .Table("from task_main_template", TaskCatalogTests.MainColumns)
            .Table("from side_task_template", TaskCatalogTests.SideColumns)
            .Table("from task_kol_template", TaskCatalogTests.KolColumns, kolRows)
            .Table("from task_event_template", TaskCatalogTests.SideColumns);
        var catalog = new TaskCatalog();
        await catalog.LoadAsync(db, items);
        return catalog;
    }

    [Fact]
    public async Task Next_moves_to_the_following_id_without_changing_any_shared_template()
    {
        TaskCatalog catalog = await LoadedCatalogAsync(new FakeKolItems());
        var playerA = new KolTask { Template = catalog.GetKolTaskTemplateById(0), Count = 10 };
        var playerB = new KolTask { Template = catalog.GetKolTaskTemplateById(0), Count = 3 };

        playerA.Next(catalog);

        Assert.Equal(1, playerA.Template!.Id);
        Assert.Equal(0, playerA.Count);
        // The Java code did ++template.id on the shared object: the table would now start with ids 1, 1, 2... and player B's task would claim to be task 1.
        Assert.Equal(0, playerB.Template!.Id);
        Assert.Equal(3, playerB.Count);
        Assert.Equal(Enumerable.Range(0, 7), catalog.KolTaskTemplates.Select(t => t.Id));
        Assert.Same(catalog.KolTaskTemplates[0], playerB.Template);
        Assert.Same(catalog.KolTaskTemplates[1], playerA.Template);
    }

    [Fact]
    public async Task Next_after_the_last_task_leaves_the_player_without_a_task()
    {
        TaskCatalog catalog = await LoadedCatalogAsync(new FakeKolItems());
        var kol = new KolTask { Template = catalog.GetKolTaskTemplateById(6), Count = 99 };

        kol.Next(catalog);

        Assert.Null(kol.Template);
        Assert.Equal(0, kol.Count);
        Assert.Equal("Đã hoàn thành hết nhiệm vụ.", kol.GetTaskInfo());
        Assert.False(kol.IsDone());
        Assert.Equal(0, kol.GetPercentProcess());
        Assert.Equal("Thưởng ", kol.GetRewardsInfo());
    }

    [Fact]
    public async Task Next_without_a_task_only_resets_the_progress()
    {
        TaskCatalog catalog = await LoadedCatalogAsync(new FakeKolItems());
        var kol = new KolTask { Count = 4 };

        kol.Next(catalog);

        Assert.Null(kol.Template);
        Assert.Equal(0, kol.Count);
    }

    [Fact]
    public async Task AddCount_and_IsDone_follow_the_amount()
    {
        TaskCatalog catalog = await LoadedCatalogAsync(new FakeKolItems());
        var kol = new KolTask { Template = catalog.GetKolTaskTemplateById(0) }; // amount 10

        for (int i = 0; i < 9; i++)
        {
            kol.AddCount();
        }

        Assert.False(kol.IsDone());
        kol.AddCount();
        Assert.True(kol.IsDone());
        Assert.Equal(100, kol.GetPercentProcess());

        var none = new KolTask();
        none.AddCount();
        Assert.Equal(0, none.Count);
    }

    [Fact]
    public async Task GetTaskInfo_replaces_the_amount()
    {
        TaskCatalog catalog = await LoadedCatalogAsync(new FakeKolItems());

        Assert.Equal("Thu thập 20 vật phẩm 1", new KolTask { Template = catalog.GetKolTaskTemplateById(1) }.GetTaskInfo());
    }

    [Fact]
    public void GetPercentProcess_overflows_in_32_bit_like_java()
    {
        var template = new KolTaskTemplate(5, "x", 1_000_000_000, new FakeKolItems());
        var kol = new KolTask { Template = template, Count = 21_474_837 }; // 21,474,837 * 100 wraps negative

        Assert.Equal(unchecked(21_474_837 * 100) / 1_000_000_000, kol.GetPercentProcess());
        Assert.True(kol.GetPercentProcess() <= 0);
        Assert.Equal(0, new KolTask { Template = new KolTaskTemplate(5, "x", 0, new FakeKolItems()), Count = 3 }.GetPercentProcess());
    }

    [Theory]
    [InlineData(0, 1836, 1)]
    [InlineData(0, 1837, 0)]
    [InlineData(1, 1837, 1)]
    [InlineData(1, 1836, 0)]
    [InlineData(2, 1836, 0)]
    [InlineData(2, 1837, 0)]
    public async Task CheckDonePickItem_counts_only_the_item_of_tasks_0_and_1(int taskId, int pickedItem, int expectedCount)
    {
        TaskCatalog catalog = await LoadedCatalogAsync(new FakeKolItems());
        var kol = new KolTask { Template = catalog.GetKolTaskTemplateById(taskId) };

        kol.CheckDonePickItem(pickedItem);

        Assert.Equal(expectedCount, kol.Count);
    }

    [Fact]
    public void CheckDonePickItem_without_a_task_does_nothing()
    {
        var kol = new KolTask();

        kol.CheckDonePickItem(1836);

        Assert.Equal(0, kol.Count);
    }

    [Fact]
    public async Task Rewards_follow_the_java_table_by_task_id()
    {
        var items = new FakeKolItems();
        TaskCatalog catalog = await LoadedCatalogAsync(items);

        (int Template, int Quantity)[][] expected =
        [
            [(1820, 3)],
            [(1592, 5), (1757, 5)],
            [(1360, 1)],
            [(1654, 1)],
            [(1808, 1)],
            [(1829, 1), (1592, 5), (1757, 5)],
            [(1592, 10), (1757, 10)],
        ];
        for (int id = 0; id < 7; id++)
        {
            Assert.Equal(expected[id], catalog.KolTaskTemplates[id].Rewards.Select(r => (r.Template!.Id + 0, r.Quantity)));
        }

        Assert.Equal([93], catalog.KolTaskTemplates[3].Rewards[0].ItemOptions.Select(o => o.OptionTemplate.Id));
        Assert.Equal([50, 103, 93], catalog.KolTaskTemplates[5].Rewards[0].ItemOptions.Select(o => o.OptionTemplate.Id));
        Assert.Equal([25, 30, 180], catalog.KolTaskTemplates[5].Rewards[0].ItemOptions.Select(o => o.Param));
    }

    [Fact]
    public void A_template_with_an_unknown_id_has_no_rewards()
    {
        Assert.Empty(new KolTaskTemplate(99, "x", 1, new FakeKolItems()).Rewards);
        Assert.Empty(new KolTaskTemplate(-1, "x", 1, new FakeKolItems()).Rewards);
    }

    [Fact]
    public async Task GetRewardsInfo_lists_quantities_names_and_the_days_of_an_expiry_option()
    {
        TaskCatalog catalog = await LoadedCatalogAsync(new FakeKolItems());

        Assert.Equal("Thưởng 3 item1820", new KolTask { Template = catalog.GetKolTaskTemplateById(0) }.GetRewardsInfo());
        Assert.Equal("Thưởng 5 item1592, 5 item1757", new KolTask { Template = catalog.GetKolTaskTemplateById(1) }.GetRewardsInfo());
        Assert.Equal("Thưởng 1 item1654 (120 ngày)", new KolTask { Template = catalog.GetKolTaskTemplateById(3) }.GetRewardsInfo());
        Assert.Equal("Thưởng 1 item1829 (180 ngày), 5 item1592, 5 item1757", new KolTask { Template = catalog.GetKolTaskTemplateById(5) }.GetRewardsInfo());
        Assert.Equal("Thưởng 10 item1592, 10 item1757", new KolTask { Template = catalog.GetKolTaskTemplateById(6) }.GetRewardsInfo());
    }

    [Fact]
    public async Task Receive_before_the_task_is_done_only_says_so()
    {
        var host = new FakeKolHost();
        TaskCatalog catalog = await LoadedCatalogAsync(host);
        var kol = new KolTask { Template = catalog.GetKolTaskTemplateById(0), Count = 9 };

        kol.Receive(new Player(), host, catalog);

        Assert.Equal(["Bạn chưa hoàn thành nhiệm vụ"], host.Notifications);
        Assert.Empty(host.Bag);
        Assert.Equal(0, host.ItemBagSent);
        Assert.Equal(0, kol.Template!.Id);
        Assert.Equal(9, kol.Count);
    }

    [Fact]
    public async Task Receive_needs_one_less_empty_slot_than_rewards_a_java_quirk()
    {
        var host = new FakeKolHost { EmptyBagSlots = 0 };
        TaskCatalog catalog = await LoadedCatalogAsync(host);

        var oneReward = new KolTask { Template = catalog.GetKolTaskTemplateById(0), Count = 10 };
        oneReward.Receive(new Player(), host, catalog);
        Assert.Single(host.Bag); // 0 empty slots are enough for one reward
        Assert.Equal(1, oneReward.Template!.Id);

        host.Notifications.Clear();
        host.Bag.Clear();
        var twoRewards = new KolTask { Template = catalog.GetKolTaskTemplateById(1), Count = 20 };
        twoRewards.Receive(new Player(), host, catalog);
        Assert.Equal(["Bạn cần trống 1 ô hành trang!"], host.Notifications);
        Assert.Empty(host.Bag);
        Assert.Equal(1, twoRewards.Template!.Id);
        Assert.Equal(20, twoRewards.Count);
    }

    [Fact]
    public async Task Receive_gives_copies_of_the_rewards_sends_the_bag_and_moves_to_the_next_task()
    {
        var host = new FakeKolHost();
        TaskCatalog catalog = await LoadedCatalogAsync(host);
        var player = new Player();
        var kol = new KolTask { Template = catalog.GetKolTaskTemplateById(5), Count = 70 };

        kol.Receive(player, host, catalog);

        Assert.Equal(["Bạn nhận được 1 item1829", "Bạn nhận được 5 item1592", "Bạn nhận được 5 item1757"], host.Notifications);
        Assert.Equal(3, host.Bag.Count);
        Assert.Equal(1, host.ItemBagSent);
        Assert.Equal(6, kol.Template!.Id);
        Assert.Equal(0, kol.Count);

        // The bag holds copies: changing one never touches the shared reward of the template.
        KolTaskTemplate fifth = catalog.KolTaskTemplates[5];
        Assert.NotSame(fifth.Rewards[0], host.Bag[0]);
        Assert.NotSame(fifth.Rewards[0].ItemOptions[0], host.Bag[0].ItemOptions[0]);
        host.Bag[0].Quantity = 99;
        host.Bag[0].ItemOptions[0].Param = 12345;
        Assert.Equal(1, fifth.Rewards[0].Quantity);
        Assert.Equal(25, fifth.Rewards[0].ItemOptions[0].Param);
    }

    [Fact]
    public async Task Receive_on_the_last_task_ends_the_chain()
    {
        var host = new FakeKolHost();
        TaskCatalog catalog = await LoadedCatalogAsync(host);
        var kol = new KolTask { Template = catalog.GetKolTaskTemplateById(6), Count = 70 };

        kol.Receive(new Player(), host, catalog);

        Assert.Equal(2, host.Bag.Count);
        Assert.Null(kol.Template);
    }

    [Fact]
    public async Task Receive_without_a_task_does_nothing()
    {
        var host = new FakeKolHost();
        TaskCatalog catalog = await LoadedCatalogAsync(host);

        new KolTask().Receive(new Player(), host, catalog);

        Assert.Empty(host.Notifications);
    }

    [Fact]
    public void Dispose_forgets_the_template()
    {
        var kol = new KolTask { Template = new KolTaskTemplate(1, "x", 1, new FakeKolItems()) };

        kol.Dispose();

        Assert.Null(kol.Template);
    }

    [Fact]
    public void Rewards_built_by_the_real_item_service_carry_the_lock_option_and_are_copied_for_the_player()
    {
        ItemTemplate[] templates = Enumerable.Range(0, 1900).Select(i => new ItemTemplate { Id = (short)i, Name = "Vật phẩm " + i }).ToArray();
        ItemOptionTemplate[] options = Enumerable.Range(0, 300).Select(i => new ItemOptionTemplate(i, "Option " + i + " #", 0)).ToArray();
        var itemService = new ItemService(new ItemCatalog(templates, options), new RandomUtil(new ScriptedRandomProvider()), new VietnamClock(new ManualTimeProvider(ManualTimeProvider.Vn(2026, 11, 3))));
        var factory = new KolTaskItemFactory(itemService);

        var template = new KolTaskTemplate(3, "x", 1, factory);

        Item reward = Assert.Single(template.Rewards);
        Assert.Equal(1654, reward.Template!.Id);
        Assert.Equal([30, 93], reward.ItemOptions.Select(o => o.OptionTemplate.Id));
        Assert.Equal([1, 120], reward.ItemOptions.Select(o => o.Param));
        Item copy = factory.CopyItem(reward);
        Assert.NotSame(reward, copy);
        Assert.NotSame(reward.ItemOptions[1], copy.ItemOptions[1]);
        Assert.Equal(120, copy.ItemOptions[1].Param);
    }
}

public class TaskCatalogTests
{
    internal static readonly string[] MainColumns = ["id", "name", "detail", "sub_name", "max_count", "notify", "npc_id", "map"];
    internal static readonly string[] SideColumns = ["id", "name", "max_count_lv1", "max_count_lv2", "max_count_lv3", "max_count_lv4", "max_count_lv5"];
    internal static readonly string[] KolColumns = ["id", "info", "max_count"];

    private static FakeSqlExecutor Db() => new FakeSqlExecutor()
        .Table("from task_main_template", MainColumns,
            new object?[] { 0, "Nhiệm vụ 0", "chi tiết 0", "bước 0.0", 1, "n0.0", 5, 21 },
            new object?[] { 0, "Nhiệm vụ 0", "chi tiết 0", "bước 0.1", -1, "n0.1", -1, 22 },
            new object?[] { 1, "Nhiệm vụ 1", "chi tiết 1", "bước 1.0", 10, "n1.0", 8, 23 })
        .Table("from side_task_template", SideColumns,
            new object?[] { 0, "Hạ %1 quái", "1-5", "6-10", "11-15", "16-20", "21-25" },
            new object?[] { 1, "Nhặt %1", "2-3", "4-5", "6-7", "8-9", "10-11" })
        .Table("from task_kol_template", KolColumns,
            new object?[] { 0, "Kol %1", "10" },
            new object?[] { 1, "Kol2 %1", "20" })
        .Table("from task_event_template", SideColumns,
            new object?[] { 0, "Sự kiện %1", "1-2", "3-4", "5-6", "7-8", "9-10" });

    private static async Task<TaskCatalog> LoadAsync(FakeSqlExecutor? db = null)
    {
        var catalog = new TaskCatalog();
        await catalog.LoadAsync(db ?? Db(), new FakeKolItems());
        return catalog;
    }

    [Fact]
    public async Task Main_tasks_are_grouped_by_id_with_their_steps_in_row_order()
    {
        FakeSqlExecutor db = Db();

        TaskCatalog catalog = await LoadAsync(db);

        Assert.Equal(2, catalog.TaskMains.Count);
        Assert.Equal(0, catalog.TaskMains[0].Id);
        Assert.Equal("Nhiệm vụ 0", catalog.TaskMains[0].Name);
        Assert.Equal("chi tiết 0", catalog.TaskMains[0].Detail);
        Assert.Equal(["bước 0.0", "bước 0.1"], catalog.TaskMains[0].SubTasks.Select(s => s.Name));
        Assert.Equal([(short)1, (short)-1], catalog.TaskMains[0].SubTasks.Select(s => s.MaxCount));
        Assert.Equal([(sbyte)5, (sbyte)-1], catalog.TaskMains[0].SubTasks.Select(s => s.NpcId));
        Assert.Equal([(short)21, (short)22], catalog.TaskMains[0].SubTasks.Select(s => s.MapId));
        Assert.Equal("n0.0", catalog.TaskMains[0].SubTasks[0].Notify);
        Assert.Single(catalog.TaskMains[1].SubTasks);
        Assert.Equal(0, catalog.TaskMains[0].SubTasks[0].Count);
        Assert.Contains("order by m.id, s.idmain", db.QueryLog[0]);
    }

    [Fact]
    public async Task Side_event_and_kol_templates_are_loaded_and_parsed()
    {
        TaskCatalog catalog = await LoadAsync();

        Assert.Equal(2, catalog.SideTaskTemplates.Count);
        Assert.Equal("Hạ %1 quái", catalog.SideTaskTemplates[0].Name);
        Assert.Equal(1, catalog.SideTaskTemplates[0].Count[0][0]);
        Assert.Equal(5, catalog.SideTaskTemplates[0].Count[0][1]);
        Assert.Equal(21, catalog.SideTaskTemplates[0].Count[4][0]);
        Assert.Equal(25, catalog.SideTaskTemplates[0].Count[4][1]);
        Assert.Equal(11, catalog.SideTaskTemplates[1].Count[4][1]);
        Assert.Single(catalog.EventTaskTemplates);
        Assert.Equal(9, catalog.EventTaskTemplates[0].Count[4][0]);
        Assert.Equal([10, 20], catalog.KolTaskTemplates.Select(k => k.MaxCount));
        Assert.Equal(["Kol %1", "Kol2 %1"], catalog.KolTaskTemplates.Select(k => k.Info));
    }

    [Fact]
    public async Task Every_query_is_ordered_so_the_position_of_a_template_is_its_id()
    {
        FakeSqlExecutor db = Db();

        await LoadAsync(db);

        Assert.Equal(4, db.QueryLog.Count);
        Assert.All(db.QueryLog, sql => Assert.Contains("order by", sql));
        Assert.All(db.QueryLog, sql => Assert.DoesNotContain("select *", sql));
    }

    [Fact]
    public async Task CreateTaskMainCopy_returns_a_new_independent_copy_each_time()
    {
        TaskCatalog catalog = await LoadAsync();

        TaskMain first = catalog.CreateTaskMainCopy(0)!;
        TaskMain second = catalog.CreateTaskMainCopy(0)!;
        first.SubTasks[0].Count = 1;
        first.Name = "đã đổi";

        Assert.NotSame(first, second);
        Assert.NotSame(catalog.TaskMains[0], first);
        Assert.Equal(0, second.SubTasks[0].Count);
        Assert.Equal("Nhiệm vụ 0", second.Name);
        Assert.Equal(0, catalog.TaskMains[0].SubTasks[0].Count);
        Assert.Equal("Nhiệm vụ 0", catalog.TaskMains[0].Name);
        Assert.Null(catalog.CreateTaskMainCopy(99));
    }

    [Fact]
    public async Task Two_players_never_share_main_task_state()
    {
        TaskCatalog catalog = await LoadAsync();
        var playerA = new Player();
        var playerB = new Player();
        playerA.PlayerTask.TaskMain = catalog.CreateTaskMainCopy(0);
        playerB.PlayerTask.TaskMain = catalog.CreateTaskMainCopy(0);

        playerA.PlayerTask.TaskMain!.SubTasks[0].Count = 1;
        playerA.PlayerTask.TaskMain.SubTasks[1].Name = "chỉ của A";
        playerA.PlayerTask.TaskMain.Detail = "chỉ của A";

        Assert.Equal(0, playerB.PlayerTask.TaskMain!.SubTasks[0].Count);
        Assert.Equal("bước 0.1", playerB.PlayerTask.TaskMain.SubTasks[1].Name);
        Assert.Equal("chi tiết 0", playerB.PlayerTask.TaskMain.Detail);
        Assert.Equal(0, catalog.TaskMains[0].SubTasks[0].Count);
        Assert.Equal("bước 0.1", catalog.TaskMains[0].SubTasks[1].Name);
    }

    [Fact]
    public async Task Lookups_by_position_follow_the_java_rules()
    {
        TaskCatalog catalog = await LoadAsync();

        Assert.Null(catalog.GetSideTaskTemplateById(-1));
        Assert.Null(catalog.GetEventTaskTemplateById(-1));
        Assert.Null(catalog.GetKolTaskTemplateById(-1));
        Assert.Equal(1, catalog.GetSideTaskTemplateById(1)!.Id);
        Assert.Equal(0, catalog.GetEventTaskTemplateById(0)!.Id);
        Assert.Equal(1, catalog.GetKolTaskTemplateById(1)!.Id);
        Assert.Throws<ArgumentOutOfRangeException>(() => catalog.GetSideTaskTemplateById(2));
        Assert.Throws<ArgumentOutOfRangeException>(() => catalog.GetEventTaskTemplateById(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => catalog.GetSideTaskTemplateById(-2));
        Assert.Null(catalog.GetKolTaskTemplateById(2));
        Assert.Null(catalog.GetKolTaskTemplateById(-2));
    }

    [Fact]
    public async Task A_second_load_replaces_the_first()
    {
        TaskCatalog catalog = await LoadAsync();

        await catalog.LoadAsync(
            new FakeSqlExecutor()
                .Table("from task_main_template", MainColumns)
                .Table("from side_task_template", SideColumns)
                .Table("from task_kol_template", KolColumns)
                .Table("from task_event_template", SideColumns),
            new FakeKolItems());

        Assert.Empty(catalog.TaskMains);
        Assert.Empty(catalog.SideTaskTemplates);
        Assert.Empty(catalog.KolTaskTemplates);
        Assert.Empty(catalog.EventTaskTemplates);
    }

    [Fact]
    public async Task A_failing_query_changes_nothing()
    {
        TaskCatalog catalog = await LoadAsync();
        FakeSqlExecutor failing = Db();
        failing.FailQueryContaining = ("from task_event_template", new InvalidOperationException("db down"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => catalog.LoadAsync(failing, new FakeKolItems()));

        Assert.Equal(2, catalog.TaskMains.Count);
        Assert.Equal(2, catalog.SideTaskTemplates.Count);
        Assert.Single(catalog.EventTaskTemplates);
    }

    [Theory]
    [InlineData("1-")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("1-x")]
    [InlineData("3000000000-1")]
    public async Task A_malformed_range_is_a_data_error_that_names_the_table_and_id_but_not_the_text(string range)
    {
        var db = new FakeSqlExecutor()
            .Table("from task_main_template", MainColumns)
            .Table("from side_task_template", SideColumns, new object?[] { 42, "x", "1-2", range, "1-2", "1-2", "1-2" })
            .Table("from task_kol_template", KolColumns)
            .Table("from task_event_template", SideColumns);

        InvalidDataException error = await Assert.ThrowsAsync<InvalidDataException>(() => LoadAsync(db));

        Assert.Contains("side_task_template", error.Message);
        Assert.Contains("42", error.Message);
        Assert.Contains("max_count_lv2", error.Message);
    }

    [Fact]
    public async Task A_kol_amount_that_is_not_a_number_is_a_data_error()
    {
        var db = new FakeSqlExecutor()
            .Table("from task_main_template", MainColumns)
            .Table("from side_task_template", SideColumns)
            .Table("from task_kol_template", KolColumns, new object?[] { 3, "x", "lots" })
            .Table("from task_event_template", SideColumns);

        InvalidDataException error = await Assert.ThrowsAsync<InvalidDataException>(() => LoadAsync(db));

        Assert.Contains("task_kol_template", error.Message);
    }

    [Fact]
    public async Task Arguments_are_checked()
    {
        var catalog = new TaskCatalog();

        await Assert.ThrowsAsync<ArgumentNullException>(() => catalog.LoadAsync(null!, new FakeKolItems()));
        await Assert.ThrowsAsync<ArgumentNullException>(() => catalog.LoadAsync(new FakeSqlExecutor(), null!));
    }
}

public class TaskPlayerTests
{
    [Fact]
    public void A_new_player_task_state_has_its_own_components_and_no_main_task()
    {
        var state = new TaskPlayer();

        Assert.Null(state.TaskMain);
        Assert.NotNull(state.SideTask);
        Assert.NotNull(state.KolTask);
        Assert.NotNull(state.EventTask);
        Assert.Equal(ConstTask.MAX_SIDE_TASK, state.SideTask.LeftTask);
        Assert.Equal(ConstTask.MAX_EVENT_TASK, state.EventTask.LeftTask);
    }

    [Fact]
    public void Two_players_do_not_share_any_component()
    {
        var a = new Player();
        var b = new Player();

        a.PlayerTask.SideTask.Count = 5;
        a.PlayerTask.KolTask.Count = 6;
        a.PlayerTask.EventTask.Count = 7;

        Assert.NotSame(a.PlayerTask, b.PlayerTask);
        Assert.NotSame(a.PlayerTask.SideTask, b.PlayerTask.SideTask);
        Assert.NotSame(a.PlayerTask.KolTask, b.PlayerTask.KolTask);
        Assert.NotSame(a.PlayerTask.EventTask, b.PlayerTask.EventTask);
        Assert.Equal(0, b.PlayerTask.SideTask.Count);
        Assert.Equal(0, b.PlayerTask.KolTask.Count);
        Assert.Equal(0, b.PlayerTask.EventTask.Count);
    }

    [Fact]
    public void Dispose_forgets_the_main_task_and_the_kol_template_but_keeps_the_components()
    {
        var state = new TaskPlayer { TaskMain = new TaskMain() };
        state.KolTask.Template = new KolTaskTemplate(1, "x", 1, new FakeKolItems());
        SideTask side = state.SideTask;

        state.Dispose();

        Assert.Null(state.TaskMain);
        Assert.Null(state.KolTask.Template);
        Assert.Same(side, state.SideTask);
        state.Dispose();
    }
}
