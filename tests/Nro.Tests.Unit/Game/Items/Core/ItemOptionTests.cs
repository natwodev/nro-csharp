using Nro.Core.Templates;
using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.Core;

public class ItemOptionTests
{
    private static readonly ItemCatalog Catalog = ItemTestKit.Catalog();

    [Fact]
    public void Option_string_replaces_every_hash_with_the_parameter()
    {
        var io = new ItemOption(new ItemOptionTemplate(7, "HP +# (#)", 0), 123);

        Assert.Equal("HP +123 (123)", io.GetOptionString());
    }

    [Theory]
    [InlineData(0, "Opt0 0")]
    [InlineData(47, "Opt47 -5")]
    [InlineData(21, "Opt21 2147483647")]
    public void Option_string_uses_invariant_digits_and_the_sign(int id, string expectedPrefix)
    {
        int param = expectedPrefix.Split(' ')[1] switch
        {
            "0" => 0,
            "-5" => -5,
            _ => int.MaxValue,
        };

        var io = ItemTestKit.Opt(Catalog, id, param);

        Assert.Equal(expectedPrefix, io.GetOptionString());
    }

    [Fact]
    public void Option_string_of_a_template_without_hash_is_the_text_itself()
    {
        var io = new ItemOption(new ItemOptionTemplate(1, "Khong the giao dich", 0), 9);

        Assert.Equal("Khong the giao dich", io.GetOptionString());
    }

    [Fact]
    public void Copy_constructor_copies_the_parameter_and_shares_only_the_template()
    {
        var original = ItemTestKit.Opt(Catalog, 47, 100);

        var copy = new ItemOption(original);
        copy.Param = 5;

        Assert.Equal(100, original.Param);
        Assert.Same(original.OptionTemplate, copy.OptionTemplate);
    }

    [Fact]
    public void Copy_constructor_rejects_null()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemOption(null!));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(6, true)]
    [InlineData(7, true)]
    [InlineData(14, true)]
    [InlineData(22, true)]
    [InlineData(23, true)]
    [InlineData(27, true)]
    [InlineData(28, true)]
    [InlineData(47, true)]
    [InlineData(1, false)]
    [InlineData(21, false)]
    [InlineData(30, false)]
    [InlineData(48, false)]
    [InlineData(299, false)]
    public void Only_nine_option_ids_can_be_upgraded(int id, bool expected)
    {
        Assert.Equal(expected, ItemTestKit.Opt(Catalog, id, 1).IsOptionCanUpgrade());
    }

    [Theory]
    [InlineData(93, true)]
    [InlineData(260, true)]
    [InlineData(261, true)]
    [InlineData(92, false)]
    [InlineData(94, false)]
    [InlineData(259, false)]
    [InlineData(262, false)]
    [InlineData(0, false)]
    public void Expiry_options_93_260_261_have_an_expiry_date(int id, bool expected)
    {
        // the ids above the synthetic table need their own template
        var io = new ItemOption(new ItemOptionTemplate(id, "x", 0), 1);

        Assert.Equal(expected, io.HaveExpiryDate());
    }

    [Fact]
    public void ToString_writes_both_numbers_as_quoted_strings()
    {
        Assert.Equal("{\"id\":\"47\",\"param\":\"100\"}", ItemTestKit.Opt(Catalog, 47, 100).ToString());
        Assert.Equal("{\"id\":\"0\",\"param\":\"-1\"}", ItemTestKit.Opt(Catalog, 0, -1).ToString());
    }

    [Fact]
    public void Default_constructor_leaves_the_parameter_zero()
    {
        var io = new ItemOption();

        Assert.Equal(0, io.Param);
    }

    [Fact]
    public void Dispose_keeps_the_option_printable()
    {
        var io = ItemTestKit.Opt(Catalog, 47, 8);

        io.Dispose();

        Assert.Equal("Opt47 8", io.GetOptionString());
    }
}
