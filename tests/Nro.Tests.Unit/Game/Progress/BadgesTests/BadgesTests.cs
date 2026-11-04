using Nro.Core.Templates;
using Nro.Game;
using Nro.Tests.Unit.Game.Support;

namespace Nro.Tests.Unit.Game.Progress.BadgesTests;

public class BadgesDataTests
{
    [Fact]
    public void Defaults_match_the_java_constructors()
    {
        var empty = new BadgesData();
        Assert.Equal(-1, empty.IdBadGes);
        Assert.Equal(-1, empty.TimeofUseBadges);
        Assert.False(empty.IsUse);

        var full = new BadgesData(5, 1234, true);
        Assert.Equal(5, full.IdBadGes);
        Assert.Equal(1234, full.TimeofUseBadges);
        Assert.True(full.IsUse);

        var badges = new Nro.Game.Progress.Badges.Badges();
        Assert.Equal(-1, badges.IdBadges);
        Assert.Equal(0, badges.LastTimeSendBadges);
    }

    [Fact]
    public void Giving_a_badge_sets_the_expiry_switches_the_others_off_and_adds_itself()
    {
        var player = new Player();
        var old1 = new BadgesData(1, 10, true);
        var old2 = new BadgesData(2, 20, true);
        player.DataBadges.Add(old1);
        player.DataBadges.Add(old2);

        var added = new BadgesData(player, 9, 30, 1_000_000L);

        Assert.Equal(9, added.IdBadGes);
        Assert.Equal(1_000_000L + 30L * 24 * 60 * 60 * 1000, added.TimeofUseBadges);
        Assert.True(added.IsUse);
        Assert.False(old1.IsUse);
        Assert.False(old2.IsUse);
        Assert.Equal(3, player.DataBadges.Count);
        Assert.Same(added, player.DataBadges[2]);
    }

    [Fact]
    public void The_day_count_overflows_in_32_bit_before_the_last_factor_like_java()
    {
        var player = new Player();

        var added = new BadgesData(player, 1, 30000, 0);

        // 30000 * 24 * 60 * 60 = 2,592,000,000 wraps to -1,702,967,296 in int, then * 1000L
        Assert.Equal(-1_702_967_296_000L, added.TimeofUseBadges);
    }

    [Fact]
    public void ToString_is_the_saved_text_with_quoted_values_and_lower_case_booleans()
    {
        Assert.Equal("{\"idBadGes\":\"5\",\"timeofUseBadges\":\"1700000000000\",\"isUse\":\"true\"}", new BadgesData(5, 1_700_000_000_000L, true).ToString());
        Assert.Equal("{\"idBadGes\":\"-1\",\"timeofUseBadges\":\"-1\",\"isUse\":\"false\"}", new BadgesData().ToString());
    }

    [Fact]
    public void A_null_player_is_rejected()
    {
        Assert.Throws<ArgumentNullException>(() => new BadgesData(null!, 1, 1, 0));
    }
}

public class BadgesTaskTests
{
    [Fact]
    public void Defaults_are_minus_one_and_such_a_task_counts_as_done()
    {
        var task = new BadgesTask();

        Assert.Equal(-1, task.Id);
        Assert.Equal(-1, task.Count);
        Assert.Equal(-1, task.CountMax);
        Assert.Equal(-1, task.IdBadgesReward);
        Assert.True(task.IsDone());
        Assert.Equal(100, task.GetPercentProcess());
    }

    [Theory]
    [InlineData(0, 5, 0, false)]
    [InlineData(1, 3, 33, false)]
    [InlineData(5, 5, 100, true)]
    [InlineData(7, 5, 100, true)]
    public void IsDone_and_GetPercentProcess(int count, int max, int percent, bool done)
    {
        var task = new BadgesTask { Count = count, CountMax = max };

        Assert.Equal(done, task.IsDone());
        Assert.Equal(percent, task.GetPercentProcess());
    }

