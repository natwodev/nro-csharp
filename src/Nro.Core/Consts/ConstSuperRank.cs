namespace Nro.Core.Consts;

// Hằng số Giải Siêu Hạng (võ đài xếp hạng, thách đấu người hạng cao hơn): câu thông báo và câu chat trong trận
// Nguồn Java: consts/ConstSuperRank.java (tên đổi sang tiếng Anh, tên Java gốc ghi cuối chú thích nếu khác; giá trị đối chiếu bằng ConstsParityTests)
public static class ConstSuperRank
{
    // Báo khi thách đấu người xếp hạng thấp hơn mình (Java: TEXT_DUOI_HANG)
    public const string TEXT_BELOW_OWN_RANK = "Không thể thi đấu dưới hạng mình";

    // Báo khi thách đấu người cùng hạng, tức chính mình (Java: TEXT_CHINH_MINH)
    public const string TEXT_YOURSELF = "Không thể thi đấu với chính mình";

    // Báo khi chọn thách đấu từ danh sách Top 100 (danh sách này chỉ để xem)
    public const string TEXT_TOP_100 = "Không thể thách đấu ở Top 100";

    // Báo khi đối thủ đang chờ lên võ đài cho trận khác (Java: TEXT_DOI_THU_CHO_THI_DAU)
    public const string TEXT_OPPONENT_WAITING = "Đối thủ đang chờ để lên võ đài";

    // Báo khi bản thân đang chờ thi đấu; cũng là trạng thái hiện trong thông tin người chơi (Java: TEXT_DANG_CHO)
    public const string TEXT_WAITING = "Đang chờ để thi đấu";

    // Báo khi võ đài đang có trận, lời thách đấu được xếp vào hàng chờ (Java: TEXT_CHO_IT_PHUT)
    public const string TEXT_WAIT_A_FEW_MINUTES = "Vui lòng chờ ít phút để lên võ đài";

    // Báo khi đối thủ đang thi đấu trận khác (Java: TEXT_DOI_THU_DANG_THI_DAU)
    public const string TEXT_OPPONENT_COMPETING = "Đối thủ đang thi đấu";

    // Báo khi bản thân đang trong một trận (Java: TEXT_DANG_THI_DAU)
    public const string TEXT_COMPETING = "Đang thi đấu";

    // Báo khi thách đấu người trong Top 10 cao hơn mình quá 2 hạng (Java: TEXT_KHONG_THE_THI_DAU_TREN_2_HANG)
    public const string TEXT_MORE_THAN_2_RANKS_ABOVE = "Không thể thi đấu trên mình 2 hạng";

    // Thông báo toàn máy chủ khi người thắng lên Top 10; %1 là tên, %2 là hạng mới
    public const string TEXT_TOP_10 = "%1 vừa lên Top %2 Giải Siêu Hạng";

    // Người thách đấu nói khi thắng; %1 là hạng mới (Java: TEXT_THANG)
    public const string TEXT_WIN = "Thắng rồi haha, lên hạng %1";

    // Người thách đấu nói khi thua (Java: TEXT_THUA)
    public const string TEXT_LOSE = "Thua rồi";

    // Bản sao đối thủ (boss Siêu Hạng) nói khi rời bản đồ mà người thách đấu đã thua (Java: TEXT_CLONE_THANG)
    public const string TEXT_CLONE_WIN = "Đầu hàng chưa?";

    // Bản sao đối thủ nói khi rời bản đồ mà người thách đấu vẫn trên võ đài và chưa chết (Java: TEXT_CLONE_THUA)
    public const string TEXT_CLONE_LOSE = "Mạnh quá, ta chịu thua";

    // Đối thủ hỏi trước khi trận bắt đầu (Java: TEXT_SAN_SANG_CHUA)
    public const string TEXT_ARE_YOU_READY = "Sẵn sàng chưa?";

    // Người thách đấu đáp lại, ngay sau đó hai bên chuyển sang kiểu PK đối kháng (Java: TEXT_SAN_SANG)
    public const string TEXT_READY = "OK";
}
