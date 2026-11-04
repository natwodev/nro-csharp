using Nro.Core.IO;
using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.Ports;

public class NullItemHostTests
{
    [Fact]
    public void The_null_host_is_one_shared_instance()
    {
        Assert.Same(NullItemHost.Instance, NullItemHost.Instance);
    }

    [Fact]
    public void The_null_host_does_nothing_and_answers_with_empty_defaults()
    {
        IItemHost host = NullItemHost.Instance;
        var player = new Player();
        var item = new Item();

        host.SendMessage(player, new Message(1));
        host.SendPoint(player);
        host.SendNotification(player, "x");
        host.SendNotificationFromAdmin(player, "x");
        host.SendItemBag(player);
        host.AddItemBag(player, item);
        host.SubQuantityItemsBag(player, item, 1);
        host.SubQuantityItemsBag(player, null, 1);
        host.RemoveMonkeyTail(player);
        host.IncreasePowerLimit(player);

        Assert.Equal(0, host.GetCountEmptyBag(player));
        Assert.Null(host.FindBagItem(player, 5));
        Assert.Empty(host.GetCombineItems(player));
        Assert.Same(ExternalItemTimes.None, host.GetExternalItemTimes(player));
    }

    [Fact]
    public void No_external_times_means_no_fusion_no_effect_and_no_clan()
    {
        ExternalItemTimes none = ExternalItemTimes.None;

        Assert.Equal(0, none.FusionType);
        Assert.False(none.IsCumber || none.IsCumber2 || none.IsPain || none.IsKefla);
        Assert.Null(none.Clan);
    }

    [Fact]
    public void A_dropped_roll_keeps_its_template_and_options()
    {
        var options = new List<ItemOption>();

        var drop = new RolledDrop(561, options);

        Assert.Equal(561, drop.TemplateId);
        Assert.Same(options, drop.Options);
        Assert.Equal(new RolledDrop(561, options), drop);
    }
}