    [Fact]
    public void GetPercentProcess_computes_in_64_bits_and_divides_by_zero_like_java()
    {
        Assert.Equal(0, new BadgesTask { Count = 1, CountMax = int.MaxValue }.GetPercentProcess());
        Assert.Equal(93, new BadgesTask { Count = 2_000_000_000, CountMax = int.MaxValue }.GetPercentProcess());
        Assert.Throws<DivideByZeroException>(() => new BadgesTask { Count = -1, CountMax = 0 }.GetPercentProcess());
    }

    [Fact]
    public void ToString_is_the_saved_text()
    {
        var task = new BadgesTask { Id = 1, Count = 2, CountMax = 5, IdBadgesReward = 3 };

        Assert.Equal("{\"id\":\"1\",\"count\":\"2\",\"countMax\":\"5\",\"idBadgesReward\":\"3\"}", task.ToString());
    }
}

public class BadgesServiceTests
{
    [Fact]
    public void TurnOnBadges_switches_on_that_badge_and_switches_off_the_rest()
    {
        var player = new Player();
        player.DataBadges.Add(new BadgesData(1, 0, true));
        player.DataBadges.Add(new BadgesData(2, 0, false));
        player.DataBadges.Add(new BadgesData(3, 0, true));

        BadgesService.TurnOnBadges(player, 2);

        Assert.Equal([false, true, false], player.DataBadges.Select(b => b.IsUse));
    }

    [Fact]
    public void TurnOnBadges_for_a_badge_the_player_does_not_own_switches_everything_off()
    {
        var player = new Player();
        player.DataBadges.Add(new BadgesData(1, 0, true));

        BadgesService.TurnOnBadges(player, 99);

        Assert.False(player.DataBadges[0].IsUse);
    }
}

public class BadgesTaskServiceTests
{
    private static readonly string[] TaskColumns = ["id", "name", "max_count", "id_badges_reward"];
    private static readonly string[] BadgeColumns = ["id", "id_effect", "id_item", "name", "options"];

    private static async Task<BadgesCatalog> CatalogAsync()
    {
        var itemCatalog = new ItemCatalog([], Enumerable.Range(0, 200).Select(i => new ItemOptionTemplate(i, "Option " + i + " #", 0)).ToArray());
        var db = new FakeSqlExecutor()
            .Table("from task_badges_template", TaskColumns,
                new object?[] { 1, "Giết boss", 5, 10 },
                new object?[] { 2, "Nhặt đồ", 3, 11 })
            .Table("from data_badges", BadgeColumns);
        var catalog = new BadgesCatalog();
        await catalog.LoadAsync(db, itemCatalog);
        return catalog;
    }

    [Fact]
    public async Task CreateAndResetTask_makes_one_fresh_task_per_template_and_forgets_the_old_ones()
    {
        BadgesCatalog catalog = await CatalogAsync();
        var player = new Player();
        player.DataTaskBadges.Add(new BadgesTask { Id = 99, Count = 5 });

        BadgesTaskService.CreateAndResetTask(player, catalog);

        Assert.Equal(2, player.DataTaskBadges.Count);
        Assert.Equal([1, 2], player.DataTaskBadges.Select(t => t.Id));
        Assert.Equal([0, 0], player.DataTaskBadges.Select(t => t.Count));
        Assert.Equal([5, 3], player.DataTaskBadges.Select(t => t.CountMax));
        Assert.Equal([10, 11], player.DataTaskBadges.Select(t => t.IdBadgesReward));
    }

    [Fact]
    public async Task Two_players_get_their_own_task_objects()
    {
        BadgesCatalog catalog = await CatalogAsync();
        var a = new Player();
        var b = new Player();
        BadgesTaskService.CreateAndResetTask(a, catalog);
        BadgesTaskService.CreateAndResetTask(b, catalog);

        BadgesTaskService.UpdateCountBagesTask(a, 1, 2);

        Assert.Equal(2, a.DataTaskBadges[0].Count);
        Assert.Equal(0, b.DataTaskBadges[0].Count);
    }

