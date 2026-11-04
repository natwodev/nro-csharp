namespace Nro.Game.Players;

// Giữ liên kết chủ sở hữu cho trạng thái bộ trang phục.
// Nguồn Java: nro/player/SetClothes.java (PLACEHOLDER(H-00) giữ nguyên: sẽ được worker H thay thế).


public class SetClothes
{

    // Người chơi sở hữu các hiệu ứng của bộ trang phục (Java: player).

    public Player Player { get; }

    // Gắn trạng thái bộ trang phục vào người chơi.

    public SetClothes(Player player)
    {
        Player = player;
    }
}
