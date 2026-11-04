using Nro.Game;

namespace Nro.Tests.Unit.Game.Effects;

public class EffectFlagBagTests
{
    private static EffectFlagBag AllOn() => new()
    {
        UseSnailShell = true,
        UseIceCream = true,
        UseDolphin = true,
        UseKite = true,
        UseDragonKite = true,
        UseBlackCat = true,
        UseFishSkewer = true,
        UsePigLauncher = true,
    };

    [Fact]
    public void New_bag_has_every_flag_off()
    {
        var bag = new EffectFlagBag();
        Assert.False(bag.UseSnailShell);
        Assert.False(bag.UseIceCream);
        Assert.False(bag.UseDolphin);
        Assert.False(bag.UseKite);
        Assert.False(bag.UseDragonKite);
        Assert.False(bag.UseBlackCat);
        Assert.False(bag.UseFishSkewer);
        Assert.False(bag.UsePigLauncher);
    }

    [Fact]
    public void Reset_switches_all_eight_flags_off()
    {
        EffectFlagBag bag = AllOn();
        bag.Reset();
        Assert.False(bag.UseSnailShell);
        Assert.False(bag.UseIceCream);
        Assert.False(bag.UseDolphin);
        Assert.False(bag.UseKite);
        Assert.False(bag.UseDragonKite);
        Assert.False(bag.UseBlackCat);
        Assert.False(bag.UseFishSkewer);
        Assert.False(bag.UsePigLauncher);
    }

    [Fact]
    public void Reset_then_set_one_leaves_only_that_one_on()
    {
        EffectFlagBag bag = AllOn();
        bag.Reset();
        bag.UseFishSkewer = true;
        Assert.True(bag.UseFishSkewer);
        Assert.False(bag.UseBlackCat);
        Assert.False(bag.UsePigLauncher);
    }

    [Fact]
    public void Reset_is_idempotent_and_Dispose_keeps_the_flags()
    {
        EffectFlagBag bag = AllOn();
        bag.Dispose();
        Assert.True(bag.UseDolphin);
        bag.Reset();
        bag.Reset();
        Assert.False(bag.UseDolphin);
    }
}
