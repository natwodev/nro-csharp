using Nro.Core.Templates;
using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.Core;

public class ItemCatalogTests
{
    [Fact]
    public void Templates_are_found_by_position()
    {
        ItemCatalog catalog = ItemTestKit.Catalog();

        Assert.Equal(0, catalog.GetTemplate(0).Id);
        Assert.Equal(1899, catalog.GetTemplate(ItemTestKit.TemplateCount - 1).Id);
        Assert.Equal(0, catalog.GetItemOptionTemplate(0).Id);
        Assert.Equal(299, catalog.GetItemOptionTemplate(ItemTestKit.OptionCount - 1).Id);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(ItemTestKit.TemplateCount)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void A_template_position_outside_the_table_throws(int id)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemTestKit.Catalog().GetTemplate(id));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(ItemTestKit.OptionCount)]
    [InlineData(int.MaxValue)]
    public void An_option_template_position_outside_the_table_throws(int id)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ItemTestKit.Catalog().GetItemOptionTemplate(id));
    }

    [Fact]
    public void An_empty_catalog_throws_for_every_id_and_has_no_shop_items()
    {
        var catalog = new ItemCatalog([], []);

        Assert.Throws<ArgumentOutOfRangeException>(() => catalog.GetTemplate(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => catalog.GetItemOptionTemplate(0));
        Assert.Empty(catalog.ShopItems);
        Assert.Equal(-1, catalog.GetItemIdByIcon(1));
    }

    [Fact]
    public void Constructor_rejects_null_tables()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemCatalog(null!, []));
        Assert.Throws<ArgumentNullException>(() => new ItemCatalog([], null!));
    }

    [Fact]
    public void Item_id_by_icon_returns_the_first_template_with_that_icon_or_minus_one()
    {
        ItemCatalog catalog = ItemTestKit.Catalog(t =>
        {
            if (t.Id == 5 || t.Id == 9)
            {
                t.IconID = 777;
            }
        });

        Assert.Equal(5, catalog.GetItemIdByIcon(777));
        Assert.Equal(-1, catalog.GetItemIdByIcon(-5));
        Assert.Equal(0, catalog.GetItemIdByIcon(1000));
    }

    [Fact]
    public void Option_param_in_the_shop_is_the_first_entry_of_the_template_that_has_the_option()
    {
        ItemCatalog bare = ItemTestKit.Catalog();
        ItemTemplate template = bare.GetTemplate(10);
        var shop = new[]
        {
            new ShopItemEntry(bare.GetTemplate(9), [ItemTestKit.Opt(bare, 47, 1)]),
            new ShopItemEntry(template, [ItemTestKit.Opt(bare, 6, 55)]),
            new ShopItemEntry(template, [ItemTestKit.Opt(bare, 47, 200), ItemTestKit.Opt(bare, 47, 300)]),
        };
        var catalog = new ItemCatalog(bare.ItemTemplates, bare.ItemOptionTemplates, shop);

        Assert.Equal(200, catalog.GetOptionParamItemShop(10, 47));
        Assert.Equal(55, catalog.GetOptionParamItemShop(10, 6));
        Assert.Equal(-1, catalog.GetOptionParamItemShop(10, 0));
        Assert.Equal(-1, catalog.GetOptionParamItemShop(11, 47));
    }

    [Fact]
    public void The_catalog_copies_its_input_so_later_changes_do_not_leak_in()
    {
        var templates = new List<ItemTemplate> { new() { Id = 0 } };
        var options = new List<ItemOptionTemplate> { new(0, "a", 0) };
        var catalog = new ItemCatalog(templates, options);

        templates.Add(new ItemTemplate { Id = 1 });
        options.Clear();

        Assert.Single(catalog.ItemTemplates);
        Assert.Single(catalog.ItemOptionTemplates);
    }
}
