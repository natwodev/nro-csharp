namespace Nro.Core.Consts;

// Hằng số map: loại map (cột type trong bảng map_template), kiểu bảng chuyển map và loại ô dùng khi dựng map
// Nguồn Java: consts/ConstMap.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstMap
{
    // Loại ô (tile) mặt trên đứng được: khi dựng map, đọc danh sách chỉ số ô thuộc loại này làm tileTop
    public const int TILE_TOP = 2;

    // Loại map thường
    public const sbyte MAP_NORMAL = 0;

    // Loại map offline: chỉ có 1 khu, như Thần điện, Tháp Karin, Hành tinh Kaio
    public const sbyte MAP_OFFLINE = 1;

    // Loại map Doanh trại độc nhãn (RedRibbonHQ) (Java: MAP_DOANH_TRAI)
    public const sbyte MAP_RED_RIBBON_HQ = 2;

    // Loại map Ngọc rồng sao đen: các hành tinh M-2, Polaris, Cretaceous...
    public const sbyte MAP_BLACK_BALL_WAR = 3;

    // Loại map Bản đồ kho báu (TreasureUnderSea): Động hải tặc, Hang Bạch Tuộc, Động kho báu, Cảng hải tặc (Java: MAP_BAN_DO_KHO_BAU)
    public const sbyte MAP_TREASURE_UNDER_SEA = 4;

    // Loại map Mabư 12h (MajinBuu12H); dữ liệu hiện không có map nào loại này (Java: MAP_MA_BU_12H)
    public const sbyte MAP_MAJIN_BUU_12H = 5;

    // Loại map Khí gas hủy diệt (DestronGas): Sa Mạc, Lâu đài Lychee, Thành Phố Santa... (Java: MAP_KHI_GAS_HUY_DIET)
    public const sbyte MAP_DESTRON_GAS = 6;

    // Loại map Đảo kho báu (suy từ tên; Java không dùng, dữ liệu không có map nào loại này) (Java: MAP_DAO_KHO_BAU)
    public const sbyte MAP_TREASURE_ISLAND = 7;

    // Loại map Con đường rắn độc (SnakeWay) (Java: MAP_CON_DUONG_RAN_DOC)
    public const sbyte MAP_SNAKE_WAY = 8;

    // Loại map Giải cứu Mị Nương: Lãnh Địa Sơn Tinh, Lãnh Địa Thuỷ Tinh, Rừng Rậm; Java không dùng (Java: MAP_GIAI_CUU_MI_NUONG)
    public const sbyte MAP_RESCUE_MI_NUONG = 9;

    // Loại map Hirudegarn (map 204); Java không dùng
    public const sbyte MAP_HIRUDEGARN = 10;

    // Loại map Mabư 14h (MajinBuu14H): Cổng Phi Thuyền, Bụng Mabư (Java: MAP_MABU_14H)
    public const sbyte MAP_MAJIN_BUU_14H = 11;

    // Loại map Tây Karin (map 146): nơi thử thách Siêu thánh thủy (SuperDivineWater) (Java: MAP_TAY_KARIN)
    public const sbyte MAP_WEST_KARIN = 12;

    // Loại map Lãnh địa Bang Hội: chỉ có 1 khu (Java: MAP_BANG_HOI)
    public const sbyte MAP_CLAN = 13;

    // Loại map Boss bang hội (BossOfTheGangs) (Java: MAP_BOSS_BANG_HOI)
    public const sbyte MAP_CLAN_BOSS = 14;

    // Kiểu bảng chuyển map: dùng Capsule bay đến map đã chọn
    public const int CHANGE_CAPSULE = 500;

    // Kiểu bảng chuyển map: chọn map Ngọc rồng sao đen, mở từ Rồng Omega
    public const int CHANGE_BLACK_BALL = 501;

    // Kiểu bảng chuyển map: chọn map Mabư; Java có xử lý nhưng không chỗ nào đặt kiểu này (Java: CHANGE_MAP_MA_BU)
    public const int CHANGE_MAP_MAJIN_BUU = 502;
}
