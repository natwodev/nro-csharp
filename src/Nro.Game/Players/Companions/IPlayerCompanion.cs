namespace Nro.Game.Players.Companions;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/PetFollow.java (dòng 109–114), nro/player/SaiBaMen.java (dòng 115–121).
// Hợp đồng nội bộ để Player.Dispose() gọi đúng hủy companion thay vì chỉ gọi IDisposable không đa hình.
public interface IPlayerCompanion
{
    // Hủy companion qua cổng đã được nối, đồng thời gỡ liên kết chủ sở hữu (Java: dispose).
    void DisposeCompanion();
}
