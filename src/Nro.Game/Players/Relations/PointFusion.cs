namespace Nro.Game.Players.Relations;

// Giữ liên kết chủ sở hữu cho điểm hợp thể.
// Nguồn Java: nro/player/PointFusion.java (PLACEHOLDER(H-00) giữ nguyên: sẽ được worker H thay thế).


public class PointFusion
{

    // Người chơi sở hữu các điểm hợp thể này (Java: player).

    public Player Player { get; }

    // Gắn điểm hợp thể vào người chơi.

    public PointFusion(Player player)
    {
        Player = player;
    }
}
