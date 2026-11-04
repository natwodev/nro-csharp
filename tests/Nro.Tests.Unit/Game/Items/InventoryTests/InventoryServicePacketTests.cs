using System.Text;
using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary>
/// Byte-exact checks of the three inventory packets (cmd -36 bag, -37 body, -35 box; layout in docs/PROTOCOL.md). The expected bytes are assembled here by hand
/// (big-endian numbers, 2-byte length + UTF-8 text) and do not use <see cref="Nro.Core.IO.Message"/>.
/// </summary>
public class InventoryServicePacketTests
{
    private static byte[] Utf(string s)
    {
        byte[] b = Encoding.UTF8.GetBytes(s);
        return [(byte)(b.Length >> 8), (byte)b.Length, .. b];
    }

    private static byte[] I(int v) => [(byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v];

    private static byte[] S(int v) => [(byte)(v >> 8), (byte)v];

    private static byte[] Cat(params byte[][] parts) => parts.SelectMany(p => p).ToArray();

    private static byte[] ExpectedItem(int id, int quantity, string info, string content, params (int Id, int Param)[] options) =>
        Cat(S(id), I(quantity), Utf(info), Utf(content), [(byte)options.Length], Cat(options.Select(o => Cat(I(o.Id), I(o.Param))).ToArray()));

    [Fact]
    public void SendItemBag_writes_cmd_minus_36_with_one_record_per_slot()
    {
        var k = new InvKit();
        k.Fill(k.Inv.ItemsBag, 2);
        k.Define(5, strRequire: 1500, name: "Ao");
        k.Inv.ItemsBag[0] = k.Item(5, 3, (1, 7), (4, 200));

        k.Service.SendItemBag(k.Player);

        var packet = Assert.Single(k.Notifier.Packets);
        Assert.Equal(-36, packet.Command);
        Assert.Equal(1, packet.Player.Id);
        byte[] expected = Cat(
            [0, 2],
            ExpectedItem(5, 3, "option1 7option4 200", "Yêu cầu sức mạnh 1500 trở lên", (1, 7), (4, 200)),
            S(-1));
        Assert.Equal(expected, packet.Data);
    }

    [Fact]
    public void SendItemBag_sorts_the_bag_before_writing_it()
    {
        var k = new InvKit();
        k.Fill(k.Inv.ItemsBag, 3);
        k.Inv.ItemsBag[2] = k.Item(6);

        k.Service.SendItemBag(k.Player);

        Assert.Equal(6, k.Inv.ItemsBag[0].Template!.Id);
        byte[] data = k.Notifier.Packets.Single().Data;
        Assert.Equal(new byte[] { 0, 3, 0, 6 }, data.Take(4));
        Assert.Equal(new byte[] { 255, 255, 255, 255 }, data.TakeLast(4));
    }

    [Fact]
    public void SendItemBag_of_an_empty_bag_is_two_bytes()
    {
        var k = new InvKit();
        k.Inv.ItemsBag.Clear();

        k.Service.SendItemBag(k.Player);

        Assert.Equal(new byte[] { 0, 0 }, k.Notifier.Packets.Single().Data);
    }

    [Fact]
    public void SendItemBag_writes_negative_quantities_and_parameters_as_two_complement()
    {
        var k = new InvKit();
        k.Fill(k.Inv.ItemsBag, 1);
        k.Inv.ItemsBag[0] = k.Item(7, -1, (1, -2));

        k.Service.SendItemBag(k.Player);

        byte[] expected = Cat([0, 1], ExpectedItem(7, -1, "option1 -2", "Yêu cầu sức mạnh 0 trở lên", (1, -2)));
        Assert.Equal(expected, k.Notifier.Packets.Single().Data);
    }

    [Fact]
    public void SendItemBody_writes_cmd_minus_37_with_the_head_and_then_the_refresh_calls()
    {
        var k = new InvKit();
        k.Host.Head = 321;
        k.Inv.ItemsBody[0] = k.Item(8, 1);

        k.Service.SendItemBody(k.Player);

        var packet = Assert.Single(k.Notifier.Packets);
        Assert.Equal(-37, packet.Command);
        byte[] expected = Cat(
            [0],
            S(321),
            [(byte)InvKit.BodySlots],
            ExpectedItem(8, 1, string.Empty, "Yêu cầu sức mạnh 0 trở lên"),
            Cat(Enumerable.Repeat(S(-1), InvKit.BodySlots - 1).ToArray()));
        Assert.Equal(expected, packet.Data);
        Assert.Equal(new[] { "SendCostume:1", "SendFlagBag" }, k.Notifier.Calls);
    }

    [Fact]
    public void SendItemBody_sends_the_chan_menh_effect_for_a_type_39_item_in_slot_11()
    {
        var k = new InvKit();
        k.Define(2003, 39);
        k.Inv.ItemsBody[11] = k.Item(2003);

        k.Service.SendItemBody(k.Player);

        Assert.Equal(new[] { "SendTrueSonOfHeaven:2003", "SendCostume:1", "SendFlagBag" }, k.Notifier.Calls);
    }

    [Fact]
    public void SendItemBody_with_a_short_body_throws_after_the_packet_was_sent()
    {
        var k = new InvKit();
        k.Fill(k.Inv.ItemsBody, 5);

        Assert.Throws<ArgumentOutOfRangeException>(() => k.Service.SendItemBody(k.Player));
        Assert.Single(k.Notifier.Packets);
    }

    [Fact]
    public void SendItemBox_writes_the_box_and_then_the_open_packet()
    {
        var k = new InvKit();
        k.Fill(k.Inv.ItemsBox, 2);
        k.Inv.ItemsBox[1] = k.Item(9, 2, (30, 0));

        k.Service.SendItemBox(k.Player);

        Assert.Equal(2, k.Notifier.Packets.Count);
        Assert.All(k.Notifier.Packets, p => Assert.Equal(-35, p.Command));
        byte[] expected = Cat([0, 2], S(-1), ExpectedItem(9, 2, "option30 0", "Yêu cầu sức mạnh 0 trở lên", (30, 0)));
        Assert.Equal(expected, k.Notifier.Packets[0].Data);
        Assert.Equal(new byte[] { 1 }, k.Notifier.Packets[1].Data);
    }

    [Fact]
    public void SendItemBoxCollection_and_the_open_methods_use_the_same_command()
    {
        var k = new InvKit();
        k.Fill(k.Inv.ItemsBoxCollection, 1);

        k.Service.SendItemBoxCollection(k.Player);
        k.Service.OpenBox(k.Player);
        k.Service.OpenBoxCollection(k.Player);

        Assert.Equal(new[] { new byte[] { 0, 1, 255, 255 }, new byte[] { 1 }, new byte[] { 1 }, new byte[] { 1 } }, k.Notifier.Packets.Select(p => p.Data));
        Assert.All(k.Notifier.Packets, p => Assert.Equal(-35, p.Command));
    }

    [Fact]
    public void Clan_box_packets_send_the_list_and_optionally_the_open_packet()
    {
        var k = new InvKit();
        k.Player.Clan = InvKit.Raw<Clan>();
        k.Host.ClanBox.Add(k.Item(9));

        k.Service.SendItemClan(k.Player);
        Assert.Single(k.Notifier.Packets);

        k.Service.SendItemBoxClan(k.Player);
        Assert.Equal(3, k.Notifier.Packets.Count);

        k.Service.OpenBoxClan(k.Player);
        Assert.Equal(new byte[] { 1 }, k.Notifier.Packets[^1].Data);
        Assert.Equal(k.Notifier.Packets[0].Data, k.Notifier.Packets[1].Data);
    }

    [Fact]
    public void A_failure_while_building_a_packet_is_logged_and_nothing_is_sent()
    {
        var k = new InvKit();
        k.Inv.ItemsBox[0] = null!;

        k.Service.SendItemBox(k.Player);

        // the list packet failed (null slot) and was skipped; the open packet still went out
        Assert.Single(k.Notifier.Packets);
        Assert.Equal(new byte[] { 1 }, k.Notifier.Packets[0].Data);
    }
}