    [Fact]
    public void UpdateCountBagesTask_adds_caps_at_the_maximum_and_touches_only_the_first_match()
    {
        var player = new Player();
        player.DataTaskBadges.Add(new BadgesTask { Id = 1, Count = 3, CountMax = 5 });
        player.DataTaskBadges.Add(new BadgesTask { Id = 1, Count = 0, CountMax = 5 });

        BadgesTaskService.UpdateCountBagesTask(player, 1, 1);
        Assert.Equal(4, player.DataTaskBadges[0].Count);

        BadgesTaskService.UpdateCountBagesTask(player, 1, 10);
        Assert.Equal(5, player.DataTaskBadges[0].Count);
        Assert.Equal(0, player.DataTaskBadges[1].Count);

        BadgesTaskService.UpdateCountBagesTask(player, 99, 1);
        Assert.Equal(5, player.DataTaskBadges[0].Count);
    }

    [Fact]
    public void UpdateDoneTask_gives_the_reward_badge_for_30_days_and_restarts_the_task()
    {
        var player = new Player();
        player.DataTaskBadges.Add(new BadgesTask { Id = 1, Count = 5, CountMax = 5, IdBadgesReward = 10 });
        player.DataTaskBadges.Add(new BadgesTask { Id = 2, Count = 1, CountMax = 5, IdBadgesReward = 11 });
        player.DataBadges.Add(new BadgesData(3, 5, true));

        BadgesTaskService.UpdateDoneTask(player, 1_000L);

        Assert.Equal(0, player.DataTaskBadges[0].Count);
        Assert.Equal(1, player.DataTaskBadges[1].Count);
        Assert.False(player.DataBadges[0].IsUse);
        // A Java quirk: the constructor adds the badge and the service adds it again, so it is in the list twice.
        Assert.Equal(3, player.DataBadges.Count);
        Assert.Same(player.DataBadges[1], player.DataBadges[2]);
        Assert.Equal(10, player.DataBadges[1].IdBadGes);
        Assert.Equal(1_000L + 30L * 86_400_000L, player.DataBadges[1].TimeofUseBadges);
        Assert.True(player.DataBadges[1].IsUse);
    }

    [Fact]
    public void UpdateDoneTask_stops_completely_when_the_player_already_owns_a_finished_task_s_badge()
    {
        var player = new Player();
        player.DataTaskBadges.Add(new BadgesTask { Id = 1, Count = 5, CountMax = 5, IdBadgesReward = 10 });
        player.DataTaskBadges.Add(new BadgesTask { Id = 2, Count = 5, CountMax = 5, IdBadgesReward = 11 });
        player.DataBadges.Add(new BadgesData(10, 5, false));

        BadgesTaskService.UpdateDoneTask(player, 0);

        // The first finished task finds its badge and ends the method: the second task is not looked at and nothing is restarted.
        Assert.Single(player.DataBadges);
        Assert.Equal(5, player.DataTaskBadges[0].Count);
        Assert.Equal(5, player.DataTaskBadges[1].Count);
    }

    [Fact]
    public void SendPercenBadgesTask_answers_the_percent_of_the_first_task_for_that_badge()
    {
        var player = new Player();
        player.DataTaskBadges.Add(new BadgesTask { Id = 1, Count = 1, CountMax = 4, IdBadgesReward = 10 });
        player.DataTaskBadges.Add(new BadgesTask { Id = 2, Count = 4, CountMax = 4, IdBadgesReward = 10 });

        Assert.Equal(25, BadgesTaskService.SendPercenBadgesTask(player, 10));
        Assert.Equal(0, BadgesTaskService.SendPercenBadgesTask(player, 77));
    }

    [Theory]
    [InlineData(0L, 0)]
    [InlineData(86_399_999L, 0)]
    [InlineData(86_400_000L, 1)]
    [InlineData(30L * 86_400_000L, 30)]
    [InlineData(-86_399_999L, 0)]
    [InlineData(-86_400_001L, -1)]
    public void SendDay_counts_whole_days_left_rounding_toward_zero(long remainingMs, int expectedDays)
    {
        var player = new Player();
        player.DataBadges.Add(new BadgesData(7, 5_000L + remainingMs, true));

        Assert.Equal(expectedDays, BadgesTaskService.SendDay(player, 7, 5_000L));
        Assert.Equal(0, BadgesTaskService.SendDay(player, 8, 5_000L));
    }
}

