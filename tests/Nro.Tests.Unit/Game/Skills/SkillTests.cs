using Nro.Game;
using Nro.Tests.Unit.Game.Support;

namespace Nro.Tests.Unit.Game.Skills;

public class SkillTests
{
    private static Skill Full(SkillTemplate template) => new()
    {
        Template = template,
        SkillId = 7,
        Point = 3,
        PowRequire = 1_000_000_000_000L,
        CoolDown = 2500,
        LastTimeUseThisSkill = 1_700_000_000_000L,
        LastTimeUseThisSkillbot = 1_700_000_000_001L,
        Dx = 120,
        Dy = 80,
        MaxFight = 4,
        ManaUse = 50,
        Damage = 300,
        CurrLevel = 500,
        MoreInfo = "info",
        Price = 25,
    };

    [Fact]
    public void Constants_have_the_values_of_the_java_class()
    {
        Assert.Equal(0, Skill.DRAGON);
        Assert.Equal(1, Skill.KAMEJOKO);
        Assert.Equal(2, Skill.DEMON);
        Assert.Equal(9, Skill.KAIOKEN);
        Assert.Equal(14, Skill.TU_SAT);
        Assert.Equal(20, Skill.DICH_CHUYEN_TUC_THOI);
        Assert.Equal(24, Skill.SUPER_KAME);
        Assert.Equal(26, Skill.MA_PHONG_BA);
        Assert.Equal(100, Skill.RANGE_ATTACK_CHIEU_DAM);
        Assert.Equal(300, Skill.RANGE_ATTACK_CHIEU_CHUONG);
    }

    [Fact]
    public void A_new_skill_has_java_defaults()
    {
        var skill = new Skill();

        Assert.Null(skill.Template);
        Assert.Equal(0, skill.SkillId);
        Assert.Equal(0, skill.CurrLevel);
        Assert.Null(skill.MoreInfo);
    }

    [Fact]
    public void Copy_keeps_the_data_and_shares_the_template()
    {
        var template = new SkillTemplate { Id = 5, Name = "Antomic" };
        Skill original = Full(template);

        var copy = new Skill(original);

        Assert.Equal(7, copy.SkillId);
        Assert.Equal(3, copy.Point);
        Assert.Equal(1_000_000_000_000L, copy.PowRequire);
        Assert.Equal(2500, copy.CoolDown);
        Assert.Equal(1_700_000_000_000L, copy.LastTimeUseThisSkill);
        Assert.Equal(120, copy.Dx);
        Assert.Equal(80, copy.Dy);
        Assert.Equal(4, copy.MaxFight);
        Assert.Equal(50, copy.ManaUse);
        Assert.Equal(300, copy.Damage);
        Assert.Equal("info", copy.MoreInfo);
        Assert.Equal(25, copy.Price);
        Assert.Same(template, copy.Template);
    }

    [Fact]
    public void Copy_does_not_carry_the_level_or_the_bot_timestamp_a_java_quirk()
    {
        Skill original = Full(new SkillTemplate { Id = 5 });

        var copy = new Skill(original);

        Assert.Equal(0, copy.CurrLevel);
        Assert.Equal(0, copy.LastTimeUseThisSkillbot);
        Assert.Equal(500, original.CurrLevel);
    }

    [Fact]
    public void Changing_a_copy_never_changes_the_original_and_the_other_way_round()
    {
        Skill original = Full(new SkillTemplate { Id = 5 });
        var copy = new Skill(original);

        copy.CurrLevel = 900;
        copy.LastTimeUseThisSkill = 5;
        copy.Damage = 1;
        original.Point = 9;

        Assert.Equal(500, original.CurrLevel);
        Assert.Equal(1_700_000_000_000L, original.LastTimeUseThisSkill);
        Assert.Equal(300, original.Damage);
        Assert.Equal(3, copy.Point);
    }

