using Nro.Game.World.Maps;

namespace Nro.Game.Players.Ports;

// Cổng trạng thái phó bản bang hội còn thuộc các service dungeon chưa port; không truy cập Clan placeholder trực tiếp.
// Nguồn Java: nro/services/MapService.java (dòng 56–245).
public interface IPlayerMapDungeonGateway
{
    // Có instance phó bản tương ứng của player hay không. (Java: player.clan.* != null)
    bool HasDungeonInstance(Player player, int mapId);

    // Lấy zone instance phó bản theo map id. (Java: player.clan.*.getMapById)
    Zone? GetDungeonZone(Player player, int mapId);
}
