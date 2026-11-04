using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Core.Templates;
using Nro.Game;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary>The production <see cref="InventoryItemFactory"/> over the real <see cref="ItemService"/> (track D).</summary>
public class InventoryItemFactoryTests
{
    private static InventoryItemFactory Make(out ItemCatalog catalog, out ManualTimeProvider time)
    {
        var templates = new List<ItemTemplate>();
        for (int i = 0; i < 20; i++)
        {
            templates.Add(new ItemTemplate { Id = (short)i, Type = 99, Name = "item" + i });
        }

        var options = new List<ItemOptionTemplate>();
        for (int i = 0; i < 10; i++)
        {
            options.Add(new ItemOptionTemplate(i, "option" + i + " #", 0));
        }

        catalog = new ItemCatalog(templates, options);
        time = new ManualTimeProvider(ManualTimeProvider.Vn(2026, 11, 3, 12));
        var clock = new VietnamClock(time);
        var service = new ItemService(catalog, new RandomUtil(new SeededRandomProvider(1)), clock);
        return new InventoryItemFactory(service, clock);
    }

    [Fact]
    public void CreateItemNull_is_a_new_empty_slot_each_time()
    {
        InventoryItemFactory f = Make(out _, out ManualTimeProvider time);

        Item a = f.CreateItemNull();
        Item b = f.CreateItemNull();

        Assert.False(a.IsNotNullItem());
        Assert.NotSame(a, b);
        Assert.Equal(time.GetUtcNow().ToUnixTimeMilliseconds(), a.CreateTime);
    }

    [Fact]
    public void CreateNewItem_has_quantity_one_and_the_standard_texts()
    {
        InventoryItemFactory f = Make(out _, out _);

        Item item = f.CreateNewItem(5);

        Assert.Equal(5, item.Template!.Id);
        Assert.Equal(1, item.Quantity);
        Assert.Equal("Yêu cầu sức mạnh 0 trở lên", item.Content);
        Assert.Equal(string.Empty, item.Info);
    }

    [Fact]
    public void NewItem_is_java_new_Item_with_a_template_only()
    {
        InventoryItemFactory f = Make(out _, out ManualTimeProvider time);

        Item item = f.NewItem(7);

        Assert.Equal(7, item.Template!.Id);
        Assert.Equal(0, item.Quantity);
        Assert.Null(item.Info);
        Assert.Null(item.Content);
        Assert.Equal(time.GetUtcNow().ToUnixTimeMilliseconds(), item.CreateTime);
        Assert.Throws<ArgumentOutOfRangeException>(() => f.NewItem(500));
    }

    [Fact]
    public void CopyItem_copies_options_and_not_quantityGD()
    {
        InventoryItemFactory f = Make(out ItemCatalog catalog, out _);
        Item item = f.CreateNewItem(3);
        item.Quantity = 12;
        item.QuantityGD = 4;
        item.ItemOptions.Add(f.CreateItemOption(2, 50));

        Item copy = f.CopyItem(item);
        copy.ItemOptions[0].Param = 1;

        Assert.NotSame(item, copy);
        Assert.Equal(12, copy.Quantity);
        Assert.Equal(0, copy.QuantityGD);
        Assert.Equal(50, item.ItemOptions[0].Param);
        Assert.Same(catalog.GetTemplate(3), copy.Template);
    }

    [Fact]
    public void CreateItemOption_looks_the_template_up_by_position()
    {
        InventoryItemFactory f = Make(out _, out _);

        ItemOption option = f.CreateItemOption(4, 9);

        Assert.Equal(4, option.OptionTemplate.Id);
        Assert.Equal(9, option.Param);
        Assert.Throws<ArgumentOutOfRangeException>(() => f.CreateItemOption(99, 1));
    }

    [Fact]
    public void Constructor_rejects_null()
    {
        InventoryItemFactory f = Make(out _, out _);
        Assert.Throws<ArgumentNullException>(() => new InventoryItemFactory(null!, new VietnamClock()));
        Assert.NotNull(f);
    }
}