    [Fact]
    public void Two_players_get_independent_levels_from_the_same_template_skill()
    {
        var template = new SkillTemplate { Id = 5 };
        Skill level = Full(template);
        template.Skillss.Add(level);

        var playerA = new Skill(template.Skillss[0]);
        var playerB = new Skill(template.Skillss[0]);
        playerA.CurrLevel = 10;
        playerB.CurrLevel = 20;

        Assert.Equal(10, playerA.CurrLevel);
        Assert.Equal(20, playerB.CurrLevel);
        Assert.Equal(500, template.Skillss[0].CurrLevel);
    }

    [Fact]
    public void Copy_of_null_is_an_error()
    {
        Assert.Throws<ArgumentNullException>(() => new Skill(null!));
    }

    [Fact]
    public void Dispose_drops_only_the_template_reference_of_that_skill()
    {
        var template = new SkillTemplate { Id = 5 };
        Skill original = Full(template);
        var copy = new Skill(original);

        copy.Dispose();

        Assert.Null(copy.Template);
        Assert.Same(template, original.Template);
        Assert.Equal(7, copy.SkillId);
    }
}

public class SkillTemplateAndNClassTests
{
    private static NClass Earth() => new()
    {
        ClassId = 0,
        Name = "Trái Đất",
        SkillTemplatess =
        [
            new SkillTemplate { Id = 0, Name = "Chiêu đấm Dragon" },
            new SkillTemplate { Id = 1, Name = "Kamejoko" },
            new SkillTemplate { Id = 9, Name = "Biến Khỉ" },
            new SkillTemplate { Id = 10, Name = "Quả cầu kênh khí" },
            new SkillTemplate { Id = 10, Name = "Duplicate id" },
        ],
    };

    [Fact]
    public void Template_defaults()
    {
        var template = new SkillTemplate();

        Assert.Equal("", template.Name);
        Assert.Empty(template.Skillss);
        Assert.Null(template.Skills);
        Assert.Null(template.Description);
        Assert.Null(template.DamInfo);
    }

    [Fact]
    public void GetSkillTemplate_returns_the_first_template_with_that_id()
    {
        NClass nclass = Earth();

        Assert.Equal("Kamejoko", nclass.GetSkillTemplate(1)!.Name);
        Assert.Equal("Quả cầu kênh khí", nclass.GetSkillTemplate(10)!.Name);
    }

    [Fact]
    public void GetSkillTemplate_returns_null_for_an_unknown_id()
    {
        Assert.Null(Earth().GetSkillTemplate(99));
        Assert.Null(new NClass().GetSkillTemplate(0));
    }

    [Theory]
    [InlineData("kame", "Kamejoko")]
    [InlineData("KAME", "Kamejoko")]
    [InlineData("bien khi", "Biến Khỉ")]
    [InlineData("BIẾN", "Biến Khỉ")]
    [InlineData("qua cau", "Quả cầu kênh khí")]
    [InlineData("dam", "Chiêu đấm Dragon")]
    [InlineData("", "Chiêu đấm Dragon")]
    public void GetSkillTemplateByName_ignores_accents_and_case_and_matches_a_part_of_the_name(string query, string expected)
    {
        Assert.Equal(expected, Earth().GetSkillTemplateByName(query)!.Name);
    }

    [Fact]
    public void GetSkillTemplateByName_returns_null_when_nothing_matches()
    {
        Assert.Null(Earth().GetSkillTemplateByName("xyz"));
    }

    [Fact]
    public void GetSkillTemplateByName_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => Earth().GetSkillTemplateByName(null!));
    }
}

public class SkillDamageConfigTests
{
    [Fact]
    public void Initial_table_matches_the_java_static_initializer()
    {
        var config = new SkillDamageConfig();

        Assert.Equal(1.0, config.GetMultiplier(Skill.DRAGON));
        Assert.Equal(0.8, config.GetMultiplier(Skill.KAMEJOKO));
        Assert.Equal(1.2, config.GetMultiplier(Skill.KAIOKEN));
        Assert.Equal(0.8, config.GetMultiplier(Skill.DICH_CHUYEN_TUC_THOI));
        Assert.Equal(1.2, config.GetMultiplier(Skill.GALICK));
        Assert.Equal(1.5, config.GetMultiplier(Skill.ANTOMIC));
        Assert.Equal(1.0, config.GetMultiplier(Skill.DEMON));
        Assert.Equal(1.0, config.GetMultiplier(Skill.MASENKO));
        Assert.Equal(1.1, config.GetMultiplier(Skill.LIEN_HOAN));
        Assert.Equal(3.0, config.GetMultiplier(Skill.TU_SAT));
        Assert.Equal(2.5, config.GetMultiplier(Skill.QUA_CAU_KENH_KHI));
        Assert.Equal(3.0, config.GetMultiplier(Skill.MAKANKOSAPPO));
    }

