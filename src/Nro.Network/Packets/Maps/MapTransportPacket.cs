namespace Nro.Network.Packets.Maps;

// Dữ liệu gói MAP_TRASPORT chọn bản đồ; Sections giữ fall-through Capsule→BlackBall bất thường của Java.
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 64–100).
public sealed record MapTransportPacket(IReadOnlyList<IReadOnlyList<MapTransportDestination>> Sections);
