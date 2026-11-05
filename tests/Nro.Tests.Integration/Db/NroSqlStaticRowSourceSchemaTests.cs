using System.Text.RegularExpressions;
using Nro.Content.StaticData;

namespace Nro.Tests.Integration.Db;

/// <summary>
/// Creates the static tables from the generated PostgreSQL schema in an isolated nro_test schema.
/// This ensures hand-written loader SQL stays aligned with the generated names and types.
/// </summary>
public class StaticRowSourceSchemaFixture : TestDatabase
{
    private static readonly string[] StaticTables =
    [
        "part", "bg_item_template", "item_template", "item_option_template", "head_avatar", "flag_bag",
        "head_animation_frame", "img_by_name", "attribute_template", "power_limit", "caption", "skill_template",
        "intrinsic", "task_main_template", "task_sub_template", "side_task_template", "clan_task_template",
        "task_kol_template", "task_event_template", "achievement_template", "shop", "tab_shop", "item_shop",
        "item_shop_option", "task_badges_template", "data_badges", "notify", "radar", "mob_template",
        "npc_template", "map_template",
    ];

    private static readonly Regex CreateTable = new(
        @"(?ms)^CREATE TABLE (?<table>[a-z_]+) \(.*?^\);\r?\n",
        RegexOptions.CultureInvariant);

    protected override async Task SeedAsync()
    {
        string schema = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "schema.sql"));
        var expected = new HashSet<string>(StaticTables, StringComparer.Ordinal);
        var created = new HashSet<string>(StringComparer.Ordinal);

        foreach (Match match in CreateTable.Matches(schema))
        {
            string table = match.Groups["table"].Value;
            if (expected.Contains(table))
            {
                await RunAsync(match.Value);
                created.Add(table);
            }
        }

        Assert.Equal(expected.Order(), created.Order());
    }
}

