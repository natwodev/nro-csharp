using Nro.Core.Consts;
using Nro.Core.IO;
using Nro.Core.Templates;
using Nro.Game;
using Nro.Tests.Unit.Game.Support;

namespace Nro.Tests.Unit.Game.Progress.Achievements;

internal sealed class FixedMagicTree : IAchievementProgressSource
{
    public int Level { get; set; }

    public int GetMagicTreeLevel(Player player) => Level;
}

public class AchievementTests
{
    private static Player WithQuests(int count)
    {
        var player = new Player();
        for (int i = 0; i < count; i++)
        {
            player.Achievement.Add(new AchievementQuest(0, false));
        }

        return player;
    }

    private static async Task<AchievementCatalog> CatalogWithAsync(params (long Max, int Money)[] rows)
    {
        var catalog = new AchievementCatalog();
        var db = new FakeSqlExecutor().Table("from achievement_template", AchievementCatalogTests.Columns,
            rows.Select((r, i) => new object?[] { i, "info1-" + i, "info2-" + i, r.Money, r.Max }).ToArray());
        await catalog.LoadAsync(db);
        return catalog;
    }

    [Fact]
    public void Get_returns_null_outside_the_list()
    {
        Player player = WithQuests(2);

        Assert.NotNull(player.Achievement.Get(0));
        Assert.NotNull(player.Achievement.Get(1));
        Assert.Null(player.Achievement.Get(2));
        Assert.Null(player.Achievement.Get(-1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(16)]
    public void GetCompleted_of_the_power_achievements_reads_and_stores_the_live_power(int index)
    {
        Player player = WithQuests(20);
        player.NPoint.Power = 123_456_789_012L;

        long completed = player.Achievement.GetCompleted(index);

        Assert.Equal(123_456_789_012L, completed);
        Assert.Equal(123_456_789_012L, player.Achievement.Get(index)!.Completed);
    }

    [Fact]
    public void GetCompleted_of_the_magic_tree_achievement_uses_the_progress_source_or_zero()
    {
        Player player = WithQuests(5);
        Assert.Equal(0, player.Achievement.GetCompleted(2));

        player.Achievement.ProgressSource = new FixedMagicTree { Level = 7 };
        Assert.Equal(7, player.Achievement.GetCompleted(2));
        Assert.Equal(7, player.Achievement.Get(2)!.Completed);
    }

    [Fact]
    public void GetCompleted_of_the_diligent_activity_achievement_is_in_whole_hours()
    {
        Player player = WithQuests(10);
        player.Achievement.AchievementList[ConstAchievement.DEDICATED_PLAYER] = new AchievementQuest(3 * 3_600_000L + 3_599_999L, false);

        Assert.Equal(3, player.Achievement.GetCompleted(ConstAchievement.DEDICATED_PLAYER));
        Assert.Equal(3 * 3_600_000L + 3_599_999L, player.Achievement.Get(ConstAchievement.DEDICATED_PLAYER)!.Completed);
    }

    [Fact]
    public void GetCompleted_of_other_achievements_is_the_stored_value_and_zero_when_missing()
    {
        Player player = WithQuests(5);
        player.Achievement.AchievementList[3] = new AchievementQuest(42, false);

        Assert.Equal(42, player.Achievement.GetCompleted(3));
        Assert.Equal(0, player.Achievement.GetCompleted(4));
        Assert.Equal(0, player.Achievement.GetCompleted(99));
        Assert.Equal(0, player.Achievement.GetCompleted(-5));
    }

    [Fact]
    public void IsFinish_is_true_for_a_received_quest_or_one_that_reached_the_amount()
    {
        Player player = WithQuests(5);
        player.Achievement.AchievementList[3] = new AchievementQuest(10, false);
        player.Achievement.AchievementList[4] = new AchievementQuest(0, true);

        Assert.True(player.Achievement.IsFinish(3, 10));
        Assert.False(player.Achievement.IsFinish(3, 11));
        Assert.True(player.Achievement.IsFinish(4, 1_000_000));
        Assert.False(player.Achievement.IsFinish(9, 0));
    }

    [Fact]
    public void IsRecieve_reads_the_flag()
    {
        Player player = WithQuests(2);
        player.Achievement.Reward(1);

        Assert.False(player.Achievement.IsRecieve(0));
        Assert.True(player.Achievement.IsRecieve(1));
        Assert.False(player.Achievement.IsRecieve(5));
    }

    [Fact]
    public async Task CanReward_needs_a_quest_that_is_not_received_and_has_reached_the_amount()
    {
        // Achievements 0, 1, 2 and 16 read live values (power, magic tree), so the plain ones 3, 4 and 5 are used here.
        AchievementCatalog catalog = await CatalogWithAsync((10, 1), (10, 1), (10, 1), (10, 1), (10, 1), (10, 1));
        Player player = WithQuests(6);
        player.Achievement.AchievementList[3] = new AchievementQuest(10, false);
        player.Achievement.AchievementList[4] = new AchievementQuest(9, false);
        player.Achievement.AchievementList[5] = new AchievementQuest(99, true);

        Assert.True(player.Achievement.CanReward(3, catalog));
        Assert.False(player.Achievement.CanReward(4, catalog));
        Assert.False(player.Achievement.CanReward(5, catalog));
    }

    [Fact]
    public async Task CanReward_reads_the_template_first_so_an_index_without_a_template_throws_even_without_a_quest()
    {
        AchievementCatalog catalog = await CatalogWithAsync((10, 1));
        Player player = WithQuests(1);

        Assert.Throws<ArgumentOutOfRangeException>(() => player.Achievement.CanReward(5, catalog));
        Assert.Throws<ArgumentOutOfRangeException>(() => player.Achievement.CanReward(-1, catalog));
    }

    [Fact]
    public void Done_adds_and_DoneNotAdd_sets_and_both_keep_the_received_flag()
    {
        Player player = WithQuests(3);
        player.Achievement.AchievementList[1] = new AchievementQuest(5, true);
        AchievementQuest before = player.Achievement.Get(1)!;

        player.Achievement.Done(1, 3);
        Assert.Equal(8, player.Achievement.Get(1)!.Completed);
        Assert.True(player.Achievement.Get(1)!.IsRecieve);
        Assert.NotSame(before, player.Achievement.Get(1));
        Assert.Equal(5, before.Completed);

        player.Achievement.DoneNotAdd(1, 2);
        Assert.Equal(2, player.Achievement.Get(1)!.Completed);
        Assert.True(player.Achievement.Get(1)!.IsRecieve);
    }

    [Fact]
    public void Done_DoneNotAdd_and_Reward_ignore_an_index_outside_the_list()
    {
        Player player = WithQuests(2);

        player.Achievement.Done(2, 1);
        player.Achievement.Done(-1, 1);
        player.Achievement.DoneNotAdd(9, 1);
        player.Achievement.Reward(-3);

        Assert.Equal(2, player.Achievement.AchievementList.Count);
        Assert.All(player.Achievement.AchievementList, q => Assert.Equal(0, q.Completed));
    }

    [Fact]
    public void Reward_marks_received_and_keeps_the_progress()
    {
        Player player = WithQuests(2);
        player.Achievement.Done(0, 7);

        player.Achievement.Reward(0);

        Assert.True(player.Achievement.IsRecieve(0));
        Assert.Equal(7, player.Achievement.Get(0)!.Completed);
    }

    [Fact]
    public void Two_players_have_independent_progress()
    {
        Player a = WithQuests(3);
        Player b = WithQuests(3);

        a.Achievement.Done(1, 5);
        a.Achievement.Reward(1);

        Assert.Equal(0, b.Achievement.Get(1)!.Completed);
        Assert.False(b.Achievement.IsRecieve(1));
    }

    [Fact]
    public void Dispose_empties_the_list()
    {
        Player player = WithQuests(3);

        player.Achievement.Dispose();

        Assert.Empty(player.Achievement.AchievementList);
        Assert.Same(player, player.Achievement.Player);
    }
}

public class AchievementCatalogTests
{
    internal static readonly string[] Columns = ["id", "info1", "info2", "money", "max_count"];

    [Fact]
    public async Task LoadAsync_reads_the_rows_by_id_with_the_java_types()
    {
        var db = new FakeSqlExecutor().Table("from achievement_template", Columns,
            new object?[] { 1, "Mạnh", "Đạt %1", 5, 20_000_000_000L },
            new object?[] { 2, "Cây", "Trồng", 7, 10L });
        var catalog = new AchievementCatalog();

        await catalog.LoadAsync(db);

        Assert.Equal(2, catalog.Templates.Count);
        Assert.Equal("Mạnh", catalog.GetTemplate(0).Info1);
        Assert.Equal("Đạt %1", catalog.GetTemplate(0).Info2);
        Assert.Equal(5, catalog.GetTemplate(0).Money);
        Assert.Equal(20_000_000_000L, catalog.GetTemplate(0).MaxCount);
        Assert.Contains("order by id", db.QueryLog.Single());
    }

    [Fact]
    public async Task GetTemplate_is_by_position_and_throws_outside_the_table()
    {
        var catalog = new AchievementCatalog();
        await catalog.LoadAsync(new FakeSqlExecutor().Table("from achievement_template", Columns, new object?[] { 1, "a", "b", 1, 1L }));

        Assert.Equal("a", catalog.GetTemplate(0).Info1);
        Assert.Throws<ArgumentOutOfRangeException>(() => catalog.GetTemplate(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => catalog.GetTemplate(-1));
    }

    [Fact]
    public async Task A_failing_load_keeps_the_previous_templates()
    {
        var catalog = new AchievementCatalog();
        await catalog.LoadAsync(new FakeSqlExecutor().Table("from achievement_template", Columns, new object?[] { 1, "a", "b", 1, 1L }));
        var failing = new FakeSqlExecutor { FailQueryContaining = ("achievement_template", new InvalidOperationException("db down")) };

        await Assert.ThrowsAsync<InvalidOperationException>(() => catalog.LoadAsync(failing));

        Assert.Single(catalog.Templates);
    }
}

public class AchievementServiceTests
{
    private static async Task<(AchievementService Service, FakeAchievementHost Host, AchievementCatalog Catalog, Player Player)> MakeAsync()
    {
        var catalog = new AchievementCatalog();
        var db = new FakeSqlExecutor().Table("from achievement_template", AchievementCatalogTests.Columns,
            new object?[] { 1, "Sức mạnh", "Đạt sức mạnh %1", 5, 20_000L },
            new object?[] { 2, "Sức mạnh 2", "Đạt %1 hơn", 6, 30_000L },
            new object?[] { 3, "Cây", "Cây", 1, 5L },
            new object?[] { 4, "Bạn thân", "Gặp %2", 7, 100L });
        await catalog.LoadAsync(db);
        var host = new FakeAchievementHost();
        var player = new Player();
        for (int i = 0; i < 20; i++)
        {
            player.Achievement.Add(new AchievementQuest(0, false));
        }

        return (new AchievementService(catalog, host), host, catalog, player);
    }

    [Fact]
    public async Task OpenAchievementUI_writes_every_template_with_progress_and_flags()
    {
        (AchievementService service, FakeAchievementHost host, _, Player player) = await MakeAsync();
        player.Gender = 1;
        player.NPoint.Power = 50_000;
        player.Achievement.AchievementList[3] = new AchievementQuest(100, true);
        player.Achievement.ProgressSource = new FixedMagicTree { Level = 2 };

        service.OpenAchievementUI(player);

        byte[] data = Assert.Single(host.MessageData);
        Assert.Equal([-76], host.Commands);
        var ui = new Message((sbyte)-76, data);
        Assert.Equal(0, ui.ReadByte());
        Assert.Equal(4, ui.ReadByte());
        Assert.Equal("Sức mạnh", ui.ReadUTF());
        Assert.Equal("Đạt sức mạnh Siêu Namếc (50 Nghìn/20 Nghìn)", ui.ReadUTF());
        Assert.Equal(5, ui.ReadShort());
        Assert.True(ui.ReadBoolean());
        Assert.False(ui.ReadBoolean());
        Assert.Equal("Sức mạnh 2", ui.ReadUTF());
        Assert.Equal("Đạt Siêu Namếc hơn (50 Nghìn/30 Nghìn)", ui.ReadUTF());
        Assert.Equal(6, ui.ReadShort());
        Assert.True(ui.ReadBoolean());
        Assert.False(ui.ReadBoolean());
        Assert.Equal("Cây", ui.ReadUTF());
        Assert.Equal("Cây (2/5)", ui.ReadUTF());
        Assert.Equal(1, ui.ReadShort());
        Assert.False(ui.ReadBoolean());
        Assert.False(ui.ReadBoolean());
        Assert.Equal("Bạn thân", ui.ReadUTF());
        Assert.Equal("Gặp Dende (100/100)", ui.ReadUTF());
        Assert.Equal(7, ui.ReadShort());
        Assert.True(ui.ReadBoolean());
        Assert.True(ui.ReadBoolean());
    }

    [Fact]
    public async Task ConfirmAchievement_pays_the_reward_marks_it_and_answers_the_client()
    {
        (AchievementService service, FakeAchievementHost host, _, Player player) = await MakeAsync();
        player.Achievement.AchievementList[3] = new AchievementQuest(100, false);

        service.ConfirmAchievement(player, 3);

        Assert.True(player.Achievement.IsRecieve(3));
        Assert.Equal(7, host.RubyAdded);
        Item item = Assert.Single(host.Bag);
        Assert.Equal(457, item.Template!.Id);
        Assert.Equal(7, item.Quantity);
        Assert.Equal(1, host.ItemBagSent);
        Assert.Equal(1, host.MoneySent);
        Assert.Equal(["Bạn vừa nhận được 7 hồng ngọc", "Bạn vừa nhận được 7 thỏi vàng"], host.Notifications);
        byte[] data = host.MessageData.Last();
        var answer = new Message((sbyte)-76, data);
        Assert.Equal(1, answer.ReadByte());
        Assert.Equal(3, answer.ReadByte());
    }

    [Fact]
    public async Task ConfirmAchievement_is_refused_when_the_account_protection_is_on()
    {
        (AchievementService service, FakeAchievementHost host, _, Player player) = await MakeAsync();
        player.Baovetaikhoan = true;
        player.Achievement.AchievementList[3] = new AchievementQuest(100, false);

        service.ConfirmAchievement(player, 3);

        Assert.Equal(["Chức năng bảo vệ đã được bật. Bạn vui lòng kiểm tra lại"], host.Notifications);
        Assert.False(player.Achievement.IsRecieve(3));
        Assert.Empty(host.Commands);
    }

    [Fact]
    public async Task ConfirmAchievement_is_refused_when_the_quest_cannot_be_rewarded()
    {
        (AchievementService service, FakeAchievementHost host, _, Player player) = await MakeAsync();
        player.Achievement.AchievementList[3] = new AchievementQuest(99, false);

        service.ConfirmAchievement(player, 3);

        Assert.Equal(["Không thể thực hiện"], host.Notifications);
        Assert.Equal(0, host.RubyAdded);
        Assert.Empty(host.Bag);
    }

    [Fact]
    public async Task ConfirmAchievement_needs_an_empty_bag_slot_and_pays_nothing_without_one()
    {
        (AchievementService service, FakeAchievementHost host, _, Player player) = await MakeAsync();
        host.EmptyBagSlots = 0;
        player.Achievement.AchievementList[3] = new AchievementQuest(100, false);

        service.ConfirmAchievement(player, 3);

        Assert.Equal(["Cần tối thiểu 1 ô trống hành trang để nhận thưởng"], host.Notifications);
        Assert.False(player.Achievement.IsRecieve(3));
        Assert.Equal(0, host.RubyAdded);
        Assert.Empty(host.Commands);
    }

    [Fact]
    public async Task ConfirmAchievement_cannot_be_used_twice_for_the_same_achievement()
    {
        (AchievementService service, FakeAchievementHost host, _, Player player) = await MakeAsync();
        player.Achievement.AchievementList[3] = new AchievementQuest(100, false);

        service.ConfirmAchievement(player, 3);
        service.ConfirmAchievement(player, 3);

        Assert.Equal(7, host.RubyAdded);
        Assert.Contains("Không thể thực hiện", host.Notifications);
    }

    [Fact]
    public async Task ConfirmAchievement_with_an_index_outside_the_template_table_throws_like_java()
    {
        (AchievementService service, _, _, Player player) = await MakeAsync();

        Assert.Throws<ArgumentOutOfRangeException>(() => service.ConfirmAchievement(player, 9));
    }

    [Theory]
    [InlineData(10000, "10000")]
    [InlineData(10001, "10 Nghìn")]
    [InlineData(-5, "-5")]
    public async Task NumberToString_switches_to_the_vietnamese_format_above_10000(long number, string expected)
    {
        (AchievementService service, _, _, _) = await MakeAsync();

        Assert.Equal(expected, service.NumberToString(number));
    }

    [Theory]
    [InlineData(0, "Siêu nhân và Bunma")]
    [InlineData(1, "Siêu Namếc và Dende")]
    [InlineData(2, "Siêu Xayda và Appule")]
    [InlineData(5, "Siêu Xayda và Appule")]
    [InlineData(-1, "Siêu Xayda và Appule")]
    public async Task Regex_names_the_race(int gender, string expected)
    {
        (AchievementService service, _, _, Player player) = await MakeAsync();
        player.Gender = (sbyte)gender;

        Assert.Equal(expected, service.Regex(player, "%1 và %2"));
    }

    [Fact]
    public async Task CheckDoneTask_adds_one_for_a_real_player_and_nothing_for_others()
    {
        (AchievementService service, FakeAchievementHost host, _, Player player) = await MakeAsync();

        service.CheckDoneTask(player, 3);
        service.CheckDoneTask(player, 3);
        host.Pl = false;
        service.CheckDoneTask(player, 3);

        Assert.Equal(2, player.Achievement.Get(3)!.Completed);
    }

    [Fact]
    public async Task CheckDoneTask_for_the_first_top_up_sets_the_paid_amount_instead_of_adding()
    {
        (AchievementService service, FakeAchievementHost host, _, Player player) = await MakeAsync();
        host.PaidAmount = 150_000;

        service.CheckDoneTask(player, ConstAchievement.FIRST_GEM_TOP_UP);
        service.CheckDoneTask(player, ConstAchievement.FIRST_GEM_TOP_UP);

        Assert.Equal(150_000, player.Achievement.Get(ConstAchievement.FIRST_GEM_TOP_UP)!.Completed);
    }

    [Fact]
    public async Task CheckDoneTaskKillMob_counts_super_monsters_hunters_prey_and_training_dummies()
    {
        (AchievementService service, _, _, Player player) = await MakeAsync();

        service.CheckDoneTaskKillMob(player, 1, 0, 5);
        service.CheckDoneTaskKillMob(player, 0, 4, 5);
        service.CheckDoneTaskKillMob(player, 0, 0, 0);
        service.CheckDoneTaskKillMob(player, 0, 1, 7);
        service.CheckDoneTaskKillMob(player, 3, 4, 0);

        Assert.Equal(2, player.Achievement.Get(ConstAchievement.SUPER_MONSTER_SLAYER)!.Completed);
        Assert.Equal(2, player.Achievement.Get(ConstAchievement.SHARPSHOOTER)!.Completed);
        Assert.Equal(2, player.Achievement.Get(ConstAchievement.METHODICAL_TRAINING)!.Completed);
    }

    [Theory]
    [InlineData(Skill.KAMEJOKO, ConstAchievement.KI_BLAST_MASTER)]
    [InlineData(Skill.MASENKO, ConstAchievement.KI_BLAST_MASTER)]
    [InlineData(Skill.ANTOMIC, ConstAchievement.KI_BLAST_MASTER)]
    [InlineData(Skill.TU_SAT, ConstAchievement.SKILL_MASTER)]
    [InlineData(Skill.MA_PHONG_BA, ConstAchievement.SKILL_MASTER)]
    [InlineData(Skill.THAI_DUONG_HA_SAN, ConstAchievement.SKILL_MASTER)]
    public async Task CheckDoneTaskUseSkill_counts_the_group_of_the_skill(int skillId, int achievement)
    {
        (AchievementService service, _, _, Player player) = await MakeAsync();
        player.PlayerSkill.SkillSelect = new Skill { Template = new SkillTemplate { Id = (sbyte)skillId } };

        service.CheckDoneTaskUseSkill(player);

        Assert.Equal(1, player.Achievement.Get(achievement)!.Completed);
        Assert.Equal(1, player.Achievement.AchievementList.Sum(q => q.Completed));
    }

    [Theory]
    [InlineData(Skill.DRAGON)]
    [InlineData(Skill.DEMON)]
    [InlineData(Skill.GALICK)]
    [InlineData(Skill.LIEN_HOAN)]
    [InlineData(Skill.KAIOKEN)]
    [InlineData(Skill.DICH_CHUYEN_TUC_THOI)]
    public async Task CheckDoneTaskUseSkill_ignores_the_basic_skills(int skillId)
    {
        (AchievementService service, _, _, Player player) = await MakeAsync();
        player.PlayerSkill.SkillSelect = new Skill { Template = new SkillTemplate { Id = (sbyte)skillId } };

        service.CheckDoneTaskUseSkill(player);

        Assert.Equal(0, player.Achievement.AchievementList.Sum(q => q.Completed));
    }

    [Fact]
    public async Task CheckDoneTaskUseSkill_ignores_non_players_and_fails_without_a_selected_skill_for_real_ones()
    {
        (AchievementService service, FakeAchievementHost host, _, Player player) = await MakeAsync();
        host.Pl = false;
        service.CheckDoneTaskUseSkill(player);
        Assert.Equal(0, player.Achievement.AchievementList.Sum(q => q.Completed));

        host.Pl = true;
        Assert.Throws<InvalidOperationException>(() => service.CheckDoneTaskUseSkill(player));
    }

    [Theory]
    [InlineData(95, 9)]
    [InlineData(-95, 9)]
    [InlineData(99, 9)]
    [InlineData(5, 0)]
    [InlineData(0, 0)]
    [InlineData(100, -1)]
    [InlineData(-100, -1)]
    [InlineData(10000, -1)]
    public async Task CheckDoneTaskFly_adds_a_tenth_of_the_distance_only_for_short_flights(int length, int expectedAdded)
    {
        (AchievementService service, _, _, Player player) = await MakeAsync();

        service.CheckDoneTaskFly(player, length);

        Assert.Equal(expectedAdded < 0 ? 0 : expectedAdded, player.Achievement.Get(ConstAchievement.SKILLED_FLYER)!.Completed);
    }
}
