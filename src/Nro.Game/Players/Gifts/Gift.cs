namespace Nro.Game.Players.Gifts;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Gift.java (dòng 4–19).
public class Gift : IDisposable
{
    // Lưu người chơi sở hữu trạng thái quà, được bỏ liên kết khi hủy (Java: player).
    public Player? Player { get; private set; }

    // Lưu cờ đã nhận quà vàng tân thủ (Java: goldTanThu).
    public bool IsNewbieGoldClaimed { get; set; }

    // Lưu cờ đã nhận quà ngọc tân thủ (Java: gemTanThu).
    public bool IsNewbieGemClaimed { get; set; }

    // Gắn trạng thái quà vào người chơi (Java: Gift).
    public Gift(Player player)
    {
        Player = player;
    }

    // Bỏ liên kết với người chơi như Java (Java: dispose).
    public void Dispose()
    {
        Player = null;
    }
}
