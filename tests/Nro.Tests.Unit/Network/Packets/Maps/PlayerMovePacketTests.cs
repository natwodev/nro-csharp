using Nro.Core.IO;
using Nro.Network.Packets.Maps;

namespace Nro.Tests.Unit.Network.Packets.Maps;

// Golden byte-exact gói chuyển động MapService.
// Nguồn Java: nro/services/MapService.java (dòng 424–436); docs/PROTOCOL.md, dòng 360.
public sealed class PlayerMovePacketTests
{
    // Java writeInt 01020304, writeShort FFFE, writeShort 8001; tất cả big-endian, nên payload có đúng tám byte này.
    [Fact]
    public void Player_move_packet_is_byte_exact()
    {
        Message message = PlayerMovePacketCodec.Encode(new PlayerMovePacket(0x01020304, -2, unchecked((short)0x8001)));

        Assert.Equal((sbyte)-7, message.Command);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04, 0xff, 0xfe, 0x80, 0x01 }, message.GetData());
        Assert.Equal(new PlayerMovePacket(0x01020304, -2, unchecked((short)0x8001)), PlayerMovePacketCodec.Decode(new Message(-7, message.GetData())));
    }
}