public class BadgesCatalogTests
{
    private static readonly string[] TaskColumns = ["id", "name", "max_count", "id_badges_reward"];
    private static readonly string[] BadgeColumns = ["id", "id_effect", "id_item", "name", "options"];

    private static ItemCatalog Items() => new([], Enumerable.Range(0, 200).Select(i => new ItemOptionTemplate(i, "Option " + i + " #", 0)).ToArray());

    private static FakeSqlExecutor Db() => new FakeSqlExecutor()
        .Table("from task_badges_template", TaskColumns,
            new object?[] { 1, "Giết boss", 5, 10 },
            new object?[] { 2, "Nhặt đồ", 3, 11 })
        .Table("from data_badges", BadgeColumns,
            new object?[] { 1, 10, 1000, "Danh hiệu A", "[{\"id\":50,\"param\":5},{\"id\":77,\"param\":20}]" },
            new object?[] { 2, 11, 1001, "Danh hiệu B", "[{\"id\":\"50\",\"param\":\"7\"}]" },
            new object?[] { 3, 12, 1002, "Danh hiệu C", "not json" },
            new object?[] { 4, 13, 1003, "Danh hiệu D", "null" },
            new object?[] { 5, 10, 1004, "Trùng hiệu ứng", "[]" });

    private static async Task<BadgesCatalog> LoadAsync()
    {
        var catalog = new BadgesCatalog();
        await catalog.LoadAsync(Db(), Items());
        return catalog;
    }

    [Fact]
    public async Task LoadAsync_reads_the_badges_with_their_options_and_the_task_templates()
    {
        BadgesCatalog catalog = await LoadAsync();

        Assert.Equal(5, catalog.BagesTemplates.Count);
        BagesTemplate first = catalog.BagesTemplates[0];
        Assert.Equal(1, first.Id);
        Assert.Equal(10, first.IdEffect);
        Assert.Equal(1000, first.IdItem);
        Assert.Equal("Danh hiệu A", first.NAME);
        Assert.Equal([50, 77], first.Options.Select(o => o.OptionTemplate.Id));
        Assert.Equal([5, 20], first.Options.Select(o => o.Param));
        Assert.Equal([50], catalog.BagesTemplates[1].Options.Select(o => o.OptionTemplate.Id));
        Assert.Equal([7], catalog.BagesTemplates[1].Options.Select(o => o.Param));
        Assert.Equal(2, catalog.TaskTemplates.Count);
        Assert.Equal("Giết boss", catalog.TaskTemplates[0].Name);
        Assert.Equal(5, catalog.TaskTemplates[0].Count);
        Assert.Equal(10, catalog.TaskTemplates[0].IdbadgesReward);
    }

    [Fact]
    public async Task Options_that_are_not_json_or_json_null_give_a_badge_without_options()
    {
        BadgesCatalog catalog = await LoadAsync();

        Assert.Empty(catalog.BagesTemplates[2].Options);
        Assert.Empty(catalog.BagesTemplates[3].Options);
        Assert.Empty(catalog.BagesTemplates[4].Options);
    }

    [Fact]
    public async Task Both_queries_are_ordered_by_id_and_the_tasks_are_read_first_like_java()
    {
        FakeSqlExecutor db = Db();

        await new BadgesCatalog().LoadAsync(db, Items());

        Assert.Equal(2, db.QueryLog.Count);
        Assert.Contains("task_badges_template", db.QueryLog[0]);
        Assert.Contains("data_badges", db.QueryLog[1]);
        Assert.All(db.QueryLog, sql => Assert.Contains("order by id", sql));
    }

