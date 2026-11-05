namespace Nro.Game.Players.Ports;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Player.java (dòng 2144–2446, System.currentTimeMillis).
// Cổng thời gian combat để test dựng mốc chính xác; production lấy đồng hồ zone/host.
public interface IPlayerClock
{
    // Trả mili giây epoch hiện tại (Java: System.currentTimeMillis).
    long NowMilliseconds { get; }
}
