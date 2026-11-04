using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary>The item-list half of <see cref="Inventory"/> (GetItemOptionParameter, HasOption, Dispose) and the construction of <see cref="InventoryService"/>.</summary>
public class InventoryListTests
{
    [Fact]
    public void GetItemOptionParameter_returns_the_first_option_with_that_id_or_zero()
    {
        var k = new InvKit();
        Item item = k.Item(10, 1, (4, 5), (4, 6), (9, 7));

        Assert.Equal(5, k.Inv.GetItemOptionParameter(item, 4));
        Assert.Equal(7, k.Inv.GetItemOptionParameter(item, 9));
        Assert.Equal(0, k.Inv.GetItemOptionParameter(item, 99));
        Assert.Equal(0, k.Inv.GetItemOptionParameter(k.Empty(), 4));
    }

    [Fact]
    public void HasOption_by_index_needs_a_real_item_with_the_option()
    {
        var k = new InvKit();
        k.Put(k.Inv.ItemsBag, 2, k.Item(10, 1, (30, 0)));

        Assert.True(k.Inv.HasOption(k.Inv.ItemsBag, 2, 30));
        Assert.False(k.Inv.HasOption(k.Inv.ItemsBag, 2, 31));
        Assert.False(k.Inv.HasOption(k.Inv.ItemsBag, 3, 30));
    }

    [Fact]
    public void HasOption_by_index_throws_outside_the_list()
    {
        var k = new InvKit();
        Assert.Throws<ArgumentOutOfRangeException>(() => k.Inv.HasOption(k.Inv.ItemsBag, -1, 30));
        Assert.Throws<ArgumentOutOfRangeException>(() => k.Inv.HasOption(k.Inv.ItemsBag, 1000, 30));
    }

    [Fact]
    public void Dispose_disposes_every_item_and_empties_all_lists_but_keeps_the_money()
    {
        var k = new InvKit();
        Inventory inv = k.Inv;
        Item body = k.Put(inv.ItemsBody, 0, k.Item(10, 1, (4, 5)));
        Item bag = k.Put(inv.ItemsBag, 0, k.Item(11));
        Item box = k.Put(inv.ItemsBox, 0, k.Item(12));
        Item collection = k.Put(inv.ItemsBoxCollection, 0, k.Item(13));
        inv.ItemsBoxLuckyRound.Add(k.Item(14));
        inv.ItemsMailbox.Add(k.Item(15));
        inv.SoldItems.Add(k.Item(16));
        inv.TrainingArmor = k.Item(17);
        Item train = inv.TrainingArmor;
        inv.Gold = 99;
        inv.Gem = 5;

        inv.Dispose();

        Assert.All(new[] { body, bag, box, collection, train }, i => Assert.False(i.IsNotNullItem()));
        Assert.Empty(inv.ItemsBody);
        Assert.Empty(inv.ItemsBag);
        Assert.Empty(inv.ItemsBox);
        Assert.Empty(inv.ItemsBoxCollection);
        Assert.Empty(inv.ItemsBoxLuckyRound);
        Assert.Empty(inv.ItemsMailbox);
        Assert.Empty(inv.SoldItems);
        Assert.Null(inv.TrainingArmor);
        Assert.Equal(99, inv.Gold);
        Assert.Equal(5, inv.Gem);
    }

    [Fact]
    public void Dispose_twice_and_use_after_dispose_are_harmless()
    {
        var inv = new Inventory();
        inv.Dispose();
        inv.Dispose();
        inv.ItemsBag.Add(new Item());
        Assert.Single(inv.ItemsBag);
    }

    [Fact]
    public void Service_constructor_rejects_missing_collaborators()
    {
        var k = new InvKit();
        var random = new SeededRandomProvider(1);
        Assert.Throws<ArgumentNullException>(() => new InventoryService(null!, k.Notifier, k.Host, random, k.GiftRepo, k.Usage));
        Assert.Throws<ArgumentNullException>(() => new InventoryService(k.Factory, null!, k.Host, random, k.GiftRepo, k.Usage));
        Assert.Throws<ArgumentNullException>(() => new InventoryService(k.Factory, k.Notifier, null!, random, k.GiftRepo, k.Usage));
        Assert.Throws<ArgumentNullException>(() => new InventoryService(k.Factory, k.Notifier, k.Host, null!, k.GiftRepo, k.Usage));
        Assert.Throws<ArgumentNullException>(() => new InventoryService(k.Factory, k.Notifier, k.Host, random, null!, k.Usage));
        Assert.Throws<ArgumentNullException>(() => new InventoryService(k.Factory, k.Notifier, k.Host, random, k.GiftRepo, null!));
        Assert.NotNull(new InventoryService(k.Factory, k.Notifier, k.Host, random, k.GiftRepo, k.Usage));
    }
}
