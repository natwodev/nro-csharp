namespace Nro.Game.World.Maps.RankSuper;

// Đại diện người chơi trong bảng xếp hạng Siêu Hạng.
// Nguồn Java: nro/map/RankSuper/SuperRank.java (PLACEHOLDER(H-00): sẽ do worker P thay thế).
public class SuperRank
{
    // Người chơi được xếp hạng.
    public Player Player { get; }

    // Gắn bản ghi xếp hạng với người chơi.
    public SuperRank(Player player)
    {
        Player = player;
    }
}
