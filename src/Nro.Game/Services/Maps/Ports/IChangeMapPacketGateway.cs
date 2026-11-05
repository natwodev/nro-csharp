using Nro.Game.Players;
using Nro.Network.Packets.Maps;

namespace Nro.Game.Services.Maps.Ports;

// Cổng phát các gói thuộc ChangeMapService; Game không tạo Message.
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 64–100, 359, 401, 741–763).
public interface IChangeMapPacketGateway
{
    // Gửi bảng đích chuyển map -91 (Java: openChangeMapTab).
    void SendMapTransport(Player player, MapTransportPacket packet);

    // Phát hiệu ứng tàu/dịch chuyển -65 theo phạm vi Java (Java: spaceShipArrive).
    void SendSpaceShipArrival(Player player, SpaceShipArrivalPacket packet, SpaceShipArrivalAudience audience);
}