    [Fact]
    public void A_skill_without_an_entry_has_factor_one()
    {
        var config = new SkillDamageConfig();

        Assert.Equal(1.0, config.GetMultiplier(Skill.MA_PHONG_BA));
        Assert.Equal(1.0, config.GetMultiplier(-1));
        Assert.Equal(1.0, config.GetMultiplier(127));
    }

    [Fact]
    public void SetMultiplier_adds_and_replaces_without_range_checks()
    {
        var config = new SkillDamageConfig();

        config.SetMultiplier(Skill.MA_PHONG_BA, 4.5);
        config.SetMultiplier(Skill.KAMEJOKO, -2.0);
        config.SetMultiplier(Skill.DRAGON, 0.0);

        Assert.Equal(4.5, config.GetMultiplier(Skill.MA_PHONG_BA));
        Assert.Equal(-2.0, config.GetMultiplier(Skill.KAMEJOKO));
        Assert.Equal(0.0, config.GetMultiplier(Skill.DRAGON));
    }

    [Fact]
    public void Two_instances_do_not_share_changes_unlike_the_java_static_map()
    {
        var first = new SkillDamageConfig();
        var second = new SkillDamageConfig();

        first.SetMultiplier(Skill.GALICK, 9.0);

        Assert.Equal(9.0, first.GetMultiplier(Skill.GALICK));
        Assert.Equal(1.2, second.GetMultiplier(Skill.GALICK));
    }

    [Fact]
    public async Task Changing_and_reading_from_many_threads_is_safe()
    {
        var config = new SkillDamageConfig();

        await Task.WhenAll(Enumerable.Range(0, 8).Select(worker => Task.Run(() =>
        {
            for (int i = 0; i < 2000; i++)
            {
                config.SetMultiplier(unchecked((sbyte)(i % 100)), worker);
                _ = config.GetMultiplier(unchecked((sbyte)((i * 7) % 100)));
            }
        })));

        Assert.InRange(config.GetMultiplier(5), 0.0, 7.0);
    }
}

public class PlayerSkillTests
{
    private static Skill WithTemplate(sbyte templateId, short skillId = 0) => new() { Template = new SkillTemplate { Id = templateId }, SkillId = skillId };

    [Fact]
    public void Bare_object_matches_the_no_argument_java_constructor()
    {
        var skills = new PlayerSkill();

        Assert.Null(skills.Player);
        Assert.Null(skills.Skills);
        Assert.Equal(-1, skills.Time);
        Assert.Equal(-1, skills.ItemTemplateSkillId);
        Assert.Equal(0, skills.Potential);
        Assert.Equal(10, skills.SkillShortCut.Length);
    }

    [Fact]
    public void Player_object_has_an_owner_and_an_empty_list_and_keeps_java_zero_defaults()
    {
        var player = new Player();

        Assert.Same(player, player.PlayerSkill.Player);
        Assert.NotNull(player.PlayerSkill.Skills);
        Assert.Empty(player.PlayerSkill.Skills!);
        Assert.Equal(0, player.PlayerSkill.Time);
        Assert.Equal(0, player.PlayerSkill.ItemTemplateSkillId);
        Assert.Equal(-1, player.LearnSkill.Time);
    }

    [Fact]
    public void GetSkillbyId_finds_by_template_id_and_returns_null_when_missing()
    {
        var player = new Player();
        player.PlayerSkill.Skills!.Add(WithTemplate(3));
        player.PlayerSkill.Skills.Add(WithTemplate(9));
        player.PlayerSkill.Skills.Add(WithTemplate(9));

        Assert.Same(player.PlayerSkill.Skills[1], player.PlayerSkill.GetSkillbyId(9));
        Assert.Same(player.PlayerSkill.Skills[0], player.PlayerSkill.GetSkillbyId(3));
        Assert.Null(player.PlayerSkill.GetSkillbyId(4));
    }

