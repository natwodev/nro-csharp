namespace Nro.Core.Consts;

// Hằng số người chơi: hành tinh (gender), kiểu PK, kiểu hợp thể, số ô trang bị/kỹ năng, id đầu khỉ và bộ kỹ năng theo hành tinh
// Nguồn Java: consts/ConstPlayer.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstPlayer
{
    // Id đầu (head) khi biến khỉ, chỉ số = cấp khỉ - 1 (Java: HEADMONKEY)
    public static readonly int[] MONKEY_HEADS = new int[] { 192, 195, 196, 199, 197, 200, 198 };

    // Id đầu chọn ngẫu nhiên khi đang dùng vật phẩm Đuôi Khỉ, cùng giá trị với MONKEY_HEADS (Java: DUOIHEADMONKEY)
    public static readonly int[] MONKEY_TAIL_HEADS = new int[] { 192, 195, 196, 199, 197, 200, 198 };

    // Hành tinh Trái Đất (giá trị gender của người chơi, đệ tử, boss) (Java: TRAI_DAT)
    public const sbyte EARTHLING = 0;

    // Hành tinh Namếc (giá trị gender) (Java: NAMEC)
    public const sbyte NAMEKIAN = 1;

    // Hành tinh Xayda (giá trị gender) (Java: XAYDA)
    public const sbyte SAIYAN = 2;

    // Kiểu PK: không PK, không đánh được người chơi khác
    public const sbyte NON_PK = 0;

    // Kiểu PK: đấu đối kháng (thách đấu, Đại hội võ thuật, Giải Siêu Hạng...)
    public const sbyte PK_PVP = 3;

    // Kiểu PK: đấu luyện tập (LuyenTap); hai bên cùng kiểu này đánh nhau được cộng tiềm năng sức mạnh
    public const sbyte PK_PVP_2 = 4;

    // Kiểu PK: tàn sát, đánh được tất cả (boss chuyển sang kiểu này khi bắt đầu tấn công)
    public const sbyte PK_ALL = 5;

    // Kiểu hợp thể: không hợp thể
    public const sbyte NON_FUSION = 0;

    // Kiểu hợp thể: Lưỡng long nhất thể, hợp thể với đệ tử có giới hạn thời gian (Java: LUONG_LONG_NHAT_THE)
    public const sbyte FUSION_DANCE = 4;

    // Kiểu hợp thể: dùng Bông tai Porata (item 454) (Java: HOP_THE_PORATA)
    public const sbyte PORATA_FUSION = 6;

    // Kiểu hợp thể: Porata cấp 2 (item 921), cộng chỉ số cao hơn (Java: HOP_THE_PORATA2)
    public const sbyte PORATA_FUSION_2 = 8;

    // Kiểu hợp thể: Porata cấp 3 (item 1943) (Java: HOP_THE_PORATA3)
    public const sbyte PORATA_FUSION_3 = 10;

    // Kiểu hợp thể: Porata cấp 4 (item 2105) (Java: HOP_THE_PORATA4)
    public const sbyte PORATA_FUSION_4 = 12;

    // Kiểu hợp thể: Porata cấp 5 (item 2106) (Java: HOP_THE_PORATA5)
    public const sbyte PORATA_FUSION_5 = 14;

    // Số ô trang bị trên người của người chơi
    public const int QTY_MAX_ITEM_BODY_PLAYER = 12;

    // Số ô trang bị trên người của đệ tử
    public const int QTY_MAX_ITEM_BODY_PET = 7;

    // Số ô kỹ năng của đệ tử (đệ Ubu, Kid Jiren, Kid Bill, Black Goku được thêm 1)
    public const int QTY_MAX_SKILL_PET = 3;

    // Id kỹ năng của phái Trái Đất; khi nạp người chơi, chiêu nào còn thiếu được thêm ở cấp 0 (Java: SKILL_TD)
    public static readonly int[] SKILL_EARTHLING = new int[] { 0, 1, 6, 20, 10, 24, 19 };

    // Id kỹ năng của phái Namếc, dùng như SKILL_EARTHLING (Java: SKILL_NAMEC)
    public static readonly int[] SKILL_NAMEKIAN = new int[] { 17, 3, 12, 11, 18, 26, 19 };

    // Id kỹ năng của phái Xayda, dùng như SKILL_EARTHLING (Java: SKILL_XAYDA)
    public static readonly int[] SKILL_SAIYAN = new int[] { 4, 5, 8, 13, 22, 25, 19 };
}
