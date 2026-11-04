namespace Nro.Core.Templates;

// Lưu cặp người chơi đang chờ trận đấu Super Rank.
// Nguồn Java: nro/template/WaitSuperRank.java (giữ đúng cặp người chơi và Java chỉ có hàm tạo hai tham số, không có hàm tạo mặc định).
public class WaitSuperRank
{
    // Mã người chơi đang chờ (Java: playerId)
    public long PlayerId { get; set; }

    // Mã đối thủ (Java: rivalId)
    public long RivalId { get; set; }

    public WaitSuperRank(long playerId, long rivalId)
    {
        PlayerId = playerId;
        RivalId = rivalId;
    }
}
