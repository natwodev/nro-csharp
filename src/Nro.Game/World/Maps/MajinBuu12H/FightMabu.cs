namespace Nro.Game.World.Maps.MajinBuu12H;

// Đại diện người chơi trong trận Ma Bư lúc 12 giờ.
// Nguồn Java: nro/map/MajinBuu12H/FightMabu.java (PLACEHOLDER(H-00): sẽ do worker P thay thế).
public class FightMabu
{
    // Người chơi tham gia trận.
    public Player Player { get; }

    // Gắn trận với người chơi tham gia.
    public FightMabu(Player player)
    {
        Player = player;
    }
}
