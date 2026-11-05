using Nro.Core.IO;
using Nro.Network.Packets.Maps;

namespace Nro.Tests.Unit.Network.Packets.Maps;

// Golden byte-exact cho codec ChangeMap I-03, đối chiếu DataOutputStream Java và PROTOCOL.md.
// Nguồn Java: source-game/Teamobi2026/SRC/src/nro/models/map/service/ChangeMapService.java (dòng 64–100, 741–763).
public sealed class MapPacketTests
{
    // -91: b 02, UTF "Về nhà" (modified UTF-8) + UTF "Namek", UTF "Map" + UTF "Xayda".
    [Fact]
    public void Map_transport_packet_is_byte_exact()
    {
        Message message = MapTransportPacketCodec.Encode(new MapTransportPacket(
        [
            [new MapTransportDestination("Về nhà", "Namek"), new MapTransportDestination("Map", "Xayda")],
        ]));

        Assert.Equal((sbyte)-91, message.Command);
        Assert.Equal(new byte[]
        {
            0x02,
            0x00, 0x09, 0x56, 0xe1, 0xbb, 0x81, 0x20, 0x6e, 0x68, 0xc3, 0xa0,
            0x00, 0x05, 0x4e, 0x61, 0x6d, 0x65, 0x6b,
            0x00, 0x03, 0x4d, 0x61, 0x70,
            0x00, 0x05, 0x58, 0x61, 0x79, 0x64, 0x61,
        }, message.GetData());
        Assert.Equal(2, MapTransportPacketCodec.Decode(new Message(-91, message.GetData())).Sections[0].Count);
    }

    // -65: i 01020304, b FE giữ signed narrowing của Java writeByte.
    [Fact]
    public void Space_ship_arrival_packet_is_byte_exact()
    {
        Message message = SpaceShipArrivalPacketCodec.Encode(new SpaceShipArrivalPacket(0x01020304, -2));

        Assert.Equal((sbyte)-65, message.Command);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04, 0xfe }, message.GetData());
        Assert.Equal(new SpaceShipArrivalPacket(0x01020304, -2), SpaceShipArrivalPacketCodec.Decode(new Message(-65, message.GetData())));
    }

}
