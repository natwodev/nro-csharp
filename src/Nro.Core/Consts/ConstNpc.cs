namespace Nro.Core.Consts;

// Hằng số NPC: id NPC, chỉ số menu, id cửa hàng và lời thoại hướng dẫn
// Nguồn Java: consts/ConstNpc.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstNpc
{
    // Lời hướng dẫn Đại hội võ thuật liên vũ trụ (lịch thi đấu, phần thưởng), Bill nói (Java: HUONG_DAN_BILL)
    public const string GUIDE_BILL_TOURNAMENT = "Đại hội võ thuật liên vũ trụ\blà nơi quy tụ các cao thủ từ khắp các Vũ Trụ\bĐược tổ chức hàng ngày tại các thời điểm như sau:\n"
            + "Thứ 2: 6h, Thứ 3: 13h, Thứ 4: 15h\bThứ 5: 17h, Thứ 6: 18h, Thứ 7: 12h, Chủ Nhật 10h\bVới các phần thưởng vô cùng hấp dẫn như sau\n"
            + "Top 10: 1 phiếu giảm giá, 1 capsule Vàng\bTop 1: tặng thêm 1 Rađa cấp 13\bPhần thưởng được trao tại vũ trụ của chiến binh\bĐến gặp Whis tại vũ trụ của ngươi để nhận thưởng";

    // Thông tin Đại hội võ thuật: lịch các giải trong ngày, giải thưởng và lệ phí đăng ký (Java: THONG_TIN_DAI_HOI_VO_THUAT)
    public const string INFO_MARTIAL_ARTS_TOURNAMENT = "Lịch thi đấu trong ngày\bGiải Nhi đồng: 8,14,18h\bGiải Siêu cấp 1: 9,13,19h\bGiải Siêu Cấp 2: 10,15,20h\bGiải Siêu cấp 3: 11,16,21h\bGiải Ngoại hạng: 12,17,22,23h\n"
            + "Giải thưởng khi thắng mỗi vòng\bGiải Nhi đồng: 2 ngọc\bGiải Siêu cấp 1: 4 ngọc\bGiải Siêu cấp 2: 6 ngọc\bGiải Siêu cấp 3: 8 ngọc\bGiải Ngoại hạng: 10.000 vàng\bVô địch: 5 viên đá nâng cấp\n"
            + "Lệ phí đăng ký các giải đấu\bGiải Nhi đồng: 2 ngọc\bGiải Siêu cấp 1: 4 ngọc\bGiải Siêu cấp 2: 6 ngọc\bGiải Siêu cấp 3: 8 ngọc\bGiải Ngoại hạng: 10.000 vàng\n"
            + "Vui lòng đến đúng giờ để đăng ký thi đấu";

    // Thông tin giải Siêu Hạng: cơ cấu giải thưởng và vé tham dự, Trọng tài nói (Java: THONG_TIN_SIEU_HANG)
    public const string INFO_SUPER_RANK = "Giải đấu thể hiện đẳng cấp thực sự\bCác trận đấu diễn ra liên tục bất kể ngày đêm\bBạn hãy tham gia thi đấu để nâng hạng\bvà nhận giải thưởng khủng nhé\n"
            + "Cơ cấu giải thưởng như sau\b(chốt và trao giải ngẫu nhiên từ 20h-23h mỗi ngày)\bTop 1 thưởng 100 ngọc\bTop 2-10 thưởng 20 ngọc\bTop 11-100 thưởng 5 ngọc\bTop 101-1000 thưởng 1 ngọc\n"
            + "Mỗi ngày các bạn được tặng 1 vé tham dự miễn phí\b(tích lũy tối đa 3 vé) khi thua sẽ mất đi 1 vé\bKhi hết vé bạn phải trả 1 ngọc để đấu tiếp\b(trừ ngọc khi trận đấu kết thúc)\n"
            + "Bạn không thể thi đấu với đấu thủ\bcó hạng nhỏ hơn mình\bChúc bạn may mắn, chào đoàn kết và quyết thắng";

    // Giới thiệu Đại hội võ thuật lần thứ 23 (thưởng rương gỗ tăng cấp theo số đối thủ hạ được), NPC Ghi danh nói (Java: NPC_DHVT23)
    public const string INFO_MARTIAL_ARTS_TOURNAMENT_23 = "Đại hội quy tụ nhiều cao thủ như là Jacky Chun, Thiên Xin Hăng, Tàu Bảy Bảy... Phần thường là 1 rương gỗ chưa nhiều vật phẩm giá trị.\bKhi hạ được 1 đối thủ, phần thưởng sẽ nâng lên 1 cấp.\bRương càng cao cấp, vật phẩm trong đó càng giá trị hơn.\n"
            + "Mỗi ngày bạn chỉ được nhận 1 phần thưởng.\bBạn hãy cố gắng hết sức mình\bđể nhận phần thưởng xứng đáng nhất nhé";

    // Hướng dẫn gọi Rồng Thần Namếc bằng 7 viên ngọc rồng Namếc (NRNM), Dende nói (Java: HUONG_DAN_NRNM)
    public const string GUIDE_NAMEK_DRAGON_BALLS = "Để gọi được Rồng Thần Namếc, anh cần phải làm các việc sau\b"
            + "1) Đang có bang hội\b2) Tập họp đủ 7 viên ngọc rồng Namếc tại đây\b3) Em sẽ gọi rồng và anh nào giữ ngọc 1 sao sẽ được chọn điều ước\b"
            + "4) Thời gian gọi Rồng Thần là 8h-22h\n"
            + "(Lưu ý) Điều ước sẽ có tác dụng với tất cả thành viên trong bang có mặt tại đây\b"
            + "Sau khi điều ước được thực hiện, tất cả ngọc sẽ biến thành đá trong 1 ngày\b"
            + "Những ai vừa nhận điều ước, hoặc bang hội nhận điều ước\b"
            + "phải chờ 7 ngày sau mới có thể nhận điều ước khác";

    // Hướng dẫn tập luyện tự động khi offline và sức mạnh tăng mỗi phút theo từng thầy (Java: TAP_TU_DONG)
    public const string GUIDE_AUTO_TRAINING = "Tập luyện vẫn tiếp tục và sức mạnh vẫn tăng khi đã Offline\n"
            + "Hiệu quả tập luyện như sau:\bThần Mèo: 20 sức mạnh mỗi phút\bYajirô: 40 sức mạnh mỗi phút\bMr.PôPô: 80 sức mạnh mỗi phút\bThượng đế: 160 sức mạnh mỗi phút\n"
            + "Khỉ Bubbles: 320 sức mạnh mỗi phút\bThần Vũ Trụ: 640 sức mạnh mỗi phút\bTổ sư Kaio: 1280 sức mạnh mỗi phút\n"
            + "Có thể tặng ngọc để thắng mà không cần thách đấu\n"
            + "Nếu đăng ký tập thường xuyên mỗi khi Offline không cần phải đến đây vẫn tập luyện được";

    // Hướng dẫn phó bản Giải cứu Mị Nương, phần 1; Java chỉ còn trong code đã comment (Java: HUONG_DAN_GIAI_CUU_MI_NUONG)
    public const string GUIDE_RESCUE_MI_NUONG = "Các Bang Hội Tập Hợp Đủ 3 Thành Viên Để Đi Giải Cứu Mị Nương\b"
            + "Tiêu DiệT Mị Nương Nhận Cải Trang\b"
            + "Nếu May Mắn Sẽ Được Cải Trang Mị Nương Vĩnh Viễn\b"
            + "Lưu Ý: Phải Mặc Cải Trang Sơn Tinh Hoặc Thuỷ Tinh Thì Mới Có Thể Gây Sát Thương Lên Boss\b\b"
            + "|7|Đánh Boss Thì Phải Mặc Cải Trang Sơn Tinh Hoặc Thuỷ Tinh Và Phải Tách Hợp Thể Ra Thì Mới Gây Sát Thương Được\b"
            + "|2|Boss Mị Nương có thể Hợp Thể nhưng phải mặc cải trang Sơn Tinh hoặc Thuỷ Tinh thì mới có thể gây sát thương\b"
            + "|0|Vào Map Nhớ Mang Theo Mũ Bảo Hiểm Và Phao Bơi Để Tránh Bị Vỡ Đầu Và Đuối Nước!";

    // Hướng dẫn phó bản Giải cứu Mị Nương, phần 2: cách kiếm cải trang Sơn Tinh, Thủy Tinh (Java: HUONG_DAN_GIAI_CUU_MI_NUONG2)
    public const string GUIDE_RESCUE_MI_NUONG_2 = "Bạn Có Thể Tìm Cải Trang Sơn Tinh Thuỷ Tinh Bằng Cách:\b"
            + "Tiêu DiệT Sơn Tinh Thuỷ Tinh Để Lấy Cải Trang\b\b"
            + "|7|Boss Xuất Hiện Ở Ngẫu Nhiên Các Map Sau:\b"
            + "|2|Đồi Hoa Cúc, Rừng Nấm, Rừng Xương, Rừng Bambo, Đảo Kame\b"
            + "|6|Hãy Chọn Phe Cờ Và Tiêu Diệt Boss!";

    // Lời Ca Lích kể chuyện tương lai bị bọn Android sát thủ tàn phá (Java: CALICK_KE_CHUYEN)
    public const string CALICK_STORY = "20 năm trước bọn Android sát thủ đã đánh bại nhóm bảo vệ trái đất của Sôngoku và Cađíc, Pôcôlô ...\n"
            + "Riêng Sôngoku vì bệnh tim nên đã chết trước đó nên không thể tham gia trận đánh...\n"
            + "Từ đó đến nay bọn chúng tàn phá Trái Đất không hề thương tiếc\n"
            + "Cháu và mẹ may mắn sống sót nhờ lẩn trốn tại tần hầm của công ty Capsule...\n"
            + "Cháu tuy cũng là siêu xayda nhưng cũng không thể làm gì được bọn Android sát thủ...\n"
            + "Chỉ có Sôngoku mới có thể đánh bại bọn chúng\n"
            + "mẹ cháu đã chế tạo thành công cỗ máy thời gian\n"
            + "và cháu quay về quá khứ để cứu Sôngoku...\n"
            + "Bệnh của Gôku ở quá khứ là nan y, nhưng với trình độ y học tương lai chỉ cần uống thuốc là khỏi...\n"
            + "Hãy đi theo cháu đến tương lai giúp nhóm của Gôku đánh bạn bọn Android sát thủ\n"
            + "Khi nào chú cần sự giúp đỡ của cháu hãy đến đây nhé";

    // Hướng dẫn phó bản Doanh Trại (trại Độc Nhãn), Lính canh nói (Java: HUONG_DAN_DOANH_TRAI)
    public const string GUIDE_BARRACKS = "1) Trại độc nhãn là nơi các ngươi không nên vào vì những tướng tá rất mạnh. Hahaha\b"
            + "2) Trong trại độc nhãn, mỗi vị tướng đều giữ ngọc rồng từ 4 sao đến 6 sao, tùy lúc\n"
            + "3) Nếu ngươi thích chết thì cứ việc vào. Nhưng ta chỉ cho vào mỗi ngày một lần thôi, để ngươi khỏi phải chết nhiều, hahaha.\b"
            + "4) Các vị tướng trong trại rất mạnh nhé, các ngươi không đơn giản có thể đánh bại họ bằng cách bình thường như đánh quái được đâu\n"
            + "5) Muốn vào, ngươi phải đi cùng một người đồng đội cùng bang (phải đứng gần ngươi). Nhưng ta khuyên là nên đi 3-4 người cùng.\b"
            + "6) Mỗi lần vào, ngươi chỉ có 30 phút để đánh. Sau 30 phút mà ngươi vẫn không thắng, ta sẽ cho máy bay chở ngươi về nhà.";

    // Hướng dẫn phó bản Khí Gas Hủy Diệt (Destron Gas), Mr. PôPô nói (Java: HUONG_DAN_KHI_GAS_HUY_DIET)
    public const string GUIDE_DESTRON_GAS = "Chúng ta gặp rắc rối rồi\bThượng Đế nói với tôi rằng có 1 loại khí\bgọi là Destron Gas, thứ này không thuộc về nơi đây\n"
            + "Nó tích tụ trên Trái Đất\bvà nó sẽ hủy diệt mọi mô tế bào sống\bCó tất cả 4 địa điểm mà Thượng Đế bảo tôi nói với cậu\bCậu có thể đến kiểm tra...\n"
            + "Đầu tiên là Thành phố Santa tọa lạc ở phía tây nam của thủ đô ở Viễn Đông.\n"
            + "Thứ hai là gần Kim Tự Tháp ở vùng Sa Mạc viễn tây của thủ đô Phía Bắc.\n"
            + "Thứ ba Vùng Đất Băng Giá ở Phương Bắc xa xôi\n"
            + "Thứ tư là Hành tinh Bóng Tối đang che phủ một phần địa cầu\bCậu đã hiểu rõ chưa?";

    // Hướng dẫn Ngọc Rồng Sao Đen (20h-21h): cách thắng và phần thưởng, Rồng Omega nói (Java: HUONG_DAN_BLACK_BALL_WAR)
    public const string GUIDE_BLACK_BALL_WAR = "Mỗi ngày từ 20h đến 21h các hành tinh có Ngọc Rồng Sao Đen sẽ xảy ra 1 cuộc đại chiến\n"
            + "Người nào tìm thấy và giữ được Ngọc Rồng Sao Đen sẽ mang phần thưởng về cho bang của mình trong vòng 1 ngày\n"
            + "Lưu ý mỗi bang có thể chiếm hữu nhiều viên khác nhau nhưng nếu cùng loại cũng chỉ nhận được 1 lần phần thưởng đó. Có 2 cách để thắng:\n"
            + "1) Giữ ngọc sao đen trên người hơn 5 phút liên tục\n"
            + "2) Sau 30 phút tham gia tàu sẽ đón về và đang giữ ngọc sao đen trên người\n"
            + "Các phần thưởng như sau\b1 sao đen: +21% sức đánh\b2 sao đen: +35% HP\b"
            + "3 sao đen: Biến 35% tấn công thành HP\b4 sao đen: Phản 35% sát thương\b"
            + "5 sao đen: 35% Sức đánh chí mạng\b6 sao đen: KI+40%\b7 sao đen: 14% Né đòn";

    // Hướng dẫn map Ma Bư: tích lũy điểm để xuống tầng tiếp theo, Babiđây và Ôsin nói (Java: HUONG_DAN_MAP_MA_BU)
    public const string GUIDE_MABU_MAP = "Tại khu vực này, ta đã dùng ma pháp phong ấn\b"
            + "dù các ngươi có mạnh đến đâu\bcũng sẽ trở thành yếu đuối như nhau\n"
            + "Chỉ có ta với con nhóc Ôsin mới giải được ma pháp này\b"
            + "Khi đó sức mạnh của ngươi sẽ được\bgiải phóng theo điểm tích lũy (TL)\n"
            + "Khi đủ điểm tích lũy bằng cách\bhạ lẫn nhau hoặc hạ boss\b"
            + "ta sẽ đưa ngươi xuống tầng tiếp theo";

    // Hướng dẫn làm đồ SKH VIP (set kích hoạt) từ 1 món Thiên Sứ và 2 món SKH thường; Java không dùng (Java: HUONG_DAN_DOI_SKH_VIP)
    public const string GUIDE_EXCHANGE_SKH_VIP = "Nguyên liệu cần để làm SKH VIP là :\n"
            + "1 món Thiên Sứ và 2 món SKH thường\n"
            + "Lưu ý SKH VIP sẽ tạo ra dưa vào món Thiên Sứ\n"
            + "Ví dụ nguyên liệu gồm : Quần Xayda Thiên Tứ + 2 món SKH thường ngẫu nhiên\n"
            + "Bạn sẽ nhận lại được Quần Xayda với chỉ số SKH VIP";

    // Hướng dẫn cày vàng: nhiệm vụ Bò Mộng, nạp lần đầu; Java không dùng (Java: HUONG_DAN_CAY)
    public const string GUIDE_GOLD_FARMING = "Cách để bạn có 1 lượng vàng lớn :\n"
            + "Có thể cày ở nv bò mộng hàng ngày"
            + "Lưu ý mỗi ngày có 20nv Nhiệm vụ càng khó lượng vàng nhận được càng nhiều \n"
            + "Hoặc bạn có thể nạp lần đầu ở npc santa =)))\n"
            + "1k VND = 1k coin ; 1k coin = 3 thỏi vàng";

    // Thông tin 3 cấp đệ tử Ngộ Không và % chỉ số cộng thêm khi hợp thể (Java: THONG_TIN_DE_TU_NGO_KHONG)
    public const string INFO_WUKONG_PET = "|7|Đệ Tử Ngộ Không Được Chia Làm 3 Cấp"
            + "\b|7|Cấp 1: Ngộ Không Sơ Sinh + 20% Chỉ Số Khi Hợp Thể"
            + "\b|7|Cấp 2: Ngộ Không Trưởng Thành + 30% Chỉ Số Khi Hợp Thể"
            + "\b|7|Cấp 3[MAX]: Tề Thiên Đại Thánh + 40% Chỉ Số Khi Hợp Thể";

    // Hướng dẫn hộ tống Đường Tăng: khung giờ, phần thưởng, tối đa 3 lần mỗi ngày; Java không dùng (Java: HUONG_DAN_HO_TONG)
    public const string GUIDE_ESCORT = "|1|Khung Giờ Hộ Tống: 12H - 23H59"
            + "\b|6|Hộ Tống Đường Tăng đến các map chỉ định"
            + "\b|7|Trong khi Hộ Tống sẽ bị các người chơi khác cướp, hãy cẩn thận!"
            + "\b|7|Khi Hộ Tống bạn có thể đánh nhau với các người chơi khác trong khu"
            + "\b|6|Hãy làm mọi cách để bảo vệ Đường Tăng của bạn"
            + "\b|6|Phần Thưởng: 1-10 Điểm Ngũ Hành Sơn, 1-10 Hồng Đào, 1-10 Xu Ruby, 1-3 Thỏi Vàng"
            + "\b|2|Giới Hạn Hộ Tống: 1 ngày Hộ Tống tối đa 3 lần";

    // Hướng dẫn đánh boss Ngưu Ma Vương và mang sừng đi đổi pet (Java: HUONG_DAN_NGUUMAVUONG)
    public const string GUIDE_BULL_DEMON_KING = "|2|Đánh Bại Boss Ngưu Ma Vương Bằng Cách:"
            + "\b|7|Lưu Ý: Dùng Chiêu Chưởng Mới Gây Sát Thương Lên Ngưu Ma Vương Được"
            + "\b|7|Nếu Dùng Chiêu Đấm Thì Ngưu Ma Vương Sẽ Hấp Thụ Chuyển Sát Thương Thành Máu Cho Bản Thân!"
            + "\b|2|Tiêu Diệt Boss Có Tỉ Lệ 70% Rơi Sừng"
            + "\b|2|Dùng Sừng Mang Đến Npc Xa Tăng Để Đổi Pet Ngưu Ma Vương";

    // Hướng dẫn kiếm Đá Sức Mạnh: boss ở thành phố Suger hoặc mua ở NPC Granola (Java: HUONG_DAN_CEREAL1)
    public const string GUIDE_CEREAL_1 = "|0|Bạn Có Thể Kiếm Đá Sức Mạnh Bằng Cách:"
            + "\b|2|Tiêu Diệt Boss Bên Thành Phố Suger"
            + "\b|2|Boss Sẽ Có Tỉ Lệ Rơi Đá Sức Mạnh"
            + "\b|7|Hoặc Là Bạn Mua Ở Npc Granola";

    // Hướng dẫn tân thủ trang 1: nơi farm mảnh Thiên Sứ, bí kíp, mảnh bông tai, thức ăn; Ông Gôhan nói (Java: HUONG_DAN_TANTHU)
    public const string GUIDE_NEWBIE = "1.Cách Kiếm Mảnh Thiên Sứ: Farm Quái Tại Hành Tinh Ngục Tù\b"
            + "2.Cách Kiếm Bí Kíp Tuyệt Kĩ: Farm Quái Ở Hành Tinh Chết\b"
            + "3.Cách Kiếm Mảnh Vỡ Bông Tai: Farm Quái Tại Các Map Lãnh Địa Bang Hội [Map Tây Thánh Địa, Đông Thánh Địa]\b"
            + "4.Cách Kiếm Mảnh Hồn Bông Tai: Farm Quái Tại Các Map Lãnh Địa Bang Hội [Map Bắc Thánh Địa, Nam Thánh Địa]\b"
            + "5.Cách Kiếm Thức Ăn: Mặc Set Thần Tinh Và Farm Quái Bên Cold\b"
            + "6.Bí Kíp: Farm Quái Bên Hành Tinh Yadart\b"
            + "7.Sao Pha Lê, Ngọc Rồng: All quái\b"
            + "Đang Update Thêm...";

    // Hướng dẫn tân thủ trang 2: cách kiếm từng loại trang bị (Java: HUONG_DAN_TANTHU2)
    public const string GUIDE_NEWBIE_2 = "1.Cách Kiếm Đồ Kích hoạt Thường: Up 3 Map Đầu Làng [map Đồi đến map Rừng]\b"
            + "2.Cách Kiếm Đồ Thần Linh: Farm Quái Trên Cold Hoặc Săn BlackGoku Tương Lai\b"
            + "3.Cách Kiếm Đồ Huỷ Diệt: Tiêu Diệt Boss Tại Các Trạm Huỷ Diệt [map sau vách núi] Hoặc Mặc đủ set Thần Linh Và Mang Thức Ăn Lên hành tinh Kaio gặp Npc Bill Để Đổi Đồ Huỷ Diệt\b"
            + "4.Cách Kiếm Đồ Thiên Sứ: Gặp Npc Whis Tại Hành Tinh Bill Để Hiểu Hơn\b"
            + "5.Cách Kiếm Đồ Kích Hoạt Vip: Gặp Npc Bà Hạt Mít Tại ĐẢO Kame Để Tìm Hiểu!\b"
            + "6.Cách Kiếm Đồ Tu Tiên: Phi Thăng Lên Tiên Giới Để Hiểu Rõ Hơn!\b"
            + "Đang Update Thêm...";

    // Hướng dẫn tân thủ trang 3: nơi xuất hiện các boss (Java: HUONG_DAN_TANTHU3)
    public const string GUIDE_NEWBIE_3 = "1.Boss Bill Và Whis: Xuất Hiện Ở Map Trạm Huỷ Diệt\b"
            + "2.Boss Tôn Ngộ Không: All Map Ngũ Hành Sơn\b"
            + "3.Boss Khỉ Con: All Map Ngũ Hành Sơn\b"
            + "4.Boss Người Xayda: Thành Phố Nát [cereal]\b"
            + "5.Sơn Tinh Thuỷ Tinh: Gặp Npc Mị Nương Tại Rừng Rambo Để Hiểu Hơn\b"
            + "6.Tàu Pảy Pảy: Nam Karin\b"
            + "7.Broly: all map\b"
            + "8.Boss Ăn Trộm Hay Boss XinBaTo Hay Boss ở Dơ: Bosss Tự Tìm Đến Ngẫu Nhiên Người Chơi\b"
            + "9.Một Số Boss Phó Bản: Phó Bản doanh TRại, kHÍ Gas Huỷ Diệt, Giải Cứu Mị Nương, Bản Đồ Kho Báu, Con Đường Rắn Độc, Một Số Map Mới...\b"
            + "10.Boss Thế Giới: 8H Tối Hàng Ngày\b"
            + "Đang Update Thêm...";

    // Hướng dẫn kiếm Điểm Công Đức bằng hộ tống Đường Tăng; Java không dùng (Java: HUONG_DAN_KIEM_DIEM_CONG_DUC)
    public const string GUIDE_MERIT_POINTS = "Hàng ngày đến làng Aru hộ tống Đường Tăng\b"
            + "Khi hộ tống đến địa điểm chỉ định\b"
            + "Sẽ nhận được ngẫu nhiên 1-5 Điểm Công Đức\b"
            + "Lưu ý: giới hạn mỗi ngày Hộ Tống 3 lần";

    // Công dụng Vòng Kim Cô cấp +1 đến +5: triệu hồi quái hỗ trợ; Java không dùng (Java: CONG_DUNG_VONG_KIM_CO)
    public const string INFO_GOLDEN_HEADBAND = "Vòng Kim Cô [+1] Triệu hồi Da Xanh với 100%HP 100%DAME GỐC bản thân\b"
            + "Vòng Kim Cô [+2] Triệu hồi Arbee với 110%HP 110%DAME GỐC bản thân\b"
            + "Vòng Kim Cô [+3] Triệu hồi Tamburine Đỏ với 120%HP 120%DAME GỐC bản thân\b"
            + "Vòng Kim Cô [+4] Triệu hồi Bee với 130%HP 130%DAME GỐC bản thân\b"
            + "Vòng Kim Cô [+5 MAX] Triệu hồi Phù Thuỷ với 140%HP 140%DAME GỐC bản thân\b"
            + "Lưu ý: + Thời gian tồn tại là 120s\b"
            + "+ Có thể cộng dồn các Mob giữa các Level";

    // Id NPC (cột id của bảng npc_template)

    // NPC Ông Gôhan: ông ở nhà của người Trái Đất (Java: ONG_GOHAN)
    public const sbyte GRANDPA_GOHAN = 0;

    // NPC Ông Paragus: ông ở nhà của người Xayda (Java: ONG_PARAGUS)
    public const sbyte GRANDPA_PARAGUS = 1;

    // NPC Ông Moori: ông ở nhà của người Namếc (Java: ONG_MOORI)
    public const sbyte GRANDPA_MOORI = 2;

    // NPC Rương đồ: rương cất đồ ở nhà (Java: RUONG_DO)
    public const sbyte STORAGE_CHEST = 3;

    // NPC Đậu thần: cây đậu thần ở nhà (Java: DAU_THAN)
    public const sbyte MAGIC_TREE = 4;

    // NPC Con mèo: NPC ẩn dùng để hiện các menu hệ thống (createMenuConMeo) (Java: CON_MEO)
    public const sbyte CAT = 5;

    // NPC Khu vực; Java không dùng (Java: KHU_VUC)
    public const sbyte ZONE = 6;

    // NPC Bunma: bán đồ cho người Trái Đất (avatar 562)
    public const sbyte BUNMA = 7;

    // NPC Dende: bán đồ cho người Namếc, gọi Rồng Thần Namếc (avatar 350)
    public const sbyte DENDE = 8;

    // NPC Appule: bán đồ cho người Xayda (avatar 565)
    public const sbyte APPULE = 9;

    // NPC Dr. Brief: nâng cấp bang và nhiệm vụ bang (tên Java viết sai Brief) (Java: DR_DRIEF)
    public const sbyte DR_BRIEF = 10;

    // NPC Cargo
    public const sbyte CARGO = 11;

    // NPC Cui: đưa người chơi đến chỗ đội quân Fide ở Thung lũng Nappa
    public const sbyte CUI = 12;

    // NPC Quy Lão Kame (Java: QUY_LAO_KAME)
    public const sbyte MASTER_ROSHI = 13;

    // NPC Trưởng lão Guru (Java: TRUONG_LAO_GURU)
    public const sbyte ELDER_GURU = 14;

    // NPC Vua Vegeta (Java: VUA_VEGETA)
    public const sbyte KING_VEGETA = 15;

    // NPC Uron
    public const sbyte URON = 16;

    // NPC Bò Mộng: giao nhiệm vụ phụ hằng ngày
    public const sbyte BO_MONG = 17;

    // NPC Thần mèo Karin (Java: THAN_MEO_KARIN)
    public const sbyte CAT_GOD_KARIN = 18;

    // NPC Thượng Đế: vòng quay may mắn (Java: THUONG_DE)
    public const sbyte KAMI = 19;

    // NPC Thần Vũ Trụ ở hành tinh Kaio: di chuyển giữa các hành tinh (Java: THAN_VU_TRU)
    public const sbyte KING_KAI = 20;

    // NPC Bà Hạt Mít: bán bùa và các chức năng nâng cấp đồ (avatar 1410) (Java: BA_HAT_MIT)
    public const sbyte FORTUNETELLER_BABA = 21;

    // NPC Trọng tài: giải Siêu Hạng (Java: TRONG_TAI)
    public const sbyte REFEREE = 22;

    // NPC Ghi danh: đăng ký Đại hội võ thuật (Java: GHI_DANH)
    public const sbyte REGISTRAR = 23;

    // NPC Rồng Thiêng (rồng thần Trái Đất) (Java: RONG_THIENG)
    public const sbyte SHENRON = 24;

    // NPC Lính canh: cho vào Doanh Trại (Java: LINH_CANH)
    public const sbyte GUARD = 25;

    // NPC Độc Nhãn: chủ trại ở phòng cuối Doanh Trại (map 57) (Java: DOC_NHAN)
    public const sbyte ONE_EYE = 26;

    // NPC Rồng Thần Namếc; Java không dùng (Java: RONG_THIENG_NAMEC)
    public const sbyte SHENRON_NAMEK = 27;

    // NPC Cửa hàng ký gửi (Java: CUA_HANG_KY_GUI)
    public const sbyte CONSIGNMENT_SHOP = 28;

    // NPC Rồng Omega (Rồng Thần Sao Đen trong npc_template): mở và trao thưởng Ngọc Rồng Sao Đen (Java: RONG_OMEGA)
    public const sbyte OMEGA_SHENRON = 29;

    // NPC Rồng 2 sao ở map Ngọc Rồng Sao Đen (Java: RONG_2S)
    public const sbyte DRAGON_2_STAR = 30;

    // NPC Rồng 3 sao ở map Ngọc Rồng Sao Đen (Java: RONG_3S)
    public const sbyte DRAGON_3_STAR = 31;

    // NPC Rồng 4 sao ở map Ngọc Rồng Sao Đen (Java: RONG_4S)
    public const sbyte DRAGON_4_STAR = 32;

    // NPC Rồng 5 sao ở map Ngọc Rồng Sao Đen (Java: RONG_5S)
    public const sbyte DRAGON_5_STAR = 33;

    // NPC Rồng 6 sao ở map Ngọc Rồng Sao Đen (Java: RONG_6S)
    public const sbyte DRAGON_6_STAR = 34;

    // NPC Rồng 7 sao ở map Ngọc Rồng Sao Đen (Java: RONG_7S)
    public const sbyte DRAGON_7_STAR = 35;

    // NPC Rồng 1 sao ở map Ngọc Rồng Sao Đen (npc_template ghi tên Rồng Thần Tà Ác) (Java: RONG_1S)
    public const sbyte DRAGON_1_STAR = 36;

    // NPC Bunma tương lai (TL) (Java: BUNMA_TL)
    public const sbyte BUNMA_FUTURE = 37;

    // NPC Ca Lích
    public const sbyte CALICK = 38;

    // NPC Santa: cửa hàng, mở rộng hành trang, lịch sử giao dịch
    public const sbyte SANTA = 39;

    // NPC Mabư mập (id 40 trong npc_template), xuất hiện ở map 202 (Java: MABU_MAP)
    public const sbyte FAT_MABU = 40;

    // NPC sự kiện Trung thu (Java: TRUNG_THU)
    public const sbyte MID_AUTUMN = 41;

    // NPC Quốc Vương: mở giới hạn sức mạnh (Java: QUOC_VUONG)
    public const sbyte KING = 42;

    // NPC Tổ Sư Kaio: nâng giới hạn sức mạnh (Java: TO_SU_KAIO)
    public const sbyte GRANDMASTER_KAIO = 43;

    // NPC Ôsin: dẫn vào map Ma Bư
    public const sbyte OSIN = 44;

    // NPC Kibit
    public const sbyte KIBIT = 45;

    // NPC Babiđây ở map Ma Bư
    public const sbyte BABIDAY = 46;

    // NPC Giu-ma Đầu Bò (Giu-ma Béo trong npc_template): khiêu chiến Boss bang hội (Java: GIUMA_DAU_BO)
    public const sbyte GIUMA = 47;

    // NPC Ngộ Không (Java: NGO_KHONG)
    public const sbyte WUKONG = 48;

    // NPC Đường Tăng (id 76; npc_template cũng có id 49 tên Đường Tăng) (Java: DUONG_TANG)
    public const sbyte TANG_MONK = 76;

    // NPC Quả trứng: trứng Mabư ở nhà (Java: QUA_TRUNG)
    public const sbyte MABU_EGG = 50;

    // NPC trứng Bill, trùng id với trứng Mabư; Java không dùng (Java: TRUNGBILL)
    public const sbyte BILL_EGG = 50;

    // NPC Dưa hấu: chín thì thu hoạch, mang đến Hùng Vương đổi quà (Java: DUA_HAU)
    public const sbyte WATERMELON = 51;

    // NPC Hùng Vương: sự kiện Giỗ Tổ (Java: HUNG_VUONG)
    public const sbyte HUNG_KING = 52;

    // NPC Tapion
    public const sbyte TAPION = 53;

    // NPC Lý Tiểu Nương: minigame Chẵn/Lẻ
    public const sbyte LY_TIEU_NUONG = 54;

    // NPC Bill (Berrus trong npc_template)
    public const sbyte BILL = 55;

    // NPC Whis
    public const sbyte WHIS = 56;

    // NPC Champa (Java không dùng)
    public const sbyte CHAMPA = 57;

    // NPC Vados
    public const sbyte VADOS = 58;

    // NPC Trọng tài thứ hai; Java không dùng (Java: TRONG_TAI_2)
    public const sbyte REFEREE_2 = 59;

    // NPC Goku SSJ
    public const sbyte GOKU_SSJ = 60;

    // NPC Goku SSJ thứ hai
    public const sbyte GOKU_SSJ_2 = 61;

    // NPC Potage
    public const sbyte POTAGE = 62;

    // NPC Jaco
    public const sbyte JACO = 63;

    // NPC id 64: Thiên Sứ Whis trong npc_template (Java không dùng)
    public const sbyte NPC_64 = 64;

    // NPC Yarirobe (Java không dùng)
    public const sbyte YARIROBE = 65;

    // NPC Nồi bánh: nấu bánh sự kiện (Java: NOI_BANH)
    public const sbyte CAKE_POT = 66;

    // NPC Mr. PôPô: phó bản Khí Gas Hủy Diệt
    public const sbyte MR_POPO = 67;

    // NPC Panchy
    public const sbyte PANCHY = 68;

    // NPC Mèo Đỏ Đen: trò cá cược thỏi vàng; Java dùng lớp ChanLe (Java: MEO_DO_DEN)
    public const sbyte RED_BLACK_CAT = 69;

    // NPC Bardock
    public const sbyte BARDOCK = 70;

    // NPC Berry
    public const sbyte BERRY = 71;

    // NPC Unknown; Java không dùng, tên Java viết sai (Java: UNKOWN)
    public const sbyte UNKNOWN = 72;

    // NPC Fide
    public const sbyte FIDE = 73;

    // NPC Tori-Bot
    public const sbyte TORIBOT = 74;

    // NPC Đại Thiên Sứ (Daishinkan): hộp thư, sự kiện đua top
    public const sbyte DAISHINKAN = 75;

    // Id 76 để trống, trùng giá trị với Đường Tăng (Java không dùng)
    public const sbyte NULL_01 = 76;

    // NPC Mị Nương; Java chỉ còn trong code đã comment (Java: MINUONG)
    public const sbyte MI_NUONG = 77;

    // NPC Ca Đíc
    public const sbyte CADIC = 78;

    // NPC Karin Lân: sự kiện Tết Nguyên Đán (xin chữ, ghép chữ, mâm ngũ quả) (Java: KARIN_KID_LAN)
    public const sbyte KARIN_LION = 79;

    // NPC Cây nêu ngày Tết (Java: CAY_NEU_NEWYEAR)
    public const sbyte NEW_YEAR_POLE = 80;

    // NPC Cây thông Noel: trang trí để nhận x2 kinh nghiệm toàn máy chủ (Java: CAY_THONG_NOEL)
    public const sbyte CHRISTMAS_TREE = 81;

    // NPC Cây thông Noel bản khác ở map 174: chế tạo người tuyết (Java: CAY_THONG_NOEL_VERSION)
    public const sbyte CHRISTMAS_TREE_VERSION = 82;

    // NPC sự kiện Vu Lan
    public const sbyte VU_LAN_EVENT = 83;

    // NPC sự kiện Halloween
    public const sbyte HALLOWEEN_EVENT = 84;

    // NPC Tranh Ngọc Namếc (Java: TRANH_NGOC_NAMEC)
    public const sbyte NAMEK_BALL_WAR = 85;

    // NPC Sói Basil; Java không dùng (Java: SOI_BARSIL)
    public const sbyte WOLF_BASIL = 86;

    // NPC Sói Lavender; Java không dùng (Java: SOI_LAVENDER)
    public const sbyte WOLF_LAVENDER = 87;

    // NPC Sói Bergamo; Java không dùng (Java: SOI_BERGAMO)
    public const sbyte WOLF_BERGAMO = 88;

    // NPC Chi Chi
    public const sbyte CHI_CHI = 89;

    // NPC sự kiện Quốc tế Phụ nữ 8/3 (Bunma Xinh Gái trong npc_template)
    public const sbyte INTERNATIONAL_WOMENS_DAY = 90;

    // NPC Hoa Hồng sự kiện 8/3 (Java: HOA_HONG_8_3)
    public const sbyte WOMENS_DAY_ROSE = 91;

    // NPC Treo đèn Trung thu (Java: TREO_DEN_TRUNG_THU)
    public const sbyte MID_AUTUMN_LANTERN = 92;

    // NPC Thỏ Đại Ca (sự kiện Trung thu) (Java: THO_DAI_KA)
    public const sbyte RABBIT_BOSS = 93;

    // NPC Rương bang hội (Java: RUONG_BANG_HOI)
    public const sbyte CLAN_CHEST = 94;

    // NPC Chẵn Lẻ (Vua Cờ Bạc trong npc_template); Java cũng dùng làm chỉ số menu của NPC này (Java: CHAN_LE)
    public const sbyte ODD_EVEN = 95;

    // NPC sự kiện Black Friday
    public const sbyte BLACK_FRIDAY = 96;

    // NPC Rương sưu tập (Java: RUONG_SUU_TAP)
    public const sbyte COLLECTION_CHEST = 97;

    // NPC Lão Già - ATM: nạp và quy đổi tiền tệ (Java: LAO_GIA)
    public const sbyte OLD_MAN = 98;

    // NPC sự kiện Valentine
    public const sbyte VALENTINE_EVENT = 99;

    // NPC sự kiện ngày Phụ nữ Việt Nam 20/10
    public const sbyte DAY_20_10 = 100;

    // Id 102, không có tên trong npc_template (Java không dùng)
    public const sbyte NULL_04 = 102;

    // NPC Nami
    public const sbyte NAMI = 106;

    // NPC Chopper
    public const sbyte CHOPPER = 109;

    // NPC Franky
    public const sbyte FRANKY = 110;

    // NPC id 112 (Bà Hạt Mít trong npc_template) ở map 199: kích hoạt cải trang, gọi boss
    public const sbyte NULL_03 = 112;

    // NPC Ca Lích Quá Khứ (Java: calichquakhu)
    public const sbyte CALICK_PAST = 117;

    // Chỉ số menu và form quy đổi thỏi vàng sang hồng ngọc (HN) ở Ông Gôhan (Java: QUY_DOI_HN)
    public const int RUBY_EXCHANGE = 1234561221;

    // Chỉ số menu (indexMenu): mỗi NPC tự hiểu chỉ số của mình nên giá trị có thể trùng giữa các NPC

    // Menu lên tầng trên ở map Ma Bư (Babiđây, Ôsin)
    public const int GO_UPSTAIRS_MENU = 10000;

    // Menu cửa hàng Santa khi có phiếu giảm giá thường (80%)
    public const int SHOP_SANTA80 = 2017;

    // Menu cửa hàng Santa khi có phiếu giảm giá VIP (50%)
    public const int SHOP_SANTA50_VIP = 2018;

    // Menu cửa hàng Santa khi có cả hai loại phiếu giảm giá
    public const int SHOP_SANTA50_80 = 2019;

    // Menu gốc dùng chung

    // Menu chỉ để thông báo, chọn gì cũng bỏ qua
    public const int IGNORE_MENU = 72002;

    // Menu gốc khi mở NPC
    public const int BASE_MENU = 31072002;

    // Menu gốc Tranh Ngọc Namếc; Java không dùng (Java: BASE_MENU_TRANH_NGOC)
    public const int BASE_MENU_NAMEK_BALL_WAR = 31072003;

    // Menu gốc đua ngọc; Java không dùng, suy từ tên (Java: BASE_MENU_DUA_NGOC)
    public const int BASE_MENU_BALL_RACE = 31072004;

    // Menu Quy Lão Kame

    // Menu mở phó bản Bản Đồ Kho Báu (Java: MENU_OPEN_DBKB)
    public const int MENU_OPEN_TREASURE_MAP = 500;

    // Menu khi Bản Đồ Kho Báu của bang đã mở (Java: MENU_OPENED_DBKB)
    public const int MENU_OPENED_TREASURE_MAP = 501;

    // Xác nhận vào Bản Đồ Kho Báu (Java: MENU_ACCEPT_GO_TO_BDKB)
    public const int MENU_ACCEPT_GO_TO_TREASURE_MAP = 502;

    // Menu Thần Vũ Trụ

    // Menu di chuyển đến hành tinh khác (Java: MENU_DI_CHUYEN)
    public const int MENU_TRAVEL = 500;

    // Menu Bà Hạt Mít

    // Menu chọn cửa hàng bùa (Java: MENU_OPTION_SHOP_BUA)
    public const int MENU_OPTION_SHOP_CHARM = 500;

    // Xác nhận bắt đầu nâng cấp hoặc ghép đồ
    public const int MENU_START_COMBINE = 501;

    // Menu nâng, đổi đồ SKH VIP; Java không dùng (Java: MENU_NANG_DOI_SKH_VIP)
    public const int MENU_UPGRADE_SKH_VIP = 502;

    // Menu sách tuyệt kỹ (Java: MENU_SACH_TUYET_KY)
    public const int MENU_ULTIMATE_SKILL_BOOK = 505;

    // Menu Whis

    // Menu đập đồ (nâng cấp trang bị) của Whis; Java không dùng (Java: MENU_DAP_DO)
    public const int MENU_UPGRADE_ITEM = 508;

    // Menu Tranh Ngọc Namếc

    // Menu đăng ký Tranh Ngọc Namếc (Java: REGISTER_TRANH_NGOC)
    public const int REGISTER_NAMEK_BALL_WAR = 800;

    // Menu rời Tranh Ngọc Namếc (Java: LOG_OUT_TRANH_NGOC)
    public const int LOG_OUT_NAMEK_BALL_WAR = 801;

    // Menu Lính canh

    // Menu vào Doanh Trại; Java chỉ còn trong code đã comment (Java: MENU_JOIN_DOANH_TRAI)
    public const int MENU_JOIN_BARRACKS = 502;

    // Menu hiện qua Con mèo

    // Mời thách đấu PvP
    public const int MAKE_MATCH_PVP = 502;

    // Mời kết bạn
    public const int MAKE_FRIEND = 503;

    // Trả thù kẻ thù
    public const int REVENGE = 504;

    // Hướng dẫn gọi rồng
    public const int TUTORIAL_SUMMON_DRAGON = 505;

    // Xác nhận gọi rồng thần
    public const int SUMMON_SHENRON = 506;

    // Menu nội tại
    public const int INTRINSIC = 507;

    // Xác nhận đổi nội tại
    public const int CONFIRM_OPEN_INTRINSIC = 508;

    // Xác nhận đổi nội tại VIP
    public const int CONFIRM_OPEN_INTRINSIC_VIP = 509;

    // Xác nhận rời bang
    public const int CONFIRM_LEAVE_CLAN = 510;

    // Xác nhận nhường chức bang chủ (Java: CONFIRM_NHUONG_PC)
    public const int CONFIRM_TRANSFER_LEADER = 511;

    // Menu admin (Java không dùng)
    public const int MENU_ADMIN = 512;

    // Menu khóa người chơi
    public const int BAN_PLAYER = 513;

    // Menu buff đệ tử cho người chơi
    public const int BUFF_PET = 514;

    // Xác nhận xóa hết vật phẩm trong rương vòng quay may mắn
    public const int CONFIRM_REMOVE_ALL_ITEM_LUCKY_ROUND = 515;

    // Menu quản lý người chơi vừa tìm
    public const int MENU_FIND_PLAYER = 516;

    // Xác nhận giải tán bang (Java không dùng)
    public const int CONFIRM_DISSOLUTION_CLAN = 517;

    // Xác nhận dịch chuyển đến chỗ ngọc rồng Namếc
    public const int CONFIRM_TELE_NAMEC = 522;

    // Xác nhận đưa vật phẩm ký gửi lên trang đầu
    public const int UP_TOP_ITEM = 527;

    // Bật hoặc tắt mã bảo vệ tài khoản (Java: MA_BAO_VE)
    public const int SECURITY_CODE = 528;

    // Mời chơi oẳn tù tì (OTT) cược vàng (Java: OTT)
    public const int ROCK_PAPER_SCISSORS = 529;

    // Đối phương chọn khi được mời chơi oẳn tù tì (Java: OTT_ACCEPT)
    public const int ROCK_PAPER_SCISSORS_ACCEPT = 530;

    // Nhận vật phẩm từ rương gỗ (Java: RUONG_GO)
    public const int WOODEN_CHEST = 531;

    // Bình chọn (đặt cược) trận đấu ở Bà Hạt Mít (Java: DAT_CUOC_HAT_MIT)
    public const int BABA_BET = 532;

    // Mua lại vật phẩm đã bán
    public const int BUY_BACK = 533;

    // Dùng Mảnh giấy có chữ MA (item 726) để vào map Siêu Thần Thủy
    public const int MENU_OPTION_USE_ITEM726 = 726;

    // Menu đưa về từ map Siêu Thần Thủy (Java: MENU_SIEU_THAN_THUY)
    public const int MENU_SUPER_DIVINE_WATER = 2006;

    // Menu xuống tầng dưới ở map Ma Bư 12h (Java: MENU_XUONG_TANG_DUOI)
    public const int MENU_GO_DOWNSTAIRS = 2007;

    // Menu Rồng Thiêng

    // Xác nhận điều ước Rồng Thiêng
    public const int SHENRON_CONFIRM = 501;

    // Danh sách điều ước rồng 1 sao, trang 1
    public const int SHENRON_1_1 = 502;

    // Danh sách điều ước rồng 1 sao, trang 2
    public const int SHENRON_1_2 = 503;

    // Danh sách điều ước rồng 2 sao
    public const int SHENRON_2 = 504;

    // Danh sách điều ước rồng 3 sao
    public const int SHENRON_3 = 505;

    // Danh sách điều ước Rồng Thần Namếc
    public const int SHOW_SHENRON_NAMEK_CONFIRM = 31720020;

    // Xác nhận điều ước Rồng Thần Namếc
    public const int SHENRON_NAMEK_CONFIRM = 31720021;

    // Xác nhận gọi Rồng Băng sự kiện Giáng sinh
    public const int SUMMON_SHENRON_EVENT_CHRISTMAS = 31720022;

    // Danh sách điều ước Rồng Băng
    public const int SHOW_SHENRON_EVENT_CONFIRM_CHRISTMAS = 31720023;

    // Xác nhận điều ước Rồng Băng
    public const int SHENRON_EVENT_CONFIRM_CHRISTMAS = 31720024;

    // Xác nhận gọi Rồng Xương sự kiện Halloween
    public const int SUMMON_SHENRON_EVENT_HALLOWEEN = 31720025;

    // Danh sách điều ước Rồng Xương
    public const int SHOW_SHENRON_EVENT_CONFIRM_HALLOWEEN = 31720026;

    // Xác nhận điều ước Rồng Xương
    public const int SHENRON_EVENT_CONFIRM_HALLOWEEN = 31720027;

    // Menu tương tác với người chơi khác (oẳn tù tì, cừu sát...)
    public const int SUB_MENU = 31720100;

    // Thông báo kết quả tập luyện tự động khi vào lại game (Java: TAP_TU_DONG_CONFIRM)
    public const int AUTO_TRAINING_CONFIRM = 31720101;

    // Xác nhận nâng cấp bang (Dr. Brief)
    public const int MENU_CLAN_UP = 4;

    // Menu nhiệm vụ bang (Dr. Brief)
    public const int MENU_CLAN_TASK = 5;

    // Xác nhận hủy nhiệm vụ bang
    public const int MENU_CLAN_TASK_REMOVE = 6;

    // Xác nhận đăng ký Đại hội võ thuật (DHVT) (Java: DANGKYDHVT_CONFIRM)
    public const int REGISTER_TOURNAMENT_CONFIRM = 0;

    // Giao Rùa con cho Quy Lão Kame để nhận quà (Java: MENU_GIAO_RUA_CON)
    public const int MENU_DELIVER_BABY_TURTLE = 10030;

    // Menu cây đậu thần

    // Cây đậu không nâng cấp, đậu chưa đầy
    public const int MAGIC_TREE_NON_UPGRADE_LEFT_PEA = 501;

    // Cây đậu không nâng cấp, đậu đã đầy
    public const int MAGIC_TREE_NON_UPGRADE_FULL_PEA = 502;

    // Xác nhận nâng cấp cây đậu
    public const int MAGIC_TREE_CONFIRM_UPGRADE = 503;

    // Cây đậu đang nâng cấp
    public const int MAGIC_TREE_UPGRADE = 504;

    // Xác nhận hủy nâng cấp cây đậu
    public const int MAGIC_TREE_CONFIRM_UNUPGRADE = 505;

    // Menu Dưa hấu

    // Dưa hấu chưa chín, chưa thu hoạch được (Java: CAN_NOT_OPEN_DUA)
    public const int CAN_NOT_OPEN_WATERMELON = 500;

    // Dưa hấu đã chín, thu hoạch được (Java: CAN_OPEN_DUA)
    public const int CAN_OPEN_WATERMELON = 501;

    // Menu trứng Mabư

    // Trứng Mabư chưa nở được
    public const int CAN_NOT_OPEN_EGG = 500;

    // Trứng Mabư đã nở được
    public const int CAN_OPEN_EGG = 501;

    // Xác nhận mở trứng Mabư
    public const int CONFIRM_OPEN_EGG = 502;

    // Xác nhận hủy trứng Mabư
    public const int CONFIRM_DESTROY_EGG = 503;

    // Menu trứng Bill

    // Trứng Bill chưa nở được (Java không dùng)
    public const int CAN_NOT_OPEN_BILL = 500;

    // Trứng Bill đã nở được (Java không dùng)
    public const int CAN_OPEN_BILL = 501;

    // Xác nhận mở trứng Bill (Java không dùng)
    public const int CONFIRM_OPEN_BILL = 502;

    // Xác nhận hủy trứng Bill (Java không dùng)
    public const int CONFIRM_DESTROY_BILL = 503;

    // Menu NPC ở nhà

    // Nhận quà tân thủ; Java không dùng (Java: QUA_TAN_THU)
    public const int NEWBIE_GIFT = 500;

    // Menu phần thưởng; Java không dùng (Java: MENU_PHAN_THUONG)
    public const int MENU_REWARD = 501;

    // Nạp thẻ; Java không dùng (Java: NAP_THE)
    public const int TOP_UP_CARD = 502;

    // Nạp tiền; Java không dùng (Java: NAP_TIEN)
    public const int TOP_UP_MONEY = 503;

    // Nạp vàng; Java không dùng (Java: NAP_VANG)
    public const int TOP_UP_GOLD = 504;

    // Menu cửa hàng có mục mua lại vật phẩm đã bán

    // Cửa hàng Dende kèm mục mua lại vật phẩm đã bán (Java: SHOP_DENDE_MUA_LAI)
    public const int SHOP_DENDE_BUYBACK = 2063;

    // Cửa hàng Dende khi chưa bán vật phẩm nào (không có mục mua lại) (Java: SHOP_DENDE_MUA_LAI_2)
    public const int SHOP_DENDE_BUYBACK_2 = 2064;

    // Cửa hàng Bunma kèm mục mua lại vật phẩm đã bán (Java: SHOP_BUMMA_MUA_LAI)
    public const int SHOP_BUMMA_BUYBACK = 2065;

    // Cửa hàng Bunma khi chưa bán vật phẩm nào (không có mục mua lại) (Java: SHOP_BUMMA_MUA_LAI_2)
    public const int SHOP_BUMMA_BUYBACK_2 = 2066;

    // Cửa hàng Appule kèm mục mua lại vật phẩm đã bán (Java: SHOP_APPULE_MUA_LAI)
    public const int SHOP_APPULE_BUYBACK = 2067;

    // Cửa hàng Appule khi chưa bán vật phẩm nào (không có mục mua lại) (Java: SHOP_APPULE_MUA_LAI_2)
    public const int SHOP_APPULE_BUYBACK_2 = 2068;

    // Menu Quốc Vương

    // Mở giới hạn sức mạnh cho bản thân (tên Java viết sai MYSELF) (Java: OPEN_POWER_MYSEFT)
    public const int OPEN_POWER_MYSELF = 500;

    // Mở giới hạn sức mạnh cho đệ tử
    public const int OPEN_POWER_PET = 501;

    // Menu Thượng Đế

    // Menu chọn vòng quay may mắn
    public const int MENU_CHOOSE_LUCKY_ROUND = 500;

    // Menu Cui

    // Đưa đến chỗ boss Kuku (nhiệm vụ 21)
    public const int MENU_FIND_KUKU = 501;

    // Đưa đến chỗ boss Mập Đầu Đinh (nhiệm vụ 21)
    public const int MENU_FIND_MAP_DAU_DINH = 502;

    // Đưa đến chỗ boss Rambo (nhiệm vụ 21)
    public const int MENU_FIND_RAMBO = 503;

    // Menu Rồng Omega

    // Ngọc Rồng Sao Đen (BDW) chưa mở
    public const int MENU_NOT_OPEN_BDW = 500;

    // Ngọc Rồng Sao Đen đang mở, cho vào
    public const int MENU_OPEN_BDW = 501;

    // Nhận thưởng Ngọc Rồng Sao Đen
    public const int MENU_REWARD_BDW = 502;

    // Menu Ôsin

    // Map Ma Bư (MMB) chưa mở (Java: MENU_NOT_OPEN_MMB)
    public const int MENU_NOT_OPEN_MABU_MAP = 500;

    // Map Ma Bư đang mở, cho vào (Java: MENU_OPEN_MMB)
    public const int MENU_OPEN_MABU_MAP = 501;

    // Nhận thưởng map Ma Bư; Java không dùng (Java: MENU_REWARD_MMB)
    public const int MENU_REWARD_MABU_MAP = 502;

    // Menu Rồng sao đen

    // Rồng sao đen phù hộ khi đang giữ ngọc sao đen (Java: MENU_PHU_HP)
    public const int MENU_BLESS_HP = 500;

    // Chọn mức phù hộ x3, x5, x7 HP hoặc sức đánh (Java: MENU_OPTION_PHU_HP)
    public const int MENU_OPTION_BLESS_HP = 501;

    // Rồng sao đen đưa về nhà
    public const int MENU_OPTION_GO_HOME = 502;

    // Menu Bò Mộng

    // Chọn cấp độ nhiệm vụ phụ (Dễ đến Địa ngục)
    public const int MENU_OPTION_LEVEL_SIDE_TASK = 500;

    // Trả hoặc hủy nhiệm vụ phụ đang làm
    public const int MENU_OPTION_PAY_SIDE_TASK = 501;

    // Sự kiện Tết: nồi nấu bánh toàn máy chủ

    // Nồi đang nấu, xem thời gian còn lại (Java: NAU_BANH)
    public const int COOK_CAKE = 2025;

    // Nồi chờ lấy bánh chín và cho nguyên liệu nấu mẻ mới (CB = chuẩn bị) (Java: CB_NAU_BANH)
    public const int PREPARE_COOK_CAKE = 2026;

    // Nấu bánh chưng (Java: NAU_BANH_CHUNG)
    public const int COOK_BANH_CHUNG = 2027;

    // Nấu bánh tét (Java: NAU_BANH_TET)
    public const int COOK_BANH_TET = 2028;

    // Bánh đã chín, có 5 phút để lấy (Java: BANH_CHIN)
    public const int CAKE_COOKED = 2029;

    // Chỉ số menu chức năng và cửa hàng

    // Hộp thư (Đại Thiên Sứ)
    public const int MAIL_BOX = 5558;

    // Xác nhận xóa hết vật phẩm trong hộp thư
    public const int CONFIRM_REMOVE_ALL_ITEM_MAIL_BOX = 5559;

    // Menu chức năng pha lê; Java không dùng (Java: CHUC_NANG_PHA_LE)
    public const int CRYSTAL_FUNCTIONS = 15122007;

    // Pha lê hóa trang bị; Java không dùng (Java: PHA_LE_HOA_TRANG_BI)
    public const int CRYSTALLIZE_EQUIPMENT = 15122008;

    // Chuyển hóa trang bị; Java không dùng (Java: CHUYEN_HOA_TRANG_BI)
    public const int TRANSFORM_EQUIPMENT = 15122009;

    // Sách tuyệt kỹ; Java không dùng (Java: SACH_TUYET_KY)
    public const int ULTIMATE_SKILL_BOOK = 15122010;

    // Xác nhận đóng thành Cuốn sách cũ (Java: DONG_THANH_SACH_CU)
    public const int BIND_OLD_BOOK = 15122011;

    // Xác nhận đổi sách tuyệt kỹ (Java: DOI_SACH_TUYET_KY)
    public const int EXCHANGE_ULTIMATE_SKILL_BOOK = 15122012;

    // Xác nhận học tuyệt kỹ ở Whis (Java: HOC_TUYET_KY)
    public const int LEARN_ULTIMATE_SKILL = 15122013;

    // Học kỹ năng (Java không dùng)
    public const int LEARN_SKILL = 15122014;

    // Học nhanh kỹ năng (Java không dùng)
    public const int FAST_LEARN_SKILL = 15122015;

    // Chế tạo trang bị Thiên Sứ; Java không dùng (Java: CHE_TAO_TRANG_BI_THIEN_SU)
    public const int CRAFT_ANGEL_EQUIPMENT = 15122016;

    // Hộp quà Thần Linh; Java không dùng (Java: HOP_QUA_THAN_LINH)
    public const int DIVINE_GIFT_BOX = 251003;

    // Cửa hàng Bunma
    public const int SHOP_BUMMA = 2056;

    // Cửa hàng Bunma cho đệ tử người Trái Đất
    public const int SHOP_BUMMA_2 = 2057;

    // Cửa hàng Appule
    public const int SHOP_APPULE = 2058;

    // Cửa hàng Appule cho đệ tử người Xayda
    public const int SHOP_APPULE_2 = 2059;

    // Cửa hàng Dende
    public const int SHOP_DENDE = 2060;

    // Cửa hàng Dende cho đệ tử người Namếc
    public const int SHOP_DENDE_2 = 2061;

    // Whis ở hành tinh Kaio: tặng 99 thức ăn để nhận đồ Thần Linh
    public const int WHIS_KAIO = 2069;

    // Whis ở hành tinh Ngục Tù (map 155): cửa hàng, đồ Chân Thiên Tử (Java: WHIS_NGUC_TU)
    public const int WHIS_PRISON = 2070;

    // Whis ở hành tinh Bill: thách đấu, học tuyệt kỹ, top 100
    public const int WHIS_BILL = 2071;

    // Whis dạy tuyệt kỹ
    public const int WHIS_SKILL = 2072;

    // Whis chế tạo trang bị Thiên Sứ (Java: WHIS_THIEN_SU)
    public const int WHIS_ANGEL_EQUIPMENT = 2073;

    // Whis trao danh hiệu Thiên Tử trong 60 phút (Java: WHIS_DANH_HIEU)
    public const int WHIS_TITLE = 2077;

    // Whis đổi trang sức Chân Thiên Tử: chọn nguyên liệu Ma Quái hoặc Tinh Thể (Java: WHIS_DOI_CHAN_THIEN_TU)
    public const int WHIS_EXCHANGE_ANKLET = 2078;

    // Đổi Chân Thiên Tử bằng nguyên liệu Ma Quái (Java: WHIS_MA_QUAI)
    public const int WHIS_PHANTOM = 2079;

    // Đổi Chân Thiên Tử bằng nguyên liệu Tinh Thể (Java: WHIS_TINH_THE)
    public const int WHIS_CRYSTAL = 2080;

    // Nhận lì xì Tết (Java: NHAN_LI_XI)
    public const int RECEIVE_LUCKY_MONEY = 2081;

    // Set Thần Linh Trái Đất; Java không dùng, suy từ tên (Java: SET_TLTD)
    public const int SET_GOD_EARTH = 1200;

    // Set Thần Linh Namếc; Java không dùng, suy từ tên (Java: SET_TLNM)
    public const int SET_GOD_NAMEK = 1201;

    // Set Thần Linh Xayda; Java không dùng, suy từ tên (Java: SET_TLXD)
    public const int SET_GOD_SAIYAN = 1202;

    // Set Hủy Diệt Trái Đất; Java không dùng, suy từ tên (Java: SET_HDTD)
    public const int SET_DESTROYER_EARTH = 1300;

    // Set Hủy Diệt Namếc; Java không dùng, suy từ tên (Java: SET_HDNM)
    public const int SET_DESTROYER_NAMEK = 1301;

    // Set Hủy Diệt Xayda; Java không dùng, suy từ tên (Java: SET_HDXD)
    public const int SET_DESTROYER_SAIYAN = 1302;

    // Dùng Gói 30 đậu thần cấp 10 (item 1703, Java không dùng)
    public const int MENU_OPTION_USE_ITEM1703 = 1703;

    // Dùng Gói 30 đậu thần cấp 11 (item 1704, Java không dùng)
    public const int MENU_OPTION_USE_ITEM1704 = 1704;

    // Dùng Hộp quà Set kích hoạt (item 1536, Java không dùng)
    public const int MENU_OPTION_USE_ITEM1536 = 1536;

    // Menu đang học kỹ năng: xem thời gian còn lại, học nhanh bằng ngọc (Java: HOC_SKILL_1)
    public const int LEARN_SKILL_1 = 2120;

    // Xác nhận hủy học kỹ năng, nhận lại 50% tiềm năng (Java: HOC_SKILL_2)
    public const int LEARN_SKILL_2 = 2121;

    // Xác nhận học kỹ năng từ sách (Java: HOC_SKILL_3)
    public const int LEARN_SKILL_3 = 2122;

    // Menu cửa hàng Santa khi không có phiếu giảm giá
    public const int SHOP_SANTA = 10003;

    // Thả hoa đăng có lời chúc: chọn lời chúc (Java: THA_HOA_DANG_CO_LOI_CHUC)
    public const int RELEASE_LANTERN_WITH_WISH = 2123;

    // Thả hoa đăng; Java không dùng (Java: THA_HOA_DANG)
    public const int RELEASE_LANTERN = 2124;

    // Nhận kẹo Halloween (Java: NHAN_KEO_HALLOWEEN)
    public const int RECEIVE_HALLOWEEN_CANDY = 2125;

    // Xác nhận tặng ngọc cho người chơi khác (Java: VE_TANG_NGOC)
    public const int GEM_GIFT_TICKET = 2126;

    // Xác nhận tặng hồng ngọc cho người chơi khác (Java: VE_TANG_HONG_NGOC)
    public const int RUBY_GIFT_TICKET = 2127;

    // Dùng Giấy màu: gói Hộp Đựng Quà (Java: GIAY_MAU)
    public const int COLOR_PAPER = 2128;

    // Gói Hộp Quà Nhẹ Nhàng (Java: GOI_HOP_QUA_NHE_NHANG)
    public const int WRAP_GIFT_BOX_SIMPLE = 2129;

    // Gói Hộp Quà Chỉn Chu (Java: GOI_HOP_QUA_CHIN_CHU)
    public const int WRAP_GIFT_BOX_NEAT = 2130;

    // Dùng Socola trái tim: chọn loại hộp quà để gói (Java: SOCOLA_TRAI_TIM)
    public const int HEART_CHOCOLATE = 2131;

    // Dùng Ống tre nước: chọn trồng 1-3 hoặc 3-5 bông hoa hồng (Java: ONG_TRE_NUOC)
    public const int BAMBOO_WATER_TUBE = 2132;

    // Xác nhận trồng 3-5 bông hoa hồng (cần thêm Thuốc Tăng Trưởng) (Java: TRONG_BONG_HOA_HONG_MAX)
    public const int PLANT_ROSE_MAX = 2133;

    // Xác nhận trồng 1-3 bông hoa hồng (Java: TRONG_BONG_HOA_HONG_MIN)
    public const int PLANT_ROSE_MIN = 2134;

    // Xác nhận dụ Khỉ đột khổng lồ (Java: CALL_KHI_DOT)
    public const int CALL_GIANT_GORILLA = 2135;

    // Rada phóng xạ: đến chỗ Godzilla và Kong (Java: RADA_PHONG_XA)
    public const int RADIATION_RADAR = 2136;

    // Nhận bánh dày đã nấu (sự kiện Hùng Vương) (Java: HUNG_VUONG_BANH_DAY)
    public const int HUNG_KING_BANH_DAY = 2137;

    // Nhận bánh chưng đã nấu (sự kiện Hùng Vương) (Java: HUNG_VUONG_BANH_CHUNG)
    public const int HUNG_KING_BANH_CHUNG = 2138;

    // Bản đồ truyền thuyết: đến chỗ Linh Thú Truyền Thuyết (Java: BAN_DO_TRUYEN_THUYET)
    public const int LEGENDARY_MAP = 2139;

    // Ghép mảnh trứng rồng nhí thành trứng rồng (Java: MANH_TRUNG_RONG_NHI)
    public const int BABY_DRAGON_EGG_FRAGMENT = 2140;

    // Xem lịch sử giao dịch (Santa) (Java: MENU_XEM_LS_GIAO_DICH)
    public const int MENU_VIEW_TRANSACTION_HISTORY = 2141;

    // Nhận nhiệm vụ KOL (Java không dùng)
    public const int RECEIVE_KOL_TASK = 2142;

    // Chọn cấp độ nhiệm vụ sự kiện Valentine
    public const int MENU_OPTION_LEVEL_EVENT_TASK = 2143;

    // Trả hoặc hủy nhiệm vụ sự kiện Valentine
    public const int MENU_OPTION_PAY_EVENT_TASK = 2144;

    // Tạo Boss bang hội (khiêu chiến 30 phút)
    public const int CREATE_CLAN_BOSS = 2145;

    // Vào khu Boss bang hội
    public const int GOTO_CLAN_BOSS = 2146;

    // Capsule kích hoạt tự chọn: chọn áo, quần, găng, giày hoặc rada (Java: CAPSULE_KICH_HOAT_TU_CHON)
    public const int SELECTABLE_ACTIVATION_CAPSULE = 2147;

    // Menu bổ sung

    // Menu bổ sung số 3 (Java không dùng)
    public const int MENU_3 = 10005;

    // Menu bổ sung số 4 (Java không dùng)
    public const int MENU_4 = 10006;

    // Menu bổ sung số 5 (Java không dùng)
    public const int MENU_5 = 10007;

    // Menu bổ sung số 6 (Java không dùng)
    public const int MENU_6 = 10008;

    // Menu bổ sung số 7 (Java không dùng)
    public const int MENU_7 = 10009;

    // Menu bổ sung số 8 (Java không dùng)
    public const int MENU_8 = 10010;

    // Menu bổ sung số 9 (Java không dùng)
    public const int MENU_9 = 10011;

    // Menu bổ sung số 10 (Java không dùng)
    public const int MENU_10 = 10012;

    // Menu bổ sung số 11 (Java không dùng)
    public const int MENU_11 = 10013;

    // Menu bổ sung số 12 (Java không dùng)
    public const int MENU_12 = 10014;

    // Menu bổ sung số 13 (Java không dùng)
    public const int MENU_13 = 10015;

    // Menu bổ sung số 14 (Java không dùng)
    public const int MENU_14 = 10016;

    // Menu bổ sung số 15 (Java không dùng)
    public const int MENU_15 = 10017;

    // Menu bổ sung số 16 (Java không dùng)
    public const int MENU_16 = 10018;

    // Menu trang bị; Java không dùng (Java: MENU_TRANG_BI)
    public const int MENU_EQUIPMENT = 10019;

    // Menu cải trang; Java không dùng (Java: MENU_CAI_TRANG)
    public const int MENU_COSTUME = 10020;

    // Menu hỗ trợ; Java không dùng (Java: HOTRO)
    public const int SUPPORT = 10021;

    // Menu quản trị cho KEY CONTROLLER
    public const int MENU_KEY = 10022;

    // Java không dùng; chưa rõ nghĩa của tên viết tắt
    public const int CLCK = 10023;

    // Admin đổi hành tinh đệ tử của người chơi
    public const int GENDER_PET = 10024;

    // Thông tin trạng thái thành viên (đã mở hay chưa)
    public const int CONFIRM_ACTIVE = 10025;

    // Rút vàng; Java không dùng (Java: RUT_VANG)
    public const int WITHDRAW_GOLD = 10026;

    // Dùng Thỏi vàng: chọn số thỏi muốn bán (Java: USE_THOI_VANG)
    public const int USE_GOLD_BAR = 10028;

    // Admin gọi bot người chơi
    public const int MENU_CALL_BOT = 10029;

    // Admin chọn loại bot: đánh quái, bán đồ, săn boss
    public const int MENU_CALL_BOTVIP = 10031;

    // Nhận thỏi vàng tích lũy của tài khoản ở Ông Gôhan (Java: CONFIRM_THOIVANG)
    public const int CONFIRM_GOLD_BAR = 10032;

    // Xem Căn cước công dân khi dùng item 1124 (Java: CAN_CUOC_CONG_DAN)
    public const int CITIZEN_ID_CARD = 2030;

    // Khảm đá vào trang bị; Java không dùng (Java: KHAM_DA_TRANG_BI)
    public const int INLAY_STONE_EQUIPMENT = 2008;

    // Hắc ám trang bị; Java không dùng (Java: HAC_AM_TRANG_BI)
    public const int DARK_EQUIPMENT = 2009;

    // Menu khi tài khoản đang bật mã bảo vệ (quên mã bảo vệ) (Java: MENU_MA_BAO_VE)
    public const int MENU_SECURITY_CODE = 1105;

    // Triệu hồi Ngưu Ma Vương; Java không dùng (Java: TRIEUHOINGUUMAVUONG)
    public const int SUMMON_BULL_DEMON_KING = 3273;

    // Các chức năng khác; Java không dùng (Java: CHUC_NANG_KHAC)
    public const int OTHER_FUNCTIONS = 2031;

    // Admin bỏ tù hoặc thả người chơi
    public const int JAIL = 2035;

    // Menu người chơi dành cho quản trị; Java không dùng (Java: MENU_PLAYER_FOR_QUAN_TRI_ADMIN)
    public const int MENU_PLAYER_FOR_ADMIN = 2036;

    // Admin đổi kỹ năng 1 của đệ tử
    public const int SKILL1_PET = 2037;

    // Admin kích hoạt tài khoản người chơi
    public const int ACTIVE = 2038;

    // Admin bỏ qua nhiệm vụ (NV) cho người chơi (Java: NEXT_NV)
    public const int NEXT_TASK = 2039;

    // Admin cấp hoặc hủy quyền KEY CONTROLLER, ADMINISTRATOR (Java: BO_NHIEM_UY_NHIEM)
    public const int GRANT_PERMISSION = 2040;

    // Admin chỉnh trạng thái đệ tử
    public const int MODE_PET = 2041;

    // Admin đổi loại đệ tử
    public const int TYPE_PET = 2042;

    // Admin chọn kỹ năng đệ tử cần đổi
    public const int SKILL_PET = 2043;

    // Admin đổi kỹ năng 2 của đệ tử
    public const int SKILL2_PET = 2044;

    // Admin đổi kỹ năng 3 của đệ tử
    public const int SKILL3_PET = 2045;

    // Admin đổi kỹ năng 4 của đệ tử
    public const int SKILL4_PET = 2046;

    // Admin đổi kỹ năng 5 của đệ tử
    public const int SKILL5_PET = 2047;

    // Admin đổi kỹ năng 6 của đệ tử
    public const int SKILL6_PET = 2100;

    // Admin đổi kỹ năng 7 của đệ tử
    public const int SKILL7_PET = 2101;

    // Admin tặng đệ tử cho người chơi (Java: TANG_PET)
    public const int GIVE_PET = 2048;

    // Tự động cộng nhanh chỉ số (HP, KI...) (Java: CHI_SO_NHANH)
    public const int QUICK_STATS = 2049;

    // Đổi màu tên nhân vật (Java: MAU_TEN)
    public const int NAME_COLOR = 2050;

    // Admin buff nâng cao
    public const int MENU_BUFF_SHOP = 2051;

    // Menu quản trị của Admin (Java: QUAN_TRI_ADMIN)
    public const int ADMIN_PANEL = 2052;

    // Menu cửa hàng Santa; Java không dùng (Java: CUA_HANG_SANTA_MENU)
    public const int SANTA_SHOP_MENU = 2053;

    // Thông tin tài khoản vừa tìm
    public const int MENU_FIND_ACCOUNT = 2054;

    // Menu thông báo của admin (Java: MENU_THONGBAO)
    public const int MENU_NOTIFICATION = 2055;

    // Ông Gôhan: chức năng hỗ trợ (Java: ONG_GIA_MENU_1)
    public const int GRANDPA_MENU_1 = 2074;

    // Ông Gôhan: hướng dẫn tân thủ (Java: ONG_GIA_MENU_2)
    public const int GRANDPA_MENU_2 = 2075;

    // Ông Gôhan: bỏ qua nhiệm vụ (Java: ONG_GIA_MENU_3)
    public const int GRANDPA_MENU_3 = 2076;

    // Nâng giới hạn sức mạnh bản thân (Tổ Sư Kaio) (Java: NANG_GIOI_HAN_POWER)
    public const int RAISE_POWER_LIMIT = 2097;

    // Nâng giới hạn sức mạnh đệ tử (Tổ Sư Kaio) (Java: NANG_GIOI_HAN_POWER_PET)
    public const int RAISE_POWER_LIMIT_PET = 2098;

    // Chọn nâng giới hạn sức mạnh cho bản thân hay đệ tử (Java: GIOI_HAN_POWER)
    public const int POWER_LIMIT = 2099;

    // Thông tin người yêu; Java chưa xử lý lựa chọn nào (Java: INFO_NGUOI_YEU)
    public const int INFO_LOVER = 2102;

    // Đổi tab đệ tử
    public const int MENU_TAB_PET = 2103;

    // Sự kiện đua top (Đại Thiên Sứ) (Java: MENU_DUA_TOP)
    public const int MENU_TOP_RACE = 2106;

    // Ông già Noel nhận lại bé tuần lộc (Java: MENU_ONG_GIA_NOEL)
    public const int MENU_SANTA_CLAUS = 2000;

    // Minigame Chọn Ai Đây (Java: CHON_AI_DAY)
    public const int LUCKY_PICK = 2107;

    // Minigame Con số may mắn (Java: CON_SO_MAY_MAN)
    public const int LUCKY_NUMBER = 2108;

    // Chọn Ai Đây: xem giải thưởng và đặt cược (Java: CHON_AI_DAY_SELECT)
    public const int LUCKY_PICK_SELECT = 2109;

    // Con số may mắn giải thỏi vàng (Java: CON_SO_MAY_MAN_VANG)
    public const int LUCKY_NUMBER_GOLD = 2110;

    // Con số may mắn giải hồng ngọc (Java: CON_SO_MAY_MAN_NGOC)
    public const int LUCKY_NUMBER_RUBY = 2111;

    // Con số may mắn giải VNĐ (Java: CON_SO_MAY_MAN_VND)
    public const int LUCKY_NUMBER_VND = 2112;

    // Tết: giao bé rắn thất lạc cho Quy Lão Kame nhận Capsule Tết
    public const int MENU_PE_NA = 2116;

    // Menu Mèo Đen; Java không dùng (Java: MENU_MEO_DEN)
    public const int MENU_BLACK_CAT = 2117;

    // Tết: giao PiLong cho Appule nhận thưởng
    public const int MENU_PI_LONG = 2118;

    // Tết: giao lân con cho Quy Lão Kame nhận Capsule Tết (Java: MENU_LAN_CON)
    public const int MENU_BABY_LION = 2119;

    // Quà mốc nạp ở Bò Mộng; Java chỉ còn phần xử lý, chỗ mở menu đã comment (Java: MENU_QUA)
    public const int MENU_GIFT = 2120;

    // Menu người chơi

    // Menu nạp tiền, ngân hàng
    public const int BANKING = 997;

    // Mua đệ tử bằng VNĐ (Java: muade)
    public const int BUY_PET = 570;

    // Đổi hành tinh đệ tử bằng VNĐ (Java: doihanhtinh)
    public const int CHANGE_PET_PLANET = 571;

    // Đổi kỹ năng đệ tử bằng VNĐ (Java: doiskill)
    public const int CHANGE_PET_SKILL = 572;

    // Menu người chơi; Java không dùng (Java: menuplayer)
    public const int PLAYER_MENU = 573;

    // Menu người chơi 2; Java không dùng (Java: menuplayer2)
    public const int PLAYER_MENU_2 = 574;

    // Dịch vụ đệ tử: mua, đổi kỹ năng, đổi hành tinh (Java: petservice)
    public const int PET_SERVICE = 575;

    // Nhận đệ tử thường hoặc đệ tử Mabư (Java: MENU_NHANDE)
    public const int MENU_RECEIVE_PET = 727;

    // Admin gửi thông báo chung hoặc riêng (Java: MENU_THONGTIN)
    public const int MENU_INFO = 728;

    // Menu EXP (Java xử lý bằng form thông báo)
    public const int MENU_EXP = 729;

    // Mở form nạp tiền (Java: MENU_NAPTIEN)
    public const int MENU_TOP_UP = 730;

    // Thông tin (Java không dùng)
    public const int INFO = 1000;

    // Nhà mạng Viettel khi nạp thẻ (Java không dùng)
    public const int VIETTEL = 5033;

    // Nhà mạng Mobifone khi nạp thẻ (Java không dùng)
    public const int MOBIFONE = 5044;

    // Nhà mạng Vinaphone khi nạp thẻ (Java không dùng)
    public const int VINAPHONE = 5055;

    // Chỉ số menu khác

    // Admin gọi boss
    public const int CALL_BOSS = 599;

    // Đập đồ kích hoạt; Java không dùng (Java: MENU_DAP_DO_KICH_HOAT)
    public const int MENU_UPGRADE_ACTIVATION_ITEM = 550;

    // Menu Đại hội võ thuật (Java không dùng); Java khai báo static không final nên giữ là biến static (Java: MENU_DHVT)
    public static int MENU_MARTIAL_ARTS_TOURNAMENT = 560;

    // Rương phụ; Java chỉ còn trong code đã comment (Java: RUONG_PHU)
    public const int SECONDARY_CHEST = 9999;

    // Mua đệ tử VIP; Java không dùng (Java: BUY_DE_TU_VIP)
    public const int BUY_VIP_PET = 3901;

    // Nạp tích lũy; Java không dùng (Java: NAPTICHLUY)
    public const int ACCUMULATED_TOP_UP = 5839;

    // Menu người chơi: giftcode, đổi mật khẩu, cộng chỉ số nhanh
    public const int MENU_PLAYER = 5239;

    // Bảng xếp hạng (Java không dùng)
    public const int TOP = 5229;

    // Quy đổi cải trang VIP theo tổng nạp (xu) ở Quy Lão Kame (Java: QUY_DOI_XU)
    public const int EXCHANGE_COIN = 52333;

    // Nhận đệ tử thường ở Ông Gôhan (Java: NHAN_DE_TU)
    public const int RECEIVE_PET = 52334;

    // Mở thành viên (MTV) miễn phí (Java: MTVFREE)
    public const int FREE_MEMBERSHIP = 52335;

    // Xác nhận mua vé tháng (Java: CONFIRM_BUY_VE_THANG)
    public const int CONFIRM_BUY_MONTHLY_PASS = 52336;

    // Xác nhận mua vé tuần (Java: CONFIRM_BUY_VE_TUAN)
    public const int CONFIRM_BUY_WEEKLY_PASS = 52337;
}
