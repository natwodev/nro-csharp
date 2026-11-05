namespace Nro.Network.Packets.Maps;

// Dữ liệu vị trí người chơi phát trong map. (Java: MapService.sendPlayerMove)
// Nguồn Java: nro/services/MapService.java (dòng 424–436).
public sealed record PlayerMovePacket(int PlayerId, short X, short Y);