public sealed class SeededStaticRowSourceSchemaFixture : StaticRowSourceSchemaFixture
{
    protected override async Task SeedAsync()
    {
        await base.SeedAsync();

        await RunAsync("insert into part (id, type, data) values (1, 2, '[]')");
        await RunAsync("insert into bg_item_template (id, image_id, layer, dx, dy) values (1, 2, 3, 4, 5)");
        await RunAsync("insert into item_template (id, type, gender, name, description, level, icon_id, part, is_up_to_up, power_require, gold, gold_sell, gem, gem_sell, ruby, ruby_sell, head, body, leg, type_event, is_gender) values (1, 2, 1, 'Item', 'Description', 3, 4, 5, true, 6000000000, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 2)");
        await RunAsync("insert into item_option_template (id, name, type) values (1, 'Option', 2)");
        await RunAsync("insert into head_avatar (head_id, avatar_id) values (1, 2)");
        await RunAsync("insert into flag_bag (id, icon_data, name, gold, gem, icon_id) values (1, '[]', 'Flag', 2, 3, 4)");
        await RunAsync("insert into head_animation_frame (id, data) values (1, '[]')");
        await RunAsync("insert into img_by_name (id, name, n_frame) values (1, 'image', 2)");
        await RunAsync("insert into attribute_template (id, name) values (1, 'Attribute')");
        await RunAsync("insert into power_limit (id, power, hp, mp, damage, defense, critical) values (1, 6000000000, 2, 3, 4, 5, 6)");
        await RunAsync("insert into caption (id, earth, saiya, namek, power) values (1, 'Earth', 'Saiya', 'Namek', 6000000000)");
        await RunAsync("insert into skill_template (nclass_id, id, name, max_point, mana_use_type, type, icon_id, dam_info, slot, skills) values (1, 2, 'Skill', 3, 4, 5, 6, 'Damage', 7, '[]')");
        await RunAsync("insert into intrinsic (id, name, param_from_1, param_to_1, param_from_2, param_to_2, icon, gender) values (1, 'Intrinsic', 2, 3, 4, 5, 6, 1)");
        await RunAsync("insert into task_main_template (id, name, detail) values (1, 'Main', 'Detail')");
        await RunAsync("insert into task_sub_template (task_main_id, name, max_count, notify, npc_id, map, idmain) values (1, 'Sub', 2, 'Notify', 3, 4, 5)");
        await RunAsync("insert into side_task_template values (1, 'Side', '1', '2', '3', '4', '5')");
        await RunAsync("insert into clan_task_template values (1, 'Clan', '1', '2', '3', '4', '5')");
        await RunAsync("insert into task_kol_template values (1, 'Kol', '2')");
        await RunAsync("insert into task_event_template values (1, 'Event', '1', '2', '3', '4', '5')");
        await RunAsync("insert into achievement_template (id, info1, info2, money, max_count) values (1, 'Info1', 'Info2', 3, 6000000000)");
        await RunAsync("insert into shop (id, npc_id, tag_name, type_shop) values (1, 2, null, null)");
        await RunAsync("insert into tab_shop (id, shop_id, name) values (1, 1, 'Tab')");
        await RunAsync("insert into item_shop (id, tab_id, temp_id, is_new, is_sell, type_sell, cost, costgold, icon_spec, create_time) values (1, 1, 1, 2, true, null, null, null, null, '2026-01-02T03:04:05Z')");
        await RunAsync("insert into item_shop_option (id, item_shop_id, option_id, param) values (1, 1, 2, 3)");
        await RunAsync("insert into task_badges_template (id, name, max_count, id_badges_reward) values (1, null, 2, 3)");
        await RunAsync("insert into data_badges (id, id_effect, id_item, name, options) values (1, 2, 3, 'Badge', '[]')");
        await RunAsync("insert into notify (id, name, text) values (1, 'Notify', 'Text')");
        await RunAsync("insert into radar (id, icon_id, rank, max, type, mob_id, body, name, info, options, aura_id) values (1, null, null, null, null, null, null, null, null, null, null)");
        await RunAsync("insert into mob_template (id, type, name, hp, range_move, speed, dart_type, damage_percent, percent_potential, percent_gold) values (1, 2, 'Mob', 3, 4, 5, 6, 7, 8, 9)");
        await RunAsync("insert into npc_template (id, name, head, body, leg, avatar) values (1, 'Npc', 2, 3, 4, null)");
        await RunAsync("insert into map_template (id, name, zones, max_player, data, type, planet_id, bg_type, tile_id, bg_id, waypoints, mobs, npcs, gender_type) values (1, 'Map', 2, 3, '[]', 4, 5, 6, 7, 8, '[]', '[]', '[]', 9)");
    }
}

public sealed class NroSqlStaticRowSourceSchemaTests : IClassFixture<StaticRowSourceSchemaFixture>
{
    private readonly StaticRowSourceSchemaFixture fixture;

    public NroSqlStaticRowSourceSchemaTests(StaticRowSourceSchemaFixture fixture) => this.fixture = fixture;

    [Fact]
    public async Task Every_read_query_executes_against_the_generated_schema_when_tables_are_empty()
    {
        await ReadEveryMethodAsync(new NroSqlStaticRowSource(fixture.Db));
    }

    internal static async Task ReadEveryMethodAsync(NroSqlStaticRowSource source)
    {
        await source.ReadPartsAsync();
        await source.ReadBgItemsAsync();
        await source.ReadItemTemplatesAsync();
        await source.ReadItemOptionTemplatesAsync();
        await source.ReadHeadAvatarsAsync();
        await source.ReadFlagBagsAsync();
        await source.ReadHeadAnimationFramesAsync();
        await source.ReadImagesByNameAsync();
        await source.ReadAttributeTemplatesAsync();
        await source.ReadPowerLimitsAsync();
        await source.ReadCaptionsAsync();
        await source.ReadSkillTemplatesAsync();
        await source.ReadIntrinsicsAsync();
        await source.ReadTaskMainsAsync();
        await source.ReadTaskSubsAsync();
        await source.ReadSideTaskTemplatesAsync();
        await source.ReadClanTaskTemplatesAsync();
        await source.ReadKolTaskTemplatesAsync();
        await source.ReadEventTaskTemplatesAsync();
        await source.ReadAchievementTemplatesAsync();
        await source.ReadShopsAsync();
        await source.ReadShopTabsAsync();
        await source.ReadShopItemsAsync();
        await source.ReadShopItemOptionsAsync();
        await source.ReadBadgeTaskTemplatesAsync();
        await source.ReadBadgeTemplatesAsync();
        await source.ReadNotificationsAsync();
        await source.ReadRadarCardsAsync();
        await source.ReadMobTemplatesAsync();
        await source.ReadNpcTemplatesAsync();
        await source.ReadMapTemplatesAsync();
    }
}

