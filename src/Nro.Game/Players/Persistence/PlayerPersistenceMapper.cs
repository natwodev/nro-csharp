using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using Nro.Core.Consts;
using Nro.Data.Repositories.Players;

namespace Nro.Game.Players.Persistence;

// Ánh xạ vỏ Player với DTO hàng dữ liệu không mất dữ liệu.
// Nguồn Java: jbcd/dao/PlayerDAO.java (C# tách mapper; Location, Clan và JSON phụ thuộc vẫn DEFERRED, lưu lại lexeme gốc).


public sealed class PlayerPersistenceMapper
{
    private readonly ConditionalWeakTable<Player, PlayerLoadState> loaded = new();
    private static readonly JsonSerializerOptions JavaJson = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    // Thực hiện Load (Java: load).

    public Player Load(PlayerPersistenceRecord row, long nowMs)
    {
        ArgumentNullException.ThrowIfNull(row);
        var player = new Player(nowMs)
        {
            Id = GetInt(row, "id"), Account_id = GetInt(row, "account_id"), Name = GetString(row, "name"),
            Head = checked((short)GetInt(row, "head")), Gender = checked((sbyte)GetInt(row, "gender")),
            HaveTennisSpaceShip = GetBool(row, "have_tennis_space_ship"), LastTimeDropTail = GetLong(row, "last_time_drop_tail"),
            LastEventGasTime = GetLong(row, "last_event_monkey_transformation_at"), TransformationState = GetInt(row, "is_transformed"),
            Notify = row["notify"] as string, LastPkCommesonTime = GetLong(row, "last_commeson_boss_fight_at"),
            LastTimeShenronAppeared_Christmas = GetLong(row, "dragon_christmas"),
            LastTimeShenronAppeared_Halloween = GetLong(row, "dragon_halloween"),
            Lastimelogin = row["initial_login_at"] as DateTime?, FirstTimeLogin = row["first_time_login"] as DateTime?,
            LastTimeLoginGame = row["last_time_login_game"] as DateTime?,
        };
        ReadInventory(player, GetString(row, "data_inventory"));
        ReadPoint(player, GetString(row, "data_point"));

        Attach(player, row);
        return player;
    }

    // Thực hiện GetDeferredData (Java: getDeferredData).

    public IPlayerPersistenceDeferredData GetDeferredData(Player player) => GetState(player).DeferredData;

    // Thực hiện Save (Java: save).