    [Theory]
    [InlineData("{\"id\":1}")]
    [InlineData("[{\"id\":1}]")]
    [InlineData("[{\"param\":1}]")]
    [InlineData("[{\"id\":\"x\",\"param\":1}]")]
    [InlineData("[{\"id\":1.5,\"param\":1}]")]
    [InlineData("[5]")]
    [InlineData("[{\"id\":999,\"param\":1}]")]
    public async Task A_well_formed_array_with_a_bad_option_fails_the_load_and_changes_nothing(string options)
    {
        BadgesCatalog catalog = await LoadAsync();
        var db = new FakeSqlExecutor()
            .Table("from task_badges_template", TaskColumns)
            .Table("from data_badges", BadgeColumns, new object?[] { 9, 1, 1, "x", options });

        await Assert.ThrowsAnyAsync<Exception>(() => catalog.LoadAsync(db, Items()));

        Assert.Equal(5, catalog.BagesTemplates.Count);
    }

    [Fact]
    public async Task Lookups_by_effect_and_by_item_return_the_first_match_or_minus_one()
    {
        BadgesCatalog catalog = await LoadAsync();

        Assert.Equal(1000, catalog.FindIdItemByIdIdEffect(10));
        Assert.Equal(1002, catalog.FindIdItemByIdIdEffect(12));
        Assert.Equal(-1, catalog.FindIdItemByIdIdEffect(99));
        Assert.Equal(10, catalog.FineIdEffectbyIdItem(1000));
        Assert.Equal(10, catalog.FineIdEffectbyIdItem(1004));
        Assert.Equal(-1, catalog.FineIdEffectbyIdItem(5));
    }

    [Fact]
    public async Task ListEffect_gives_each_item_of_the_owned_badges_once()
    {
        BadgesCatalog catalog = await LoadAsync();
        var player = new Player();
        player.DataBadges.Add(new BadgesData(11, 0, false));
        player.DataBadges.Add(new BadgesData(10, 0, false));
        player.DataBadges.Add(new BadgesData(10, 0, false));
        player.DataBadges.Add(new BadgesData(99, 0, false));

        Assert.Equal([1001, 1000, 1004], catalog.ListEffect(player));
        Assert.Empty(catalog.ListEffect(new Player()));
    }

    [Fact]
    public async Task SendListItemOption_returns_the_options_of_the_worn_badge_and_the_last_template_wins()
    {
        BadgesCatalog catalog = await LoadAsync();
        var player = new Player();
        player.DataBadges.Add(new BadgesData(10, 0, true));

        // Effect 10 is on two templates (A with two options, then "Trùng hiệu ứng" with none): the last match is the answer.
        Assert.Empty(catalog.SendListItemOption(player));

        var other = new Player();
        other.DataBadges.Add(new BadgesData(11, 0, true));
        Assert.Equal([7], catalog.SendListItemOption(other).Select(o => o.Param));
    }

    [Fact]
    public async Task SendListItemOption_is_empty_when_no_owned_badge_is_in_use()
    {
        BadgesCatalog catalog = await LoadAsync();
        var player = new Player();
        player.DataBadges.Add(new BadgesData(11, 0, false));

        Assert.Empty(catalog.SendListItemOption(player));
    }

    [Fact]
    public async Task The_returned_option_list_cannot_change_the_template()
    {
        BadgesCatalog catalog = await LoadAsync();
        var player = new Player();
        player.DataBadges.Add(new BadgesData(11, 0, true));

        IReadOnlyList<ItemOption> options = catalog.SendListItemOption(player);

        Assert.False(options is List<ItemOption>);
        Assert.Single(catalog.BagesTemplates[1].Options);
    }

    [Fact]
    public async Task Arguments_are_checked()
    {
        var catalog = new BadgesCatalog();

        await Assert.ThrowsAsync<ArgumentNullException>(() => catalog.LoadAsync(null!, Items()));
        await Assert.ThrowsAsync<ArgumentNullException>(() => catalog.LoadAsync(Db(), null!));
        Assert.Throws<ArgumentNullException>(() => catalog.ListEffect(null!));
    }
}
