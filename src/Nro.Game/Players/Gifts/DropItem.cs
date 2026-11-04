namespace Nro.Game.Players.Gifts;

// Giữ liên kết chủ sở hữu cho dữ liệu vật phẩm rơi.
// Nguồn Java: nro/player/DropItem.java (PLACEHOLDER(H-00) giữ nguyên: sẽ được worker H thay thế).


public class DropItem
{

    // Người chơi sở hữu danh sách vật phẩm rơi này (Java: player).

    public Player Player { get; }

    // Gắn dữ liệu vật phẩm rơi vào người chơi.

    public DropItem(Player player)
    {
        Player = player;
    }
}