public sealed class NroSqlStaticRowSourceSampleRowTests : IClassFixture<SeededStaticRowSourceSchemaFixture>
{
    private readonly SeededStaticRowSourceSchemaFixture fixture;

    public NroSqlStaticRowSourceSampleRowTests(SeededStaticRowSourceSchemaFixture fixture) => this.fixture = fixture;

    [Fact]
    public async Task Every_read_method_maps_a_hand_inserted_schema_row()
    {
        var source = new NroSqlStaticRowSource(fixture.Db);

        Assert.Single(await source.ReadPartsAsync());
        Assert.Single(await source.ReadBgItemsAsync());
        Assert.Single(await source.ReadItemTemplatesAsync());
        Assert.Single(await source.ReadItemOptionTemplatesAsync());
        Assert.Single(await source.ReadHeadAvatarsAsync());
        Assert.Single(await source.ReadFlagBagsAsync());
        Assert.Single(await source.ReadHeadAnimationFramesAsync());
        Assert.Single(await source.ReadImagesByNameAsync());
        Assert.Single(await source.ReadAttributeTemplatesAsync());
        Assert.Single(await source.ReadPowerLimitsAsync());
        Assert.Single(await source.ReadCaptionsAsync());
        Assert.Single(await source.ReadSkillTemplatesAsync());
        Assert.Single(await source.ReadIntrinsicsAsync());
        Assert.Single(await source.ReadTaskMainsAsync());
        Assert.Single(await source.ReadTaskSubsAsync());
        Assert.Single(await source.ReadSideTaskTemplatesAsync());
        Assert.Single(await source.ReadClanTaskTemplatesAsync());
        Assert.Single(await source.ReadKolTaskTemplatesAsync());
        Assert.Single(await source.ReadEventTaskTemplatesAsync());
        Assert.Single(await source.ReadAchievementTemplatesAsync());

        var shop = Assert.Single(await source.ReadShopsAsync());
        Assert.Equal(string.Empty, shop.TagName);
        Assert.Null(shop.TypeShop);
        Assert.Single(await source.ReadShopTabsAsync());
        var shopItem = Assert.Single(await source.ReadShopItemsAsync());
        Assert.Equal((short)2, shopItem.IsNew);
        Assert.True(shopItem.IsSell);
        Assert.Null(shopItem.TypeSell);
        Assert.Null(shopItem.Cost);
        Assert.Equal(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), shopItem.CreateTime);
        Assert.Single(await source.ReadShopItemOptionsAsync());
        var badgeTask = Assert.Single(await source.ReadBadgeTaskTemplatesAsync());
        Assert.Equal(string.Empty, badgeTask.Name);
        Assert.Single(await source.ReadBadgeTemplatesAsync());
        Assert.Single(await source.ReadNotificationsAsync());
        var radar = Assert.Single(await source.ReadRadarCardsAsync());
        Assert.Null(radar.IconId);
        Assert.Null(radar.Name);

        var mob = Assert.Single(await source.ReadMobTemplatesAsync());
        Assert.Equal((short)7, mob.DamagePercent);
        Assert.Equal((short)8, mob.PercentPotential);
        Assert.Equal((short)9, mob.PercentGold);
        var npc = Assert.Single(await source.ReadNpcTemplatesAsync());
        Assert.Null(npc.Avatar);
        var map = Assert.Single(await source.ReadMapTemplatesAsync());
        Assert.Equal(9, map.GenderType);
    }
}
