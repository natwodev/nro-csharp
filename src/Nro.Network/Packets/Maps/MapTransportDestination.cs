namespace Nro.Network.Packets.Maps;

// Dữ liệu một đích đến trong bảng chuyển bản đồ (Java: Zone.map.mapName, planetName).
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 66–98).
public sealed record MapTransportDestination(string MapName, string PlanetName);
