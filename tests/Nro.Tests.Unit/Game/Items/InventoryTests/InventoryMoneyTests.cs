using System.Text.Json;
using Nro.Game;

namespace Nro.Tests.Unit.Game.Items.InventoryTests;

/// <summary>
/// The money half of <see cref="Inventory"/> against the Java <c>Inventory.java</c>: Java <c>int</c> arithmetic wraps, gold is a <c>long</c>, the add methods clamp AFTER adding.
/// The named cases below are boundary cases worked out by hand; <see cref="JavaReplay_matches_the_unmodified_Inventory_java"/> replays 700 scenarios produced by the REAL
/// <c>Inventory.java</c> (see <c>java/InventoryMoneyGenerator.java.txt</c>), so the expectations were measured, not written by the porter.
/// </summary>
public class InventoryMoneyTests
{
    [Fact]
    public void Constants_are_the_java_values()
    {
        Assert.Equal(2_000_000_000_000L, Inventory.LIMIT_GOLD);
        Assert.Equal(2_000_000_000L, Inventory.DRAGON_WISH_GOLD_CAP);
        Assert.Equal(2_000_000_000, Inventory.LIMIT_RUBY);
        Assert.Equal(2_000_000_000, Inventory.LIMIT_GEM);
        Assert.Equal(2_000_000_000, Inventory.LIMIT_VIP_EXPERIENCE);
        Assert.Equal(100, Inventory.MAX_ITEMS_BAG);
        Assert.Equal(100, Inventory.MAX_ITEMS_BOX);
        Assert.Equal(Inventory.LIMIT_GOLD, new Inventory().GetGoldLimit());
    }

    [Fact]
    public void New_inventory_is_empty_and_has_all_seven_lists()
    {
        var inv = new Inventory();
        Assert.Equal(0, inv.Gold);
        Assert.Equal(0, inv.Gem);
        Assert.Equal(0, inv.Ruby);
        Assert.Equal(0, inv.VipExperience);
        Assert.Null(inv.TrainingArmor);
        Assert.Empty(inv.GiftCodes);
        Assert.Empty(inv.ItemsBody);
        Assert.Empty(inv.ItemsBag);
        Assert.Empty(inv.ItemsBox);
        Assert.Empty(inv.ItemsBoxCollection);
        Assert.Empty(inv.ItemsBoxLuckyRound);
        Assert.Empty(inv.ItemsMailbox);
        Assert.Empty(inv.SoldItems);
    }

    [Theory]
    [InlineData(0L, 5, 5L)]
    [InlineData(1_999_999_999_999L, 1, 2_000_000_000_000L)]
    [InlineData(1_999_999_999_999L, 2, 2_000_000_000_000L)]
    [InlineData(2_000_000_000_000L, 1, 2_000_000_000_000L)]
    [InlineData(2_000_000_000_000L, -1, 1_999_999_999_999L)]
    [InlineData(2_147_483_647L, 1, 2_147_483_648L)]
    [InlineData(2_147_483_647L, int.MaxValue, 4_294_967_294L)]
    [InlineData(0L, -5, -5L)]
    [InlineData(2_500_000_000_000L, 0, 2_000_000_000_000L)]
    public void AddGold_is_a_long_add_then_clamp(long start, int add, long expected)
    {
        var inv = new Inventory { Gold = start };
        inv.AddGold(add);
        Assert.Equal(expected, inv.Gold);
    }

    [Theory]
    [InlineData(0, 5, 5)]
    [InlineData(1_999_999_999, 1, 2_000_000_000)]
    [InlineData(2_000_000_000, 1, 2_000_000_000)]
    [InlineData(2_000_000_000, 147_483_647, 2_000_000_000)]
    [InlineData(5, -10, -5)]
    [InlineData(int.MinValue, -1, 2_000_000_000)]
    public void AddGem_adds_in_32_bits_then_clamps(int start, int add, int expected)
    {
        var inv = new Inventory { Gem = start };
        inv.AddGem(add);
        Assert.Equal(expected, inv.Gem);
    }

