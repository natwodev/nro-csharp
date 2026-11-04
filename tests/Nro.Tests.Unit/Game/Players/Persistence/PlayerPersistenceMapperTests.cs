using Nro.Data.Db;
using Nro.Data.Repositories.Players;
using Nro.Game;
using Nro.Game.Players.Persistence;
using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Nro.Tests.Unit.Game.Players.Persistence;

public class PlayerPersistenceMapperTests
{
    [Fact]
    public void A_loaded_and_unchanged_player_keeps_all_114_database_columns_and_the_original_nested_json()
    {
        var mapper = new PlayerPersistenceMapper();
        PlayerPersistenceRecord source = mapper.CreateNew(42, "roundtrip", 1, 103, 1_700_000_000_000L)
            .With("id", 7)
            .With("items_body", "[\"[-1,0,\\\"[]\\\",1700000000000]\"]");

        Player player = mapper.Load(source, 1_700_000_000_000L);
        PlayerPersistenceRecord saved = mapper.Save(player);

        Assert.Equal(114, source.Values.Count);
        var differences = source.Values
            .OrderBy(static entry => entry.Key, StringComparer.Ordinal)
            .Zip(saved.Values.OrderBy(static entry => entry.Key, StringComparer.Ordinal))
            .Where(static pair => !pair.First.Equals(pair.Second))
            .Select(static pair => $"{pair.First.Key}: source={Describe(pair.First.Value)}, saved={Describe(pair.Second.Value)}")
            .ToArray();

        Assert.True(differences.Length == 0, string.Join(Environment.NewLine, differences));
        Assert.Equal("[\"[-1,0,\\\"[]\\\",1700000000000]\"]", saved["items_body"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{}")]
    public void Json_codec_rejects_missing_malformed_or_wrong_shape_data(string? json)
    {
        Assert.Throws<FormatException>(() => PlayerJsonCodec.ReadArray(json, "items_body"));
    }

    [Fact]
    public void Json_codec_keeps_the_exact_large_number_lexeme()
    {
        const string source = "[9223372036854775807,123456789012345678901234567890]";
        Assert.Equal(source, PlayerJsonCodec.ReadArray(source, "data_inventory"));
    }

    [Fact]
    public void New_player_has_the_java_starting_money_stats_and_utc_timestamps()
    {
        var mapper = new PlayerPersistenceMapper();
        PlayerPersistenceRecord row = mapper.CreateNew(9, "saiyan", 2, 104, 1_700_000_000_000L);

        Assert.Equal("[2000,99999999,99999999,0]", row["data_inventory"]);
        Assert.Equal("[0,1200,1200,1000,1000,100,100,15,0,0,0,100,100,15,100,100]", row["data_point"]);
        Assert.Equal(DateTimeKind.Utc, ((DateTime)row["create_time"]!).Kind);
        Assert.Equal(DateTimeKind.Unspecified, ((DateTime)row["lastimelogin"]!).Kind);
    }

    [Fact]
    public void New_player_matches_PlayerDAO_starting_tree_equipment_bag_box_and_skills()
    {
        const long now = 1_700_000_000_000L;
        var mapper = new PlayerPersistenceMapper();
        PlayerPersistenceRecord row = mapper.CreateNew(9, "saiyan", 2, 104, now);

        Assert.Equal($"[1,5,0,{now},{now}]", row["data_magic_tree"]); // PlayerDAO.java:94-99
        Assert.Equal($"[2,1,\"[\\\"[47,3]\\\"]\",{now}]", Slot((string)row["items_body"]!, 0)); // :109-148
        Assert.Equal($"[8,1,\"[\\\"[6,20]\\\"]\",{now}]", Slot((string)row["items_body"]!, 1));
        Assert.Equal(12, ArrayLength((string)row["items_body"]!));
        Assert.Equal(20, ArrayLength((string)row["items_bag"]!));
        Assert.Equal(20, ArrayLength((string)row["items_box"]!));
        Assert.Equal("[4,-1,-1,-1,-1,-1,-1,-1,-1,-1]", row["skills_shortcut"]); // :355-365
        Assert.Equal("[4]", row["bought_skill"]); // :478-480
    }

    [Theory]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)1)]
    [InlineData((sbyte)2)]
    public void New_player_matches_all_113_persisted_values_from_PlayerDAO_and_the_player_DDL(sbyte gender)
    {
        const long now = 1_700_000_000_000L;
        const int accountId = 73;
        string name = $"gender{gender}";
        var mapper = new PlayerPersistenceMapper();

        PlayerPersistenceRecord row = mapper.CreateNew(accountId, name, gender, (short)(102 + gender), now);
        IReadOnlyDictionary<string, object?> expected = ExpectedNewPlayerValuesFromJavaGolden(gender, now);

        Assert.Equal(113, expected.Count);
        Assert.Equal(PlayerPersistenceColumns.All.Skip(1).OrderBy(static column => column).ToArray(), expected.Keys.OrderBy(static column => column).ToArray());
        Assert.Equal(114, row.Values.Count);
        Assert.Equal(PlayerPersistenceColumns.All.OrderBy(static column => column).ToArray(), row.Values.Keys.OrderBy(static column => column).ToArray());
        Assert.Null(row["notify"]);
        Assert.Null(row["data_super_rank"]);
        Assert.Null(row["learn_skill"]);
        AssertPlayerValuesMatch(expected, row);
    }

