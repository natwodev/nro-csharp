namespace Nro.Game.Players.Ports;

// Cổng phát vị trí người chơi; Game không tạo Message hoặc gọi broadcaster trực tiếp.
// Nguồn Java: nro/services/MapService.java (dòng 424–436).
public interface IPlayerMoveGateway
{
    // Phát gói -7 đến toàn bộ người chơi cùng zone, gồm player. (Java: Service.sendMessAllPlayerInMap)
    void SendPlayerMove(Player player);
}
