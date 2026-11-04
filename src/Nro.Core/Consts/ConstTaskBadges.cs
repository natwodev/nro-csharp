namespace Nro.Core.Consts;

// Hằng số nhiệm vụ danh hiệu: id trong bảng task_badges_template; làm đủ maxCount lần thì nhận danh hiệu idbadgesReward (-1 là không có danh hiệu)
// Nguồn Java: consts/ConstTaskBadges.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstTaskBadges
{
    // Đại gia mới nhú: nạp tích lũy 1 triệu trong ngày (Java: DAI_GIA_MOI_NHU)
    public const sbyte RISING_TYCOON = 1;

    // Trùm ước rồng: ước Rồng Thần 1 sao 100 lần (Java: TRUM_UOC_RONG)
    public const sbyte DRAGON_WISH_MASTER = 2;

    // Trùm săn boss: hạ boss 300 lần (mô tả ghi Cumber, Black Goku, Cooler, Xên; code đếm cả nhiều boss khác) (Java: TRUM_SAN_BOSS)
    public const sbyte BOSS_HUNTER = 3;

    // Thánh đập đồ +7: mô tả là đập 5 trang bị +7 trong ngày; code đếm mỗi lần nâng cấp thành công từ +7 lên +8 (Java: THANH_DAP_DO_7)
    public const sbyte UPGRADE_MASTER_PLUS_7 = 4;

    // Cao thủ siêu hạng: nhận thưởng Top 1 Giải Siêu Hạng (Java: CAO_THU_SIEU_HANG)
    public const sbyte SUPER_RANK_MASTER = 5;

    // Nông dân chăm chỉ: hoàn thành 10 nhiệm vụ siêu khó (hoặc địa ngục) ở Bò Mộng (Java: NONG_DAN_CHAM_CHI)
    public const sbyte HARDWORKING_FARMER = 6;

    // Kẻ thao túng sói: đánh bại hoặc cho Sói Héc Quyn ăn xương 20 lần; không có danh hiệu thưởng (Java: KE_THAO_TUNG_SOI)
    public const sbyte WOLF_MANIPULATOR = 7;

    // Nước anh Báo: cho Xinbatô nước 5 lần; không có danh hiệu thưởng (Java: NUOC_ANH_BAO)
    public const sbyte WATER_FOR_XINBATO = 8;

    // Ông thần ve chai: mô tả là nhặt đồ 500 lần trong ngày; code đếm mỗi lần hạ quái (Java: ONG_THAN_VE_CHAI)
    public const sbyte SCRAP_GOD = 9;

    // Bị móc sạch túi: tiêu diệt 30 Boss Ăn Trộm; Java không có chỗ cộng tiến độ (Java: BI_MOC_SACH_TUI)
    public const sbyte PICKPOCKETED = 10;

    // Ở Dơ: tiêu diệt 30 boss Ở Dơ; không có danh hiệu thưởng (Java: O_DO)
    public const sbyte FILTHY = 11;

    // Fan cứng: mốc 1 lần; Java không có chỗ cộng tiến độ (Java: FAN_CUNG)
    public const sbyte DIE_HARD_FAN = 12;

    // Gõ đầu trẻ, bản 1 (danh hiệu 240): mốc 1 lần; Java không có chỗ cộng tiến độ (Java: GO_DAU_TRE_1)
    public const sbyte TEACHER_1 = 13;

    // Gõ đầu trẻ, bản 2 (danh hiệu 242): mốc 1 lần; Java không có chỗ cộng tiến độ (Java: GO_DAU_TRE_2)
    public const sbyte TEACHER_2 = 14;

    // Gõ đầu trẻ, bản 3 (danh hiệu 243): mốc 1 lần; Java không có chỗ cộng tiến độ (Java: GO_DAU_TRE_3)
    public const sbyte TEACHER_3 = 15;

    // X-mas: danh hiệu Giáng sinh, mốc 1 lần; Java không có chỗ cộng tiến độ (giữ cách viết XMASS của Java)
    public const sbyte XMASS = 16;

    // Em xinh, em đẹp: mốc 1 lần; Java không có chỗ cộng tiến độ (Java: EM_XINH_EM_DEP)
    public const sbyte PRETTY_AND_BEAUTIFUL = 17;

    // Tay nhanh hơn não: mốc 1 lần; Java không có chỗ cộng tiến độ (Java: TAY_NHANH_HON_NAO)
    public const sbyte HANDS_FASTER_THAN_BRAIN = 18;

    // KOL (người có sức ảnh hưởng): mốc 1 lần, không có danh hiệu thưởng; Java không có chỗ cộng tiến độ
    public const sbyte KOL = 19;

    // Mùa đông không lạnh: mốc 1 lần; Java không có chỗ cộng tiến độ (Java: MUA_DONG_KHONG_LANH)
    public const sbyte WARM_WINTER = 20;

    // Mẹ rồng: mốc 7 lần; Java không có chỗ cộng tiến độ (Java: ME_RONG)
    public const sbyte DRAGON_MOTHER = 21;
}