    [Fact]
    public void New_player_uses_every_DEFAULT_and_nullable_rule_from_player_DDL()
    {
        const long now = 1_700_000_000_000L;
        PlayerPersistenceRecord row = new PlayerPersistenceMapper().CreateNew(73, "schema", 0, 102, now);
        IReadOnlyDictionary<string, string> defaults = ReadPlayerSchemaDefaults();

        // schema.sql:556-670 is the authority. The identity column is deliberately omitted from INSERT.
        Assert.DoesNotContain("id", defaults.Keys);
        var expected = defaults.ToDictionary(
            static pair => pair.Key,
            pair => (object?)MaterializeDdlDefault(pair.Key, pair.Value, now),
            StringComparer.Ordinal);
        AssertPlayerValuesMatch(expected, row);

        // The DDL has no DEFAULT for these nullable columns; do not invent an empty JSON array.
        Assert.Null(row["notify"]); // schema.sql:606
        Assert.Null(row["data_super_rank"]); // schema.sql:626
        Assert.Null(row["learn_skill"]); // schema.sql:641
    }

    [Fact]
    public void Deferred_columns_are_explicit_and_preserve_their_original_lexeme()
    {
        var mapper = new PlayerPersistenceMapper();
        PlayerPersistenceRecord source = mapper.CreateNew(42, "deferred", 0, 64, 1_700_000_000_000L)
            .With("data_location", "[42,101,385]")
            .With("clan_id", 123);

        Player player = mapper.Load(source, 1_700_000_000_000L);
        IPlayerPersistenceDeferredData deferred = mapper.GetDeferredData(player);

        Assert.Contains("data_location", deferred.DeferredColumns);
        Assert.Contains("clan_id", deferred.DeferredColumns);
        Assert.Equal("[42,101,385]", deferred.GetRawColumn("data_location"));
        Assert.Equal(123, deferred.GetRawColumn("clan_id"));
        Assert.Equal(source["data_location"], mapper.Save(player)["data_location"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    public void Unchanged_notify_preserves_null_and_the_text_zero(string? notify)
    {
        var mapper = new PlayerPersistenceMapper();
        PlayerPersistenceRecord source = mapper.CreateNew(42, "roundtrip", 1, 103, 1_700_000_000_000L)
            .With("notify", notify);

        Player player = mapper.Load(source, 1_700_000_000_000L);
        PlayerPersistenceRecord saved = mapper.Save(player);

        Assert.Equal(notify, player.Notify);
        Assert.Equal(notify, saved["notify"]);
    }

    [Theory]
    [InlineData("saiyan")]
    [InlineData("a1b2c3d4e5")]
    public void CreateNew_accepts_lowercase_ascii_alphanumeric_names(string name)
    {
        var mapper = new PlayerPersistenceMapper();

        PlayerPersistenceRecord row = mapper.CreateNew(9, name, 2, 104, 1_700_000_000_000L);

        Assert.Equal(name, row.Name);
    }

    [Fact]
    public void CreateNew_rejects_uppercase_name_before_it_can_be_persisted()
    {
        var mapper = new PlayerPersistenceMapper();

        ArgumentException exception = Assert.Throws<ArgumentException>(() => mapper.CreateNew(9, "SaiYan", 2, 104, 1_700_000_000_000L));

        Assert.Contains(PlayerCharacterName.ValidationMessage, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void NormalizeAndValidate_matches_Java_lowercase_creation_behavior()
    {
        Assert.Equal("saiyan", PlayerCharacterName.NormalizeAndValidate("SaiYan"));
    }

    [Theory]
    [InlineData("abcd")]
    [InlineData("abcdefghijk")]
    [InlineData("sai-yan")]
    [InlineData("sàiyan")]
    [InlineData("sai yan")]
    public void CreateNew_rejects_names_outside_the_persisted_ascii_lowercase_pattern(string name)
    {
        var mapper = new PlayerPersistenceMapper();

        ArgumentException exception = Assert.Throws<ArgumentException>(() => mapper.CreateNew(9, name, 2, 104, 1_700_000_000_000L));

        Assert.Contains(PlayerCharacterName.ValidationMessage, exception.Message, StringComparison.Ordinal);
    }

    private static string Describe(object? value) => value is null
        ? "<null>"
        : $"{value.GetType().FullName}({value})";

    private static void AssertPlayerValuesMatch(IReadOnlyDictionary<string, object?> expected, PlayerPersistenceRecord actual)
    {
        string[] differences = expected
            .Where(pair => !Equals(pair.Value, actual[pair.Key]))
            .Select(pair => $"{pair.Key}: expected={Describe(pair.Value)}, actual={Describe(actual[pair.Key])}")
            .OrderBy(static difference => difference, StringComparer.Ordinal)
            .ToArray();

        Assert.True(differences.Length == 0, string.Join(Environment.NewLine, differences));
    }

    private static int ArrayLength(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement.GetArrayLength();
    }

    private static string Slot(string json, int index)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        return document.RootElement[index].GetString()!;
    }

    // Golden was emitted by a standalone Java/json-simple extraction of PlayerDAO.createNewPlayer:57-524.
    // It contains each of the 47 INSERT parameters; DDL defaults fill the other writable columns.
    private static IReadOnlyDictionary<string, object?> ExpectedNewPlayerValuesFromJavaGolden(sbyte gender, long now)
    {
        var expected = ReadPlayerSchemaDefaults().ToDictionary(
            static pair => pair.Key,
            pair => (object?)MaterializeDdlDefault(pair.Key, pair.Value, now),
            StringComparer.Ordinal);
        expected["notify"] = null;
        expected["data_super_rank"] = null;
        expected["learn_skill"] = null;

        using JsonDocument golden = JsonDocument.Parse(File.ReadAllText(FindWorkspaceFile("tests", "golden", $"player-new-gender{gender}.json")));
        foreach (JsonProperty property in golden.RootElement.EnumerateObject())
        {
            switch (property.Name)
            {
                case "have_tennis_space_ship":
                    expected[property.Name] = property.Value.GetInt32() != 0;
                    break;
                case "lastimelogin_epoch_ms":
                    expected["lastimelogin"] = DateTime.SpecifyKind(DateTime.UnixEpoch.AddMilliseconds(property.Value.GetInt64()), DateTimeKind.Unspecified);
                    break;
                case "last_time_login_game_epoch_ms":
                    expected["last_time_login_game"] = DateTime.UnixEpoch.AddMilliseconds(property.Value.GetInt64());
                    break;
                case "account_id" or "head" or "gender" or "clan_id":
                    expected[property.Name] = property.Value.GetInt32();
                    break;
                default:
                    expected[property.Name] = property.Value.GetString();
                    break;
            }
        }

        Assert.Equal(now, golden.RootElement.GetProperty("lastimelogin_epoch_ms").GetInt64());
        Assert.Equal(47, golden.RootElement.EnumerateObject().Count());
        return expected;
    }

    private static IReadOnlyDictionary<string, string> ReadPlayerSchemaDefaults()
    {
        string[] lines = File.ReadAllLines(FindWorkspaceFile("postgres", "schema", "schema.sql"));
        int start = Array.FindIndex(lines, static line => line == "CREATE TABLE player (");
        Assert.True(start >= 0, "The player table was not found in postgres/schema/schema.sql.");
        int end = Array.FindIndex(lines, start, static line => line == ");");
        Assert.True(end > start, "The player table has no closing delimiter in postgres/schema/schema.sql.");

        var defaults = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int index = start + 1; index < end; index++)
        {
            Match match = Regex.Match(lines[index], @"^\s{4}(?<column>[a-z0-9_]+)\s+.*?\sDEFAULT\s+(?<value>'[^']*'|[^\s,]+)");
            if (match.Success && match.Groups["column"].Value != "id")
            {
                defaults.Add(match.Groups["column"].Value, match.Groups["value"].Value);
            }
        }

        return defaults;
    }

    private static object MaterializeDdlDefault(string column, string expression, long now)
    {
        if (expression == "now()") return DateTime.UnixEpoch.AddMilliseconds(now);
        if (expression == "false") return false;
        if (expression.StartsWith('\'') && expression.EndsWith('\'')) return expression[1..^1];
        if (IsBigintColumn(column))
        {
            return long.Parse(expression, CultureInfo.InvariantCulture);
        }

        return int.Parse(expression, CultureInfo.InvariantCulture);
    }

    private static bool IsBigintColumn(string column) => column is "last_time_drop_tail" or "last_time_khi_su_kien" or "barracks_state" or "lasttimepkcommeson" or "dragon_christmas" or "dragon_halloween" or "last_time_update_task";

    private static string FindWorkspaceFile(params string[] parts)
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            string candidate = Path.Combine([directory.FullName, .. parts]);
            if (File.Exists(candidate)) return candidate;
        }

        throw new FileNotFoundException("Could not find the workspace PostgreSQL schema.");
    }

}