    [Fact]
    public void GetSkillbyId_skips_a_disposed_skill_instead_of_failing()
    {
        var player = new Player();
        Skill disposed = WithTemplate(3);
        disposed.Dispose();
        player.PlayerSkill.Skills!.Add(disposed);
        player.PlayerSkill.Skills.Add(WithTemplate(3));

        Assert.Same(player.PlayerSkill.Skills[1], player.PlayerSkill.GetSkillbyId(3));
    }

    [Fact]
    public void GetSkillbyId_without_a_list_is_an_error()
    {
        Assert.Throws<InvalidOperationException>(() => new PlayerSkill().GetSkillbyId(1));
    }

    [Fact]
    public void GetSizeSkill_counts_skills_whose_id_is_not_minus_one()
    {
        var player = new Player();
        player.PlayerSkill.Skills!.Add(WithTemplate(0, 1));
        player.PlayerSkill.Skills.Add(WithTemplate(1, -1));
        player.PlayerSkill.Skills.Add(WithTemplate(2, 0));
        player.PlayerSkill.Skills.Add(WithTemplate(3, -1));

        Assert.Equal(2, player.PlayerSkill.GetSizeSkill());
    }

    [Fact]
    public void GetSizeSkill_wraps_like_a_java_byte_after_127()
    {
        var player = new Player();
        for (int i = 0; i < 130; i++)
        {
            player.PlayerSkill.Skills!.Add(WithTemplate(0, 1));
        }

        Assert.Equal(-126, player.PlayerSkill.GetSizeSkill());
    }

    [Fact]
    public void GetSizeSkill_of_an_empty_list_is_zero_and_of_no_list_an_error()
    {
        Assert.Equal(0, new Player().PlayerSkill.GetSizeSkill());
        Assert.Throws<InvalidOperationException>(() => new PlayerSkill().GetSizeSkill());
    }

    [Theory]
    [InlineData(Skill.DRAGON, 1)]
    [InlineData(Skill.DEMON, 1)]
    [InlineData(Skill.GALICK, 1)]
    [InlineData(Skill.KAIOKEN, 1)]
    [InlineData(Skill.LIEN_HOAN, 1)]
    [InlineData(Skill.KAMEJOKO, 2)]
    [InlineData(Skill.ANTOMIC, 2)]
    [InlineData(Skill.MASENKO, 2)]
    [InlineData(Skill.LIEN_HOAN_CHUONG, 4)]
    [InlineData(Skill.SUPER_KAME, 4)]
    [InlineData(Skill.MA_PHONG_BA, 4)]
    [InlineData(Skill.TU_SAT, 3)]
    [InlineData(Skill.THAI_DUONG_HA_SAN, 3)]
    [InlineData(Skill.QUA_CAU_KENH_KHI, 3)]
    [InlineData(-5, 3)]
    public void GetIndexSkillSelect_groups_the_skills(int templateId, int expected)
    {
        var player = new Player();
        player.PlayerSkill.SkillSelect = WithTemplate((sbyte)templateId);

        Assert.Equal(expected, player.PlayerSkill.GetIndexSkillSelect());
    }

    [Fact]
    public void GetIndexSkillSelect_without_a_selection_or_template_is_an_error()
    {
        var player = new Player();
        Assert.Throws<InvalidOperationException>(() => player.PlayerSkill.GetIndexSkillSelect());

        player.PlayerSkill.SkillSelect = new Skill();
        Assert.Throws<InvalidOperationException>(() => player.PlayerSkill.GetIndexSkillSelect());
    }

