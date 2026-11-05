using Nro.Game.Players.Ports;
using Nro.Game.World.Maps;

namespace Nro.Game.Players.Gifts;

// Nguồn Java: source-game/SRC_6/AWN_Version/AWN_Version/src/nro/player/DropItem.java (dòng 1–47).
public sealed class DropItem : IDisposable
{
    // Người chơi sở hữu dữ liệu vật phẩm rơi, null sau dispose (Java: player).
    public Player? Player { get; private set; }

    // Gắn dữ liệu vật phẩm rơi vào người chơi (Java: DropItem).
    public DropItem(Player player) => Player = player ?? throw new ArgumentNullException(nameof(player));

    // Rơi riêng DHVT 726 tại map 52 khi túi còn chỗ và chưa có vật phẩm tương ứng (Java: update).
    public void Update(IDropItemGateway gateway, IPlayerRandom random)
    {
        ArgumentNullException.ThrowIfNull(gateway);
        ArgumentNullException.ThrowIfNull(random);
        Player? player = Player;
        Zone? zone = player?.Zone;
        if (player is null || !player.IsPl() || zone is null || zone.Map.MapId != 52 || gateway.GetEmptyBagCount(player) <= 0
            || gateway.HasBagItem(player, 726) || gateway.HasItemMapForPlayer(player, 726))
        {
            return;
        }

        int x = random.NextInt(100, zone.Map.MapWidth - 100);
        int y = zone.Map.YPhysicInTop(x, 100);
        gateway.DropItemMapForPlayer(player, zone, 726, 1, x, y, [new ItemMapOption(30, 0), new ItemMapOption(93, 1)]);
    }

    // Bỏ liên kết chủ sở hữu như Java (Java: dispose).
    public void Dispose() => Player = null;
}
