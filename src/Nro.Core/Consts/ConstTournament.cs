namespace Nro.Core.Consts;

// Hằng số Đại hội võ thuật (mỗi giờ một giải, đăng ký ở bản đồ 52, thi đấu ở bản đồ 51): hạng giải, mốc phút, câu thông báo, tên và phí từng giải
// Nguồn Java: consts/ConstTournament.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstTournament
{
    // Giải Nhi đồng (8h, 14h, 18h); các hạng giải là chỉ số của TOURNAMENT_NAMES, TOURNAMENT_GEMS, TOURNAMENT_GOLDS (Java: NHI_DONG)
    public const int JUNIOR = 0;

    // Giải Siêu cấp 1 (9h, 13h, 19h) (Java: SIEU_CAP_1)
    public const int SUPER_CLASS_1 = 1;

    // Giải Siêu cấp 2 (10h, 15h, 20h) (Java: SIEU_CAP_2)
    public const int SUPER_CLASS_2 = 2;

    // Giải Siêu cấp 3 (11h, 16h, 21h) (Java: SIEU_CAP_3)
    public const int SUPER_CLASS_3 = 3;

    // Giải Ngoại hạng (12h, 17h, 22h, 23h), phí đăng ký bằng vàng (Java: NGOAI_HANG)
    public const int PREMIER = 4;

    // Chỉ được đăng ký khi phút hiện tại trong giờ nhỏ hơn mốc này
    public const int MINS_MAX_CAN_REG = 25;

    // Phút trong giờ bắt đầu thi đấu (giải diễn ra lúc XXh30)
    public const int MINS_START = 30;

    // Quá phút này trong giờ thì kết thúc mọi trận và đặt lại vòng đấu
    public const int MINS_END = 57;

    // Gửi người đã đăng ký nhưng không có mặt ở bản đồ 52 khi giải bắt đầu, bị loại khỏi danh sách (Java: TEXT_TRUAT_QUYEN)
    public const string TEXT_DISQUALIFIED = "Những cái loại ngủ quá giờ trưa ý, tao đố thằng nào giàu được.";

    // Gửi người bị lẻ cặp khi xếp cặp, được đi thẳng vào vòng sau (Java: TEXT_DOI_THU_BO_CUOC)
    public const string TEXT_OPPONENT_GAVE_UP = "Bạn đã thắng vì đối thủ đã bỏ cuộc, chờ tại đây để thi đấu vòng tiếp theo";

    // Trọng tài nói khi một người rơi khỏi võ đài; %1 là tên người thắng (Java: TEXT_NPC_CHAT_ROI_DAI)
    public const string TEXT_NPC_CHAT_RING_OUT = "Đối thủ đã rơi khỏi võ đài, %1 đã thắng";

    // Hiện khi đăng ký thành công; %1 là giờ của giải, %2 là giờ phút hiện tại (Java: TEXT_DANG_KY_THANH_CONG)
    public const string TEXT_REGISTER_SUCCESS = "Bạn đã đăng ký thành công, nhớ có mặt tại đây trước %1h30\bBây giờ là %2, đến trễ coi như bỏ cuộc";

    // Thông báo cho người thua khi bị đưa ra khỏi võ đài (Java: TEXT_CHIA_BUON)
    public const string TEXT_CONDOLENCE = "Xin lỗi vì quá đẹp trai";

    // Gửi người thắng khi đối thủ rời bản đồ võ đài giữa trận (Java: TEXT_DOI_THU_BO_CUOC_ROI_MAP)
    public const string TEXT_OPPONENT_LEFT_MAP = "Đối thủ bỏ cuộc, bạn đã chiến thắng";

    // Gửi người rời bản đồ võ đài giữa trận, bị xử thua (Java: TEXT_XU_THUA_BO_CHAY)
    public const string TEXT_LOSE_BY_FLEEING = "Bạn bị xử thua vì đã bỏ chạy";

    // Trọng tài nói khi một người rời bản đồ võ đài; %1 là tên người thắng (Java: TEXT_NPC_CHAT_DOI_THU_BO_CUOC_ROI_MAP)
    public const string TEXT_NPC_CHAT_OPPONENT_LEFT_MAP = "Đối thủ bỏ cuộc %1 đã thắng";

    // Gửi người thắng khi đối thủ kiệt sức (chết) (Java: TEXT_DOI_THU_KIET_SUC)
    public const string TEXT_OPPONENT_EXHAUSTED = "Đối thủ đã kiệt sức, bạn đã thắng";

    // Dành cho người thua vì kiệt sức; Java không dùng (chuỗi giữ chỗ) (Java: TEXT_XU_THUA_KIET_SUC)
    public const string TEXT_LOSE_BY_EXHAUSTION = ".....";

    // Trọng tài nói khi một người kiệt sức; %1 là tên người thắng (Java: TEXT_NPC_CHAT_DOI_THU_KIET_SUC)
    public const string TEXT_NPC_CHAT_OPPONENT_EXHAUSTED = "Đối thủ đã kiệt sức, %1 đã thắng";

    // Dành cho người thắng khi hết giờ; Java không dùng (chuỗi giữ chỗ) (Java: TEXT_HET_GIO)
    public const string TEXT_TIME_UP = ".....";

    // Dành cho người thua khi hết giờ; Java không dùng (chuỗi giữ chỗ) (Java: TEXT_XU_THUA_HET_GIO)
    public const string TEXT_LOSE_BY_TIME_UP = ".....";

    // Trọng tài nói khi hết giờ thi đấu; %1 là tên người thắng (Java: TEXT_NPC_CHAT_HET_GIO)
    public const string TEXT_NPC_CHAT_TIME_UP = "Hết giờ thi đấu %1 đã chiến thắng vì quá đẹp trai";

    // Hiện khi hủy đăng ký (chọn đăng ký lần nữa khi đã có tên) (Java: TEXT_HUY_DANG_KY)
    public const string TEXT_REGISTRATION_CANCELLED = "Bạn đã hủy đăng ký thành công";

    // Thông báo cho người thắng trận khi được đưa về chờ vòng sau (Java: TEXT_THANG_VONG_NAY)
    public const string TEXT_WON_THIS_ROUND = "Bạn đã thắng vòng này, xin chờ tại đây ít phút để thi đấu vòng sau";

    // Hiện khi người vừa vô địch giải gần đây định đăng ký tiếp (Java: TEXT_DA_VO_DICH)
    public const string TEXT_ALREADY_CHAMPION = "Bạn đã vô địch giải gần đây, vui lòng đợi giải sau";

    // Gửi nhà vô địch khi giải kết thúc (Java: TEXT_VO_DICH)
    public const string TEXT_CHAMPION = "Bạn đã vô địch giải đấu, xin chúc mừng bạn, bạn được thưởng 5 viên đá nâng cấp và 30 thỏi vàng";

    // Thông báo cho mọi người chơi khác khi có người vô địch; %1 là tên, %2 là tên giải (Java: TEXT_KHOE_VO_DICH)
    public const string TEXT_CHAMPION_ANNOUNCEMENT = "Chúc mừng %1 vừa vô địch giải %2";

    // Báo chỉ được dự giải Ngoại hạng và Nhi đồng; Java không dùng (Java: TEXT_CHI_CO_THE_THAM_GIA_GIAI_NHI_DONG)
    public const string TEXT_CAN_ONLY_JOIN_JUNIOR = "Bạn chỉ có thể tham gia giải Ngoại hạng và Nhi Đồng";

    // Báo chỉ được dự giải Ngoại hạng và Siêu cấp 1; Java không dùng (Java: TEXT_CHI_CO_THE_THAM_GIA_GIAI_SIEU_CAP_1)
    public const string TEXT_CAN_ONLY_JOIN_SUPER_CLASS_1 = "Bạn chỉ có thể tham gia giải Ngoại hạng và Siêu cấp 1";

    // Báo chỉ được dự giải Ngoại hạng và Siêu cấp 2; Java không dùng (Java: TEXT_CHI_CO_THE_THAM_GIA_GIAI_SIEU_CAP_2)
    public const string TEXT_CAN_ONLY_JOIN_SUPER_CLASS_2 = "Bạn chỉ có thể tham gia giải Ngoại hạng và Siêu cấp 2";

    // Báo chỉ được dự giải Ngoại hạng và Siêu cấp 3; Java không dùng (Java: TEXT_CHI_CO_THE_THAM_GIA_GIAI_SIEU_CAP_3)
    public const string TEXT_CAN_ONLY_JOIN_SUPER_CLASS_3 = "Bạn chỉ có thể tham gia giải Ngoại hạng và Siêu cấp 3";

    // Báo chỉ được dự giải Ngoại hạng; Java không dùng (Java: TEXT_CHI_CO_THE_THAM_GIA_GIAI_NGOAI_HANG)
    public const string TEXT_CAN_ONLY_JOIN_PREMIER = "Bạn chỉ có thể tham gia giải Ngoại hạng";

    // Tên từng giải, chỉ số là hạng giải (JUNIOR đến PREMIER) (Java: tournamentNames)
    public static readonly string[] TOURNAMENT_NAMES = new string[] { "Nhi đồng", "Siêu cấp 1", "Siêu cấp 2", "Siêu cấp 3", "Ngoại hạng" };

    // Phí đăng ký bằng ngọc theo từng giải; cũng là số ghi trong thông báo thưởng khi thắng trận (Java: tournamentGems)
    public static readonly int[] TOURNAMENT_GEMS = new int[] { 2, 4, 6, 8, 0 };

    // Phí đăng ký bằng vàng theo từng giải (chỉ Ngoại hạng: 10.000 vàng) (Java: tournamentGolds)
    public static readonly int[] TOURNAMENT_GOLDS = new int[] { 0, 0, 0, 0, 10000 };
}