    public PlayerPersistenceRecord Save(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);
        PlayerPersistenceRecord original = GetState(player).Row;
        PlayerPersistenceRecord saved = original;
        saved = ReplaceWhenChanged(saved, original, "name", player.Name, StringMatches);
        saved = ReplaceWhenChanged(saved, original, "head", (int)player.Head, IntMatches);
        saved = ReplaceWhenChanged(saved, original, "gender", (int)player.Gender, IntMatches);
        saved = ReplaceWhenChanged(saved, original, "have_tennis_space_ship", player.HaveTennisSpaceShip, BoolMatches);
        saved = ReplaceWhenChanged(saved, original, "last_time_drop_tail", player.LastTimeDropTail, LongMatches);
        saved = ReplaceWhenChanged(saved, original, "last_event_monkey_transformation_at", player.LastEventGasTime, LongMatches);
        saved = ReplaceWhenChanged(saved, original, "is_transformed", player.TransformationState, IntMatches);
        saved = ReplaceWhenChanged(saved, original, "notify", player.Notify, StringMatches);
        saved = ReplaceWhenChanged(saved, original, "last_commeson_boss_fight_at", player.LastPkCommesonTime, LongMatches);
        saved = ReplaceWhenChanged(saved, original, "dragon_christmas", player.LastTimeShenronAppeared_Christmas, LongMatches);
        saved = ReplaceWhenChanged(saved, original, "dragon_halloween", player.LastTimeShenronAppeared_Halloween, LongMatches);
        saved = ReplaceWhenChanged(saved, original, "initial_login_at", player.Lastimelogin, DateTimeMatches);
        saved = ReplaceWhenChanged(saved, original, "first_time_login", player.FirstTimeLogin, DateTimeMatches);
        saved = ReplaceWhenChanged(saved, original, "last_time_login_game", player.LastTimeLoginGame, DateTimeMatches);
        if (!InventoryMatches(player, original["data_inventory"] as string)) saved = saved.With("data_inventory", WriteInventory(player));
        if (!PointMatches(player, original["data_point"] as string)) saved = saved.With("data_point", WritePoint(player));
        Attach(player, saved);
        return saved;
    }

    // Thực hiện CreateNew (Java: createNew).

    public PlayerPersistenceRecord CreateNew(int accountId, string name, sbyte gender, short head, long nowMs)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(accountId);
        PlayerCharacterName.EnsureValidStoredName(name);
        if (gender is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(gender));

        DateTime utc = DateTime.UnixEpoch.AddMilliseconds(nowMs);
        Dictionary<string, object?> values = CreateDdlDefaults(utc);
        values["id"] = 0; values["account_id"] = accountId; values["name"] = name; values["head"] = (int)head; values["gender"] = (int)gender;
        values["have_tennis_space_ship"] = false; values["clan_id"] = -1;
        values["data_inventory"] = "[2000,99999999,99999999,0]";
        values["data_location"] = $"[{39 + gender},100,384]";
        values["data_point"] = NewPoint(gender);
        values["data_magic_tree"] = $"[1,5,0,{nowMs},{nowMs}]";
        values["items_body"] = NewBodyItems(gender, nowMs);
        values["items_bag"] = NewEmptyItemSlots(20, nowMs);
        values["items_box"] = NewBoxItems(nowMs);
        values["items_box_collection"] = NewEmptyItemSlots(20, nowMs);
        values["items_box_lucky_round"] = NewEmptyItemSlots(110, nowMs);
        values["sold_items"] = NewEmptyItemSlots(20, nowMs);
        values["item_mails_box"] = NewEmptyItemSlots(200, nowMs);
        values["friends"] = "[]"; values["enemies"] = "[]";
        values["data_intrinsic"] = ZeroArray(8); values["data_item_time"] = ZeroArray(79); values["data_task"] = "[0,0,0]";
        values["data_mabu_egg"] = "[]"; values["data_charm"] = RepeatedArray(nowMs, 21); values["skills"] = NewSkills(gender); values["skills_shortcut"] = NewSkillShortcut(gender);
        values["training_data"] = "[]"; values["pet"] = "[]"; values["my_father"] = "[]"; values["my_mother"] = "[]";
        values["data_black_ball"] = NestedRepeatedArray("[0,0,0]", 7); values["data_side_task"] = "[-1,0,0,0,20,0]";
        values["data_kol_task"] = "[0,0]"; values["data_event_task"] = "[-1,0,0,0,20,0]"; values["daily_gift_activity_data"] = ZeroArray(44);
        values["bill_data"] = "[]"; values["watermelon_plant_data"] = "[]"; values["lover_data"] = "[]";
        values["bought_skill"] = $"[{InitialSkill(gender)}]";
        values["create_time"] = utc; values["first_time_login"] = utc;
        values["initial_login_at"] = DateTime.SpecifyKind(utc, DateTimeKind.Unspecified); values["last_time_login_game"] = utc;
        values["master_does_not_attack"] = "false"; values["father_does_not_attack"] = "false"; values["mother_does_not_attack"] = "false";
        values["notify"] = null; values["lover_does_not_attack"] = "false"; values["first_child_does_not_attack"] = "false"; values["second_child_does_not_attack"] = "false"; values["third_child_does_not_attack"] = "false";
        values["clan_treasure_under_sea_achievement"] = ""; values["clan_destron_gas_achievement"] = ""; values["clan_snake_way_achievement"] = "";
        values["data_badges"] = "[]"; values["data_task_badges"] = "[]"; values["daily_gift"] = "[]";
        return new PlayerPersistenceRecord(values);
    }

    // Hiện thực giá trị mặc định PostgreSQL vì PlayerRepository chèn mọi cột ghi được; cột không có DDL default chủ ý null cho tới PlayerDAO.createNewPlayer cấp giá trị.
    private static Dictionary<string, object?> CreateDdlDefaults(DateTime utc)
    {
        var values = PlayerPersistenceColumns.All.ToDictionary(static column => column, static _ => (object?)null, StringComparer.Ordinal);
        var defaults = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            // Giá trị identity không được chèn; đây chỉ là sentinel trước khi persist.
            ["id"] = 0,
            ["head"] = 102,
            ["have_tennis_space_ship"] = false,
            ["clan_id"] = -1,
            ["last_time_drop_tail"] = 0L,
            ["last_event_monkey_transformation_at"] = 0L,
            ["create_time"] = utc,
            ["point_pvp"] = 0,
            ["data_card"] = "[]",
            ["time_travel_duration"] = "[0,0]",
            ["whis_sparring"] = 0,
            ["is_transformed"] = 0,
            ["account_ban_check_data"] = "[0,0,0]",
            ["master_does_not_attack"] = "false",
            ["father_does_not_attack"] = "false",
            ["mother_does_not_attack"] = "false",
            ["treasure_under_sea_uses"] = "[]",
            ["barracks_state"] = 0L,
            ["snake_way_data"] = "[]",
            ["data_clan_task"] = "[]",
            ["data_achievement"] = "[]",
            ["data_vip"] = "[]",
            ["last_commeson_boss_fight_at"] = 0L,
            ["super_divine_water_state"] = "[]",
            ["account_protection_data"] = "[]",
            ["first_time_login"] = utc,
            ["last_time_login_game"] = utc,
            ["wood_chest_data"] = "[]",
            ["deathmatch_arena_data"] = "[]",
            ["dragon_christmas"] = 0L,
            ["dragon_halloween"] = 0L,
            ["rank"] = 0,
            ["data_item_event"] = "[]",
            ["card_data"] = "[0,0,0,0,0,0,0,0]",
            ["check_in_data"] = "[0,0,0,0,0,0,0,0,0,0]",
            ["punching_machine_data"] = "[0,0,0]",
            ["lover_does_not_attack"] = "false",
            ["first_child_does_not_attack"] = "false",
            ["second_child_does_not_attack"] = "false",
            ["third_child_does_not_attack"] = "false",
            ["event_point"] = 0,
            ["event_point_boss"] = 0,
            ["ngu_hanh_son_event_points"] = 0,
            ["monster_hunt_event_points"] = 0,
            ["destruction_points"] = 0,
            ["namek_battlefield_points"] = 0,
            ["lunar_new_year_event_points"] = 0,
            ["christmas_event_points"] = 0,
            ["halloween_event_points"] = 0,
            ["international_womens_day_event_points"] = 0,
            ["moon_festival_event_points"] = 0,
            ["hung_king_event_points"] = 0,
            ["black_friday_event_points"] = 0,
            ["vietnamese_womens_day_event_points"] = 0,
            ["data_event_new_year"] = "[0,0,0,0]",
            ["christmas_event_data"] = "[0,0,0,0]",
            ["vu_lan_event_data"] = "[0,0,0]",
            ["data_event_halloween"] = "[0,0]",
            ["international_womens_day_event_data"] = "[0,0]",
            ["moon_festival_event_data"] = "[0,0]",
            ["hung_king_event_data"] = "[0,0,0,0,0,0]",
            ["data_event_black_friday"] = "[0,0]",
            ["daily_counter_data"] = "[0,0,0,0,0,0,0,0,0,0]",
            ["hp_point_fusion"] = 0,
            ["mp_point_fusion"] = 0,
            ["point_fusion_damage"] = 0,
            ["last_time_update_task"] = 1L,
            ["title"] = "[0,0,0,0,0,0,0,0]",
        };

        foreach ((string column, object? value) in defaults)
        {
            values[column] = value;
        }

        return values;
    }

    // Thực hiện Attach (Java: attach).

    public void Attach(Player player, PlayerPersistenceRecord row)
    {
        ArgumentNullException.ThrowIfNull(player); ArgumentNullException.ThrowIfNull(row);
        loaded.Remove(player); loaded.Add(player, new PlayerLoadState(row));
    }

    private PlayerLoadState GetState(Player player) => loaded.TryGetValue(player, out PlayerLoadState? state)
        ? state : throw new InvalidOperationException("Only a player loaded or created by PlayerPersistenceMapper can be saved.");

    private sealed class PlayerLoadState
    {
        // Gắn hàng gốc và dữ liệu hoãn xử lý để Save giữ nguyên lexeme.
        public PlayerLoadState(PlayerPersistenceRecord row)
        {
            Row = row;
            DeferredData = new PlayerPersistenceDeferredData(row, DeferredColumns);
        }

    // Lưu trạng thái Row (Java: row).

        public PlayerPersistenceRecord Row { get; }

    // Lưu trạng thái DeferredData (Java: deferredData).

        public IPlayerPersistenceDeferredData DeferredData { get; }
    }

    private static readonly IReadOnlySet<string> DeferredColumns = new HashSet<string>(
        PlayerPersistenceColumns.All.Where(static column => column is not "id" and not "account_id" and not "name" and not "head" and not "gender" and not "have_tennis_space_ship" and not "last_time_drop_tail" and not "last_event_monkey_transformation_at" and not "is_transformed" and not "notify" and not "last_commeson_boss_fight_at" and not "dragon_christmas" and not "dragon_halloween" and not "initial_login_at" and not "first_time_login" and not "last_time_login_game" and not "data_inventory" and not "data_point"), StringComparer.Ordinal);

    private static int GetInt(PlayerPersistenceRecord row, string column) => row[column] switch { int value => value, long value => checked((int)value), null => 0, _ => throw new FormatException($"Player column '{column}' is not an integer.") };
    private static long GetLong(PlayerPersistenceRecord row, string column) => row[column] switch { long value => value, int value => value, null => 0, _ => throw new FormatException($"Player column '{column}' is not an Int64.") };
    private static string GetString(PlayerPersistenceRecord row, string column) => row[column] as string ?? throw new FormatException($"Player column '{column}' is null.");
    private static bool GetBool(PlayerPersistenceRecord row, string column) => Convert.ToBoolean(row[column] ?? false, System.Globalization.CultureInfo.InvariantCulture);
    private static PlayerPersistenceRecord ReplaceWhenChanged<T>(PlayerPersistenceRecord saved, PlayerPersistenceRecord original, string column, T value, Func<object?, T, bool> matches) => matches(original[column], value) ? saved : saved.With(column, value);
    private static bool StringMatches(object? original, string? value) => original is null ? value is null : original is string text && text == value;
    private static bool IntMatches(object? original, int value) => original is int number && number == value || original is long longNumber && longNumber == value;
    private static bool LongMatches(object? original, long value) => original is long number && number == value || original is int intNumber && intNumber == value;
    private static bool BoolMatches(object? original, bool value) => original is bool boolean && boolean == value || original is int number && (number != 0) == value || original is long longNumber && (longNumber != 0) == value;
    private static bool DateTimeMatches(object? original, DateTime? value) => original switch { null => value is null, DateTime dateTime => dateTime == value, _ => false };

    private static void ReadInventory(Player player, string json)
    {
        long[] values = ReadNumbers(json, "data_inventory", 4);
        player.Inventory.Gold = values[0]; player.Inventory.Gem = checked((int)values[1]); player.Inventory.Ruby = checked((int)values[2]); player.Inventory.VipExperience = checked((int)values[3]);
    }
    private static bool InventoryMatches(Player player, string? source)
    {
        if (source is null) return false;
        long[] values = ReadNumbers(source, "data_inventory", 4);
        return values[0] == player.Inventory.Gold && values[1] == player.Inventory.Gem && values[2] == player.Inventory.Ruby && values[3] == player.Inventory.VipExperience;
    }
    private static string WriteInventory(Player player) => $"[{player.Inventory.Gold},{player.Inventory.Gem},{player.Inventory.Ruby},{player.Inventory.VipExperience}]";
    private static void ReadPoint(Player player, string json)
    {
        long[] values = ReadNumbers(json, "data_point", 16);
        player.NPoint.LimitPower = checked((sbyte)values[0]); player.NPoint.Power = values[1]; player.NPoint.Potential = values[2]; player.NPoint.Stamina = checked((int)values[3]); player.NPoint.MaxStamina = checked((int)values[4]); player.NPoint.BaseHp = values[5]; player.NPoint.BaseMp = values[6]; player.NPoint.BaseDamage = values[7]; player.NPoint.BaseDefense = checked((int)values[8]); player.NPoint.BaseCriticalChance = checked((int)values[9]); player.NPoint.Hp = values[11]; player.NPoint.Mp = values[12]; player.NPoint.Damage = values[13]; player.NPoint.HpMax = values[14]; player.NPoint.MpMax = values[15];
    }
    private static bool PointMatches(Player player, string? source)
    {
        if (source is null) return false;
        long[] values = ReadNumbers(source, "data_point", 16);
        return values[0] == player.NPoint.LimitPower && values[1] == player.NPoint.Power && values[2] == player.NPoint.Potential && values[3] == player.NPoint.Stamina && values[4] == player.NPoint.MaxStamina && values[5] == player.NPoint.BaseHp && values[6] == player.NPoint.BaseMp && values[7] == player.NPoint.BaseDamage && values[8] == player.NPoint.BaseDefense && values[9] == player.NPoint.BaseCriticalChance && values[11] == player.NPoint.Hp && values[12] == player.NPoint.Mp && values[13] == player.NPoint.Damage && values[14] == player.NPoint.HpMax && values[15] == player.NPoint.MpMax;
    }
    private static long[] ReadNumbers(string json, string column, int minimumLength)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Array) throw new FormatException($"{column} must be a JSON array.");
        long[] values = document.RootElement.EnumerateArray().Select(static value => value.GetInt64()).ToArray();
        if (values.Length < minimumLength) throw new FormatException($"{column} has fewer than {minimumLength} values.");
        return values;
    }
    private static string WritePoint(Player player) => $"[{player.NPoint.LimitPower},{player.NPoint.Power},{player.NPoint.Potential},{player.NPoint.Stamina},{player.NPoint.MaxStamina},{player.NPoint.BaseHp},{player.NPoint.BaseMp},{player.NPoint.BaseDamage},{player.NPoint.BaseDefense},{player.NPoint.BaseCriticalChance},0,{player.NPoint.Hp},{player.NPoint.Mp},{player.NPoint.Damage},{player.NPoint.HpMax},{player.NPoint.MpMax}]";
    private static string NewPoint(sbyte gender)
    {
        int hp = gender == 0 ? 200 : 100; int mp = gender == 1 ? 200 : 100; int damage = gender == 2 ? 15 : 12;
        int max = gender == 0 ? 200 : 100;
        return $"[0,1200,1200,1000,1000,{hp},{mp},{damage},0,0,0,{hp},{mp},{damage},{max},{max}]";
    }
    private static string NewBodyItems(sbyte gender, long nowMs)
    {
        int defence = gender == 2 ? 3 : 2; long hp = gender == 0 ? 30 : 20;
        var slots = new List<string>(ConstPlayer.QTY_MAX_ITEM_BODY_PLAYER) { NewItemSlot(gender, 1, NewOptions(47, defence), nowMs), NewItemSlot(6 + gender, 1, NewOptions(6, hp), nowMs) };
        while (slots.Count < ConstPlayer.QTY_MAX_ITEM_BODY_PLAYER) slots.Add(NewItemSlot(-1, 0, "[]", nowMs));
        return JsonSerializer.Serialize(slots, JavaJson);
    }
    private static string NewBoxItems(long nowMs)
    {
        var slots = new List<string>(20) { NewItemSlot(12, 1, NewOptions(14, 1), nowMs) };
        while (slots.Count < 20) slots.Add(NewItemSlot(-1, 0, "[]", nowMs));
        return JsonSerializer.Serialize(slots, JavaJson);
    }
    private static string NewEmptyItemSlots(int count, long nowMs) => JsonSerializer.Serialize(Enumerable.Range(0, count).Select(_ => NewItemSlot(-1, 0, "[]", nowMs)), JavaJson);
    // PlayerDAO thêm JSON của từng option vào JSONArray nên mảng option là một chuỗi JSON.
    private static string NewOptions(int optionId, long parameter) => JsonSerializer.Serialize(new[] { $"[{optionId},{parameter}]" }, JavaJson);
    private static string NewItemSlot(int itemId, int quantity, string options, long nowMs) => $"[{itemId},{quantity},{JsonSerializer.Serialize(options, JavaJson)},{nowMs}]";
    private static string ZeroArray(int count) => $"[{string.Join(',', Enumerable.Repeat(0, count))}]";
    private static string RepeatedArray(long value, int count) => $"[{string.Join(',', Enumerable.Repeat(value, count))}]";
    private static string NestedRepeatedArray(string value, int count) => JsonSerializer.Serialize(Enumerable.Repeat(value, count), JavaJson);
    private static int InitialSkill(sbyte gender) => gender switch { 0 => 0, 1 => 2, _ => 4 };
    private static string NewSkills(sbyte gender)
    {
        int[] ids = gender switch { 0 => [0, 1, 6, 9, 10, 20, 22, 19, 24], 1 => [2, 3, 7, 11, 12, 17, 18, 19, 26], _ => [4, 5, 8, 13, 14, 21, 23, 19, 25] };
        return JsonSerializer.Serialize(ids.Select((id, index) => $"[{id},{(index == 0 ? 1 : 0)},0]"), JavaJson);
    }
    private static string NewSkillShortcut(sbyte gender) => $"[{InitialSkill(gender)},-1,-1,-1,-1,-1,-1,-1,-1,-1]";
}