    [Fact]
    public void AddGem_wraps_before_the_clamp_so_two_limits_added_give_a_negative_balance()
    {
        var inv = new Inventory { Gem = 2_000_000_000 };
        inv.AddGem(2_000_000_000);

        // 4,000,000,000 does not fit in an int: Java wraps to -294,967,296, which is NOT above the limit, so it is kept.
        Assert.Equal(-294_967_296, inv.Gem);
    }

    [Fact]
    public void AddGem_at_int_max_wraps_to_int_min()
    {
        var inv = new Inventory { Gem = int.MaxValue };
        inv.AddGem(1);
        Assert.Equal(int.MinValue, inv.Gem);
    }

    [Fact]
    public void AddRuby_and_AddVipExperience_follow_the_same_rules_as_AddGem()
    {
        var inv = new Inventory { Ruby = 2_000_000_000, VipExperience = 2_000_000_000 };
        inv.AddRuby(2_000_000_000);
        inv.AddVipExperience(2_000_000_000);
        Assert.Equal(-294_967_296, inv.Ruby);
        Assert.Equal(-294_967_296, inv.VipExperience);

        inv.Ruby = 1_999_999_999;
        inv.VipExperience = 1_999_999_999;
        inv.AddRuby(2);
        inv.AddVipExperience(2);
        Assert.Equal(2_000_000_000, inv.Ruby);
        Assert.Equal(2_000_000_000, inv.VipExperience);
    }

    [Fact]
    public void Subtract_methods_do_not_stop_at_zero()
    {
        var inv = new Inventory { Gold = 3, Gem = 3, Ruby = 3, VipExperience = 3 };
        inv.SubtractGold(10);
        inv.SubtractGem(10);
        inv.SubtractRuby(10);
        inv.SubtractVipExperience(10);
        Assert.Equal(-7, inv.Gold);
        Assert.Equal(-7, inv.Gem);
        Assert.Equal(-7, inv.Ruby);
        Assert.Equal(-7, inv.VipExperience);
    }

    [Fact]
    public void SubtractGold_long_overload_handles_values_above_32_bits()
    {
        var inv = new Inventory { Gold = 5_000_000_000L };
        inv.SubtractGold(3_000_000_000L);
        Assert.Equal(2_000_000_000L, inv.Gold);

        inv.SubtractGold(int.MinValue);
        Assert.Equal(2_000_000_000L + 2_147_483_648L, inv.Gold);
    }

    [Fact]
    public void SubtractGem_wraps_at_int_min()
    {
        var inv = new Inventory { Gem = int.MinValue };
        inv.SubtractGem(1);
        Assert.Equal(int.MaxValue, inv.Gem);
    }

    [Theory]
    [InlineData(10, 50, 3, 7, 50)]
    [InlineData(10, 50, 10, 0, 50)]
    [InlineData(10, 50, 11, 0, 49)]
    [InlineData(0, 50, 50, 0, 0)]
    [InlineData(0, 50, 51, 0, -1)]
    [InlineData(10, 50, 0, 10, 50)]
    public void SubtractGemAndRuby_pays_with_ruby_first_then_gem(int ruby, int gem, int pay, int expectedRuby, int expectedGem)
    {
        var inv = new Inventory { Ruby = ruby, Gem = gem };
        inv.SubtractGemAndRuby(pay);
        Assert.Equal(expectedRuby, inv.Ruby);
        Assert.Equal(expectedGem, inv.Gem);
    }

    [Fact]
    public void SubtractGemAndRuby_can_leave_gem_negative_when_the_total_is_not_enough()
    {
        var inv = new Inventory { Ruby = 2, Gem = 3 };
        inv.SubtractGemAndRuby(10);
        Assert.Equal(0, inv.Ruby);
        Assert.Equal(-5, inv.Gem);
    }

    [Fact]
    public void GetGemAndRuby_wraps_in_32_bits()
    {
        Assert.Equal(15, new Inventory { Gem = 10, Ruby = 5 }.GetGemAndRuby());
        Assert.Equal(int.MinValue, new Inventory { Gem = int.MaxValue, Ruby = 1 }.GetGemAndRuby());
        Assert.Equal(-294_967_296, new Inventory { Gem = 2_000_000_000, Ruby = 2_000_000_000 }.GetGemAndRuby());
    }

