using Nro.Core.Consts;
using Nro.Game.Players.Ports;

namespace Nro.Game.Players.Relations;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/Fusion.java (dòng 1–35).
public sealed class Fusion : IDisposable
{
    // Thời lượng Lưỡng long nhất thể theo mili giây (Java: TIME_FUSION).
    public const int TIME_FUSION = 600_000;

    // Người chơi đang giữ trạng thái hợp thể, null sau dispose (Java: player).
    public Player? Player { get; private set; }

    // Kiểu hợp thể hiện tại (Java: typeFusion).
    public sbyte TypeFusion { get; set; }

    // Mốc bắt đầu hợp thể theo mili giây epoch (Java: lastTimeFusion).
    public long LastTimeFusion { get; set; }

    // Gắn trạng thái hợp thể vào người chơi (Java: Fusion).
    public Fusion(Player player) => Player = player ?? throw new ArgumentNullException(nameof(player));

    // Tự tách Lưỡng long nhất thể sau đúng 600000 ms qua đồng hồ/cổng tiêm (Java: update).
    public void Update(IPlayerClock clock, IFusionGateway gateway)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(gateway);
        if (Player is not null && TypeFusion == ConstPlayer.FUSION_DANCE && clock.NowMilliseconds - LastTimeFusion >= TIME_FUSION)
        {
            gateway.UnfusePet(Player);
        }
    }

    // Bỏ liên kết người chơi như Java (Java: dispose).
    public void Dispose() => Player = null;
}
