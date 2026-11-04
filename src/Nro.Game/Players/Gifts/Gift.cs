namespace Nro.Game.Players.Gifts;

// Giữ liên kết chủ sở hữu cho dữ liệu quà tặng.
// Nguồn Java: nro/player/Gift.java (PLACEHOLDER(H-00) giữ nguyên: sẽ được worker H thay thế).


public class Gift
{

    // Người chơi sở hữu dữ liệu quà tặng này (Java: player).

    public Player Player { get; }

    // Gắn dữ liệu quà tặng vào người chơi.

    public Gift(Player player)
    {
        Player = player;
    }
}
