using Nro.Core.IO;
using Nro.Game.Players;
using Nro.Game.Players.Sessions;
using Nro.Game.World.Maps;
using Nro.Host.Adapters.Packets;
using Nro.Network.Packets.Players;
using Nro.Tests.Unit.Game.World;

namespace Nro.Tests.Unit.Network.Packets.Players;

// Byte-exact cho I-01, tính tay từ Service.java/PlayerService.java và PROTOCOL.md; chưa có golden chạy Java thật.
// Nguồn Java: nro/services/Service.java (dòng 119–145, 417–513) và nro/services/PlayerService.java (dòng 114–177).
public sealed class PlayerPacketsTests
{
    // -128/0: b 00, i 01020304, s FFFF, b FE, b 01, s FFFE, b FF theo Service.addEffectChar.
    [Fact]
    public void Character_effect_packet_is_byte_exact_and_preserves_signed_narrowing()
    {
        Message message = CharacterEffectPacketCodec.Encode(new CharacterEffectPacket(0x01020304, -1, -2, 1, -2, -1));

        Assert.Equal((sbyte)-128, message.Command);
        Assert.Equal(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0xff, 0xff, 0xfe, 0x01, 0xff, 0xfe, 0xff }, message.GetData());
    }

    // -30/5: b 05 + i 7FFFFFFF, vì CrisGH chặn 2147483648 ở int.MaxValue khi Manager.readInt=true.
    [Fact]
    public void Current_hp_packet_clamps_to_int_and_has_the_java_sub_command()
    {
        Message message = CurrentHpPacketCodec.Encode(new CurrentHpPacket(2_147_483_648L), readInt: true);

        Assert.Equal((sbyte)-30, message.Command);
        Assert.Equal(new byte[] { 0x05, 0x7f, 0xff, 0xff, 0xff }, message.GetData());
    }

    // -30/6: b 06 + L FFFFFFFFFFFFFFFE when Manager.readInt=false, exactly WriteCris(long,false).
    [Fact]
    public void Current_mp_packet_uses_long_when_read_int_is_disabled()
    {
        Message message = CurrentMpPacketCodec.Encode(new CurrentMpPacket(-2), readInt: false);

        Assert.Equal(new byte[] { 0x06, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe }, message.GetData());
    }

    // -30/4 at version 214: b 04, L gold, i gem, i hp, i mp, i ruby; every number is big-endian.
    [Fact]
    public void Player_info_packet_uses_long_gold_at_version_214()
    {
        var packet = new PlayerInfoHpMpMoneyPacket(0x0102030405060708L, -1, 0x11223344, -2, 0x55667788);
        Message message = PlayerInfoHpMpMoneyPacketCodec.Encode(packet, clientVersion: 214, readInt: true);

        Assert.Equal(new byte[]
        {
            0x04, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0xff, 0xff, 0xff, 0xff, 0x11, 0x22, 0x33, 0x44,
            0xff, 0xff, 0xff, 0xfe, 0x55, 0x66, 0x77, 0x88,
        }, message.GetData());
    }

    // -30/4 before version 214: Java casts gold long to i, retaining low 32 bits 05060708.
    [Fact]
    public void Player_info_packet_truncates_gold_to_int_before_version_214()
    {
        Message message = PlayerInfoHpMpMoneyPacketCodec.Encode(new PlayerInfoHpMpMoneyPacket(0x0102030405060708L, 0, 0, 0, 0), clientVersion: 213, readInt: true);

        Assert.Equal(new byte[] { 0x04, 0x05, 0x06, 0x07, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, message.GetData());
    }

    // Java DataOutput.writeUTF dùng modified UTF-8: A=41, NUL=C080, U+1F600=EDA0BDEDB880; adapter không thay payload broadcast.
    [Fact]
    public void Broadcast_preserves_utf_payload_and_online_registry_order()
    {
        var sent = new List<string>();
        Player first = NewPlayer(1, "first", sent);
        Player second = NewPlayer(2, "second", sent);
        var adapter = new BroadcastPacketAdapter(() => [first, second], _ => null);
        Message message = new(44);
        message.Writer().WriteInt(0x01020304);
        message.Writer().WriteUTF("A\0😀");

        adapter.SendToAll(message);

        Assert.Equal(["first", "second"], sent);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x00, 0x09, 0x41, 0xc0, 0x80, 0xed, 0xa0, 0xbd, 0xed, 0xb8, 0x80 }, ((RecordingSession)first.Session!.Transport!).Packets.Single().Payload);
    }

    // Zone overload loops i=size-1..0 in Java; Player overload loops i=0..size-1. Session null must be ignored.
    [Fact]
    public void Zone_broadcast_uses_the_java_direction_and_ignores_missing_session()
    {
        var sent = new List<string>();
        Zone zone = NewZone();
        Player one = NewPlayer(1, "one", sent, zone);
        Player two = NewPlayer(2, "two", sent, zone);
        Player noSession = new() { Id = 3, IsPlayer = true, Zone = zone };
        zone.Players.Add(one);
        zone.Players.Add(two);
        zone.Players.Add(noSession);
        var adapter = new BroadcastPacketAdapter(() => [], _ => null);
        Message message = new(7);

        adapter.SendToZone(zone, message);
        adapter.SendToZoneOf(one, message);

        Assert.Equal(["two", "one", "one", "two"], sent);
    }

    // addEffectChar ghi id một lần rồi gửi -128/0 theo thứ tự zone Player overload Java.
    [Fact]
    public void Add_character_effect_deduplicates_state_and_sends_the_effect_packet()
    {
        var sent = new List<string>();
        Zone zone = NewZone();
        Player player = NewPlayer(0x01020304, "owner", sent, zone);
        zone.Players.Add(player);
        var adapter = new BroadcastPacketAdapter(() => [player], _ => null);

        adapter.AddCharacterEffect(player, -1, -2, 1, -2, -1);
        adapter.AddCharacterEffect(player, -1, -2, 1, -2, -1);

        Assert.Equal([-1], player.IdEffChar);
        Assert.Equal(["owner", "owner"], sent);
        RecordingSession session = (RecordingSession)player.Session!.Transport!;
        Assert.Equal(new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0xff, 0xff, 0xfe, 0x01, 0xff, 0xfe, 0xff }, session.Packets[0].Payload);
    }

    // Adapter vitals lấy version từ MySession, gửi qua IPlayerSessionTransport giả và không ném khi player chưa có session.
    [Fact]
    public void Player_packet_adapter_sends_vitals_through_the_session_transport()
    {
        var sent = new List<string>();
        Player player = NewPlayer(4, "vitals", sent, version: 214);
        player.Inventory.Gold = 9;
        player.Inventory.Gem = 8;
        player.NPoint.Hp = 7;
        player.NPoint.Mp = 6;
        player.Inventory.Ruby = 5;
        var adapter = new PlayerPacketAdapter(readInt: true);

        adapter.SendCurrentHp(player, player.NPoint.Hp);
        adapter.SendCurrentMp(player, player.NPoint.Mp);
        adapter.SendInfoHpMpMoney(player);
        adapter.SendCurrentHp(new Player { IsPlayer = true }, 1);

        RecordingSession session = (RecordingSession)player.Session!.Transport!;
        Assert.Equal(new sbyte[] { -30, -30, -30 }, session.Packets.Select(p => p.Command));
        Assert.Equal(new byte[] { 0x04, 0, 0, 0, 0, 0, 0, 0, 9, 0, 0, 0, 8, 0, 0, 0, 7, 0, 0, 0, 6, 0, 0, 0, 5 }, session.Packets[2].Payload);
    }

    // -42: 7×i, b FF/14/14/01, i, i, b FE, L, s 100, s FFFE, b FF. D17 giữ defg hai byte Java.
    [Fact]
    public void Player_point_packet_is_byte_exact_and_keeps_the_java_short_base_defense()
    {
        Message message = PlayerPointPacketCodec.Encode(new PlayerPointPacket(1, -2, 0x01020304, -1, 2, 3, 4, -1, 0x05060708, -9, -2, 0x0102030405060708L, -2, -1));

        Assert.Equal((sbyte)-42, message.Command);
        Assert.Equal(new byte[]
        {
            0,0,0,1, 0xff,0xff,0xff,0xfe, 1,2,3,4, 0xff,0xff,0xff,0xff, 0,0,0,2, 0,0,0,3, 0,0,0,4,
            0xff,20,20,1, 5,6,7,8, 0xff,0xff,0xff,0xf7, 0xfe, 1,2,3,4,5,6,7,8, 0,100, 0xff,0xfe, 0xff,
        }, message.GetData());
    }

    // -17 ép id xuống b 04; -8 ép cùng id xuống s 0304, cPk 0; tọa độ -1 và 65536 giữ low 16 bit Java.
    [Fact]
    public void Character_death_packets_are_byte_exact_for_narrowing_and_protocol_d9()
    {
        var packet = new CharacterDeathPacket(0x01020304, -1, 65536);

        Assert.Equal(new byte[] { 0x04, 0xff, 0xff, 0, 0 }, CharacterDeathPacketCodec.EncodeSelf(packet).GetData());
        Assert.Equal(new byte[] { 0x03, 0x04, 0, 0xff, 0xff, 0, 0 }, CharacterDeathPacketCodec.EncodeOther(packet).GetData());
    }

    // -30/0: tính tay từ Java: b sub, i id, b task/gender, s head, UTF modified (A/NUL/😀), rồi các sentinel/đếm rỗng và tail cố định.
    [Fact]
    public void Character_load_packet_is_byte_exact_with_modified_utf_and_overflowing_gold()
    {
        var packet = new CharacterLoadPacket(
            0x01020304, -2, -1, -2, "A\0😀", -3, -2, [-1], 0x0102030405060708L, -1, 0x11223344,
            [new CharacterLoadItem(-1, 0, "", "", [])], [], [], [], true, -2, true, -2, -1, -2);

        Message message = CharacterLoadPacketCodec.Encode(packet, clientVersion: 213);

        Assert.Equal((sbyte)-30, message.Command);
        Assert.Equal(new byte[]
        {
            0, 1,2,3,4, 0xfe, 0xff, 0xff,0xfe, 0,9, 0x41,0xc0,0x80,0xed,0xa0,0xbd,0xed,0xb8,0x80,
            0, 0xfd, 0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xfe, 0,0, 0,0, 0xff, 1, 0xff,0xff,
            5,6,7,8, 0xff,0xff,0xff,0xff, 0x11,0x22,0x33,0x44, 1, 0xff,0xff, 0, 0, 0,0,
            2,2, 2,3, 2,0x19, 1, 0xff,0xff,0xff,0xfe, 1, 0xff,0xfe, 0xff, 0xff,0xfe,
        }, message.GetData());
    }

    // Adapter I-02 bỏ session null/đóng như Player.SendMessage/IPlayerSessionTransport; không được ném ở đường login/chết.
    [Fact]
    public void Character_packet_adapter_ignores_missing_or_closed_session()
    {
        var sent = new List<string>();
        Player open = NewPlayer(1, "closed", sent);
        ((RecordingSession)open.Session!.Transport!).Disconnect();
        var adapter = new PlayerPacketAdapter(readInt: true);

        adapter.SendPlayerPoint(new Player { IsPlayer = true });
        adapter.SendCharacterLoad(new Player { IsPlayer = true }, []);
        adapter.SendCharacterDeath(new Player { IsPlayer = true });
        adapter.SendPlayerPoint(open);
        adapter.SendCharacterLoad(open, []);
        adapter.SendCharacterDeath(open);

        Assert.Empty(sent);
    }

    private static Player NewPlayer(long id, string label, List<string> sent, Zone? zone = null, int version = 0)
    {
        var transport = new RecordingSession(label, sent);
        var player = new Player { Id = id, IsPlayer = true, Zone = zone, SessionGateway = new PlayerSessionGatewayAdapter() };
        player.SetSession(new MySession { Version = version, Transport = transport });
        return player;
    }

    private static Zone NewZone()
    {
        var game = new FakeGame();
        var map = new Map(game.Runtime, 1, "test", 0, 0, 0, 0, 0, [], [], 0, 7, [], [], 0);
        return new Zone(map, 0, 7);
    }

}
