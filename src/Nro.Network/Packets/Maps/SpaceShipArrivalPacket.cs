namespace Nro.Network.Packets.Maps;

// Dữ liệu hiệu ứng tàu vũ trụ/dịch chuyển (Java: spaceShipArrive).
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 741–763).
public sealed record SpaceShipArrivalPacket(int CharacterId, sbyte SpaceShipType);
