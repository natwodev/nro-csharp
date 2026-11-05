namespace Nro.Game.Players;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Location.java (dòng 4–10).
public class Location
{
    // Lưu tọa độ ngang của người chơi (Java: x).
    public int X { get; set; }

    // Lưu tọa độ dọc của người chơi (Java: y).
    public int Y { get; set; }

    // Lưu thời điểm người chơi di chuyển lần cuối theo epoch mili giây (Java: lastTimeplayerMove).
    public long LastPlayerMoveTime { get; set; }
}
