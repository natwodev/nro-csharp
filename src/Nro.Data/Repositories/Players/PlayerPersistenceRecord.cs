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
        "items_box_collection", "items_box_lucky_round", "items_daban", "item_mails_box", "friends", "enemies",
        "data_intrinsic", "data_item_time", "data_task", "data_mabu_egg", "data_charm", "skills", "skills_shortcut",
        "data_luyentap", "pet", "my_father", "my_mother", "last_time_drop_tail", "last_time_khi_su_kien",
        "data_black_ball", "data_side_task", "data_kol_task", "data_event_task", "create_time", "point_pvp", "data_card",
        "bill_data", "data_trong_dua_hau", "time_travel_duration", "whis_sparring", "isbienhinh", "check_band_account",
        "master_does_not_attack", "master_does_not_attack_bo", "master_does_not_attack_me", "notify", "bandokhobau", "barracks_state",
        "conduongrandoc", "data_clan_task", "data_achievement", "data_vip", "lasttimepkcommeson", "sieuthanthuy",
        "baovetaikhoan", "lastimelogin", "first_time_login", "last_time_login_game", "data_badges", "data_task_badges",
        "ruonggo", "vodaisinhtu", "dragon_christmas", "dragon_halloween", "rank", "data_super_rank", "data_item_event",
        "data_the_card", "data_diem_danh", "data_may_dam", "data_nguoi_yeu", "lover_does_not_attack",
        "conone_does_not_attack", "contwo_does_not_attack", "conthree_does_not_attack", "thanh_tich_bang", "thanh_tich_bang2",
        "thanh_tich_bang3", "daily_gift", "bought_skill", "learn_skill", "event_point", "event_point_boss", "event_point_nhs",
        "event_point_quai", "diem_huy_diet", "diem_chien_truong_namek", "diem_su_kien_tet", "diem_su_kien_giangsinh",
        "diem_su_kien_halloween", "diem_su_kien_8_3", "diem_su_kien_trungthu", "diem_su_kien_hung_vuong",
        "diem_su_kien_black_friday", "diem_su_kien_20_10", "data_event_new_year", "data_event_christ_mas", "data_event_vulan",
        "data_event_halloween", "data_event_8_3", "data_event_trung_thu", "data_event_hung_vuong", "data_event_black_friday",
        "data_event_models", "data_models", "hp_point_fusion", "mp_point_fusion", "point_fusion_damage", "last_time_update_task", "title",
    ];
}
