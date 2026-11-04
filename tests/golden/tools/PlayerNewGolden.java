import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Path;
import java.util.LinkedHashMap;
import java.util.Map;
import org.json.simple.JSONArray;
import org.json.simple.JSONObject;

/**
 * Reproducible, database-free extraction of PlayerDAO.createNewPlayer (lines
 * 57-532).  Its string-building statements retain the source order and values;
 * every wall-clock expression is replaced by FIXED_EPOCH_MS so the oracle is
 * deterministic.  The final map is the 47 INSERT arguments, with the two
 * Timestamp arguments represented as epoch milliseconds for JSON.
 */
public final class PlayerNewGolden {
    private static final long FIXED_EPOCH_MS = 1_700_000_000_000L;
    private static final int QTY_MAX_ITEM_BODY_PLAYER = 12; // ConstPlayer.QTY_MAX_ITEM_BODY_PLAYER

    private PlayerNewGolden() { }

    public static void main(String[] args) throws IOException {
        if (args.length != 1) {
            throw new IllegalArgumentException("Usage: PlayerNewGolden <output-directory>");
        }
        Path output = Path.of(args[0]);
        Files.createDirectories(output);
        for (byte gender = 0; gender <= 2; gender++) {
            String json = JSONObject.toJSONString(build(gender));
            Files.writeString(output.resolve("player-new-gender" + gender + ".json"), json, StandardCharsets.UTF_8);
        }
    }

    // PlayerDAO.java:40-55, with System.currentTimeMillis() fixed.
    @SuppressWarnings("unchecked")
    private static String createEmptyItemSlots(int size) {
        JSONArray dataArray = new JSONArray();
        JSONArray item = new JSONArray();
        JSONArray options = new JSONArray();
        for (int i = 0; i < size; i++) {
            item.add(-1);
            item.add(0);
            item.add(options.toJSONString());
            item.add(FIXED_EPOCH_MS);
            dataArray.add(item.toJSONString());
            item.clear();
        }
        return dataArray.toJSONString();
    }

