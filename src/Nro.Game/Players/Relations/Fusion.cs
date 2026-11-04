namespace Nro.Game.Players.Relations;

// Giữ liên kết chủ sở hữu cho trạng thái hợp thể.
// Nguồn Java: nro/player/Fusion.java (PLACEHOLDER(H-00) giữ nguyên: sẽ được worker H thay thế).


public class Fusion
{

    // Người chơi đang giữ trạng thái hợp thể này (Java: player).

    public Player Player { get; }

    // Gắn trạng thái hợp thể vào người chơi.

    public Fusion(Player player)
    {
        Player = player;
    }
}
