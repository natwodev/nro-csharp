namespace Nro.Core.Consts;

/// <summary>
/// Port of <c>consts/ConstTournament.java</c>: class and constant names are preserved; values were machine-translated from Java
/// (port-csharp/tools/consts/gen_consts_cs.py), not copied by hand.
/// Automatically verified against the real values read from the compiled Java class: <c>ConstsParityTests</c> (tests/golden/consts.json).
/// </summary>
public static class ConstTournament
{
    public const int NHI_DONG = 0;
    public const int SIEU_CAP_1 = 1;
    public const int SIEU_CAP_2 = 2;
    public const int SIEU_CAP_3 = 3;
    public const int NGOAI_HANG = 4;

    public const int MINS_MAX_CAN_REG = 25;
    public const int MINS_START = 30;
    public const int MINS_END = 57;

    public const string TEXT_TRUAT_QUYEN = "Những cái loại ngủ quá giờ trưa ý, tao đố thằng nào giàu được.";
    public const string TEXT_DOI_THU_BO_CUOC = "Bạn đã thắng vì đối thủ đã bỏ cuộc, chờ tại đây để thi đấu vòng tiếp theo";
    public const string TEXT_NPC_CHAT_ROI_DAI = "Đối thủ đã rơi khỏi võ đài, %1 đã thắng";
    public const string TEXT_DANG_KY_THANH_CONG = "Bạn đã đăng ký thành công, nhớ có mặt tại đây trước %1h30\bBây giờ là %2, đến trễ coi như bỏ cuộc";
    public const string TEXT_CHIA_BUON = "Xin lỗi vì quá đẹp trai";
    public const string TEXT_DOI_THU_BO_CUOC_ROI_MAP = "Đối thủ bỏ cuộc, bạn đã chiến thắng";
    public const string TEXT_XU_THUA_BO_CHAY = "Bạn bị xử thua vì đã bỏ chạy";
    public const string TEXT_NPC_CHAT_DOI_THU_BO_CUOC_ROI_MAP = "Đối thủ bỏ cuộc %1 đã thắng";
    public const string TEXT_DOI_THU_KIET_SUC = "Đối thủ đã kiệt sức, bạn đã thắng";
    public const string TEXT_XU_THUA_KIET_SUC = ".....";
    public const string TEXT_NPC_CHAT_DOI_THU_KIET_SUC = "Đối thủ đã kiệt sức, %1 đã thắng";
    public const string TEXT_HET_GIO = ".....";
    public const string TEXT_XU_THUA_HET_GIO = ".....";
    public const string TEXT_NPC_CHAT_HET_GIO = "Hết giờ thi đấu %1 đã chiến thắng vì quá đẹp trai";
    public const string TEXT_HUY_DANG_KY = "Bạn đã hủy đăng ký thành công";
    public const string TEXT_THANG_VONG_NAY = "Bạn đã thắng vòng này, xin chờ tại đây ít phút để thi đấu vòng sau";
    public const string TEXT_DA_VO_DICH = "Bạn đã vô địch giải gần đây, vui lòng đợi giải sau";
    public const string TEXT_VO_DICH = "Bạn đã vô địch giải đấu, xin chúc mừng bạn, bạn được thưởng 5 viên đá nâng cấp và 30 thỏi vàng";
    public const string TEXT_KHOE_VO_DICH = "Chúc mừng %1 vừa vô địch giải %2";
    public const string TEXT_CHI_CO_THE_THAM_GIA_GIAI_NHI_DONG = "Bạn chỉ có thể tham gia giải Ngoại hạng và Nhi Đồng";
    public const string TEXT_CHI_CO_THE_THAM_GIA_GIAI_SIEU_CAP_1 = "Bạn chỉ có thể tham gia giải Ngoại hạng và Siêu cấp 1";
    public const string TEXT_CHI_CO_THE_THAM_GIA_GIAI_SIEU_CAP_2 = "Bạn chỉ có thể tham gia giải Ngoại hạng và Siêu cấp 2";
    public const string TEXT_CHI_CO_THE_THAM_GIA_GIAI_SIEU_CAP_3 = "Bạn chỉ có thể tham gia giải Ngoại hạng và Siêu cấp 3";
    public const string TEXT_CHI_CO_THE_THAM_GIA_GIAI_NGOAI_HANG = "Bạn chỉ có thể tham gia giải Ngoại hạng";

    public static readonly string[] tournamentNames = new string[] { "Nhi đồng", "Siêu cấp 1", "Siêu cấp 2", "Siêu cấp 3", "Ngoại hạng" };
    public static readonly int[] tournamentGems = new int[] { 2, 4, 6, 8, 0 };
    public static readonly int[] tournamentGolds = new int[] { 0, 0, 0, 0, 10000 };
}