    [Fact]
    public void Short_cut_messages_are_two_sub_command_messages_with_the_raw_bytes()
    {
        var skills = new PlayerSkill();
        skills.SkillShortCut[0] = 1;
        skills.SkillShortCut[1] = -1;
        skills.SkillShortCut[9] = 26;

        var messages = skills.BuildSkillShortCutMessages();

        Assert.Equal(2, messages.Count);
        byte[] tail = [1, 255, 0, 0, 0, 0, 0, 0, 0, 26];
        foreach ((string kind, int index) in new[] { ("KSkill", 0), ("OSkill", 1) })
        {
            Assert.Equal(-30, messages[index].Command);
            byte[] expected = [61, 0, 6, .. System.Text.Encoding.ASCII.GetBytes(kind), 0, 0, 0, 10, .. tail];
            Assert.Equal(expected, messages[index].GetData());
        }
    }

    [Fact]
    public void SendSkillShortCut_gives_both_messages_to_the_sender_in_order()
    {
        var skills = new PlayerSkill();
        var sent = new List<byte[]>();

        skills.SendSkillShortCut(m => sent.Add(m.GetData()));

        Assert.Equal(2, sent.Count);
        Assert.Contains((byte)'K', sent[0]);
        Assert.Contains((byte)'O', sent[1]);
    }

    [Fact]
    public void SendSkillShortCut_does_not_swallow_a_failing_sender()
    {
        var skills = new PlayerSkill();

        Assert.Throws<InvalidOperationException>(() => skills.SendSkillShortCut(_ => throw new InvalidOperationException("send failed")));
    }

    [Fact]
    public void SendSkillShortCut_requires_a_sender()
    {
        Assert.Throws<ArgumentNullException>(() => new PlayerSkill().SendSkillShortCut(null!));
    }

    [Fact]
    public void Dispose_clears_everything_and_disposes_the_skills()
    {
        var player = new Player();
        Skill first = WithTemplate(3);
        Skill second = WithTemplate(4);
        player.PlayerSkill.Skills!.Add(first);
        player.PlayerSkill.Skills.Add(second);
        player.PlayerSkill.SkillSelect = second;

        player.PlayerSkill.Dispose();

        Assert.Null(first.Template);
        Assert.Null(second.Template);
        Assert.Null(player.PlayerSkill.Skills);
        Assert.Null(player.PlayerSkill.SkillSelect);
        Assert.Null(player.PlayerSkill.Player);
        player.PlayerSkill.Dispose();
    }
}

public class NewSkillTests
{
    private static (Player Player, FakeSkillSpecialHost Host) Setup(sbyte templateId = Skill.SUPER_KAME, int point = 2, short currLevel = 0)
    {
        var player = new Player();
        player.PlayerSkill.SkillSelect = new Skill { Template = new SkillTemplate { Id = templateId }, Point = point, CurrLevel = currLevel };
        return (player, new FakeSkillSpecialHost());
    }

    [Fact]
    public void Constants_and_initial_state()
    {
        var player = new Player();

        Assert.Equal(2000, NewSkill.TIME_GONG);
        Assert.Equal(3000, NewSkill.TIME_END_24_25);
        Assert.Equal(11000, NewSkill.TIME_END_26);
        Assert.Same(player, player.NewSkill.Player);
        Assert.Empty(player.NewSkill.PlayersTaget);
        Assert.False(player.NewSkill.IsActive);
        Assert.False(player.NewSkill.IsStartSkillSpecial);
    }

    [Fact]
    public void SetSkillSpecial_raises_the_level_of_a_real_player_and_tells_the_client()
    {
        (Player player, FakeSkillSpecialHost host) = Setup(currLevel: 10);

        player.NewSkill.SetSkillSpecial(host, 1000, 1, 0, 0, 500, 0);

        Assert.Equal(11, player.PlayerSkill.SkillSelect!.CurrLevel);
        Assert.Equal([(short)11], host.LevelsSent);
        Assert.Same(player.PlayerSkill.SkillSelect, player.NewSkill.SkillSelect);
    }

    [Fact]
    public void SetSkillSpecial_stops_raising_the_level_at_1000()
    {
        (Player player, FakeSkillSpecialHost host) = Setup(currLevel: 1000);

        player.NewSkill.SetSkillSpecial(host, 1000, 1, 0, 0, 500, 0);

        Assert.Equal(1000, player.PlayerSkill.SkillSelect!.CurrLevel);
        Assert.Empty(host.LevelsSent);
    }

