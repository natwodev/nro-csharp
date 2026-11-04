namespace Nro.Data.Models.Players;

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
