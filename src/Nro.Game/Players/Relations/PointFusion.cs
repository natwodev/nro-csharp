namespace Nro.Game.Players.Relations;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/PointFusion.java (dòng 6–47).
public class PointFusion : IDisposable
{
    // Lưu điểm HP hợp thể (Java: HpFusion).
    public int FusionHp { get; set; }

    // Lưu điểm MP hợp thể (Java: MpFusion).
    public int FusionMp { get; set; }

    // Lưu điểm sát thương hợp thể (Java: DameFusion).
    public int FusionDamage { get; set; }

    // Lưu người chơi sở hữu điểm hợp thể, được bỏ liên kết khi hủy (Java: player).
    public Player? Player { get; private set; }

    // Gắn điểm hợp thể vào người chơi và khởi tạo ba điểm bằng không (Java: PointFusion).
    public PointFusion(Player player)
    {
        Player = player;
        FusionHp = 0;
        FusionMp = 0;
        FusionDamage = 0;
    }

    // Bỏ liên kết người chơi như Java (Java: dispose).
    public void Dispose()
    {
        Player = null;
    }
}
