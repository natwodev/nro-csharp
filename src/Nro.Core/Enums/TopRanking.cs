namespace Nro.Core.Enums;

// Liệt kê các bảng xếp hạng một giá trị; các bảng nhiều giá trị có phương thức riêng trong ITopRepository.
// Nguồn Java: jbcd/dao/TopDAO.java (mỗi giá trị tương ứng một hằng String Java; TOP_NV, TOP_WHIS, TOP_VDST và TOP_THOI_VANG được truy vấn riêng).
public enum TopRanking
{
    // Máy đấm Trái Đất (Java: TOP_MAY_DAM_TRAI_DAT).
    PunchingMachineEarthling,

    // Máy đấm Namếc (Java: TOP_MAY_DAM_NAMEC).
    PunchingMachineNamekian,

    // Máy đấm Xayda (Java: TOP_MAY_DAM_XAYDA).
    PunchingMachineSaiyan,

    // Sức mạnh (Java: TOP_SM).
    Power,

    // HP (Java: TOP_HP).
    Hp,

    // KI (Java: TOP_KI).
    Ki,

    // Sức đánh (Java: TOP_SD).
    Damage,

    // Sức mạnh đệ tử (Java: TOP_SM_PET).
    PetPower,

    // HP đệ tử (Java: TOP_HP_PET).
    PetHp,

    // KI đệ tử (Java: TOP_KI_PET).
    PetKi,

    // Sức đánh đệ tử (Java: TOP_SD_PET).
    PetDamage,

    // VNĐ (Java: TOP_VND).
    Vnd,

    // Ngọc (Java: TOP_COIN).
    Coin,

    // Hồng ngọc (Java: TOP_HONG_NGOC).
    Ruby,

    // Đã nạp (Java: TOP_DA_NAP).
    TotalTopUp,

    // Chiến trường Namếc (Java: TOP_NAMEK_WAR).
    NamekWar,

    // Mở lì xì (Java: TOP_MO_LIXI).
    OpenLuckyMoney,

    // Tặng lì xì (Java: TOP_TANG_LIXI).
    GiveLuckyMoney,

    // Bắn pháo hoa (Java: TOP_BAN_PHAO_HOA).
    LaunchFireworks,

    // Bắn pháo hoa VIP (Java: TOP_BAN_PHAO_HOA_VIP).
    LaunchFireworksVip,

    // Trang trí cây Noel (Java: TOP_TRANG_TRI_CAY_NOEL).
    DecorateChristmasTree,

    // Chế tạo người tuyết (Java: CHE_TAO_NGUOI_TUYET).
    CraftSnowman,

    // Chế tạo người tuyết băng giá (Java: CHE_TAO_NGUOI_TUYET_BANG_GIA).
    CraftFrozenSnowman,

    // Đốt diêm (Java: TOP_DOT_DIEM).
    BurnMatches,

    // Đốt pháo Vu Lan (Java: TOP_DOT_PHAO_VU_LAN).
    BurnVuLanFirecrackers,

    // Hoa đăng (Java: TOP_HOA_DANG).
    FlowerLantern,

    // Hoa đăng lời chúc (Java: TOP_HOA_DANG_LOI_CHUC).
    FlowerLanternWish,

    // Mở hộp quà ma quỷ (Java: TOP_MO_HOP_QUA_MA_QUY).
    OpenGhostGiftBox,

    // Thiệp Halloween (Java: TOP_THIEP_HALLOWEEN).
    HalloweenCard,

    // Mở thiệp 8/3 (Java: TOP_MO_THIEP_83).
    OpenWomensDayCard,

    // Tặng bông hoa hồng (Java: TOP_TANG_BONG_HOA_HONG).
    GiveRoses,

    // Làm bánh Trung Thu (Java: TOP_LAM_BANH_TRUNG_THU).
    MakeMoonCake,

    // Mở bánh Trung Thu đặc biệt (Java: TOP_MO_BANH_TRUNG_THU_DAC_BIET).
    OpenSpecialMoonCake,

    // Mở trứng vàng (Java: TOP_MO_TRUNG_VANG).
    OpenGoldenEgg,

    // Mở hộp quà Giỗ Tổ (Java: TOP_MO_HOP_QUA_GIO_TO).
    OpenHungKingGiftBox,

    // Dâng bánh (Java: TOP_DANG_BANH).
    OfferCakes,

    // Đổi dưa hấu (Java: TOP_DOI_DUA_HAU).
    ExchangeWatermelon,

    // Mở hộp Black Friday (Java: TOP_MO_HOP_BLACK_FRIDAY).
    OpenBlackFridayBox,

    // Mua sắm Black Friday (Java: TOP_MUA_SAM_BLACK_FRIDAY).
    BlackFridayShopping,

    // Các bảng xếp hạng Java không từng gọi vẫn giữ vì SQL hợp lệ; TOP_SK, TOP_NHS, TOP_DC và TOP_3_WHIS không port vì MariaDB cũng lỗi.

    // Điểm PVP, Java server không gọi (Java: TOP_PVP).
    Pvp,

    // Nạp tiền, Java server không gọi (Java: TOP_NAP).
    TopUp,

    // Đua nạp, Java server không gọi (Java: TOP_DUA_NAP).
    TopUpRace,
}