    // PlayerDAO.createNewPlayer:57-532.  DB UPDATE/INSERT calls are replaced by the returned ordered arguments.
    @SuppressWarnings("unchecked")
    private static Map<String, Object> build(byte gender) {
        int userId = 73;
        String name = "gender" + gender;
        int hair = 102 + gender;
        JSONArray dataArray = new JSONArray();

        dataArray.add(2000); // vàng
        dataArray.add(99999999); // ngọc xanh
        dataArray.add(99999999); // hồng ngọc
        dataArray.add(0); // point_EXP
        String inventory = dataArray.toJSONString();
        dataArray.clear();

        dataArray.add(39 + gender); // map
        dataArray.add(100); // x
        dataArray.add(384); // y
        String location = dataArray.toJSONString();
        dataArray.clear();

        dataArray.add(0); dataArray.add(1200); dataArray.add(1200); dataArray.add(1000); dataArray.add(1000);
        dataArray.add(gender == 0 ? 200 : 100); dataArray.add(gender == 1 ? 200 : 100);
        dataArray.add(gender == 2 ? 15 : 12); dataArray.add(0); dataArray.add(0); dataArray.add(0);
        dataArray.add(gender == 0 ? 200 : 100); dataArray.add(gender == 1 ? 200 : 100);
        dataArray.add(gender == 2 ? 15 : 12); dataArray.add(gender == 0 ? 200 : 100);
        dataArray.add(gender == 0 ? 200 : 100);
        String point = dataArray.toJSONString();
        dataArray.clear();

        dataArray.add(1); dataArray.add(5); dataArray.add(0); dataArray.add(FIXED_EPOCH_MS); dataArray.add(FIXED_EPOCH_MS);
        String magicTree = dataArray.toJSONString();
        dataArray.clear();

        int idAo = gender == 0 ? 0 : gender == 1 ? 1 : 2;
        int idQuan = gender == 0 ? 6 : gender == 1 ? 7 : 8;
        int def = gender == 2 ? 3 : 2;
        long hp = gender == 0 ? 30 : 20;
        JSONArray item = new JSONArray();
        JSONArray options = new JSONArray();
        JSONArray opt = new JSONArray();
        for (int i = 0; i < QTY_MAX_ITEM_BODY_PLAYER; i++) {
            switch (i) {
                case 0:
                    opt.add(47); opt.add(def); item.add(idAo); item.add(1); options.add(opt.toJSONString()); opt.clear(); break;
                case 1:
                    opt.add(6); opt.add(hp); item.add(idQuan); item.add(1); options.add(opt.toJSONString()); opt.clear(); break;
                default:
                    item.add(-1); item.add(0); break;
            }
            item.add(options.toJSONString());
            item.add(FIXED_EPOCH_MS);
            dataArray.add(item.toJSONString());
            options.clear(); item.clear();
        }
        String itemsBody = dataArray.toJSONString();
        dataArray.clear();

        for (int i = 0; i < 20; i++) {
            item.add(-1); item.add(0); item.add(options.toJSONString()); item.add(FIXED_EPOCH_MS);
            dataArray.add(item.toJSONString()); options.clear(); item.clear();
        }
        String itemsBag = dataArray.toJSONString();
        dataArray.clear();

        for (int i = 0; i < 20; i++) {
            if (i == 0) {
                opt.add(14); opt.add(1); item.add(12); item.add(1); options.add(opt.toJSONString()); opt.clear();
            } else { item.add(-1); item.add(0); }
            item.add(options.toJSONString()); item.add(FIXED_EPOCH_MS); dataArray.add(item.toJSONString()); options.clear(); item.clear();
        }
        String itemsBox = dataArray.toJSONString();
        dataArray.clear();
        String itemsBoxCollection = createEmptyItemSlots(20);
        String itemsBoxLuckyRound = createEmptyItemSlots(110);
        String itemsDaBan = createEmptyItemSlots(20);
        String itemsMailBox = createEmptyItemSlots(200);
        String friends = dataArray.toJSONString();
        String enemies = dataArray.toJSONString();

        addZeros(dataArray, 8); String intrinsic = dataArray.toJSONString(); dataArray.clear();
        addZeros(dataArray, 79); String itemTime = dataArray.toJSONString(); dataArray.clear();
        dataArray.add(0); dataArray.add(0); dataArray.add(0); String task = dataArray.toJSONString(); dataArray.clear();
        String mabuEgg = dataArray.toJSONString();
        for (int i = 0; i < 21; i++) { dataArray.add(FIXED_EPOCH_MS); }
        String charms = dataArray.toJSONString(); dataArray.clear();

        int[] skillsArr = gender == 0 ? new int[]{0, 1, 6, 9, 10, 20, 22, 19, 24}
                : gender == 1 ? new int[]{2, 3, 7, 11, 12, 17, 18, 19, 26}
                : new int[]{4, 5, 8, 13, 14, 21, 23, 19, 25};
        JSONArray skill = new JSONArray();
        for (int i = 0; i < skillsArr.length; i++) {
            skill.add(skillsArr[i]); skill.add(i == 0 ? 1 : 0); skill.add(0); dataArray.add(skill.toString()); skill.clear();
        }
        String skills = dataArray.toJSONString(); dataArray.clear();

        dataArray.add(gender == 0 ? 0 : gender == 1 ? 2 : 4);
        for (int i = 0; i < 9; i++) { dataArray.add(-1); }
        String skillsShortcut = dataArray.toJSONString(); dataArray.clear();
        String petData = dataArray.toJSONString(); String myFather = dataArray.toJSONString(); String myMother = dataArray.toJSONString();
        JSONArray blackBall = new JSONArray();
        for (int i = 1; i <= 7; i++) { blackBall.add(0); blackBall.add(0); blackBall.add(0); dataArray.add(blackBall.toJSONString()); blackBall.clear(); }
        String dataBlackBall = dataArray.toString(); dataArray.clear();
        dataArray.add(-1); dataArray.add(0); dataArray.add(0); dataArray.add(0); dataArray.add(20); dataArray.add(0);
        String dataSideTask = dataArray.toJSONString(); dataArray.clear();
        addZeros(dataArray, 44); String dataEventModels = dataArray.toJSONString(); dataArray.clear();
        String thanhTichBang = ""; String thanhTichBang2 = ""; String thanhTichBang3 = "";
        String billData = dataArray.toJSONString(); String trongDuaHau = dataArray.toJSONString(); String dataNguoiYeu = "[]";
        String dataBadges = "[]"; String dataTaskBadges = "[]"; String dailyGift = "[]"; String dataLuyenTap = "[]";
        dataArray.add(0); dataArray.add(0); String kolTask = dataArray.toJSONString(); dataArray.clear();
        dataArray.add(-1); dataArray.add(0); dataArray.add(0); dataArray.add(0); dataArray.add(20); dataArray.add(0);
        String eventTask = dataArray.toJSONString(); dataArray.clear();
        dataArray.add(gender == 0 ? 0 : gender == 1 ? 2 : 4); String dataBoughtSkill = dataArray.toJSONString(); dataArray.clear();

        // PlayerDAO.java:483-524 INSERT parameter order, preserving the Java column names normalized by the C# schema.
        Map<String, Object> row = new LinkedHashMap<>();
        row.put("account_id", userId); row.put("name", name); row.put("head", hair); row.put("gender", (int) gender); row.put("have_tennis_space_ship", 0); row.put("clan_id", -1);
        row.put("data_inventory", inventory); row.put("data_location", location); row.put("data_point", point); row.put("data_magic_tree", magicTree);
        row.put("items_body", itemsBody); row.put("items_bag", itemsBag); row.put("items_box", itemsBox); row.put("items_box_collection", itemsBoxCollection); row.put("items_box_lucky_round", itemsBoxLuckyRound);
        row.put("items_daban", itemsDaBan); row.put("item_mails_box", itemsMailBox); row.put("friends", friends); row.put("enemies", enemies); row.put("data_intrinsic", intrinsic); row.put("data_item_time", itemTime);
        row.put("data_task", task); row.put("data_mabu_egg", mabuEgg); row.put("data_charm", charms); row.put("skills", skills); row.put("skills_shortcut", skillsShortcut); row.put("data_luyentap", dataLuyenTap);
        row.put("pet", petData); row.put("my_father", myFather); row.put("my_mother", myMother); row.put("data_black_ball", dataBlackBall); row.put("data_side_task", dataSideTask); row.put("data_kol_task", kolTask); row.put("data_event_task", eventTask);
        row.put("bill_data", billData); row.put("data_trong_dua_hau", trongDuaHau); row.put("data_nguoi_yeu", dataNguoiYeu); row.put("lastimelogin_epoch_ms", FIXED_EPOCH_MS); row.put("last_time_login_game_epoch_ms", FIXED_EPOCH_MS);
        row.put("thanh_tich_bang", thanhTichBang); row.put("thanh_tich_bang2", thanhTichBang2); row.put("thanh_tich_bang3", thanhTichBang3); row.put("bought_skill", dataBoughtSkill); row.put("data_badges", dataBadges); row.put("data_task_badges", dataTaskBadges);
        row.put("daily_gift", dailyGift); row.put("data_event_models", dataEventModels);
        return row;
    }

    @SuppressWarnings("unchecked")
    private static void addZeros(JSONArray array, int count) {
        for (int i = 0; i < count; i++) { array.add(0); }
    }
}