    [Fact]
    public void SetSkillSpecial_does_not_raise_the_level_of_a_non_player()
    {
        (Player player, FakeSkillSpecialHost host) = Setup(currLevel: 5);
        host.Pl = false;

        player.NewSkill.SetSkillSpecial(host, 1000, 1, 0, 0, 500, 0);

        Assert.Equal(5, player.PlayerSkill.SkillSelect!.CurrLevel);
        Assert.Empty(host.LevelsSent);
    }

    [Theory]
    [InlineData(false, 0)]
    [InlineData(true, 2)]
    public void SetSkillSpecial_sets_the_item_type_from_the_special_item_effect(bool ncd, int expected)
    {
        (Player player, FakeSkillSpecialHost host) = Setup();
        host.UsingNcd = ncd;

        player.NewSkill.SetSkillSpecial(host, 0, 1, 0, 0, 0, 0);

        Assert.Equal(expected, player.NewSkill.TypeItem);
    }

    [Theory]
    // dir, point, xPlayer, xTarget, expected reach
    [InlineData(1, 2, 0, 1000, 300)]    // the skill's own reach (point * 100 + 100) is shorter than the distance
    [InlineData(-1, 2, 0, 1000, 300)]   // negative direction: -300, made positive
    [InlineData(1, 2, 0, 200, 200)]     // the target is nearer than the reach: the real distance
    [InlineData(1, 2, 0, 50, 300)]      // target closer than 100: the reach again
    [InlineData(1, 2, 0, 100, 100)]     // exactly 100 away and nearer than the reach: the distance
    [InlineData(1, 0, 0, 5000, 100)]    // point 0: reach 100
    [InlineData(1, 2, 500, 100, 300)]   // target on the left, 400 away, farther than the reach 300: the reach
    [InlineData(1, 2, 500, 350, 150)]   // target on the left, 150 away: the real distance made positive
    public void SetSkillSpecial_computes_the_reach(int dir, int point, int xPlayer, int xTarget, int expected)
    {
        (Player player, FakeSkillSpecialHost host) = Setup(Skill.SUPER_KAME, point);

        player.NewSkill.SetSkillSpecial(host, 0, (sbyte)dir, (short)xPlayer, 0, (short)xTarget, 0);

        Assert.Equal(expected, player.NewSkill.XObjTaget);
    }

    [Fact]
    public void SetSkillSpecial_uses_75_for_ma_phong_ba_whatever_the_distance()
    {
        (Player player, FakeSkillSpecialHost host) = Setup(Skill.MA_PHONG_BA, 7);

        player.NewSkill.SetSkillSpecial(host, 0, 1, 0, 0, 20000, 0);

        Assert.Equal(75, player.NewSkill.XObjTaget);
    }

    [Fact]
    public void SetSkillSpecial_makes_the_target_y_positive_and_keeps_the_16_bit_wrap_of_abs_minus_32768()
    {
        (Player player, FakeSkillSpecialHost host) = Setup();

        player.NewSkill.SetSkillSpecial(host, 0, 1, 0, 0, 0, -120);
        Assert.Equal(120, player.NewSkill.YObjTaget);

        player.NewSkill.SetSkillSpecial(host, 0, 1, 0, 0, 0, short.MinValue);
        Assert.Equal(short.MinValue, player.NewSkill.YObjTaget);
    }

    [Fact]
    public void SetSkillSpecial_resets_the_cast_state_and_starts_the_250_ms_timer()
    {
        (Player player, FakeSkillSpecialHost host) = Setup();
        player.NewSkill.StepSkillSpecial = 5;

        player.NewSkill.SetSkillSpecial(host, 1000, -1, 12, 34, 56, 78);

        Assert.True(player.NewSkill.IsStartSkillSpecial);
        Assert.Equal(0, player.NewSkill.StepSkillSpecial);
        Assert.Equal(1000, player.NewSkill.LastTimeSkillSpecial);
        Assert.Equal(-1, player.NewSkill.Dir);
        Assert.Equal(12, player.NewSkill.XPlayer);
        Assert.Equal(34, player.NewSkill.YPlayer);
        Assert.True(player.NewSkill.IsActive);
        Assert.Equal(250, player.NewSkill.PeriodMs);
        Assert.Equal(1250, player.NewSkill.NextRunAtMs);
    }

