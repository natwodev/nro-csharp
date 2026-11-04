namespace Nro.Core.Consts;

// Hằng số chỉ số menu NPC (indexMenu truyền vào createOtherMenu): menu riêng của Bà Hạt Mít và Đại Thiên Sứ
// Nguồn Java: consts/ConstMenu.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstMenu
{
    // Bà Hạt Mít: menu pha lê gồm Ép sao trang bị, Pha lê hoá trang bị (Java: MENU_PHA_LE)
    public const int MENU_CRYSTAL = 251003;

    // Bà Hạt Mít: chuyển hoá trang bị dùng vàng hoặc ngọc; Java có xử lý nhưng không chỗ nào mở menu này (Java: MENU_CHUYEN_HOA_TRANG_BI)
    public const int MENU_CONVERT_EQUIPMENT = 251004;

    // Bà Hạt Mít: xác nhận pha lê hoá trang bị bằng ngọc (Java: MENU_PHA_LE_HOA_TRANG_BI)
    public const int MENU_CRYSTALLIZE_EQUIPMENT = 251005;

    // Bà Hạt Mít: nâng đồ Hủy diệt, nâng trang bị Kích hoạt thường hoặc VIP (Java: MENU_NANG_CAP_TRANG_BI)
    public const int MENU_UPGRADE_EQUIPMENT = 251006;

    // Bà Hạt Mít: chức năng khác gồm Build đồ, Pháp sư hoá, Siêu hoá cải trang, Tinh ấn trang bị, Nâng cấp giáp luyện tập (Java: CHUC_NANG_BHM_KHAC)
    public const int OTHER_FUNCTIONS_BHM = 251007;

    // Bà Hạt Mít: build đồ gồm Gia hạn vật phẩm, Tẩy đồ (Java: BUILD_DO_BHM)
    public const int BUILD_ITEMS_BHM = 251008;

    // Bà Hạt Mít: cửa hàng gồm nâng cấp chân mệnh và hai shop SHOP_BHM, SHOP_THOI_BHM; Java có xử lý nhưng không chỗ nào mở menu này
    public const int SHOP_BHM = 251009;

    // Bà Hạt Mít: khi có Bông tai Porata cấp 2, chọn nâng lên cấp 3 hoặc mở chỉ số (Java: MENU_BONG_TAI)
    public const int MENU_EARRING = 250010;

    // Đại Thiên Sứ: menu hiển thị (suy từ tên; Java không dùng)
    public const int MENU_SHOW = 0;
}
