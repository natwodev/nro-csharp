namespace Nro.Game.Players.Gifts;

// Giữ liên kết chủ sở hữu cho trạng thái sự kiện người chơi.
// Nguồn Java: nro/player/PlayerEvent.java (PLACEHOLDER(H-00) giữ nguyên: sẽ được worker H thay thế).


public class PlayerEvent
{

    // Người chơi sở hữu tiến độ sự kiện này (Java: player).

    public Player Player { get; }

    // Gắn trạng thái sự kiện vào người chơi.

    public PlayerEvent(Player player)
    {
        Player = player;
    }
}