    [Fact]
    public void Getters_return_the_fields()
    {
        var inv = new Inventory { Gold = 7_000_000_000L, Gem = 1, Ruby = 2, VipExperience = 3 };
        Assert.Equal(7_000_000_000L, inv.GetGold());
        Assert.Equal(1, inv.GetGem());
        Assert.Equal(2, inv.GetRuby());
        Assert.Equal(3, inv.GetVipExperience());
    }

    [Fact]
    public void JavaReplay_matches_the_unmodified_Inventory_java()
    {
        string path = Path.Combine(TestPaths.ProjectDirectory("Nro.Tests.Unit"), "Game", "Items", "InventoryTests", "inventory-money.json");
        using JsonDocument doc = JsonDocument.Parse(File.ReadAllBytes(path));
        JsonElement cases = doc.RootElement.GetProperty("cases");
        Assert.True(cases.GetArrayLength() >= 700);

        JsonElement constants = doc.RootElement.GetProperty("constants");
        Assert.Equal(Inventory.LIMIT_GOLD, constants.GetProperty("LIMIT_GOLD").GetInt64());
        Assert.Equal(Inventory.DRAGON_WISH_GOLD_CAP, constants.GetProperty("LIMIT_GOLD2").GetInt64());
        Assert.Equal(Inventory.LIMIT_GEM, constants.GetProperty("LIMIT_GEM").GetInt32());
        Assert.Equal(Inventory.LIMIT_RUBY, constants.GetProperty("LIMIT_RUBY").GetInt32());
        Assert.Equal(Inventory.LIMIT_VIP_EXPERIENCE, constants.GetProperty("LIMIT_EXP_VIP").GetInt32());
        Assert.Equal(Inventory.MAX_ITEMS_BAG, constants.GetProperty("MAX_ITEMS_BAG").GetInt32());
        Assert.Equal(Inventory.MAX_ITEMS_BOX, constants.GetProperty("MAX_ITEMS_BOX").GetInt32());
        Assert.Equal(new Inventory().GetGoldLimit(), constants.GetProperty("goldLimit").GetInt64());

        var failures = new List<string>();
        int index = 0;
        foreach (JsonElement c in cases.EnumerateArray())
        {
            string op = c.GetProperty("op").GetString()!;
            long arg = c.GetProperty("arg").GetInt64();
            JsonElement before = c.GetProperty("before");
            JsonElement after = c.GetProperty("after");
            var inv = new Inventory
            {
                Gold = before[0].GetInt64(),
                Gem = before[1].GetInt32(),
                Ruby = before[2].GetInt32(),
                VipExperience = before[3].GetInt32(),
            };

            switch (op)
            {
                case "addGold": inv.AddGold((int)arg); break;
                case "addRuby": inv.AddRuby((int)arg); break;
                case "addGem": inv.AddGem((int)arg); break;
                case "addExpVip": inv.AddVipExperience((int)arg); break;
                case "subGem": inv.SubtractGem((int)arg); break;
                case "subGoldInt": inv.SubtractGold((int)arg); break;
                case "subGoldLong": inv.SubtractGold(arg); break;
                case "subRuby": inv.SubtractRuby((int)arg); break;
                case "subExpVip": inv.SubtractVipExperience((int)arg); break;
                case "subGemAndRuby": inv.SubtractGemAndRuby((int)arg); break;
                default: throw new InvalidOperationException("unknown op " + op);
            }

            bool ok = inv.Gold == after[0].GetInt64() && inv.Gem == after[1].GetInt32() && inv.Ruby == after[2].GetInt32()
                && inv.VipExperience == after[3].GetInt32() && inv.GetGemAndRuby() == c.GetProperty("gemAndRuby").GetInt32();
            if (!ok)
            {
                failures.Add($"#{index} {op}({arg}) from {before}: got [{inv.Gold},{inv.Gem},{inv.Ruby},{inv.VipExperience}] want {after}");
            }

            index++;
        }

        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures.Take(10)));
    }
}
