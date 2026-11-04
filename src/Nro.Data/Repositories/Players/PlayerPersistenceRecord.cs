using Nro.Data.Db;
using System.Collections.ObjectModel;

namespace Nro.Data.Repositories.Players;

// Biểu diễn đầy đủ, không mất dữ liệu của một dòng player trong cơ sở dữ liệu.
// Nguồn Java: jbcd/dao/PlayerDAO.java (các cột JSON giữ dạng văn bản để PostgreSQL jsonb hoặc System.Text.Json không đổi khoảng trắng và cách ghi số).
public sealed class PlayerPersistenceRecord
{
    private readonly IReadOnlyDictionary<string, object?> _values;
    private readonly IReadOnlySet<string> _changedColumns;

    // Tạo bản ghi từ đủ các cột vật lý của player; mọi cột trừ id được đánh dấu đã đổi.
    public PlayerPersistenceRecord(IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        var copy = new Dictionary<string, object?>(values, StringComparer.Ordinal);
        foreach (string column in PlayerPersistenceColumns.All)
        {
            if (!copy.ContainsKey(column))
            {
                throw new ArgumentException($"The player row is missing column '{column}'.", nameof(values));
            }
        }

        _values = new ReadOnlyDictionary<string, object?>(copy);
        _changedColumns = new HashSet<string>(PlayerPersistenceColumns.All.Where(static column => column != "id"), StringComparer.Ordinal);
    }

    private PlayerPersistenceRecord(IReadOnlyDictionary<string, object?> values, IReadOnlySet<string> changedColumns)
    {
        _values = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(values, StringComparer.Ordinal));
        _changedColumns = new HashSet<string>(changedColumns, StringComparer.Ordinal);
    }

    // Lấy tập giá trị cột theo tên.
    public IReadOnlyDictionary<string, object?> Values => _values;

    // Lấy các cột vật lý đã thay đổi kể từ lúc đọc từ PostgreSQL.
    public IReadOnlySet<string> ChangedColumns => _changedColumns;

    // Lấy id người chơi, yêu cầu giá trị Int32.
    public int Id => _values["id"] is int value ? value : throw new InvalidOperationException("Player id is not an Int32.");

    // Lấy tên người chơi.
    public string Name => (string)_values["name"]!;

    // Lấy giá trị cột theo tên, không có cột trả null.
    public object? this[string column] => _values.TryGetValue(column, out object? value) ? value : null;

    // Trả bản ghi mới với một cột được thay đổi; không cho phép cột ngoài danh sách player.
    public PlayerPersistenceRecord With(string column, object? value)
    {
        if (!PlayerPersistenceColumns.All.Contains(column, StringComparer.Ordinal))
        {
            throw new ArgumentOutOfRangeException(nameof(column), column, "Unknown player column.");
        }

        var copy = new Dictionary<string, object?>(_values, StringComparer.Ordinal) { [column] = value };
        var changed = new HashSet<string>(_changedColumns, StringComparer.Ordinal);
        if (column != "id")
        {
            changed.Add(column);
        }

        return new PlayerPersistenceRecord(copy, changed);
    }

    // Tạo bản ghi vừa đọc từ cơ sở dữ liệu, chưa có cột nào thay đổi.
    internal static PlayerPersistenceRecord FromDatabase(IReadOnlyDictionary<string, object?> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return new PlayerPersistenceRecord(values, new HashSet<string>(StringComparer.Ordinal));
    }
}

// Khai báo tên và thứ tự các cột vật lý của player.
// Nguồn Java: jbcd/dao/PlayerDAO.java (thứ tự 114 cột được bảo toàn để đọc và ghi không mất dữ liệu).
public static class PlayerPersistenceColumns
{
    // Danh sách đầy đủ 114 cột vật lý theo đúng thứ tự cơ sở dữ liệu.
    public static readonly string[] All =
    [
        "id", "account_id", "name", "head", "gender", "have_tennis_space_ship", "clan_id",
        "data_inventory", "data_location", "data_point", "data_magic_tree", "items_body", "items_bag", "items_box",
        "items_box_collection", "items_box_lucky_round", "sold_items", "item_mails_box", "friends", "enemies",
        "data_intrinsic", "data_item_time", "data_task", "data_mabu_egg", "data_charm", "skills", "skills_shortcut",
        "training_data", "pet", "my_father", "my_mother", "last_time_drop_tail", "last_event_monkey_transformation_at",
        "data_black_ball", "data_side_task", "data_kol_task", "data_event_task", "create_time", "point_pvp", "data_card",
        "bill_data", "watermelon_plant_data", "time_travel_duration", "whis_sparring", "is_transformed", "account_ban_check_data",
        "master_does_not_attack", "father_does_not_attack", "mother_does_not_attack", "notify", "treasure_under_sea_uses", "barracks_state",
        "snake_way_data", "data_clan_task", "data_achievement", "data_vip", "last_commeson_boss_fight_at", "super_divine_water_state",
        "account_protection_data", "initial_login_at", "first_time_login", "last_time_login_game", "data_badges", "data_task_badges",
        "wood_chest_data", "deathmatch_arena_data", "dragon_christmas", "dragon_halloween", "rank", "data_super_rank", "data_item_event",
        "card_data", "check_in_data", "punching_machine_data", "lover_data", "lover_does_not_attack",
        "first_child_does_not_attack", "second_child_does_not_attack", "third_child_does_not_attack", "clan_treasure_under_sea_achievement", "clan_destron_gas_achievement",
        "clan_snake_way_achievement", "daily_gift", "bought_skill", "learn_skill", "event_point", "event_point_boss", "ngu_hanh_son_event_points",
        "monster_hunt_event_points", "destruction_points", "namek_battlefield_points", "lunar_new_year_event_points", "christmas_event_points",
        "halloween_event_points", "international_womens_day_event_points", "moon_festival_event_points", "hung_king_event_points",
        "black_friday_event_points", "vietnamese_womens_day_event_points", "data_event_new_year", "christmas_event_data", "vu_lan_event_data",
        "data_event_halloween", "international_womens_day_event_data", "moon_festival_event_data", "hung_king_event_data", "data_event_black_friday",
        "daily_gift_activity_data", "daily_counter_data", "hp_point_fusion", "mp_point_fusion", "point_fusion_damage", "last_time_update_task", "title",
    ];
}