    [Fact]
    public void SetSkillSpecial_without_a_selected_skill_or_after_dispose_is_an_error()
    {
        var player = new Player();
        var host = new FakeSkillSpecialHost();
        Assert.Throws<InvalidOperationException>(() => player.NewSkill.SetSkillSpecial(host, 0, 1, 0, 0, 0, 0));

        player.PlayerSkill.SkillSelect = new Skill { Template = new SkillTemplate() };
        player.NewSkill.Dispose();
        Assert.Throws<InvalidOperationException>(() => player.NewSkill.SetSkillSpecial(host, 0, 1, 0, 0, 0, 0));
    }

    [Fact]
    public void Tick_runs_one_step_per_period_like_a_fixed_delay_timer()
    {
        (Player player, FakeSkillSpecialHost host) = Setup();
        player.NewSkill.SetSkillSpecial(host, 1000, 1, 0, 0, 100, 0);

        Assert.False(player.NewSkill.Tick(host, 1249));
        Assert.Equal(0, host.Updates);

        Assert.True(player.NewSkill.Tick(host, 1250));
        Assert.Equal(1, host.Updates);
        Assert.Equal(1500, player.NewSkill.NextRunAtMs);

        Assert.False(player.NewSkill.Tick(host, 1499));
        Assert.True(player.NewSkill.Tick(host, 1700)); // late: the next one is a full period after the late run, not catching up
        Assert.Equal(1950, player.NewSkill.NextRunAtMs);
        Assert.Equal(2, host.Updates);
    }

    [Fact]
    public void Tick_leaves_the_start_flag_true_even_after_it_was_cleared_a_java_quirk()
    {
        (Player player, FakeSkillSpecialHost host) = Setup();
        player.NewSkill.SetSkillSpecial(host, 0, 1, 0, 0, 100, 0);
        player.NewSkill.IsStartSkillSpecial = false;

        player.NewSkill.Tick(host, 250);

        Assert.True(player.NewSkill.IsStartSkillSpecial);
        Assert.Equal(1, host.Updates);
    }

    [Fact]
    public void CloseSkillSpecial_stops_the_timer_and_clears_the_targets()
    {
        (Player player, FakeSkillSpecialHost host) = Setup();
        player.NewSkill.SetSkillSpecial(host, 0, 1, 0, 0, 100, 0);
        player.NewSkill.PlayersTaget.Add(new Player());
        player.NewSkill.StepSkillSpecial = 3;

        player.NewSkill.CloseSkillSpecial();

        Assert.False(player.NewSkill.IsActive);
        Assert.False(player.NewSkill.IsStartSkillSpecial);
        Assert.Equal(0, player.NewSkill.StepSkillSpecial);
        Assert.Empty(player.NewSkill.PlayersTaget);
        Assert.False(player.NewSkill.Tick(host, 10_000));
        Assert.Equal(0, host.Updates);
    }

    [Fact]
    public void Start_while_active_changes_nothing()
    {
        (Player player, FakeSkillSpecialHost host) = Setup();
        player.NewSkill.SetSkillSpecial(host, 1000, 1, 0, 0, 100, 0);

        player.NewSkill.Start(5000, 2000);

        Assert.Equal(250, player.NewSkill.PeriodMs);
        Assert.Equal(1250, player.NewSkill.NextRunAtMs);
    }

    [Fact]
    public void Tick_after_dispose_stops_the_timer_without_running_a_step()
    {
        (Player player, FakeSkillSpecialHost host) = Setup();
        player.NewSkill.SetSkillSpecial(host, 0, 1, 0, 0, 100, 0);
        player.NewSkill.Dispose();

        Assert.False(player.NewSkill.Tick(host, 250));

        Assert.False(player.NewSkill.IsActive);
        Assert.Equal(0, host.Updates);
        Assert.Null(player.NewSkill.Player);
        Assert.Null(player.NewSkill.SkillSelect);
    }
}
