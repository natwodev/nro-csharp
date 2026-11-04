using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Nro.Core.Util;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.IO;
using Nro.Game;
using Nro.Tests.Unit.Helpers;
using Nro.Tests.Unit.Core.Logging;
using Nro.Tests.Unit.Game.Effects;
using static Nro.Tests.Unit.Game.Effects.Pkt;

namespace Nro.Tests.Unit.Game.Effects.Services;

public class EffectMapServiceTests
{
    private sealed class ThrowingBroadcaster : IEffectBroadcaster
    {
        public void SendToPlayer(Player player, Message message) => throw new InvalidOperationException("boom");

        public void SendToZoneOf(Player player, Message message) => throw new InvalidOperationException("boom");

        public void SendToZone(Zone zone, Message message) => throw new InvalidOperationException("boom");

        public void SendToZoneOfMob(IEffectMobTarget mob, Message message) => throw new InvalidOperationException("boom");

        public void SendToZoneExcept(Player player, Message message) => throw new InvalidOperationException("boom");

        public void SendToAll(Message message) => throw new InvalidOperationException("boom");
    }

    private static (EffectMapService Service, FakeWorld World) Make()
    {
        var world = new FakeWorld();
        return (new EffectMapService(world, new GameLogger(new RecordingLogger())), world);
    }

    private static Zone NewZone() => (Zone)RuntimeHelpers.GetUninitializedObject(typeof(Zone));

    [Fact]
    public void SendEffectMapToPlayer_writes_id_twice_and_ignores_loop()
    {
        (EffectMapService service, FakeWorld world) = Make();
        Player player = world.Add(null, 7);
        service.SendEffectMapToPlayer(player, 9, 3, 77, 300, 400, 50);
        SentPacket packet = Assert.Single(world.Packets);
        Assert.Equal("Player:7", packet.Scope);
        Assert.Equal(113, packet.Command);
        Assert.Equal(Of(B(9), B(3), B(9), S(300), S(400), S(50)), packet.Data);
    }

    [Fact]
    public void SendEffectMapToAllInMap_player_writes_loop_layer_id()
    {
        (EffectMapService service, FakeWorld world) = Make();
        Player player = world.Add(null, 7);
        service.SendEffectMapToAllInMap(player, 9, 3, 2, 300, 400, 50);
        SentPacket packet = Assert.Single(world.Packets);
        Assert.Equal("ZoneOf:7", packet.Scope);
        Assert.Equal(113, packet.Command);
        Assert.Equal(Of(B(2), B(3), B(9), S(300), S(400), S(50)), packet.Data);
    }

    [Fact]
    public void SendEffectMapToAllInMap_zone_goes_to_the_zone()
    {
        (EffectMapService service, FakeWorld world) = Make();
        service.SendEffectMapToAllInMap(NewZone(), 1, 0, 1, -5, 32767, 0);
        SentPacket packet = Assert.Single(world.Packets);
        Assert.Equal("Zone", packet.Scope);
        Assert.Equal(Of(B(1), B(0), B(1), S(-5), S(32767), S(0)), packet.Data);
    }

    [Fact]
    public void Values_are_cut_to_the_wire_width_like_Java()
    {
        (EffectMapService service, FakeWorld world) = Make();
        Player player = world.Add(null, 7);
        service.SendEffectMapToAllInMap(player, 257, -1, 256, 65537, -65536, 70000);
        Assert.Equal(Of(B(0), B(0xFF), B(1), S(1), S(0), S(70000 - 65536)), Assert.Single(world.Packets).Data);
    }

    [Fact]
    public void HoaBang_on_goes_to_the_zone_with_the_player_id_cut_to_int()
    {
        (EffectMapService service, FakeWorld world) = Make();
        Player player = world.Add(null, 0x1_0000_0005L);
        service.SendFrozenCharacterEffect(player);
        SentPacket packet = Assert.Single(world.Packets);
        Assert.Equal("ZoneOf:" + player.Id, packet.Scope);
        Assert.Equal(-128, packet.Command);
        Assert.Equal(Of(B(0), 5, S(202), B(1), B(0xFF), S(10), B(0)), packet.Data);
    }

    [Fact]
    public void HoaBang_off_goes_to_the_player_and_then_to_everybody()
    {
        (EffectMapService service, FakeWorld world) = Make();
        Player player = world.Add(null, 12);
        service.RemoveFrozenCharacterEffect(player);
        Assert.Equal(2, world.Packets.Count);
        Assert.Equal("Player:12", world.Packets[0].Scope);
        Assert.Equal("All", world.Packets[1].Scope);
        byte[] expected = Of(B(1), 12, S(202), B(1), B(0xFF), S(10), B(1));
        Assert.All(world.Packets, p => Assert.Equal(-128, p.Command));
        Assert.All(world.Packets, p => Assert.Equal(expected, p.Data));
    }

    [Fact]
    public void A_failing_broadcaster_is_logged_and_never_thrown()
    {
        var log = new RecordingLogger();
        var service = new EffectMapService(new ThrowingBroadcaster(), new GameLogger(log));
        var player = new Player { Id = 1 };

        service.SendEffectMapToPlayer(player, 1, 1, 1, 1, 1, 1);
        service.SendFrozenCharacterEffect(player);
        service.RemoveFrozenCharacterEffect(player);
        service.SendEffectMapToAllInMap(NewZone(), 1, 1, 1, 1, 1, 1);
        service.SendEffectMapToAllInMap(player, 1, 1, 1, 1, 1, 1);

        Assert.Equal(5, log.Entries.Count(e => e.Level == LogLevel.Error && e.Exception is InvalidOperationException));
    }

    [Fact]
    public void Constructor_rejects_null_arguments()
    {
        Assert.Throws<ArgumentNullException>(() => new EffectMapService(null!, new GameLogger(new RecordingLogger())));
        Assert.Throws<ArgumentNullException>(() => new EffectMapService(new FakeWorld(), null!));
    }
}
